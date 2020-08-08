namespace ARFireworkAPI.Services
{
    public interface ITokenStorage
    {
        /// <summary>
        /// If the access token is in the local storage, will return it. Otherwise return null
        /// </summary>
        /// <returns>the access token or null</returns>
        string RetrieveAccessToken();

        /// <summary>
        /// Stores the tokken in the local storage
        /// </summary>
        /// <param name="token"></param>
        void StoreAccessToken(string token);

        /// <summary>
        /// Clears the token from the local storage
        /// </summary>
        void ClearToken();
    }

}
