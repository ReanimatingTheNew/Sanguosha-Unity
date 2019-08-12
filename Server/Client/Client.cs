﻿using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Threading;
using CommonClass;
using CommonClass.Game;
using CommonClassLibrary;
using SanguoshaServer.Game;
using SanguoshaServer.Package;
using static SanguoshaServer.Package.FunctionCard;
using CommandType = CommonClassLibrary.CommandType;

/*
 此类负责处理人机逻辑
*/

namespace SanguoshaServer
{
    public class Client : IDisposable
    {
        public enum GameStatus {
            normal,
            ready,
            online,
            offline,
            bot,
        }

        public event ConnectDelegate Connected;
        public event DisconnectDelegate Disconnected;
        public event LeaveRoomDelegate LeaveRoom;
        public event GetReadyDelegate GetReady;
        public event GameControlDelegate GameControl;

        //public delegate void MessageDelegate(object sender, MessageEventArgs e);
        //public delegate void LeaveHallDelegate(object sender, EventArgs e);
        //public delegate void JoinHallDelegate(object sender, JoinHallEventArgs e);
        //public delegate void LeaveDeskDelegate(object sender, LeaveDeskEventArgs
        //public delegate void GameOverDelegate(object sender, GameOverEventArgs e);

        public delegate void ConnectDelegate(object sender, EventArgs e);
        public delegate void DisconnectDelegate(object sender, EventArgs e);
        public delegate void LeaveRoomDelegate(Client client, int room_id, bool kicked);
        public delegate void GetReadyDelegate(Client client, bool ready);
        public delegate void GameControlDelegate(Client client, MyData data);

        private MsgPackSession session;
        private GameHall hall;
        private Profile profile;

        public int UserID { get; set; }
        public string UserName { get; set; }
        public int UserRight { get; set; }
        public bool IsLogin { get; set; }
        public Profile Profile => profile;

        public string IP { get; }

        public GameStatus Status { get; set; }
        public int GameRoom
        {
            get => game_room;
            set
            {
                game_room = value;
                if (room != null)
                    room = null;
            }
        }
        
        public MsgPackSession Session {
            get { return session; }
        }
        public List<string> CommandArgs { get; set; }
        public bool IsClientResponseReady { get; set; }
        public List<string> ClientReply { get; set; }
        public CommandType ExpectedReplyCommand { get; set; }
        public bool IsWaitingReply { get; set; }
        public List<string> CheatArgs { get; set; }
        public string RoleReserved { get; set; } = string.Empty;

        //public int ExpectedReplySerial { get; set; }
        public Mutex mutex = new Mutex();

        private int game_room = 0;
        private Room room = null;

        //构造函数
        public Client(GameHall hall, MsgPackSession session, Profile profile = new Profile())
        {
            this.hall = hall;
            this.session = session;
            IsLogin = false;
            if (session != null)
            {
                IP = session.RemoteEndPoint.ToString();
            }
            this.profile = profile;
            if (profile.UId < 0)
                UserID = profile.UId;

            InitCallBakcs();
            discard_skill = new DiscardSkill();
            yiji_skill = new YijiViewAsSkill();
            exchange_skill = new ExchangeSkill();
        }

        //bot only
        public Client(GameHall hall, Profile profile = new Profile())
        {
            this.hall = hall;
            session = null;
            UserName = profile.NickName;
            IsLogin = false;
            this.profile = profile;
            if (profile.UId < 0)
                UserID = profile.UId;
        }

        public override bool Equals(object obj)
        {
            Client other = (Client)obj;
            return UserID == other.UserID && UserName == other.UserName && GameRoom == other.GameRoom;
        }

        public override int GetHashCode()
        {
            return UserID.GetHashCode() * UserName.GetHashCode() * GameRoom.GetHashCode();
        }

        #region  向客户端发送信息相关
        //登录
        public void SendLoginReply(MyData data)
        {
            byte[] bytes = PacketTranslator.data2byte(data, PacketTranslator.GetTypeString(TransferType.TypeLogin));
            if (session != null)
            {
                session.Send(bytes, 0, bytes.Length);
            }
        }

        //聊天大厅
        public void SendProfileReply(MyData data)
        {
            byte[] bytes = PacketTranslator.data2byte(data, PacketTranslator.GetTypeString(TransferType.TypeUserProfile));
            if (session != null)
            {
                session.Send(bytes, 0, bytes.Length);
            }
        }

        //切换
        public void SendSwitchReply(MyData data)
        {
            byte[] bytes = PacketTranslator.data2byte(data, PacketTranslator.GetTypeString(TransferType.TypeSwitch));
            if (session != null)
            {
                session.Send(bytes, 0, bytes.Length);
            }
        }

        //发送游戏信息/操作请求给客户端
        public void SendRoomNotify(List<string> message_body)
        {
            if (session != null)
            {
                MyData data = new MyData
                {
                    Description = PacketDescription.Room2Cient,
                    Protocol = protocol.GameNotification,
                    Body = message_body
                };
                byte[] bytes = PacketTranslator.data2byte(data, PacketTranslator.GetTypeString(TransferType.TypeGameControll));

                session.Send(bytes, 0, bytes.Length);
            }
        }
        public void SendRoomRequest(CommandType command, List<string> message_body)
        {
            if (session != null)
            {
                message_body.Insert(0, command.ToString());
                MyData data = new MyData
                {
                    Description = PacketDescription.Room2Cient,
                    Protocol = protocol.GameRequest,
                    Body = message_body
                };
                byte[] bytes = PacketTranslator.data2byte(data, PacketTranslator.GetTypeString(TransferType.TypeGameControll));

                session.Send(bytes, 0, bytes.Length);
            }
        }

        //发送聊天信息给客户端
        public void SendMessage(MyData data)
        {
            byte[] bytes = PacketTranslator.data2byte(data, PacketTranslator.GetTypeString(TransferType.TypeMessage));
            if (session != null)
                session.Send(bytes, 0, bytes.Length);
        }
        #endregion

        #region 登录相关
        //用户名检查
        public void CheckUserName(MyData data)
        {
            string UserName = data.Body[0], Password = data.Body[1];
            ClientMessage message = ClientDBOperation.CheckUserName(this, UserName, Password);
            if (message == ClientMessage.Connected)
                //Raise the Connected Event 
                Connected?.Invoke(this, new EventArgs());
            else if (message == ClientMessage.PasswordWrong)
            {
                //通知客户端
                MyData wrong = new MyData
                {
                    Description = PacketDescription.Hall2Cient,
                    Protocol = protocol.ClientMessage,
                    Body = new List<string> { ClientMessage.PasswordWrong.ToString(), true.ToString() }
                };
                SendLoginReply(wrong);
                //关闭连接
            }
            else
            {
                //账号不存在
                //通知客户端
                MyData wrong = new MyData {
                    Description = PacketDescription.Hall2Cient,
                    Protocol = protocol.ClientMessage,
                    Body = new List<string> { ClientMessage.NoAccount.ToString(), true.ToString() }
                };
                SendLoginReply(wrong);
                //关闭连接
            }
        }

        //从数据库读取个人信息并发送给客户端
        public bool GetProfile(bool to_hall = true)
        {
            profile = ClientDBOperation.GetProfile(this, to_hall);
            //首次登录个人信息为空
            if (Profile.UId <= 0)
            {
                //向客户端发送起名请求
                //MyData data = new MyData
                //{
                //    Description = PacketDescription.Hall2Cient,
                //    Protocol = protocol.NickName
                //};
                //SendProfileReply(data);

                MyData data = new MyData
                {
                    Description = PacketDescription.Hall2Cient,
                    Protocol = protocol.UserProfile,
                    Body = new List<string> { JsonUntity.Object2Json(Profile) }
                };

                SendProfileReply(data);
                return false;
            }
            else
            {
                //发送至客户端
                MyData data = new MyData
                {
                    Description = PacketDescription.Hall2Cient,
                    Protocol = protocol.UserProfile,
                    Body = new List<string> { JsonUntity.Object2Json(Profile), to_hall.ToString() }
                };

                SendProfileReply(data);
                return true;
            }
        }
        #endregion
        //账号注册和个人信息更新
        public void Register(MyData data)
        {
            string id = data.Body[0];
            string pwd = data.Body[1];
            if (ClientDBOperation.Register(this, id, pwd))
            {
                hall.OutPut(UserID + "：注册成功");

                MyData message = new MyData
                {
                    Description = PacketDescription.Hall2Cient,
                    Protocol = protocol.ClientMessage,
                    Body = new List<string> { ClientMessage.RegisterSuccesful.ToString(), false.ToString() }
                };
                SendLoginReply(message);

                //初始化与room的交互方法

                if (Connected != null)
                {
                    EventArgs e = new EventArgs();
                    Connected(this, e);
                }
            }
            else
            {
                //用户名已注册
                hall.OutPut(id + "已注册");
                //发送信息通知客户端
                MyData message = new MyData
                {
                    Description = PacketDescription.Hall2Cient,
                    Protocol = protocol.ClientMessage,
                    Body = new List<string> { ClientMessage.AccountDuplicated.ToString(), true.ToString() }
                };
                SendLoginReply(message);
            }
        }
        //建立新的个人信息表
        public void InsertNewProfile(string NickName)
        {
            bool succe = ClientDBOperation.InsertNewProfile(this, NickName);
            if (!succe)
            {
                //昵称重名
                //通知客户端
                MyData data = new MyData
                {
                    Description = PacketDescription.Hall2Cient,
                    Protocol = protocol.NickName,
                    Body = new List<string>() { false.ToString() }
                };
                SendProfileReply(data);
            }
            else
            {
                MyData data = new MyData
                {
                    Description = PacketDescription.Hall2Cient,
                    Protocol = protocol.NickName,
                    Body = new List<string>() { true.ToString() }
                };
                SendProfileReply(data);

                hall.InterHall(this, false);
            }
        }
        //更新个人信息
        public void UpDateProfile(MyData data) {
            switch (data.Protocol) {
                case protocol.NickName:
                    InsertNewProfile(data.Body[0]);
                    break;
                case protocol.UserProfile:
                    Profile profile = JsonUntity.Json2Object<Profile>(data.Body[0]);
                    UpDateProfileAvatar(profile);
                    break;
                default:
                    return;
            }
        }
        private void UpDateProfileAvatar(Profile profile)
        {
            if (profile.UId == Profile.UId && Engine.CheckShwoAvailable(profile) && CheckTitle(profile.Title))
            {
                this.profile.Avatar = profile.Avatar;
                this.profile.Title = profile.Title;
                this.profile.Frame = profile.Frame;
                this.profile.Bg = profile.Bg;

                ClientDBOperation.UpDateProfileAvatar(profile);

                SendProfile2Client();
            }
        }

        public void UpdateProfileGamePlay(Profile profile)
        {
            if (profile.UId == this.profile.UId)
            {
                this.profile.GamePlay = profile.GamePlay;
                this.profile.Win = profile.Win;
                this.profile.Lose = profile.Lose;
                this.profile.Escape = profile.Escape;
                this.profile.Draw = profile.Draw;
                
                SendProfile2Client();
            }
        }

        public void AddProfileTitle(int title_id)
        {
            if (!profile.Titles.ContainsKey(title_id))
            {
                profile.Titles.Add(title_id, DateTime.Now.ToString());
                SendProfile2Client();
            }
        }

