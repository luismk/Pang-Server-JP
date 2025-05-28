using System;
using System.Collections.Generic;
using System.Linq;
using Pangya_GameServer.GameType;
using PangyaAPI.SQL;
using PangyaAPI.Utilities;
using PangyaAPI.Utilities.Log;
using _smp = PangyaAPI.Utilities.Log;
namespace Pangya_GameServer.Cmd
{
    public class CmdMailBoxInfo2 : Pangya_DB
    {
        public CmdMailBoxInfo2(uint _uid)
        {
            this.m_uid = _uid;
        }

        public CmdMailBoxInfo2()
        {
            this.m_uid = 0u;
        }


        public uint GetUID()
        {
            return m_uid;
        }

        public void SetUID(uint _uid)
        {
            this.m_uid = _uid;
        }

        public Dictionary<uint, EmailInfoEx> getInfo()
        {
            return new Dictionary<uint, EmailInfoEx>(m_emails);
        }

        protected override void lineResult(ctx_res _result, uint _index_result)
        {
            checkColumnNumber(17);

            try
            {
                uint id = IFNULL(_result.data[0]);

                var it_email = m_emails.Where(c => c.Key == id).FirstOrDefault();

                if (it_email.Key != 0) // Verifica se o email já existe
                {
                    // Já tem, adiciona apenas o item
                    EmailInfo.item item = new EmailInfo.item
                    {
                        _typeid = IFNULL(_result.data[7])
                    };

                    if (item._typeid != 0)
                    {
                        item.id = IFNULL(_result.data[6]); // ID vem antes de typeid na consulta
                        item.flag_time = (byte)IFNULL(_result.data[8]);
                        item.qntd = IFNULL(_result.data[9]);
                        item.tempo_qntd = IFNULL(_result.data[10]);
                        item.pang = IFNULL(_result.data[11]);
                        item.cookie = IFNULL(_result.data[12]);
                        item.gm_id = IFNULL(_result.data[13]);
                        item.flag_gift = IFNULL(_result.data[14]);

                        if (is_valid_c_string(_result.data[15]))
                        {
                            STRCPY_TO_MEMORY_FIXED_SIZE(ref item.ucc_img_mark, sizeof(char), _result.data[15]);
                        }

                        item.type = (short)IFNULL(_result.data[16]);

                        // Adiciona o item ao email existente
                        it_email.Value.itens.Add(item);
                    }
                }
                else
                {
                    // Não tem, cria um novo
                    EmailInfoEx email = new EmailInfoEx(0u)
                    {
                        id = id
                    };

                    if (is_valid_c_string(_result.data[1]))
                    {
                        STRCPY_TO_MEMORY_FIXED_SIZE(ref email.from_id, sizeof(char), _result.data[1]);
                    }

                    if (is_valid_c_string(_result.data[2]))
                    {
                        STRCPY_TO_MEMORY_FIXED_SIZE(ref email.gift_date, sizeof(char), _result.data[2]);
                    }

                    if (is_valid_c_string(_result.data[3]))
                    {
                        try
                        {
                            // Translate Msg From Encoded Char not printed 
                            STRCPY_TO_MEMORY_FIXED_SIZE(ref email.msg, sizeof(char), _result.data[3]);

                        }
                        catch (exception e)
                        {
                            _smp.message_pool.push(new message("[CmdMailBoxInfo2::LineResult][ErrorSystem][Error during translation] " + e.getFullMessageError(), type_msg.CL_FILE_LOG_AND_CONSOLE));
                        }
                    }

                    email.visit_count = IFNULL(_result.data[4]);
                    email.lida_yn = (byte)IFNULL(_result.data[5]);

                    // Adiciona o item
                    EmailInfo.item item = new EmailInfo.item
                    {
                        _typeid = IFNULL(_result.data[7])
                    };

                    if (item._typeid != 0)
                    {
                        item.id = IFNULL(_result.data[6]); // ID vem antes de typeid na consulta
                        item.flag_time = (byte)IFNULL(_result.data[8]);
                        item.qntd = IFNULL(_result.data[9]);
                        item.tempo_qntd = IFNULL(_result.data[10]);
                        item.pang = IFNULL(_result.data[11]);
                        item.cookie = IFNULL(_result.data[12]);
                        item.gm_id = IFNULL(_result.data[13]);
                        item.flag_gift = IFNULL(_result.data[14]);

                        if (is_valid_c_string(_result.data[15]))
                        {
                            STRCPY_TO_MEMORY_FIXED_SIZE(ref item.ucc_img_mark, sizeof(char), _result.data[15]);
                        }

                        item.type = (short)IFNULL(_result.data[16]);

                        // Adiciona o item ao novo email
                        email.itens.Add(item);
                    }

                    // Adiciona o email ao dicionário
                    m_emails.Add(email.id, email);
                }
            }
            catch (Exception e)
            {

                throw e;
            }
        }

        protected override Response prepareConsulta()
        {
            if (m_uid == 0u)
            {
                throw new exception("[CmdMailBoxInfo2::PrepareConsulta][Error] m_uid is invalid(0)", ExceptionError.STDA_MAKE_ERROR_TYPE(STDA_ERROR_TYPE.PANGYA_DB, 4, 0));
            }

            if (m_emails.Count > 0)
            {
                m_emails.Clear();
            }

            var r = procedure(m_szConsulta, m_uid.ToString());

            checkResponse(r, "Não conseguiu pegar todos os emails da caixa de correio do player[UID=" + m_uid + "]");

            return r;
        }

        private uint m_uid;
        private Dictionary<uint, EmailInfoEx> m_emails = new Dictionary<uint, EmailInfoEx>();

        private const string m_szConsulta = "pangya.ProcGetAllEmailFromMailBox";
    }
}
