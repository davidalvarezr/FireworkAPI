using System;

namespace ARFireworkAPI.Serializable
{
    [Serializable]
    public class Credentials
    {
        public string password;

        public Credentials(string password)
        {
            this.password = password;
        }
    }
}