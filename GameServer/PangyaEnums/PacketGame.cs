﻿namespace Pangya_GameServer.PangyaEnums
{  
    /// <summary>
    /// Names of Packets Server
    /// </summary>
    public enum PacketIDServer
    {
        CLIENT_CONNECT_TO_SERVER_0x3F = 0x3F,
        SERVER_CHATMSG_0x40,
        SERVER_ANNOUNCEMENT_0x41,
        SERVER_NOTIFY_0x42,
        SERVER_NEW_USER_0x43,
        SERVER_LOGIN_ACK_0x44,
        SERVER_MY_STATISTICS_0x45,
        SERVER_USERLIST_0x46,
        SERVER_ROOMLIST_0x47,
        SERVER_ROOMUSERLIST_0x48,
        SERVER_ROOM_ENTER_RESULT_0x49,
        SERVER_ROOM_INFO_CHANGED_0x4A,
        SERVER_ROOM_USER_INFO_CHANGED_0x4B,
        SERVER_EXIT_ROOM_ACK_0x4C,
        SERVER_CHANNEL_LIST_0x4D,
        SERVER_CHANNEL_ENTER_ACK_0x4E,
        SERVER_CHANNEL_LEAVE_ACK_0x4F,
        SERVER_CHANGE_NICK_ACK_0x50,
        SERVER_CHANGE_SCHOOL_ACK_0x51,
        SERVER_GAME_START_0x52,
        SERVER_GAME_SYNC_START_0x53,
        SERVER_CURRENT_GAME_CANCEL_0x54,
        SERVER_SHOT_0x55,
        SERVER_CAMERA_0x56,
        SERVER_CLICK_0x57,
        SERVER_POWER_SHOT_0x58,
        SERVER_CLUB_0x59,
        SERVER_USE_ITEM_0x5A,
        SERVER_WIND_0x5B,
        SERVER_TIMEOUT_0x5C,
        SERVER_EMOTICON_0x5D,
        SERVER_CADDIE_ABILITY_0x5E,
        SERVER_UPDATE_DIRECTION_0x5F,
        SERVER_DROP_0x60,
        SERVER_DISCONNECT_0x61,
        SERVER_KEEPALIVE_0x62,
        SERVER_NEXT_TURN_0x63,
        SERVER_SHOT_RESULT_0x64,
        SERVER_MOVE_NEXT_HOLE_0x65,
        SERVER_GAME_END_0x66,
        SERVER_RETURN_ROOM_0x67,
        SERVER_RESPONSE_BUY_ITEM_0x68,
        SERVER_RESPONSE_SELL_ITEM_0x69,
        SERVER_RESPONSE_GIFT_ITEM_0x6A,
        SERVER_RESPONSE_EQUIP_ITEM_0x6B,
        SERVER_UPDATE_STATUS_0x6C,
        SERVER_UPDATE_HOLE_0x6D,
        SERVER_UPDATE_POS_0x6E,
        SERVER_FIRST_LOGIN_0x6F,
        SERVER_CHARACTER_INFO_0x70,
        SERVER_CADDIE_INFO_0x71,
        SERVER_EQUIP_INFO_0x72,
        SERVER_ITEM_INFO_0x73,
        SERVER_SERVER_DB_ERROR_0x74,
        SERVER_YOU_INVALID_PACKET_0x75,
        SERVER_FINAL_PLAYERINFO_0x76,
        SERVER_ALL_READY_WAIT_0x77,
        SERVER_CLIENT_READY_0x78,
        SERVER_GAME_RESULT_0x79,
        SERVER_GIFT_INFO_0x7A,
        SERVER_MOVE_GIFT_0x7B,
        SERVER_DECISION_ROOM_MASTER_0x7C,
        SERVER_CHANGE_TEAM_0x7D,
        SERVER_KICK_FROM_ROOM_0x7E,
        SERVER_GAMESTART_FALIED_0x7F,
        SERVER_GAME_PLAYER_STATISTICS_0x80,
        SERVER_VOTE_FOR_BANISH_0x81,
        SERVER_BANISH_0x82,
        SERVER_INVITE_0x83,
        SERVER_WHISPER_0x84,
        SERVER_REFUSAL_MSG_0x85,
        SERVER_DETAIL_ROOM_INFO_0x86,
        SERVER_TUTORIAL_ACK_0x87,
        SERVER_USER_STATE_0x88,
        SERVER_RESPONSE_USERINFO_0x89,
        SERVER_REQUEST_SHOT_RESULT_0x8A,
        SERVER_PAUSE_0x8B,
        SERVER_GAME_END_EARLY_0x8C,
        SERVER_GAME_TIME_SYNC_0x8D,
        SERVER_SLEEP_0x8E,
        SERVER_SKIP_0x8F,
        SERVER_TEESHOT_READY_ACK_0x90,
        SERVER_TEAM_GAME_END_0x91,
        SERVER_ASK_GOSTOP_0x92,
        SERVER_REEMPLOY_CADDIE_ACK_0x93,
        SERVER_REPORT_0x94,
        SERVER_MSN_ACK_0x95,
        SERVER_MY_CASH_0x96,
        SERVER_30S_MATCH_TID_0x97,
        SERVER_CHANGE_TARGET_0x98,
        SERVER_REQUEST_PLAYINFO_0x99,
        SERVER_ADMIT_IDENTITY_0x9A,
        SERVER_GAMEMODE_0x9B,
        SERVER_SHOT_COMMEND_0x9C,
        SERVER_RETARGET_MC_0x9D,
        SERVER_WEATHER_0x9E,
        SERVER_ACK_SERVER_LIST_0x9F,
        SERVER_TITLE_LIST_0xA0,
        SERVER_RESPONSE_USERINFO_OFFLINE_0xA1,
        SERVER_RESPONSE_RANKADDRESS_0xA2,
        SERVER_LOADING_INFO_0xA3,
        SERVER_REPLAY_0xA4,
        SERVER_ENCHANT_0xA5,
        SERVER_RESPONSE_DRAWBACK_GIFT_0xA6,
        SERVER_UPDATE_HOLE_ITEM_0xA7,
        SERVER_BANISH_ALL_0xA8,
        SERVER_SET_SYSTEM_0xA9,
        SERVER_NEW_ITEM_0xAA,
        SERVER_WINNING_PRIZE_0xAB,
        SERVER_CHAT_PENALITY_0xAC,
        SERVER_FIND_USER_ACK_0xAD,
        SERVER_ROOMGALLERYLIST_0xAE,
        SERVER_UPDATE_EXP_0xAF, // FAMILY
        SERVER_TEAMCHAT_0xB0,
        SERVER_RECOVER_ITEMSLOT_0xB1,
        SERVER_NOTELIST_0xB2,
        SERVER_MATCH_GAME_END_0xB3,
        SERVER_SP_TROPHY_LIST_0xB4,
        SERVER_USE_TIME_ITEM_0xB5,
        SERVER_TIME_ITEM_REMOVE_0xB6,
        SERVER_BUY_TIME_ITEM_ACK_0xB7,
        SERVER_OPEN_FORTUNE_0xB8,
        SERVER_NEW_RECORD_0xB9,
        SERVER_RESPONSE_SERVER_TIME_0xBA, // BB (ORDER TH S4.9)
        SERVER_SET_EVENT_0xBB,
        SERVER_SPY_ENTER_ROOM_0xBC,
        SERVER_SPY_START_GAME_0xBD,
        SERVER_SPY_HOLD_GAME_0xBE,
        SERVER_GUILD_MATCHUP_INFO_0xBF,
        SERVER_REQUEST_GUILD_INFO_ACK_0xC0,
        SERVER_REQ_GUILD_MATCH_RECORD_ACK_0xC1,
        SERVER_GUILD_SCORE_0xC2,
        SERVER_BEFORE_START_COUNT_0xC3,
        SERVER_SYNC_ACTIVITY_0xC4,
        SERVER_DELETE_ITEM_0xC5,
        SERVER_RESTORE_0xC6,
        SERVER_SPEED_RATE_0xC7,
        SERVER_MY_PANG_0xC8,
        SERVER_ONELINE_MSG_0xC9,   // ONELINE TICKER
        SERVER_ONELINE_QUERY_0xCA,
        SERVER_RESPONSE_ONELINE_MSG_0xCB,
        SERVER_EXT_PRIZE_0xCC,
        SERVER_COMPOUND_ACK_0xCD,
        SERVER_PRIZE_LIST_0xCE,
        SERVER_QUEST_LIST_0xCF,
        SERVER_ITEM_INFO_GUID_0xD0,
        SERVER_MAP_SCORE_EVENT_0xD1,
        SERVER_MAP_BINGO_EVENT_0xD2,
        SERVER_REFLECTED_ITEM_0xD3,
        SERVER_CHECK_CADDIE_WARNING_0xD4,
        SERVER_USE_COUPON_0xD5,
        SERVER_RESPONSE_BONGDARISHOP_ITEM_0xD6,
        SERVER_REQUEST_GGCSAUTH_0xD7,
        SERVER_GUILD_TROPHY_LIST_0xD8,
        SERVER_TIMEATTACK_CONTROL_0xD9,
        SERVER_SERVER_SERVICE_CONTROL_0xDA,
        SERVER_NEW_ITEM_FAILED_0xDB,
        SERVER_GIFT_DELETE_0xDC,
        SERVER_SCRATCH_ITEM_0xDD,
        SERVER_SCRATCH_SERIAL_NUMBER_0xDE,
        SERVER_SCRATCH_CARD_NUMBER_0xDF,
        SERVER_UPDATE_PATCH_VERSION_0xE0,
        SERVER_MASCOT_INFO_0xE1,
        SERVER_CHANGE_MASCOT_0xE2,
        SERVER_TRADE_OPEN_SHOP_0xE3,
        SERVER_TRADE_CLOSE_SHOP_0xE4,
        SERVER_TRADE_EDIT_SHOPT_0xE5,
        SERVER_TRADE_ENTER_SHOPT_0xE6,
        SERVER_TRADE_EXIT_SHOP_0xE7,
        SERVER_TRADE_EDIT_TITLE_0xE8,
        SERVER_TRADE_SHOW_VISITOR_0xE9,
        SERVER_TRADE_INCOME_0xEA,
        SERVER_TRADE_EDIT_ITEM_0xEB,
        SERVER_TRADE_BUY_ITEM_0xEC,
        SERVER_TRADE_MINUS_ITEM_0xED,
        SERVER_TRADE_BLOCK_0xEE,
        SERVER_RECYCLE_ITEM_0xEF,
        SERVER_MOVE_ROOM_0xF0,
        SERVER_LOGIN_ACK_END_0xF1,
        SERVER_COUNTDOWN_0xF2,
        SERVER_JACKPOT_0xF3,
        SERVER_WAITLIST_0xF4,
        SERVER_ENTER_LOBBY_0xF5,
        SERVER_LEAVE_LOBBY_0xF6,
        SERVER_MOVE_LOBBY_TO_LOBBY_0xF7,
        SERVER_AUTO_SHUTDOWN_NOTICE_0xF8,
        SERVER_UPDATE_SERVER_INFO_0xF9,
        SERVER_ALL_PRIZE_LIST_0xFA,
        SERVER_BS_USABLE_TIMES_0XFB, // BS (BONGDARISHOP = BONGDARI SHOP)
        SERVER_ACK_MESSENGER_SERVER_LIST_0xFC,
        SERVER_POINT_EVENT_POINT_0xFD,
        SERVER_POINT_EVENT_EXCHANGE_ITEM_0xFE,
        SERVER_POINT_EVENT_REMAINED_ITEM_0xFF,
        SERVER_POINT_EVENT_SUBSCRIBED_ITEM_0x100,
        SERVER_LOGIN_FROM_NETCAFE_0x101,
        SERVER_UPDATE_GACHA_TICKETS_0x102,
        SERVER_RESPONSE_REFRESH_ITEMLIST_0x103,
        SERVER_RAWMATL_DROP_0x104,
        SERVER_OPEN_FORTUNE_NEW_0x105,
        SERVER_MY_PANGYA_QUIZ_LEVEL_0x106,
        SERVER_CHECK_PARAN_KID_0x107,
        SERVER_CHANGE_NICK_FREE_ACK_0x108,
        SERVER_OPEN_NEWYEAR_MONEY_0x109,
        SERVER_NEW_GIFT_LIST_0x10A,
        SERVER_BONGDARI_BONUS_TIMES_0x10B,
        SERVER_TO_BE_DISCONNECTED_0x10C,
        SERVER_OPEN_EVENT_GIFTBOX_0x10D,
        SERVER_USER_MATCH_HISTORY_0x10E,
        SERVER_LEVELUP_ITEM_0x10F,
        SERVER_GM_TOOLKIT_RESULT_0x110,
        SERVER_ONLINE_MSG_BLOCK_0x111,
        SERVER_FIRST_CONNECT_0x112,
        SERVER_INTRUSION_0x113, // ENTER AFTER IN TOURNEY
        SERVER_MAKE_NOTIFYDLG_0x114,
        SERVER_MAKE_BONUSTICKET_0x115,
        SERVER_RESPONSE_GUILD_WAITING_LIST_0x116,
        SERVER_RESPONSE_GUILD_ACCEPT_MEMBER_0x117,
        SERVER_RESPONSE_GUILD_INVITATION_LIST_0x118,
        SERVER_RESPONSE_GUILD_ACCEPT_INVITATION_0x119,
        SERVER_OPEN_TIKI_REPORT_0x11A,
        SERVER_TIKI_REPORT_LEAVE_USER_0x11B,
        SERVER_YOU_GOT_TIKI_REPORT_0x11C,
        SERVER_WORLD_TOUR_EVENT_FLAG_0x11D,
        SERVER_WORLD_TOUR_EVENT_CONFIRM_GIFT_0x11E,
        SERVER_COMPLETE_QUEST_FLAG_0x11F,
        SERVER_COMPLETE_QUEST_GRADE_GIFT_0x120,
        SERVER_RESPONSE_KICK_0x121,
        SERVER_BLOCK_CHAT_ACK_0x122,
        SERVER_NOTIFY_UNDER18_0x123,
        SERVER_MULTIPLIER_SET_0x124,
        SERVER_NETCAFEPANGDBL_0x125, // DBL "DOUBLE"
        SERVER_MAP_EVENT_0x126,
        SERVER_HALLOWEEN2007_EVENT_INFO_0x127,
        SERVER_HALLOWEEN2007_EVENT_CONFIRM_GIFT_0x128,
        SERVER_OPEN_LUCKY_POUCH_0x129,
        SERVER_NOTICE_TO_CLIENT_0x12A,
        SERVER_REALMYROOM_ENTER_RESULT_0x12B,
        SERVER_RMR_SAVE_RESULT_0x12C,
        SERVER_RMR_LOAD_RESULT_0x12D,
        SERVER_UCC_RESPONSE_0x12E,
        SERVER_CHECK_INVITE_ACK_0x12F,
        SERVER_CANCEL_INVITE_NOTIFY_0x130,
        SERVER_SEND_TGAUGE_0x131, // TREASURE HUNTER
        SERVER_SEND_TPOINT_0x132,
        SERVER_SEND_TGIFTLIST_0x133,
        SERVER_UPDATE_TGIFTLIST_0x134,
        SERVER_CARD_INFO_CLEAR_0x135,
        SERVER_AVILITY_CARD_INFO_CLEAR_0x136,
        SERVER_AVILITY_CARD_INFO_0x137,
        SERVER_CARD_INFO_0x138,
        SERVER_DELETE_CARD_0x139,
        SERVER_REQUEST_MATCHING_0x13A,         // GHOST
        SERVER_RESPONSE_MATCHING_0x13B,
        SERVER_NOTIFY_MATCHING_STATE_0x13C,
        SERVER_NOTIFY_ROOM_POSITION_0x13D,
        SERVER_NOTIFY_MATCHING_CANCEL_0x13E,
        SERVER_NOTIFY_USER_INSTREST_LIST_0x13F,
        SERVER_REPONSE_UPDATE_USER_INSTREST_0x140,
        SERVER_NOTIFY_MATCHED_CONDITION_0x141,
        SERVER_DELETE_SPECIAL_AVILITY_0x142,
        SERVER_GAME_GHOST_0x143,
        SERVER_NOTIFY_INSTREST_ALL_LIST_0x144,
        SERVER_GIFT_INFO_PAGE_0x145,
        SERVER_SEND_MAIL_SUCCESS_0x146,
        SERVER_SEND_MAIL_FAILED_0x147,
        SERVER_GET_MAILLIST_SUCCESS_0x148,
        SERVER_GET_MAILLIST_FAILED_0x149,
        SERVER_GET_MAIL_SUCCESS_0x14A,
        SERVER_GET_MAIL_FAILED_0x14B,
        SERVER_MOVE_INCLUDE_ITEM_SUCCESS_0x14C,
        SERVER_MOVE_INCLUDE_ITEM_FAILED_0x14D,
        SERVER_NOTICE_APPROACH_GAME_RESULT_0x14E,
        SERVER_NOTICE_APPROACH_MISSION_TYPE_0x14F,
        SERVER_NOTICE_APPROACH_MISSION_WINNER_0x150,
        SERVER_NOTICE_APPROACH_GAME_END_0x151,
        SERVER_NOTICE_APPROACH_GIFT_0x152,
        SERVER_SECURITY_KEY_0x153,
        SERVER_OPEN_CARDPACK_0x154,
        SERVER_REPONSE_GET_USERINFO_0x155,
        SERVER_SEND_USEREQUIP_0x156,
        SERVER_SEND_USERINFO_0x157,
        SERVER_SEND_USER_STAT_0x158,
        SERVER_SEND_TROPHY_0x159,
        SERVER_SEND_SPECIALTROPHY_0x15A,
        SERVER_SEND_GUILDTROPHY_0x15B,
        SERVER_SEND_NEWRECORD_0x15C,
        SERVER_SEND_GUILDINFO_0x15D,
        SERVER_SEND_CHARACTEREQUIP_0x15E,
        SERVER_SEND_MATCHHISTORY_0x15F,
        SERVER_USE_CARD_0x160,
        SERVER_CHANGE_CARD_INFO_0x161,
        SERVER_ATTACH_CARD_0x162,
        SERVER_OTHERSERVER_ROOMLIST_0x163,
        SERVER_DELETE_MAIL_SUCCESS_0x164,
        SERVER_DELETE_MAIL_FAILED_0x165,
        SERVER_NEW_MAIL_LIST_0x166,
        SERVER_DELETE_READY_MAIL_LIST_0x167,
        SERVER_RMR_USERLIST_0x168,
        SERVER_NORMAL_TROPHY_LIST_0x169,
        SERVER_MASCOT_EFFECT_SEED_0x16A,
        GCR_REFRESH_GUILDMGR_0x16B,
        SERVER_ITEMSTORAGE_RES_ACCESS_0x16C,
        SERVER_ITEMSTORAGE_RES_PAGE_0x16D,
        SERVER_ITEMSTORAGE_RES_PUSH_0x16E,
        SERVER_ITEMSTORAGE_RES_POP_0x16F,
        SERVER_ITEMSTORAGE_RES_STATE_0x170,
        SERVER_ITEMSTORAGE_RES_PANG_INOUT_0x171,
        SERVER_ITEMSTORAGE_RES_PANG_0x172,
        SERVER_ITEMSTORAGE_RES_LOCK_0x173,
        SERVER_ITEMSTORAGE_RES_CHANGE_PW_0x174,
        SERVER_ITEMSTORAGE_RES_EDIT_0x175,
        SERVER_ITEMSTORAGE_RES_SET_PASSWORD_0x176,
        SERVER_ITEMSTORAGE_REFRESH_ITEMS_0x177,
        SERVER_CARDSTACK_INFO_0x178,
        SERVER_BURNNING_SP_CARD_INFO_0x179,
        SERVER_CHANGE_ATTACH_CARD_INFO_0x17A,
        SERVER_CHECK_COMEBACK_0x17B,
        SERVER_YOU_MAILIST_REFRESH_0x17C,
        SERVER_USER_BLOCK_0x17D,
        SERVER_FURNITURE_ABILITY_USE_SUCCESS_0x17E,
        SERVER_FURNITURE_ABILITY_USE_FAILED_0x17F,
        SERVER_INSERT_ITEM_0x180,
        SERVER_ITEM_BUFF_0x181,
        SERVER_PRIVATE_TRADE_0x182,
        SERVER_VALENTINE_OPEN_RES_0x183,
        SERVER_VALENTINE_GIFT_RES_0x184,
        SERVER_TIKI_MAGICBOX_RES_VERSION_0x185,
        SERVER_TIKI_MAGICBOX_RES_OUTPUT_0x186,
        SERVER_TICKET_RESULT_0x187,
        SERVER_USE_CARD_REMOVE_RES_0x188,
        SERVER_ACK_MISSION_EVENT_OPEN_0x189,
        SERVER_ACK_MISSION_EVENT_GIFT_0x18A,
        SERVER_ACK_BINGO_EVENT_INFO_0x18B,
        SERVER_ACK_BINGO_EVENT_GIFT_0x18C,
        SERVER_RESPONSE_CUTIN_0x18D,
        SERVER_NOTICE_WEB_MATCH_0x18E,
        SERVER_EXTEND_RENTAL_0x18F,
        SERVER_DELETE_RENTAL_0x190,
        SERVER_RESPONSE_UPGRADE_CADDIE_0x194 = 0x194,
        SERVER_SEND_MY_MAPSTAT_0x195,
        SERVER_CHARACTER_STAT_IN_CHATROOM_ACK_0x196,
        SERVER_SUPPLY_PACK_OPEN_0x197, // COMMET REFIL
        SERVER_SEND_SSC_COIN_0x198,
        SERVER_LAST_HOLE_FINISHED_0x199,
        SERVER_KOOH_BIRTHDAY_BINGO_0x19A,
        SERVER_KOOH_BIRTHDAY_GIFT_0x19B,
        SERVER_KOOH_BIRTHDAY_MISSION_SUCCESS_0x19C,
        SERVER_RES_USE_NEW_RANDOMBOX_0x19D,
        SERVER_HEARTBEAT_0x1A9 = 0x1A9,
        SERVER_WEB_AUTH_KEY_ACK_0x1AD = 0x1AD, // APARTIR DAQUI SÃO MEUS PRÓPRIOS NOMES PARAS OS TIPOS DE PACOTES
        SERVER_REQ_ALL_UCC_FROM_ALL_PLAYER_ACK_0x1B1 = 0x1B1,
        SERVER_NEW_GUILD_MNGR_CREATE_ACK_0x1B5 = 0x1B5,
        SERVER_NEW_GUILD_MNGR_CHECK_NAME_ACK_0x1B6,
        SERVER_NEW_GUILD_MNGR_CHANGE_NAME_ACK_0x1B7,
        SERVER_NEW_GUILD_MNGR_INFO_ACK_0x1B8,
        SERVER_NEW_GUILD_MNGR_CHANGE_NOTICE_MSG_ACK_0x1B9,
        SERVER_NEW_GUILD_MNGR_CHANGE_INFO_MSG_ACK_0x1BA,
        SERVER_NEW_GUILD_MNGR_DELETE_ACK_0x1BB,
        SERVER_NEW_GUILD_MNGR_REQ_PAGE_ACK_0x1BC,
        SERVER_NEW_GUILD_MNGR_SEARCH_ACK_0x1BD,
        SERVER_NEW_GUILD_MNGR_PLAYER_PAST_ACTIVITIES_ACK_0x1BE,
        SERVER_NEW_GUILD_MNGR_PLAYER_CHANGE_STATUS_GUILD_0x1BF,
        SERVER_NEW_GUILD_MNGR_MEMBER_JOIN_ACK_0x1C0,
        SERVER_NEW_GUILD_MNGR_MEMBER_JOIN_CANCEL_ACK_0x1C1,
        SERVER_NEW_GUILD_MNGR_MEMBER_AGREE_ACK_0x1C2,
        SERVER_NEW_GUILD_MNGR_MEMBER_PROMOTE_ACK_0x1C4 = 0x1C4,
        SERVER_NEW_GUILD_MNGR_MEMBER_CHANGE_MSG_ACK_0x1C5,
        SERVER_NEW_GUILD_MNGR_MEMBER_INFO_ACK_0x1C6,
        SERVER_NEW_GUILD_MNGR_MEMBER_LEAVE_ACK_0x1C7,
        SERVER_NEW_GUILD_MNGR_MEMBER_KICK_ACK_0x1C8,
        SERVER_NEW_GUILD_MNGR_EMBLEM_CHANGE_ACK_0x1C9,
        SERVER_NEW_GUILD_MNGR_EMBLEM_CHANGE_CONFIRM_ACK_0x1CA,
        //SERVER_UNKNOWN_0x1CB_A_0x1D0,
        SERVER_NEW_GUILD_MNGR_NEW_OR_LEAVE_MEMBER_0x1D1,
        SERVER_SEND_BROADCAST_NOTICE_0x1D3 = 0x1D3,
        SERVER_REQ_CHANGE_GAME_SERVER_ACK_0x1D4,
        SERVER_UPDATE_LEVEL_AND_EXP_0x1D9 = 0x1D9,
        SERVER_REQ_POINT_SHOP_OPEN_ACK_0x1E7 = 0x1E7, // TIKI SHOP LEGACY
        SERVER_REQ_POINT_SHOP_POINT_ACK_0x1E8,
        SERVER_REQ_POINT_SHOP_EXCHANGE_TP_BY_ITEM_ACK_0x1E9,
        SERVER_REQ_POINT_SHOP_EXCHANGE_ITEM_BY_TP_ACK_0x1EA,
        SERVER_REQ_NEW_SCRATCH_OPEN_ACK_0x1EB,
        SERVER_CAMERA_INITIAL_GRANDZODIAC_ACK_0x1EC,
        //SERVER_UNKNOWN_0x1ED,
        SERVER_GRANDZODIAC_INITIAL_POSITION_0x1EE = 0x1EE,
        SERVER_GRANDZODIAC_HOLE_STAT_0x1EF,
        SERVER_GRANDZODIAC_GOLDEN_BEAM_START_0x1F0,
        SERVER_GRANDZODIAC_GOLDEN_BEAM_END_0x1F1,
        SERVER_GRANDZODIAC_END_GAME_0x1F2,
        SERVER_GRANDZODIAC_END_GAME_SCORE_0x1F3,
        SERVER_GRANDZODIAC_MOVE_NEXT_HOLE_0x1F4,
        SERVER_GRANDZODIAC_SCORE_SHOT_0x1F5,
        SERVER_NOTIFY_CLIENTE_FROM_SOMETHING_AFTER_ENTERING_IN_CHANNEL_0x1F6,
        SERVER_SHOT_END_LOCATION_DATA_ACK_0x1F7,
        SERVER_MARK_ON_COURSE_ACK_0x1F8,
        SERVER_GRANDZODIAC_INIT_HOLE_0x1F9,
        SERVER_GRANDZODIAC_UPDATE_TROPHY_0x1FA,

        SERVER_REQ_NEW_SCRATCH_PLAY_ACK_0x1FD = 0x1FD,
        //SERVER_UNKNOWN_0x1FE_A_0x1FF,
        SERVER_GRANDZODIAC_REMAIN_TIME_0x200 = 0x200,
        SERVER_GRANDZODIAC_FINISH_LOAD_COURSE_0x201,
        //SERVER_UNKNOWN_0x202,
        SERVER_ACTIVE_WING_EFFECT_0x203 = 0x203,
        //SERVER_UNKNOWN_0x204_A_0x20D,
        SERVER_REQ_ENTER_SHOP_ACK_0x20E = 0x20E,
        //SERVER_UNKNOWN_0x20F,
        SERVER_YOU_RECEIVED_NEW_MAIL_0x210 = 0x210,
        SERVER_REQ_NEW_MAILBOX_OPEN_MAILBOX_ACK_0x211,
        SERVER_REQ_NEW_MAILBOX_OPEN_MAIL_ACK_0x212,
        SERVER_REQ_NEW_MAILBOX_SEND_MAIL_ACK_0x213,
        SERVER_REQ_NEW_MAILBOX_MOVE_ITEM_TO_MYROOM_ACK_0x214,
        SERVER_REQ_NEW_MAILBOX_DELETE_MAIL_ACK_0x215,
        SERVER_UPDATE_ABSTRACT_ALL_ITEM_0x216,
        SERVER_REQ_NEW_BONGDARISHOP_PLAY_NORMAL_ACK_0x21B = 0x21B,
        //SERVER_UNKNOWN_0x21C,
        SERVER_ACHIEVEMENT_COUNTER_ITEM_INFO_0x21D = 0x21D,
        SERVER_ACHIEVEMENT_INFO_0x21E,
        //SERVER_UNKNOWN_0x21F,
        SERVER_ACHIEVEMENT_UPDATE_0x220 = 0x220,
        //SERVER_UNKNOWN_0x221_A_0x224,
        SERVER_REQ_DAILYQUEST_OPEN_ACK_0x225 = 0x225,
        SERVER_REQ_DAILYQUEST_AGREE_ACK_0x226,
        SERVER_REQ_DAILYQUEST_GET_REWARD_ACK_0x227,
        SERVER_REQ_DAILYQUEST_FORFEIT_ACK_0x228,
        SERVER_REQ_LOLO_CARD_COMPOSE_ACK_0x229,
        SERVER_LOLO_CARD_COMPOSE_WIN_CARD_0x22A,
        SERVER_ACTIVE_AUTO_COMMAND_ACK_0x22B,
        SERVER_REQ_ACHIEVEMENT_OPEN_ACK_0x22C,
        SERVER_ACHIEVEMENT_PLAYER_INFO_0x22D,
        SERVER_ACHIEVEMENT_CLEAR_QUEST_0x22E,
        SERVER_REQ_CADIE_MAGICBOX_EXCHANGE_ITEM_ACK_0x22F,
        SERVER_NEW_START_GAME_FLAG_0x230,
        SERVER_NEW_START_GAME_FLAG2_0x231,
        //SERVER_UNKNOWN_0x232_A_0x234,
        SERVER_EQUIP_ITEM_UPDATE_0x235 = 0x235,
        SERVER_ACTIVE_PAWS_EFFECT_ACK_0x236,
        SERVER_ACTIVE_RING_EFFECT_ACK_0x237,
        //SERVER_UNKNOWN_0x238_A_0x23C,
        SERVER_CLUBSETWORKSHOP_REQ_UP_LEVEL_ACK_0x23D = 0x23D,
        SERVER_CLUBSETWORKSHOP_CONFIRM_UP_LEVEL_ACK_0x23E,
        SERVER_CLUBSETWORKSHOP_CANCEL_UP_LEVEL_ACK_0x23F,
        SERVER_CLUBSETWORKSHOP_REQ_UP_RANK_ACK_0x240,
        SERVER_CLUBSETWORKSHOP_UP_RANK_TRANSFORM_DIALOG_0x241,
        SERVER_CLUBSETWORKSHOP_CONFIRM_UP_RANK_TRANSFORM_ACK_0x242,
        SERVER_CLUBSETWORKSHOP_CANCEL_UP_RANK_TRANSFORM_ACK_0x243,
        SERVER_NEW_END_GAME_FLAG_0x244, // PODE SER O UPDATE CLUBSETWORKSHOP MASTERY
        SERVER_CLUBSETWORKSHOP_REQ_TRANSFER_MASTERY_POINT_ACK_0x245,
        SERVER_CLUBSETWORKSHOP_REQ_RECOVERY_POINT_ACK_0x246,
        SERVER_CLUBSETWORKSHOP_REQ_RESET_CLUBSET_ACK_0x247,
        SERVER_ATTENDENCE_REWARD_OPEN_ACK_0x248,
        SERVER_ATTENDENCE_REWARD_CHECK_DAY_ACK_0x249,
        //SERVER_UNKNOWN_0x24A_A_0x24B,
        SERVER_ACTIVE_EARCUFF_EFFECT_ACK_0x24C = 0x24C,
        //SERVER_UNKNOWN_0x24D_A_0x24E,
        SERVER_NEW_END_GAME_FLAG2_0x24F = 0x24F,
        SERVER_GRANDPRIX_REQ_ENTER_LOBBY_ACK_0x250,
        SERVER_GRANDPRIX_REQ_LEAVE_LOBBY_ACK_0x251,
        //SERVER_UNKNOWN_0x252,
        SERVER_GRANDPRIX_START_GAME_FLAG_0x253 = 0x253,
        SERVER_GRANDPRIX_REQ_LEAVE_ROOM_ACK_0x254,
        SERVER_GRANDPRIX_MOVE_NEXT_HOLE_0x255,
        SERVER_GRANDPRIX_SEND_BOTS_0x256,
        SERVER_SEND_GRANDPRIX_SPECIAL_TROPHY_0x257,
        SERVER_GRANDPRIX_PODIUM_0x258,
        SERVER_GRANDPRIX_TIMEOUT_0x259,
        SERVER_GRANDPRIX_CLEAR_0x25A,
        //SERVER_UNKNOWN_0x25B,
        SERVER_GRANDPRIX_UPDATE_TROPHY_0x25C = 0x25C,
        SERVER_GRANDPRIX_INIT_SPECIAL_TROPHY_0x25D,
        //SERVER_UNKNOWN_0x25E_A_0x263,
        SERVER_MEMORIALSHOP_PLAY_ACK_0x264 = 0x264,
        SERVER_ACTIVE_GLOVE_EFFECT_ACK_0x265,
        SERVER_ACTIVE_RING_GROUND_EFFECT_ACK_0x266,
        //SERVER_UNKNOWN_0x267_A_0x269,
        SERVER_TOGGLE_ASSIST_ACK_0x26A = 0x26A,
        SERVER_ACTIVE_ASSIST_GREEN_ACK_0x26B,
        SERVER_MEMORIALSHOP_PLAY_BIG_ACK_0x26C,
        SERVER_PREMIUM_TIME_END_0x26D,
        SERVER_CHARACTER_STATS_EXPAND_MASTERY_ACK_0x26E,
        SERVER_CHARACTER_STATS_UPGRADE_STAT_ACK_0x26F,
        SERVER_CHARACTER_STATS_DOWNGRADE_STAT_ACK_0x270,
        SERVER_CHARACTER_STATS_EQUIP_CARD_ACK_0x271,
        SERVER_CHARACTER_STATS_EQUIP_CARD_WITH_CLUB_PATCHER_ACK_0x272,
        SERVER_CHARACTER_STATS_REMOVE_CARD_ACK_0x273,
        SERVER_NEW_TIKISHOP_EXCHANGE_ITEM_ACK_0x274,
        //SERVER_UNKNOWN_0x275,
        SERVER_DISCONNECT_PLAYER_0x276 = 0x276,
        //SERVER_UNKNOWN_0x277_A_0x27D,
        SERVER_ACTIVE_RING_PAWS_RAINBOW_EFFECT_ACK_0x27E = 0x27E,
        SERVER_ACTIVE_RING_POWER_GAUGE_EFFECT_ACK_0x27F,
        SERVER_ACTIVE_RING_MIRACLE_SIGN_EFFECT_ACK_0x280,
        SERVER_ACTIVE_RING_PAWS_RING_SET_EFFECT_ACK_0x281, 
    }

    /// <summary>
    /// Names of Packets Client
    /// </summary>
    public enum PacketIDClient
    {
        CLIENT_NONE_0x00,
        CLIENT_TIMETOALIVE_0x01,
        CLIENT_CONNECT_0x02,
        CLIENT_CHATMSG_0x03,
        CLIENT_ENTER_CHANNEL_0x04,
        CLIENT_LEAVE_CHANNEL_0x05,
        CLIENT_MY_STATISTICS_0x06,
        CLIENT_REQUEST_USERINFO_OFFLINE_0x07,
        CLIENT_REQUEST_CREATE_ROOM_0x08,
        CLIENT_REQUEST_JOIN_ROOM_0x09,
        CLIENT_REQUEST_ROOMINFO_CHANGED_0x0A,
        CLIENT_LOBBY_USERINFO_CHANGED_0x0B,
        CLIENT_REQUEST_USERINFO_CHANGED_0x0C,
        CLIENT_SET_PLAYER_READY_0x0D,
        CLIENT_REQUEST_START_GAME_0x0E,
        CLIENT_EXIT_ROOM_0x0F,
        CLIENT_REQUEST_CHANGE_TEAM_0x10,
        CLIENT_LOAD_OK_0x11,
        CLIENT_SHOT_0x12,
        CLIENT_CAMERA_0x13,
        CLIENT_CLICK_0x14,
        CLIENT_POWER_SHOT_0x15,
        CLIENT_CLUB_0x16,
        CLIENT_USE_ITEM_0x17,
        CLIENT_EMOTICON_0x18,
        CLIENT_DROP_0x19,
        CLIENT_HOLE_INFO_0x1A,
        CLIENT_SHOT_RESULT_0x1B,
        CLIENT_SHOT_ACK_0x1C,
        CLIENT_REQUEST_BUY_ITEM_0x1D,
        CLIENT_REQUEST_SELL_ITEM_0x1E,
        CLIENT_REQUEST_GIFT_ITEM_0x1F,
        CLIENT_REQUEST_EQUIP_ITEM_0x20,
        CLIENT_FIRST_LOGIN_0x21,
        CLIENT_TIMECHECK_0x22,
        CLIENT_REQUEST_GIFT_INFO_0x23,
        CLIENT_MOVE_GIFT_0x24,
        CLIENT_SKIP_0x25,
        CLIENT_REQUEST_BANISH_0x26,
        CLIENT_VOTE_FOR_BANISH_0x27,
        CLIENT_COMMIT_MASTER_0x28,
        CLIENT_INVITE_0x29,
        CLIENT_WHISPER_0x2A,
        CLIENT_REQUEST_USERLIST_0x2B,
        CLIENT_REQUEST_ROOMLIST_0x2C,
        CLIENT_REQUEST_DETAIL_ROOM_INFO_0x2D,
        CLIENT_I_FINISH_TUTORIAL_0x2E,
        CLIENT_REQUEST_USERINFO_0x2F,
        CLIENT_PAUSE_0x30,
        CLIENT_HOLE_STAT_0x31,
        CLIENT_SLEEP_0x32,
        CLIENT_REPORT_ERROR_0x33,
        CLIENT_TEESHOT_READY_0x34,
        CLIENT_TEAM_HOLEIN_PANG_0x35,
        CLIENT_ANSWER_GOSTOP_0x36,
        CLIENT_END_STROKE_GAME_0x37,
        CLIENT_CHANGE_NICK_0x38,
        CLIENT_REEMPLOY_CADDIE_0x39,
        CLIENT_REPORT_0x3A,
        CLIENT_CHANGE_SCHOOL_0x3B,
        CLIENT_MSN_REQUEST_0x3C,
        CLIENT_REQUEST_CASH_0x3D,
        CLIENT_JOIN_GALLERY_0x3E, // GALLERY
        CLIENT_CHANGE_TARGET_0x3F, // GALLERY
        CLIENT_PLAYINFO_0x40, // GALLERY
        CLIENT_REQUEST_IDENTITY_0x41,
        CLIENT_SHOT_COMMAND_0x42,
        CLIENT_REQUEST_SERVER_LIST_0x43,
        CLIENT_REQUEST_TITLE_LIST_0x44,
        CLIENT_CHANGE_TITLE_0x45,
        CLIENT_SET_JJANG_0x46,
        CLIENT_REQUEST_RANKADDRESS_0x47,
        CLIENT_LOADING_INFO_0x48,
        CLIENT_REPLAY_OFFLINE_0x49,
        CLIENT_REPLAY_ONLINE_0x4A,
        CLIENT_ENCHANT_0x4B,
        CLIENT_BANISH_ALL_0x4C,
        CLIENT_REQUEST_DRAWBACK_GIFT_0x4D,
        CLIENT_SET_SYSTEM_0x4E,
        CLIENT_CHAT_PENALITY_0x4F,
        CLIENT_FIND_USER_0x50,
        CLIENT_TITLE_0x51,
        CLIENT_MATCH_HOLEIN_PANG_0x52,
        CLIENT_UPDATE_EXP_0x53,
        CLIENT_TEAMCHAT_0x54,
        CLIENT_ALLOW_WHISPER_0x55,
        CLIENT_CHECK_PCBANG_0x56,
        CLIENT_NOTICE_0x57,
        CLIENT_RESTORE_0x58,
        CLIENT_OPEN_FORTUNE_0x59,
        CLIENT_OFFLINE_GAME_0x5A,
        CLIENT_REQUEST_GUILD_LIST_0x5B,
        CLIENT_REQUEST_SERVER_TIME_0x5C,
        CLIENT_SPY_ENTER_ROOM_0x5D,
        CLIENT_SPY_PLAY_INFO_0x5E,
        CLIENT_REQ_GUILD_MATCH_RECORD_0x5F,
        CLIENT_DESTROY_ROOM_0x60,
        CLIENT_REQUEST_KICK_0x61,
        CLIENT_REQUEST_GUILD_INFO_0x62,
        CLIENT_SYNC_ACTIVITY_0x63,
        CLIENT_DELETE_ITEM_0x64,
        CLIENT_SPEED_RATE_0x65,
        CLIENT_ONELINE_REQUEST_0x66, // ONELINE == TICKER
        CLIENT_ONELINE_QUERY_0x67,
        CLIENT_COMPOUND_0x68,
        CLIENT_UPDATE_GAME_OPTIONI_0x69, // MACRO
        CLIENT_REFLECTED_ITEM_0x6A,
        CLIENT_CHECK_CADDIE_WARNNING_0x6B,
        CLIENT_FLUSH_ITEM_0x6C,
        CLIENT_REQUEST_BONGDARISHOP_ITEM_0x6D,
        CLIENT_INSERT_COUPON_0x6E,
        CLIENT_APPLY_EVENT_0x6F,
        CLIENT_SCRATCH_ITEM_0x70,
        CLIENT_SCRATCH_SERIAL_NUMBER_0x71,
        CLIENT_SCRATCH_CARD_NUMBER_0x72,
        CLIENT_CHANGE_MASCOT_0x73,
        CLIENT_TRADE_OPEN_SHOP_0x74,
        CLIENT_TRADE_CLOSE_SHOP_0x75,
        CLIENT_TRADE_EDIT_SHOP_0x76,
        CLIENT_TRADE_ENTER_SHOP_0x77,
        CLIENT_TRADE_EXIT_SHOP_0x78,
        CLIENT_TRADE_EDIT_TITLE_0x79,
        CLIENT_TRADE_SHOW_VISITOR_0x7A,
        CLIENT_TRADE_INCOME_0x7B,
        CLIENT_TRADE_EDIT_ITEM_0x7C,
        CLIENT_TRADE_BUY_ITEM_0x7D,
        CLIENT_RECYCLE_ITEM_0x7E,
        CLIENT_GAME_END_EARLY_0x7F,
        CLIENT_USERINFO_SEASON2_0x80,
        CLIENT_ENTER_LOBBY_0x81,
        CLIENT_LEAVE_LOBBY_0x82,
        CLIENT_MOVE_LOBBY_TO_LOBBY_0x83,
        CLIENT_BLOCK_CHAT_0x84,
        CLIENT_REQUEST_PROFILE_0x85,
        CLIENT_LOCATE_USER_0x86,
        CLIENT_SECURITYKEY_CHECK_0x87,
        CLIENT_RESPONSE_GGCSAUTH_0x88,
        CLIENT_SHOWMETHEMONEY_0x89,
        CLIENT_BS_USABLE_TIMES_0x8A, // BONGDARISHOP BS == BONGDARI SHOP
        CLIENT_REQUEST_MESSENGER_SERVER_LIST_0x8B,
        CLIENT_POINT_EVENT_POINT_0x8C,
        CLIENT_POINT_EVENT_EXCHANGE_ITEM_0x8D,
        CLIENT_POINT_EVENT_REMAINED_ITEM_0x8E,
        CLIENT_GM_COMMAND_0x8F,
        CLIENT_CHECK_PARAN_KID_0x90,
        CLIENT_CHANGE_NICK_FREE_0x91,
        CLIENT_CHANGE_NIC_PARAN_0x92,
        CLIENT_OPEN_NEWYEAR_MONEY_0x93,
        CLIENT_OPEN_EVENT_GIFTBOX_0x94,
        CLIENT_GIFT_LIST_PAGE_0x95,
        CLIENT_NEW_GIFT_LIST_0x96,
        CLIENT_ALL_GIFT_LIST_0x97,
        CLIENT_REQ_PLAYTIME_UPDATE_0x98,
        CLIENT_ONLINE_MESSAGE_BLOCK_0x99,
        CLIENT_UPDATE_PCBANG_MASCOTMSG_0x9A,
        CLIENT_LOG_ROOMINFO_BY_CLIENT_0x9B,
        CLIENT_USER_MATCH_HISTORY_0x9C,
        CLIENT_INTRUSION_0x9D, // AFTER START GAME ENTER TOURNEY
        CLIENT_REQUEST_REFRESH_GACHA_TICKETS_0x9E,
        CLIENT_REQUEST_BUY_GACHA_TICKETS_0x9F,
        CLIENT_REQUEST_REFRESH_ITEMLIST_0xA0,
        CLIENT_UPDATE_INGAME_WEBPAGE_0xA1,
        CLIENT_REQUEST_PANG_INFO_0xA2,
        CLIENT_OPEN_FORTUNE_NEW_0xA3,
        CLIENT_REQUEST_PANGYA_QUIZ_LEVEL_0xA4,
        CLIENT_GUILD_REQ_WAITING_LIST_0xA5,
        CLIENT_GUILD_INVITE_MEMBER_0xA6,
        CLIENT_GUILD_ACCEPT_MEMBER_0xA7,
        CLIENT_GUILD_ACCEPT_INVITATION_0xA8,
        CLIENT_GUILD_REQ_INVITATION_LIST_0xA9,
        CLIENT_USE_TIKI_REPORT_0xAA,
        CLIENT_OPEN_TIKI_REPORT_0xAB,
        CLIENT_UPDATE_EXP_TIKI_0xAC,
        CLIENT_WORLD_TOUR_EVENT_REQ_GIFT_0xAD,
        CLIENT_COMPLETE_QUEST_0xAE, // NEW TUTORIAL, É O MESMO NOME NO ENUM SERVER TYPE
        CLIENT_COMPLETE_QUEST_FLAG_0xAF,
        CLIENT_SELECT_QUEST_GIFT_0xB0,
        CLIENT_OPEN_NEWYEARPOPPER_0xB1,
        CLIENT_OPEN_LUCKY_POUCH_0xB2,
        CLIENT_NOTICE_TO_SERVER_0xB3,
        CLIENT_DIRECTJOIN_ROOM_0xB4,
        CLIENT_REQUEST_CREATE_REALMYRROM_0xB5,
        CLIENT_RMR_OBJECTSAVE_0xB6,
        CLIENT_RMR_OBJECTLOAD_0xB7,
        CLIENT_RMR_AUTHORITY_0xB8,
        CLIENT_UCC_REQUEST_0xB9,
        CLIENT_CHECK_INVITE_0xBA,
        CLIENT_CANCEL_INVITE_0xBB,
        CLIENT_REQUEST_TGAUGE_0xBC,
        CLIENT_USE_CARD_REQUEST_0xBD,
        CLIENT_REQUEST_UPDATE_USER_INSTREST_0xBE,
        CLIENT_REQUEST_MATCHING_0xBF,
        CLIENT_REQUEST_MATCHING_CANCEL_0xC0,
        CLIENT_REQUEST_UPDATE_USER_PLACE_0xC1,
        CLIENT_GAME_GHOST_0xC2,
        CLIENT_REQUEST_MAILBOX_SEND_MAIL_0xC3,
        CLIENT_REQUEST_MAILBOX_GET_MAILBOX_0xC4,
        CLIENT_REQUEST_MAILBOX_GET_MAIL_0xC5,
        CLIENT_REQUEST_MAILBOX_DELETE_MAIL_0xC6,
        CLIENT_REQUEST_MAILBOX_MOVE_HAVE_ITEM_0xC7,
        CLIENT_PARTS_ATTACH_CARD_0xC8,
        CLIENT_SECURITY_KEY_0xC9,
        CLIENT_OPEN_CARDPACK_0xCA,
        CLIENT_CAN_APPROACH_READY_0xCB, 
        CLIENT_ITEMSTORAGE_REQ_ACCESS_0xCC,
        CLIENT_ITEMSTORAGE_REQ_PAGE_0xCD,
        CLIENT_ITEMSTORAGE_REQ_PUSH_0xCE,
        CLIENT_ITEMSTORAGE_REQ_POP_0xCF,
        CLIENT_ITEMSTORAGE_SET_PASSWORD_0xD0,
        CLIENT_ITEMSTORAGE_REQ_CHANGE_PW_0xD1,
        CLIENT_ITEMSTORAGE_SET_LOCK_0xD2,
        CLIENT_ITEMSTORAGE_REQ_STATE_0xD3,
        CLIENT_ITEMSTORAGE_REQ_PANG_INOUT_0xD4,
        CLIENT_ITEMSTORAGE_REQ_PANG_0xD5,
        CLIENT_ITEMSTORAGE_REQ_EDIT_0xD6,
        CLIENT_FURNITURE_ABILITY_USE_0xD7,
        CLIENT_ITEM_BUFF_0xD8,
        CLIENT_PRIVATE_TRADE_0xD9,
        CLIENT_VALENTINE_OPEN_REQ_0xDA,
        CLIENT_VALENTIME_GIFT_REQ_0xDB,
        CLIENT_TIKI_MAGICBOX_REQ_VERSION_0xDC,
        CLIENT_TIKI_MAGICBOX_REQ_OUTPUT_0xDD,
        CLIENT_WHISPEROFF_TASK_0xDE,
        CLIENT_TICKET_REQUEST_0xDF,
        CLIENT_USE_CARD_REMOVE_REQ_0xE0,
        CLIENT_REQ_MISSION_EVENT_OPEN_0xE1,
        CLIENT_REQ_MISSION_EVENT_GIFT_0xE2,
        CLIENT_REQ_BINGO_EVENT_INFO_0xE3,
        CLIENT_NEW_MAILIST_0xE4,
        CLIENT_REQ_CUTIN_0xE5,
        CLIENT_REQ_EXTEND_RENTAL_0xE6,
        CLIENT_REQ_DELETE_RENTAL_0xE7,
        CLIENT_REQUEST_UPGRADE_CADDIE_0xEA = 0xEA,
        CLIENT_REQ_CHARACTER_STAT_IN_CHATROOM_0xEB,
        CLIENT_SUPPLY_PACK_OPEN_0xEC, // COMET REFIL
        CLIENT_KOOH_BIRTHDAY_BINGO_0xED,
        CLIENT_KOOH_BIRTHDAY_GIFT_0xEE,
        CLIENT_REQ_USE_NEW_RANDOMBOX_0xEF,
        CLIENT_HEARTBEAT_0xF4 = 0xF4,
        CLIENT_WEB_AUTH_KEY_0xFB = 0xFB,    // APARTIR DAQUI SÃO MEUS PRÓPRIOS NOMES PARAS OS TIPOS DE PACOTES
        CLIENT_REQ_ALL_UCC_FROM_ALL_PLAYER_0xFE = 0xFE,
        CLIENT_NEW_GUILD_MNGR_CREATE_0x101 = 0x101,
        CLIENT_NEW_GUILD_MNGR_CHECK_NAME_0x102,
        CLIENT_NEW_GUILD_MNGR_CHANGE_NAME_0x103,
        CLIENT_NEW_GUILD_MNGR_INFO_0x104,
        CLIENT_NEW_GUILD_MNGR_CHANGE_NOTICE_MSG_0x105,
        CLIENT_NEW_GUILD_MNGR_CHANGE_INFO_MSG_0x106,
        CLIENT_NEW_GUILD_MNGR_DELETE_0x107,
        CLIENT_NEW_GUILD_MNGR_REQ_PAGE_0x108,
        CLIENT_NEW_GUILD_MNGR_SEARCH_0x109,
        CLIENT_NEW_GUILD_MNGR_PLAYER_PAST_ACTIVITIES_0x10A,
        CLIENT_NEW_GUILD_MNGR_MEMBER_JOIN_0x10C = 0x10C,
        CLIENT_NEW_GUILD_MNGR_MEMBER_JOIN_CANCEL_0x10D,
        CLIENT_NEW_GUILD_MNGR_MEMBER_AGREE_0x10E,
        CLIENT_NEW_GUILD_MNGR_MEMBER_PROMOTE_0x110 = 0x110,
        CLIENT_NEW_GUILD_MNGR_MEMBER_CHANGE_MSG_0x111,
        CLIENT_NEW_GUILD_MNGR_MEMBER_INFO_0x112,
        CLIENT_NEW_GUILD_MNGR_MEMBER_LEAVE_0x113,
        CLIENT_NEW_GUILD_MNGR_MEMBER_KICK_0x114,
        CLIENT_NEW_GUILD_MNGR_EMBLEM_CHANGE_0x115,
        CLIENT_NEW_GUILD_MNGR_EMBLEM_CHANGE_CONFIRM_0x116,
        CLIENT_REQ_CHANGE_GAME_SERVER_0x119 = 0x119,
        CLIENT_REQ_POINT_SHOP_OPEN_0x126 = 0x126, // TIKI SHOP LEGACY
        CLIENT_REQ_POINT_SHOP_POINT_0x127,
        CLIENT_REQ_POINT_SHOP_EXCHANGE_TP_BY_ITEM_0x128,
        CLIENT_REQ_POINT_SHOP_EXCHANGE_ITEM_BY_TP_0x129,
        CLIENT_REQ_NEW_SCRATCH_OPEN_0x12A,
        CLIENT_REQ_NEW_SCRATCH_PLAY_0x12B,
        CLIENT_CAN_GRANDZODIAC_READY_0x12C,
        CLIENT_CAMERA_INITIAL_GRANDZODIAC_0x12D,
        CLIENT_MARK_ON_COURSE_0x12E,
        CLIENT_SHOT_END_LOCATION_DATA_0x12F,
        CLIENT_LEAVE_PRACTICE_0x130,
        CLIENT_LEAVE_CHIP_IN_PRACTICE_0x131,
        CLIENT_START_FIRST_HOLE_GRANDZODIAC_0x137 = 0x137,
        CLIENT_ACTIVE_WING_EFFECT_0x138,
        CLIENT_REQ_ENTER_SHOP_0x140 = 0x140,
        CLIENT_REQ_CHANGE_WIND_NEXT_HOLE_REPEAT_0x141,
        CLIENT_REQ_NEW_MAILBOX_OPEN_MAILBOX_0x143 = 0x143,
        CLIENT_REQ_NEW_MAILBOX_OPEN_MAIL_0x144,
        CLIENT_REQ_NEW_MAILBOX_SEND_MAIL_0x145,
        CLIENT_REQ_NEW_MAILBOX_MOVE_ITEM_TO_MYROOM_0x146,
        CLIENT_REQ_NEW_MAILBOX_DELETE_MAIL_0x147,
        CLIENT_REQ_NEW_BONGDARISHOP_PLAY_NORMAL_0x14B = 0x14B,
        CLIENT_REQ_DAILYQUEST_OPEN_0x151 = 0x151,
        CLIENT_REQ_DAILYQUEST_AGREE_0x152,
        CLIENT_REQ_DAILYQUEST_GET_REWARD_0x153,
        CLIENT_REQ_DAILYQUEST_FORFEIT_0x154,
        CLIENT_REQ_LOLO_CARD_COMPOSE_0x155,
        CLIENT_ACTIVE_AUTO_COMMAND_0x156,
        CLIENT_REQ_ACHIEVEMENT_OPEN_0x157,
        CLIENT_REQ_CADIE_MAGICBOX_EXCHANGE_ITEM_0x158,
        CLIENT_ACTIVE_PAWS_EFFECT_0x15C = 0x15C,
        CLIENT_ACTIVE_RING_EFFECT_0x15D,
        CLIENT_CLUBSETWORKSHOP_REQ_UP_LEVEL_0x164 = 0x164,
        CLIENT_CLUBSETWORKSHOP_CONFIRM_UP_LEVEL_0x165,
        CLIENT_CLUBSETWORKSHOP_CANCEL_UP_LEVEL_0x166,
        CLIENT_CLUBSETWORKSHOP_REQ_UP_RANK_0x167,
        CLIENT_CLUBSETWORKSHOP_CONFIRM_UP_RANK_TRANSFORM_0x168,
        CLIENT_CLUBSETWORKSHOP_CANCEL_UP_RANK_TRANSFORM_0x169,
        CLIENT_CLUBSETWORKSHOP_REQ_RECOVERY_POINT_0x16B = 0x16B,
        CLIENT_CLUBSETWORKSHOP_REQ_TRANSFER_MASTERY_POINT_0x16C,
        CLIENT_CLUBSETWORKSHOP_REQ_RESET_CLUBSET_0x16D,
        CLIENT_ATTENDENCE_REWARD_OPEN_0x16E,
        CLIENT_ATTENDENCE_REWARD_CHECK_DAY_0x16F,
        CLIENT_ACTIVE_EARCUFF_EFFECT_0x171 = 0x171,
        CLIENT_WORKSHOP_EVENT_2013_OPEN_0x172,  
        CLIENT_GRANDPRIX_REQ_ENTER_LOBBY_0x176 = 0x176,
        CLIENT_GRANDPRIX_REQ_LEAVE_LOBBY_0x177,
        CLIENT_GRANDPRIX_REQ_ENTER_ROOM_0x179 = 0x179,
        CLIENT_GRANDPRIX_REQ_LEAVE_ROOM_0x17A,
        CLIENT_MEMORIALSHOP_PLAY_0x17F = 0x17F,
        CLIENT_ACTIVE_GLOVE_EFFECT_0x180,
        CLIENT_ACTIVE_RING_GROUND_EFFECT_0x181,
        CLIENT_TOGGLE_ASSIST_0x184 = 0x184,
        CLIENT_ACTIVE_ASSIST_GREEN_0x185,
        CLIENT_MEMORIALSHOP_PLAY_BIG_0x186,
        CLIENT_CHARACTER_STATS_EXPAND_MASTERY_0x187,
        CLIENT_CHARACTER_STATS_UPGRADE_STAT_0x188,
        CLIENT_CHARACTER_STATS_DOWNGRADE_STAT_0x189,
        CLIENT_CHARACTER_STATS_EQUIP_CARD_0x18A,
        CLIENT_CHARACTER_STATS_EQUIP_CARD_WITH_CLUB_PATCHER_0x18B,
        CLIENT_CHARACTER_STATS_REMOVE_CARD_0x18C,
        CLIENT_NEW_TIKISHOP_EXCHANGE_ITEM_0x18D,
        CLIENT_ARIN_EVENT_2014_OPEN_0x192 = 0x192,
        CLIENT_ACTIVE_RING_PAWS_RAINBOW_EFFECT_0x196 = 0x196,
        CLIENT_ACTIVE_RING_POWER_GAUGE_EFFECT_0x197,
        CLIENT_ACTIVE_RING_MIRACLE_SIGN_EFFECT_0x198,
        CLIENT_ACTIVE_RING_PAWS_RING_SET_EFFECT_0x199,
    }
}
