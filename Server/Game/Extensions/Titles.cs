﻿using System.Collections.Generic;
using CommonClass.Game;
using SanguoshaServer.Game;
using SanguoshaServer.Package;

namespace SanguoshaServer.Extensions
{
    //逆转裁判
    public class AceAttorney : Title
    {
        public AceAttorney(int id) : base(id)
        {
            EventList = new List<TriggerEvent> { TriggerEvent.GameFinished };
            MarkId = 1;
        }

        public override void OnEvent(TriggerEvent triggerEvent, Room room, Player player, object data)
        {
            foreach (Player p in room.GetAlivePlayers())
                if (p.ClientId <= 0) return;

            if (data is string winners)
            {
                if (winners == ".") return;
                foreach (Player p in room.GetAlivePlayers())
                {
                    int id = p.ClientId;
                    if (id > 0 && p.Name == winners && p.Hp == 1 && !ClientDBOperation.CheckTitle(id, TitleId))
                    {
                        int value = ClientDBOperation.GetTitleMark(id, MarkId);
                        //value++;
                        value += 10;        //for test
                        if (value >= 50)
                        {
                            ClientDBOperation.SetTitle(id, TitleId);
                            Client client = room.Hall.GetClient(id);
                            if (client != null)
                                client.AddProfileTitle(TitleId);
                        }
                        else
                            ClientDBOperation.SetTitleMark(id, MarkId, value);
                    }
                }
            }
        }
    }

    //扬名立万
    public class MakeAName : Title
    {
        public MakeAName(int id) : base(id)
        {
            EventList = new List<TriggerEvent> { TriggerEvent.GameFinished };
            MarkId = 2;
        }

        public override void OnEvent(TriggerEvent triggerEvent, Room room, Player player, object data)
        {
            if (data is string winners)
            {
                if (winners == ".") return;
                foreach (Player p in room.Players)
                {
                    int id = p.ClientId;
                    if (id > 0 && winners.Contains(p.Name) && !ClientDBOperation.CheckTitle(id, TitleId))
                    {
                        int value = ClientDBOperation.GetTitleMark(id, MarkId);
                        //value++;
                        value += 10;        //for test
                        if (value >= 100)
                        {
                            ClientDBOperation.SetTitle(id, TitleId);
                            Client client = room.Hall.GetClient(id);
                            if (client != null)
                                client.AddProfileTitle(TitleId);
                        }
                        else
                            ClientDBOperation.SetTitleMark(id, MarkId, value);
                    }
                }
            }
        }
    }

    //我要打十个
    public class KillThemAll : Title
    {
        public KillThemAll(int id) : base(id)
        {
            EventList = new List<TriggerEvent> { TriggerEvent.BeforeGameOverJudge };
        }

        public override void OnEvent(TriggerEvent triggerEvent, Room room, Player player, object data)
        {
            if (data is DeathStruct death && death.Damage.From != null && death.Damage.From.GetMark("multi_kill_count") >= 6)
            {
                int id = death.Damage.From.ClientId;
                if (id > 0 && !ClientDBOperation.CheckTitle(id, TitleId))
                {
                    ClientDBOperation.SetTitle(id, TitleId);
                    Client client = room.Hall.GetClient(id);
                    if (client != null)
                        client.AddProfileTitle(TitleId);
                }
            }
        }
    }

    //螺旋矛盾
    public class Contradictory : Title
    {
        public Contradictory(int id) : base(id)
        {
            EventList = new List<TriggerEvent> { TriggerEvent.FinishJudge };
        }

        public override void OnEvent(TriggerEvent triggerEvent, Room room, Player player, object data)
        {
            if (data is JudgeStruct judge && judge.Reason == EightDiagram.ClassName && judge.IsBad())
            {
                int id = player.ClientId;
                if (id > 0 && !ClientDBOperation.CheckTitle(id, TitleId))
                {
                    player.AddMark("Contradictory");
                    if (player.GetMark("Contradictory") >= 10)
                    {
                        ClientDBOperation.SetTitle(id, TitleId);
                        Client client = room.Hall.GetClient(id);
                        if (client != null)
                            client.AddProfileTitle(TitleId);
                    }
                }
            }
        }
    }

    //无存在感
    public class Nonexistence : Title
    {
        public Nonexistence(int id) : base(id)
        {
            EventList = new List<TriggerEvent> { TriggerEvent.EventPhaseStart, TriggerEvent.Death };
            MarkId = 3;
        }

