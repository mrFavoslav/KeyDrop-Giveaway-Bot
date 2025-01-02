using System;
using System.CodeDom;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Dynamic;
using System.Linq;
using System.Net.WebSockets;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

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
		}

		KeyValuePair<CheckBox,TextBox>[] LabelPairs;
		TextBox t1, t2;

		Color defButtClr = Color.White;
		private async void button1_Click(object sender, EventArgs e)
		{
			try
			{
				var json = GetJson();
				json.action = "set_labels";
				string str = json.Serialize();
				HaltUI(); // got json, stop ui
				try
				{
					using (ClientWebSocket ws = new ClientWebSocket())
					using (CancellationTokenSource cts = new CancellationTokenSource())
					{
						var cn = (Action)(() => cts.CancelAfter(connectionTimeout));
						cn();
						await ws.ConnectAsync(connectUri, cts.Token);
						cn();
						await ws.SendAsync(new ArraySegment<byte>(Encoding.ASCII.GetBytes(str)), WebSocketMessageType.Text, true, cts.Token);
						cn();
						await ws.CloseAsync(WebSocketCloseStatus.NormalClosure, "Closing", cts.Token);
					}
					var a = (Action)(() =>
					{
						Clipboard.SetText(str);
						button1.ForeColor = Color.Green;
					});
					if (InvokeRequired)
						Invoke(a);
					else
						a();
					await Task.Delay(1000);
					a = (Action)(() => button1.ForeColor = defButtClr);
					if (InvokeRequired)
						Invoke(a);
					else
						a();
				}
				catch (Exception ex2)
				{
					MessageBox.Show(ex2.ToString(), "Critical BG Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
				}
				if (InvokeRequired)
				{
					Invoke((Action)ResumeUI);
				}
				else
				{
					ResumeUI();
				}
			}
			catch (Exception ex)
			{
				MessageBox.Show(ex.ToString(), "Critical Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
			}
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

		IEnumerable<TextBox> tex(Control.ControlCollection ctl) => from x in ctl.Cast<Control>() where x is TextBox select (TextBox)x;

		private void FuckUpInputs()
		{
			foreach (var ctl in tex(Controls).Concat(tex(groupBox1.Controls)))
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
			var js = JsonSerializer.Deserialize<JsonThingy>("{\"action\":\"get_labels\",\"labels\":[[\"AMATEUR\",1],[\"CONTENDER\",1],[\"CHAMPION\",1],[\"LEGEND\",1],[\"CHALLENGER\",1]],\"wo_captcha_cooldown\":1,\"w_captcha_cooldown\":1}");
			await GetDataAsync();
		}

		Task currentTask = Task.CompletedTask;

		const string uriString = "ws://localhost:54321",
			getDataStr = "{\"action\":\"get_labels\"}";
		byte[] getDataBuf = Encoding.ASCII.GetBytes(getDataStr);
		const int connectionTimeout = 5000;
		Uri connectUri = new Uri(uriString);
		
		private async Task GetDataAsync()
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
							*/
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
		}

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
