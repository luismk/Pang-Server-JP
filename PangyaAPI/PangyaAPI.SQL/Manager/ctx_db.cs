namespace PangyaAPI.SQL
{
    public class ctx_db
    {
		public string engine;
		public string ip;
		public string db_name;
		public string user;
		public string pass;
		public uint port;

        public string CreateStrConnection()
        {               
            switch (engine)
            {
                case "REMOTE":
                    return "Server=" + ip + "," + port + ";DATABASE=" + db_name + ";UID=" + user + ";PWD=" + pass + "; Min Pool Size=27;Max Pool Size=250;";
                default:
                    return "Data Source=" + ip + "," + port + ";DATABASE=" + db_name + ";UID=" + user + ";PWD=" + pass + "; Min Pool Size=27;Max Pool Size=250;";
            }
        }
    }
}