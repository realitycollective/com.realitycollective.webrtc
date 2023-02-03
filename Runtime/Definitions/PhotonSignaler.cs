using System.Collections.Generic;
using System.Threading.Tasks;
using ExitGames.Client.Photon;
using Microsoft.MixedReality.WebRTC;
using Microsoft.MixedReality.WebRTC.Unity;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine;

namespace RealityToolkit.webrtc.Definitions
{
    public class PhotonSignaler : Signaler, IInRoomCallbacks
    {
        public int LocalPeerId;
        public int RemotePeerId;

        public override Task SendMessageAsync(SdpMessage message)
        {
            return SendMessageImplAsync(new DssMessage(message));
        }

        public override Task SendMessageAsync(IceCandidate candidate)
        {
            return SendMessageImplAsync(new DssMessage(candidate));
        }
        
        private Task SendMessageImplAsync(DssMessage message)
        {
            var data = System.Text.Encoding.UTF8.GetBytes(JsonUtility.ToJson(message));
            List<object> objects = new List<object>
            {
                LocalPeerId,
                RemotePeerId,
                data
            };
            var properties = new Hashtable { { SignalerMessages.RealTimeCommunication, objects } };
            PhotonNetwork.LocalPlayer.SetCustomProperties(properties);
            return Task.CompletedTask;
        }
        
        private void HandlePlayerPropertyChange( string property, object input)
        {
            switch (input)
            {
                case object[] value when property == SignalerMessages.RealTimeCommunication:
                    int remotePeerId = (int)value[0];
                    int localPeerId = (int) value[1];
                    byte[] sdpMessage = (byte[])value[2];
                    Debug.Log($"Message between {remotePeerId} and {localPeerId}");
                    if (localPeerId == PhotonNetwork.LocalPlayer.ActorNumber && remotePeerId == RemotePeerId)
                    {
                        Debug.Log("Acting on this message");
                        var json = System.Text.Encoding.UTF8.GetString(sdpMessage);
                        ProcessMessage(json);  
                    }
                    break;
            }
        }
        
        private void ProcessMessage(string json)
        {
            if (PeerConnection.Peer == null)
            {
                PeerConnection.OnInitialized.AddListener(() =>
                {
                    ProcessMessage(json);
                });
                return;
            }
            
            DssMessage msg = JsonUtility.FromJson<DssMessage>(json);
            
            if (msg != null)
            {
                // depending on what type of message we get, we'll handle it differently
                // this is the "glue" that allows two peers to establish a connection.
                //DebugLogLong($"Received SDP message: type={msg.MessageType} data={msg.Data}");
                switch (msg.MessageType)
                {
                    case DssMessage.Type.Offer:
                        // Apply the offer coming from the remote peer to the local peer
                        var sdpOffer = new SdpMessage {Type = SdpMessageType.Offer, Content = msg.Data};
                        PeerConnection.HandleConnectionMessageAsync(sdpOffer).ContinueWith(_ =>
                            {
                                // If the remote description was successfully applied then immediately send
                                // back an answer to the remote peer to acccept the offer.
                                _nativePeer.CreateAnswer();
                            },
                            TaskContinuationOptions.OnlyOnRanToCompletion |
                            TaskContinuationOptions.RunContinuationsAsynchronously);
                        break;

                    case DssMessage.Type.Answer:
                        // No need to wait for completion; there is nothing interesting to do after it.
                        var sdpAnswer = new SdpMessage {Type = SdpMessageType.Answer, Content = msg.Data};
                        _ = PeerConnection.HandleConnectionMessageAsync(sdpAnswer);
                        break;

                    case DssMessage.Type.Ice:
                        // this "parts" protocol is defined above, in OnIceCandidateReadyToSend listener
                        _nativePeer.AddIceCandidate(msg.ToIceCandidate());
                        break;

                    default:
                        Debug.Log("Unknown message: " + msg.MessageType + ": " + msg.Data);
                        break;
                }
            }
        }

        public void OnPlayerEnteredRoom(Player newPlayer)
        {
        }

        public void OnPlayerLeftRoom(Player otherPlayer)
        {
        }

        public void OnRoomPropertiesUpdate(Hashtable propertiesThatChanged)
        {
        }

        public void OnPlayerPropertiesUpdate(Player targetPlayer, Hashtable changedProps)
        {
            if (changedProps.TryGetValue(SignalerMessages.RealTimeCommunication,out var value))
            {
                HandlePlayerPropertyChange(SignalerMessages.RealTimeCommunication,value);
            }
        }

        public void OnMasterClientSwitched(Player newMasterClient)
        {
        }
    }
}