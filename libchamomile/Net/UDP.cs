#region Using
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
#endregion

namespace libchamomile.Net {
    public static class UDPSender {
        #region 로컬 변수
        private static int PortNumber { get; set; }
        private static string IPBandWidth { get; set; }
        private static UdpClient udp;
        #endregion

        #region 내부 함수
        /// <summary>
        /// UDP Sender를 사용하기 전에 Port 번호와 IPBandWidth를 설정하는 함수입니다.
        /// 자세한 사항은 UDP Broadcast 기술 문서를 참조하십시오.
        /// </summary>
        /// <param name="_PortNumber">Port 번호를 입력합니다.</param>
        /// <param name="IPBandWidth">BroadCast 대역대로 메세지를 전송하려면 192.168.0.255 형태의 BoradCast 주소를 String으로 입력합니다.</param>
        public static void SetUDPConfig(int _PortNumber, string _IPBandWidth) {
            PortNumber = _PortNumber;
            IPBandWidth = _IPBandWidth;
        }

        /// <summary>
        /// UDP 메세지를 전송합니다.
        /// </summary>
        /// <param name="message">전송할 메세지를 입력합니다.</param>
        public static void SendMessage(string message) {
            udp = new UdpClient();
            IPEndPoint ip = new IPEndPoint(IPAddress.Parse(IPBandWidth), PortNumber);

            udp.Client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
            byte[] bytes = Encoding.UTF8.GetBytes(message);

            udp.Send(bytes, bytes.Length, ip);
            udp.Close();
        }
        #endregion
    }

    public static class UDPReceiver {
        #region Delegate / Event
        /// <summary>
        /// 메세지가 수신되었을 때 발동되는 이벤트 핸들러입니다. 콜백 메세지를 사용할 경우 아래 이벤트 핸들러로 연결해주셔야 합니다.
        /// </summary>
        public delegate void ReceiveMessageHandler(string message);
        public static event ReceiveMessageHandler OnReceiveMessage;

        #endregion

        #region 로컬 변수
        public static Queue<string> MessageQueue;
        public static bool isUnityEngine = false;
        private static int PortNumber { get; set; }

        private static UdpClient udp = null;
        private static IAsyncResult ar = null;

        #endregion

        #region 내부 함수

        /// <summary>
        /// UDP BroadCast 메세지 수신을 시작합니다.
        /// </summary>
        /// <param name="_PortNumber">Port 번호를 입력합니다.</param>
        public static void StartUDP(int _PortNumber) {
            try {
                PortNumber = _PortNumber;

                if (isUnityEngine) {
                    MessageQueue = new Queue<string>();
                }

                if (udp != null) {
                    throw new Exception("이미 실행되어 있습니다. 실행 상태를 확인하시고 다시 시도하여 주십시오.");
                }

                udp = new UdpClient();

                IPEndPoint localEp = new IPEndPoint(IPAddress.Any, PortNumber);

                udp.Client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
                udp.ExclusiveAddressUse = false;
                udp.Client.Bind(localEp);

                StartListening();

            }
            catch (Exception ex) {
                throw ex;
            }
        }

        /// <summary>
        /// UDP BroadCast 메세지 수신을 중단합니다.
        /// </summary>
        public static void StopUDPReceiver() {
            try {
                if (udp != null) {
                    udp.Close();
                    udp = null;
                }
            }
            catch (Exception ex) {
                throw ex;
            }
        }

        /// <summary>
        /// BroadCast 리스닝 함수입니다.
        /// </summary>
        private static void StartListening() {
            ar = udp.BeginReceive(Receive, new object());
        }

        /// <summary>
        /// 메세지가 수신되었을때 호출되는 Callback입니다.
        /// </summary>
        private static void Receive(IAsyncResult ar) {
            try {
                if (udp != null) {
                    IPEndPoint ip = new IPEndPoint(IPAddress.Any, PortNumber);
                    byte[] bytes = udp.EndReceive(ar, ref ip);
                    string message = Encoding.UTF8.GetString(bytes);
                    StartListening();

                    if (isUnityEngine) {
                        MessageQueue.Enqueue(message);
                    }
                    else {
                        if (OnReceiveMessage != null) {
                            OnReceiveMessage(message);
                        }
                    }

                }
            }
            catch (SocketException ex) {
                throw ex;
            }
            catch (Exception ex) {
                throw ex;
            }
        }
        #endregion
    }
}
