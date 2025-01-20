using PangyaAPI.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using _smp = PangyaAPI.Utilities.Log;
namespace GameServer.PangType
{
    public class GMInfo
    {
        public GMInfo()
        {
            m_uid = 0;

            visible = 0;   /*Deixa o GM invis�vel, depois ele fica vis�vel se ele quiser com o comando*/
            whisper = 1;
            channel = 0;
            map_open = new SortedDictionary<uint, bool>();
        }
        protected void clear()
        {
            m_uid = 0;

            visible = 0;   /*Deixa o GM invis�vel, depois ele fica vis�vel se ele quiser com o comando*/
            whisper = 1;
            channel = 0;

            map_open.Clear();
        }

        public void openPlayerWhisper(uint _uid)
        {
            if (_uid == 0)
                throw new exception("[GMInfo::openPlayerWhisper][Error] GM[UID=" + m_uid + "] tentou adicionar player[UID="
                        +_uid + "] a lista de whisper, mas o _uid eh invalido. Hacker ou Bug.");


            var it = map_open.Where(c => c.Key == _uid);

            if (it.Any())
                map_open[_uid] = true;
            else
                _smp.message_pool.push("[GMInfo::openPlayerWhisper][WARNING] GM[UID=" + (m_uid) + "] tentou add player[UID="
                        + (_uid) + "] a lista de whisper abertos, mas ele ja esta na lista");

        }
        public void closePlayerWhisper(uint _uid)
        {
            if (_uid == 0)
                throw new exception("[GMInfo::openPlayerWhisper][Error] GM[UID=" + (m_uid) + "] tentou excluir player[UID="
            + (_uid) + "] da lista de whisper, mas o _uid eh invalido. Hacker ou Bug.");


            var it = map_open.Where(c => c.Key == _uid);

            if (it.Any())
                map_open.Remove(_uid);
            else
                _smp.message_pool.push("[[GMInfo::openPlayerWhisper][WARNING] GM[UID=" + (m_uid) + "] tentou excluir player[UID="
                + (_uid) + "] da lista de whisper, mas ele nao esta na lista.");

        }

        public bool isOpenPlayerWhisper(uint _uid)
        {
            bool ret = false;

            foreach (var el in map_open)
            {
                if (el.Key == _uid)
                {
                    ret = true;
                    break;
                }
            }
            return ret;
        }

        public void setGMUID(uint _uid)
        {
            if (_uid == 0)
                throw new exception("[GMInfo::setGMUID][Error] GM[UID=" + (m_uid) + "] tentou setar o UID do GM para UID[value="
                        + (_uid) + "], mas o uid eh invalido. Hacker ou Bug.");

            m_uid = _uid;
        }

        public byte visible; // 0 ou 1, Visible
        public byte whisper; // 0 ou 1, Whisper Geral
        public byte channel; // 0 ou 1, Whisper do Canal

        uint m_uid;

        SortedDictionary<uint, bool> map_open;  // UID dos player que o GM deixou o whisper aberto para ver os chat deles
    }
}