        private void SendProfile2Client()
        {
            MyData data = new MyData
            {
                Description = PacketDescription.Hall2Cient,
                Protocol = protocol.UserProfile,
                Body = new List<string> { JsonUntity.Object2Json(profile) }
            };

            SendProfileReply(data);
        }
        private bool CheckTitle(int id)
        {
            return profile.Titles.ContainsKey(id);
        }
        public void OnDisconnected()
        {
            ClientDBOperation.UpdateStatus(UserName, false);
            Disconnected?.Invoke(this, new EventArgs());
        }

        public void RequestLeaveRoom(bool kicked = false)
        {
            GameRoom = 0;
            ClientDBOperation.UpdateGameRoom(UserName, 0);

            LeaveRoom?.Invoke(this, GameRoom, kicked);
        }

        public void RequstReady(bool ready)
        {
            GetReady?.Invoke(this, ready);
        }

        public void ControlGame(MyData data)
        {
            CommandType request_command = (CommandType)Enum.Parse(typeof(CommandType), data.Body[0]);
            if (data.Protocol == protocol.GameRequest && IsWaitingReply && request_command == CommandType.S_COMMAND_OPERATE)
            {
                List<string> arg = data.Body;
                arg.RemoveAt(0);
                CommandType command = ExpectedReplyCommand;
                if (m_requestResponsePair.ContainsKey(ExpectedReplyCommand))
                {
                    command = m_requestResponsePair[ExpectedReplyCommand];
                }
                callbacks[command](arg);
            }
            else
            {
                if (data.Protocol == protocol.GameRequest && callbacks.ContainsKey(request_command))
                {
                    if (data.Body.Count > 1)            //规定由服务器处理的操作，除结束出牌外，不可能由客户端提交结果
                        return;
                }
                GameControl?.Invoke(this, data);
            }
        }

        private Dictionary<CommandType, Action<List<string>>> callbacks = new Dictionary<CommandType, Action<List<string>>>();
        private Dictionary<CommandType, CommandType> m_requestResponsePair = new Dictionary<CommandType, CommandType>();
        private Player requestor = null;
        private HandlingMethod method;
        private Dictionary<Player, List<WrappedCard>> all_cards = new Dictionary<Player, List<WrappedCard>>();
        private Dictionary<Player, List<WrappedCard>> hand_cards = new Dictionary<Player, List<WrappedCard>>();
        private Dictionary<Player, List<WrappedCard>> equip_cards = new Dictionary<Player, List<WrappedCard>>();
        private Dictionary<Player, List<WrappedCard>> selected_cards = new Dictionary<Player, List<WrappedCard>>();
        private Dictionary<string, List<WrappedCard>> available_cards = new Dictionary<string, List<WrappedCard>>();
        private Dictionary<string, List<string>> prepends = new Dictionary<string, List<string>>();
        private Dictionary<string, List<string>> appends = new Dictionary<string, List<string>>();
        private List<WrappedCard> guhuo_cards = new List<WrappedCard>();
        private WrappedCard selected_guhuo;
        private Dictionary<string, List<string>> available_equip_skills = new Dictionary<string, List<string>>();
        private Dictionary<string, List<string>> available_head_skills = new Dictionary<string, List<string>>();
        private Dictionary<string, List<string>> available_deputy_skills = new Dictionary<string, List<string>>();
        private ViewAsSkill pending_skill = null;
        private Player skill_owner = null;
        private string skill_position;
        private bool ok_enable, cancel_enable;
        private bool cancel_able;
        private bool skill_invoke;
        private WrappedCard initial_transfer_card = null;
        private List<Player> available_targets = new List<Player>();
        private List<Player> selected_targets = new List<Player>();
        private List<Player> extra_targets = new List<Player>();
        private AskForMoveCardsStruct guanxing = new AskForMoveCardsStruct();

        //for choose player min & max, huashen box or other dialog
        private List<string> ex_information;

        //choose player
        private int max_num, min_num;
        private string hightlight_skill;

        //variable control
        private bool m_do_request;
        PromoteStruct promote_skill = new PromoteStruct();
        private bool auto_target, first_selection, first_pending, double_click;
        private readonly bool auto_preshow;
        private readonly bool intel_select;

        private WrappedCard viewas_card;
        private DiscardSkill discard_skill;
        private YijiViewAsSkill yiji_skill;
        private ExchangeSkill exchange_skill;

        private void InitCallBakcs()
        {
            // init request response pair
            m_requestResponsePair[CommandType.S_COMMAND_PLAY_CARD] = CommandType.S_COMMAND_RESPONSE_CARD;
            m_requestResponsePair[CommandType.S_COMMAND_NULLIFICATION] = CommandType.S_COMMAND_RESPONSE_CARD;
            m_requestResponsePair[CommandType.S_COMMAND_SHOW_CARD] = CommandType.S_COMMAND_RESPONSE_CARD;
            m_requestResponsePair[CommandType.S_COMMAND_ASK_PEACH] = CommandType.S_COMMAND_RESPONSE_CARD;
            m_requestResponsePair[CommandType.S_COMMAND_PINDIAN] = CommandType.S_COMMAND_RESPONSE_CARD;
            m_requestResponsePair[CommandType.S_COMMAND_EXCHANGE_CARD] = CommandType.S_COMMAND_DISCARD_CARD;

            //Client notifications
            callbacks[CommandType.S_COMMAND_PLAY_CARD] = new Action<List<string>>(OnPlayCardRespond);
            callbacks[CommandType.S_COMMAND_RESPONSE_CARD] = new Action<List<string>>(OnPlayCardRespond);
            callbacks[CommandType.S_COMMAND_NULLIFICATION] = new Action<List<string>>(OnPlayCardRespond);
            callbacks[CommandType.S_COMMAND_SHOW_CARD] = new Action<List<string>>(OnPlayCardRespond);
            callbacks[CommandType.S_COMMAND_ASK_PEACH] = new Action<List<string>>(OnPlayCardRespond);
            callbacks[CommandType.S_COMMAND_PINDIAN] = new Action<List<string>>(OnPlayCardRespond);

            callbacks[CommandType.S_COMMAND_DISCARD_CARD] = new Action<List<string>>(OnPlayCardRespond);
            callbacks[CommandType.S_COMMAND_EXCHANGE_CARD] = new Action<List<string>>(OnPlayCardRespond);
            callbacks[CommandType.S_COMMAND_SKILL_YIJI] = new Action<List<string>>(OnPlayCardRespond);

            callbacks[CommandType.S_COMMAND_CHOOSE_PLAYER] = new Action<List<string>>(OnChoosePlayerResponse);
            callbacks[CommandType.S_COMMAND_CHOOSE_EXTRA_TARGET] = new Action<List<string>>(OnChooseExtraResponse);
            
            callbacks[CommandType.S_COMMAND_SKILL_MOVECARDS] = new Action<List<string>>(OnGuanxingRespond);
        }

        public bool PlayCardRequst(Room room, Player player, CommandType type, string prompt = null,
            HandlingMethod method = HandlingMethod.MethodNone, int notice_index = -1, string position = null)
        {
            this.room = room;
            m_do_request = true;
            requestor = player;
            ExpectedReplyCommand = type;
            this.method = method;
            skill_position = position;
            pending_skill = null;
            skill_invoke = false;
            if (type == CommandType.S_COMMAND_RESPONSE_CARD)
                hightlight_skill = room.GetRoomState().GetCurrentResponseSkill();
            else
                hightlight_skill = null;
            ex_information = null;

            string pattern = room.GetRoomState().GetCurrentCardUsePattern(player);
            CardUseStruct.CardUseReason reason = room.GetRoomState().GetCurrentCardUseReason();
            if (!string.IsNullOrEmpty(pattern) && pattern.EndsWith("!"))
                cancel_able = false;
            else
                cancel_able = !string.IsNullOrEmpty(pattern) || type == CommandType.S_COMMAND_PLAY_CARD;

            const string rx_pattern = @"@?@?([_A-Za-z]+)(\d+)?!?";
            if (!string.IsNullOrEmpty(pattern))
            {
                Match result = Regex.Match(pattern, rx_pattern);
                if (player != null && result.Length > 0)
                {
                    string skill_name = result.Groups[1].ToString();
                    ViewAsSkill skill = Engine.GetViewAsSkill(skill_name);
                    if (skill != null && skill.IsAvailable(room, player, reason, pattern, position))
                    {
                        skill_invoke = true;
                        pending_skill = skill;
                    }
                }
            }

            List<Player> requestors = new List<Player>();
            if (requestor != null)
                requestors.Add(requestor);
            else
                requestors = room.GetPlayers(this);

            foreach (Player p in requestors) {
                equip_cards[p] = RoomLogic.GetPlayerEquips(room, p);
                hand_cards[p] = RoomLogic.GetPlayerHandcards(room, p);
            }

            HandleInfos();

            Operate arg = GetPacket2Client(true, prompt, notice_index);
            m_do_request = false;
            return room.DoRequest(room.GetPlayers(this)[0], type, new List<string> { JsonUntity.Object2Json(arg) }, true);
        }

        public bool DiscardRequest(Room room, Player player, List<int> ids, string prompt, string reason, string position, int discard_num, int min_num, bool optional)
        {
            this.room = room;
            m_do_request = true;
            requestor = player;
            ExpectedReplyCommand = CommandType.S_COMMAND_DISCARD_CARD;
            hightlight_skill = reason;
            skill_position = position;

            pending_skill = discard_skill;

            discard_skill.Optional = optional;
            discard_skill.Reserved.Clear();
            discard_skill.AvailableCards = new List<int>(ids);
            discard_skill.Num = discard_num;
            discard_skill.MinNum = min_num;
            skill_invoke = true;

            equip_cards.Clear();
            equip_cards[player] = new List<WrappedCard>();
            hand_cards.Clear();
            hand_cards[player] = new List<WrappedCard>();
            string mark = "no";
            foreach (int id in ids)
            {
                WrappedCard card = room.GetCard(id);
                if (room.GetCardPlace(id) == Player.Place.PlaceEquip)
                {
                    mark = "yes";
                    equip_cards[player].Add(card);
                }
                else
                    hand_cards[player].Add(card);
            }

            ex_information = new List<string> { min_num.ToString(), discard_num.ToString(), mark };
            cancel_able = optional;

            HandleInfos();
            m_do_request = false;
            ClientReply = null;
            Operate arg = GetPacket2Client(true, prompt);
            return room.DoRequest(player, ExpectedReplyCommand, new List<string> { JsonUntity.Object2Json(arg) }, true);
        }

