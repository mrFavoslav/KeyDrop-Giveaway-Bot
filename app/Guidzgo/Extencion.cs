using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Guidzgo
{
	public static class Extencion
	{
		public static void TryDispose(this IDisposable d)
		{
			try
			{
				d.Dispose();
			}
			catch { }
		}

		public static async Task ReadExactlyAsync(this Stream s, byte[] buf, int index, int lenght, CancellationToken token = default)
		{
			int r = 0;
			while (r < lenght)
			{
				var r2 = await s.ReadAsync(buf, index + r, lenght - r, token);
				if (r2 == 0)
					throw new EndOfStreamException();
				r += r2;
			} 
		}

		public static async Task AcceptTcpClientAsync(this TcpListener tcp, CancellationToken token)
		{
			void callback(IAsyncResult r)
			{

			}
			var res = tcp.BeginAcceptTcpClient(callback, null);
		}
	}
}
