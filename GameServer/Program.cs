using GameServer.Session;
using PangyaAPI.TCP.PangyaServer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameServer
{
    internal class Program
    {
        public static GameServerTcp.GameServer gs;
        static void Main(string[] args)
        {
            gs = new GameServerTcp.GameServer();//chama a class com servidor imbutido

            for (; ; )
            {
                var comando = Console.ReadLine().Split(new char[] { ' ' }, 2);
                gs.RunCommand(comando);
            }
        }
    }
}
