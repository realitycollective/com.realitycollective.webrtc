using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using RealityCollective.ServiceFramework.Services;
using UnityEditor.MemoryProfiler;
using UnityEngine;

namespace RealityToolkit.WebRTC
{
    public class WebRTCService : BaseServiceWithConstructor, IWebRTCService
    {
        public Action<string> OnDataReceived { get; }
        private Dictionary<int,Connection> _connections = new Dictionary<int,Connection>();

        public Task AddConnection(int localPeerId, int remotePeerId, bool startConnection)
        {
            throw new NotImplementedException();
        }

        public void RemoveConnection(int remotePeerId)
        {
            if (_connections.ContainsKey(remotePeerId))
            {
                //GameObject.Destroy(_connections[remotePeerId].Signaler.gameObject);
                _connections.Remove(remotePeerId);
            }
        }

        public void RemoveAllConnections()
        {
            throw new NotImplementedException();
        }

        public void SendData<T>(int remotePeerId, T data)
        {
            throw new NotImplementedException();
        }
    }
}