        public bool PeachRequest(Room room, Player dying, int num)
        {
            this.room = room;
            m_do_request = true;
            requestor = null;
            ExpectedReplyCommand = CommandType.S_COMMAND_ASK_PEACH;
            method = HandlingMethod.MethodUse;

            hightlight_skill = null;
            pending_skill = null;
            skill_owner = null;
            skill_invoke = false;
            skill_position = null;
            cancel_able = true;

            foreach (Player player in room.GetPlayers(this)) {
                equip_cards[player] = RoomLogic.GetPlayerEquips(room, player);
                hand_cards[player] = RoomLogic.GetPlayerHandcards(room, player);
            }

            ex_information = new List<string> { dying.Name, num.ToString() };
            HandleInfos();
            m_do_request = false;
            Operate arg = GetPacket2Client(true);
            return room.DoRequest(room.GetPlayers(this)[0], ExpectedReplyCommand, new List<string> { JsonUntity.Object2Json(arg) }, true);
        }
        public void NullificationRequest(Room room, string trick_name, Player from, Player to)
        {
            this.room = room;
            m_do_request = true;
            requestor = null;
            ExpectedReplyCommand = CommandType.S_COMMAND_NULLIFICATION;
            method = HandlingMethod.MethodUse;

            hightlight_skill = null;
            pending_skill = null;
            skill_owner = null;
            skill_invoke = false;
            skill_position = null;
            cancel_able = true;

            foreach (Player player in room.GetPlayers(this)) {
                equip_cards[player] = RoomLogic.GetPlayerEquips(room, player);
                hand_cards[player] = RoomLogic.GetPlayerHandcards(room, player);
            }

            ex_information = new List<string> { trick_name, from?.Name, to?.Name };
            HandleInfos();
            m_do_request = false;
            CommandArgs = new List<string> { JsonUntity.Object2Json(GetPacket2Client(true)) };
        }
        public bool ShowCardRequest(Room room, Player player, Player from)
        {
            this.room = room;
            requestor = player;
            ExpectedReplyCommand = CommandType.S_COMMAND_SHOW_CARD;
            method = HandlingMethod.MethodNone;

            hightlight_skill = null;
            pending_skill = null;
            skill_owner = null;
            skill_invoke = false;
            skill_position = null;

            ok_enable = false;
            cancel_enable = false;
            cancel_able = false;

            equip_cards.Clear();
            hand_cards.Clear();
            hand_cards[player] = RoomLogic.GetPlayerHandcards(room, player);
            available_cards.Clear();
            available_cards[player.Name] = hand_cards[player];
            selected_cards.Clear();

            appends.Clear();
            prepends.Clear();

            available_equip_skills.Clear();
            available_head_skills.Clear();
            available_deputy_skills.Clear();

            available_targets.Clear();
            selected_targets.Clear();
            guhuo_cards.Clear();
            selected_guhuo = null;

            ex_information = new List<string> { from.Name };
            
            return room.DoRequest(player, CommandType.S_COMMAND_SHOW_CARD, new List<string> { JsonUntity.Object2Json(GetPacket2Client(true)) }, true);
        }
        public bool ExtraRequest(Room room, Player player, List<Player> selected, WrappedCard card,
                                string prompt, string skill_name, string position)
        {
            this.room = room;
            m_do_request = true;
            requestor = player;
            ExpectedReplyCommand = CommandType.S_COMMAND_CHOOSE_EXTRA_TARGET;
            this.method = HandlingMethod.MethodUse;
            skill_position = position;
            viewas_card = card;

            ok_enable = false;
            cancel_enable = true;
            cancel_able = true;

            skill_invoke = false;
            pending_skill = null;
            hightlight_skill = skill_name;

            selected_targets.Clear();
            extra_targets = new List<Player>(selected);

            available_cards.Clear();
            selected_cards.Clear();

            available_equip_skills.Clear();
            available_head_skills.Clear();
            available_deputy_skills.Clear();

            guhuo_cards.Clear();
            selected_guhuo = null;

            ex_information = null;

            equip_cards.Clear();
            hand_cards.Clear();

            CheckExtraTarger();

            Operate arg = GetPacket2Client(true, prompt);
            m_do_request = false;
            return room.DoRequest(room.GetPlayers(this)[0], ExpectedReplyCommand, new List<string> { JsonUntity.Object2Json(arg) }, true);
        }

        public bool YijiRequest(Room room, Player player, List<int> ids, List<Player> targets, string prompt,
                               int max_num, bool option, string expand_pile, string position)
        {
            this.room = room;
            m_do_request = true;
            requestor = player;
            ExpectedReplyCommand = CommandType.S_COMMAND_SKILL_YIJI;
            hightlight_skill = null;
            skill_position = position;

            pending_skill = yiji_skill;
            yiji_skill.Initialize(ids, max_num, targets, expand_pile);
            skill_invoke = true;

            ex_information = new List<string> { max_num.ToString(), option.ToString() };
            cancel_able = option;

            equip_cards.Clear();
            equip_cards[player] = RoomLogic.GetPlayerEquips(room, player);
            hand_cards.Clear();
            hand_cards[player] = RoomLogic.GetPlayerHandcards(room, player);

            HandleInfos();
            m_do_request = false;
            return room.DoRequest(player, ExpectedReplyCommand, new List<string> { JsonUntity.Object2Json(GetPacket2Client(true, prompt)) }, true);
        }

        public bool ExchangeRequest(Room room, Player player, string prompt, string reason, string position,
                                   int discard_num, int min_num, string pattern, string expand_pile)
        {
            this.room = room;
            m_do_request = true;
            requestor = player;
            ExpectedReplyCommand = CommandType.S_COMMAND_EXCHANGE_CARD;
            hightlight_skill = reason;
            skill_position = position;
            pending_skill = exchange_skill;

            exchange_skill.Initialize(discard_num, min_num, expand_pile, pattern);
            skill_invoke = true;

            ex_information = new List<string> { discard_num.ToString(), min_num.ToString() };
            cancel_able = (min_num == 0);

            equip_cards.Clear();
            equip_cards.Clear();
            equip_cards[player] = RoomLogic.GetPlayerEquips(room, player);
            hand_cards.Clear();
            hand_cards[player] = RoomLogic.GetPlayerHandcards(room, player);

            HandleInfos();
            m_do_request = false;
            return room.DoRequest(player, ExpectedReplyCommand, new List<string> { JsonUntity.Object2Json(GetPacket2Client(true, prompt)) }, true);
        }

        public void PindianRequest(Room room, Player player, Player from)
        {
            this.room = room;
            requestor = player;
            ExpectedReplyCommand = CommandType.S_COMMAND_PINDIAN;
            method = HandlingMethod.MethodPindian;

            hightlight_skill = null;
            pending_skill = null;
            skill_owner = null;
            skill_invoke = false;
            skill_position = null;

            ok_enable = false;
            cancel_enable = false;
            cancel_able = false;

            equip_cards.Clear();
            hand_cards.Clear();
            hand_cards[player] = RoomLogic.GetPlayerHandcards(room, player);
            available_cards.Clear();
            available_cards[player.Name] = RoomLogic.GetPlayerHandcards(room, player);
            selected_cards.Clear();

            appends.Clear();
            prepends.Clear();

            available_equip_skills.Clear();
            available_head_skills.Clear();
            available_deputy_skills.Clear();

            available_targets.Clear();
            selected_targets.Clear();
            guhuo_cards.Clear();
            selected_guhuo = null;

            ex_information = new List<string> { from.Name };

            CommandArgs = new List<string> { JsonUntity.Object2Json(GetPacket2Client(true)) };
        }

        public bool CheckExtraTarger(Player target = null)
        {
            Skill skill = Engine.GetSkill(hightlight_skill);
            if (skill != null && skill is TargetModSkill ts)
            {
                List<Player> current_selected = new List<Player>(), old_selected = new List<Player>();
                foreach (Player p in selected_targets)
                    current_selected.Add(p);
                foreach (Player p in extra_targets)
                    old_selected.Add(p);
                List<Player> all_selected = new List<Player>(current_selected);
                all_selected.AddRange(old_selected);
                FunctionCard fcard = Engine.GetFunctionCard(viewas_card.Name);
                if (target != null)
                {
                    return ts.CheckExtraTargets(room, requestor, target, viewas_card, old_selected, current_selected)
                            && fcard.ExtratargetFilter(room, all_selected, target, requestor, viewas_card);
                }
                else
                {
                    available_targets.Clear();
                    List<Player> targets = new List<Player>(), available = new List<Player>(), selected = new List<Player>(extra_targets);
                    selected.AddRange(selected_targets);
                    foreach (Player p in room.GetAlivePlayers())
                        if (!selected.Contains(p))
                            targets.Add(p);

                    foreach (Player p in targets)
                    {
                        if (ts.CheckExtraTargets(room, requestor, p, viewas_card, old_selected, current_selected)
                                && fcard.ExtratargetFilter(room, all_selected, p, requestor, viewas_card))
                            available.Add(p);
                    }

                    if (available.Count == 0 && current_selected.Count > 0)
                    {
                        current_selected.RemoveAt(current_selected.Count - 1);
                        foreach (Player p in targets) {
                            if (ts.CheckExtraTargets(room, requestor, p, viewas_card, old_selected, current_selected)
                                    && fcard.ExtratargetFilter(room, all_selected, p, requestor, viewas_card))
                                available_targets.Add(p);
                        }
                    }
                    else
                        available_targets = available;
                }
            }
            return false;
        }
        public bool ChooseRequest(Room room, Player player, List<Player> targets, string prompt,
                                 string skillName, string position, int max_num, int min_num)
        {
            this.room = room;
            m_do_request = true;
            requestor = player;
            ExpectedReplyCommand = CommandType.S_COMMAND_CHOOSE_PLAYER;
            method = HandlingMethod.MethodNone;
            skill_position = position;
            pending_skill = null;
            skill_invoke = false;
            available_targets = targets;
            this.max_num = max_num;
            this.min_num = min_num;
            ex_information = new List<string> { min_num.ToString(), max_num.ToString() };

            viewas_card = null;
            skill_owner = player;
            hightlight_skill = skillName;

            prepends.Clear();
            appends.Clear();
            guhuo_cards.Clear();

            available_cards.Clear();
            available_equip_skills.Clear();
            available_head_skills.Clear();
            available_deputy_skills.Clear();

            selected_guhuo = null;
            selected_cards.Clear();
            selected_targets.Clear();

            cancel_able = (min_num == 0);
            ok_enable = false;
            cancel_enable = cancel_able;

            Operate arg = GetPacket2Client(true, prompt);
            m_do_request = false;
            return room.DoRequest(player,ExpectedReplyCommand, new List<string> { JsonUntity.Object2Json(arg) }, true);
        }

        public bool GuanxingRequest(Room room, Player player, string reason,
                                   List<int> ups, List<int> downs, int min_num, int max_num, bool can_refuse, bool write_step, string position)
        {
            this.room = room;
            ExpectedReplyCommand = CommandType.S_COMMAND_SKILL_MOVECARDS;
            requestor = player;
            
            m_do_request = true;
            skill_position = position;
            pending_skill = null;
            skill_invoke = write_step;
            hightlight_skill = reason;
            skill_owner = player;
            cancel_able = can_refuse;
            cancel_enable = can_refuse;
            this.max_num = max_num;
            this.min_num = min_num;

            available_cards.Clear();
            guhuo_cards.Clear();
            selected_cards.Clear();
            prepends.Clear();
            appends.Clear();
            selected_guhuo = null;
            available_targets.Clear();
            selected_targets.Clear();
            available_head_skills.Clear();
            available_deputy_skills.Clear();
            available_equip_skills.Clear();

            guanxing.Top = ups;
            guanxing.Bottom = downs;
            CheckMoveCards();

            Operate arg = GetPacket2Client(true);
            m_do_request = false;
            return room.DoRequest(player, CommandType.S_COMMAND_SKILL_MOVECARDS, new List<string> { JsonUntity.Object2Json(arg) }, true);
        }
        private void CheckMoveCards()
        {
            Skill skill = Engine.GetSkill(hightlight_skill);
            available_cards[requestor.Name] = new List<WrappedCard>();
            if (skill != null)
            {
                foreach (int id in guanxing.Bottom)
                    available_cards[requestor.Name].Add(room.GetCard(id));
                foreach (int id in guanxing.Top)
                {
                    if (skill.MoveFilter(room, id, guanxing.Bottom))
                        available_cards[requestor.Name].Add(room.GetCard(id));
                }
                ok_enable = guanxing.Bottom.Count >= min_num && guanxing.Bottom.Count <= max_num && skill.MoveFilter(room, -1, guanxing.Bottom);
                guanxing.Success = ok_enable;
                if (guanxing.Success && skill_invoke)
                {
                    ClientReply = new List<string> { JsonUntity.Object2Json(guanxing) };
                    //foreach (int id in guanxing.Top)
                    //    room.OutPut(string.Format("up :{0} {1}", room.GetCard(id).Name, room.GetCard(id).Number));
                    //foreach (int id in guanxing.Bottom)
                    //    room.OutPut(string.Format("down :{0} {1}", room.GetCard(id).Name, room.GetCard(id).Number));
                }
            }
            ex_information = new List<string> { JsonUntity.Object2Json(guanxing.Top), JsonUntity.Object2Json(guanxing.Bottom), min_num.ToString(), max_num.ToString() };
        }

