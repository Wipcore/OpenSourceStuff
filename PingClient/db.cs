using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;

namespace Wipcore.Core
{
	public class db : IDisposable
	{
		private DbConnection _cn = null;
		private DbProviderFactory _factory = null;

		public bool FillSchema { get; set; }  // Ctor sets this to true
		private int CommandTimeout { get; set; }  // Ctor sets this to 600s (default is 60s)

		public db(string DbProvider, string connstr)
		{
			// Retrieve the installed providers and factories.
			DataTable dtProviders = DbProviderFactories.GetFactoryClasses();

			DataRow[] rows = dtProviders.Select("InvariantName='" + DbProvider + "'");

			_factory = DbProviderFactories.GetFactory(rows[0]);


			_cn = _factory.CreateConnection();
			_cn.ConnectionString = connstr;

			_cn.Open();

			FillSchema = true;
			CommandTimeout = 600;
		}

		void IDisposable.Dispose()
		{
			if (_cn != null)
			{
				_cn.Close();
				_cn.Dispose();
				_cn = null;
			}
		}

		public DataTable ExecuteDataTableSQL(string sql)
		{
			DataTable dt = new DataTable();

			using (DbCommand cmd = _cn.CreateCommand())
			{
				cmd.Connection = _cn;
				cmd.CommandType = CommandType.Text;
				cmd.CommandText = sql;
				cmd.CommandTimeout = CommandTimeout;

				using (DbDataAdapter da = _factory.CreateDataAdapter())
				{
					da.SelectCommand = cmd;

					if (FillSchema)
					{
						da.FillSchema(dt, SchemaType.Source);
					}
					da.Fill(dt);
				}
			}

			return dt;
		}
	}
}
