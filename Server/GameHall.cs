﻿using CommonClass;
using CommonClass.Game;
using CommonClassLibrary;
using SanguoshaServer.Game;
using SuperSocket.SocketBase;
using SuperSocket.SocketBase.Protocol;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data;
using System.Threading;


namespace SanguoshaServer
{
    public class GameHall
    {
        private ConcurrentDictionary<int, Client> UId2ClientTable;
        private ConcurrentDictionary<MsgPackSession, Client> Session2ClientTable;
        private ConcurrentDictionary<int, Room> RId2Room;
        private ConcurrentDictionary<Room, Thread> Room2Thread;
        private Form1 form;
        private int room_serial = 0;

        public GameHall(Form1 form)
        {
            this.form = form;
            UId2ClientTable = new ConcurrentDictionary<int, Client>();
            Session2ClientTable = new ConcurrentDictionary<MsgPackSession, Client>();
            RId2Room = new ConcurrentDictionary<int, Room>();
            Room2Thread = new ConcurrentDictionary<Room, Thread>();

            new Engine();
        }
        public Room GetRoom(int room_id)
        {
            if (RId2Room.TryGetValue(room_id, out Room room))
                return room;

            return null;
        }
        public Client GetClient(int uid)
        {
            if (UId2ClientTable.TryGetValue(uid, out Client client))
                return client;
            else
                return null;
        }

        #region 客户端首次对话时
        public void OnConnected(MsgPackSession session) {
            Client new_client = new Client(this, session);
            Session2ClientTable.TryAdd(session, new_client);

            OutPut(session.LocalEndPoint.ToString() + " 进行了连接");

            //Attach the Delegates
            new_client.Connected += OnConnected;
        }
        #endregion

        #region 客户端断线时操作
        internal void OnDisconnected(MsgPackSession session, CloseReason value)
        {
            OutPut(session.SessionID + " disconnected:" + value);
            if (Session2ClientTable.TryGetValue(session, out Client client))
            {
                client.Connected -= OnConnected;
                //删除session对应
                Session2ClientTable.TryRemove(session, out Client remove);
                session = null;
                //删除uid对应
                if (client != null)
                {
                    client.OnDisconnected();

                    if (UId2ClientTable.ContainsKey(client.UserID))
                        UId2ClientTable.TryRemove(client.UserID, out remove);
                    //广播离线信息
                    ClientList.Instance().RemoveClient(client.UserID);
                    //更新到form1
                    UpdateUsers(ClientList.Instance().GetUserList(0));

                    MyData data = new MyData()
                    {
                        Description = PacketDescription.Hall2Cient,
                        Protocol = Protocol.UpdateHallLeave,
                        Body = new List<string> { client.UserID.ToString() }
                    };

                    List<Client> clients = new List<Client>(UId2ClientTable.Values);
                    foreach (Client other in clients)
                        other.SendProfileReply(data);
                }
            }
        }
        #endregion

        #region 信息输出至form
        public delegate void OutDelegate(string text);
        public void OutPut(string message)
        {
            if (form.InvokeRequired)
            {
                OutDelegate outdelegate = new OutDelegate(OutPut);
                form.BeginInvoke(outdelegate, new object[] { message });
                return;
            }
            form.AddLoginMessage(message);
        }
        public delegate void UpdateUserDelegate(DataTable message);
        public void UpdateUsers(DataTable message)
        {
            if (form.InvokeRequired)
            {
                UpdateUserDelegate outdelegate = new UpdateUserDelegate(UpdateUsers);
                form.BeginInvoke(outdelegate, new object[] { message });
                return;
            }
            form.UpdateUser(message);
        }
        
        public void Debug(string message)
        {
            if (form.InvokeRequired)
            {
                OutDelegate outdelegate = new OutDelegate(Debug);
                form.BeginInvoke(outdelegate, new object[] { message });
                return;
            }
            form.AddDebugMessage(message);
        }
        #endregion

