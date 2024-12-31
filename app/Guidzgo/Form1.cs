using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Net.WebSockets;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.Json.Serialization;
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
		}

		KeyValuePair<CheckBox,TextBox>[] LabelPairs;
		TextBox t1, t2;

		private void button1_Click(object sender, EventArgs e)
		{
			try
			{
				string str = GetJson().Serialize();
				HaltUI(); // got json, stop ui
				Task.Run(() =>
				{
					try
					{
						Invoke(new Action(() => Clipboard.SetText(str)));
						MessageBox.Show("Set");
					}
					catch
					{

					}
					Invoke(new Action(() =>
					{
						ResumeUI();
					}));
				});
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
				t1 = ParseBox(t1),
				t2 = ParseBox(t2),
				l = (from x in LabelPairs where x.Key.Checked select new object[] { x.Key.Text, ParseBox(x.Value) }).ToArray()
			};
			return j;
		}

		char[] NumChars = (from x in Enumerable.Range(0, 10) select x.ToString()[0]).ToArray();
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
		public object l { get; set; } // format: "l":[["Label name",5000],["Another label",2000]] (aka array of len=2 arrays that contain string at index0 and whole nubmer at index1
		public long t1 { get; set; }
		public long t2 { get; set; }

		public string Serialize() => System.Text.Json.JsonSerializer.Serialize(this, typeof(JsonThingy));
	}
}
