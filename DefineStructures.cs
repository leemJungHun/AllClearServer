using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.InteropServices;

namespace DefineServerUtility
{
    [StructLayout(LayoutKind.Sequential)]
    public struct Packet
    {
        // 프로토콜 넘버이다.
        [MarshalAs(UnmanagedType.U4)] public int _protocolID;
        // _datas에 들어가는 구조체의 실질 메모리 크기
        [MarshalAs(UnmanagedType.U2)] public short _totalSize;
        // 신호를 받을 주체
        [MarshalAs(UnmanagedType.U8)] public long _targetID;
        // 실제 정보 구조체의 바이트 배열
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 1002)] public byte[] _datas;
    }


    #region[Send Struct]
    [StructLayout(LayoutKind.Sequential)]
    public struct Send_GivingUUID
    {
        [MarshalAs(UnmanagedType.U8)] public long _UUID;

    }

    [StructLayout(LayoutKind.Sequential)]
    public struct Send_SessionID
    {
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 16)] public string _sessionID;

    }
    [StructLayout(LayoutKind.Sequential)]
    public struct Send_JoinRoom
    {
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 16)] public string _sessionID;

    }
    [StructLayout(LayoutKind.Sequential)]
    public struct Send_ExitRoom
    {
        [MarshalAs(UnmanagedType.U8)] public long _UUID;
        [MarshalAs(UnmanagedType.U4)] public int _index;
    }
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
    public struct Send_JoinResult
    {
        [MarshalAs(UnmanagedType.Bool)] public bool _join;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 32)] public string _sessionID;
    }
    [StructLayout(LayoutKind.Sequential)]
    public struct Send_PlayerPosition
    {
        [MarshalAs(UnmanagedType.U8)] public long _UUID;
        [MarshalAs(UnmanagedType.R4)] public float _x;
        [MarshalAs(UnmanagedType.R4)] public float _z;
        [MarshalAs(UnmanagedType.Bool)] public bool _init;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 16)] public string _sessionID;
    }
    [StructLayout(LayoutKind.Sequential)]
    public struct Send_PlayerMove
    {
        [MarshalAs(UnmanagedType.U8)] public long _UUID;
        [MarshalAs(UnmanagedType.R4)] public float _rx;
        [MarshalAs(UnmanagedType.R4)] public float _mz;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 16)] public string _sessionID;
    }
    [StructLayout(LayoutKind.Sequential)]
    public struct Send_PlayerIndex
    {
        [MarshalAs(UnmanagedType.U8)] public long _UUID;
        [MarshalAs(UnmanagedType.U4)] public int _index;
    }
    [StructLayout(LayoutKind.Sequential)]
    public struct Send_GameReady
    {
        [MarshalAs(UnmanagedType.U4)] public int _index;
        [MarshalAs(UnmanagedType.Bool)] public bool _ready;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 16)] public string _sessionID;
    }
    [StructLayout(LayoutKind.Sequential)]
    public struct Send_GameStart
    {
        [MarshalAs(UnmanagedType.Bool)] public bool _start;
    }
    [StructLayout(LayoutKind.Sequential)]
    public struct Send_PlayerInfo
    {
        [MarshalAs(UnmanagedType.U8)] public long _UUID;
        [MarshalAs(UnmanagedType.U4)] public int _index;
    }
    [StructLayout(LayoutKind.Sequential)]
    public struct Send_PlayerSpace
    {
        [MarshalAs(UnmanagedType.U8)] public long _UUID;

    }
    [StructLayout(LayoutKind.Sequential)]
    public struct Send_PlayerZ
    {
        [MarshalAs(UnmanagedType.U8)] public long _UUID;

    }
    #endregion[Send Struct]

    #region[Receive Struct]
    [StructLayout(LayoutKind.Sequential)]
    public struct Receive_GivingUUID
    {
        [MarshalAs(UnmanagedType.U8)] public long _UUID;

    }
    [StructLayout(LayoutKind.Sequential)]
    public struct Receive_SessionID
    {
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 16)] public string _sessionID;

    }
    [StructLayout(LayoutKind.Sequential)]
    public struct Receive_JoinRoom
    {
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 16)] public string _sessionID;

    }
    [StructLayout(LayoutKind.Sequential)]
    public struct Receive_ExitRoom
    {
        [MarshalAs(UnmanagedType.U8)] public long _UUID;
        [MarshalAs(UnmanagedType.U4)] public int _index;
    }
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
    public struct Receive_JoinResult
    {
        [MarshalAs(UnmanagedType.Bool)] public bool _join;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 32)] public string _sessionID;
    }
    [StructLayout(LayoutKind.Sequential)]
    public struct Receive_PlayerPosition
    {
        [MarshalAs(UnmanagedType.U8)] public long _UUID;
        [MarshalAs(UnmanagedType.R4)] public float _x;
        [MarshalAs(UnmanagedType.R4)] public float _z;
        [MarshalAs(UnmanagedType.Bool)] public bool _init;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 16)] public string _sessionID;
    }

    public struct Receive_PlayerMove
    {
        [MarshalAs(UnmanagedType.U8)] public long _UUID;
        [MarshalAs(UnmanagedType.R4)] public float _rx;
        [MarshalAs(UnmanagedType.R4)] public float _mz;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 16)] public string _sessionID;
    }
    [StructLayout(LayoutKind.Sequential)]
    public struct Receive_PlayerIndex
    {
        [MarshalAs(UnmanagedType.U8)] public long _UUID;
        [MarshalAs(UnmanagedType.U4)] public int _index;
    }
    [StructLayout(LayoutKind.Sequential)]
    public struct Receive_GameReady
    {
        [MarshalAs(UnmanagedType.U4)] public int _index;
        [MarshalAs(UnmanagedType.Bool)] public bool _ready;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 16)] public string _sessionID;
    }
    [StructLayout(LayoutKind.Sequential)]
    public struct Receive_PlayerInfo
    {
        [MarshalAs(UnmanagedType.U8)] public long _UUID;
        [MarshalAs(UnmanagedType.U4)] public int _index;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct Receive_PlayerSpace
    {
        [MarshalAs(UnmanagedType.U8)] public long _UUID;

    }
    [StructLayout(LayoutKind.Sequential)]
    public struct Receive_PlayerZ
    {
        [MarshalAs(UnmanagedType.U8)] public long _UUID;

    }
    #endregion[Receive Struct]
}
