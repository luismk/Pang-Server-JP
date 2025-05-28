namespace Pangya_GameServer.PangyaEnums
{
    public enum COMMON_CMD_GM : short
    {
        CCG_VISIBLE = 3,
        CCG_WHISPER,
        CCG_CHANNEL,
        CCG_OPEN_WHISPER_PLAYER_LIST = 8,
        CCG_CLOSE_WHISPER_PLAYER_LIST,
        CCG_KICK,
        CCG_DISCONNECT,                     // Disconnect UID
        CCG_DESTROY = 13,
        CCG_CHANGE_WIND_VERSUS,
        CCG_CHANGE_WEATHER,
        CCG_IDENTITY,
        CCG_GIVE_ITEM = 18,
        CCG_GOLDENBELL,
    }
}
