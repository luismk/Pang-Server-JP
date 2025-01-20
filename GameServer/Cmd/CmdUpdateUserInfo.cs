using GameServer.Cmd;
using GameServer.PangType;
using PangyaAPI.SQL;
using PangyaAPI.Utilities;
using System;

// Arquivo cmd_update_user_info.cpp
// Criado em 08/09/2018 as 12:42 por Acrisio
// Implementa��o da classe CmdUpdateUserInfo

#if _WIN32
// C++ TO C# CONVERTER TASK: There is no equivalent to most C++ 'pragma' directives in C#:
//#pragma pack(1)
#endif

// Arquivo cmd_update_user_info.hpp
// Criado em 08/09/2018 as 12:36 por Acrisio
// Defini��o da classe CmdUpdateUserInfo


// C++ TO C# CONVERTER WARNING: The following #include directive was ignored:
//#include "../../Projeto IOCP/PANGYA_DB/pangya_db.h"
// C++ TO C# CONVERTER NOTE: The following #define macro was replaced in-line:
// ORIGINAL LINE: #define m_title skin_typeid[5]
// C++ TO C# CONVERTER NOTE: The following #define macro was replaced in-line:
// ORIGINAL LINE: #define GameServer.Cmd_C_ITEM_QNTD c[0]
// C++ TO C# CONVERTER NOTE: The following #define macro was replaced in-line:
// ORIGINAL LINE: #define GameServer.Cmd_C_ITEM_TICKET_REPORT_ID_HIGH c[1]
// C++ TO C# CONVERTER NOTE: The following #define macro was replaced in-line:
// ORIGINAL LINE: #define GameServer.Cmd_C_ITEM_TICKET_REPORT_ID_LOW c[2]
// C++ TO C# CONVERTER NOTE: The following #define macro was replaced in-line:
// ORIGINAL LINE: #define GameServer.Cmd_C_ITEM_TIME c[3]

namespace GameServer.Cmd
{
	public class CmdUpdateUserInfo : Pangya_DB
	{
			public CmdUpdateUserInfo()
			{
				this.m_uid = 0u;
				this.m_ui = new UserInfoEx();
			}

			public CmdUpdateUserInfo(uint _uid,
				UserInfoEx _ui
				)
				{   this.m_uid = _uid;
				this.m_ui = _ui;
				}	   
			public uint getUID()
			{
				return (m_uid);
			}

			public void setUID(uint _uid)
			{
			m_uid = _uid;
			}

			public UserInfoEx getInfo()
			{	  return m_ui;							    
			}

			public void setInfo(UserInfoEx _ui)
			{	   m_ui = _ui;
			}

			protected override void lineResult(ctx_res _result, uint _index_result)
			{

				// N�o usa por que � um UPDATE
				return;
			}

			protected override Response prepareConsulta()
			{

				if(m_uid == 0)
				{
					throw new exception("[CmdUpdateUserInfo::prepareConsulta][Error] m_uid is invalid(zero)", ExceptionError.STDA_MAKE_ERROR_TYPE(STDA_ERROR_TYPE.PANGYA_DB,
						4, 0));
				}

				var r = procedure(
					m_szConsulta,
					Convert.ToString(m_uid) + ", " + Convert.ToString(m_ui.best_drive) + ", " + Convert.ToString(m_ui.best_long_putt) + ", " + Convert.ToString(m_ui.best_chip_in) + ", " + Convert.ToString(m_ui.combo) + ", " + Convert.ToString(m_ui.all_combo) + ", " + Convert.ToString(m_ui.tacada) + ", " + Convert.ToString(m_ui.putt) + ", " + Convert.ToString(m_ui.tempo) + ", " + Convert.ToString(m_ui.tempo_tacada) + ", " + Convert.ToString(m_ui.acerto_pangya) + ", " + Convert.ToString(m_ui.timeout) + ", " + Convert.ToString(m_ui.ob) + ", " + Convert.ToString(m_ui.total_distancia) + ", " + Convert.ToString(m_ui.hole) + ", " + Convert.ToString(m_ui.hole_in) + ", " + Convert.ToString(m_ui.hio) + ", " + Convert.ToString(m_ui.bunker) + ", " + Convert.ToString(m_ui.fairway) + ", " + Convert.ToString(m_ui.albatross) + ", " + Convert.ToString(m_ui.mad_conduta) + ", " + Convert.ToString(m_ui.putt_in) + ", " + Convert.ToString(m_ui.media_score) + ", " + Convert.ToString(m_ui.best_score[0]) + ", " + Convert.ToString(m_ui.best_score[1]) + ", " + Convert.ToString(m_ui.best_score[2]) + ", " + Convert.ToString(m_ui.best_score[3]) + ", " + Convert.ToString(m_ui.best_score[4]) + ", " + Convert.ToString(m_ui.best_pang[0]) + ", " + Convert.ToString(m_ui.best_pang[1]) + ", " + Convert.ToString(m_ui.best_pang[2]) + ", " + Convert.ToString(m_ui.best_pang[3]) + ", " + Convert.ToString(m_ui.best_pang[4]) + ", " + Convert.ToString(m_ui.sum_pang) + ", " + Convert.ToString(m_ui.event_flag) + ", " + Convert.ToString(m_ui.jogado) + ", " + Convert.ToString(m_ui.team_game) + ", " + Convert.ToString(m_ui.team_win) + ", " + Convert.ToString(m_ui.team_hole) + ", " + Convert.ToString(m_ui.ladder_point) + ", " + Convert.ToString(m_ui.ladder_hole) + ", " + Convert.ToString(m_ui.ladder_win) + ", " + Convert.ToString(m_ui.ladder_lose) + ", " + Convert.ToString(m_ui.ladder_draw) + ", " + Convert.ToString(m_ui.quitado) + ", " + Convert.ToString(m_ui.skin_pang) + ", " + Convert.ToString(m_ui.skin_win) + ", " + Convert.ToString(m_ui.skin_lose) + ", " + Convert.ToString(m_ui.skin_run_hole) + ", " + Convert.ToString(m_ui.skin_all_in_count) + ", " + Convert.ToString(m_ui.disconnect) + ", " + Convert.ToString(m_ui.jogados_disconnect) + ", " + Convert.ToString(m_ui.event_value) + ", " + Convert.ToString(m_ui.skin_strike_point) + ", " + Convert.ToString(m_ui.sys_school_serie) + ", " + Convert.ToString(m_ui.game_count_season) + ", " + Convert.ToString(m_ui.total_pang_win_game) + ", " + Convert.ToString(m_ui.medal.lucky) + ", " + Convert.ToString(m_ui.medal.fast) + ", " + Convert.ToString(m_ui.medal.best_drive) + ", " + Convert.ToString(m_ui.medal.best_chipin) + ", " + Convert.ToString(m_ui.medal.best_puttin) + ", " + Convert.ToString(m_ui.medal.best_recovery) + ", " + Convert.ToString(m_ui._16bit_nao_sei));

				checkResponse(r, "nao conseguiu atualizar o User Info do player[UID=" + Convert.ToString(m_uid) + "]");

				return r;
			}	    

			private uint m_uid = new uint();
			private UserInfoEx m_ui = new UserInfoEx();

			private const string m_szConsulta = "pangya.ProcUpdateUserInfo";
	}
}
