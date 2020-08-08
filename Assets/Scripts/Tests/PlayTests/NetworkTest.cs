using NUnit.Framework;
using System;
using System.Collections;
using Random = System.Random;
using System.Globalization;
using ARFireworkAPI.Models;
using ARFireworkAPI.Services;
using UnityEngine.TestTools;
using Network = ARFireworkAPI.Services.Network;

namespace Tests.PlayTests
{
    public class NetworkTest
    {
        private Random _r;
        private Network _network;
        private TokenStorage _tokenStorage;
        private string _password;
        private const string FailingPassword = "i'm sure that no body will set this password...";

        [SetUp]
        public void Init()
        {
            _r = new Random();
            _network = Network.Instance;
            _tokenStorage = TokenStorage.Instance;
            new EnvSetter();
            _password = Environment.GetEnvironmentVariable("password");
        }

        [UnityTest]
        public IEnumerator TestLogin()
        {
            yield return _network.SendCode(_password);
            Assert.True(_network.AuthenticationResponse.IsSuccessful());
            Assert.IsNotNull(_network.AuthenticationResponse.Person.name);
            Assert.IsNotNull(_network.AuthenticationResponse.Person.id);
        }

        [UnityTest]
        public IEnumerator TestLoginFailure()
        {
            yield return _network.SendCode(FailingPassword);
            Assert.False(_network.AuthenticationResponse.IsSuccessful());
        }

        [UnityTest]
        public IEnumerator TestConnectionStillValid()
        {
            yield return _network.SendCode(_password); // valid auth
            yield return _network.CheckConnectionStillValid();
            Assert.True(_network.ConnectionIsStillValid);
        }


        [UnityTest]
        public IEnumerator TestLogout()
        {
            yield return _network.SendCode(_password);
            _network.Logout();
            yield return _network.CheckConnectionStillValid();
            Assert.False(_network.ConnectionIsStillValid);
        }
        
        [UnityTest]
        public IEnumerator TestPlaceFirework()
        {
            yield return _network.SendCode(_password); // valid auth
            var firework = new Firework("User Test", FireworkType.Normal, "10", "11", "12");
            yield return _network.PlaceFirework(firework);
            Assert.True(_network.PlaceFireworkResponse == 200);
        }

        // TODO: try to fail the placement of a Firework
        [Test]
        public void TestPlaceFireworkFailure()
        {
            Assert.True(false);
            _tokenStorage.ClearToken();
            var x = RandCoordinate();
            var y = RandCoordinate();
            var z = RandCoordinate();
            var firework = new Firework("Jean Dujardin", FireworkType.Normal, x, y, z);
            var statusCode = _network.PlaceFirework(firework);
            Assert.AreNotEqual(statusCode, 200);
        }

        [Test]
        public void TestBindReceiveFireworkPlacement()
        {
            Assert.True(false);
            // Firework firework = null;
            // var x = RandCoord();
            // var y = RandCoord();
            // var z = RandCoord();
            // var fireworkToSend = new Firework("Jean Dujardin", FireworkType.Normal, x, y, z);
            // _network.sendCode(_password); // user authentication
            // _network.bindReceiveFireworkPlacement((Firework fireworkReceived) =>
            // {
            //     Debug.Log("Inside bindReceiveFireworkPlacement");
            //     firework = fireworkReceived;
            // });
            // _network.placeFirework(fireworkToSend);
            // await Task.Delay(2000).ConfigureAwait(false);
            // Assert.True(fireworkToSend.Equals(firework));
        }


        private string RandCoordinate()
        {
            return ((_r.NextDouble() * 100) - 50).ToString(new CultureInfo("en-US"));
        }
    }
}