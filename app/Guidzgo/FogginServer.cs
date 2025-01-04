using System;
using System.Buffers.Text;
using System.CodeDom;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Net.WebSockets;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace Guidzgo
{
	public class FogginServer
	{
		public IPAddress IP;
		public int Port;
		public FogginServer(IPAddress ip, int port)
		{
			IP = ip;
			Port = port;
		}
		public CancellationTokenSource cts = null;
		public Task serverTask = Task.CompletedTask;
		public async Task StartAsync()
		{
			await StopInternalAsync();
			cts = new CancellationTokenSource();
			samefor = new SemaphoreSlim(clMax);
			serverTask = Task.Run(Loop);
		}

		public FogginClient[] Clients = new FogginClient[clMax];

		public ClientUpdateHandler ClientJoined = x => Task.CompletedTask;
		public ClientUpdateHandler ClientLeft = x => Task.CompletedTask;

		public delegate Task ClientUpdateHandler(FogginClient client);

		private async Task StopInternalAsync()
		{
			if (cts != null)
			{
				cts.Cancel();
				tcp.Stop();
				await serverTask;
				cts.Dispose();
				cts = null;
			}
			if (samefor != null)
			{
				samefor.Dispose();
				samefor = null;
			}
		}

		public TcpListener tcp = null;
		public async Task Loop()
		{
			tcp = new TcpListener(IP, Port);
			tcp.Start(clMax - 1);
			bool insideLoop = false;
			try
			{
				while (true)
				{
					await samefor.WaitAsync(cts.Token);
					insideLoop = true; // space taken
					var cl = new FogginClient()
					{
						cl = await tcp.AcceptTcpClientAsync(),
						serv = this,
						Token = cts.Token
					};
					for (int i = 0; i < clMax; i++)
					{
						if (Clients[i] == null)
						{
							Clients[i] = cl;
							cl.whoami = i;
							break;
						}
					}
					cl.task = cl.Loop(); // queued for taking space
					samefor.Release(); // space released for queued
					insideLoop = false;
					cts.Token.ThrowIfCancellationRequested();
				}
			}	
			catch
			{
				if (cts.Token.IsCancellationRequested)
				{
					try
					{
						await Task.WhenAll(from x in Clients where x != null select x.FuckOffGracefullyAsync()); // fuck off all clients
					}
					catch (Exception e)
					{

					}
					

					// wait for clients to exit semaphore
					int count = clMax - (insideLoop ? 1 : 0);
					for (int i = 0; i < count; i++)
					{
						await samefor.WaitAsync();
					}
					
				}
				else
				{
					
				}
			}
			tcp.Stop();
		}
		const int clMax = 20;

		public SemaphoreSlim samefor;
		public async Task StopAsync()
		{
			await StopInternalAsync();
		}
	}

	public class FogginClient
	{
		public TcpClient cl;
		public NetworkStream ns;
		public Task task;
		public FogginServer serv;
		public CancellationToken Token;
		public int whoami = -1;


		public static byte[] ERR404 = Get404();

		private static byte[] Get404()
		{
			const string msg = "yo balls itch";
			byte[] payload = Encoding.ASCII.GetBytes(msg);
			return Encoding.ASCII.GetBytes(
				"HTTP/1.1 404 Not Found\r\n" +
				"Content-Length: " + payload.Length + "\r\n" +
				"Connection: close\r\n" +
				"\r\n").Concat(payload).ToArray();
		}

		public CancellationTokenSource localCts = null;

		public async Task Loop()
		{
			bool gotSema = false;
			try
			{
				await serv.samefor.WaitAsync(Token);
				gotSema = true;
				localCts = CancellationTokenSource.CreateLinkedTokenSource(Token);
				Token = localCts.Token;
				await DoHttp();
				Processor.Token = Token;
				Processor.Start(this);
				await serv.ClientJoined(this);
				// connection as socket estabilished
				try
				{
					await Task.Delay(-1, Token);
				}
				catch
				{
					try
					{
						await FuckOffGracefullyAsync();
					}
					catch
					{

					}
				}

			}
			catch (Nah)
			{
				// this client is bad so we gunna be a poor lil http server that don understand it
				await FuckOffAsync();
			}
			catch
			{
				await FuckOffAsync();
			}
			try
			{
				cl.Client.Shutdown(SocketShutdown.Both);
			}
			catch { }
			await recv.WaitAsync();
			cl.TryDispose();
			ns.TryDispose();
			recv.Dispose();
			try
			{
				await serv.ClientLeft(this);
			}
			catch { }
			Closed = true;
			serv.Clients[whoami] = null;
			if (gotSema)
				serv.samefor.Release();
		}

		private async Task FuckOffAsync()
		{
			try
			{
				try
				{
					await ns.WriteAsync(ERR404, 0, ERR404.Length);
					await ns.FlushAsync();
				}
				catch
				{
					using (var s = GetStream())
					{
						await s.WriteAsync(ERR404, 0, ERR404.Length);
						await s.FlushAsync();
					}
				}
				
				/*cl.Client.Send(ERR404);
				cl.Client.LingerState.Enabled = true;
				cl.Client.LingerState.LingerTime = 5000;
				try
				{
					Thread.Sleep(5000);
					//cl.Client.Close();
				}
				catch { }*/

			}
			catch
			{

			}
			
		}

		public Func<string, bool> RequestOk = x => true;
		public bool Closing = false;
		async Task DoHttp()
		{
			ns = GetStream();
			string resp = null;
			using (var registration = Token.Register(ns.TryDispose))
			using (StreamReader sr = new StreamReader(ns, Encoding.UTF8, true, 1024, leaveOpen: true))
			{
				var first = await sr.ReadLineAsync();
				string[] vals = first.Split(' ');
				if (vals[0] == "GET" && RequestOk(vals[1]) && HTTPHighEnough(vals[2]))
				{
					// compute headers
					while (true)
					{
						string s = await sr.ReadLineAsync();
						if (s.Length == 0)
							break;
						var key = s.Substring(0, s.IndexOf(':'));
						if (!Headers.ContainsKey(key))
							Headers.Add(key, GetValues(s.Substring(key.Length + 1)));
						else
							Headers[key] = Headers[key].Concat(GetValues(s.Substring(key.Length + 1)));
					}

					// accept ws client

					var key2 = Headers["Sec-WebSocket-Key"].First();
					//var proto = Headers["Sec-WebSocket-Protocol"].ToArray();
					var ver = int.Parse(Headers["Sec-WebSocket-Version"].First());

					if (/*!proto.Contains("chat") || */ver != 13)
						throw new Nah();

					string acceptStr;

					using (var sha = SHA1.Create())
						acceptStr = Convert.ToBase64String(sha.ComputeHash(Encoding.ASCII.GetBytes(key2 + ProtoConstant)));

					resp = "HTTP/1.1 101 Switching Protocols\r\n" +
						"Upgrade: websocket\r\n" +
						"Connection: Upgrade\r\n" +
						"Sec-WebSocket-Accept: " + acceptStr + "\r\n" +
						//"Sec-WebSocket-Protocol: chat\r\n" +
						"Ligma: yes\r\n" +
						"\r\n";
				}
				else
					throw new Nah();
			}

			byte[] buf = Encoding.ASCII.GetBytes(resp);
			ns.Write(buf, 0, buf.Length);
		}

		private NetworkStream GetStream() => new NetworkStream(cl.Client, false);

		public enum Opcode : byte
		{
			Continuation = 0,
			Text = 1,
			Binary = 2,
			Close = 8,
			Ping = 9,
			Pong = 10,
			Unknown = 255
		}

		public Opcode Get(byte b)
		{
			foreach (var o in Enum.GetValues(typeof(Opcode)).Cast<byte>())
			{
				if (o == b)
					return (Opcode)o;
			}
			return Opcode.Unknown;
		}

		public async Task SendAsync(string str, Opcode opcode = Opcode.Text, CancellationToken token = default)
		{
			if (Closed)
				throw new SocketClosed();
			byte[] payload = Encoding.UTF8.GetBytes(str);
			await SendAsync(payload, opcode, token);
		}

		public async Task SendAsync(byte[] payload, Opcode opcode = Opcode.Binary, CancellationToken token = default)
		{
			if (Closed)
				throw new SocketClosed();
			using (var cts = CancellationTokenSource.CreateLinkedTokenSource(token, Token))
			{
				if (!Closing)
					token = cts.Token;
				int extraSize = payload.Length > 125 ? (payload.Length > ushort.MaxValue ? 8 : 2) : 0;

				byte[] buf = new byte[2 + extraSize];
				buf[0] = (byte)(1 << 7 | (byte)opcode);

				switch (extraSize)
				{
					case 0:
						buf[1] = (byte)payload.Length;
						break;

					case 2:
						buf[1] = 126;
						BitConverter.GetBytes((ushort)payload.Length).Reverse().ToArray().CopyTo(buf, 2);
						break;

					case 8:
						buf[1] = 127;
						BitConverter.GetBytes((ulong)payload.LongLength).Reverse().ToArray().CopyTo(buf, 2);
						break;
				}
				await ns.WriteAsync(buf, 0, buf.Length, token);
				await ns.WriteAsync(payload, 0, payload.Length, token);
			}
		}

		private SemaphoreSlim recv = new SemaphoreSlim(1);

		public async Task<ReceivedPacket> ReceiveAsync(bool finishClose = true, CancellationToken token = default)
		{
			if (Closed)
				throw new SocketClosed();
			try
			{
				await recv.WaitAsync(token);
				using (CancellationTokenSource cts = CancellationTokenSource.CreateLinkedTokenSource(token, Token))
				{ // TODO: do not create cts if not needed
					if (!Closing)
						token = cts.Token;
					byte[] buf = new byte[4096];
					var m = new ManualResetEvent(false);
					await ns.ReadExactlyAsync(buf, 0, 1, token);
					bool final(byte b) => (b & (1 << 7)) != 0;
					bool fin = final(buf[0]);
					Opcode op = Get((byte)(buf[0] & 0x0F));
					Opcode type = op;
					if (type == Opcode.Text || type == Opcode.Binary || type == Opcode.Close)
					{
						if (!fin)
						{
							if (Closing) // fucking w me, so just kick the client off
							{
								await FuckOffAsync();
							}
							else
							{
								await FuckOffGracefullyAsync();
							}
							throw new NotSupportedException("no non-fin packets!!!");
						}
						if (op == Opcode.Close)
						{

						}
						await ns.ReadExactlyAsync(buf, 1, 1, token);
						bool masked = (buf[1] & (1 << 7)) != 0;
						buf[1] &= 0b01111111; // clear masked bit
						int size = 0;
						int index = 2;
						switch (buf[1])
						{
							case 126:
								await ns.ReadExactlyAsync(buf, 2, 2, token);
								ushort sz2 = BitConverter.ToUInt16(buf.Skip(2).Take(2).Reverse().ToArray(), 0);
								if (sz2 > (ulong)(buf.Length - index - 2))
								{
									if (Closing) // fucking w me, so just kick the client off
									{
										await FuckOffAsync();
									}
									else
									{
										await FuckOffGracefullyAsync();
									}
									throw new NotSupportedException("Payload size too big (was gonna be " + sz2 + ")");
								}
								size = sz2;
								index += 2;
								break;

							case 127:
								await ns.ReadExactlyAsync(buf, 2, 2, token);
								ulong sz = BitConverter.ToUInt64(buf.Skip(2).Take(8).Reverse().ToArray(), 0);
								if (sz > (ulong)(buf.LongLength - index - 8))
								{
									if (Closing) // fucking w me, so just kick the client off
									{
										await FuckOffAsync();
									}
									else
									{
										await FuckOffGracefullyAsync();
									}
									throw new NotSupportedException("Payload size too big (was gonna be " + sz + ")");
								}
								size = (int)sz;
								index += 8;
								break;

							default:
								size = buf[1];
								break;
						}

						int preMask = index;
						if (masked)
						{
							byte[] maskKey = new byte[4];
							await ns.ReadExactlyAsync(maskKey, 0, 4, token);
							index += 4;
							await ns.ReadExactlyAsync(buf, index, size, token);
							Unmask(buf, maskKey, index, size);
						}
						else
						{
							await ns.ReadExactlyAsync(buf, index, size, token);
						}
						var pkt = new ReceivedPacket()
						{
							InternalBuffer = buf,
							Opcode = op,
							InternalBufferOffset = index,
							PayloadLength = size
						};
						Func<CancellationToken, Task> act = async x =>
						{
							await ns.WriteAsync(buf, 0, preMask, x);
							await ns.WriteAsync(buf, index, size, x);
							FinishClose();
						};
						if (finishClose && pkt.Opcode == Opcode.Close)
						{
							// automatically respond with the same packet, the buffer was already unmasked and masked bit was reset, so we can do this
							await act(Token);
						}
						pkt.ContinueClose = act;
						recv.Release();
						return pkt;
					}
					else
					{
						throw new Desynced();
					}
				}
			}
			catch (Exception ex)
			{
				try
				{
					recv.Release();
				}
				catch { }
				if (ex.InnerException is SocketException)
				{
					FinishClose();
				}
				throw;
			}
			
		}

		public PaketoProcessor Processor = new PaketoProcessor();

		//public const int 

		public void Unmask(byte[] buf, byte[] key, int offset, int length)
		{
			for (int i = 0; i < length; i++)
			{
				buf[i + offset] = (byte)(buf[i + offset] ^ key[i % 4]);
			}
		}

		public class ReceivedPacket
		{
			public Opcode Opcode;
			public bool IsText => Opcode == Opcode.Text;
			public bool IsBinary => Opcode == Opcode.Binary;
			public bool IsClosing => Opcode == Opcode.Close;

			public string Text
			{
				get
				{
					if (textInternal == null)
						textInternal = Encoding.UTF8.GetString(InternalBuffer, InternalBufferOffset, PayloadLength);
					return textInternal;
				}
			}

			string textInternal = null;

			public byte[] InternalBuffer;
			public int InternalBufferOffset = 0;
			public int PayloadLength = 0;

			public Func<CancellationToken, Task> ContinueClose;
		}

		public bool Closed = false;
		public async Task FuckOffGracefullyAsync(ushort reasonCode = 1000, string reason = "fuck off im closing down 🖕")
		{
			if (Closing || Closed)
				return;
			Closing = true;
			byte[] payload = (BitConverter.GetBytes(reasonCode)).Reverse().Concat(Encoding.UTF8.GetBytes(reason)).ToArray();
			using (CancellationTokenSource cts = new CancellationTokenSource())
			{
				cts.CancelAfter(5000);
				await SendAsync(payload, Opcode.Close, cts.Token);
				try
				{
					while (true)
					{
						var p = await ReceiveAsync(false,cts.Token);
						if (p.Opcode == Opcode.Close)
						{
							if (p.InternalBuffer.Skip(p.InternalBufferOffset).Take(p.PayloadLength).SequenceEqual(payload))
							{
								// our packet, close is done
								
							}
							else
							{
								// not ours, but let's close
								await p.ContinueClose(cts.Token);
							}

							FinishClose();
						}
					}
				}
				catch
				{

				}
			}
		}

		void FinishClose()
		{
			try
			{
				cl.Client.Shutdown(SocketShutdown.Both);
			}
			catch
			{

			}
			localCts.Cancel();
			Closed = true;
		}

		const string ProtoConstant = "258EAFA5-E914-47DA-95CA-C5AB0DC85B11";

		class Nah : Exception { }
		class Desynced : Exception { }

		public class SocketClosed : Exception { }
		private IEnumerable<string> GetValues(string val)
		{
			foreach (var v in val.Split(','))
			{
				var v2 = v.Trim();
				if (v2.Length == 0)
					continue;
				yield return v2;
			}
		}

		Dictionary<string, IEnumerable<string>> Headers = new Dictionary<string, IEnumerable<string>>();

		private bool HTTPHighEnough(string str)
		{
			const string start = "HTTP/";
			if (str.StartsWith(start) && float.Parse(str.Substring(start.Length),CultureInfo.InvariantCulture) >= 1.1f)
				return true;
			return false;
		}
	}
}
