using GameServer.PangType;
using PangyaAPI.SQL;
using System;
using System.Data;

namespace GameServer.Cmd
{
    public class CmdGuildInfo : Pangya_DB
    {
        private uint m_uid;
        private uint m_option;
        GuildInfoEx m_gi = new GuildInfoEx();
        protected override string _getName { get; } = "CmdGuildInfo";
        public CmdGuildInfo(uint uid, uint _option)
        {
            this.m_uid = uid;
            this.m_option = _option;
        }      

        protected override void lineResult(ctx_res _result, uint _index_result)
        {
            checkColumnNumber(13);
            try
            {
				m_gi.uid = _result.GetUInt32(0);

				if (_result.data[1] != null)
					m_gi.name = _result.data[1].ToString();


				m_gi.total_member = _result.GetUInt32(2);


				if (_result.data[3] != null)
				{
					m_gi.Image = (_result.data[3]).ToString().Replace(".png","");
				}
				else
				{
                    m_gi.Image = "guild_mark";
                }

				if ((_result.data[5]) !=null)
				{
                    m_gi.Notice = (_result.data[5]).ToString();
				}

				if ((_result.data[6]) != null)
                {
                    m_gi.Introducting = (_result.data[6]).ToString();
				}
				m_gi.point = _result.GetUInt32(7);
				m_gi.pang = _result.GetUInt32(8);
				m_gi.Position = _result.GetUInt32(9);
				m_gi.LeaderUID = _result.GetUInt32(10);
				if (_result.data[11] != null)
				{
                    m_gi.LeaderNickname = (_result.data[11]).ToString();
				}
			}
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);

            }
        }

        protected override Response prepareConsulta()
        {
            var r = procedure("pangya.ProcGetGuildInfo " + m_uid.ToString() + ", " + m_option.ToString());
            checkResponse(r, "nao conseguiu pegar o guild info do player: " + (m_uid));
            return r;
        }


        public GuildInfoEx getInfo()
        {
            return m_gi;
        }
    }
}