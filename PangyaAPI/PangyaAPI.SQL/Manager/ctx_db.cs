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

        // Método para criar a string de conexão
        public string CreateStrConnection()
        {
            // Verifica o tipo de engine (por exemplo, REMOTE ou LOCAL)
            switch (engine)
            {
                case "REMOTE":
                    // Para conexões REMOTAS, adiciona a configuração para SQL Server e Unicode
                    return $"Server={ip},{port};DATABASE={db_name};UID={user};PWD={pass};" +
                           "Min Pool Size=27;Max Pool Size=250;TrustServerCertificate=True;" +
                           "MultipleActiveResultSets=True;Integrated Security=False;";

                default:
                    // Para outros casos, usa o padrão SQL Server e Unicode
                    return $"Data Source={ip},{port};DATABASE={db_name};UID={user};PWD={pass};" +
                           "Min Pool Size=27;Max Pool Size=250;TrustServerCertificate=True;" +
                           "MultipleActiveResultSets=True;Integrated Security=False;";//Charset=Shift-JIS;
            }
        }
    }
}
