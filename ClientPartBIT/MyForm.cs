using System;
using System.Configuration;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Windows.Forms;

namespace ClientPartBIT
{
	public partial class MyForm : Form
	{
		public class ReceiveState
		{
			public Socket RcvSocket = null;
			public const int BufferSize = 1024;
			public byte[] Buff = new byte[BufferSize];
			public StringBuilder Str = new StringBuilder();
		}

		public string ServerName { get; set; }
		public static ManualResetEvent ConnectionDone = new ManualResetEvent(false);
		public static ManualResetEvent SendDone = new ManualResetEvent(false);
		public static ManualResetEvent ReceiveDone = new ManualResetEvent(false);
		public static string response = string.Empty;

		private IPAddress _address;
		public MyForm()
		{
			InitializeComponent();
			RunMonitor();
		}

		private static void ConnectionCallback(IAsyncResult res)
		{
			try
			{
				Socket client = (Socket)res.AsyncState;
				client.EndConnect(res);
				ConnectionDone.Set();
				Console.WriteLine("Client connected.");
			}
			catch (Exception ex)
			{
				Console.WriteLine(ex);
			}
		}
		private static void SendCallback(IAsyncResult res)
		{
			try
			{
				Socket client = (Socket)res.AsyncState;
				/// надо бы где-то показать...
				int bytesSent = client.EndSend(res);
				Console.WriteLine("Sent {0} bytes to server!", bytesSent);
				SendDone.Set();
			}
			catch(Exception ex)
			{
				Console.WriteLine(ex);
			}
		}
		private static void Send(Socket client, string text)
		{
			try
			{
				byte[] byteData = Encoding.Unicode.GetBytes(text);
				client.BeginSend(
					byteData,
					0,
					byteData.Length,
					0,
					new AsyncCallback(SendCallback),
					client);
			}
			catch(Exception ex)
			{
				Console.WriteLine(ex);
			}
		}
		private static void Receive(Socket client)
		{
			try
			{
				ReceiveState rcv = new ReceiveState();
				rcv.RcvSocket = client;
				client.BeginReceive(
					rcv.Buff,
					0,
					ReceiveState.BufferSize,
					0,
					new AsyncCallback(ReceiveCallback),
					rcv
				);
			}
			catch(Exception ex)
			{
				Console.WriteLine(ex);
			}
		}
		private static void ReceiveCallback(IAsyncResult res)
		{
			try
			{
				ReceiveState state = (ReceiveState)res.AsyncState;
				Socket client = state.RcvSocket;
				int BytesReceived = client.EndReceive(res);
				if (BytesReceived > 0)
				{
					state.Str.Append(Encoding.Unicode.GetString(state.Buff, 0, BytesReceived));
					client.BeginReceive(
						state.Buff,
						0,
						ReceiveState.BufferSize,
						0,
						new AsyncCallback(ReceiveCallback),
						state
					);
				}
				else
				{
					/// получили всё
					/// 
					if (state.Str.Length > 1)
					{
						/// сохр полученную строку...
						response = state.Str.ToString();
					}
					ReceiveDone.Set();
				}
			}
			catch(Exception ex)
			{
				Console.WriteLine(ex);
			}
		}

		private void RunMonitor()
		{
			try
			{
				ServerName = Properties.Settings.Default.ServerName.ToString();
				if (String.IsNullOrWhiteSpace(ServerName)) throw new Exception(@"Не указано имя сервера для соединения!");

				IPHostEntry ipHost = Dns.GetHostEntry(ServerName);
				IPAddress[] ipAddresses = ipHost.AddressList;
				foreach (IPAddress ip in ipAddresses)
				{
					if (ip.AddressFamily == AddressFamily.InterNetwork)
					{
						_address = ip;
						break;
					}
				}
				if (_address == null) throw new Exception("Адрес IPv4 не найден!");
				IPEndPoint remoteEndPoint = new IPEndPoint(_address, 11111);

				Socket client = new Socket(_address.AddressFamily,
						SocketType.Stream, ProtocolType.Tcp);

				client.BeginConnect(
					remoteEndPoint,
					new AsyncCallback(ConnectionCallback),
					client);
				ConnectionDone.WaitOne();

				while (true)
				{
					IDataObject data = Clipboard.GetDataObject();
					if (data.GetDataPresent(DataFormats.StringFormat))
					{
						string stringToSend = (string)data.GetData(DataFormats.StringFormat);
						Console.WriteLine("Получено из клипборда: {0}", stringToSend);
						/// впихиваем строку клиенту...

						Send(client, stringToSend);
						SendDone.WaitOne();

						Receive(client);
						ReceiveDone.WaitOne();

						Console.WriteLine("Получен ответ: {0}", response);

						client.Shutdown(SocketShutdown.Both);
						client.Disconnect(true);

						break;
					}
					Thread.Sleep(1000);
				}
				client.Close();
				client.Dispose();
			}
			catch (Exception ex)
			{
				Console.WriteLine(ex);
			}
		}
	}
}