        #region 同账号重复登录时
        public bool HandleSameAccount(int uid) {
            Client same = GetClient(uid);
            if (same != null)
            {
                //发送同账号登录信息
                MyData data = new MyData
                {
                    Description = PacketDescription.Hall2Cient,
                    Protocol = Protocol.ClientMessage,
                    Body = new List<string> { ClientMessage.LoginDuplicated.ToString(), true.ToString() }
                };
                same.SendLoginReply(data);

                OutPut(string.Format("{0} 重复登录", same.UserName));

                //断开连接
                same.Session.Close();

                //测试
                UId2ClientTable.TryRemove(uid, out Client client);

                return true;
            }
            else
                return false;
        }
        #endregion

        #region 处理客户端请求
        public void OnRequesting(MsgPackSession session, BinaryRequestInfo requestInfo) {
            Client client = Session2ClientTable[session];
            MyData data = null;
            try
            {
                data = PacketTranslator.Unpack(requestInfo.Body);
            }
            catch (Exception e)
            {
                Debug(string.Format("error at parse client request {0} {1}", e.Message, e.TargetSite));
                Debug(string.Format("error messsage {0} {1}", requestInfo?.ToString(), requestInfo.Body?.ToString()));
                session.Close();
                return;
            }
            TransferType request = PacketTranslator.GetTransferType(requestInfo.Key);

            //OutPut(string.Format("请求协议为{0}, 内容为{1}", data.Protocol.ToString(), data.Body[0]));
            if (client.UserName == null && !request.Equals(TransferType.TypeLogin))
            {
                OutPut(string.Format("{0} 未登录客户端，关闭连接", client.IP));
                session.Close();
                return;
            }

            if (data.Description == PacketDescription.Client2Hall)
            {
                switch (request)
                {
                    case TransferType.TypeLogin:                          //登录相关
                        switch (data.Protocol)
                        {
                            case Protocol.Login:
                                client.CheckUserName(data);
                                break;
                            case Protocol.Register:                       //用户注册
                                client.Register(data);
                                break;
                            case Protocol.PasswordChange:                 //密码修改
                                break;
                            default:
                                return;
                        }
                        break;
                    case TransferType.TypeMessage:                        //公共事件 比如 聊天
                        MessageForward(client, data);
                        break;
                    case TransferType.TypeSwitch:                           //切换room和hall
                        RoomSwitch(client, data);
                        break;
                    case TransferType.TypeUserProfile:                      //修改个人信息
                        client.UpDateProfile(data);
                        break;
                    default:
                        Debug(string.Format("{0} 对聊天大厅无效的请求", client.UserName));
                        break; ;
                }
            }
            else if (data.Description == PacketDescription.Client2Room)
            {
                switch (request)
                {
                    case TransferType.TypeMessage:                        //公共事件 比如 聊天
                        MessageForward(client, data);
                        break;
                    case TransferType.TypeGameControll:                   //游戏内各操作
                        client.ControlGame(data);
                        break;
                    case TransferType.TypeSwitch:                           //切换room和hall
                        RoomSwitch(client, data);
                        break;
                    default:
                        Debug(string.Format("{0} 对ROOM无效的请求", client.UserName));
                        break; ;
                }
            }
            else
            {
                OutPut("无效的客户端请求");
            }
        }
        #endregion

        #region 客户端登录成功时
        public void OnConnected(object sender, EventArgs e)
        {
            if (sender is Client temp)
            {
                //同名已连接用户强制离线
                bool same = HandleSameAccount(temp.UserID);

                //Add the client to the Hashtable 
                UId2ClientTable.TryAdd(temp.UserID, temp);
                OutPut("Client Connected:" + temp.UserID);

                //check last game is over
                bool reconnect = false;
                if (temp.GameRoom > 0)
                {
                    if (RId2Room.TryGetValue(temp.GameRoom, out Room room) && room.GameStarted)
                    {
                        reconnect = true;
                    }
                    else
                    {
                        //更新用户room id
                        ClientDBOperation.UpdateGameRoom(temp.UserName, -1);
                    }
                }

                InterHall(temp, reconnect);
            }
        }