        private void HandleInfos()
        {
            List<Player> requestors = new List<Player>();
            if (requestor != null)
                requestors.Add(requestor);
            else
                requestors = room.GetPlayers(this);

            viewas_card = null;
            if (ExpectedReplyCommand == CommandType.S_COMMAND_RESPONSE_CARD && !string.IsNullOrEmpty(hightlight_skill))
                skill_owner = requestor;
            else
                skill_owner = null;
            if (!skill_invoke)
                pending_skill = null;

            prepends.Clear();
            appends.Clear();
            guhuo_cards.Clear();

            available_cards.Clear();
            available_equip_skills.Clear();
            available_head_skills.Clear();
            available_deputy_skills.Clear();
            available_targets.Clear();

            selected_guhuo = null;
            selected_cards.Clear();
            selected_targets.Clear();
            
            ok_enable = false;
            cancel_enable = cancel_able;
            room.DoNotify(this, CommandType.S_COMMAND_LOG_EVENT, new List<string> { GameEventType.S_GAME_EVENT_CLIENT_TIP.ToString(), false.ToString() });

            CardUseStruct.CardUseReason reason = room.GetRoomState().GetCurrentCardUseReason();
            if (!skill_invoke)
            {
                foreach (Player player in requestors)
                {
                    available_equip_skills[player.Name] = new List<string>();
                    available_head_skills[player.Name] = new List<string>();
                    string pattern = room.GetRoomState().GetCurrentCardUsePattern(player);
                    foreach (WrappedCard card in equip_cards[player])
                    {
                        Skill skill = Engine.GetSkill(card.Name);
                        if (skill != null)
                        {
                            ViewAsSkill vs = ViewAsSkill.ParseViewAsSkill(skill);
                            if (vs != null && vs.IsAvailable(room, player, reason, pattern))
                            {
                                if (available_equip_skills.ContainsKey(player.Name))
                                    available_equip_skills[player.Name].Add(skill.Name);
                                else
                                    available_equip_skills[player.Name] = new List<string> { skill.Name };
                            }
                        }
                    }
                    //耦合双雄断肠 主
                    List<string> heads = player.GetHeadSkillList(true, true);
                    if (player.HasFlag("shuangxiong_head") && !heads.Contains("shuangxiong"))
                        heads.Add("shuangxiong");
                    foreach (string skill in heads)
                    {
                        ViewAsSkill vs = ViewAsSkill.ParseViewAsSkill(skill);
                        if (vs != null && vs.IsAvailable(room, player, reason, pattern, "head"))
                        {
                            if (available_head_skills.ContainsKey(player.Name))
                                available_head_skills[player.Name].Add(skill);
                            else
                                available_head_skills[player.Name] = new List<string> { skill };
                        }
                    }
                    //耦合双雄断肠 副
                    List<string> deputys = player.GetDeputySkillList(true, true);
                    if (player.HasFlag("shuangxiong_deputy") && !deputys.Contains("shuangxiong"))
                        deputys.Add("shuangxiong");
                    foreach (string skill in deputys)
                    {
                        ViewAsSkill vs = ViewAsSkill.ParseViewAsSkill(skill);
                        if (vs != null && vs.IsAvailable(room, player, reason, pattern, "deputy"))
                        {
                            if (available_deputy_skills.ContainsKey(player.Name))
                                available_deputy_skills[player.Name].Add(skill);
                            else
                                available_deputy_skills[player.Name] = new List<string> { skill };
                        }
                    }
                    if (player.HasFlag("shuangxiong_deputy") && (!available_deputy_skills.ContainsKey(player.Name) ||
                        !available_deputy_skills[player.Name].Contains("shuangxiong")) && ExpectedReplyCommand == CommandType.S_COMMAND_PLAY_CARD)
                    {
                        if (available_deputy_skills.ContainsKey(player.Name))
                            available_deputy_skills[player.Name].Add("shuangxiong");
                        else
                            available_deputy_skills[player.Name] = new List<string> { "shuangxiong" };
                    }
                }
            }

            //pre active skill
            PromoteStruct promote = promote_skill;
            promote_skill = new PromoteStruct();
            Player promoter = room.FindPlayer(promote.Name);
            if (promoter != null && !skill_invoke && pending_skill == null && !string.IsNullOrEmpty(promote.SkillName) && room.GetPlayers(this).Contains(promoter)
                    && room.GetRoomState().GetCurrentCardUsePattern(promoter) == promote.Pattern && reason == promote.Reason)
            {
                ViewAsSkill pro_skill = Engine.GetViewAsSkill(promote.SkillName);
                if (pro_skill != null && pro_skill.IsAvailable(room, promoter, promote.Reason, room.GetRoomState().GetCurrentCardUsePattern(promoter), promote.SkillPosition))
                {
                    pending_skill = pro_skill;
                    skill_position = promote.SkillPosition;
                }
            }

            if (pending_skill != null)
            {
                StartPending(requestor);
            }
            else
            {
                pending_skill = null;
                foreach (Player player in requestors) {
                    all_cards[player] = new List<WrappedCard>(hand_cards[player]);
                    all_cards[player].AddRange(equip_cards[player]);
                    prepends[player.Name] = new List<string>();
                    if (ExpectedReplyCommand == CommandType.S_COMMAND_PLAY_CARD || method == HandlingMethod.MethodResponse || method == HandlingMethod.MethodUse)
                    {
                        foreach (string pile in player.GetHandPileList(false)) {
                            List <WrappedCard> cards = new List<WrappedCard>();
                            List<int> ids = new List<int>();
                            foreach (int id in player.GetPile(pile)) {
                                cards.Add(room.GetCard(id));
                                ids.Add(id);
                            }
                            all_cards[player].AddRange(cards);
                            if (ids.Count > 0)
                                prepends[player.Name].Add(string.Format("{0}:{1}", pile, string.Join("+", JsonUntity.IntList2StringList(ids))));
                        }
                    }
                    available_cards[player.Name] = new List<WrappedCard>();
                    foreach (WrappedCard card in all_cards[player])
                        if (CheckCardAvailable(player, card))
                            available_cards[player.Name].Add(card);

                    #region new transfer card
                    if (ExpectedReplyCommand == CommandType.S_COMMAND_PLAY_CARD
                        && Engine.GetViewAsSkill("transfer").IsAvailable(room, player, room.GetRoomState().GetCurrentCardUseReason(), string.Empty))
                    {
                        if (available_head_skills.ContainsKey(player.Name))
                            available_head_skills[player.Name].Add("transfer");
                        else
                            available_head_skills[player.Name] = new List<string> { "transfer" };
                        foreach (WrappedCard card in hand_cards[player])
                        {
                            if (card.Transferable)
                            {
                                WrappedCard transfer = new WrappedCard(TransferCard.ClassName)
                                {
                                    Skill = "transfer",
                                    Mute = true
                                };
                                transfer.AddSubCard(card.Id);
                                available_cards[player.Name].Add(transfer);
                            }
                        }
                    }
                    #endregion

                    #region old transfer card
                    /*
                    if (ExpectedReplyCommand == CommandType.S_COMMAND_PLAY_CARD)
                    {
                        foreach (WrappedCard card in hand_cards[player])
                        {
                            if (card.Transferable)
                            {
                                WrappedCard transfer = new WrappedCard(TransferCard.ClassName)
                                {
                                    Skill = "transfer",
                                    Mute = true
                                };
                                transfer.AddSubCard(card.Id);
                                available_cards[player.Name].Add(transfer);
                            }
                        }
                    }
                    */
                    #endregion
                }

                if (intel_select && ExpectedReplyCommand != CommandType.S_COMMAND_PLAY_CARD && available_cards.Count > 0       //auto select intel card
                        && (ExpectedReplyCommand == CommandType.S_COMMAND_RESPONSE_CARD
                            || (m_requestResponsePair.ContainsKey(ExpectedReplyCommand) && m_requestResponsePair[ExpectedReplyCommand] == CommandType.S_COMMAND_RESPONSE_CARD)))
                {
                    foreach (string name in available_cards.Keys) {
                        Player p = room.FindPlayer(name);
                        if (available_cards[name].Count > 0)
                        {
                            WrappedCard auto_select = available_cards[name][0];
                            selected_cards[p] = new List<WrappedCard> { auto_select };
                            viewas_card = auto_select;
                            EnableTargets(p);
                            break;
                        }
                    }
                }
                else
                {
                    GetPacket2Client(false);
                }
            }
        }

        private void StartPending(Player player)
        {
            prepends.Clear();
            appends.Clear();
            guhuo_cards.Clear();
            selected_guhuo = null;

            string equip_skiil = null;
            if (available_equip_skills.ContainsKey(player.Name) && available_equip_skills[player.Name].Contains(pending_skill.Name))
                equip_skiil = pending_skill.Name;
            available_equip_skills.Clear();
            if (!string.IsNullOrEmpty(equip_skiil))
                available_equip_skills[player.Name] = new List<string> { equip_skiil };

            available_targets.Clear();
            selected_cards.Clear();
            selected_targets.Clear();
            ok_enable = false;
            if (skill_invoke)
                cancel_enable = cancel_able;
            else
                cancel_enable = true;

            skill_owner = player;
            all_cards[player] = new List<WrappedCard>();
            if (hand_cards.ContainsKey(player))
                all_cards[player].AddRange(hand_cards[player]);
            if (equip_cards.ContainsKey(player))
                all_cards[player].AddRange(equip_cards[player]);

            //if (pending_skill.Name == "huashen")
            //    return PendingHuashen(player);

            if (intel_select)
                first_pending = true;
            prepends[player.Name] = new List<string>();
            appends[player.Name] = new List<string>();
            bool expand = pending_skill.IsResponseOrUse();
            if (expand)
            {
                foreach (string pile in player.GetHandPileList(false)) {
                    List<WrappedCard> cards = new List<WrappedCard>();
                    List<int> ids = new List<int>();
                    foreach (int id in player.GetPile(pile)) {
                        cards.Add(room.GetCard(id));
                        ids.Add(id);
                    }
                    all_cards[player].AddRange(cards);
                    if (ids.Count > 0)
                        prepends[player.Name].Add(string.Format("{0}:{1}", pile, string.Join("+", JsonUntity.IntList2StringList(ids))));
                }
            }
            else
            {
                if (!string.IsNullOrEmpty(pending_skill.GetExpandPile()))
                {
                    foreach (string pile_name in pending_skill.GetExpandPile().Split(',')) {
                        string new_name = pile_name;
                        List<int> pile = new List<int>();
                        if (new_name.StartsWith("%"))
                        {
                            new_name = new_name.Substring(1);
                            foreach (Player p in room.GetAlivePlayers())
                                pile.AddRange(p.GetPile(new_name));
                        }
                        else
                        {
                            pile = player.GetPile(new_name);
                        }
                        List<WrappedCard> cards = new List<WrappedCard>();
                        foreach (int id in pile)
                            cards.Add(room.GetCard(id));

                        all_cards[player].AddRange(cards);
                        if (pile.Count > 0)
                        {
                            appends[player.Name].Add(string.Format("{0}:{1}", new_name, string.Join("+", JsonUntity.IntList2StringList(pile))));
                        }
                    }
                }
            }

            #region new transfer card
            if (pending_skill.Name == "transfer")
            {
                selected_cards[player] = new List<WrappedCard> { initial_transfer_card };
                initial_transfer_card = null;
            }
            #endregion

            UpdatePending();
        }

