
using Microsoft.MixedReality.WebRTC;
using PeerConnection = Microsoft.MixedReality.WebRTC.Unity.PeerConnection;

namespace RealityToolkit.webrtc.Definitions
{
    public struct Connection
    {
        public PeerConnection PeerConnection;
        public PhotonSignaler Signaler;
        public DataChannel DataChannel;
        
        public Connection(PeerConnection peerConnection, PhotonSignaler sharingSignaler, DataChannel dataChannel = null)
        {
            PeerConnection = peerConnection;
            Signaler = sharingSignaler;
            DataChannel = dataChannel;
        }
    }
}