        public void InterHall(Client client, bool reconnect)
        {
            //update user profile
            //bring to hall
            bool proceed = client.GetProfile(!reconnect);
            if (!proceed) return;

            ClientList.Instance().AddClient(client.UserID, client.Profile.NickName);
            //更新到form1
            UpdateUsers(ClientList.Instance().GetUserList(0));

            MyData data = new MyData();
            if (!reconnect)
            {
                //发送当前已登录的其他玩家信息和游戏房间信息
                DataSet ds = new DataSet();
                ds.Tables.Add(ClientList.Instance().GetUserList(0).Copy());
                ds.Tables.Add(RoomList.Instance().GetRoomList().Copy());

                data = new MyData
                {
                    Description = PacketDescription.Hall2Cient,
                    Protocol = Protocol.GetProfile,
                    Body = new List<string> { JsonUntity.DataSet2Json(ds) }
                };
                client.SendProfileReply(data);
            }

            //通知其他客户端更新
            data = new MyData()
            {
                Description = PacketDescription.Hall2Cient,
                Protocol = Protocol.UpdateHallJoin,
                Body = new List<string> { client.UserID.ToString(), client.Profile.NickName, "0" }
            };

            List<Client> clients = new List<Client>(UId2ClientTable.Values);
            foreach (Client other in clients)
            {
                if (other != client)
                    other.SendProfileReply(data);
            }
        }
        #endregion

        #region 消息转发
        private void MessageForward(Client sourcer, MyData data)
        {
            MyData message = new MyData
            {
                Description = PacketDescription.Hall2Cient,
                Protocol = data.Protocol
            };
            message.Body = data.Body;

            List<Client> clients = new List<Client>(UId2ClientTable.Values);
            switch (data.Protocol) {
                case Protocol.Message2Hall:
                    foreach (Client client in clients)
                    {
                        if (client.GameRoom <= 0)
                            client.SendMessage(message);
                    }
                    break;
                case Protocol.Message2Client:
                    Client destination = GetClient(int.Parse(data.Body[2]));
                    if (destination != null)
                    {
                        destination.SendMessage(message);
                    }
                    break;
                case Protocol.Message2Room:
                    message.Description = PacketDescription.Room2Cient;
                    Room room = GetRoom(sourcer.GameRoom);
                    if (room != null)
                    {
                        if (!room.GameStarted || !room.Setting.SpeakForbidden || data.Body.Count == 3)
                        {
                            if (!room.GameStarted)
                            {
                                foreach (Client dest in room.Clients)
                                    if (dest.GameRoom == room.RoomId)
                                        dest.SendMessage(message);
                            }
                            else
                            {
                                //假如双方有分冷暖阵营，则只有同阵营之间可以通讯
                                Game3v3Camp camp = room.GetPlayers(sourcer)[0].Camp;
                                foreach (Client dest in room.Clients)
                                    if (dest.GameRoom == room.RoomId && room.GetPlayers(dest)[0].Camp == camp)
                                        dest.SendMessage(message);
                            }
                        }
                    }
                    break;
                case Protocol.MessageSystem:
                    if (sourcer.UserRight >= 3)
                    {
                        foreach (Client client in clients)
                            client.SendMessage(message);
                    }
                    break;
                default:
                    return;
            }
        }
        #endregion

