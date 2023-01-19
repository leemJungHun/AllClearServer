using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace DefineServerUtility
{
    public enum eSendMessage
    {
        Connect_GivingUUID = 0,
        Room_Create,
        Room_Join,
        Room_JoinResult,
        Room_Enter,
        Room_CreateSuccess,
        Room_Exit,
        Player_Position,
        Player_Move,
        Player_Index,
        Player_PositionUpdate,
        Player_Space,
        Player_Z,
        Game_Ready,
        Game_Start,
        Response_PlayerInfo,
        Request_PlayerInfo
    }
    public enum eReceiveMessage
    {
        GivingUUID = 0,
        Room_Create,
        Room_Join,
        Room_JoinResult,
        Room_Enter,
        Room_CreateSuccess,
        Room_Exit,
        Player_Position,
        Player_Move,
        Player_Index,
        Player_PositionUpdate,
        Player_Space,
        Player_Z,
        Game_Ready,
        Game_Start,
        Response_PlayerInfo,
        Request_PlayerInfo
    }


}

