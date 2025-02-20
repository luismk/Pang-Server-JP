// Arquivo CmdAchievementInfo.cs
// Criado em 21/03/2018 às 21:56 por Acrisio
// Implementação da classe CmdAchievementInfo

using GameServer.GameType;
using PangyaAPI.SQL;
using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace GameServer.Cmd
{
    public class CmdAchievementInfo : Pangya_DB
    {
        private uint m_uid;
        private readonly Dictionary<uint, AchievementInfoEx> map_ai = new Dictionary<uint, AchievementInfoEx>();

        public CmdAchievementInfo()
        {
            m_uid = 0;

            //if (!sIff.Instance.IsLoaded)
            //    sIff.Instance.Load();
        }

        public CmdAchievementInfo(uint _uid)
        {
            m_uid = _uid;

            //if (!sIff.Instance.IsLoaded)
            //    sIff.Instance.Load();
        }

        ~CmdAchievementInfo()
        {
        }

        protected override void lineResult(ctx_res _result, uint _index_result)
        {
            checkColumnNumber(10);

            //var ai = new AchievementInfoEx();
            //var qsi = new QuestStuffInfo();
            //var cii = new CounterItemInfo { active = 1 };

            //ai._typeid = IFNULL(_result.data[1]);
            //ai.id = IFNULL(_result.data[2]);

            //qsi.id = IFNULL(_result.data[4]);
            //qsi._typeid = IFNULL(_result.data[5]);
            //cii._typeid = IFNULL(_result.data[6]);
            //cii.id = qsi.counter_item_id = IFNULL(_result.data[7]);
            //cii.value = IFNULL(_result.data[8]);
            //qsi.clear_date_unix = IFNULL(_result.data[9]);

            //if (!map_ai.ContainsKey(ai._typeid) ||
            //    (map_ai.TryGetValue(ai._typeid, out var existingAi) && existingAi.id != ai.id))
            //{

            //    ai.active = (byte)IFNULL(_result.data[0]);
            //    ai.status = IFNULL(_result.data[3]);

            //    CheckAchievementRetorno(ai);
            //    CheckQuestAchievement(ai, qsi);

            //    ai.v_qsi.Add(qsi);

            //    if (cii.id > 0)
            //        ai.map_counter_item[cii.id] = cii;

            //    map_ai[ai._typeid] = ai;
            //}
            //else if (map_ai[ai._typeid].id == ai.id)
            //{
            //    CheckQuestAchievement(map_ai[ai._typeid], qsi);
            //    map_ai[ai._typeid].v_qsi.Add(qsi);

            //    if (cii.id > 0)
            //        map_ai[ai._typeid].map_counter_item[cii.id] = cii;
            //}
            //else
            //{
            //    // requestCommonCmdGM duplicate TypeId logic here
            //}
        }

        protected override Response prepareConsulta()
        {
            map_ai.Clear();

            var stopwatch = Stopwatch.StartNew();

            var response = procedure(m_szConsulta, m_uid.ToString());

            stopwatch.Stop();
            Console.WriteLine($"[CmdAchievementInfo][Log] Query executed in {stopwatch.ElapsedMilliseconds}ms");

            checkResponse(response, $"Failed to retrieve achievement info for player: {m_uid}");

            return response;
        }

        private void CheckAchievementRetorno(AchievementInfoEx ai)
        {
            //var achievement = sIff.Instance.FindAchievement(ai.TypeId);

            //if (sIff.Instance.GetItemGroupIdentify(ai.TypeId) != Iff.QUEST_ITEM && achievement != null)
            //{
            //    ai.QuestBaseTypeId = achievement.TypeIdQuestIndex;
            //}
            //else if (sIff.Instance.GetItemGroupIdentify(ai.TypeId) == Iff.ACHIEVEMENT)
            //{
            //    Console.WriteLine($"[CmdAchievementInfo::LineResult][WARNING] Achievement[TypeId={ai.TypeId}] not found in .iff file for player: {m_uid}");
            //}
        }

        private void CheckQuestAchievement(AchievementInfoEx ai, QuestStuffInfo qsi)
        {
            //if (ai.Status == 3 && (ai.QuestBaseTypeId == 0 || qsi.TypeId == ai.QuestBaseTypeId) && qsi.CounterItemId <= 0)
            //{
            //    Console.WriteLine($"[CmdAchievementInfo::LineResult][WARNING] Quest achievement[TypeId={qsi.TypeId}] does not have a counter item. Player: {m_uid}");
            //}
        }

        public Dictionary<uint, AchievementInfoEx> GetInfo()
        {
            return map_ai;
        }

        public uint GetUID()
        {
            return m_uid;
        }

        public void SetUID(uint uid)
        {
            m_uid = uid;
        }
        string m_szConsulta = "pangya.ProcGetNewAchievement";
    }
}