        #region 更新roomlist、进入/退出游戏房间
        private void RoomSwitch(Client client, MyData data)
        {
            Room room = null;
            switch (data.Protocol)
            {
                case Protocol.CreateRoom:
                    CreateRoom(client, JsonUntity.Json2Object<GameSetting>(data.Body[0]));
                    break;
                case Protocol.JoinRoom:
                    int room_id = -1;
                    string pass = string.Empty;
                    if (data.Body.Count > 0)
                    {
                        room_id = int.Parse(data.Body[0]);
                        pass = data.Body[1].ToString();
                    }
                    else
                    {
                        room_id = client.GameRoom;
                    }
                    
                    bool result = false;
                    if (RId2Room.TryGetValue(room_id, out room))
                    {
                        //if (JoinRoom != null)
                        //{
                        //    Delegate[] delArray = JoinRoom.GetInvocationList();
                        //    foreach (Delegate del in delArray)
                        //    {
                        //        Room target = (Room)del.Target;
                        //        if (target != null && target.RoomId == room_id)
                        //        {
                        //            JoinRoomDelegate method = (JoinRoomDelegate)del;
                        //            result = method(client, room_id, pass);
                        //        }
                        //    }
                        //}
                        //JoinRoom?.Invoke(client, room_id, pass);
                        result = room.OnClientRequestInter(client, room_id, pass);
                    }
                    if (!result)
                    {
                        Debug(string.Format("{0} join request fail at hall", client.UserName));
                        data = new MyData
                        {
                            Description = PacketDescription.Hall2Cient,
                            Protocol = Protocol.JoinRoom,
                        };
                        client.SendSwitchReply(data);
                    }
                    else
                    {
                        ClientList.Instance().AddClient(client.UserID, client.Profile.NickName, room_id);
                        UpdateUsers(ClientList.Instance().GetUserList(0));
                    }
                    break;
                case Protocol.LeaveRoom:
                    if (RId2Room.TryGetValue(client.GameRoom, out room))
                    {
                        client.RequestLeaveRoom();

                        //更新form
                        ClientList.Instance().AddClient(client.UserID, client.Profile.NickName);
                        UpdateUsers(ClientList.Instance().GetUserList(0));
                    }
                    break;
                case Protocol.UpdateRoom:
                    client.RequstReady(bool.Parse(data.Body[0]));
                    break;
                case Protocol.RoleReserved:
                    if (client.UserRight >= 2 && RId2Room.TryGetValue(client.GameRoom, out room))
                    {
                        if (!room.GameStarted && room.Setting.GameMode == "Classic")
                            client.RoleReserved = data.Body[0];
                    }
                    break;
                case Protocol.GeneralReserved:
                    if (client.UserRight >= 2 && RId2Room.TryGetValue(client.GameRoom, out room) && !room.GameStarted
                        && (room.Setting.GameMode == "Classic" || room.Setting.GameMode == "Hegemony") && data.Body.Count <= 2)
                    {
                        client.CheatArgs = data.Body;
                    }
                    break;
                case Protocol.KickOff:
                    if (RId2Room.TryGetValue(client.GameRoom, out room) && room != null && room.Host == client && !room.GameStarted)
                    {
                        int victim_id = int.Parse(data.Body[0]);
                        Client victim = GetClient(victim_id);
                        if (victim != null || victim_id < 0)
                        {
                            victim.RequestLeaveRoom(true);

                            //更新form
                            ClientList.Instance().AddClient(client.UserID, client.Profile.NickName);
                            UpdateUsers(ClientList.Instance().GetUserList(0));
                        }
                    }
                    break;
                case Protocol.ConfigChange:
                    if (RId2Room.TryGetValue(client.GameRoom, out room) && room != null && room.Host == client && !room.GameStarted)
                    {
                        GameSetting setting = JsonUntity.Json2Object<GameSetting>(data.Body[0]);
                        GameMode mode = Engine.GetMode(room.Setting.GameMode);

                        if (room.Setting.GameMode == setting.GameMode && room.Setting.PlayerNum == setting.PlayerNum && setting.GeneralPackage.Count > 0 && setting.CardPackage.Count > 0)
                        {
                            foreach (string general in setting.GeneralPackage)
                                if (!mode.GeneralPackage.Contains(general))
                                    return;

                            foreach (string card in setting.CardPackage)
                                if (!mode.CardPackage.Contains(card))
                                    return;

                            room.ChangeSetting(setting);
                        }
                    }
                    break;
                default:
                    break;
                    
            }
        }

