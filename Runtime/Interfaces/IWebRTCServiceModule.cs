using System;
using System.Threading.Tasks;
using RealityCollective.ServiceFramework.Interfaces;
using RealityToolkit.webrtc.Definitions;

namespace RealityToolkit.WebRTC
{
    /// <summary>
    /// Interface contract for specific identity provider implementations for use in the <see cref="IWebRTCService"/>.
    /// </summary>
    public interface IWebRTCServiceModule : IServiceModule
    {
        /// <summary>
        /// Event for when we receive data
        /// </summary>
        event Action<byte[]> OnDataReceived;

        event Action<int, Connection> OnConnectionCreated;
        
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
        void RemoveConnection(Connection connection);

        /// <summary>
        /// Send data across a webrtc connection
        /// </summary>
        /// <param name="remotePeerId">the id of the remote peer we want to send data to</param>
        /// <param name="data">Data we want to send across the a webrtc connection</param>
        /// <typeparam name="T">Type of data we want to send</typeparam>
        void SendData(Connection connection,byte[] data);
    }
}