using System;
using System.Collections;
using System.Collections.Generic;
using ARFireworkAPI.Adapters;
using ARFireworkAPI.Models;
using ARFireworkAPI.Serializable;
using Newtonsoft.Json.Linq;
using PusherClient;
using UnityEngine;
using UnityEngine.Networking;
using System.Linq;
using Newtonsoft.Json; // Make 'Select' extension available


namespace ARFireworkAPI.Services
{
    public class Network : INetwork
    {
        private static Network _instance;
        private static readonly object Padlock = new object();
        private readonly TokenStorage _tokenStorage;

        public static string Protocol { get; private set; } = "http://";
        public static string Host { get; private set; } = "localhost";
        private const string Prefix = "/api/";

        private const string PlaceFireworkRoute = "firework/broadcast";
        private const string TriggerFireworkRoute = "firework/trigger";
        private const string LoginRoute = "login";
        private const string ProtectedResourceRoute = "protected-resource-test";

        // PUSHER
        private Pusher _pusher;
        private Channel _channel;

        private readonly string _pusherAuthEndpoint;
        private readonly string _appKey;
        private readonly string _appCluster;
        private readonly string _channelFirework;

        public AuthenticationResponse AuthenticationResponse { get; private set; }
        public bool ConnectionIsStillValid { get; private set; }

        public int PlaceFireworkResponse { get; private set; }
        public int TriggerFireworkResponse { get; private set; }

        public static Network Instance
        {
            get
            {
                lock (Network.Padlock)
                    if (Network._instance == null)
                    {
                        Network._instance = new Network();
                    }

                return Network._instance;
            }
        }


        private Network()
        {
            new EnvSetter();
            _pusherAuthEndpoint = Environment.GetEnvironmentVariable("pusher_auth_endpoint");
            _appKey = Environment.GetEnvironmentVariable("app_key");
            _appCluster = Environment.GetEnvironmentVariable("app_cluster");
            _channelFirework = Environment.GetEnvironmentVariable("channel_firework");
            _tokenStorage = TokenStorage.Instance;
        }

        public void SetProtocol(string protocol)
        {
            Protocol = protocol;
        }

        public void SetHost(string host)
        {
            Host = host;
        }

        public void BindReceiveFireworkPlacement(OnReceiveFireworkPlacement f)
        {
            _channel.Bind("firework-placement", (dynamic data) => { f(new FireworkReceived(data)); });
        }

        public void BindReceiveFireworkTrigger(OnReceiveFireworkTrigger f)
        {
            throw new System.NotImplementedException();
        }

        public IEnumerator CheckConnectionStillValid()
        {
            ConnectionIsStillValid = false;
            yield return TryAccessProtectedResource();
        }

        public void Logout()
        {
            _tokenStorage.ClearToken();
        }

        public IEnumerator PlaceFirework(Firework firework)
        {
            // fill form
            var form = new WWWForm();
            // form.AddField("author", firework.Author);
            form.AddField("type", firework.Type.ToString());
            form.AddField("x", firework.GetX().ToString());
            form.AddField("y", firework.GetY().ToString());
            form.AddField("z", firework.GetZ().ToString());

            var accessToken = _tokenStorage.RetrieveAccessToken();

            using (var www = UnityWebRequest.Post(BuildUrl(PlaceFireworkRoute), form))
            {
                www.SetRequestHeader("Accept", "application/json");
                www.SetRequestHeader("Authorization", $"Bearer {accessToken}");

                yield return www.SendWebRequest();

                if (www.isNetworkError || www.isHttpError)
                {
                    PlaceFireworkResponse = (int) www.responseCode;
                    Debug.Log("Error: " + www.error);
                    Debug.Log("Reason: " + www.downloadHandler.text);
                    Debug.Log("PlaceFireworkResponse: " + PlaceFireworkResponse);
                    // Debug.LogError(www.downloadHandler.text);
                }
                else
                {
                    // Debug.Log("Response Text:" + www.downloadHandler.text);
                    PlaceFireworkResponse = (int) www.responseCode;
                    // Debug.Log("PlaceFireworkResponse: " + PlaceFireworkResponse);
                }
            }
        }

        public IEnumerator TriggerFirework(IEnumerable<FireworkReceived> fireworksReceived)
        {
            var fireworkIds = fireworksReceived.Select(fr => fr.Id).ToArray();
            yield return TriggerFirework(fireworkIds);
        }

