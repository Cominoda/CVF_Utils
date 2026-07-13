using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CVF_Utils
{
	public class SqlLogin
	{
        public string Serveur { get;  set; }
		public string Database { get; set; } 

        public string User { get;  set; }
		public string Password { get;  set; }
		public bool IsLocalDB { get; set; } = false;

        public SqlLogin() { }
		public SqlLogin (string serveur, string database, string user, string password)
		{
			Serveur = serveur;
			Database = database;
			User = user;
			Password = password;
		}
	}
}