        private void CreateRoom(Client client, GameSetting setting)
        {
            //检查游戏设置是否正常
            GameMode mode = Engine.GetMode(setting.GameMode);
            if (!RId2Room.ContainsKey(client.GameRoom) && mode.Name == setting.GameMode && mode.CardPackage.Count > 0 && mode.GeneralPackage.Count > 0
                && setting.GeneralPackage.Count > 0 && setting.CardPackage.Count > 0)
            {
                if (!mode.PlayerNum.Contains(setting.PlayerNum))
                    setting.PlayerNum = mode.PlayerNum[0];
                
                foreach (string general in setting.GeneralPackage)
                {
                    if (!mode.GeneralPackage.Contains(general))
                    {
                        setting.GeneralPackage = mode.GeneralPackage;
                        break;
                    }
                }
                foreach (string card in setting.CardPackage)
                {
                    if (!mode.GeneralPackage.Contains(card))
                    {
                        setting.CardPackage = mode.CardPackage;
                        break;
                    }
                }

                //Debug(string.Format("创建room的hall当前线程为{0}", Thread.CurrentThread.ManagedThreadId));

                int room_id = ++room_serial;
                Room room = new Room(this, room_id, client, setting);
                RId2Room.TryAdd(room_id, room);

                //通知form更新
                ClientList.Instance().AddClient(client.UserID, client.Profile.NickName, room_id);
                UpdateUsers(ClientList.Instance().GetUserList(0));
            }
            else
            {
                //设置错误，通知客户端
                MyData data = new MyData
                {
                    Description = PacketDescription.Hall2Cient,
                    Protocol = Protocol.CreateRoom,
                };
                client.SendSwitchReply(data);
            }
        }

        public void StartGame(Room room)
        {
            //更新房间号至数据库，以备断线重连
            foreach (Client client in room.Clients)
                if (client.UserID > 0)
                    ClientDBOperation.UpdateGameRoom(client.UserName, client.GameRoom);
            
            Thread thread = new Thread(room.Run);
            Room2Thread.TryAdd(room, thread);
            thread.Start();
        }

        //广播状态更改了的room
        public void BroadCastRoom(Room room)
        {
            lock (this)
            {
                RoomList.Instance().AddRoom(room.RoomId, room.Setting.Name, !string.IsNullOrEmpty(room.Setting.PassWord),
                                                room.Setting.GameMode, room.GameStarted, room.Clients.Count, room.Setting.PlayerNum);
                MyData data = new MyData
                {
                    Description = PacketDescription.Hall2Cient,
                    Protocol = Protocol.UPdateRoomList,
                    Body = new List<string>
                    {
                        room.RoomId.ToString(),
                        room.Setting.Name,
                        (!string.IsNullOrEmpty(room.Setting.PassWord)).ToString(),
                        room.Setting.GameMode,
                        room.GameStarted.ToString(),
                        room.Clients.Count.ToString(),
                        room.Setting.PlayerNum.ToString()
                    }
                };
                List<Client> clients = new List<Client>(UId2ClientTable.Values);
                foreach (Client client in clients)
                {
                    client.SendProfileReply(data);
                }
            }
        }

        public void RemoveRoom(Room room, Client host, List<Client> clients)
        {
            //Debug("remove at " + Thread.CurrentThread.ManagedThreadId.ToString());

            //更新该room下用户的房间号为0
            foreach (Client client in clients)
            {
                if (client.UserID > 0)
                {
                    client.GameRoom = 0;
                    ClientDBOperation.UpdateGameRoom(client.UserName, 0);
                }
            }

            //若房主存在，重建一个新的房间
            if (host != null)
            {
                CreateRoom(host, room.Setting);
                int id = host.GameRoom;

                //Debug(string.Format("host {0} {1}", host.Profile.NickName, host.GameRoom));

                if (id > 0 && id != room.RoomId)
                {
                    Room new_room = GetRoom(id);
                    foreach (Client client in clients)
                    {
                        if (client == host || client.Status != Client.GameStatus.online) continue;
                        new_room.OnClientRequestInter(client, id, room.Setting.PassWord);
                    }
                }
            }

            if (!RId2Room.TryRemove(room.RoomId, out Room remove))
                Debug(string.Format("remove room {0} failed", room.RoomId));

            if (Room2Thread.TryGetValue(room, out Thread thread))
            {
                thread.Abort();
                Room2Thread.TryRemove(room, out Thread _trhead);
                thread = null;
            }
            RoomList.Instance().RemoveRoom(room.RoomId);
            int room_id = room.RoomId;
            room.Dispose();
            room = null;

            MyData data = new MyData
            {
                Description = PacketDescription.Hall2Cient,
                Protocol = Protocol.UPdateRoomList,
                Body = new List<string>
                    {
                        room_id.ToString()
                    }
            };

            List<Client> all = new List<Client>(UId2ClientTable.Values);
            foreach (Client client in all)
            {
                client.SendProfileReply(data);
            }
        }

