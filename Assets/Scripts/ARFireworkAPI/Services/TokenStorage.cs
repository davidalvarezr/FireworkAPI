using UnityEngine;

namespace ARFireworkAPI.Services
{
    public class TokenStorage : ITokenStorage
    {
        private static TokenStorage _instance = null;
        private static readonly object Padlock = new object();

        public static TokenStorage Instance
        {
            get
            {
                lock (TokenStorage.Padlock)
                    if (TokenStorage._instance == null)
                    {
                        TokenStorage._instance = new TokenStorage();
                    }

                return TokenStorage._instance;
            }
        }

        public const string Key = "access_token";

        private TokenStorage()
        {
        } // Constructor

        public string RetrieveAccessToken()
        {
            return PlayerPrefs.HasKey(TokenStorage.Key) ? PlayerPrefs.GetString(TokenStorage.Key) : null;
        }

        public void StoreAccessToken(string token)
        {
            PlayerPrefs.SetString(TokenStorage.Key, token);
            PlayerPrefs.Save();
        }

        public void ClearToken()
        {
            PlayerPrefs.DeleteKey(TokenStorage.Key);
            PlayerPrefs.Save();
        }
    }
}