        public IEnumerator TriggerFirework(uint[] fireworkIds)
        {
            var fireworkIdsStr = string.Join(",", fireworkIds);
            Debug.Log(fireworkIdsStr);
            var form = new WWWForm();
            form.AddField("firework_ids", fireworkIdsStr);
            
            var accessToken = _tokenStorage.RetrieveAccessToken();
            
            using (var www = UnityWebRequest.Post(BuildUrl(TriggerFireworkRoute), form))
            {
                www.SetRequestHeader("Accept", "application/json");
                www.SetRequestHeader("Authorization", $"Bearer {accessToken}");
                yield return www.SendWebRequest();

                TriggerFireworkResponse = (int) www.responseCode;
                Debug.Log(TriggerFireworkResponse);
                
                if (www.isNetworkError || www.isHttpError)
                {
                    Debug.Log(www.error);
                    Debug.Log(www.downloadHandler.text);
                }
                else
                {
                    // Debug.Log("Form uploaded complete!");
                    // var responseText = www.downloadHandler.text;
                    // Debug.Log("Response Text:" + responseText);
                }
            }
        }

        public IEnumerator SendCode(string code)
        {
            AuthenticationResponse = null;
            yield return Login(code);
            if (!AuthenticationResponse.IsSuccessful()) yield break;
            _tokenStorage.StoreAccessToken(AuthenticationResponse.AccessToken);
            InitializePusher(AuthenticationResponse.AccessToken);
        }

        // PRIVATE METHODS =============================================================================================

        #region PrivateMethods

        private IEnumerator Login(string password)
        {
            var form = new WWWForm();
            form.AddField("password", password);

            using (var www = UnityWebRequest.Post(BuildUrl(LoginRoute), form))
            {
                www.SetRequestHeader("Accept", "application/json");
                yield return www.SendWebRequest();

                if (www.isNetworkError || www.isHttpError)
                {
                    AuthenticationResponse =
                        new AuthenticationResponse(www.responseCode.ToString(), "error in http or network");
                    Debug.Log(www.error);
                    // Debug.LogError(www.downloadHandler.text);
                }
                else
                {
                    // Debug.Log("Form uploaded complete!");
                    var responseText = www.downloadHandler.text;
                    // Debug.Log("Response Text:" + responseText);
                    var accessToken = (string) JObject.Parse(responseText)["token"];
                    // Debug.Log(accessToken);
                    var person = JObject.Parse(responseText)["user"].ToObject<Person>();
                    AuthenticationResponse =
                        new AuthenticationResponse(www.responseCode.ToString(), person, accessToken);
                }
            }
        }

        private IEnumerator TryAccessProtectedResource()
        {
            var accessToken = _tokenStorage.RetrieveAccessToken();

            if (accessToken != null)
            {
                using (var www = UnityWebRequest.Get(BuildUrl(ProtectedResourceRoute)))
                {
                    www.SetRequestHeader("Accept", "application/json");
                    var bearerToken = $"Bearer {accessToken}";
                    Debug.Log(bearerToken);
                    www.SetRequestHeader("Authorization", bearerToken);
                    yield return www.SendWebRequest();

                    if (www.isNetworkError || www.isHttpError)
                    {
                        ConnectionIsStillValid = false;
                        Debug.LogError(www.responseCode.ToString());
                        Debug.LogError(www.error);
                        Debug.LogError(www.downloadHandler.text);
                    }
                    else
                    {
                        Debug.Log(www.responseCode.ToString());
                        Debug.Log(www.downloadHandler.text);
                        ConnectionIsStillValid = true;
                    }
                }
            }
            else
            {
                ConnectionIsStillValid = false;
            }
        }

        private string BuildUrl(string route)
        {
            return $"{Protocol}{Host}{Prefix}{route}";
        }

        private async void InitializePusher(string accessToken)
        {
            var uri = $"{Protocol}{Host}{Prefix}{_pusherAuthEndpoint}";
            // Debug.Log("uri: " + uri);
            // Debug.Log("app key: " + _appKey);
            // Debug.Log("app cluster: " + _appCluster);
            // Debug.Log("access token: " + accessToken);
            // Debug.Log("channel: " + _channelFirework);

            _pusher = new Pusher(_appKey, new PusherOptions()
            {
                Authorizer = new MyAuthorizer(uri, accessToken),
                Cluster = _appCluster,
            });

            _pusher.Error += (s, e) => { Debug.Log("Errored"); };
            _pusher.ConnectionStateChanged += (sender, state) => { Debug.Log("Connection state changed"); };
            _pusher.Connected += sender => { Debug.Log("Connected"); };

            _channel = await _pusher.SubscribeAsync(_channelFirework);
            _channel.Subscribed += s => { Debug.Log("Subscribed"); };

            await _pusher.ConnectAsync();
        }

        #endregion
    }
}