using System;
using System.Collections.Generic;
using System.Data;
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
			Console.WriteLine("WipClusterPeer activated");
		}

		public void Ping()
		{
			// This is only a placeholder for the ping function
			// which is executing on other computers.
			return;
		}
	}

	public class Server
	{
		public string Address { get; set; }
		public int Port { get; set; }
		public string Name { get; set; }
		public string FriendlyName { get; set; }
	}

	class Program
	{
		static void Main(string[] args)
		{
			if (!(args.Length == 2 || (args.Length >= 3 && args[0] == "-servers")))
			{
				string usage =
@"PingClient 0.003 gamma. Client application for testing remoting ping.

Usage1: PingClient <connstr> <delay>
Usage2: PingClient -servers <servers> <delay>

connstr:  connection string. Don't specify any provider.
servers:  list of servers, on the format: server:port/guid server:port/guid server:port/guid
delay:    repeat delay in seconds";

				LogLine(usage);
				return;
			}

			string[] servers = null;
			string connstr = null;

			int delay = int.Parse(args.Last());

			if (args[0] == "-servers")
			{
				servers = args.Skip(1).Take(args.Length - 2).ToArray();
			}
			else
			{
				connstr = args[0];
			}

			TcpChannel chan = new TcpChannel();
			ChannelServices.RegisterChannel(chan);

			do
			{
				try
				{
					Server[] servers2 = GetServers(connstr, servers);

					foreach (var server in servers2)
					{
						LogLine("Server: '" + server.Address + "'");
					}

					PingServers(servers2);
				}
				catch (System.Exception ex)
				{
					LogLine("Unknown error: " + ex.ToString());
				}
				LogLine("Sleeping a while...");
				LogLine("");
				System.Threading.Thread.Sleep(delay * 1000);
			}
			while (!Console.KeyAvailable);

			return;
		}

		private static Server[] GetServers(string connstr, string[] servers)
		{
			DateTime t1, t2;
			DataTable dt;

			if (connstr != null)
			{
				t1 = DateTime.UtcNow;
				try
				{
					using (db enova = new db("System.Data.SqlClient", connstr))
					{
						string sql = "select * from WipClusterPeer where Deleted=0";
						dt = enova.ExecuteDataTableSQL(sql);
					}
				}
				catch (System.Exception ex)
				{
					t2 = DateTime.UtcNow;
					LogLine("Couldn't connect to database: " + (t2 - t1));
					LogLine(ex.ToString());
					return null;
				}
				t2 = DateTime.UtcNow;

				LogLine("" + DateTime.UtcNow + ": Found " + dt.Rows.Count + " nodes: " + (t2 - t1));

				return dt.AsEnumerable().Select(
					dr => new Server
					{
						Address = (string)dr["Address"],
						Port = (int)dr["Port"],
						Name = (string)dr["Name"],
						FriendlyName = (string)dr["FriendlyName"]
					}).ToArray();
			}
			else
			{
				char[] separators = ":/".ToCharArray();

				return servers.Select(
					s => new Server
					{
						Address = s.Split(separators)[0],
						Port = int.Parse(s.Split(separators)[1]),
						Name = s.Split(separators)[2],
						FriendlyName = "."
					}).ToArray();
			}
		}

		static void PingServers(Server[] servers)
		{
			DateTime t1, t2;

			foreach (Server server in servers)
			{
				string address = server.Address;
				int port = server.Port;
				string name = server.Name;
				string friendlyname = server.FriendlyName;


				string url = "tcp://" + address + ":" + port + "/" + name;
				Log(friendlyname + ": '" + url + "': ");
				WipClusterPeer wip = (WipClusterPeer)Activator.GetObject(typeof(WipClusterPeer), url);
				if (wip == null)
				{
					LogLine("Couldn't connect to server.");
				}
				else
				{
					t1 = DateTime.UtcNow;
					try
					{
						wip.Ping();
					}
					catch (System.Exception ex)
					{
						t2 = DateTime.UtcNow;
						LogLine("Ping NOT ok: " + (t2 - t1));
						LogLine(ex.ToString());
						continue;
					}
					t2 = DateTime.UtcNow;
					LogLine("Ping ok: " + (t2 - t1));
				}
			}
		}

		static void Log(string message)
		{
			Console.Write(message);
			using (StreamWriter sw = new StreamWriter("PingClientLog.txt", true))
			{
				sw.Write(message);
			}
		}
		static void LogLine(string message)
		{
			Console.WriteLine(message);
			using (StreamWriter sw = new StreamWriter("PingClientLog.txt", true))
			{
				sw.WriteLine(message);
			}
		}
	}
}
