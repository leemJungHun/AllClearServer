using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Runtime.InteropServices;
using DefineServerUtility;
using System.Numerics;


namespace AllClearGameServer
{
    public struct stSocket 
    {
        public long _uuid;
        public string _sessionID;
        public Socket _client;
        public Vector3 _position;

        public stSocket(long uuid, Socket socket)
        {
            _uuid = uuid;
            _sessionID = string.Empty;
            _client = socket;
            _position = new Vector3();
        }
    }

    public struct stRoom
    {
        public List<long> _userUUIDList;
        public List<bool> _redayList;
        public bool _isReady;
        public stRoom(bool isReady)
        {
            _userUUIDList = new List<long>();
            _redayList = new List<bool>();
            _isReady = false;
        }
    }

    class TCPGServer
    {
        short _port;
        long _nowUUID = 1000000000000;
        bool _isEnd = false;

        Socket _waitServer;
        Thread _sendThread;
        Thread _receiveThread;
        Random _ran = new Random();
        Queue<Packet> _sendQ = new Queue<Packet>();
        Queue<Packet> _receiveQ = new Queue<Packet>();
        Dictionary<long, stSocket> _clients = new Dictionary<long, stSocket>();
        Dictionary<string, stRoom> _rooms = new Dictionary<string, stRoom>();
        public TCPGServer(short port)
        {
            _port = port;
            try
            {
                _waitServer = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                _waitServer.Bind(new IPEndPoint(IPAddress.Any, _port));
                _waitServer.Listen(ConvertPacketFunc._maxPerson);

                Console.WriteLine("소켓 생성 성공!!");
            }
            catch(Exception ex)
            {
                Console.WriteLine("소켓 생성 실패!!");
                Console.WriteLine(ex.Message);
            }

            _isEnd = false;
            _sendThread = new Thread(() => SendProcess());
            _receiveThread = new Thread(() => ReceiveProcess());

            _sendThread.Start();
            _receiveThread.Start();
        }

        ~TCPGServer()
        {
            ReleaseServer();
        }

        public void ReleaseServer()
        {

        }

        public bool MainProcess()
        {
            if(_waitServer.Poll(0, SelectMode.SelectRead))
            {
                stSocket add = new stSocket(++_nowUUID, _waitServer.Accept());
                _clients.Add(add._uuid, add);

                new Thread(() => ReceiveProcess(add._client,add._uuid)).Start();
                //add에게 아이디를 알려주도록 한다.
                Send_GivingUUID subPack;
                subPack._UUID = add._uuid;
                Packet send = ConvertPacketFunc.CreatePack((int)eSendMessage.Connect_GivingUUID, add._uuid, Marshal.SizeOf(subPack), ConvertPacketFunc.StructureToByteArray(subPack));
                _sendQ.Enqueue(send);
            }

            if (Console.KeyAvailable)
            {
                ConsoleKeyInfo keys = Console.ReadKey(true);
                if (keys.Key == ConsoleKey.Escape)
                    return false;
            }

            return true;
        }

        void SendProcess()
        {
            while (!_isEnd)
            {
                if(_sendQ.Count > 0)
                {
                    Packet pack = _sendQ.Dequeue();
                    byte[] buffer = ConvertPacketFunc.StructureToByteArray(pack);
                    if (_clients.ContainsKey(pack._targetID))
                        _clients[pack._targetID]._client.Send(buffer);
                    else
                    {
                        //에러 표시
                    }
                }
            }
        }

        void ReceiveProcess()
        {
            while (!_isEnd)
            {
                if (_receiveQ.Count > 0)
                {
                    Packet recv = _receiveQ.Dequeue();
                    switch ((eReceiveMessage)recv._protocolID)
                    {
                        case eReceiveMessage.Room_Create:
                            // 생성 결과 send
                            //SelectAccountProc(recv);
                            Receive_CreateRoom(recv);
                            break;
                        case eReceiveMessage.Player_Position:
                            Receive_PlayerPosition(recv);
                            break;
                        case eReceiveMessage.Room_Join:
                            Receive_JoinRoom(recv);
                            break;
                        case eReceiveMessage.Player_Move:
                            Receive_PlayerMove(recv);
                            break;
                        case eReceiveMessage.Game_Ready:
                            Receive_GameReady(recv);
                            break;
                        case eReceiveMessage.Room_Exit:
                            Send_ExitRoom(recv._targetID, _clients[recv._targetID]._sessionID);
                            break;
                        case eReceiveMessage.Request_PlayerInfo:
                            Send_PlayerInfo(_clients[recv._targetID]._sessionID, recv._targetID);
                            break;
                        case eReceiveMessage.Player_Space:
                            Send_PlayerSpace(_clients[recv._targetID]._sessionID, recv._targetID);
                            break;
                        case eReceiveMessage.Player_Z:
                            Send_PlayerZ(_clients[recv._targetID]._sessionID, recv._targetID);
                            break;
                    }
                }
            }
                
        }

