using ARFireworkAPI.Serializable;

namespace ARFireworkAPI.Models
{
    public class AuthenticationResponse
    {
        public Person Person { get; }

        public string AccessToken { get; }
        public string StatusCode { get; }

        public string ErrorReason { get; } = null;
        
        

        public AuthenticationResponse(string statusCode, Person person, string accessToken)
        {
            Person = person;
            AccessToken = accessToken;
            StatusCode = statusCode;
        }

        public AuthenticationResponse(string statusCode, string errorReason)
        {
            StatusCode = statusCode;
            ErrorReason = errorReason;
        }

        public bool IsSuccessful()
        {
            return StatusCode == "200" && ErrorReason == null;
        }
    }
}


