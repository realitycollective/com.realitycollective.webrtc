using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.MixedReality.WebRTC;
using Microsoft.MixedReality.WebRTC.Unity;
using RealityCollective.ServiceFramework.Definitions;
using RealityCollective.ServiceFramework.Services;
using RealityToolkit.webrtc.Definitions;
using UnityEngine;
using AudioRenderer = Microsoft.MixedReality.WebRTC.Unity.AudioRenderer;
using PeerConnection = Microsoft.MixedReality.WebRTC.Unity.PeerConnection;

namespace RealityToolkit.WebRTC
{
    public class WebRTCService : BaseServiceWithConstructor, IWebRTCService
    {
        public Action<string> OnDataReceived { get; }
        private Dictionary<int,Connection> _connections;
        private MicrophoneSource _microphoneSource;

        public WebRTCService(string name, uint priority, BaseProfile profile) : base(name, priority)
        {
        }
        
        public override void Initialize()
        {
            _connections = new Dictionary<int, Connection>();
            _microphoneSource = GameObject.FindObjectOfType<MicrophoneSource>();
            base.Initialize();
        }

        public async Task AddConnection(int localPeerId, int remotePeerId, bool startConnection)
        {
            if (!_connections.ContainsKey(remotePeerId))
            {
                //Create container gameobject to hold all components of the connection
                GameObject connectionGameObject = new GameObject($"{localPeerId}ConnectionTo{remotePeerId}");
                connectionGameObject.SetActive(false);

                //Add PeerConnection & Signaler components
                PeerConnection peerConnection = connectionGameObject.AddComponent<PeerConnection>();
                PhotonSignaler sharingSignaler = connectionGameObject.AddComponent<PhotonSignaler>();


                //Setup of the PeerConnection & Signaler
                sharingSignaler.PeerConnection = peerConnection;
                Connection connection;
                connection = new Connection(peerConnection, sharingSignaler);
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
                peerConnection.OnInitialized.AddListener(async () =>
                {
                    if (startConnection)
                    {
                        var dataChannel = await peerConnection.Peer.AddDataChannelAsync("DataPipeline", true, true);
                        dataChannel.MessageReceived += bytes => Debug.Log(System.Text.Encoding.UTF8.GetString(bytes));
                        connection.DataChannel = dataChannel;
                        _connections[connection.Signaler.RemotePeerId] = connection;
                        peerConnection.StartConnection();
                    }
                    else
                    {
                        peerConnection.Peer.DataChannelAdded += channel =>
                        {
                            connection.DataChannel = channel;
                            _connections[connection.Signaler.RemotePeerId] = connection;
                            channel.MessageReceived += bytes => Debug.Log(System.Text.Encoding.UTF8.GetString(bytes));
                        };
                    }
                });
                connectionGameObject.SetActive(true);
#if !UNITY_EDITOR
                connectionGameObject.GetComponent<AudioSource>().mute = true;
#endif
                _connections.Add(remotePeerId, connection);
            }
        }

        public void RemoveConnection(int remotePeerId)
        {
            if (_connections.ContainsKey(remotePeerId))
            {
                GameObject.Destroy(_connections[remotePeerId].Signaler.gameObject);
                _connections.Remove(remotePeerId);
            }
        }

        public void RemoveAllConnections()
        {
            foreach (var connection in _connections)
            {
                GameObject.Destroy(connection.Value.Signaler.gameObject);
            }
            _connections.Clear();
        }

        public void SendData(int remotePeerId, byte[] data)
        {
            if (_connections.TryGetValue(remotePeerId,out var connection))
            {
                connection.DataChannel.SendMessage(data);
            }
        }
    }
}

