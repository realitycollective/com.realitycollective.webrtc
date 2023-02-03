using System;
using System.Threading.Tasks;
using RealityCollective.ServiceFramework.Interfaces;

namespace RealityToolkit.WebRTC
{
    public interface IWebRTCService : IService
    {
        /// <summary>
        /// Event for when we receive data
        /// </summary>
        Action<byte[]> OnDataReceived { get; }
        
        /// <summary>
        /// Is the device connected to the internet?
        /// </summary>
        bool HasInternetConnection { get; }

        /// <summary>
        /// Create a connection with a remote peer
        /// </summary>
        /// <param name="localPeerId">Our local id for the peer connection</param>
        /// <param name="remotePeerId">Id of the remote peer we want to connect to</param>
        /// <param name="startConnection">Auto initialize and start the connection</param>
        /// <returns></returns>
        Task AddConnection(int localPeerId, int remotePeerId, bool startConnection);

        /// <summary>
        /// Remove a connection with a specific peer
        /// </summary>
        /// <param name="remotePeerId"></param>
        void RemoveConnection(int remotePeerId);

        /// <summary>
        /// Remove all active connections
        /// </summary>
        void RemoveAllConnections();

        /// <summary>
        /// Send data across a webrtc connection
        /// </summary>
        /// <param name="remotePeerId">the id of the remote peer we want to send data to</param>
        /// <param name="data">Data we want to send across the a webrtc connection</param>
        /// <typeparam name="T">Type of data we want to send</typeparam>
        void SendData(int remotePeerId,byte[] data);
    }
}