        void ReceiveProcess(Socket _client, long uuid)
        {
            while (!_isEnd)
            {
                try
                {
                    if (_client.Poll(1000, SelectMode.SelectRead) && _client.Available == 0)
                    {
                        try
                        {
                            Send_ExitRoom(uuid, _clients[uuid]._sessionID);
                            _clients.Remove(uuid);
                            Console.WriteLine("클라이언트 종료");
                            break;
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine(ex.Message);
                            break;
                        }
                    }

                    if (_client.Poll(0, SelectMode.SelectRead))
                    {
                        byte[] buffer = new byte[ConvertPacketFunc._maxByte];
                        try
                        {
                            int receiveLength = _client.Receive(buffer);
                            if (receiveLength != 0)
                            {
                                Packet pack = (Packet)ConvertPacketFunc.ByteArrayToStructure(buffer, typeof(Packet), receiveLength);
                                _receiveQ.Enqueue(pack);
                            }
                            else
                            {
                                continue;
                            }
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine(ex.Message);
                        }
                    }
                }
                catch(Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }
                
                Thread.Sleep(1);
            }
        }



        #region [SendProcessingFunc]
        void Send_SessionID(string sessionID, long targetID)
        {
            Send_SessionID sendSessionID;
            sendSessionID._sessionID = sessionID;
            Packet send;
            send._protocolID = (int)eSendMessage.Room_CreateSuccess;
            send._targetID = targetID;
            byte[] data = ConvertPacketFunc.StructureToByteArray(sendSessionID);
            send = ConvertPacketFunc.CreatePack(send._protocolID, send._targetID, Marshal.SizeOf(sendSessionID), data);
            _sendQ.Enqueue(send);
        }

        void Send_JoinResult(bool join,string sessionID, long targetID)
        {
            if (join)
            {
                Packet posUpdate;
                posUpdate._protocolID = (int)eSendMessage.Player_PositionUpdate;

                foreach (long uuid in _rooms[sessionID]._userUUIDList)
                {
                    if (targetID != uuid)
                    {
                        posUpdate._targetID = uuid;
                        posUpdate = ConvertPacketFunc.CreatePack(posUpdate._protocolID, posUpdate._targetID, 0, null);

                        _sendQ.Enqueue(posUpdate);
                    }
                }
            }

            Send_JoinResult joinResult;
            joinResult._join = join;
            joinResult._sessionID = sessionID;
            Packet send;
            send._protocolID = (int)eSendMessage.Room_JoinResult;
            send._targetID = targetID;
            byte[] data = ConvertPacketFunc.StructureToByteArray(joinResult);
            send = ConvertPacketFunc.CreatePack(send._protocolID, send._targetID, Marshal.SizeOf(joinResult), data);
            _sendQ.Enqueue(send);
        }

        void Send_ExitRoom(long uuid, string sessionID)
        {
            //방 나감 처리
            int index = _rooms[_clients[uuid]._sessionID]._userUUIDList.IndexOf(uuid);
            _rooms[_clients[uuid]._sessionID]._redayList.RemoveAt(index);
            _rooms[_clients[uuid]._sessionID]._userUUIDList.RemoveAt(index);

            Send_ExitRoom exitRoom;
            exitRoom._UUID = uuid;
            exitRoom._index = index;
            Packet send;
            byte[] data = ConvertPacketFunc.StructureToByteArray(exitRoom);
            send._protocolID = (int)eSendMessage.Room_Exit;
            
            foreach (long targetID in _rooms[sessionID]._userUUIDList)
            {
                send._targetID = targetID;
                send = ConvertPacketFunc.CreatePack(send._protocolID, send._targetID, Marshal.SizeOf(exitRoom), data);

                _sendQ.Enqueue(send);
                if (index == 0)
                {
                    stSocket socket = _clients[targetID];
                    socket._sessionID = string.Empty;
                    _clients[targetID] = socket;
                }
            }
            if (index == 0)
            {
                _rooms.Remove(sessionID);
            }

        }
        public void Send_Position(float x, float z, long target, long uuid, string sessionID)
        {
            Packet send;
            Send_PlayerPosition position;
            position._sessionID = sessionID;
            position._UUID = uuid;
            position._x = x;
            position._z = z;
            position._init = true;
            byte[] data = ConvertPacketFunc.StructureToByteArray(position);
            send._protocolID = (int)eSendMessage.Player_Position;
            send._targetID = target;
            send = ConvertPacketFunc.CreatePack(send._protocolID, send._targetID, Marshal.SizeOf(position), data);
            _sendQ.Enqueue(send);
        }