        #endregion

        #region 添加AI相关
        private int bot_id = 0;
        public int GetBotId()
        {
            lock (this)
            {
                return --bot_id;
            }
        }
        public void AddBot(Client client)
        {
            UId2ClientTable.TryAdd(client.UserID, client);
        }

        public void RemoveBot(Client client)
        {
            UId2ClientTable.TryRemove(client.UserID, out Client remove);
        }
        #endregion

        #region 自动生成AI进行游戏压力测试
        public void AutoTest()
        {
            for (int i = 0; i < 10; i++)
            {
                int room_id = ++room_serial;
                GameMode mode = Engine.GetMode("Hegemony");
                GameSetting setting = new GameSetting
                {
                    Name = string.Format("test room {0}", room_id),
                    PlayerNum = 10,
                    GameMode = "Hegemony",
                    CardPackage = mode.CardPackage,
                    GeneralPackage = mode.GeneralPackage,
                    GeneralCount = 7,
                    LordConvert = true
                };

                Room room = new Room(this, room_id, setting);
                if (!RId2Room.TryAdd(room_id, room))
                    Debug(string.Format("add room {0} failed", room_id));

                BroadCastRoom(room);
            }

            for (int i = 0; i < 10; i++)
            {
                int room_id = ++room_serial;
                GameMode mode = Engine.GetMode("Classic");
                GameSetting setting = new GameSetting
                {
                    Name = string.Format("test room {0}", room_id),
                    PlayerNum = 8,
                    GameMode = "Classic",
                    CardPackage = mode.CardPackage,
                    GeneralPackage = mode.GeneralPackage,
                    GeneralCount = 5,
                };

                Room room = new Room(this, room_id, setting);
                if (!RId2Room.TryAdd(room_id, room))
                    Debug(string.Format("add room {0} failed", room_id));


                BroadCastRoom(room);
            }

            for (int i = 0; i < 10; i++)
            {
                int room_id = ++room_serial;
                GameMode mode = Engine.GetMode("GuanduWarfare");
                GameSetting setting = new GameSetting
                {
                    Name = string.Format("test room {0}", room_id),
                    PlayerNum = 4,
                    GameMode = "GuanduWarfare",
                    CardPackage = mode.CardPackage,
                    GeneralPackage = mode.GeneralPackage,
                    GeneralCount = 0,
                };

                Room room = new Room(this, room_id, setting);
                if (!RId2Room.TryAdd(room_id, room))
                    Debug(string.Format("add room {0} failed", room_id));

                BroadCastRoom(room);
            }
        }

        public void RemoveRoom(Room room)
        {
            //Debug("remove at " + Thread.CurrentThread.ManagedThreadId.ToString());
            GameSetting setting = room.Setting;
            RId2Room.TryRemove(room.RoomId, out Room remove);
            if (Room2Thread.TryGetValue(room, out Thread thread))
            {
                thread.Abort();
                Room2Thread.TryRemove(room, out Thread _trhead);
                thread = null;
            }
            RoomList.Instance().RemoveRoom(room.RoomId);
            int room_id = room.RoomId;
            room.Dispose();
            room = null;

            MyData data = new MyData
            {
                Description = PacketDescription.Hall2Cient,
                Protocol = Protocol.UPdateRoomList,
                Body = new List<string>
                    {
                        room_id.ToString()
                    }
            };

            List<Client> clients = new List<Client>(UId2ClientTable.Values);
            foreach (Client client in clients)
            {
                client.SendProfileReply(data);
            }

            int new_id = ++room_serial;
            Room new_room = new Room(this, new_id, setting);
            if (!RId2Room.TryAdd(new_id, new_room))
                Debug(string.Format("add room {0} failed", new_id));

            BroadCastRoom(new_room);
        }
        #endregion
    }
}