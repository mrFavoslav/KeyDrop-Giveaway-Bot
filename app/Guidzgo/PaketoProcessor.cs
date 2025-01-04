using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using static Guidzgo.FogginClient;

namespace Guidzgo
{
	public class PaketoProcessor
	{
		public CancellationToken Token = default;
		public SemaphoreSlim brmbrm = new SemaphoreSlim(1);
		public async Task Loop(FogginClient c)
		{
			bool acq = false;
			void exit()
			{
				if (acq)
					brmbrm.Release();
				acq = false;
			}
			async Task enter()
			{
				if (acq)
					return;
				await brmbrm.WaitAsync().ConfigureAwait(false);
				acq = true;
			}
			try
			{
				while (!c.Closed && !c.Closing)
				{
					await enter();
					try
					{
						var resp = await c.ReceiveAsync();
						if (resp.IsText || resp.IsBinary)
						{
							if (Packets.Count < maxQueueSize)
							{
								Packets.Enqueue(resp);
							}
						}
					}
					catch (Exception e)
					{
						
						if (c.serv.cts.Token.IsCancellationRequested || e is SocketClosed || e is EndOfStreamException)
						{
							exit();
							throw;
						}
					}
					exit();
				}

			}
			catch
			{
				
			}
			Running = false;
			exit();
		}

		int maxQueueSize = 5;

		ConcurrentQueue<FogginClient.ReceivedPacket> Packets = new ConcurrentQueue<FogginClient.ReceivedPacket>();

		public void Start(FogginClient c)
		{
			if (Running)
				return;
			Running = true;
			Task.Run(() => Loop(c));
		}

		public async Task<FogginClient.ReceivedPacket> Receive(int timeout = -1)
		{
			while (Running)
			{
				await brmbrm.WaitAsync(timeout);
				var r = Packets.TryDequeue(out var packet);
				brmbrm.Release();
				if (r)
					return packet;
			}
			throw new Exception("packet processor not running");
		}

		public bool Running = false;
	}
}
