using System;
using System.Threading.Tasks;
using Microsoft.MixedReality.WebRTC;
using Microsoft.MixedReality.WebRTC.Unity;
using RealityCollective.ServiceFramework.Definitions;
using RealityCollective.ServiceFramework.Interfaces;
using RealityCollective.ServiceFramework.Modules;
using RealityToolkit.WebRTC;
using RealityToolkit.webrtc.Definitions;
using UnityEngine;
using AudioRenderer = Microsoft.MixedReality.WebRTC.Unity.AudioRenderer;
using Object = UnityEngine.Object;
using PeerConnection = Microsoft.MixedReality.WebRTC.Unity.PeerConnection;

namespace RealityToolkit.webrtc
{
    public class MicrosoftWebRTCServiceModule : BaseServiceModule, IWebRTCServiceModule
    {
        private MicrophoneSource _microphoneSource;

        public MicrosoftWebRTCServiceModule(string name, uint priority, BaseProfile profile, IService parentService) : base(name, priority, profile, parentService)
        {
        }

        public override void Initialize()
        {
            _microphoneSource = Object.FindObjectOfType<MicrophoneSource>();
            base.Initialize();
        }

        public event Action<byte[]> OnDataReceived;
        public event Action<int, Connection> OnConnectionCreated;

        public Task AddConnection(int localPeerId, int remotePeerId, bool startConnection)
        {
            //Create container gameobject to hold all components of the connection
                GameObject connectionGameObject = new GameObject($"{localPeerId}ConnectionTo{remotePeerId}");
                connectionGameObject.SetActive(false);

                //Add PeerConnection & Signaler components
                PeerConnection peerConnection = connectionGameObject.AddComponent<PeerConnection>();
                PhotonSignaler sharingSignaler = connectionGameObject.AddComponent<PhotonSignaler>();


                //Setup of the PeerConnection & Signaler
                sharingSignaler.PeerConnection = peerConnection;
                var connection = new Connection(peerConnection, sharingSignaler);
                connection.Signaler.PeerConnection = connection.PeerConnection;
                connection.Signaler.LocalPeerId = localPeerId;
                connection.Signaler.RemotePeerId = remotePeerId;

                //Add audio track for the audio communication
                MediaLine audioLine = connection.PeerConnection.AddMediaLine(MediaKind.Audio);
                audioLine.Source = _microphoneSource;
                AudioReceiver audioReceiver = connectionGameObject.AddComponent<AudioReceiver>();
                AudioRenderer audioRenderer = connectionGameObject.AddComponent<AudioRenderer>();
                audioReceiver.AudioStreamStarted.AddListener(audioRenderer.StartRendering);
                audioReceiver.AudioStreamStopped.AddListener(audioRenderer.StopRendering);
                audioLine.Receiver = audioReceiver;


                //When PeerConnection is ready we can start a connection sequence 
                peerConnection.OnInitialized.AddListener(()=>InitializePeerConnection(peerConnection,connection,startConnection));
                connectionGameObject.SetActive(true);
#if !UNITY_EDITOR
                connectionGameObject.GetComponent<AudioSource>().mute = true;
#endif
            return Task.CompletedTask;
        }

        Task IWebRTCServiceModule.AddConnection(int localPeerId, int remotePeerId, bool startConnection)
        {
            return AddConnection(localPeerId, remotePeerId, startConnection);
        }

        public void RemoveConnection(Connection connection)
        {
            Object.Destroy(connection.Signaler.gameObject);
        }

        public void SendData(Connection connection, byte[] data)
        {
            connection.DataChannel.SendMessage(data);
        }
        
        private async void InitializePeerConnection(PeerConnection peerConnection, Connection connection,bool startConnection = true)
        {
            if (startConnection)
            {
                var dataChannel = await peerConnection.Peer.AddDataChannelAsync("DataPipeline", true, true);
                dataChannel.MessageReceived += bytes => Debug.Log(System.Text.Encoding.UTF8.GetString(bytes));
                connection.DataChannel = dataChannel;
                peerConnection.StartConnection();
            }
            else
            {
                peerConnection.Peer.DataChannelAdded += channel =>
                {
                    connection.DataChannel = channel;
                    channel.MessageReceived += OnDataReceived;
                };
            }
            OnConnectionCreated?.Invoke(connection.Signaler.RemotePeerId,connection);
        }
    }
}