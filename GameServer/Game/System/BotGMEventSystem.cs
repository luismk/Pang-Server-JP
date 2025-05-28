using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Pangya_GameServer.Cmd;
using Pangya_GameServer.Game.Utils;
using Pangya_GameServer.GameType;
using PangyaAPI.SQL.Manager;
using PangyaAPI.Utilities;
using PangyaAPI.Utilities.Log;

namespace Pangya_GameServer.Game.System
{
    public class BotGMEvent
    {
        List<stRangeTime> m_rt;      // Times to make room event
        List<stReward> m_rewards;
        bool m_load;
        PangyaTime m_st;                            // Usando para n�o ficar criando direto na fun��o de check

        public BotGMEvent()
        {

            this.m_rt = new List<stRangeTime>();
            this.m_rewards = new List<stReward>();
            this.m_load = false;
            this.m_st = new PangyaTime();
            // Inicializa
            initialize();
        }

        public void clear()
        {

            if (!m_rt.empty())
            {
                m_rt.Clear();
            }

            if (!m_rewards.empty())
            {
                m_rewards.Clear();
            }

            m_load = false;


        }
        public void load()
        {

            if (isLoad())
            {
                clear();
            }

            initialize();
        }

        public bool isLoad()
        {
            return m_load;
        }

        public void initialize()
        {



            CmdBotGMEventInfo cmd_bgei = new CmdBotGMEventInfo(); // Waiter

            NormalManagerDB.add(0,
                cmd_bgei, null, null);

            if (cmd_bgei.getException().getCodeError() != 0)
            {
                throw cmd_bgei.getException();
            }

            cmd_bgei.setTipo(1);

            NormalManagerDB.add(0,
                cmd_bgei, null, null);

            if (cmd_bgei.getException().getCodeError() != 0)
            {
                throw cmd_bgei.getException();
            }

            m_rt = cmd_bgei.getTimeInfo();
            m_rewards = cmd_bgei.getRewardInfo();
             
            // Log 
            message_pool.push(new message("[BotGMEvent::initialize][Log] Carregou " + Convert.ToString(m_rt.Count) + " times.", type_msg.CL_FILE_LOG_AND_CONSOLE));
             
            message_pool.push(new message("[BotGMEvent::initialize][Log] Bot GM Event System carregado com sucesso!", type_msg.CL_FILE_LOG_AND_CONSOLE));

            m_load = true;

        }
        public bool checkTimeToMakeRoom()
        {

            if (!isLoad())
            {

                message_pool.push(new message("[BotGMEvent::checkTimeToMakeRoom][Error] Bot GM Event not have initialized, please call init function first.", type_msg.CL_FILE_LOG_AND_CONSOLE));

                return false;
            }

            bool is_time = false;

            UtilTime.GetLocalTime(out DateTime time);

            m_st.CreateTime(time);

            var it = m_rt.FirstOrDefault(_el =>
            {
                return _el.isBetweenTime(m_st);
            });

            is_time = (it != null);

            return is_time;
        }
        public bool messageSended()
        {

            if (!isLoad())
            {

                message_pool.push(new message("[BotGMEvent::messageSended][Error] Bot GM Event not have initialized, please call init function first.", type_msg.CL_FILE_LOG_AND_CONSOLE));

                return false;
            }

            bool is_sended = false;

            UtilTime.GetLocalTime(out DateTime time);

            m_st.CreateTime(time);

            var it = m_rt.FirstOrDefault(_el =>
            {
                return _el.isBetweenTime(m_st);
            });

            is_sended = (it != null && it.m_sended_message);

            return is_sended;
        }
        public void setSendedMessage()
        {

            if (!isLoad())
            {

                message_pool.push(new message("[BotGMEvent::setSendedMessage][Error] Bot GM Event not have initialized, please call init function first.", type_msg.CL_FILE_LOG_AND_CONSOLE));

                return;
            }

            UtilTime.GetLocalTime(out DateTime time);

            m_st.CreateTime(time);

            m_rt.ForEach(_el =>
             {
                 if (_el.isBetweenTime(m_st))
                 {
                     _el.m_sended_message = true;
                 }
                 else
                 {
                     _el.m_sended_message = false;
                 }
             });

        }
        public stRangeTime getInterval()
        {

            if (!isLoad())
            {

                message_pool.push(new message("[BotGMEvent::getInterval][Error] Bot GM Event not have initialized, please call init function first.", type_msg.CL_FILE_LOG_AND_CONSOLE));

                return null;
            }

            stRangeTime rt = null;

            UtilTime.GetLocalTime(out DateTime time);

            m_st.CreateTime(time);

            var it = m_rt.FirstOrDefault(_el =>
            {
                return _el.isBetweenTime(m_st);
            });

            if (it != null)
            {
                rt = (it);
            }

            return rt;
        }
        public List<stReward> calculeReward()
        {

            List<stReward> v_reward = new List<stReward>();



            // No m ximo 3 pr mios
            uint num_r = (uint)new Random().Next(1, 3);

            Lottery lottery = new Lottery();
            Lottery.LotteryCtx ctx = null;

            foreach (var el in m_rewards)
            {
                lottery.Push(el.rate, el);
            }

            bool remove_to_roleta = num_r < lottery.GetCountItem();

            // Not loop infinite
            num_r = num_r > lottery.GetCountItem() ? (uint)lottery.GetCountItem() : num_r;

            while (num_r > 0)
            {

                if ((ctx = lottery.SpinRoleta(remove_to_roleta)) == null)
                {

                    // Log
                    message_pool.push(new message("[BotGMEvent::calculeReward][Error][WARNING] nao conseguiu sortear um reward na lottery.", type_msg.CL_FILE_LOG_AND_CONSOLE));

                    // Continua
                    continue;
                }

                v_reward.Add((stReward)ctx.Value);

                // decrease num_r(reward)
                num_r--;
            }

            return new List<stReward>(v_reward);
        }
    }

    public class sBotGMEvent : Singleton<BotGMEvent>
    {
    }
}
