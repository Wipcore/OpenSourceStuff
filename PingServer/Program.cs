using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Remoting;
using System.Runtime.Remoting.Channels;
using System.Runtime.Remoting.Channels.Tcp;
using System.Text;

namespace Wipcore.Core
{
	public class WipClusterPeer : MarshalByRefObject
	{
		public WipClusterPeer()
		{
			Logger.LogLine("WipClusterPeer activated");
		}

		public void Ping()
		{
			Logger.LogLine("Got ping");

			return;
		}
	}

	class Program
	{
		static void Main(string[] args)
		{
			if (args.Length != 2)
			{
				string usage =
@"PingServer 0.003 gamma. Server application for testing remoting ping.

Usage: PingServer <port> <name>

port:     tcp port
name:     guid";

				Logger.LogLine(usage);
				return;
			}

			int port = int.Parse(args[0]);
			string name = args[1];

			TcpServerChannel channel = new TcpServerChannel(port);
			ChannelServices.RegisterChannel(channel);

			RemotingConfiguration.RegisterWellKnownServiceType(typeof(WipClusterPeer), name, WellKnownObjectMode.SingleCall);

			Logger.LogLine("Listening on port " + port + " with name '" + name + "'");

			do
			{
				System.Threading.Thread.Sleep(200);
			}
			while (!Console.KeyAvailable);

			return;
		}
	}

	static public class Logger
	{
		static public void Log(string message)
		{
			Console.Write(message);
			using (StreamWriter sw = new StreamWriter("PingServerLog.txt", true))
			{
				sw.Write(message);
			}
		}

		static public void LogLine(string message)
		{
			Console.WriteLine(message);
			using (StreamWriter sw = new StreamWriter("PingServerLog.txt", true))
			{
				sw.WriteLine(message);
			}
		}
	}
}