        private void UpdatePending(WrappedCard guhuo_card = null)
        {
            if (pending_skill == null || skill_owner == null)
            {
                GetPacket2Client(false);
                return;
            }

            available_cards.Clear();
            Player player = skill_owner;
            WrappedCard viewascard = null;
            if (this.guhuo_cards.Count == 0)
            {
                this.guhuo_cards.Clear();
                selected_guhuo = null;
                List<WrappedCard> pending_cards = new List<WrappedCard>(all_cards[player]);
                List<WrappedCard> selecteds = selected_cards.ContainsKey(player) ? new List<WrappedCard>(selected_cards[player]) : new List<WrappedCard>();
                List<WrappedCard> available = new List<WrappedCard>();
                foreach (WrappedCard card1 in all_cards[player]) {
                    foreach (WrappedCard card2 in selecteds) {
                        if (card1.Equals(card2))
                        {
                            pending_cards.Remove(card1);
                            break;
                        }
                    }
                }

                foreach (WrappedCard card in pending_cards)
                    if (!card.HasFlag("using") && pending_skill.ViewFilter(room, selecteds, card, player))
                        available.Add(card);

                if (available.Count == 0 && selecteds.Count > 0)
                {
                    selecteds.Remove(selecteds[selecteds.Count - 1]);
                    foreach (WrappedCard card in pending_cards)
                    {
                        if (!card.HasFlag("using") && pending_skill.ViewFilter(room, selecteds, card, player))
                        {
                            if (available_cards.ContainsKey(player.Name))
                                available_cards[player.Name].Add(card);
                            else
                                available_cards[player.Name] = new List<WrappedCard> { card };
                        }
                    }
                }
                else
                    available_cards[player.Name] = available;

                if (first_pending && available_cards[player.Name].Count > 0                       //auto select intel card for pending skill
                    && (pending_skill is OneCardViewAsSkill) && (!selected_cards.ContainsKey(player) || selected_cards[player].Count == 0))
                {
                    selecteds.Clear();
                    selecteds.Add(available_cards[player.Name][0]);
                    if (pending_skill.GetGuhuoCards(room, selecteds, player).Count == 0)
                        selected_cards[player] = selecteds;
                }
                first_pending = false;

                List<WrappedCard> guhuo_cards = pending_skill.GetGuhuoCards(room, selected_cards.ContainsKey(player) ? selected_cards[player] : new List<WrappedCard>(), player);
                if (guhuo_cards.Count > 0)
                {
                    viewas_card = null;
                    ok_enable = false;

                    selected_targets.Clear();
                    if (pending_skill.GetGuhuoType() == ViewAsSkill.GuhuoType.VirtualCard)
                        available_cards.Clear();
                    foreach (WrappedCard card in guhuo_cards)
                    {
                        this.guhuo_cards.Add(card);
                        if (pending_skill.GetGuhuoType() == ViewAsSkill.GuhuoType.VirtualCard && CheckCardAvailable(player, card))
                        {
                            if (available_cards.ContainsKey(player.Name))
                                available_cards[player.Name].Add(card);
                            else
                                available_cards[player.Name] = new List<WrappedCard> { card };
                        }
                    }

                    GetPacket2Client(false);
                    return;
                }
                List<WrappedCard> new_selected = selected_cards.ContainsKey(player) ? selected_cards[player] : new List<WrappedCard>();
                viewascard = pending_skill.ViewAs(room, new_selected, player);
            }
            else if (pending_skill.GetGuhuoType() == ViewAsSkill.GuhuoType.PopUpBox)
            {
                List<WrappedCard> pending_cards = new List<WrappedCard>(all_cards[player]);
                List<WrappedCard> selecteds = selected_cards.ContainsKey(player) ? new List<WrappedCard>(selected_cards[player]) : new List<WrappedCard>();
                List<WrappedCard> available = new List<WrappedCard>();
                foreach (WrappedCard card1 in all_cards[player])
                {
                    foreach (WrappedCard card2 in selecteds)
                    {
                        if (card1.Equals(card2))
                        {
                            pending_cards.Remove(card1);
                            break;
                        }
                    }
                }
                
                foreach (WrappedCard card in pending_cards)
                    if (!card.HasFlag("using") && pending_skill.ViewFilter(room, selecteds, card, player))
                        available.Add(card);

                if (available.Count == 0 && selecteds.Count > 0)
                {
                    selecteds.Remove(selecteds[selecteds.Count - 1]);
                    foreach (WrappedCard card in pending_cards)
                    {
                        if (!card.HasFlag("using") && pending_skill.ViewFilter(room, selecteds, card, player))
                        {
                            if (available_cards.ContainsKey(player.Name))
                                available_cards[player.Name].Add(card);
                            else
                                available_cards[player.Name] = new List<WrappedCard> { card };
                        }
                    }
                }
                else
                    available_cards[player.Name] = available;

                List<WrappedCard> guhuo_cards = pending_skill.GetGuhuoCards(room, selected_cards.ContainsKey(player) ? selected_cards[player] : new List<WrappedCard>(), player);
                if (selected_guhuo != null)
                {
                    viewas_card = null;
                    ok_enable = false;

                    selected_targets.Clear();
                    foreach (WrappedCard card in guhuo_cards)
                    {
                        if (selected_guhuo.Name == card.Name)
                        {
                            selected_guhuo = card;
                            break;
                        }
                    }

                    viewascard = pending_skill.ViewAs(room, new List<WrappedCard> { selected_guhuo }, player);
                }
                else
                {
                    GetPacket2Client(false);
                    return;
                }
            }
            else
            {
                available_cards[player.Name] = new List<WrappedCard>();
                foreach (WrappedCard card in guhuo_cards)
                    if (CheckCardAvailable(player, card))
                        available_cards[player.Name].Add(card);

                if (guhuo_card != null)
                    viewascard = pending_skill.ViewAs(room, new List<WrappedCard> { guhuo_card }, player);
            }

            if ((viewascard != null || viewas_card != null) && (viewascard == null || viewas_card == null || !viewascard.Equals(this.viewas_card)))
            {
                selected_targets.Clear();
            }
            if (viewascard != null)
                viewascard.SkillPosition = skill_position;

            viewas_card = viewascard;
            EnableTargets(player);
        }
        private bool CheckCardAvailable(Player player, WrappedCard card)
        {
            FunctionCard fcard = Engine.GetFunctionCard(card?.Name);
            if (player == null || fcard == null || card.HasFlag("using")) return false;

            bool ok_enable = true;
            if ((ExpectedReplyCommand == CommandType.S_COMMAND_PLAY_CARD || method == HandlingMethod.MethodUse) && !fcard.IsAvailable(room, player, card))
            {
                ok_enable = false;
            }

            string pattern = room.GetRoomState().GetCurrentCardUsePattern(player);
            if (ok_enable && !string.IsNullOrEmpty(pattern) && ExpectedReplyCommand != CommandType.S_COMMAND_PLAY_CARD && (ExpectedReplyCommand == CommandType.S_COMMAND_RESPONSE_CARD
                    || (m_requestResponsePair.ContainsKey(ExpectedReplyCommand) && m_requestResponsePair[ExpectedReplyCommand] == CommandType.S_COMMAND_RESPONSE_CARD)))
            {
                if (RoomLogic.IsCardLimited(room, player, card, method))
                {
                    ok_enable = false;
                }
                else if (!skill_invoke && fcard.TypeID != CardType.TypeSkill)
                {
                    if (pattern.EndsWith("!")) pattern = pattern.Substring(0, pattern.Length - 1);
                    pattern = Engine.GetPattern(pattern).GetPatternString();
                    if ((method == HandlingMethod.MethodResponse || method == HandlingMethod.MethodUse) && pattern.Contains("hand"))
                        pattern = pattern.Replace("hand", string.Join(",", player.GetHandPileList()));
                    ExpPattern p = (ExpPattern)Engine.GetPattern(pattern);
                    if (!p.Match(player, room, card))
                    {
                        ok_enable = false;
                    }
                }
            }

            return ok_enable;
        }
        private void EnableTargets(Player player)
        {
            available_targets.Clear();
            if (viewas_card == null)
            {
                room.DoNotify(this, CommandType.S_COMMAND_LOG_EVENT, new List<string> { GameEventType.S_GAME_EVENT_CLIENT_TIP.ToString(), false.ToString()});
                ok_enable = false;
                GetPacket2Client(false);
                return;
            }

            ok_enable = CheckCardAvailable(player, viewas_card);
            if (!ok_enable)
            {
                selected_targets.Clear();
                GetPacket2Client(false);
                return;
            }
            FunctionCard fcard = Engine.GetFunctionCard(viewas_card.Name);
            if (!skill_invoke && fcard?.TargetFixed(viewas_card) == true && pending_skill != null && !m_do_request
                    && (!selected_cards.ContainsKey(skill_owner) || selected_cards[skill_owner].Count == 0) && viewas_card.SubCards.Count == 0)
            {
                selected_targets.Clear();
                Reply2Server(true, player);
                return;
            }

            if (fcard.TypeID == CardType.TypeSkill || ExpectedReplyCommand == CommandType.S_COMMAND_PLAY_CARD || method == HandlingMethod.MethodUse)
            {
                ok_enable = fcard.TargetFixed(viewas_card);
                if (!ok_enable)
                {
                    bool check = true;
                    List<Player> targets = new List<Player>();
                    if (selected_targets.Count > 0)
                    {
                        foreach (Player p in selected_targets)
                        {
                            if (fcard.TargetFilter(room, targets, p, player, viewas_card))
                                targets.Add(p);
                            else
                            {
                                check = false;
                                break;
                            }
                        }
                    }

                    if (!check)
                    {
                        selected_targets.Clear();
                        targets.Clear();
                    }

                    List<Player> check_available = new List<Player>(targets);
                    List<Player> available = new List<Player>();
                    foreach (Player p in room.GetAlivePlayers())
                        if (fcard.TargetFilter(room, check_available, p, player, viewas_card))
                            available.Add(p);

                    if (available.Count == 0 && check_available.Count > 0)
                    {
                        check_available.Remove(check_available[check_available.Count - 1]);
                        foreach (Player p in room.GetAlivePlayers())
                            if (fcard.TargetFilter(room, check_available, p, player, viewas_card))
                                available_targets.Add(p);
                    }
                    else
                        available_targets = available;

                    room.DoNotify(this, CommandType.S_COMMAND_LOG_EVENT, new List<string> { GameEventType.S_GAME_EVENT_CLIENT_TIP.ToString(), false.ToString() });
                    //show tip for liegong
                    if (fcard is Slash && RoomLogic.PlayerHasSkill(room, player, "liegong") && player.Phase == Player.PlayerPhase.Play)
                    {
                        bool weapon = player.Weapon.Key != -1 && !viewas_card.SubCards.Contains(player.Weapon.Key);
                        List<string> liegong_targets = new List<string>();
                        foreach (Player p in available_targets)
                            if (p.HandcardNum >= player.Hp || p.HandcardNum <= RoomLogic.GetAttackRange(room, player, weapon))
                                liegong_targets.Add(p.Name);

                        if (liegong_targets.Count > 0)
                            room.DoNotify(this, CommandType.S_COMMAND_LOG_EVENT, new List<string>{ GameEventType.S_GAME_EVENT_CLIENT_TIP.ToString(), true.ToString(),
                            "liegong", JsonUntity.Object2Json(liegong_targets) });
                    }

                    if (first_selection && selected_targets.Count == 0 && fcard is Slash && player.HasFlag("slashTargetFixToOne"))
                    {
                        foreach (Player p in available_targets)
                        {
                            if (p.HasFlag("SlashAssignee"))
                            {
                                selected_targets.Add(p);
                                targets.Add(p);
                                break;
                            }
                        }
                    }

                    //player chain animation
                    if (first_selection)
                    {
                        bool chain_animation = false;
                        if (fcard is FireSlash || fcard is ThunderSlash || fcard is FireAttack)
                        {
                            foreach (Player p in available_targets)
                            {
                                if (p.Chained)
                                {
                                    chain_animation = true;
                                    break;
                                }
                            }
                        }
                        else if (fcard is BurningCamps)
                        {
                            List<Player> players = RoomLogic.GetFormation(room, room.GetNextAlive(player));
                            foreach (Player target in players)
                            {
                                if (RoomLogic.IsProhibited(room, player, target, viewas_card) == null && target.Chained)
                                {
                                    chain_animation = true;
                                    break;
                                }
                            }
                        }
                        if (chain_animation)
                            room.NotifyChainedAnimation(player);
                    }

                    first_selection = false;

                    //reserve extra available targets
                    if (!fcard.CanRecast(room, player, viewas_card) && selected_targets.Count == 0 && available_targets.Count == 1 && auto_target)
                    {
                        selected_targets.Add(available_targets[0]);
                        targets.Add(available_targets[0]);
                        available_targets.Clear();
                        foreach (Player p in room.GetAlivePlayers())
                            if (fcard.TargetFilter(room, targets, p, player, viewas_card))
                                available_targets.Add(p);
                    }
                    auto_target = false;

                    //show tip for lijian
                    if (fcard is LijianCard && selected_targets.Count > 0)
                    {
                        List<string> lijian_targets = new List<string>();
                        foreach (Player p in selected_targets)
                            lijian_targets.Add(p.Name);

                        room.DoNotify(this, CommandType.S_COMMAND_LOG_EVENT, new List<string>{ GameEventType.S_GAME_EVENT_CLIENT_TIP.ToString(), true.ToString(),
                        "lijian", JsonUntity.Object2Json(lijian_targets) });
                    }

                    if (fcard.TargetsFeasible(room, targets, player, viewas_card))
                        ok_enable = true;
                }
            }
            else
                selected_targets.Clear();

            GetPacket2Client(false);
        }

