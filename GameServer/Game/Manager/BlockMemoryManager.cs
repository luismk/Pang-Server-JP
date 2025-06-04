using PangyaAPI.Utilities.Log;
using System.Collections.Generic;
using System.Threading;
using System;
using System.Linq;
using System.Collections.Concurrent;

namespace Pangya_GameServer.Game.Manager
{
    public class BlockMemoryManager
    {
        public class BlockCtx
        {
            public BlockCtx(uint _ul = 0u)
            {
                cs = new object();
            }
            ~BlockCtx()

            {
                cs = null;
            }
            public object cs;
        }

        public BlockMemoryManager()
        {
        }

        ~BlockMemoryManager()
        {
            clear();
        }

        public static void blockUID(uint _uid)
        {

            var it = mp_block.FirstOrDefault(c => c.Key == _uid);

            if (!mp_block.Any(c => c.Key == _uid))
            { // N�o tem Cria um

                var itt = mp_block.TryAdd(_uid, new BlockCtx(0u));

                if (!itt)
                {
                    message_pool.push(new message("[BlockMemoryManager::blockUID][Error] tentou inserir um block ja existente no map[KEY=" + Convert.ToString(_uid) + "]. Bug", type_msg.CL_FILE_LOG_AND_CONSOLE));
                }

                it = mp_block.FirstOrDefault(c => c.Key == _uid);
            }

            // Enter Critical Section
            Monitor.Enter(it.Value.cs);
        }

        public static void unblockUID(uint _uid)
        {

            var it = mp_block.FirstOrDefault(c=> c.Key == _uid);

            if (it.Key == 0)
            {
                message_pool.push(new message("[BlockMemoryManager::unblockUID][Error] block[UID=" + Convert.ToString(_uid) + "] nao existe no map. Bug", type_msg.CL_FILE_LOG_AND_CONSOLE));

                return;
            }


            Monitor.Exit(it.Value.cs);
        }

        protected static void clear()
        {

            // Acho que desaloca a mem�ria, se n�o tenho que rodar em um loop, para desalocar ela
            if (mp_block.Count != 0)
            {
                mp_block.Clear();
            }
        }

        protected static ConcurrentDictionary<uint, BlockCtx> mp_block = new ConcurrentDictionary<uint, BlockCtx>();
    }
}