        public void Send_Index(long target, long uuid, string sessionID)
        {
            Packet send;
            Send_PlayerIndex index;
            index._UUID = uuid;
            index._index = _rooms[sessionID]._userUUIDList.IndexOf(uuid);
            byte[] data = ConvertPacketFunc.StructureToByteArray(index);
            send._protocolID = (int)eSendMessage.Player_Index;
            send._targetID = target;
            send = ConvertPacketFunc.CreatePack(send._protocolID, send._targetID, Marshal.SizeOf(index), data);
            _sendQ.Enqueue(send);
        }

        public void Send_Move(float rx, float mz, long target, long uuid, string sessionID)
        {
            Packet send;
            Send_PlayerMove move;
            move._sessionID = sessionID;
            move._UUID = uuid;
            move._rx = rx;
            move._mz = mz;
            byte[] data = ConvertPacketFunc.StructureToByteArray(move);
            send._protocolID = (int)eSendMessage.Player_Move;
            send._targetID = target;
            send = ConvertPacketFunc.CreatePack(send._protocolID, send._targetID, Marshal.SizeOf(move), data);
            _sendQ.Enqueue(send);
        }
        public void Send_GameStart(string sessionID)
        {
            Packet send;
            Send_GameStart gameStart;
            gameStart._start = _rooms[sessionID]._isReady;
            byte[] data = ConvertPacketFunc.StructureToByteArray(gameStart);
            send._protocolID = (int)eSendMessage.Game_Start;
            foreach(long uuid in _rooms[sessionID]._userUUIDList)
            {
                send._targetID = uuid;
                send = ConvertPacketFunc.CreatePack(send._protocolID, send._targetID, Marshal.SizeOf(gameStart), data);
                _sendQ.Enqueue(send);
            }
        }

        void Send_PlayerInfo(string sessionID, long targetID)
        {
            Packet send;
            Send_PlayerInfo playerInfo;
            foreach (long uuid in _rooms[sessionID]._userUUIDList)
            {
                playerInfo._index = _rooms[sessionID]._userUUIDList.IndexOf(uuid);
                playerInfo._UUID = uuid;
                byte[] data = ConvertPacketFunc.StructureToByteArray(playerInfo);
                send._protocolID = (int)eSendMessage.Response_PlayerInfo;
                send._targetID = targetID;
                send = ConvertPacketFunc.CreatePack(send._protocolID, send._targetID, Marshal.SizeOf(playerInfo), data);
                _sendQ.Enqueue(send);
            }
        }

        void Send_PlayerSpace(string sessionID, long uuid)
        {
            Packet send;
            Send_PlayerSpace playerSpace;
            foreach (long targetID in _rooms[sessionID]._userUUIDList)
            {
                if (targetID != uuid)
                {
                    playerSpace._UUID = uuid;
                    byte[] data = ConvertPacketFunc.StructureToByteArray(playerSpace);
                    send._protocolID = (int)eSendMessage.Player_Space;
                    send._targetID = targetID;
                    send = ConvertPacketFunc.CreatePack(send._protocolID, send._targetID, Marshal.SizeOf(playerSpace), data);
                    _sendQ.Enqueue(send);

                }
            }
        }

        void Send_PlayerZ(string sessionID, long uuid)
        {
            Packet send;
            Send_PlayerZ playerZ;
            foreach (long targetID in _rooms[sessionID]._userUUIDList)
            {
                if (targetID != uuid)
                {
                    playerZ._UUID = uuid;
                    byte[] data = ConvertPacketFunc.StructureToByteArray(playerZ);
                    send._protocolID = (int)eSendMessage.Player_Z;
                    send._targetID = targetID;
                    send = ConvertPacketFunc.CreatePack(send._protocolID, send._targetID, Marshal.SizeOf(playerZ), data);
                    _sendQ.Enqueue(send);

                }
            }
        }