        Operate GetPacket2Client(bool first_time, string prompt = null, int notice_index = -1)
        {
            Operate args = new Operate();
            if (m_do_request && !first_time) return args;

            args.Request = first_time;
            args.Operator = requestor?.Name;
            args.Prompt = prompt;
            args.NoticeIndex = notice_index;
            args.SkillInvoke = skill_invoke;
            
            List<string> hightlight_skills = new List<string>();
            if (pending_skill != null)
                hightlight_skills.Add(pending_skill.Name);
            else if (viewas_card != null && viewas_card.Name == TransferCard.ClassName)
                hightlight_skills.Add("transfer");

            if (!string.IsNullOrEmpty(hightlight_skill))
                hightlight_skills.Add(hightlight_skill);
            args.HighLightSkills = hightlight_skills;

            args.SkillPosition = skill_position;
            args.SkillOwner = skill_owner?.Name;

            List<string> available_names = new List<string>(), selected_names = new List<string>();
            foreach (Player p in available_targets)
                available_names.Add(p.Name);
            foreach (Player p in selected_targets)
                selected_names.Add(p.Name);
            args.AvailableTargets = available_names;
            args.SelectedTargets = selected_names;

            args.OKEnable = ok_enable;
            args.CancelEnable = cancel_enable;

            args.GuhuoCards = new List<string>();
            foreach (WrappedCard card in guhuo_cards)
                args.GuhuoCards.Add(RoomLogic.CardToString(room, card));
            args.Guhuo = selected_guhuo?.Name;
            if (guhuo_cards.Count > 0)
                args.GuhuoType = (int)pending_skill.GetGuhuoType();
            
            if (pending_skill != null && pending_skill.Name == "yigui" && (!selected_cards.ContainsKey(skill_owner) || selected_cards[skill_owner].Count == 0))
                args.ExInfo = (List<string>)skill_owner.GetTag("spirit");
            else
                args.ExInfo = ex_information;

            args.AvailableEquip = available_equip_skills;
            args.AvailableHead = available_head_skills;
            args.AvailableDeputy = available_deputy_skills;
            args.AvailableCards = new Dictionary<string, List<string>>();
            foreach (string name in available_cards.Keys)
            {
                args.AvailableCards[name] = new List<string>();
                foreach (WrappedCard card in available_cards[name])
                {
                    if (guhuo_cards.Contains(card))
                        args.AvailableCards[name].Add(card.Name);
                    else if (card.Name == TransferCard.ClassName)
                        args.AvailableCards[name].Add(RoomLogic.CardToString(room, card));
                    else
                        args.AvailableCards[name].Add(card.Id.ToString());

                }
            }
            args.SelectedCards = new Dictionary<string, List<string>>();
            foreach (Player player in selected_cards.Keys)
            {
                args.SelectedCards[player.Name] = new List<string>();
                foreach (WrappedCard card in selected_cards[player])
                {
                    if (!guhuo_cards.Contains(card))
                        args.SelectedCards[player.Name].Add(card.Id.ToString());
                    else
                        args.SelectedCards[player.Name].Add(card.Name);
                }
            }

            args.PrependPile = prepends;
            args.AppendPile = appends;

            if (!first_time)
            {
                room.DoNotify(this, CommandType.S_COMMAND_OPERATE, new List<string> { JsonUntity.Object2Json(args) });
            }

            return args;
        }

        private void Reply2Server(bool reply, Player player = null)
        {
            CommandType type = ExpectedReplyCommand;
            if (m_requestResponsePair.ContainsKey(type))
                type = m_requestResponsePair[type];

            room.DoNotify(this, CommandType.S_COMMAND_UNKNOWN, new List<string> { true.ToString() });
            List<string> packet = new List<string> { ExpectedReplyCommand.ToString() };

            if (reply)
            {
                if (type == CommandType.S_COMMAND_RESPONSE_CARD && viewas_card != null && player != null)
                {
                    if (ExpectedReplyCommand == CommandType.S_COMMAND_PINDIAN)
                    {
                        room.DoBroadcastNotify(ExpectedReplyCommand, new List<string> { GuanxingStep.S_GUANXING_MOVE.ToString(), player.Name, "-1" }, this);
                        room.DoNotify(this, ExpectedReplyCommand, new List<string> { GuanxingStep.S_GUANXING_MOVE.ToString(), player.Name, viewas_card.Id.ToString() });
                    }

                    List<string> targetNames = new List<string>();
                    FunctionCard fcard = Engine.GetFunctionCard(viewas_card.Name);
                    if (fcard?.TargetFixed(viewas_card) == false)
                    {
                        foreach (Player target in selected_targets)
                            targetNames.Add(target.Name);
                    }

                    string card_str = RoomLogic.CardToString(room, viewas_card);

                    packet.Add(player.Name);
                    packet.Add(card_str);
                    packet.Add(JsonUntity.Object2Json(targetNames));
                }
                else if (type == CommandType.S_COMMAND_DISCARD_CARD && viewas_card != null)
                {
                    packet.Add(JsonUntity.Object2Json(JsonUntity.IntList2StringList(viewas_card.SubCards)));
                }
                else if (type == CommandType.S_COMMAND_CHOOSE_PLAYER)
                {
                    List<string> names = new List<string>();
                    foreach (Player p in selected_targets)
                        names.Add(p.Name);

                    packet.Add(JsonUntity.Object2Json(names));
                }
                else if (type == CommandType.S_COMMAND_SKILL_YIJI && viewas_card != null)
                {
                    packet.Add(JsonUntity.Object2Json(viewas_card.SubCards));
                    packet.Add(selected_targets[0].Name);
                }
                else if (type == CommandType.S_COMMAND_CHOOSE_EXTRA_TARGET && selected_targets.Count > 0)
                {
                    List<string> names = new List<string>();
                    foreach (Player p in selected_targets)
                        names.Add(p.Name);
                    packet.Add(JsonUntity.Object2Json(names));
                }
                else if (type == CommandType.S_COMMAND_SKILL_MOVECARDS)
                {
                    packet.Add(JsonUntity.Object2Json(guanxing));
                }
            }

            viewas_card = null;
            guhuo_cards.Clear();
            pending_skill = null;
            selected_guhuo = null;

            //mutex.ReleaseMutex();
            MyData data = new MyData
            {
                Protocol = protocol.GameReply,
                Description = PacketDescription.Client2Room,
                Body = packet
            };
            GameControl?.Invoke(this, data);
        }

        private void OnGuanxingRespond(List<string> args)
        {
            mutex.WaitOne();
            RequestType type = (RequestType)Enum.Parse(typeof(RequestType), args[0]);
            bool error = false;

            if (type == RequestType.S_REQUEST_MOVECARDS && args.Count == 3)
            {
                List<int> ups = guanxing.Top, downs = guanxing.Bottom;

                int from = int.Parse(args[1]);
                int to = int.Parse(args[2]);
                if (from == 0 || to == 0)
                {
                    error = true;
                }
                else
                {
                    int card = -1;
                    if (from > 0)
                    {
                        from = from - 1;
                        if (available_cards[requestor.Name].Contains(room.GetCard(ups[from])))
                        {
                            card = ups[from];
                            ups.Remove(card);
                        }
                    }
                    else
                    {
                        from = -from - 1;

                        if (available_cards[requestor.Name].Contains(room.GetCard(downs[from])))
                        {
                            card = downs[from];
                            downs.Remove(card);
                        }
                    }

                    if (card > -1)
                    {
                        if (to > 0)
                        {
                            to = to - 1;
                            if (to > ups.Count)
                                ups.Add(card);
                            else
                                ups.Insert(to, card);
                        }
                        else
                        {
                            to = -to - 1;
                            if (to > downs.Count)
                                downs.Add(card);
                            else
                                downs.Insert(to, card);
                        }

                        CheckMoveCards();
                        GetPacket2Client(false);
                        List<string> arg = new List<string> { GuanxingStep.S_GUANXING_MOVE.ToString(), args[1], args[2] };
                        room.DoBroadcastNotify(CommandType.S_COMMAND_MIRROR_MOVECARDS_STEP, arg, this);
                    }
                    else
                    {
                        error = true;
                    }
                }
            }
            else if (type == RequestType.S_REQUEST_SYS_BUTTON && args.Count == 2)
            {
                bool confirm = bool.Parse(args[1]);
                if ((confirm && ok_enable) || (!confirm && cancel_able))
                    Reply2Server(confirm);
                else
                    error = true;
            }
            else
            {
                error = true;
            }

            if (error)
            {
                room.Debug(string.Format("request type: {0} got error message {1}", ExpectedReplyCommand.ToString(), JsonUntity.Object2Json(args)));
                GetPacket2Client(false);
            }

            mutex.ReleaseMutex();
        }

