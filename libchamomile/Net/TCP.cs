#region Using
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
#endregion

/****************************libchamomile.NET.TCP 사용시 주의사항**********************************

TCP 서버, TCP Client를 구축할 시에는 서버와 클라이언트 상호간의 메세지를 수신할 각각의 GetMessageDelegateEvent를 
초기화 부분에 연결해주어야 합니다.
*/

namespace libchamomile.Net {
    /// <summary>
    /// TCP 서버를 구축할 수 있는 클래스입니다.
    /// </summary>
    public static class TCPServer {
        #region Delegate / Event
        public delegate void ClientConnectedDelegate(TcpClient Socket, string IP);
        public static event ClientConnectedDelegate OnClientConnectedEvent;

        public delegate void OnReceiveMessageDelegate(string Message);
        public static event OnReceiveMessageDelegate OnReceiveMessageEvent;
        #endregion

        #region 로컬 변수
        private static TcpListener ServerSocket;
        private static TcpClient ClientSocket = default(TcpClient);

        private static bool ServerStarted = false;
        #endregion

        #region 내부 함수
        /// <summary>
        /// TCP 서버를 시작합니다.
        /// </summary>
        /// <param name="Port">포트를 입력합니다.</param>
        public static void StartServer(int Port) {
            try {
                if (ServerStarted == false) {
                    ServerSocket = new TcpListener(IPAddress.Any, Port);
                    ServerSocket.Start();
                    ServerStarted = true;

                    while (true) {
                        ClientSocket = ServerSocket.AcceptTcpClient();

                        OnClientConnectedEvent(ClientSocket, ((IPEndPoint)ClientSocket.Client.RemoteEndPoint).Address.ToString());
                        Thread thread = new Thread(() => WorkerThreadFunc(ClientSocket));
                        thread.Start();
                    }
                }
            }
            catch (SocketException ex) {
                throw ex;
            }

        }

        /// <summary>
        /// 서버에 접속한 클라이언트에게 메세지를 보냅니다.
        /// </summary>
        /// <param name="Socket">클라이언트 소켓을 입력합니다.</param>
        /// <param name="Message">보낼 메세지를 입력합니다.</param>
        public static void SendMessage(TcpClient Socket, string Message) {
            try {
                byte[] SendBytes = null;
                SendBytes = Encoding.UTF8.GetBytes(Message);

                NetworkStream NetStream = Socket.GetStream();
                NetStream.Write(SendBytes, 0, SendBytes.Length);
                NetStream.Flush();

            }
            catch (SocketException ex) {
                throw ex;
            }
        }

        /// <summary>
        /// 서버에 접속한 모든 클라이언트에게 메세지를 보냅니다.
        /// </summary>
        /// <param name="Message">보낼 메세지를 입력합니다.</param>
        public static void SendMessageToAllClient(TcpClient[] Socket, string Message) {
            try {
                byte[] SendBytes = null;
                SendBytes = Encoding.UTF8.GetBytes(Message);

                foreach (var sock in Socket) {
                    NetworkStream NetStream = sock.GetStream();
                    NetStream.Write(SendBytes, 0, SendBytes.Length);
                    NetStream.Flush();
                }

            }
            catch (SocketException ex) {
                throw ex;
            }
        }

        /// <summary>
        /// 서버를 중단합니다.
        /// </summary>
        public static void StopServer() {
            try {
                ServerSocket.Stop();

            }
            catch (SocketException ex) {
                throw ex;
            }
        }
        #endregion

        #region Worker 함수
        /// <summary>
        /// 클라이언트 접속 시 멀티 스레드로 구동되는 Worker 함수입니다.
        /// </summary>
        private static void WorkerThreadFunc(TcpClient Socket) {
            byte[] ReceiveBytes = new byte[1000];
            string ClientMessage = null;

            while (true) {
                NetworkStream NetStream = Socket.GetStream();
                int length = NetStream.Read(ReceiveBytes, 0, ReceiveBytes.Length);

                ClientMessage = Encoding.UTF8.GetString(ReceiveBytes, 0, length);
                ClientMessage = ClientMessage.Trim();
                OnReceiveMessageEvent(ClientMessage);
                Thread.Sleep(1);
            }
        }
        #endregion
    }

    /// <summary>
    /// 서버에 접속할 수 있는 TCP 클라이언트 클래스 입니다.
    /// </summary>
    public static class TCPClient {
        #region Delegate / Event
        public delegate void OnReceiveMessageDelegate(string Message);
        public static event OnReceiveMessageDelegate OnReceiveMessageEvent;

        public delegate void OnDisconnectedServerDelegate(SocketException ex);
        public static event OnDisconnectedServerDelegate OnDisconnectedServerEvent;
        #endregion

        #region 로컬 변수
        private static NetworkStream ServerSocket;
        private static TcpClient ClientSocket = new TcpClient();

        private static Thread ReceiveThread;

        public static bool isConnected = false;

        public static Queue<string> MessageQueue;
        public static bool isUnityEngine = false;
        #endregion

        #region 내부 함수
        /// <summary>
        /// TCP 서버에 접속합니다.
        /// </summary>
        /// <param name="ServerIP">접속할 서버 IP를 입력합니다.</param>
        /// <param name="PortNumber">포트 번호를 입력합니다.</param>
        public static void ConnectServer(string ServerIP, int PortNumber) {
            try {
                if (isUnityEngine) {
                    MessageQueue = new Queue<string>();
                }

                ClientSocket.Connect(ServerIP, PortNumber);

                ReceiveThread = new Thread(Receiver);
                ReceiveThread.Start();

                isConnected = true;
            }
            catch (SocketException ex) {
                throw ex;
            }

        }

        /// <summary>
        /// 서버로부터 메세지를 수신하는 함수입니다.
        /// </summary>
        private static void Receiver() {
            try {
                while (true) {
                    ServerSocket = ClientSocket.GetStream();
                    byte[] ReceiveBytes = new byte[1000];

                    int length = ServerSocket.Read(ReceiveBytes, 0, ReceiveBytes.Length);
                    string receiveMessage = Encoding.UTF8.GetString(ReceiveBytes, 0, length);

                    if (isUnityEngine) {
                        MessageQueue.Enqueue(receiveMessage);
                    }
                    else {
                        if (OnReceiveMessageEvent != null) {
                            OnReceiveMessageEvent(receiveMessage);
                        }
                    }
                    Thread.Sleep(1);
                }
            }
            catch (SocketException ex) {
                OnDisconnectedServerEvent(ex);
            }

        }

        /// <summary>
        /// 서버에게 메세지를 전송합니다.
        /// </summary>
        /// <param name="Message">보낼 메세지를 입력합니다.</param>
        public static void SendMessage(string Message) {
            try {
                byte[] SendBytes = Encoding.UTF8.GetBytes(Message);
                ServerSocket.Write(SendBytes, 0, SendBytes.Length);
                ServerSocket.Flush();
            }
            catch (SocketException ex) {
                throw ex;
            }
        }

        /// <summary>
        /// 서버와의 접속을 종료합니다.
        /// </summary>
        public static void DisconnectServer() {
            try {
                isConnected = false;
                ReceiveThread.Abort();
                ClientSocket.Close();
            }
            catch (SocketException ex) {
                throw ex;
            }
        }
        #endregion
    }
}
