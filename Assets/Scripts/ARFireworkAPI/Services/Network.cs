using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using ARFireworkAPI.Models;
using ARFireworkAPI.Serializable;
using Newtonsoft.Json.Linq;
using Proyecto26;
using RSG;
using UnityEditor;
using UnityEngine;
using UnityEngine.Networking;

namespace ARFireworkAPI.Services
{
    public class Network : INetwork
    {
        private static Network _instance;
        private static readonly object Padlock = new object();
        private readonly TokenStorage _tokenStorage;
        private static readonly string Prefix = "/api/";
        private static readonly string PlaceFireworkRoute = "firework/broadcast";

        public AuthenticationResponse AuthenticationResponse { get; private set; }
        public bool ConnectionIsStillValid { get; private set; }

        public int PlaceFireworkResponse { get; private set; }

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
            _tokenStorage = TokenStorage.Instance;
        }
        

        public void BindReceiveFireworkPlacement(OnReceiveFireworkPlacement f)
        {
            throw new NotImplementedException();
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
            // 1) TODO: bind on receive
            
            // 2) fill form
            var form = new WWWForm();
            form.AddField("author", firework.Author);
            form.AddField("type", firework.Type.ToString());
            form.AddField("x", firework.GetX().ToString());
            form.AddField("y", firework.GetY().ToString());
            form.AddField("z", firework.GetZ().ToString());
            
            var accessToken = _tokenStorage.RetrieveAccessToken();
            
            using (var www = UnityWebRequest.Post($"{Prefix}{PlaceFireworkRoute}", form))
            {
                www.SetRequestHeader("Accept", "application/json");
                www.SetRequestHeader("Authorization", $"Bearer {accessToken}");
                
                yield return www.SendWebRequest();

                if (www.isNetworkError || www.isHttpError)
                {
                    PlaceFireworkResponse = (int) www.responseCode;
                    Debug.Log(www.error);
                    Debug.Log(PlaceFireworkResponse);
                    // Debug.LogError(www.downloadHandler.text);
                }
                else
                {
                    Debug.Log("Response Text:" + www.downloadHandler.text);
                    PlaceFireworkResponse = (int) www.responseCode;
                    Debug.Log(PlaceFireworkResponse);
                }
            }
        }

        public IEnumerator SendCode(string code)
        {
            AuthenticationResponse = null;
            yield return Login(code);
            if (AuthenticationResponse.IsSuccessful())
            {
                _tokenStorage.StoreAccessToken(AuthenticationResponse.AccessToken);
            }
        }

        private IEnumerator Login(string password)
        {
            var form = new WWWForm();
            form.AddField("password", password);

            using (var www = UnityWebRequest.Post("/api/login", form))
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
                    Debug.Log("Form uploaded complete!");
                    var responseText = www.downloadHandler.text;
                    Debug.Log("Response Text:" + responseText);
                    var accessToken = (string) JObject.Parse(responseText)["token"];
                    Debug.Log(accessToken);
                    var person = (Person) JObject.Parse(responseText)["user"].ToObject<Person>();
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
                using (var www = UnityWebRequest.Get("/api/protected-resource-test"))
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
    }
}