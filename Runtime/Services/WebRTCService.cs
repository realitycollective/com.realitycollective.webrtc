using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using RealityCollective.ServiceFramework.Definitions;
using RealityCollective.ServiceFramework.Interfaces;
using RealityCollective.ServiceFramework.Services;
using RealityToolkit.webrtc.Definitions;


namespace RealityToolkit.WebRTC
{
    [System.Runtime.InteropServices.Guid("fee55586-d74b-453d-aecc-c42468ed61c1")]
    public class WebRTCService : BaseServiceWithConstructor, IWebRTCService
    {
        /// <inheritdoc />
        public event Action<byte[]> OnDataReceived;
        public bool HasInternetConnection => HasInternet();

        #region Private Properties
        private Dictionary<int, Connection> connections;
        #endregion

        #region IService Implementation
        /// <inheritdoc />
        public WebRTCService(string name, uint priority, BaseProfile profile) : base(name, priority)
        {
        }
        
        /// <inheritdoc />
        public override void Initialize()
        {
            if (HasInternetConnection)
            {
                connections = new Dictionary<int, Connection>();
                foreach (var serviceModule in ServiceModules)
                {
                    ((IWebRTCServiceModule)serviceModule).OnConnectionCreated += OnConnectionCreated;
                    ((IWebRTCServiceModule)serviceModule).OnDataReceived += OnDataReceived;
                }
                base.Initialize();
            }
        }
        #endregion
        

        #region IWebRTCService Implementation
        /// <inheritdoc />
        public Task AddConnection(int localPeerId, int remotePeerId, bool startConnection, IServiceModule module)
        {
            foreach (var serviceModule in ServiceModules)
            {
                if (Equals(serviceModule, module))
                {
                    (serviceModule as IWebRTCServiceModule)?.AddConnection(localPeerId,remotePeerId,startConnection);
                }
            }

            return Task.CompletedTask;
        }

        /// <inheritdoc />
        public void RemoveConnection(int remotePeerId, IServiceModule module = null)
        {
            if (!connections.ContainsKey(remotePeerId))
            {
                return;
            }
            
            foreach (var serviceModule in ServiceModules)
            {
                if (Equals(serviceModule, module))
                {
                    (serviceModule as IWebRTCServiceModule)?.RemoveConnection(connections[remotePeerId]);
                }
            }
            connections.Remove(remotePeerId);
        }

        /// <inheritdoc />
        public void RemoveAllConnections(IServiceModule module = null)
        {
            foreach (var serviceModule in ServiceModules)
            {
                if (Equals(serviceModule, module))
                {
                    foreach (var connection in connections)
                    {
                        (serviceModule as IWebRTCServiceModule)?.RemoveConnection(connection.Value);
                    }
                }
            }
            connections.Clear();
        }

        /// <inheritdoc />
        public void SendData(int remotePeerId, byte[] data, IServiceModule module = null)
        {
            foreach (var serviceModule in ServiceModules)
            {
                if (Equals(serviceModule, module))
                {
                    if (connections.TryGetValue(remotePeerId, out var connection))
                    {
                        connection.DataChannel.SendMessage(data);
                    }
                }
            }
        }
        #endregion IWebRTCService Implementation

        #region Private Functions

        private void OnConnectionCreated(int remotePeerID,Connection connection)
        {
            connections.Add(remotePeerID,connection);
        }

        private bool HasInternet()
        {
            try
            {
                using var client = new WebClient();
                using var stream = client.OpenRead("https://www.google.com");
                return true;
            }
            catch
            {
                return false;
            }
        }
        #endregion
        
    }
}

