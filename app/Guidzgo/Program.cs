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
			Application.EnableVisualStyles();
			Application.SetCompatibleTextRenderingDefault(false);
			Application.Run(Get());
		}

		static Form1 Get() // the shit gets loaded before it is required for some reason, this is just for isolation
		{
			return Form1.Instance;
		}
	}
}
