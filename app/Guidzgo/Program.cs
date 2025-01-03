using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Guidzgo
{
	internal static class Program
	{
		/// <summary>
		/// Hlavní vstupní bod aplikace.
		/// </summary>
		[STAThread]
		static void Main()
		{
			try
			{
				Application.EnableVisualStyles();
				Application.SetCompatibleTextRenderingDefault(false);
				Application.Run(Form1.Instance);
			}
			catch (Exception ex)
			{
				Clipboard.SetText(ex.ToString());
				MessageBox.Show(ex.ToString());
			}
			
		}
	}
}