        private void OnChooseExtraResponse(List<string> args)
        {
            mutex.WaitOne();
            RequestType type = (RequestType)Enum.Parse(typeof(RequestType), args[0]);
            bool error = false;
            if (type == RequestType.S_REQUEST_TARGET && args.Count == 2)
            {
                Player target = room.FindPlayer(args[1]);
                if (available_targets.Contains(target) || selected_targets.Contains(target))
                {
                    if (selected_targets.Contains(target))
                    {
                        selected_targets.Remove(target);
                    }
                    else
                    {
                        if (!CheckExtraTarger(target) && selected_targets.Count > 0)
                            selected_targets.RemoveAt(selected_targets.Count - 1);
                        selected_targets.Add(target);
                    }
                    if (selected_targets.Count > 0)
                        ok_enable = true;
                    else
                        ok_enable = false;

                    CheckExtraTarger();
                    GetPacket2Client(false);
                }
                else
                    error = true;
            }
            else if (type == RequestType.S_REQUEST_DOUBLE_CLICK && args.Count == 4)
            {
                Player target = room.FindPlayer(args[3]);
                if (target != null)
                {
                    if (selected_targets.Count == 0 || (selected_targets.Count == 1 && selected_targets.Contains(target)))
                    {
                        selected_targets = new List<Player>() { target };
                        Reply2Server(true);
                    }
                }
                else
                    error = true;
            }
            else if (type == RequestType.S_REQUEST_SYS_BUTTON && args.Count == 2)
            {
                bool ok_button = bool.Parse(args[1]);
                if (ok_button && ok_enable)
                {
                    Reply2Server(true);
                }
                else if (!ok_button && cancel_enable)
                {
                    Reply2Server(false);
                }
                else
                    error = true;
            }

            if (error)
                room.Debug(string.Format("request type: {0} got error message {1}", ExpectedReplyCommand.ToString(), JsonUntity.Object2Json(args)));

            mutex.ReleaseMutex();
        }

        private void OnChoosePlayerResponse(List<string> args)
        {
            mutex.WaitOne();
            RequestType type = (RequestType)Enum.Parse(typeof(RequestType), args[0]);
            bool error = false;
            if (type == RequestType.S_REQUEST_TARGET && args.Count == 2)
            {
                Player target = room.FindPlayer(args[1]);
                if (available_targets.Contains(target))
                {
                    if (selected_targets.Contains(target))
                    {
                        selected_targets.Remove(target);
                    }
                    else
                    {
                        if (selected_targets.Count > max_num)
                            selected_targets.Clear();
                        else if (selected_targets.Count == max_num)
                            selected_targets.RemoveAt(selected_targets.Count - 1);

                        selected_targets.Add(target);
                    }
                    if (selected_targets.Count <= max_num && selected_targets.Count >= min_num && selected_targets.Count > 0)
                        ok_enable = true;
                    else
                        ok_enable = false;

                    GetPacket2Client(false);
                }
                else
                    error = true;
            }
            else if (type == RequestType.S_REQUEST_SYS_BUTTON && args.Count == 2)
            {
                bool ok_button = bool.Parse(args[1]);
                if (ok_button && ok_enable)
                {
                    Reply2Server(true);
                }
                else if (!ok_button && cancel_enable)
                {
                    Reply2Server(false);
                }
                else
                    error = true;
            }
            else if (type == RequestType.S_REQUEST_DOUBLE_CLICK && args.Count == 4)
            {
                Player target = room.FindPlayer(args[3]);
                if (target != null)
                {
                    if (min_num <= 1 && (selected_targets.Count == 0 || (selected_targets.Count == 1 && selected_targets.Contains(target))))
                    {
                        selected_targets = new List<Player>{ target};
                        Reply2Server(true);
                    }
                }
                else
                    error = true;
            }

            if (error)
                room.Debug(string.Format("request type: {0} got error message {1}", ExpectedReplyCommand.ToString(), JsonUntity.Object2Json(args)));

            mutex.ReleaseMutex();
        }

        WrappedCard GetSelected(Player player, WrappedCard card)
        {
            if (selected_cards.ContainsKey(player))
            {
                return selected_cards[player].Find(t => (!RoomLogic.IsVirtualCard(room, t) && t.Id == card.Id || t.Equals(card)));
            }
            return null;
        }

        private void OnPlayCardRespond(List<string> args)
        {
            mutex.WaitOne();
            bool success = false;
            
            RequestType type = (RequestType)Enum.Parse(typeof(RequestType), args[0]);

            if (type == RequestType.S_REQUEST_CARD && args.Count == 4)
            {
                Player player = room.FindPlayer(args[1]);
                WrappedCard card = RoomLogic.ParseCard(room, args[2]);
                auto_target = bool.Parse(args[3]);

                bool guhuo_check = false;
                if (card != null && RoomLogic.IsVirtualCard(room, card) && pending_skill != null && guhuo_cards.Count > 0)
                {
                    foreach (WrappedCard guhuo in guhuo_cards)
                    {
                        string str = RoomLogic.CardToString(room, guhuo);
                        if (str == args[2] || (pending_skill.GetGuhuoType() == ViewAsSkill.GuhuoType.PopUpBox && card.Name == guhuo.Name))
                        {
                            card = guhuo;
                            guhuo_check = true;
                            break;
                        }
                    }
                }

                #region old transfer card
                /*
                else if (card.Name == TransferCard.ClassName && available_cards.ContainsKey(args[1]))
                {
                    card = available_cards[args[1]].Find(t => t.Name == TransferCard.ClassName && t.GetEffectiveId() == card.GetEffectiveId());
                }
                */
                #endregion
                if (player == null)
                    room.Debug(args[1] + " is null " + room.GetAlivePlayers().Count.ToString());

                //huashen
                bool huashen = false;
                if (pending_skill != null && pending_skill.Name == "yigui" && !guhuo_check && card == null && Engine.GetGeneral(args[2], room.Setting.GameMode) != null)
                {
                    huashen = HandleHuashen(skill_owner, args[2]);
                    success = huashen;
                }

                if (!huashen && player != null && card != null && (pending_skill == null || skill_owner == player)
                    && ((available_cards.ContainsKey(player.Name) && available_cards[player.Name].Contains(card))
                    || ((guhuo_cards.Count == 0 || pending_skill.GetGuhuoType() == ViewAsSkill.GuhuoType.PopUpBox) && GetSelected(player, card) != null)
                    || (guhuo_check && pending_skill.GetGuhuoType() == ViewAsSkill.GuhuoType.PopUpBox)))
                {
                    success = true;
                    CardClicked(player, card);
                }
            }
            else if (type == RequestType.S_REQUEST_DOUBLE_CLICK && args.Count == 4)
            {
                Player player = room.FindPlayer(args[1]);
                WrappedCard card = JsonUntity.Json2Object<WrappedCard>(args[2]);
                Player target = room.FindPlayer(args[3]);
                auto_target = false;
                double_click = true;

                if (player != null && card != null && (pending_skill == null || skill_owner == player)
                        && ((available_cards.ContainsKey(player.Name) && (available_cards[player.Name].Contains(card) || available_cards[player.Name].Contains(room.GetCard(card.Id))))
                        || (guhuo_cards.Count == 0 && GetSelected(player, card) != null)))
                {
                    success = true;
                    double_click = true;
                    CardClicked(player, card);
                }
                else if (target != null)
                {
                    if (pending_skill != null)
                    {
                        player = skill_owner;
                    }
                    else
                    {
                        foreach (Player p in selected_cards.Keys) {
                            if (selected_cards[p].Count > 0)
                            {
                                player = p;
                                break;
                            }
                        }
                    }
                    if (player != null && viewas_card != null && target != null && (available_targets.Contains(target) || selected_targets.Contains(target)))
                    {
                        FunctionCard fcard = Engine.GetFunctionCard(viewas_card.Name);
                        if (!fcard.TargetFixed(viewas_card))
                        {
                            success = true;
                            double_click = true;
                            TargetClicked(player, target);
                        }
                    }
                }
            }
            else if (type == RequestType.S_REQUEST_SKILL && args.Count == 5)
            {
                Player player = room.FindPlayer(args[1]);
                string skill_name = args[2];

                #region new transfer card
                const string rx_pattern = @"transfer(\d+)";
                Match result = Regex.Match(skill_name, rx_pattern);
                if (result.Success && result.Length > 0)
                {
                    skill_name = "transfer";

                    if (pending_skill == null || pending_skill.Name != skill_name)
                    {
                        int id = int.Parse(result.Groups[1].ToString());
                        initial_transfer_card = room.GetCard(id);
                    }
                }
                #endregion

                ViewAsSkill skill = Engine.GetViewAsSkill(skill_name);
                string position = args[3];
                auto_target = bool.Parse(args[4]);

                string pattern = room.GetRoomState().GetCurrentCardUsePattern(player);
                if (player != null && skill != null && skill.IsAvailable(room, player, room.GetRoomState().GetCurrentCardUseReason(), pattern, position)
                        && ((available_equip_skills.ContainsKey(player.Name) && available_equip_skills[player.Name].Contains(skill_name))
                        || (position == "head" && available_head_skills.ContainsKey(player.Name) && available_head_skills[player.Name].Contains(skill_name))
                        || (position == "deputy" && available_deputy_skills.ContainsKey(player.Name) && available_deputy_skills[player.Name].Contains(skill_name))))
                {
                    success = true;
                    SkillClick(player, skill, position);
                }
            }
            else if (type == RequestType.S_REQUEST_TARGET && args.Count == 2)
            {
                Player target = room.FindPlayer(args[1]);
                Player player = null;
                if (pending_skill != null)
                {
                    player = skill_owner;
                }
                else
                {
                    foreach (Player p in selected_cards.Keys) {
                        if (selected_cards[p].Count > 0)
                        {
                            player = p;
                            break;
                        }
                    }
                }
                if (player != null && viewas_card != null && target != null && (available_targets.Contains(target) || selected_targets.Contains(target)))
                {
                    FunctionCard fcard = Engine.GetFunctionCard(viewas_card.Name);
                    if (!fcard.TargetFixed(viewas_card))
                    {
                        success = true;
                        TargetClicked(player, target);
                    }
                }
            }
            else if (type == RequestType.S_REQUEST_SYS_BUTTON && args.Count == 2)
            {
                bool ok_button = bool.Parse(args[1]);
                if (ok_button && ok_enable)
                {
                    Player player = null;
                    if (pending_skill != null)
                    {
                        player = skill_owner;
                    }
                    else
                    {
                        foreach (Player p in selected_cards.Keys) {
                            if (selected_cards[p].Count > 0)
                            {
                                player = p;
                                break;
                            }
                        }
                    }

                    if (player != null)
                    {
                        success = true;
                        //handle discard by steps
                        if (ExpectedReplyCommand == CommandType.S_COMMAND_DISCARD_CARD && !discard_skill.IsFull())
                        {
                            discard_skill.Reserved = new List<int>(viewas_card.SubCards);
                            ex_information = new List<string> { viewas_card.SubCards.Count.ToString() };

                            ClientReply = new List<string> { JsonUntity.Object2Json(viewas_card.SubCards) };

                            selected_cards.Clear();
                            UpdatePending();
                        }
                        else
                            Reply2Server(true, player);
                    }
                }
                else if (!ok_button && cancel_enable)
                {
                    success = true;
                    DoCancelButton();
                }
            }
            else if (type == RequestType.S_REQUEST_SWITCH_CARDS && args.Count == 3)
            {
                //QStringList card_str;
                //JsonUtils::tryParse(args[1], card_str);
                //if (card_str.isEmpty())
                //{
                //    success = true;
                //    selected_cards.Clear();
                //}
                //else if (available_cards.Contains(args[2].toString()))
                //{
                //    bool check = true;
                //    foreach (QString str, card_str) {
                //        if (!available_cards[args[2].toString()].contains(str))
                //        {
                //            check = false;
                //            break;
                //        }
                //    }
                //    ServerPlayer* player = room->findPlayer(args[2].toString());
                //    if (check && (!pending_skill || skill_owner == player))
                //    {
                //        success = true;
                //        selected_cards.clear();
                //        foreach (QString str, card_str)
                //    selected_cards[player] << Card::Parse(str, room);
                //    }
                //}

                if (success)
                {
                    if (pending_skill != null)
                        UpdatePending();
                    else
                        HandleInfos();
                }
            }
            else if (type == RequestType.S_REQUEST_HUASHEN && args.Count == 2)
            {
                string skill = args[1];
                ViewAsSkill vs = Engine.GetViewAsSkill(skill);
                if (vs != null && vs.IsAvailable(room, skill_owner, room.GetRoomState().GetCurrentCardUseReason(), room.GetRoomState().GetCurrentCardUsePattern(skill_owner)))
                {
                    success = true;
                    pending_skill = vs;
                    StartPending(skill_owner);
                }
            }
            else if (type == RequestType.S_REQUEST_DASHBOARD_CHANGE && args.Count == 1)
            {
                success = true;
                if (pending_skill != null && !skill_invoke)
                    DoCancelButton();
                else
                    HandleInfos();
            }

            if (!success)
            {
                room.Debug(string.Format("request type: {0} got error message {1}", ExpectedReplyCommand.ToString(), JsonUntity.Object2Json(args)));
            }
            mutex.ReleaseMutex();
        }

