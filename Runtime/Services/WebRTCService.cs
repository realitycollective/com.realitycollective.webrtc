using System;
using System.Threading.Tasks;
using RealityCollective.ServiceFramework.Services;

namespace RealityToolkit.WebRTC
{
    public class WebRTCService : BaseServiceWithConstructor, IWebRTCService
    {
        public Action<string> OnDataReceived { get; }
        public Task AddConnection(int localPeerId, int remotePeerId, bool startConnection)
        {
            throw new NotImplementedException();
        }

        public void RemoveConnection(int remotePeerId)
        {
            throw new NotImplementedException();
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

