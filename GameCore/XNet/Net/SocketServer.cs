using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using XNet.Libs.Utility;

namespace XNet.Libs.Net
{
    /// <summary>
    /// Tcp 服务器
    /// @author:xxp
    /// @date:2013/01/10
    /// </summary>
    public class SocketServer
    {

        /// <summary>
        /// 消息最大字节数
        /// </summary>
        public static int MaxReceiveSize = 1024 * 1024; //1kb

        private readonly ManualResetEvent MREvent = new ManualResetEvent(false);
        private Socket _socket;
        private volatile bool IsWorking;
        private readonly CancellationTokenSource tokenSource = new CancellationTokenSource();

        /// <summary>
        /// 最大连接数
        /// </summary>
        public int MaxClient { get; set; } = 6000;
        /// <summary>
        /// 当前连接管理
        /// </summary>
        public ConnectionManager CurrentConnectionManager { private set; get; }
        /// <summary>
        /// Listen port
        /// </summary>
        public int Port { set; get; }
        /// <summary>
        /// 消息处理管理
        /// </summary>
        public IClientMessageHandlerManager HandlerManager { set; get; }

        public SocketServer(ConnectionManager cManager, int port)
        {
            CurrentConnectionManager = cManager;
            Port = port;
        }

        private void ReceivedMessag(Client client, Message msg)
        {
            switch (msg.Class)
            {
                case MessageClass.Action:
                    {
                        client.SetActionMessage(msg);
                    }
                    break;
                case MessageClass.Ping:
                    { client?.SendMessage(msg); }
                    break;
                case MessageClass.Package:
                    {
                        var package = MessageBufferPackage.ParseFromMessage(msg);
                        foreach (var i in package.Messages)
                        {
                            ReceivedMessag(client, i);
                        }
                    }
                    break;
                default:
                    {
                        this.HandlerManager?.Handle(msg, client);
                        if (this.HandlerManager == null)
                        {
                            Debuger.LogWaring($"No handle of {msg.Class}");
                        }
                    }
                    break;
            }
            client.LastMessageTime = DateTime.UtcNow;
        }

        private void OnAccept(IAsyncResult ar)
        {
            MREvent.Set();
            if (!IsWorking) return; //server closed 
            Socket server = (Socket)ar.AsyncState;
            Socket client = server.EndAccept(ar);
            if (client == null) return; //client is empty
            client.NoDelay = true;

            client.SendTimeout = client.ReceiveTimeout = 3000;

            if (CurrentConnectionManager.Count >= MaxClient)
            {
                Debuger.LogWaring($"Client count  limit {CurrentConnectionManager.Count }/{MaxClient}");
                try { client.Close(); }
                catch { }
                return;
            }

            Debuger.DebugLog($"client {client.RemoteEndPoint} connect {CurrentConnectionManager.Count }/{MaxClient} ");
            var nClient = CurrentConnectionManager.CreateClient(client, this);
            try
            {
                _ = nClient.Socket.BeginReceive(nClient.Buffer, 0,
                    nClient.Buffer.Length,
                    SocketFlags.None,
                    new AsyncCallback(OnDataReceived), nClient);
            }
            catch (Exception ex)
            {
                HandleException(nClient, ex);
            }
        }

        private void OnDataReceived(IAsyncResult ar)
        {
            var nClient = ar.AsyncState as Client;
            try
            {
                if (!nClient.Enable) throw new Exception("Client had closed");
                int count = nClient.Socket.EndReceive(ar, out SocketError errorCode);
                if (errorCode != SocketError.Success) throw new Exception("Error Code:" + errorCode);
                if (count <= 0) throw new Exception("Clent receive No data!");
                nClient.Stream.Write(nClient.Buffer, 0, count);
                while (nClient.Stream.Read(out Message message))
                {
                    ReceivedMessag(nClient, message);
                }
                nClient?.Socket?.BeginReceive(nClient.Buffer, 0,
                        nClient.Buffer.Length,
                        SocketFlags.None,
                        new AsyncCallback(OnDataReceived), nClient);
            }
            catch (Exception ex)
            {
                HandleException(nClient, ex);
            }
        }

        private void HandleException(Client client, Exception ex)
        {
            RemoveClient(client);
        }

        public void RemoveClient(Client client)
        {
            client.Close();
            CurrentConnectionManager.RemoveClient(client);
        }

        
        /// <summary>
        /// Begin listen
        /// </summary>
        public void Start()
        {
            IsWorking = true;
            CurrentConnectionManager.Clear();
            _socket = new Socket(IPAddress.Any.AddressFamily, SocketType.Stream, ProtocolType.Tcp)
            {
                //_socket.SendTimeout = 999;
                //_socket.ReceiveTimeout = 999;
                LingerState = new LingerOption(true, 10),
                NoDelay = true
            };
            _socket.Bind(new IPEndPoint(IPAddress.Any, Port));
            _socket.Listen(2000);
            Task.Factory.StartNew(() =>
            {
                while (IsWorking)
                {
                    MREvent.Reset();
                    //Debuger.Log("Begin accept");
                    _socket.BeginAccept(new AsyncCallback(OnAccept), _socket);
                    MREvent.WaitOne();
                }
            }, tokenSource.Token);

            Debuger.DebugLog("Server Listen at port:" + Port);
        }
        /// <summary>
        /// 停止
        /// </summary>
        public void Stop()
        {
            if (IsWorking)
            {
                IsWorking = false;

                Debuger.DebugLog("Socket Server:Stoping....");
                CurrentConnectionManager.Each((t) => {
                    try { t.Close(); } catch { }
                    return false;
                });
                CurrentConnectionManager.Clear();

                try { _socket?.Close();}
                catch { }
                Debuger.DebugLog("Mrevetn set");
                MREvent.Set();
                tokenSource?.Cancel();
                _socket = null;
                Debuger.DebugLog("Socket Stoped");
            }
        }

        /// <summary>
        /// 关闭一个会话
        /// </summary>
        /// <param name="client"></param>
        public void DisConnectClient(Client client)
        {
            RemoveClient(client);
        }
    }
}
