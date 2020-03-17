using System;
using System.Net.Sockets;
using System.Collections.Concurrent;
using XNet.Libs.Utility;

namespace XNet.Libs.Net
{
    public delegate void OnClientDisconnect(Client client);

	/// <summary>
	/// 连接
	/// @author:xxp
	/// @date:2013/01/10
	/// </summary>
	public class Client
	{
		/// <summary>
		/// 缓存大小
		/// </summary>
		public static int BUFFER_SIZE = 1024;

        private volatile bool IsClose;
		private readonly ConcurrentQueue<Message> _actionMessage = new ConcurrentQueue<Message>();

		public const int MAX_ACTION_BUFFER = 10;

		/// <summary>
		/// 连接ID
		/// </summary>
		public int ID { get; private set; }
		/// <summary>
		/// 是否关闭
		/// </summary>
		public bool Enable { get { return !IsClose; } }
		/// <summary>
		/// 连接断开事件 
		/// </summary>
		public event OnClientDisconnect OnDisconnect;
        /// <summary>
        /// have admission
        /// </summary>
		public bool HaveAdmission { get; set; }
		/// <summary>
		/// Gets or sets the last message time.
		/// </summary>
		/// <value>The last message time.</value>
		public DateTime LastMessageTime { set; get; }
        /// <summary>
        /// user state
        /// always use save sessionkey
        /// </summary>
		public object UserState { set; get; }
        /// <summary>
        /// Buffer
        /// </summary>
		public byte[] Buffer { private set; get; }
        /// <summary>
        /// Stream
        /// </summary>
		public MessageStream Stream { private set; get; }
        /// <summary>
        /// Server
        /// </summary>
		public SocketServer Server { private set; get; }

        /// <summary>
        /// socket
        /// </summary>
		public Socket Socket { private set; get; }

        /// <summary>
        /// create .ctor
        /// </summary>
        /// <param name="server"></param>
        /// <param name="client"></param>
        /// <param name="id"></param>
		public Client (SocketServer server, Socket client, int id)
		{
			ID = id;
			Buffer = new byte[BUFFER_SIZE];
			Stream = new MessageStream ();
			IsClose = false;
			Server = server;
			Socket = client;
			HaveAdmission = false;
		}

		/// <summary>
		/// 关闭
		/// </summary>
		public void Close()
		{
			if (IsClose) return;
			OnDisconnect?.Invoke(this);
			IsClose = true;
			try { Socket?.Close(); }
			catch { }
			Socket = null;
		}

		/// <summary>
		/// 发送一个消息
		/// </summary>
		/// <param name="message"></param>
		public void SendMessage(Message message)
		{
			BeginSendMessage(message.ToBytes());
		}


        private void OnEndSentData(IAsyncResult ar)
		{
			try
			{
				Socket.EndSend(ar);
			}
			catch (Exception ex)
			{
				HandleException(ex);
			}
		}


		private bool BeginSendMessage(byte[] msg)
		{
			try
			{
				if (IsClose) return false;
				Socket.BeginSend(msg, 0, msg.Length, SocketFlags.None,new AsyncCallback(OnEndSentData), this);
				return true;
			}
			catch (Exception ex)
			{
				HandleException(ex);
				return false;
			}
		}

		private void HandleException( Exception ex)
		{
			Server.RemoveClient(this);
			Debuger.DebugLog(ex.ToString());
		}
		/// <summary>
		/// get last action message 
		/// </summary>
		/// <param name="message"></param>
		/// <returns></returns>
		public bool TryGetActionMessage(out Message message)
		{
			if (_actionMessage.Count > 0)
				return _actionMessage.TryDequeue(out message);
			message = null;
			return false;
		}

		/// <summary>
		/// save action
		/// </summary>
		/// <param name="action"></param>
		public void SetActionMessage(Message action)
		{
			if (_actionMessage.Count >= MAX_ACTION_BUFFER)
				_actionMessage.TryDequeue(out Message _);
			_actionMessage.Enqueue(action);
		}

		public override string ToString()
		{
			return $"{UserState}-{Socket?.RemoteEndPoint}";
		}
	}

}