        public override void OnEvent(TriggerEvent triggerEvent, Room room, Player player, object data)
        {
            if (triggerEvent == TriggerEvent.EventPhaseStart && player.Phase == Player.PlayerPhase.Start && player.GetMark("Nonexistence") == 0)
            {
                player.SetMark("Nonexistence", 1);
            }
            else if (triggerEvent == TriggerEvent.BeforeGameOverJudge && player.GetMark("Nonexistence") == 0)
            {
                int id = player.ClientId;
                if (id > 0 && !ClientDBOperation.CheckTitle(id, TitleId))
                {
                    int value = ClientDBOperation.GetTitleMark(id, MarkId);
                    value++;
                    if (value >= 30)
                    {
                        ClientDBOperation.SetTitle(id, TitleId);
                        Client client = room.Hall.GetClient(id);
                        if (client != null)
                            client.AddProfileTitle(TitleId);
                    }
                    else
                        ClientDBOperation.SetTitleMark(id, MarkId, value);
                }
            }
        }
    }

    //逆取顺守
    public class Coup : Title
    {
        public Coup(int id) : base(id)
        {
            EventList = new List<TriggerEvent> { TriggerEvent.Death, TriggerEvent.GameFinished };
        }

        public override void OnEvent(TriggerEvent triggerEvent, Room room, Player player, object data)
        {
            if (room.Setting.GameMode != "Hegemony") return;

            if (triggerEvent == TriggerEvent.Death && data is DeathStruct death && death.Damage.From != null && player.GetRoleEnum() == Player.PlayerRole.Lord)
            {
                if (player.Kingdom == Engine.GetGeneral(death.Damage.From.ActualGeneral1, room.Setting.GameMode).Kingdom)
                    death.Damage.From.SetMark("Coup", 1);
            }
            else if (triggerEvent == TriggerEvent.GameFinished && data is string winners)
            {
                if (winners == ".") return;
                foreach (Player p in room.Players)
                    if (p.ClientId <= 0) return;

                foreach (Player p in room.GetAlivePlayers())
                {
                    int id = p.ClientId;
                    if (id > 0 && p.Name == winners && p.GetMark("Coup") > 0 && !ClientDBOperation.CheckTitle(id, TitleId))
                    {
                        ClientDBOperation.SetTitle(id, TitleId);
                        Client client = room.Hall.GetClient(id);
                        if (client != null)
                            client.AddProfileTitle(TitleId);
                    }
                }
            }
        }
    }

    public class Rebel : Title
    {
        public Rebel(int id) : base(id)
        {
            EventList = new List<TriggerEvent> { TriggerEvent.Death, TriggerEvent.GameFinished };
            MarkId = 4;
        }

        public override void OnEvent(TriggerEvent triggerEvent, Room room, Player player, object data)
        {
            if (room.Setting.GameMode != "Classic" || room.Players.Count < 8) return;

            if (triggerEvent == TriggerEvent.Death && data is DeathStruct death
                && (player.GetRoleEnum() == Player.PlayerRole.Renegade || player.GetRoleEnum() == Player.PlayerRole.Loyalist))
            {
                if (death.Damage.From == null || death.Damage.From.GetRoleEnum() != Player.PlayerRole.Rebel)
                {
                    room.SetTag("Rebel", false);
                }
            }
            else if (triggerEvent == TriggerEvent.GameFinished && data is string winners)
            {
                if (winners == "." || room.ContainsTag("Rebel")) return;
                foreach (Player p in room.Players)
                    if (p.ClientId <= 0) return;

                foreach (Player p in room.GetAlivePlayers())
                    if (p.GetRoleEnum() == Player.PlayerRole.Lord || p.GetRoleEnum() == Player.PlayerRole.Loyalist || p.GetRoleEnum() == Player.PlayerRole.Renegade)
                        return;

                foreach (Player p in room.Players)
                {
                    int id = p.ClientId;
                    if (id > 0 && winners.Contains(p.Name) && !ClientDBOperation.CheckTitle(id, TitleId))
                    {
                        int value = ClientDBOperation.GetTitleMark(id, MarkId);
                        value++;
                        if (value >= 20)
                        {
                            ClientDBOperation.SetTitle(id, TitleId);
                            Client client = room.Hall.GetClient(id);
                            if (client != null)
                                client.AddProfileTitle(TitleId);
                        }
                        else
                            ClientDBOperation.SetTitleMark(id, MarkId, value);
                    }
                }
            }
        }
    }
}
