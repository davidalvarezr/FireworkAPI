using System.Linq;
using ARFireworkAPI.Services;
using NUnit.Framework;

namespace Tests.EditorTests
{
    [TestFixture]
    public class TokenStorageTest
    {
        private ITokenStorage _tokenStorage;

        [SetUp]
        public void Init()
        {
            this._tokenStorage = TokenStorage.Instance;
        }

        [Test]
        public void TestAccessTokenShouldBeNullAfterClearing()
        {
            this._tokenStorage.ClearToken();
            var shouldBeNull = this._tokenStorage.RetrieveAccessToken();
            Assert.IsNull(shouldBeNull);
        }

        [Test]
        public void TestAccessTokenShouldBeEqualWhenStoringAndRetrieving()
        {
            var randStr = RandomString(8);
            _tokenStorage.StoreAccessToken(randStr);
            var retrieval = _tokenStorage.RetrieveAccessToken();
            Assert.AreEqual(randStr, retrieval);
        }

        // https://stackoverflow.com/questions/1344221/how-can-i-generate-random-alphanumeric-strings
        private static readonly System.Random Random = new System.Random();
        private static string RandomString(int length)
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            return new string(Enumerable.Repeat(chars, length)
                .Select(s => s[Random.Next(s.Length)]).ToArray());
        }
    }
}



