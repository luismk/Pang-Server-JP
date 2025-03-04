﻿using GameServer.Game;
using GameServer.GameType;
using GameServer.Session;
using PangyaAPI.Network.PangyaServer;
using PangyaAPI.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Runtime.Remoting.Channels;
using System.Text;
using System.Threading.Tasks;
using PangyaAPI.Network.Pangya_St;
using GameServer.Cmd;
using PangyaAPI.Network.PangyaPacket;
using GameServer.Game.System;
using PangyaAPI.SQL.Manager;
using GameServer.Game.Utils;

namespace GameServer
{
    public class GameServer
    {
         static void Main(string[] args)
        {                              
            sgs.gs.getInstance().Start();
            for (; ; )
            {
                var comando = Console.ReadLine().Split(new char[] { ' ' }, 2);
                sgs.gs.getInstance().RunCommand(comando);
            }
        }
    }
}
