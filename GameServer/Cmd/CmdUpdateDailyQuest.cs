using System;
using Pangya_GameServer.GameType;
using PangyaAPI.SQL;
namespace Pangya_GameServer.Cmd
{
    public class CmdUpdateDailyQuest : Pangya_DB
    {
        public CmdUpdateDailyQuest(DailyQuestInfo _dqi)
        {
            this.m_dqi = (_dqi);
            this.m_updated = false;
        }

        public virtual void Dispose()
        {
        }

        public DailyQuestInfo getInfo()
        {
            // C++ TO C# CONVERTER TASK: The following line was determined to contain a copy constructor call - this should be verified and a copy constructor should be created:
            return m_dqi;
        }

        public void setInfo(DailyQuestInfo _dqi)
        {
            // C++ TO C# CONVERTER TASK: The following line was determined to be a copy assignment (rather than a reference assignment) - this should be verified and a 'CopyFrom' method should be created:
            m_dqi = _dqi;
        }

        public bool isUpdated()
        {
            return m_updated;
        }

        protected override void lineResult(ctx_res _result, uint _index_result)
        {

            checkColumnNumber(5, (uint)_result.cols);

            // Update ON DB
            m_updated = IFNULL<bool>(_result.data[0]);

            if (!m_updated)
            { // Não atualizou, pega os valores atualizados do banco de dados

                for (var i = 0u; i < 3u; ++i)
                {
                    // C++ TO C# CONVERTER TASK: The following line was determined to be a copy assignment (rather than a reference assignment) - this should be verified and a 'CopyFrom' method should be created:
                    m_dqi._typeid[i] = IFNULL<uint>(_result.data[1u + i]);
                    m_dqi._typeid[i] = IFNULL(_result.data[1u + i]); // 1 + 3
                }

                if (_result.data[4] != null)
                {
                    m_dqi.date.CreateTime(_translateDate(_result.data[4]));
                }
            }

            return;
        }

        protected override Response prepareConsulta()
        {

            m_updated = false;

            string reg_date = "null";

            if (!m_dqi.date.IsEmpty)
            {
                reg_date = _db.makeText(_formatDate(m_dqi.date.ConvertTime()));
            }

            var r = procedure(
                m_szConsulta,
                Convert.ToString(m_dqi._typeid[0]) + ", " + Convert.ToString(m_dqi._typeid[1]) + ", " + Convert.ToString(m_dqi._typeid[2]) + ", " + reg_date);

            checkResponse(r, "nao conseguiu atualizar o sistema de Daily Quest[" + m_dqi.ToString() + "] no banco de dados");

            return r;
        }
        private DailyQuestInfo m_dqi = new DailyQuestInfo();
        private bool m_updated; // true atualizou no DB, false, outro j� atualizou pega o valor do DB

        private const string m_szConsulta = "pangya.ProcUpdateDailyQuest";
    }
}