        #endregion [SendProcessingFunc]
        #region [RecvProcessingFunc]
        void Receive_CreateRoom(Packet recv)
        {
            Console.WriteLine("{0}이 방 생성 요청", recv._targetID);

            string sessionID = string.Empty;
            while (true)
            {
                sessionID = SessionCreate();
                if (!_rooms.ContainsKey(sessionID))
                {
                    break;
                }
            }
            stRoom room = new stRoom(false);
            room._userUUIDList.Add(recv._targetID);
            room._redayList.Add(false);
            _rooms.Add(sessionID, room);
            stSocket socket = _clients[recv._targetID];
            socket._sessionID = sessionID;
            _clients[recv._targetID] = socket;
            Send_SessionID(sessionID,recv._targetID);
        }
        void Receive_JoinRoom(Packet recv)
        {
            Receive_JoinRoom join = (Receive_JoinRoom)ConvertPacketFunc.ByteArrayToStructure(recv._datas, typeof(Receive_JoinRoom), recv._totalSize);

            if (_rooms.ContainsKey(join._sessionID))
            {
                if(_rooms[join._sessionID]._userUUIDList.Count < 4)
                {
                    _rooms[join._sessionID]._userUUIDList.Add(recv._targetID);
                    _rooms[join._sessionID]._redayList.Add(false);

                    stSocket socket = _clients[recv._targetID];
                    socket._sessionID = join._sessionID;
                    _clients[recv._targetID] = socket;
                    Send_JoinResult(true, join._sessionID, recv._targetID);
                }
                else
                {
                    Send_JoinResult(false, "방이 찼습니다.", recv._targetID);
                }
            }
            else
            {
                Send_JoinResult(false, "세션 ID가 없습니다.", recv._targetID);
            }
        }
        void Receive_PlayerPosition(Packet recv)
        {
            Receive_PlayerPosition playerPosition = (Receive_PlayerPosition)ConvertPacketFunc.ByteArrayToStructure(recv._datas, typeof(Receive_PlayerPosition), recv._totalSize);
            stSocket movePlayer = _clients[playerPosition._UUID];
            if (movePlayer._position.X != playerPosition._x || movePlayer._position.Z != playerPosition._z)
            {
                string sessionID = playerPosition._sessionID;
                long targetID = playerPosition._UUID;
                movePlayer._position = new Vector3(playerPosition._x, 0, playerPosition._z);
                _clients[playerPosition._UUID] = movePlayer;
                if (playerPosition._init) {
                    foreach (long uuid in _rooms[sessionID]._userUUIDList)
                    {
                        if (uuid != targetID)
                        {
                            Send_Position(_clients[uuid]._position.X, _clients[uuid]._position.Z, targetID, uuid, sessionID);
                            Send_Position(_clients[targetID]._position.X, _clients[targetID]._position.Z, uuid, targetID, sessionID);
                        }
                        Send_Index(targetID, uuid, sessionID);
                        Send_Index(uuid, targetID, sessionID);
                    }
                }
            }        
        }

        void Receive_PlayerMove(Packet recv)
        {
            Receive_PlayerMove move = (Receive_PlayerMove)ConvertPacketFunc.ByteArrayToStructure(recv._datas, typeof(Receive_PlayerMove), recv._totalSize);

            try
            {

                foreach (long uuid in _rooms[move._sessionID]._userUUIDList)
                {
                    if (uuid != move._UUID)
                    {
                        Send_Move(move._rx, move._mz, uuid, move._UUID, move._sessionID);
                    }
                }
            }
            catch(Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        void Receive_GameReady(Packet recv)
        {
            Receive_GameReady gameReady = (Receive_GameReady)ConvertPacketFunc.ByteArrayToStructure(recv._datas, typeof(Receive_GameReady), recv._totalSize);

            try
            {
                _rooms[gameReady._sessionID]._redayList[gameReady._index] = gameReady._ready;
                Console.WriteLine("{0} 준비: {1}", gameReady._index, gameReady._ready);
                stRoom room = _rooms[gameReady._sessionID];
                foreach (bool ready in _rooms[gameReady._sessionID]._redayList)
                {
                    if (!ready)
                    {
                        if (room._isReady)
                        {
                            Console.WriteLine("게임시작 X");
                            room._isReady = false;
                            _rooms[gameReady._sessionID] = room;
                            Send_GameStart(gameReady._sessionID);
                        }
                        return;
                    }  
                }
                room._isReady = true;
                Console.WriteLine("게임시작");
                _rooms[gameReady._sessionID] = room;
                Send_GameStart(gameReady._sessionID);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        #endregion [RecvProcessingFunc]

        #region[CommonFunc]
        string SessionCreate()
        {
            string randomSession = string.Empty;
            for (int i = 1; i <= 5; i++)
            {
                int Char = _ran.Next(1, 52);
                if (Char > 26)
                {
                    Char = Char + 70;
                }
                else
                {
                    Char = Char + 64;
                }
                randomSession = randomSession + (char)Char;
            }
            return randomSession;
        }
        #endregion[CommonFunc]

        
    }
}
