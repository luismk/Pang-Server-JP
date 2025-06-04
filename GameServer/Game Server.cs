using System;
using Pangya_GameServer.GameType;
using System.Runtime.InteropServices;
using PangyaAPI.Utilities.Log;
using PangyaAPI.Utilities;
using System.Diagnostics;
using PangyaAPI.Network.Pangya_St;
using System.Xml.Linq;
using PangyaAPI.Utilities.BinaryModels;
using System.Collections.Generic;
using System.Security.Policy;
using System.Threading;
using Pangya_GameServer.UTIL;
namespace Pangya_GameServer
{
    public class GameServer
    {
        static void Main()
        {
            try
            { 
                sgs.gs.getInstance().Start();
                for (; ; )
                {
                    var input = Console.ReadLine();
                    var comando = new Queue<string>(input.Split(' '));
                    sgs.gs.getInstance().CheckCommand(comando);
                }
            }
            catch (Exception e)
            { 
                throw e;
            }
        }
    }
}
