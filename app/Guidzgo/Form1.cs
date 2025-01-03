using System;
using System.CodeDom;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Dynamic;
using System.Linq;
using System.Net;
using System.Net.WebSockets;
using System.Runtime.CompilerServices;
using System.Runtime.Remoting.Messaging;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using static Guidzgo.FogginClient;

namespace Guidzgo
{
	public partial class Form1 : Form
	{
		public Form1()
		{
			InitializeComponent();

			LabelPairs = (from x in groupBox1.Controls.Cast<Control>()
						  where x is CheckBox
						  let c = (CheckBox)x
						  select new KeyValuePair<CheckBox, TextBox>(c,
				   (TextBox)groupBox1.Controls[c.Text.Substring(0, c.Name.Length - 1) + "T"])).ToArray();
			t1 = textBox6;
			t2 = textBox7;
			defButtClr = button1.ForeColor;
			logBoxSize = logBox.Height + 12;
			LogsEnabled = false;
			connLabel.Text = "";
			logBox.MaxLength = 1024 * 8;
		}

		KeyValuePair<CheckBox,TextBox>[] LabelPairs;
		TextBox t1, t2;

		Color defButtClr = Color.White;
		private async void button1_Click(object sender, EventArgs e)
		{
			try
			{
				if (clientCount == 0)
				{
					MessageBox.Show("There are no clients connected", "guh", MessageBoxButtons.OK, MessageBoxIcon.Warning);
					return;
				}

				var json = GetJson();
				json.action = "update_labels";
				string str = json.Serialize();
				HaltUI(); // got json, stop ui
				try
				{
					using (CancellationTokenSource cts = new CancellationTokenSource())
					{
						var d = Task.Delay(5000);
						var r = await Task.WhenAny(
							Task.WhenAll(from x in Server.Clients where x != null select x.SendAsync(str, token: cts.Token)),
							d);
						if (r == d)
						{
							// timed out
							Log("A timeout was reached whils trying to send data to clients");
						}
						else
						{
							Log("Sucessfully sent data to clients");
						}
					}
					
				}
				catch (Exception ex2)
				{
					await Lock(() => MessageBox.Show(ex2.ToString(), "Critical BG Error", MessageBoxButtons.OK, MessageBoxIcon.Error));
				}
				await Lock(ResumeUI);
			}
			catch (Exception ex)
			{
				MessageBox.Show(ex.ToString(), "Critical Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
			}
		}

		int logBoxSize;

		private bool LogsEnabled
		{
			get => logBox.Enabled;

			set
			{
				if (value)
				{
					if (!LogsEnabled)
					{
						logBox.Enabled = true;
						Height += logBoxSize;
					}
				}
				else
				{
					if (LogsEnabled)
					{
						logBox.Enabled = false;
						Height -= logBoxSize;
					}
				}
			}
		}

		private void UpdateCount()
		{
			Action a = () =>
			{
				connLabel.Text = "conn: " + clientCount;
			};
			if (InvokeRequired)
				Invoke(a);
			else
				a();
		}

		bool finalClose = false;
		bool finalClosing = false;
		private void Form1_FormClosing(object sender, FormClosingEventArgs e)
		{
			if (finalClose)
				return;
			e.Cancel = true;
			if (finalClosing)
				return;
			finalClosing = true;
			Enabled = false;
			Server.StopAsync().ContinueWith(x =>
			{
				finalClose = true;
				if (InvokeRequired)
					Invoke((Action)Close);
				else
					Close();
			});
		}

		private string GetTime(long v)
		{
			const long m_s = 1000,
					m_m = m_s * 60,
					m_h = m_m * 60;
			long h = v / m_h;
			v -= h * m_h;
			long m = v / m_m;
			v -= m * m_m;
			long s = v / m_s;
			v -= s * m_s;
			StringBuilder sb = new StringBuilder();
			if (h > 0)
				sb.Append($"{h}h ");
			if (m > 0)
				sb.Append($"{m}m ");
			if (s > 0)
				sb.Append($"{s}s ");
			if (v > 0)
				sb.Append($"{v} ");
			if (sb.Length > 0)
			{
				sb.Remove(sb.Length - 1, 1);
				return sb.ToString();
			}
			else
				return "0";
		}

		private void HaltUI()
		{
			Enabled = false;
		}

		private void ResumeUI()
		{
			Enabled = true;
		}

		public static Form1 Instance = new Form1();

		public static void Log(object o)
		{
			Instance.Lock(() => Instance.LogInternal(o)).Wait();
		}

		public static async Task LogAsync(object o)
		{
			await Instance.Lock(() => Instance.LogInternal(o));
		}

		public void LogInternal(object o)
		{
			string str = (o as string) ?? o.ToString();
			str.Replace("\r", string.Empty);
			var split = str.Split('\n');
			for (int i = 0; i < split.Length; i++)
			{
				logBox.AppendText(split[i] + Environment.NewLine);
			}
		}

		IEnumerable<TextBox> tex(Control.ControlCollection ctl) => from x in ctl.Cast<Control>() where x is TextBox select (TextBox)x;

		private void FuckUpInputs()
		{
			foreach (var ctl in tex(Controls).Except(new TextBox[] { logBox }).Concat(tex(groupBox1.Controls)))
			{
				ctl.Text = ctl.Text.Trim();
				ctl.Text = GetTime(ParseBox(ctl));
			}
		}

		private JsonThingy GetJson()
		{
			FuckUpInputs();
			var j = new JsonThingy()
			{
				wo_captcha_cooldown = ParseBox(t1),
				w_captcha_cooldown = ParseBox(t2),
				labels = (from x in LabelPairs where x.Key.Checked select new object[] { x.Key.Text, ParseBox(x.Value) }).ToArray()
			};
			return j;
		}

		char[] NumChars = (from x in Enumerable.Range(0, 10) select x.ToString()[0]).ToArray();

		private async void Form1_Load(object sender, EventArgs e)
		{
			Server.ClientJoined += (FogginClient c) =>
			{
				Interlocked.Increment(ref clientCount);
				UpdateCount();
				return Task.CompletedTask;

			};
			Server.ClientLeft += (FogginClient c) =>
			{
				Interlocked.Decrement(ref clientCount);
				UpdateCount();
				return Task.CompletedTask;
			};

			Server.ClientJoined += FirstJoin;

			await Server.StartAsync();

			foreach (var elm in toDisable)
				elm.Enabled = false;
		}

		public SemaphoreSlim uiLock = new SemaphoreSlim(1);
		public async Task Lock(Action a)
		{
			
			try
			{
				if (InvokeRequired)
				{
					await uiLock.WaitAsync();
					try
					{
						Invoke(a);
					}
					catch
					{

					}
					uiLock.Release();
				}
				else
				{
					a();
				}
			}
			catch
			{

			}
			
			
		}

		private IEnumerable<Control> toDisable => new Control[] {
		groupBox1, textBox6, label1, textBox7, label2, button1
		};

		public class NoCanDo : Exception { }

		public async Task LoadJson(string str)
		{
			var js = JsonSerializer.Deserialize<JsonThingy>(str);
			
			if (js != null)
			{
				if (js.labels is JsonElement je)
				{
					List<(string entry, long val)> changes = new List<(string entry, long val)>();
					using (var en1 = je.EnumerateArray())
					{
						while (en1.MoveNext())
						{
							using (var en2 = en1.Current.EnumerateArray())
							{
								if (en2.MoveNext())
								{
									string name = en2.Current.GetString();
									if (en2.MoveNext())
									{
										long time = en2.Current.GetInt64();
										if (name != null)
										{
											changes.Add((name, time));
											continue;
										}
									}
								}
							}
							throw new NoCanDo(); // failed parsing, throw
						}
					}
					await Lock(() =>
					{
						foreach (var e in LabelPairs)
						{
							e.Key.Checked = false;
							e.Value.Text = "30s";
						}
						foreach (var change in changes)
						{
							try
							{
								var l = LabelPairs.First(x => x.Key.Text == change.entry);
								l.Key.Checked = true;
								l.Value.Text = GetTime(change.val);
							}
							catch
							{

							}
						}
						t1.Text = GetTime(js.wo_captcha_cooldown);
						t2.Text = GetTime(js.w_captcha_cooldown);
						foreach (var elm in toDisable)
							elm.Enabled = true;
					});
				}
			}
			else
				throw new NoCanDo();
		}

		private async Task FirstJoin(FogginClient c)
		{
			try
			{
				Log("Attempting to get labels off connected client...");
				using (CancellationTokenSource cts= new CancellationTokenSource())
				{
					cts.CancelAfter(5000);
					await c.SendAsync(GetDataMessage, FogginClient.Opcode.Text, cts.Token);
					Log("Request sent");
					cts.CancelAfter(-1);
					var msg = await c.Processor.Receive(5000);
					Log("Response received");
					if (msg.IsText)
					{
						await LoadJson(msg.Text);
						Log("Sucessfully loaded json");
					}
					else
						return;
				}
				
			}
			catch (TaskCanceledException)
			{
				Log("Timed out");
				return;
			}
			catch (NoCanDo)
			{
				Log("Cannot parse input json");
				return;
			}
			catch (Exception e)
			{
				Log(e);
				return;
			}
			Server.ClientJoined -= FirstJoin;
		}

		int clientCount = 0;

		FogginServer Server = new FogginServer(IPAddress.Loopback, serverPort);

		const int serverPort = 54321;

		const string getDataStr = "{\"action\":\"get_labels\"}";
		static byte[] GetDataMessage = Encoding.ASCII.GetBytes(getDataStr);
		const int connectionTimeout = 5000;

		private void pictureBox1_Click(object sender, EventArgs e)
		{
			LogsEnabled = !LogsEnabled;
		}

		/*private async Task GetDataAsync()
		{
			
			HaltUI();
			using (ClientWebSocket ws = new ClientWebSocket())
			using (CancellationTokenSource cts = new CancellationTokenSource())
			{
				try
				{
					var tk = cts.Token;
					var cn = (Action)(() => cts.CancelAfter(connectionTimeout));
					cn();
					await ws.ConnectAsync(connectUri, tk);
					cn();
					await ws.SendAsync(new ArraySegment<byte>(getDataBuf), WebSocketMessageType.Text, true, tk);
					byte[] buf = new byte[1024 * 4]; // 4KB buffer is more than enough
					var seg = new ArraySegment<byte>(buf);
					cn();

					while (true)
					{
						var res = await ws.ReceiveAsync(seg, tk);
						if (res.MessageType == WebSocketMessageType.Text)
						{

							string tex = Encoding.UTF8.GetString(buf, 0, res.Count);
							#region tempCode
							/*
							var js = JsonSerializer.Deserialize<JsonElement>(tex);
							if (js.TryGetProperty("action",out var action) && action.ValueKind == JsonValueKind.String)
							{
								string str = action.GetString();
								if (str == "set_labels")
								{
									if (js.TryGetProperty("labels",out var labels))
									{
										if (labels.ValueKind == JsonValueKind.Array)
										{
											cts.CancelAfter(-1); // abort cancellation
											List<(string, long)> ls = new List<(string, long)>();
											using (var en = labels.EnumerateArray())
											{
												foreach (var v in en)
												{
													if (v.ValueKind == JsonValueKind.Array)
													{
														using (var en2 = v.EnumerateArray())
														{
															en2.MoveNext();
															if (en2.Current.ValueKind == JsonValueKind.String)
															{
																string s = en2.Current.GetString();
																en2.MoveNext();
																if (en2.Current.ValueKind == JsonValueKind.Number)
																{
																	long n = en2.Current.GetInt64();
																	if (n >= 0)
																	{
																		ls.Add((s, n));
																	}
																}
															}
														}
													}
												}
											}

											// process the received input
											
										}
									}
								}
							}
							*//*
							#endregion tempCode
							var js = JsonSerializer.Deserialize<JsonThingy>(tex);
							if (js.action == "get_labels")
							{
								var a = (Action)(() =>
								{
									foreach (var chk in LabelPairs)
									{
										using (var en = ((JsonElement)js.labels).EnumerateArray())
										{
											foreach (var e in en)
											{
												using (var en2 = e.EnumerateArray())
												{
													en2.MoveNext();
													if (en2.Current.GetString() == chk.Key.Text && en2.MoveNext())
													{
														chk.Value.Text = GetTime(en2.Current.GetInt64());
														chk.Key.Checked = true;
														continue;
													}
												}
											}
										}
										chk.Key.Checked = false;
									}
									textBox6.Text = GetTime(js.wo_captcha_cooldown);
									textBox7.Text = GetTime(js.w_captcha_cooldown);
								});
								if (InvokeRequired)
									Invoke(a);
								else
									a();
								break;
							}
						}
					}
					cn();
					await ws.CloseAsync(WebSocketCloseStatus.NormalClosure, "Closing", tk);
				}
				catch (TaskCanceledException)
				{
					Action err = () => MessageBox.Show("Could not fetch current data from " + uriString + " in time", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
					if (InvokeRequired)
						Invoke(err);
					else
						err();
				}
				catch (Exception ex)
				{
					Action err = () => MessageBox.Show(ex.ToString(), "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
					if (InvokeRequired)
						Invoke(err);
					else
						err();
				}
				if (InvokeRequired)
					Invoke((Action)ResumeUI);
				else
					ResumeUI();
			}
		}*/

		private long ParseBox(TextBox box)
		{
			try
			{
				long v = 0;
				StringBuilder current = new StringBuilder();
				const long m_s = 1000,
					m_m = m_s * 60,
					m_h = m_m * 60;
				bool wasSpace = false;
				foreach (char c in box.Text)
				{
					if (NumChars.Contains(c))
					{
						if (wasSpace) // there was a space, treat this as a separate ms number
						{
							if (current.Length > 0)
							{
								v += long.Parse(current.ToString());
								current.Clear();
							}
							wasSpace = false;
						}
						current.Append(c);
					}
					else
					{
						switch (c)
						{
							case ' ': // skip
								wasSpace = true;
								goto yesSpace;

							case 'h':
								v += long.Parse(current.ToString()) * m_h;
								current.Clear();
								break;

							case 'm':
								v += long.Parse(current.ToString()) * m_m;
								current.Clear();
								break;

							case 's':
								v += long.Parse(current.ToString()) * m_s;
								current.Clear();
								break;

							default:
								throw new Exception("Unknown character: " + c);
						}
						wasSpace = false;
					yesSpace: { }
					}
				}
				if (current.Length > 0)
					v += long.Parse(current.ToString());

				return v;
			}
			catch (Exception ex)
			{
				MessageBox.Show("Cannot parse " + box.Text + " from " + box.Name + ":" + Environment.NewLine + ex, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
				throw;
			}
		}
	}

	public class JsonThingy
	{
		public string action { get; set; } // BRUH WHYYY
		public object labels { get; set; }
		// old format: "l":[["Label name",5000],["Another label",2000]] (aka array of len=2 arrays that contain string at index0 and whole nubmer at index1
		public long wo_captcha_cooldown { get; set; }
		public long w_captcha_cooldown { get; set; }

		public string Serialize() => JsonSerializer.Serialize(this, typeof(JsonThingy));
	}
}