        private bool HandleHuashen(Player requestor, string general)
        {
            List<string> huashens = requestor.ContainsTag("spirit") ? (List<string>)requestor.GetTag("spirit") : new List<string>();
            if (!huashens.Contains(general)) return false;

            WrappedCard card = new WrappedCard(general);
            selected_cards[requestor] = new List<WrappedCard> { card };
            UpdatePending();

            return true;
        }

        private void CardClicked(Player player, WrappedCard card)
        {
            if (double_click)
            {
                double_click = false;
                WrappedCard result = null;
                List <WrappedCard> selected = new List<WrappedCard>();
                if (guhuo_cards.Contains(card))
                {
                    selected = selected_cards[player];
                    result = pending_skill.ViewAs(room, new List<WrappedCard> { card }, player);
                }
                else if (!RoomLogic.IsVirtualCard(room, card))
                {
                    if (pending_skill != null)
                    {
                        selected = selected_cards[player];
                        if (GetSelected(player, card) == null && pending_skill.ViewFilter(room, selected_cards[player], card, player))
                            selected.Add(card);
                        if (pending_skill.GetGuhuoCards(room, selected, player).Count == 0)
                            result = pending_skill.ViewAs(room, selected, player);
                    }
                    else
                    {
                        selected.Add(card);
                        result = card;
                    }
                }

                if (result != null)
                {
                    FunctionCard fcard = Engine.GetFunctionCard(result.Name);
                    //handle discard by steps
                    if (ExpectedReplyCommand == CommandType.S_COMMAND_DISCARD_CARD && !discard_skill.IsFull())
                    {
                        discard_skill.Reserved = new List<int>(result.SubCards);
                        ex_information = new List<string> { result.SubCards.Count.ToString() };
                        selected_cards.Clear();

                        ClientReply = new List<string> { JsonUntity.Object2Json(result.SubCards) };

                        UpdatePending();
                    }
                    else
                    {
                        bool check_targetfix = true;
                        if ((ExpectedReplyCommand == CommandType.S_COMMAND_PLAY_CARD || method == HandlingMethod.MethodUse)
                            && !fcard.TargetFixed(result) && !fcard.CanRecast(room, player, result))
                            check_targetfix = false;

                        if (check_targetfix)
                        {
                            selected_targets.Clear();
                            selected_cards.Clear();
                            selected_cards[player] = selected;
                            viewas_card = result;
                            Reply2Server(true, player);
                        }
                    }
                }
                return;
            }

            first_selection = true;
            List <WrappedCard> old_selected = selected_cards.ContainsKey(player) ? new List<WrappedCard>(selected_cards[player]) : new List<WrappedCard>();
            selected_cards.Clear();
            selected_cards[player] = old_selected;

            if (guhuo_cards.Contains(card))
            {
                if (selected_guhuo == null || !selected_guhuo.Equals(card))
                {
                    selected_guhuo = card;
                    UpdatePending(card);
                }
                else
                {
                    selected_guhuo = null;
                    UpdatePending();
                }
            }
            #region old tranfer card
            /*
            else if (RoomLogic.IsVirtualCard(room, card) && card.Name == TransferCard.ClassName)
            {
                selected_cards[player] = new List<WrappedCard> { room.GetCard(card.GetEffectiveId()) };
                viewas_card = card;
                EnableTargets(player);
            }
            */
            #endregion
            else
            {
                if (pending_skill != null)
                {
                    WrappedCard selected = GetSelected(player, card);
                    if (selected != null)
                    {
                        selected_cards[player].Remove(selected);
                    }
                    else
                    {
                        if (!pending_skill.ViewFilter(room, selected_cards[player], card, player) && selected_cards[player].Count > 0)
                            selected_cards[player].RemoveAt(selected_cards[player].Count - 1);
                        
                        selected_cards[player].Add(card);
                    }
                    UpdatePending();
                }
                else
                {
                    if (GetSelected(player, card) != null)
                    {
                        ok_enable = false;
                        selected_cards.Clear();
                        available_targets.Clear();
                        selected_targets.Clear();
                        viewas_card = null;

                        GetPacket2Client(false);
                    }
                    else
                    {
                        selected_cards[player] = new List<WrappedCard> { card };
                        
                        if (viewas_card == null || card.Name != viewas_card.Name)
                            selected_targets.Clear();

                        //国战鏖战模式对桃做特殊判断
                        if (room.BloodBattle && card.Name == Peach.ClassName)
                        {
                            WrappedCard slash = new WrappedCard(Slash.ClassName);
                            slash.AddSubCard(card);
                            slash = RoomLogic.ParseUseCard(room, slash);
                            if (Engine.MatchExpPattern(room, room.GetRoomState().GetCurrentCardUsePattern(player), player, slash)
                                && Slash.Instance.IsAvailable(room, player, slash))
                                viewas_card = slash;
                            else
                            {
                                WrappedCard jink = new WrappedCard(Jink.ClassName);
                                jink.AddSubCard(card);
                                jink = RoomLogic.ParseUseCard(room, jink);
                                viewas_card = jink;
                            }
                        }
                        else
                            viewas_card = card;
                        EnableTargets(player);
                    }
                }
            }
        }

        private void TargetClicked(Player player, Player target)
        {
            FunctionCard fcard = Engine.GetFunctionCard(viewas_card.Name);
            if (double_click)
            {
                double_click = false;
                if (selected_targets.Count == 0 || (selected_targets.Count == 1 && selected_targets.Contains(target)))
                {
                    List <Player> selected = new List<Player> { target };
                    if (fcard.TargetsFeasible(room, selected, player, viewas_card))
                    {
                        selected_targets = new List<Player> { target };
                        Reply2Server(true, player);
                    }
                }
                return;
            }

            if (selected_targets.Contains(target))
            {
                if (fcard.Votes)
                {
                    List<Player> targets = new List<Player>(selected_targets);
                    if (fcard.TargetFilter(room, targets, target, player, viewas_card))
                    {
                        selected_targets.Add(target);
                    }
                    else
                    {
                        selected_targets.RemoveAll(s => s== target);
                    }
                }
                else
                {
                    selected_targets.RemoveAll(s => s == target);
                }
            }
            else
            {
                List<Player> selected = new List<Player>(selected_targets);
                if (!fcard.TargetFilter(room, selected, target, player, viewas_card) && selected_targets.Count > 0)
                    selected_targets.RemoveAt(selected_targets.Count - 1);
                selected_targets.Add(target);
            }

            EnableTargets(player);
        }
        private void SkillClick(Player player, ViewAsSkill skill, string position)
        {
            if (pending_skill == skill && skill_owner == player)
            {
                DoCancelButton();
            }
            else
            {
                skill_position = position;
                pending_skill = skill;
                skill_owner = player;

                StartPending(player);
            }
        }

        private void PendingHuashen(Player player)
        {
            //huashen_pending = true;
            //available_cards.Clear();
            //Dictionary<string, List<string>> huashens = player.GetHuashen();
            //if (huashens.Count == 0)
            //{
            //    HandleInfos();
            //    return;
            //}

            //List<string> args = new List<string> { JsonUntity.Dictionary2Json(huashens) };

            //GetPacket2Client(false, null, -1, args);
        }

        private void DoCancelButton()
        {
            if (pending_skill != null)
            {
                if (skill_invoke)
                {                 //if skill is actived by default then no reply
                    Reply2Server(false);
                }
                else
                {
                    HandleInfos();              //the skill is actived by manually just cancel it
                }
            }
            else if (ExpectedReplyCommand == CommandType.S_COMMAND_PLAY_CARD)           //do discard at play phase
                Reply2Server(false);
            else
                Reply2Server(false);                                                    //cancel respond
        }

        #region IDisposable Support
        private bool disposedValue = false; // 要检测冗余调用

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    // TODO: 释放托管状态(托管对象)。
                    mutex.Dispose();
                }

                // TODO: 释放未托管的资源(未托管的对象)并在以下内容中替代终结器。
                // TODO: 将大型字段设置为 null。

                disposedValue = true;
            }
        }

        // TODO: 仅当以上 Dispose(bool disposing) 拥有用于释放未托管资源的代码时才替代终结器。
        // ~Client() {
        //   // 请勿更改此代码。将清理代码放入以上 Dispose(bool disposing) 中。
        //   Dispose(false);
        // }

        // 添加此代码以正确实现可处置模式。
        public void Dispose()
        {
            // 请勿更改此代码。将清理代码放入以上 Dispose(bool disposing) 中。
            Dispose(true);
            // TODO: 如果在以上内容中替代了终结器，则取消注释以下行。
            // GC.SuppressFinalize(this);
        }
        #endregion
    }
}