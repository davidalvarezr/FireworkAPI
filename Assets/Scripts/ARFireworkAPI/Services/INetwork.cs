﻿using System.Collections;
using System.Threading.Tasks;
using ARFireworkAPI.Models;
using RSG;

namespace ARFireworkAPI.Services
{
    /// <summary>
    /// 
    /// </summary>
    /// <param name="firework">The firework that has been placed</param>
    public delegate void OnReceiveFireworkPlacement(Firework firework);

    /// <summary>
    /// 
    /// </summary>
    /// <param name="fireworkId">All the firework id's the app needs to trigger</param>
    public delegate void OnReceiveFireworkTrigger(int[] fireworkId);

    public interface INetwork
    {
        /// <summary>
        /// Send the personnal code to the API. If successful, store the access token inside the local storage.
        /// Check the result in AuthenticationResponse
        /// </summary>
        /// <param name="code">the private code</param>
        /// <returns>AuthenticationResponse</returns>
        IEnumerator SendCode(string code);

        /// <summary>
        /// Try to send the token from the local storage to see if it already has a valid connection.
        /// Check the result in ConnectionIsStillValid property
        /// </summary>
        /// <returns></returns>
        IEnumerator CheckConnectionStillValid();

        /// <summary>
        /// Deletes the access_token from the storage
        /// </summary>
        void Logout();
        
        /// <summary>
        /// Send a http request to place the firework
        /// </summary>
        /// <param name="firework"></param>
        /// <returns>The HTTP response code: 200 if successful</returns>
        IEnumerator PlaceFirework(Firework firework);

        /// <summary>
        /// The function given in parameter will be called each time the app receives an FireworkPlacement event
        /// </summary>
        /// <param name="f">The callback function</param>
        void BindReceiveFireworkPlacement(OnReceiveFireworkPlacement f);

        /// <summary>
        /// The function given in parameter will be called each time the app receives an FireworkTrigger event
        /// </summary>
        /// <param name="f">The callback function</param>
        void BindReceiveFireworkTrigger(OnReceiveFireworkTrigger f);
    }
}