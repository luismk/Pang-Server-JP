namespace Pangya_GameServer.PangyaEnums
{
    public enum COMMON_CMD_GM : short
    {
        CCG_DUMMY = 0x0,
        CCG_HELP = 0x1,
        CCG_COMMAND = 0x2,
        CCG_VISIBLE = 0x3,
        CCG_WHISPER = 0x4,
        CCG_CHANNEL = 0x5,
        CCG_SHOWSTATUS = 0x6,
        CCG_LIST = 0x7,
        CCG_OPEN_WHISPER_PLAYER_LIST = 0x8,
        CCG_CLOSE_WHISPER_PLAYER_LIST = 0x9,
        CCG_KICK = 0xA,
        CCG_DISCONNECT = 0xB,// Disconnect UID
        CCG_DESTROY = 0xC,
        CCG_CHANGE_WIND_VERSUS = 0xD,
        CCG_CHANGE_WEATHER = 0xE, 
        CCG_IDENTITY = 0xF,
        CCG_NOTICE = 0x10,
        CCG_GIVEITEM = 0x11,
        CCG_GOLDENBELL = 0x12,
        CCG_SETPRIZE = 0x13,
        CCG_UNSETPRIZE = 0x14,
        CCG_SHOWPRIZE = 0x15,
        CCG_LOADSCRIPT = 0x16,
        CCG_SETREADY = 0x17,
        CCG_NOTICEPRIZE = 0x18,
        CCG_GETTID = 0x19,
        CCG_SETMISSION = 0x1A,
        CCG_FINDITEM = 0x1B,
        CCG_CATEGORY = 0x1C,
        CCG_WEBMATCHMAP = 0x1D,
        CCG_WEBMATCHHOLE = 0x1E
    }
}
