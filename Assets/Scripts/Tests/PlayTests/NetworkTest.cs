using NUnit.Framework;
using System;
using System.Collections;
using Random = System.Random;
using System.Globalization;
using ARFireworkAPI.Models;
using ARFireworkAPI.Services;
using UnityEngine;
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
            var firework = new Firework(FireworkType.Normal, "-1", "1,1", "-1.2");
            yield return _network.PlaceFirework(firework);
            Assert.True(_network.PlaceFireworkResponse == 200);
        }
        
        [UnityTest]
        public IEnumerator TestPlaceFireworkFailureFormat()
        {
            yield return _network.SendCode(_password); // valid auth
            Firework firework = null;
            Assert.Throws<FormatException>(() =>
            {
                firework = new Firework(FireworkType.Normal, "should not work", "11", "12");
            });

            // fireword is still null here
            
            // Assert.Throws<NullReferenceException>(() =>
            // {
            //     yield return _network.PlaceFirework(firework);
            // });
            //
            // Assert.True(_network.PlaceFireworkResponse != 200);
        }
        
        [UnityTest]
        public IEnumerator TestPlaceFireworkFailureAuth()
        {
            _network.Logout(); // remove auth (if token was stored)
            var firework = new Firework(FireworkType.Normal, "10", "11", "12");
            yield return _network.PlaceFirework(firework);
            Assert.False(_network.PlaceFireworkResponse == 200); // should not receive OK
            Assert.True(_network.PlaceFireworkResponse == 401); // should receive Unauthorized
        }

        [UnityTest]
        public IEnumerator TestBindReceiveFireworkPlacement()
        {
            yield return _network.SendCode(_password); // user authentication
            var firework = new Firework(FireworkType.Normal, RandCoordinate(), 
                RandCoordinate(), RandCoordinate());
            Debug.Log(firework);
            _network.BindReceiveFireworkPlacement((FireworkReceived fireworkReceived) =>
            {
                Assert.True(fireworkReceived.Id != 0u);
                Assert.True(fireworkReceived.Equals(firework));
            });
            yield return new WaitForSeconds(1); // Wait to be sure binding is done
            yield return _network.PlaceFirework(firework);
            yield return new WaitForSeconds(1); // Wait to be sure code has the time to go in the body of the Binding
            
        }


        private string RandCoordinate()
        {
            return ((_r.NextDouble() * 100) - 50).ToString(new CultureInfo("en-US"));
        }
    }
}