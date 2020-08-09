using System;
using System.Net;
using PusherClient;

namespace ARFireworkAPI.Adapters
{
    public class MyAuthorizer : IAuthorizer
    {
        private Uri _authEndpoint;
        private string _accessToken;
        public MyAuthorizer (string authEndpoint, string accessToken)
        {
            _authEndpoint = new Uri(authEndpoint);
            _accessToken = accessToken;
        }

        public string Authorize(string channelName, string socketId)
        {
            string authToken = null;

            using (var webClient = new System.Net.WebClient())
            {
                string data = String.Format("channel_name={0}&socket_id={1}", channelName, socketId);
                webClient.Headers[HttpRequestHeader.ContentType] = "application/x-www-form-urlencoded";
                webClient.Headers[HttpRequestHeader.Accept] = "application/json";
                webClient.Headers[HttpRequestHeader.Authorization] = $"Bearer {_accessToken}";
                authToken = webClient.UploadString(_authEndpoint, "POST", data);
            }
            return authToken;
        }
    }
}