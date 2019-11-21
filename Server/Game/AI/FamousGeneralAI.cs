﻿using System;
using System.Collections.Generic;
using CommonClass;
using CommonClass.Game;
using SanguoshaServer.Game;
using SanguoshaServer.Package;
using static CommonClass.Game.Player;
using static SanguoshaServer.Package.FunctionCard;

namespace SanguoshaServer.AI
{
    public class FamousGeneralAI : AIPackage
    {
        public FamousGeneralAI() : base("FamousGeneral")
        {
            events = new List<SkillEvent>
            {
                new JiushiAI(),
                new LuoyingAI(),
                new HuituoAI(),
                new MingjianAI(),
                new XingshuaiAI(),
                new ZhenlieAI(),
                new MijiAI(),
                new ShangshiAI(),
                new QuanjiAI(),
                new PaiyiAI(),
                new HuomoAI(),
                new ZuodingAI(),
                new ChengxiangAI(),
                new RenxinAI(),
                new QingxiAI(),
                new JingceAI(),
                new JiangchiAI(),
                new SidiAI(),
                new PindiAI(),
                new FaenAI(),

                new ZhichiAI(),
                new MingceAI(),
                new QietingAI(),
                new XianzhouAI(),
                new ZishouAI(),
                new JueceAI(),
                new MiejiAI(),
                new FenchengAI(),
                new ShouxiAI(),
                new HuiminAI(),
                new XianzhenAI(),
                new ZhigeAI(),
                new HuaiyiAI(),

                new EnJXAI(),
                new YuanJXAI(),
                new XuanhuoJXAI(),
                new ZhimanJXAI(),
                new FumianAI(),
                new DaiyanAI(),
                new QiaoshiAI(),
                new YanyuAI(),
                new XiantuAI(),
                new QiangzhiAI(),
                new DangxianJXAI(),
                new FuliAI(),
                new DuliangAI(),
                new FuhunAI(),
                new WuyanAI(),
                new JujianAI(),
                new BenxiAI(),
                new ZhanjueAI(),
                new QinwangAI(),
                new XiansiAI(),
                new XiansiVSAI(),
                new WurongAI(),
                new ZhongyongAI(),

                new AnxuAI(),
                new ZhuiyiAI(),
                new BingyiAI(),
                new ShenxingAI(),
                new BuyiJXAI(),
                new AnguoAI(),
                new WenguaAI(),
                new WenguaVSAI(),
                new FuzhuAI(),
                new YaomingAI(),
                new XuanfengAI(),
                new PinkouAI(),
                new FenliAI(),
                new KuangbiAI(),
                new ZenhuiAI(),
                new JiaojinAI(),
                new ChunlaoAI(),
                new LihuoAI(),
                new DanshouAI(),
                new DuodaoAI(),
                new AnjianAI(),
                new PojunAI(),
            };

            use_cards = new List<UseCard>
            {
                new MingjianCardAI(),
                new MingceCardAI(),
                new AnxuCardAI(),
                new XianzhouCardAI(),
                new ShenxingCardAI(),
                new YanyuCardAI(),
                new PaiyiCardAI(),
                new AnguoCardAI(),
                new MiejiCardAI(),
                new FenchengCardAI(),
                new WenguaCardAI(),
                new DuliangCardAI(),
                new JujianCardAI(),
                new KuangbiCardAI(),
                new XianzhenCardAI(),
                new PindiCardAI(),
                new WurongCardAI(),
                new ZhigeCardAI(),
                new HuaiyiCardAI(),
            };
        }
    }

    public class JiushiAI : SkillEvent
    {
        public JiushiAI() : base("jiushi") { }

        public override List<WrappedCard> GetViewAsCards(TrustedAI ai, string pattern, Player player)
        {
            List<WrappedCard> result = new List<WrappedCard>();
            if (player.FaceUp && pattern == Analeptic.ClassName)
            {
                WrappedCard ana = new WrappedCard(Analeptic.ClassName) { Skill = Name };
                WrappedCard jiushi = new WrappedCard(JiushiCard.ClassName) { Skill = Name };
                ana.UserString = RoomLogic.CardToString(ai.Room, jiushi);
                result.Add(ana);
            }

            return result;
        }

        public override double UseValueAdjust(TrustedAI ai, Player player, List<Player> targets, WrappedCard card)
        {
            return -7;
        }
    }

    public class LuoyingAI : SkillEvent
    {
        public LuoyingAI() : base("luoying") { }

        public override AskForMoveCardsStruct OnMoveCards(TrustedAI ai, Player player, List<int> ups, List<int> downs, int min, int max)
        {
            AskForMoveCardsStruct move = new AskForMoveCardsStruct
            {
                Success = true,
                Top = ups,
                Bottom = downs
            };
            return move;
        }
    }

    public class HuituoAI : SkillEvent
    {
        public HuituoAI() : base("huituo") { key = new List<string> { "playerChosen:huituo" }; }
        public override void OnEvent(TrustedAI ai, TriggerEvent triggerEvent, Player player, object data)
        {
            if (triggerEvent == TriggerEvent.ChoiceMade && data is string str && player != ai.Self)
            {
                List<string> strs = new List<string>(str.Split(':'));
                if (strs[1] == Name)
                {
                    Room room = ai.Room;
                    Player target = room.FindPlayer(strs[2]);
                    if (ai.GetPlayerTendency(target) != "unknown") ai.UpdatePlayerRelation(player, target, true);
                }
            }
        }
        public override List<Player> OnPlayerChosen(TrustedAI ai, Player player, List<Player> targets, int min, int max)
        {
            List<Player> friends = ai.GetFriends(player);
            ai.SortByDefense(ref friends, false);
            foreach (Player p in friends)
                if (p.IsWounded()) return new List<Player> { p };

            return new List<Player> { friends[0] };
        }

        public override ScoreStruct GetDamageScore(TrustedAI ai, DamageStruct damage)
        {
            ScoreStruct score = new ScoreStruct
            {
                Score = ai.HasSkill(Name, damage.To) ? 1.5 : 0
            };
            return score;
        }
    }

    public class MingjianAI : SkillEvent
    {
        public MingjianAI() : base("mingjian") { }

        public override List<WrappedCard> GetTurnUse(TrustedAI ai, Player player)
        {
            if (!player.HasUsed(MingjianCard.ClassName) && !player.IsKongcheng())
                return new List<WrappedCard> { new WrappedCard(MingjianCard.ClassName) { Skill = Name } };
            return new List<WrappedCard>();
        }
    }

    public class MingjianCardAI : UseCard
    {
        public MingjianCardAI() : base(MingjianCard.ClassName) { }
        public override void OnEvent(TrustedAI ai, TriggerEvent triggerEvent, Player player, object data)
        {
            if (triggerEvent == TriggerEvent.CardTargetAnnounced && data is CardUseStruct use && player != ai.Self)
            {
                Player target = use.To[0];
                if (ai.GetPlayerTendency(target) != "unknown") ai.UpdatePlayerRelation(player, target, true);
            }
        }
        public override void Use(TrustedAI ai, Player player, ref CardUseStruct use, WrappedCard card)
        {
            List<Player> friends = ai.FriendNoSelf;
            Room room = ai.Room;
            room.SortByActionOrder(ref friends);
            foreach (Player p in friends)
            {
                if (!ai.HasSkill("zishu", p) && !ai.WillSkipPlayPhase(p))
                {
                    card.AddSubCards(player.GetCards("h"));
                    use.Card = card;
                    use.To.Add(p);
                    return;
                }
            }
        }
        public override double UsePriorityAdjust(TrustedAI ai, Player player, List<Player> targets, WrappedCard card)
        {
            return 0;
        }
    }

    public class XingshuaiAI : SkillEvent
    {
        public XingshuaiAI() : base("xingshuai") { }
        public override bool OnSkillInvoke(TrustedAI ai, Player player, object data)
        {
            if (data is string str && str.StartsWith("@xingshuai-src"))
            {
                Room room = ai.Room;
                string[] strs = str.Split(':');
                Player target = room.FindPlayer(strs[1]);
                if (target.GetRoleEnum() == Player.PlayerRole.Lord)
                {
                    if (player.GetRoleEnum() == Player.PlayerRole.Loyalist) return true;
                    if (player.GetRoleEnum() == Player.PlayerRole.Renegade && room.GetAlivePlayers().Count > 2
                        && ai.GetKnownCardsNums(Analeptic.ClassName, "he", target, player) + ai.GetKnownCardsNums(Peach.ClassName, "he", target, player) == 0
                        && player.Hp > 1) return true;
                }
            }
            else
                return true;

            return false;
        }
    }

    public class ZhenlieAI : SkillEvent
    {
        public ZhenlieAI() : base("zhenlie") { }

        public override bool OnSkillInvoke(TrustedAI ai, Player player, object data)
        {
            Room room = ai.Room;
            if (room.GetTag(Name) is CardUseStruct use)
            {
                if (!ai.IsCardEffect(use.Card, player, use.From)) return false;

                if (use.Card.Name.Contains(Slash.ClassName) || use.Card.Name == Duel.ClassName || use.Card.Name == ArcheryAttack.ClassName || use.Card.Name == SavageAssault.ClassName)
                {
                    bool avoid = false;
                    int index = (int)player.GetTag(Name);
                    if (use.EffectCount[index].Nullified) return false;

                    if (use.Card.Name.Contains(Slash.ClassName))
                    {
                        if (use.EffectCount[index].Effect2 == 1 && (ai.GetKnownCardsNums(Jink.ClassName, "he", player) > 0
                            || (ai.HasArmorEffect(player, EightDiagram.ClassName) && !player.ArmorIsNullifiedBy(use.From))))
                            avoid = true;

                        if (use.From.HasWeapon(Axe.ClassName) && use.From.GetCardCount(true) > 3) avoid = false;
                    }
                    else if (use.EffectCount[index].Effect2 == 1 && use.Card.Name == ArcheryAttack.ClassName && (ai.GetKnownCardsNums(Jink.ClassName, "he", player) > 0
                        || (ai.HasArmorEffect(player, EightDiagram.ClassName) && !player.ArmorIsNullifiedBy(use.From))))
                        avoid = true;
                    else if (use.EffectCount[index].Effect2 == 1 && use.Card.Name == SavageAssault.ClassName && ai.GetKnownCardsNums(Slash.ClassName, "he", player) > 0)
                        avoid = true;
                    else if (use.EffectCount[index].Effect2 == 1 && use.Card.Name == Duel.ClassName
                        && ai.GetKnownCardsNums(Slash.ClassName, "he", player) > 2 && !ai.HasSkill("wushuang", use.From))
                        avoid = true;

                    if (!avoid)
                    {
                        DamageStruct damage = new DamageStruct(use.Card, use.From, player, 1 + use.Drank + use.ExDamage + use.EffectCount[index].Effect1);
                        if (use.Card.Name == FireSlash.ClassName)
                            damage.Nature = DamageStruct.DamageNature.Fire;
                        else if (damage.Card.Name == ThunderSlash.ClassName)
                            damage.Nature = DamageStruct.DamageNature.Thunder;

                        ScoreStruct score = ai.GetDamageScore(damage);
                        if (score.Score < -4 && !ai.IsFriend(use.From))
                            return true;
                        else if (ai.IsFriend(use.From) && score.Score < -20 && player.Hp == 1 && use.From.GetRoleEnum() == Player.PlayerRole.Lord)
                            return true;
                    }
                }
                else if (ai.IsFriend(use.From))
                {
                    return false;
                }
                else if (use.Card.Name == Snatch.ClassName && player.Hp > 1)
                    return true;
            }

            return false;
        }
    }

    public class MijiAI : SkillEvent
    {
        public MijiAI() : base("miji") { }

        public override bool OnSkillInvoke(TrustedAI ai, Player player, object data)
        {
            return true;
        }
    }

    public class ShangshiAI : SkillEvent
    {
        public ShangshiAI() : base("shangshi") { }

        public override bool OnSkillInvoke(TrustedAI ai, Player player, object data)
        {
            return true;
        }
    }

    public class QuanjiAI : SkillEvent
    {
        public QuanjiAI() : base("quanji") { }

        public override bool OnSkillInvoke(TrustedAI ai, Player player, object data)
        {
            return true;
        }

        public override List<int> OnExchange(TrustedAI ai, Player player, string pattern, int min, int max, string pile)
        {
            List<int> ids = player.GetCards("h");
            ai.SortByKeepValue(ref ids, false);
            return new List<int> { ids[0] };
        }
    }

    public class PaiyiAI : SkillEvent
    {
        public PaiyiAI() : base("paiyi")
        { }

        public override List<WrappedCard> GetTurnUse(TrustedAI ai, Player player)
        {
            List<WrappedCard> result = new List<WrappedCard>();
            if (player.GetPile("quanji").Count > 0 && !player.HasUsed(PaiyiCard.ClassName))
            {
                WrappedCard card = new WrappedCard(PaiyiCard.ClassName) { Skill = Name };
                card.AddSubCard(player.GetPile("quanji")[0]);
                result.Add(card);
            }

            return result;
        }
    }

    public class PaiyiCardAI : UseCard
    {
        public PaiyiCardAI() : base(PaiyiCard.ClassName) { }

        public override void OnEvent(TrustedAI ai, TriggerEvent triggerEvent, Player player, object data)
        {
            if (triggerEvent == TriggerEvent.CardTargetAnnounced && data is CardUseStruct use && player != ai.Self)
            {
                Player target = use.To[0];
                if (target.HandcardNum + 2 <= player.HandcardNum && player != target && ai.GetPlayerTendency(target) != "unknown")
                    ai.UpdatePlayerRelation(player, target, true);
            }
        }
        public override void Use(TrustedAI ai, Player player, ref CardUseStruct use, WrappedCard card)
        {
            use.Card = card;
            use.To.Add(player);
        }

        public override double UsePriorityAdjust(TrustedAI ai, Player player, List<Player> targets, WrappedCard card)
        {
            return 6;
        }
    }

    public class ZuodingAI : SkillEvent
    {
        public ZuodingAI() : base("zuoding") { key = new List<string> { "playerChosen:zuoding" }; }
        public override void OnEvent(TrustedAI ai, TriggerEvent triggerEvent, Player player, object data)
        {
            if (triggerEvent == TriggerEvent.ChoiceMade && data is string str && player != ai.Self)
            {
                string[] strs = str.Split(':');
                if (strs[1] == Name)
                {
                    Room room = ai.Room;
                    Player target = room.FindPlayer(strs[2]);
                    if (ai.GetPlayerTendency(target) != "unknown")
                        ai.UpdatePlayerRelation(player, target, true);
                }
            }
        }

        public override List<Player> OnPlayerChosen(TrustedAI ai, Player player, List<Player> targets, int min, int max)
        {
            ai.SortByDefense(ref targets, false);
            foreach (Player p in targets)
                if (ai.IsFriend(p))
                    return new List<Player> { p };

            return new List<Player>();
        }

        public override double TargetValueAdjust(TrustedAI ai, WrappedCard card, Player from, List<Player> targets, Player to)
        {
            Room room = ai.Room;
            Player zhongyao = RoomLogic.FindPlayerBySkillName(room, Name);
            if (!(Engine.GetFunctionCard(card.Name) is DelayedTrick) && card.Suit == WrappedCard.CardSuit.Spade
                && !room.ContainsTag(Name) && zhongyao != null && ai.IsFriend(zhongyao, to))
            {
                return ai.IsFriend(to) ? 1.5 : -1;
            }

            return 0;
        }
    }

    public class HuomoAI : SkillEvent
    {
        public HuomoAI() : base("huomo") { }

        public override List<WrappedCard> GetTurnUse(TrustedAI ai, Player player)
        {
            List<int> ids = new List<int>();
            Room room = ai.Room;
            foreach (int id in player.GetCards("he"))
            {
                WrappedCard card = room.GetCard(id);
                if (!(Engine.GetFunctionCard(card.Name) is BasicCard) && WrappedCard.IsBlack(card.Suit))
                    ids.Add(id);
            }
            List<WrappedCard> result = new List<WrappedCard>();
            if (ids.Count > 0)
            {
                int sub = -1;
                List<double> values = ai.SortByKeepValue(ref ids, false);
                if (values[0] < 0) sub = ids[0];
                else
                {
                    ai.SortByUseValue(ref ids, false);
                    sub = ids[0];
                }

                WrappedCard huomo = new WrappedCard(HuomoCard.ClassName) { Skill = Name };
                huomo.AddSubCard(sub);

                if (!player.HasFlag("huomo_Slash"))
                {
                    WrappedCard slash = new WrappedCard(Slash.ClassName) { Skill = Name };
                    slash.AddSubCard(sub);
                    huomo.UserString = Slash.ClassName;
                    slash.UserString = RoomLogic.CardToString(room, huomo);

                    result.Add(slash);
                }

                if (!player.HasFlag("huomo_Peach"))
                {
                    WrappedCard slash = new WrappedCard(Peach.ClassName) { Skill = Name };
                    slash.AddSubCard(sub);
                    huomo.UserString = Peach.ClassName;
                    slash.UserString = RoomLogic.CardToString(room, huomo);

                    result.Add(slash);
                }
            }

            return result;
        }

        public override List<WrappedCard> GetViewAsCards(TrustedAI ai, string pattern, Player player)
        {
            List<int> ids = new List<int>();
            Room room = ai.Room;
            List<WrappedCard> result = new List<WrappedCard>();
            if (room.GetRoomState().GetCurrentCardUseReason() == CardUseStruct.CardUseReason.CARD_USE_REASON_RESPONSE_USE
                    || room.GetRoomState().GetCurrentCardUseReason() == CardUseStruct.CardUseReason.CARD_USE_REASON_PLAY)
            {
                foreach (int id in player.GetCards("he"))
                {
                    WrappedCard card = room.GetCard(id);
                    if (!(Engine.GetFunctionCard(card.Name) is BasicCard) && WrappedCard.IsBlack(card.Suit))
                        ids.Add(id);
                }
                if (ids.Count > 0)
                {
                    if (pattern == Slash.ClassName && !player.HasFlag("huomo_Slash"))
                    {
                        int sub = -1;
                        List<double> values = ai.SortByKeepValue(ref ids, false);
                        if (values[0] < 0)
                            sub = ids[0];
                        else
                        {
                            ai.SortByUseValue(ref ids, false);
                            sub = ids[0];
                        }

                        WrappedCard huomo = new WrappedCard(HuomoCard.ClassName) { Skill = Name };
                        huomo.AddSubCard(sub);

                        WrappedCard slash = new WrappedCard(Slash.ClassName) { Skill = Name };
                        slash.AddSubCard(sub);
                        huomo.UserString = Slash.ClassName;
                        slash.UserString = RoomLogic.CardToString(room, huomo);
                        result.Add(slash);

                        WrappedCard fslash = new WrappedCard(FireSlash.ClassName) { Skill = Name };
                        fslash.AddSubCard(sub);
                        huomo.UserString = FireSlash.ClassName;
                        fslash.UserString = RoomLogic.CardToString(room, huomo);
                        result.Add(fslash);

                        WrappedCard tslash = new WrappedCard(ThunderSlash.ClassName) { Skill = Name };
                        tslash.AddSubCard(sub);
                        huomo.UserString = ThunderSlash.ClassName;
                        tslash.UserString = RoomLogic.CardToString(room, huomo);
                        result.Add(tslash);
                    }
                    else if (pattern == Analeptic.ClassName && !player.HasFlag("huomo_Analeptic"))
                    {
                        int sub = -1;
                        List<double> values = ai.SortByKeepValue(ref ids, false);
                        if (values[0] < 0)
                            sub = ids[0];
                        else
                        {
                            ai.SortByUseValue(ref ids, false);
                            sub = ids[0];
                        }

                        WrappedCard huomo = new WrappedCard(HuomoCard.ClassName) { Skill = Name };
                        huomo.AddSubCard(sub);

                        WrappedCard slash = new WrappedCard(Analeptic.ClassName) { Skill = Name };
                        slash.AddSubCard(sub);
                        huomo.UserString = Analeptic.ClassName;
                        slash.UserString = RoomLogic.CardToString(room, huomo);

                        result.Add(slash);
                    }
                    else if (pattern == Peach.ClassName && !player.HasFlag("huomo_Peach"))
                    {
                        int sub = -1;
                        List<double> values = ai.SortByKeepValue(ref ids, false);
                        if (values[0] < 0)
                            sub = ids[0];
                        else
                        {
                            ai.SortByUseValue(ref ids, false);
                            sub = ids[0];
                        }

                        WrappedCard huomo = new WrappedCard(HuomoCard.ClassName) { Skill = Name };
                        huomo.AddSubCard(sub);

                        WrappedCard slash = new WrappedCard(Peach.ClassName) { Skill = Name };
                        slash.AddSubCard(sub);
                        huomo.UserString = Peach.ClassName;
                        slash.UserString = RoomLogic.CardToString(room, huomo);

                        result.Add(slash);
                    }
                    else if (pattern == Jink.ClassName && !player.HasFlag("huomo_Jink"))
                    {
                        int sub = -1;
                        List<double> values = ai.SortByKeepValue(ref ids, false);
                        if (values[0] < 0)
                            sub = ids[0];
                        else
                        {
                            ai.SortByUseValue(ref ids, false);
                            sub = ids[0];
                        }

                        WrappedCard huomo = new WrappedCard(HuomoCard.ClassName) { Skill = Name };
                        huomo.AddSubCard(sub);

                        WrappedCard slash = new WrappedCard(Jink.ClassName) { Skill = Name };
                        slash.AddSubCard(sub);
                        huomo.UserString = Jink.ClassName;
                        slash.UserString = RoomLogic.CardToString(room, huomo);

                        result.Add(slash);
                    }
                }
            }

            return result;
        }

        public override double CardValue(TrustedAI ai, Player player, WrappedCard card, bool isUse, Place place)
        {
            if (card.GetEffectiveId() == -1) return 0;

            if (ai.HasSkill(Name, player) && !(Engine.GetFunctionCard(ai.Room.GetCard(card.GetEffectiveId()).Name) is BasicCard)
                && WrappedCard.IsBlack(ai.Room.GetCard(card.GetEffectiveId()).Suit))
            {
                return 2;
            }

            return 0;
        }

        public override double UseValueAdjust(TrustedAI ai, Player player, List<Player> targets, WrappedCard card)
        {
            return -2;
        }
    }

    public class ChengxiangAI : SkillEvent
    {
        public ChengxiangAI() : base("chengxiang") { }

        public override bool OnSkillInvoke(TrustedAI ai, Player player, object data)
        {
            return true;
        }

        public override double CardValue(TrustedAI ai, Player player, WrappedCard card, bool isUse, Place place)
        {
            FunctionCard fcard = Engine.GetFunctionCard(card.Name);
            if (ai.HasSkill(Name, player) && (fcard is Peach || fcard is Analeptic))
                return 2;
            if (ai.HasSkill(Name, player) && fcard is EquipCard)
                return 1;

            return 0;
        }

        public override AskForMoveCardsStruct OnMoveCards(TrustedAI ai, Player player, List<int> ups, List<int> downs, int min, int max)
        {
            AskForMoveCardsStruct result = new AskForMoveCardsStruct
            {
                Top = new List<int>(ups),
                Bottom = new List<int>(),
                Success = true
            };
            Room room = ai.Room;
            List<double> values = ai.SortByUseValue(ref ups);
            Dictionary<int, double> points = new Dictionary<int, double>();
            for (int i = 0; i < ups.Count; i++)
            {
                int id = ups[i];
                double value = values[i];
                value *= 1 + (0.3 * (13 - room.GetCard(id).Number) / 13);
                points[id] = value;
            }
            ups.Sort((x, y) => { return points[x] > points[y] ? -1 : 1; });

            int number = 0;
            foreach (int id in ups)
            {
                WrappedCard card = room.GetCard(id);
                if (number + card.Number <= 13)
                {
                    number += card.Number;
                    result.Bottom.Add(id);
                    result.Top.Remove(id);
                }
            }

            return result;
        }
    }

    public class RenxinAI : SkillEvent
    {
        public RenxinAI() : base("renxin") { }

        public override CardUseStruct OnResponding(TrustedAI ai, Player player, string pattern, string prompt, object data)
        {
            CardUseStruct use = new CardUseStruct(null, player, new List<Player>());
            string[] strs = prompt.Split(':');
            Room room = ai.Room;
            Player target = room.FindPlayer(strs[1]);
            if (ai.IsFriend(target))
            {
                List<int> ids = new List<int>();
                foreach (int id in player.GetCards("he"))
                {
                    WrappedCard card = room.GetCard(id);
                    if (Engine.GetFunctionCard(card.Name) is EquipCard && RoomLogic.CanDiscard(room, player, player, id))
                        ids.Add(id);
                }

                if (ids.Count > 0)
                {
                    ai.SortByKeepValue(ref ids, false);
                    use.Card = new WrappedCard(RenxinCard.ClassName);
                    use.Card.AddSubCard(ids[0]);
                }
            }

            return use;
        }

        public override double CardValue(TrustedAI ai, Player player, WrappedCard card, bool isUse, Place place)
        {
            FunctionCard fcard = Engine.GetFunctionCard(card.Name);
            if (ai.HasSkill(Name, player) && fcard is EquipCard)
                return 0.7;

            return 0;
        }
    }


    public class QingxiAI : SkillEvent
    {
        public QingxiAI() : base("qingxi") { }

        public override bool OnSkillInvoke(TrustedAI ai, Player player, object data)
        {
            Room room = ai.Room;
            if (data is Player target && room.GetTag(Name) is DamageStruct damage && ai.IsEnemy(target) && !player.HasWeapon(CrossBow.ClassName))
            {
                double value = ai.GetDamageScore(damage).Score;
                DamageStruct _damage = damage;
                _damage.Damage++;
                if (ai.GetDamageScore(_damage).Score > value + 2)
                    return true;                
            }

            return false;
        }

        public override CardUseStruct OnResponding(TrustedAI ai, Player player, string pattern, string prompt, object data)
        {
            Room room = ai.Room;
            CardUseStruct use = new CardUseStruct(null, player, new List<Player>());
            if (room.GetTag(Name) is DamageStruct damage)
            {
                double value = ai.GetDamageScore(damage).Score;
                DamageStruct _damage = damage;
                _damage.Damage++;
                if (ai.GetDamageScore(_damage).Score > value + 2)
                {
                    int count = player.GetMark(Name);
                    int peach = 0;
                    List<int> ids = new List<int>(), cards = player.GetCards("h");
                    ai.SortByKeepValue(ref cards, false);
                    foreach (int id in cards)
                    {
                        if (RoomLogic.CanDiscard(room, player, player, id))
                        {
                            if (ai.IsCard(id, Peach.ClassName, player) || ai.IsCard(id, Analeptic.ClassName, player)) peach++;
                            ids.Add(id);
                        }

                        if (ids.Count >= count)
                            break;
                    }

                    if (peach == 0 && ids.Count == count)
                    {
                        use.Card = new WrappedCard(QingxiCard.ClassName);
                        use.Card.AddSubCards(ids);
                    }
                }
            }

            return use;
        }
    }

    public class JingceAI : SkillEvent
    {
        public JingceAI() : base("jingce") { }
        public override bool OnSkillInvoke(TrustedAI ai, Player player, object data)
        {
            if (player.GetMark("@rob") > 0)
            {
                Player gn = RoomLogic.FindPlayerBySkillName(ai.Room, "jieying_gn");
                if (gn != null && !ai.IsFriend(gn)) return false;
            }

            return true;
        }
    }

    public class JiangchiAI : SkillEvent
    {
        public JiangchiAI() : base("jiangchi") { }

        public override string OnChoice(TrustedAI ai, Player player, string choice, object data)
        {
            int count = ai.GetKnownCardsNums(Slash.ClassName, "he", player);
            if (count > 1)
                return "less";
            else if (count == 0 || player.Hp > player.HandcardNum)
                return "more";

            return "cancel";
        }
    }

    public class SidiAI : SkillEvent
    {
        public SidiAI() : base("sidi") { key = new List<string> { "cardExchange:sidi" }; }

        public override void OnEvent(TrustedAI ai, TriggerEvent triggerEvent, Player player, object data)
        {
            if (triggerEvent == TriggerEvent.ChoiceMade && data is string str && ai.Self != player)
            {
                string[] strs = str.Split(':');
                Room room = ai.Room;
                if (strs[1] == Name && strs.Length > 2 && !string.IsNullOrEmpty(strs[2]))
                {
                    Player target = room.Current;
                    if (ai.GetPlayerTendency(target) != "unknown")
                        ai.UpdatePlayerRelation(player, target, false);
                }
            }
        }

        public override List<int> OnExchange(TrustedAI ai, Player player, string pattern, int min, int max, string pile)
        {
            Room room = ai.Room;
            Player target = room.Current;
            if (!ai.HasSkill("zhixi", target) && ai.IsEnemy(target))
            {
                List<int> ids = new List<int>();
                string _pattern = pattern.Substring(1, pattern.Length - 1);
                foreach (int id in player.GetCards("he"))
                    if (RoomLogic.CanDiscard(room, player, player, id) && Engine.MatchExpPattern(room, _pattern, player, room.GetCard(id)))
                        ids.Add(id);

                if (ids.Count > 0)
                {
                    List<double> values = ai.SortByKeepValue(ref ids, false);
                    if (values[0] < 0)
                        return new List<int> { ids[0] };

                    if (values[0] < 2.5 && target.HandcardNum > 4)
                        return new List<int> { ids[0] };
                    if (target.HandcardNum - target.Hp >= 2 && values[0] < 4)
                        return new List<int> { ids[0] };
                    WrappedCard slash = new WrappedCard(Slash.ClassName);
                    if (ai.IsCardEffect(slash, target, player) && ai.SlashIsEffective(slash, target).Score > 4 && values[0] < 3.5)
                        return new List<int> { ids[0] };
                }
            }

            return new List<int>();
        }
    }

    public class PindiAI : SkillEvent
    {
        public PindiAI() : base("pindi") { key = new List<string> { "skillChoice:pindi" }; }
        public override void OnEvent(TrustedAI ai, TriggerEvent triggerEvent, Player player, object data)
        {
            if (triggerEvent == TriggerEvent.ChoiceMade && data is string str)
            {
                Room room = ai.Room;
                List<string> strs = new List<string>(str.Split(':'));
                if (strs[1] == Name && ai.Self == player)
                {
                    string choice = strs[2];
                    Player target = null;
                    foreach (Player p in room.GetAlivePlayers())
                    {
                        if (p.HasFlag("pindi_target"))
                        {
                            target = p;
                            break;
                        }
                    }
                    if (target != null)
                    {
                        if (choice == "draw")
                            ai.UpdatePlayerRelation(player, target, true);
                        else
                        {
                            int count = player.GetMark(Name);
                            bool friend = false;
                            if (count == 1)
                            {
                                foreach (int id in target.GetEquips())
                                {
                                    if (ai.GetKeepValue(id, target, Place.PlaceEquip) < 0)
                                    {
                                        friend = true;
                                        break;
                                    }
                                }
                            }

                            ai.UpdatePlayerRelation(player, target, friend);
                        }
                    }
                }
            }
        }
        public override List<WrappedCard> GetTurnUse(TrustedAI ai, Player player)
        {
            List<int> ids = new List<int>(), equips = new List<int>(), trick = new List<int>(), basic = new List<int>();
            Room room = ai.Room;
            foreach (int id in player.GetCards("he"))
            {
                if (RoomLogic.CanDiscard(room, player, player, id))
                {
                    WrappedCard card = room.GetCard(id);
                    FunctionCard fcard = Engine.GetFunctionCard(card.Name);
                    if (!player.HasFlag(string.Format("{0}_{1}", Name, fcard.TypeID)))
                    {
                        ids.Add(id);
                        switch (fcard.TypeID)
                        {
                            case CardType.TypeBasic:
                                basic.Add(id);
                                break;
                            case CardType.TypeTrick:
                                trick.Add(id);
                                break;
                            case CardType.TypeEquip:
                                equips.Add(id);
                                break;
                        }
                    }
                }
            }

            if (ids.Count > 0)
            {
                int count = player.GetMark(Name) + 1;
                List<double> values = ai.SortByKeepValue(ref ids, false);
                if (values[0] < 0)
                {
                    WrappedCard pd = new WrappedCard(PindiCard.ClassName) { Skill = Name };
                    pd.AddSubCard(ids[0]);
                    return new List<WrappedCard> { pd };
                }

                values = ai.SortByUseValue(ref ids, false);
                if (values[0] < 3)
                {
                    WrappedCard pd = new WrappedCard(PindiCard.ClassName) { Skill = Name };
                    pd.AddSubCard(ids[0]);
                    return new List<WrappedCard> { pd };
                }

                if (values[0] < 4.3 && count > 1)
                {
                    WrappedCard pd = new WrappedCard(PindiCard.ClassName) { Skill = Name };
                    pd.AddSubCard(ids[0]);
                    return new List<WrappedCard> { pd };
                }
            }

            return new List<WrappedCard>();
        }
    }

    public class PindiCardAI : UseCard
    {
        public PindiCardAI() : base(PindiCard.ClassName) { }

        public override void Use(TrustedAI ai, Player player, ref CardUseStruct use, WrappedCard card)
        {
            int count = player.GetMark("pindi") + 1;
            List<Player> friends = ai.GetFriends(player);
            ai.SortByDefense(ref friends, false);
            foreach (Player p in friends)
            {
                if (!p.HasFlag("pindi") && (!p.IsKongcheng() || ai.NeedKongcheng(player)))
                {
                    use.Card = card;
                    use.To = new List<Player> { p };
                    return;
                }
            }

            List<Player> enemies = ai.GetEnemies(player);
            ai.SortByDefense(ref enemies, false);
            foreach (Player p in enemies)
            {
                if (!p.HasFlag("pindi") && p.GetCardCount(true) >= count)
                {
                    bool skip = false;
                    foreach (int id in p.GetEquips())
                    {
                        if (ai.GetKeepValue(id, p, Place.PlaceEquip) < 0)
                        {
                            skip = true;
                            break;
                        }
                    }
                    if (skip) continue;

                    use.Card = card;
                    use.To = new List<Player> { p };
                    return;
                }
            }
        }

        public override string OnChoice(TrustedAI ai, Player player, string choices, object data)
        {
            if (data is Player target && ai.IsFriend(target))
                return "draw";

            return "discard";
        }

        public override double UsePriorityAdjust(TrustedAI ai, Player player, List<Player> targets, WrappedCard card)
        {
            return 4.3;
        }
    }

    public class FaenAI : SkillEvent
    {
        public FaenAI() : base("faen")
        {
            key = new List<string> { "skillInvoke:faen" };
        }

        public override void OnEvent(TrustedAI ai, TriggerEvent triggerEvent, Player player, object data)
        {
            if (triggerEvent == TriggerEvent.ChoiceMade && data is string str && player != ai.Self)
            {
                List<string> strs = new List<string>(str.Split(':'));
                if (strs[1] == Name)
                {
                    Player target = null;
                    foreach (Player p in ai.Room.GetAlivePlayers())
                    {
                        if (p.HasFlag(Name))
                        {
                            target = p;
                            break;
                        }
                    }

                    bool friendly = strs[2] == "yes";
                    if (ai.GetPlayerTendency(target) != "unknown") ai.UpdatePlayerRelation(player, target, friendly);
                }
            }
        }
    }

    public class ZhichiAI : SkillEvent
    {
        public ZhichiAI() : base("zhichi")
        {
        }

        public override bool IsCardEffect(TrustedAI ai, WrappedCard card, Player from, Player to)
        {
            if (to != null && to.GetMark("@late") > 0)
            {
                Room room = ai.Room;
                FunctionCard fcard = Engine.GetFunctionCard(card.Name);
                if (fcard is Slash || (fcard is TrickCard && !(fcard is DelayedTrick)))
                    return false;
            }

            return true;
        }
    }

    public class MingceAI : SkillEvent
    {
        public MingceAI() : base("mingce")
        {
        }

        public override List<WrappedCard> GetTurnUse(TrustedAI ai, Player player)
        {
            if (!player.HasUsed(MingceCard.ClassName))
                return new List<WrappedCard> { new WrappedCard(MingceCard.ClassName) { Skill = Name } };

            return new List<WrappedCard>();
        }

        public override string OnChoice(TrustedAI ai, Player player, string choice, object data)
        {
            Room room = ai.Room;
            Player target = null;
            foreach (Player p in room.GetAlivePlayers())
            {
                if (p.HasFlag("MingceTarget"))
                {
                    target = p;
                    break;
                }
            }
            WrappedCard slash = new WrappedCard(Slash.ClassName);
            ScoreStruct score = ai.SlashIsEffective(slash, target);
            if (score.Score > 1.5)
                return "use";

            return "draw";
        }

        public override List<Player> OnPlayerChosen(TrustedAI ai, Player player, List<Player> targets, int min, int max)
        {
            if (ai.Target[Name] != null && targets.Contains(ai.Target[Name]))
                return new List<Player> { ai.Target[Name] };

            foreach (Player p in targets)
                if (ai.IsEnemy(p)) return new List<Player> { p };

            return new List<Player>();
        }
    }

    public class MingceCardAI : UseCard
    {
        public MingceCardAI() : base(MingceCard.ClassName)
        {
        }

        public override void Use(TrustedAI ai, Player player, ref CardUseStruct use, WrappedCard card)
        {
            ai.Target["mingce"] = null;
            ai.Number[Name] = 5;
            List<Player> enemies = ai.GetEnemies(player), friends = ai.FriendNoSelf;
            ai.SortByDefense(ref enemies, false);
            ai.SortByDefense(ref friends, false);
            Room room = ai.Room;

            List<int> ids = new List<int>();
            foreach (int id in player.GetCards("he"))
            {
                WrappedCard wrapped = room.GetCard(id);
                FunctionCard fcard = Engine.GetFunctionCard(wrapped.Name);
                if (fcard is Slash || fcard is EquipCard)
                    ids.Add(id);
            }
            if (ids.Count > 0 && friends.Count > 0)
            {
                int sub = -1;
                List<double> values = ai.SortByKeepValue(ref ids, false);
                if (values[0] < 0)
                    sub = ids[0];

                WrappedCard slash = new WrappedCard(Slash.ClassName);
                List<ScoreStruct> scores = new List<ScoreStruct>();
                foreach (Player p in friends)
                {
                    foreach (Player enemy in enemies)
                    {
                        if (RoomLogic.InMyAttackRange(room, p, enemy, slash) && RoomLogic.IsProhibited(room, p, enemy, slash) == null)
                        {
                            ScoreStruct score = new ScoreStruct
                            {
                                Players = new List<Player> { p },
                                Card = slash,
                            };

                            DamageStruct damage = new DamageStruct(slash, p, enemy);
                            if (ai.HasArmorEffect(enemy, Vine.ClassName) && slash.Name == Slash.ClassName && p.HasWeapon(Fan.ClassName))
                            {
                                WrappedCard fan = new WrappedCard(FireSlash.ClassName);
                                fan.AddSubCard(slash);
                                fan = RoomLogic.ParseUseCard(room, fan);
                                damage.Card = fan;
                            }

                            if (damage.Card.Name == FireSlash.ClassName)
                                damage.Nature = DamageStruct.DamageNature.Fire;
                            else if (damage.Card.Name == ThunderSlash.ClassName)
                                damage.Nature = DamageStruct.DamageNature.Thunder;

                            ScoreStruct damage_score = ai.GetDamageScore(damage);
                            if (damage_score.Score > 0)
                            {
                                ScoreStruct effect = ai.SlashIsEffective(damage.Card, p, enemy);
                                if (effect.Score > 0)
                                {
                                    score.Score = effect.Score;
                                    if (effect.Rate > 0)
                                    {
                                        score.Score += Math.Min(1, effect.Rate) * damage_score.Score;
                                        score.Damage = damage;
                                        score.DoDamage = damage_score.DoDamage;
                                        scores.Add(score);
                                    }
                                }
                            }
                        }
                    }
                }

                if (scores.Count > 0)
                {
                    scores.Sort((x, y) => { return x.Score > y.Score ? -1 : 1; });
                    if (scores[0].Score > 2)
                    {
                        use.To = scores[0].Players;
                        ai.Target["mingce"] = scores[0].Damage.To;
                        if (sub > -1)
                        {
                            use.Card = card;
                            use.Card.AddSubCard(sub);
                            return;
                        }
                    }
                }

                List<Player> targets = use.To.Count > 0 ? new List<Player>(use.To) : ai.FriendNoSelf;
                KeyValuePair<Player, int> pair = ai.GetCardNeedPlayer(ids, targets);
                if (pair.Key != null && pair.Value > -1)
                {
                    card.AddSubCard(pair.Value);
                    use.Card = card;
                    if (use.To.Count == 0)
                    {
                        use.To.Add(pair.Key);
                        ai.Number[Name] = 0.8;
                    }
                    return;
                }
                else if (use.To.Count > 0)
                {
                    card.AddSubCard(ids[0]);
                    use.Card = card;
                    return;
                }

                if (sub > -1)
                {
                    use.Card = card;
                    use.Card.AddSubCard(sub);
                    use.To.Add(friends[0]);
                    return;
                }

                ai.Number[Name] = 0.8;
                if (ai.GetOverflow(player) > 0)
                {
                    ai.SortByUseValue(ref ids, false);
                    foreach (int id in ids)
                    {
                        if (room.GetCardPlace(id) == Place.PlaceHand)
                        {
                            card.AddSubCard(ids[0]);
                            use.Card = card;
                            use.To.Add(friends[0]);
                            return;
                        }
                    }
                }

                foreach (int id in ids)
                {
                    WrappedCard wrapped = room.GetCard(id);
                    FunctionCard fcard = Engine.GetFunctionCard(wrapped.Name);
                    if (fcard.TypeID == CardType.TypeEquip && !(fcard is EquipCard) && !(fcard is DefensiveHorse))
                    {
                        foreach (Player p in targets)
                        {
                            if (ai.GetSameEquip(wrapped, p) == null)
                            {
                                card.AddSubCard(id);
                                use.Card = card;
                                use.To.Add(p);
                                return;
                            }
                        }
                    }
                }

                foreach (int id in ids)
                {
                    WrappedCard wrapped = room.GetCard(id);
                    if (wrapped.Name.Contains(Slash.ClassName))
                    {
                        card.AddSubCard(id);
                        use.Card = card;
                        use.To.Add(friends[0]);
                        return;
                    }
                }

                foreach (int id in ids)
                {
                    WrappedCard wrapped = room.GetCard(id);
                    FunctionCard fcard = Engine.GetFunctionCard(wrapped.Name);
                    if (fcard.TypeID == CardType.TypeEquip)
                    {
                        foreach (Player p in targets)
                        {
                            if (ai.GetSameEquip(wrapped, p) == null && !ai.WillSkipPlayPhase(player))
                            {
                                card.AddSubCard(id);
                                use.Card = card;
                                use.To.Add(p);
                                return;
                            }
                        }
                    }
                }
            }
        }

        public override double UsePriorityAdjust(TrustedAI ai, Player player, List<Player> targets, WrappedCard card)
        {
            return ai.Number[Name];
        }

        public override void OnEvent(TrustedAI ai, TriggerEvent triggerEvent, Player player, object data)
        {
            if (triggerEvent == TriggerEvent.CardTargetAnnounced && data is CardUseStruct use && player != ai.Self)
            {
                Player target = use.To[0];
                if (ai.GetPlayerTendency(target) != "unknown") ai.UpdatePlayerRelation(player, target, true);
            }
        }
    }

    public class QietingAI : SkillEvent
    {
        public QietingAI() : base("qieting")
        {
            key = new List<string> { "cardChosen:qieting" };
        }

        public override void OnEvent(TrustedAI ai, TriggerEvent triggerEvent, Player player, object data)
        {
            if (triggerEvent == TriggerEvent.ChoiceMade && data is string str && player != ai.Self)
            {
                Room room = ai.Room;
                List<string> strs = new List<string>(str.Split(':'));
                if (strs[1] == Name)
                {
                    int id = int.Parse(strs[2]);
                    Player target = room.FindPlayer(strs[3]);
                    if (ai.GetPlayerTendency(target) != "unknown")
                        ai.UpdatePlayerRelation(player, target, ai.GetKeepValue(id, target) > 0);
                }
            }
        }
        public override bool OnSkillInvoke(TrustedAI ai, Player player, object data)
        {
            return true;
        }

        public override string OnChoice(TrustedAI ai, Player player, string choice, object data)
        {
            Room room = ai.Room;
            Player target = room.Current;
            if (ai.IsFriend(target))
            {
                for (int i = 0; i < 6; i++)
                {
                    if (target.GetEquip(i) >= 0 && ai.GetKeepValue(target.GetEquip(i), target, Player.Place.PlaceEquip) < 0
                        && player.GetEquip(i) < 0 && RoomLogic.CanPutEquip(player, room.GetCard(target.GetEquip(i))))
                        return "getequipc";
                }

                return "draw";
            }
            else if (ai.IsEnemy(target))
            {
                for (int i = 0; i < 6; i++)
                {
                    if (target.GetEquip(i) >= 0 && ai.GetKeepValue(target.GetEquip(i), target, Player.Place.PlaceEquip) > 0
                        && player.GetEquip(i) < 0 && RoomLogic.CanPutEquip(player, room.GetCard(target.GetEquip(i))))
                        return "getequipc";
                }

                return "draw";
            }

            return "getequipc";
        }

        public override List<int> OnCardsChosen(TrustedAI ai, Player from, Player to, string flags, int min, int max, List<int> disable_ids)
        {
            Room room = ai.Room;
            if (ai.IsFriend(to))
            {
                for (int i = 0; i < 6; i++)
                {
                    if (to.GetEquip(i) >= 0 && ai.GetKeepValue(to.GetEquip(i), to, Player.Place.PlaceEquip) < 0
                        && from.GetEquip(i) < 0 && RoomLogic.CanPutEquip(from, room.GetCard(to.GetEquip(i))))
                        return new List<int> { to.GetEquip(i) };
                }
            }
            else if (ai.IsEnemy(to))
            {
                if (to.GetTreasure() && !from.GetTreasure() && RoomLogic.CanPutEquip(from, room.GetCard(to.Treasure.Key)))
                    return new List<int> { to.Treasure.Key };
                if (to.GetArmor() && !from.GetArmor() && RoomLogic.CanPutEquip(from, room.GetCard(to.Armor.Key)))
                    return new List<int> { to.Armor.Key };
                if (to.GetDefensiveHorse() && !from.GetDefensiveHorse() && RoomLogic.CanPutEquip(from, room.GetCard(to.DefensiveHorse.Key)))
                    return new List<int> { to.DefensiveHorse.Key };
                if (to.GetWeapon() && !from.GetWeapon() && RoomLogic.CanPutEquip(from, room.GetCard(to.Weapon.Key)))
                    return new List<int> { to.Weapon.Key };
                if (to.GetOffensiveHorse() && !from.GetOffensiveHorse() && RoomLogic.CanPutEquip(from, room.GetCard(to.OffensiveHorse.Key)))
                    return new List<int> { to.OffensiveHorse.Key };
            }

            return base.OnCardsChosen(ai, from, to, flags, min, max, disable_ids);
        }
    }

    public class XianzhouAI : SkillEvent
    {
        public XianzhouAI() : base("xianzhou")
        {
        }

        public override List<WrappedCard> GetTurnUse(TrustedAI ai, Player player)
        {
            if (player.GetEquips().Count > 0 && player.GetMark("@handover") > 0)
            {
                Room room = ai.Room;
                foreach (int id in player.GetCards("h"))
                {
                    WrappedCard card = room.GetCard(id);
                    FunctionCard fcard = Engine.GetFunctionCard(card.Name);
                    if (fcard is EquipCard && ai.GetSameEquip(card, player) == null && RoomLogic.CanPutEquip(player, card))
                        return new List<WrappedCard>();
                }

                return new List<WrappedCard> { new WrappedCard(XianzhouCard.ClassName) { Skill = Name, Mute = true } };
            }

            return new List<WrappedCard>();
        }

        public override string OnChoice(TrustedAI ai, Player player, string choice, object data)
        {
            Room room = ai.Room;
            Player caifuren = null;
            foreach (Player p in room.GetOtherPlayers(player))
            {
                if (p.HasFlag("xianzhou_source"))
                {
                    caifuren = p;
                    break;
                }
            }
            int count = player.GetMark("xianzhou");

            double max = 0;
            Dictionary<Player, double> _points = new Dictionary<Player, double>();
            foreach (Player _p in room.GetOtherPlayers(player))
            {
                if (!RoomLogic.InMyAttackRange(room, player, _p)) continue;
                DamageStruct damage = new DamageStruct("xianzhou", player, _p);
                double value = ai.GetDamageScore(damage).Score;
                if (value > 0)
                    _points[_p] = value;
            }

            List<Player> targets = new List<Player>(_points.Keys);
            if (targets.Count > 0)
            {
                targets.Sort((x, y) => { return _points[x] > _points[y] ? -1 : 1; });

                for (int i = 0; i < Math.Min(targets.Count, count); i++)
                    max += _points[targets[i]];
            }

            double recover = 0;
            if (caifuren.Alive && ai.IsFriend(caifuren))
                recover = 3 * Math.Min(caifuren.GetLostHp(), count);

            if (max > recover) return "damage";

            return "recover";
        }

        public override List<Player> OnPlayerChosen(TrustedAI ai, Player player, List<Player> choose, int min, int max)
        {
            List<Player> result = new List<Player>();
            Room room = ai.Room;
            Dictionary<Player, double> _points = new Dictionary<Player, double>();
            foreach (Player _p in choose)
            {
                if (!RoomLogic.InMyAttackRange(room, player, _p)) continue;
                DamageStruct damage = new DamageStruct("xianzhou", player, _p);
                double value = ai.GetDamageScore(damage).Score;
                if (value > 0)
                    _points[_p] = value;
            }

            List<Player> targets = new List<Player>(_points.Keys);
            if (targets.Count > 0)
            {
                targets.Sort((x, y) => { return _points[x] > _points[y] ? -1 : 1; });

                for (int i = 0; i < Math.Min(targets.Count, max); i++)
                    result.Add(targets[i]);
            }

            return result;
        }
    }

    public class ZishouAI : SkillEvent
    {
        public ZishouAI() : base("zishou") { }
        public override bool OnSkillInvoke(TrustedAI ai, Player player, object data)
        {
            if (data is int n && player.HandcardNum + n + Zishou.GetAliveKingdoms(ai.Room) <= RoomLogic.GetMaxCards(ai.Room, player) + 1)
                return true;

            return false;
        }
    }

    public class XianzhouCardAI : UseCard
    {
        public XianzhouCardAI() : base(XianzhouCard.ClassName)
        {
        }

        public override void Use(TrustedAI ai, Player player, ref CardUseStruct use, WrappedCard card)
        {
            bool recover = false;
            if (card.SubCards.Count > 1 && player.GetLostHp() > 1) recover = true;
            Room room = ai.Room;
            List<Player> friends = ai.FriendNoSelf;
            ai.SortByDefense(ref friends, false);
            if (recover && friends.Count > 0)
            {
                use.Card = card;
                use.To.Add(friends[0]);
                return;
            }

            Dictionary<Player, double> points = new Dictionary<Player, double>();
            foreach (Player p in friends)
            {
                Dictionary<Player, double> _points = new Dictionary<Player, double>();
                foreach (Player _p in room.GetOtherPlayers(p))
                {
                    if (!RoomLogic.InMyAttackRange(room, p, _p)) continue;
                    DamageStruct damage = new DamageStruct("xianzhou", p, _p);
                    double value = ai.GetDamageScore(damage).Score;
                    if (value > 0)
                        _points[_p] = value;
                }

                List<Player> targets = new List<Player>(_points.Keys);
                if (targets.Count > 0)
                {
                    targets.Sort((x, y) => { return _points[x] > _points[y] ? -1 : 1; });
                    double max = 0;
                    for (int i = 0; i < Math.Min(targets.Count, card.SubCards.Count); i++)
                        max += _points[targets[i]];

                    points[p] = max;
                }
            }

            List<Player> _targets = new List<Player>(points.Keys);
            if (_targets.Count > 0)
            {
                _targets.Sort((x, y) => { return points[x] > points[y] ? -1 : 1; });
                if (points[_targets[0]] > 10)
                {
                    use.Card = card;
                    use.To.Add(_targets[0]);
                }
            }
        }

        public override double UsePriorityAdjust(TrustedAI ai, Player player, List<Player> targets, WrappedCard card)
        {
            return 4;
        }
    }

    public class JueceAI : SkillEvent
    {
        public JueceAI() : base("juece")
        { key = new List<string> { "playerChosen:juece" }; }
        public override void OnEvent(TrustedAI ai, TriggerEvent triggerEvent, Player player, object data)
        {
            if (triggerEvent == TriggerEvent.ChoiceMade && data is string str && player != ai.Self && ai is StupidAI _ai)
            {
                string[] strs = str.Split(':');
                if (strs[1] == Name)
                {
                    Room room = ai.Room;
                    Player target = room.FindPlayer(strs[2]);
                    if (ai.GetPlayerTendency(target) != "unknown" && !_ai.NeedDamage(new DamageStruct(Name, player, target)))
                        ai.UpdatePlayerRelation(player, target, false);
                }
            }
        }

        public override List<Player> OnPlayerChosen(TrustedAI ai, Player player, List<Player> targets, int min, int max)
        {
            List<ScoreStruct> scores = new List<ScoreStruct>();
            foreach (Player p in targets)
            {
                DamageStruct damage = new DamageStruct(Name, player, p);
                ScoreStruct score = ai.GetDamageScore(damage);
                score.Players = new List<Player> { p };
                scores.Add(score);
            }

            if (scores.Count > 0)
            {
                scores.Sort((x, y) => { return x.Score > y.Score ? -1 : 1; });
                if (scores[0].Score > 0) return scores[0].Players;
            }

            return new List<Player>();
        }
    }

    public class MiejiAI : SkillEvent
    {
        public MiejiAI() : base("mieji")
        {
        }

        public override List<WrappedCard> GetTurnUse(TrustedAI ai, Player player)
        {
            return base.GetTurnUse(ai, player);
        }
    }

    public class MiejiCardAI : UseCard
    {
        public MiejiCardAI() : base(MiejiCard.ClassName) { }

        public override void OnEvent(TrustedAI ai, TriggerEvent triggerEvent, Player player, object data)
        {
            if (triggerEvent == TriggerEvent.CardTargetAnnounced && data is CardUseStruct use && player != ai.Self)
            {
                Player target = use.To[0];
                if (ai.GetPlayerTendency(target) != "unknown")
                    ai.UpdatePlayerRelation(player, target, false);
            }
        }

        public override void Use(TrustedAI ai, Player player, ref CardUseStruct use, WrappedCard card)
        {
            base.Use(ai, player, ref use, card);
        }
    }

    public class FenchengAI : SkillEvent
    {
        public FenchengAI() : base("fencheng") { }

        public override List<WrappedCard> GetTurnUse(TrustedAI ai, Player player)
        {
            if (player.GetMark("@burn") > 0)
                return new List<WrappedCard> { new WrappedCard(FenchengCard.ClassName) { Skill = Name, Mute = true } };

            return new List<WrappedCard>();
        }

        public override CardUseStruct OnResponding(TrustedAI ai, Player player, string pattern, string prompt, object data)
        {
            Room room = ai.Room;
            int count = (int)room.GetTag(Name);
            CardUseStruct use = new CardUseStruct(null, player, new List<Player>());
            List<int> ids = new List<int>();
            foreach (int id in player.GetCards("he"))
                if (RoomLogic.CanDiscard(room, player, player, id))
                    ids.Add(id);
            DamageStruct damage = new DamageStruct(Name, room.Current, player, 2, DamageStruct.DamageNature.Fire);
            ScoreStruct score = ai.GetDamageScore(damage);
            double chain = ai.ChainDamage(damage);
            if (score.Score + chain >= 0) return use;
            if (ids.Count >= count)
            {
                Player next = room.GetNextAlive(player, 1, false);
                if (next != room.Current && !(player.Chained && next.Chained) && ai.IsFriend(next) && next.GetCardCount(true) < count + 1 && !next.IsNude())
                {
                    DamageStruct _damage = new DamageStruct(Name, room.Current, next, 2, DamageStruct.DamageNature.Fire);
                    ScoreStruct _score = ai.GetDamageScore(_damage);
                    double _chain = ai.ChainDamage(_damage);
                    if (_score.Score + _chain < -10 && _score.Score + _chain < score.Score + chain)
                        return use;
                }

                List<int> sub = new List<int>();
                double value = 0;
                List<double> values = ai.SortByKeepValue(ref ids, false);
                for (int i = 0; i < count; i++)
                {
                    value += values[i];
                    sub.Add(ids[i]);
                }

                if (value / 2 < Math.Abs(score.Score + chain))
                {
                    use.Card = new WrappedCard(FenchengCard.ClassName);
                    use.Card.AddSubCards(sub);
                    return use;
                }
            }

            return use;    
        }
    }

    public class FenchengCardAI : UseCard
    {
        public FenchengCardAI() : base(FenchengCard.ClassName) { }

        public override void Use(TrustedAI ai, Player player, ref CardUseStruct use, WrappedCard card)
        {
            base.Use(ai, player, ref use, card);
        }
    }

    public class ShouxiAI : SkillEvent
    {
        public ShouxiAI() : base("shouxi") { }

        public override string OnChoice(TrustedAI ai, Player player, string choice, object data)
        {
            if (data is CardUseStruct use && (!ai.IsFriend(use.From) || ai.GetPlayerTendency(player) == "unknown") && ai.IsCardEffect(use.Card, player, use.From))
            {
                List<string> choices = new List<string>(choice.Split('+'));
                choices.Remove("cancel");
                for (int i = choices.Count - 1; i >= 0; i--)
                    if (use.From.HasEquip(choices[i]))
                        return choices[i];
            }

            return "cancel";
        }

        public override List<int> OnExchange(TrustedAI ai, Player player, string pattern, int min, int max, string pile)
        {
            Room room = ai.Room;
            if (room.GetTag(Name) is CardUseStruct use)
            {
                Player target = null;
                foreach (Player p in room.GetOtherPlayers(player))
                {
                    if (p.HasFlag("shouxi_from"))
                    {
                        target = p;
                        break;
                    }
                }

                DamageStruct damage = new DamageStruct(use.Card, player, player, 1 + use.Drank + use.ExDamage);
                if (use.Card.Name == FireSlash.ClassName)
                    damage.Nature = DamageStruct.DamageNature.Fire;
                else if (use.Card.Name == ThunderSlash.ClassName)
                    damage.Nature = DamageStruct.DamageNature.Thunder;
                if (ai.IsEnemy(target) && ai.IsCardEffect(use.Card, target, player) && ai.GetDamageScore(damage).Score > 5)
                {
                    List<int> ids = new List<int>();
                    foreach (int id in player.GetCards("he"))
                        if ((room.GetCard(id).Name == pattern || (pattern == Slash.ClassName && room.GetCard(id).Name.Contains(Slash.ClassName)))
                            && RoomLogic.CanDiscard(room, player, player, id))
                            ids.Add(id);

                    if (ids.Count > 0)
                    {
                        ai.SortByUseValue(ref ids, false);
                        return new List<int> { ids[0] };
                    }
                }
            }

            return new List<int>();
        }
    }

    public class HuiminAI : SkillEvent
    {
        public HuiminAI() : base("huimin") { key = new List<string> { "playerChosen:huimin" }; }
        public override void OnEvent(TrustedAI ai, TriggerEvent triggerEvent, Player player, object data)
        {
            if (triggerEvent == TriggerEvent.ChoiceMade && data is string str && player != ai.Self)
            {
                string[] strs = str.Split(':');
                if (strs[1] == Name)
                {
                    Room room = ai.Room;
                    Player target = room.FindPlayer(strs[2]);
                    if (ai.GetPlayerTendency(target) != "unknown")
                        ai.UpdatePlayerRelation(player, target, true);
                }
            }
        }
        public override bool OnSkillInvoke(TrustedAI ai, Player player, object data)
        {
            int good = 0, bad = 0;
            foreach (Player p in ai.Room.GetAlivePlayers())
            {
                if (p.HandcardNum < p.Hp)
                {
                    if (ai.IsFriend(p))
                        good++;
                    else
                        bad++;
                }
            }

            return good > bad;
        }

        public override List<Player> OnPlayerChosen(TrustedAI ai, Player player, List<Player> targets, int min, int max)
        {
            ai.SortByDefense(ref targets, false);
            foreach (Player p in targets)
                if (ai.IsFriend(p)) return new List<Player> { p };

            return new List<Player> { targets[0] };
        }
    }

    public class XianzhenAI : SkillEvent
    {
        public XianzhenAI() : base("xianzhen")
        { }


        public override List<WrappedCard> GetTurnUse(TrustedAI ai, Player player)
        {
            if (ai.WillShowForAttack() && !player.IsKongcheng() && !player.HasUsed(XianzhenCard.ClassName))
            {
                WrappedCard card = new WrappedCard(XianzhenCard.ClassName)
                {
                    Skill = Name,
                    ShowSkill = Name
                };
                return new List<WrappedCard> { card };
            }

            return null;
        }
        public override double CardValue(TrustedAI ai, Player player, WrappedCard card, bool isUse, Player.Place place)
        {
            if (ai.HasSkill(Name, player) && !isUse)
            {
                if (RoomLogic.GetCardNumber(ai.Room, card) > 11)
                    return 1;
                if (card.Name.Contains(Slash.ClassName))
                    return 0.3;
            }

            return 0;
        }

        public override WrappedCard OnPindian(TrustedAI ai, Player requestor, List<Player> player)
        {
            Room room = ai.Room;
            if (requestor == ai.Self)
            {
                List<int> ids = ai.Self.GetCards("h");
                if (ai.GetKnownCards(player[0]).Count == player[0].HandcardNum)
                {
                    ai.SortByUseValue(ref ids, false);
                    WrappedCard pindian = null;
                    int app = ai.HasSkill("yingyang") ? 3 : 0;

                    int max_enemy = 0;
                    pindian = ai.GetMaxCard(player[0]);
                    if (pindian != null)
                    {
                        max_enemy = pindian.Number;
                        if (ai.HasSkill("yingyang"))
                            max_enemy = Math.Min(13, max_enemy + 3);

                        foreach (int id in ids)
                        {
                            if (!ai.IsCard(id, Peach.ClassName, requestor) && !ai.IsCard(id, Slash.ClassName, requestor) && !ai.IsCard(id, Analeptic.ClassName, requestor)
                                && Math.Min(13, room.GetCard(id).Number + app) > max_enemy)
                                return room.GetCard(id);
                        }
                    }
                }
                else
                {
                    WrappedCard max = ai.GetMaxCard(ai.Self, ai.Self.GetCards("h"), new List<string> { Peach.ClassName, Analeptic.ClassName, Slash.ClassName });
                    int max_number = 0;
                    if (max != null)
                    {
                        max_number = max.Number;
                        if (ai.HasSkill("yingyang"))
                            max_number = Math.Min(13, max_number + 3);

                        if (ai.IsEnemy(player[0]) && max_number > 10 || ai.IsFriend(player[0]))
                            return max;
                    }
                }

                ai.SortByKeepValue(ref ids, false);
                return ai.Room.GetCard(ids[0]);
            }
            else
            {
                WrappedCard max = ai.GetMaxCard(ai.Self, ai.Self.GetCards("h"), new List<string> { Peach.ClassName, Analeptic.ClassName });
                int max_number = 0;
                if (max != null)
                {
                    max_number = max.Number;
                    if (ai.HasSkill("yingyang"))
                        max_number = Math.Min(13, max_number + 3);
                    if (max_number > 10)
                        return max;
                }

                max = ai.GetMaxCard();
                if (max != null)
                {
                    max_number = max.Number;
                    if (ai.HasSkill("yingyang"))
                        max_number = Math.Min(13, max_number + 3);
                    if (max_number > 10)
                        return max;
                }

                List<int> ids = ai.Self.GetCards("h");
                ai.SortByKeepValue(ref ids, false);
                return ai.Room.GetCard(ids[0]);
            }
        }
    }

    public class ZhigeAI : SkillEvent
    {
        public ZhigeAI() : base("zhige") { }

        public override List<WrappedCard> GetTurnUse(TrustedAI ai, Player player)
        {
            if (!player.HasUsed(ZhigeCard.ClassName))
            {
                return new List<WrappedCard> { new WrappedCard(ZhigeCard.ClassName) { Skill = Name } };
            }

            return new List<WrappedCard>();
        }

        public override List<int> OnExchange(TrustedAI ai, Player player, string pattern, int min, int max, string pile)
        {
            Room room = ai.Room;
            Player target = room.Current;
            WrappedCard slash = new WrappedCard(Slash.ClassName);

            List<ScoreStruct> scores = new List<ScoreStruct>();
            if (ai.IsFriend(target))
            {
                foreach (Player p in room.GetOtherPlayers(player))
                {
                    if (RoomLogic.InMyAttackRange(room, player, p) && RoomLogic.IsProhibited(room, player, p , slash) == null
                        && ai.IsCardEffect(slash, p, player) && !ai.IsCancelTarget(slash, p, player))
                    {
                        ScoreStruct score = ai.SlashIsEffective(slash, p);
                        scores.Add(score);
                    }
                }

                double lose = 0;
                if (player.GetWeapon())
                {
                    double value = ai.GetKeepValue(player.Weapon.Key, player, Place.PlaceEquip);
                    if (value < 0)
                        lose = -value;
                }
                if (scores.Count > 0)
                {
                    scores.Sort((x, y) => { return x.Score > y.Score ? -1 : 1; });
                    if (scores[0].Score > lose)
                        return new List<int>();
                    else if (scores[0].Score < 0)
                    {
                        List<int> ids = player.GetCards("he");
                        ai.SortByKeepValue(ref ids, false);
                        foreach (int id in ids)
                        {
                            WrappedCard card = room.GetCard(id);
                            if (Engine.MatchExpPattern(room, pattern, player, card))
                                return new List<int> { id };
                        }
                    }
                }

                if (lose > 0)
                    return new List<int> { player.Weapon.Key };
            }
            else
            {
                if (player.GetWeapon())
                {
                    double value = ai.GetKeepValue(player.Weapon.Key, player, Place.PlaceEquip);
                    if (value < 0)
                        return new List<int> { player.Weapon.Key };
                }

                foreach (Player p in room.GetOtherPlayers(player))
                {
                    if (RoomLogic.InMyAttackRange(room, player, p) && RoomLogic.IsProhibited(room, player, p, slash) == null
                        && ai.IsCardEffect(slash, p, player) && !ai.IsCancelTarget(slash, p, player))
                    {
                        ScoreStruct score = ai.SlashIsEffective(slash, p);
                        scores.Add(score);
                    }
                }

                if (scores.Count > 0)
                {
                    scores.Sort((x, y) => { return x.Score < y.Score ? -1 : 1; });
                    if (scores[0].Score < -3)
                    {
                        List<int> ids = player.GetCards("he");
                        ai.SortByKeepValue(ref ids, false);
                        foreach (int id in ids)
                        {
                            WrappedCard card = room.GetCard(id);
                            if (Engine.MatchExpPattern(room, pattern, player, card))
                                return new List<int> { id };
                        }
                    }
                }
            }

            return new List<int>();
        }

        public override List<Player> OnPlayerChosen(TrustedAI ai, Player player, List<Player> targets, int min, int max)
        {
            Player slasher = null;
            Room room = ai.Room;
            foreach (Player p in room.GetAlivePlayers())
            {
                if (p.HasFlag("zhige_slasher"))
                {
                    slasher = p;
                    break;
                }
            }
            WrappedCard slash = new WrappedCard(Slash.ClassName);
            List<ScoreStruct> scores = new List<ScoreStruct>();
            foreach (Player p in targets)
            {
                if (ai.IsCardEffect(slash, p, slasher) && !ai.IsCancelTarget(slash, p, slasher))
                {
                    ScoreStruct score = ai.SlashIsEffective(slash, slasher, p);
                    score.Players = new List<Player> { p };
                    scores.Add(score);
                }
            }
            if (scores.Count > 0)
            {
                scores.Sort((x, y) => { return x.Score > y.Score ? -1 : 1; });
                if (scores[0].Score > 0)
                {
                    return scores[0].Players;
                }
            }

            scores.Clear();
            foreach (Player p in targets)
            {
                ScoreStruct score = ai.SlashIsEffective(slash, slasher, p);
                score.Players = new List<Player> { p };
                scores.Add(score);
            }
            if (scores.Count > 0)
            {
                scores.Sort((x, y) => { return x.Score > y.Score ? -1 : 1; });
                return scores[0].Players;
            }

            return base.OnPlayerChosen(ai, player, targets, min, max);
        }
    }

    public class ZhigeCardAI : UseCard
    {
        public ZhigeCardAI() : base(ZhigeCard.ClassName)
        {
        }

        public override void OnEvent(TrustedAI ai, TriggerEvent triggerEvent, Player player, object data)
        {
            if (triggerEvent == TriggerEvent.CardTargetAnnounced && data is CardUseStruct use && ai.Self != player)
            {
                Player target = use.To[0];
                if (ai.GetPlayerTendency(target) != "unknown")
                {
                    if (target.GetWeapon() && ai.GetKeepValue(target.Weapon.Key, target, Place.PlaceEquip) < 0)
                        ai.UpdatePlayerRelation(player, target, true);
                }
            }
        }

        public override void Use(TrustedAI ai, Player player, ref CardUseStruct use, WrappedCard card)
        {
            List<Player> targets = new List<Player>();
            Room room = ai.Room;
            foreach (Player p in room.GetOtherPlayers(player))
                if (RoomLogic.InMyAttackRange(room, player, p))
                    targets.Add(p);

            if (targets.Count > 0)
            {
                WrappedCard slash = new WrappedCard(Slash.ClassName);
                List<ScoreStruct> scores = new List<ScoreStruct>();
                
                foreach (Player slasher in targets)
                {
                    if (!ai.IsFriend(slasher) && slasher.GetWeapon() && ai.GetKeepValue(slasher.Weapon.Key, slasher, Place.PlaceEquip) < 0) continue;
                    foreach (Player p in room.GetOtherPlayers(slasher))
                    {
                        if (RoomLogic.InMyAttackRange(room, slasher, p) && RoomLogic.IsProhibited(room, slasher, p, slash) == null
                            && ai.IsCardEffect(slash, p, slasher) && !ai.IsCancelTarget(slash, p, slasher))
                        {
                            ScoreStruct score = ai.SlashIsEffective(slash, slasher, p);
                            score.Players = new List<Player> { slasher };
                            if (score.Score > 0)
                            {
                                if (ai.IsFriend(slasher))
                                    score.Score += 0.5;

                                scores.Add(score);
                            }
                        }
                    }
                }

                if (scores.Count > 0)
                {
                    scores.Sort((x, y) => { return x.Score > y.Score ? -1 : 1; });
                    use.Card = card;
                    use.To = scores[0].Players;
                    return;
                }

                foreach (Player slasher in targets)
                {
                    if (ai.IsFriend(slasher) && slasher.GetWeapon() && ai.GetKeepValue(slasher.Weapon.Key, slasher, Place.PlaceEquip) < 0)
                    {
                        use.Card = card;
                        use.To = new List<Player> { slasher };
                        return;
                    }
                }

                scores.Clear();
                foreach (Player slasher in targets)
                {
                    foreach (Player p in room.GetOtherPlayers(slasher))
                    {
                        if (RoomLogic.InMyAttackRange(room, slasher, p) && RoomLogic.IsProhibited(room, slasher, p, slash) == null)
                        {
                            ScoreStruct score = ai.SlashIsEffective(slash, slasher, p);
                            score.Players = new List<Player> { slasher };
                            if (ai.IsFriend(slasher))
                                score.Score += 0.5;

                            scores.Add(score);
                        }
                    }
                }

                if (scores.Count > 0)
                {
                    scores.Sort((x, y) => { return x.Score > y.Score ? -1 : 1; });
                    if (scores[0].Score >= 0)
                    {
                        use.Card = card;
                        use.To = scores[0].Players;
                        return;
                    }
                }
            }
        }

        public override double UsePriorityAdjust(TrustedAI ai, Player player, List<Player> targets, WrappedCard card)
        {
            return 5;
        }
    }

    public class HuaiyiAI : SkillEvent
    {
        public HuaiyiAI() : base("huaiyi")
        {
            key = new List<string> { "cardChosen:huaiyi" };
        }

        public override List<WrappedCard> GetTurnUse(TrustedAI ai, Player player)
        {
            ai.Choice[Name] = string.Empty;
            ai.Number[Name] = 2.8;
            List<int> ids = player.GetCards("h");
            Room room = ai.Room;
            List<int> black = new List<int>(), red = new List<int>();
            double red_value = 0, black_value = 0;
            foreach (int id in ids)
            {
                if (!RoomLogic.CanDiscard(room, player, player, id)) continue;
                WrappedCard card = room.GetCard(id);
                if (WrappedCard.IsBlack(card.Suit))
                {
                    black.Add(id);
                    black_value += ai.GetUseValue(id, player);
                }
                else
                {
                    red.Add(id);
                    red_value += ai.GetUseValue(id, player);
                }
            }

            if (black.Count > 0 && red.Count > 0)
            {
                List<ScoreStruct> scores = new List<ScoreStruct>();
                foreach (Player p in room.GetOtherPlayers(player))
                {
                    if (!p.IsNude() && RoomLogic.CanGetCard(room, player, p, "he"))
                        scores.Add(ai.FindCards2Discard(player, p, Name, "he", HandlingMethod.MethodGet));
                }

                if (scores.Count > 0)
                {
                    double red_get_value = 0, black_get_value = 0;
                    scores.Sort((x, y) => { return x.Score > y.Score ? -1 : 1; });
                    for (int i = 0; i < scores.Count; i++)
                    {
                        if (i < red.Count)
                        {
                            red_get_value += scores[i].Score;
                            if (i == 2)
                            {
                                red_get_value -= player.Hp > 2 ? 2 : 3;
                                if (player.Hp == 1)
                                    red_get_value -= 4;
                            }
                        }

                        if (i < black.Count)
                        {
                            black_get_value += scores[i].Score;
                            if (i == 2)
                            {
                                red_get_value -= player.Hp > 2 ? 2 : 3;
                                if (player.Hp == 1)
                                    black_get_value -= 4;
                            }
                        }
                    }

                    double do_black = black_get_value - black_value, do_red = red_get_value - red_value;
                    if (do_black > 0 || do_red > 0)
                    {
                        if (do_red > do_black)
                            ai.Choice[Name] = "red";
                        else
                            ai.Choice[Name] = "black";

                        return new List<WrappedCard> { new WrappedCard(HuaiyiCard.ClassName) { Skill = Name } };
                    }

                    if (ai.GetOverflow(player) > 0)
                    {
                        double one = scores[0].Score;
                        if (one > 0)
                        {
                            if (red_value > black_value)
                                ai.Choice[Name] = "black";
                            else
                                ai.Choice[Name] = "red";
                            ai.Number[Name] = 0.5;
                            return new List<WrappedCard> { new WrappedCard(HuaiyiCard.ClassName) { Skill = Name } };
                        }
                    }
                }
            }

            return new List<WrappedCard>();
        }
        public override List<Player> OnPlayerChosen(TrustedAI ai, Player player, List<Player> targets, int min, int max)
        {
            List<ScoreStruct> scores = new List<ScoreStruct>();
            Room room = ai.Room;
            List<Player> result = new List<Player>();
            foreach (Player p in targets)
            {
                ScoreStruct score = ai.FindCards2Discard(player, p, Name, "he", HandlingMethod.MethodGet);
                score.Players = new List<Player> { p };
                scores.Add(score);
            }

            if (scores.Count > 0)
            {
                double get_value = 0;
                scores.Sort((x, y) => { return x.Score > y.Score ? -1 : 1; });
                for (int i = 0; i < Math.Min(max, scores.Count); i++)
                {
                    get_value += scores[i].Score;
                    if (i == 2)
                    {
                        get_value -= player.Hp > 2 ? 2 : 3;
                        if (player.Hp == 1)
                            get_value -= 4;

                        if (get_value <= 0) return scores[0].Players;
                    }
                    result.AddRange(scores[i].Players);
                }
            }

            return result;
        }

        public override string OnChoice(TrustedAI ai, Player player, string choice, object data)
        {
            if (!string.IsNullOrEmpty(ai.Choice[Name])) return ai.Choice[Name];

            return "black";
        }

        public override void OnEvent(TrustedAI ai, TriggerEvent triggerEvent, Player player, object data)
        {
            //针对所选择的卡牌判断敌友
            if (ai.Self == player && !(ai is StupidAI)) return;
            Room room = ai.Room;
            if (triggerEvent == TriggerEvent.ChoiceMade && data is string str)
            {
                List<string> strs = new List<string>(str.Split(':'));
                if (strs[1] == Name)
                {
                    int card_id = int.Parse(strs[2]);
                    Player target = room.FindPlayer(strs[4]);

                    if (room.GetCardPlace(card_id) == Place.PlaceHand)
                    {
                        ai.UpdatePlayerRelation(player, target, ai.HasSkill("kongcheng|kongcheng_jx") && target.HandcardNum == 1 ? true : false);
                    }
                    else if (room.GetCardPlace(card_id) == Place.PlaceEquip)
                    {
                        bool friendly = ai.GetKeepValue(card_id, target, Place.PlaceEquip) < 0;

                        if (ai is StupidAI)
                        {
                            if (target != ai.Self || ai.Self.GetRoleEnum() == PlayerRole.Lord)
                            {
                                if (target.GetRoleEnum() == PlayerRole.Lord)
                                {
                                    ai.UpdatePlayerIntention(player, friendly ? "loyalist" : "rebel", friendly ? 80 : 40);
                                }
                                else if (ai.GetPlayerTendency(target) == "loyalist" && player.GetRoleEnum() != PlayerRole.Lord)
                                {
                                    ai.UpdatePlayerIntention(player, friendly ? "loyalist" : "rebel", friendly ? 80 : 40);
                                }
                                else if (ai.GetPlayerTendency(target) == "rebel" && player.GetRoleEnum() != PlayerRole.Lord)
                                {
                                    ai.UpdatePlayerIntention(player, friendly ? "rebel" : "loyalist", friendly ? 80 : 40);
                                }
                            }
                            else if (ai.Self == target && ai.GetPlayerTendency(ai.Self) == "rebel")
                            {
                                ai.UpdatePlayerIntention(player, "loyalist", friendly ? 60 : 30);
                            }
                            else if (ai.Self == target && ai.GetPlayerTendency(ai.Self) == "loyalist")
                            {
                                ai.UpdatePlayerIntention(player, "rebel", friendly ? 60 : 30);
                            }
                        }
                        else
                            ai.UpdatePlayerRelation(player, target, friendly);
                    }
                }
            }
        }
    }

    public class HuaiyiCardAI : UseCard
    {
        public HuaiyiCardAI() : base(HuaiyiCard.ClassName)
        {}

        public override void Use(TrustedAI ai, Player player, ref CardUseStruct use, WrappedCard card)
        {
            use.Card = card;
        }

        public override double UsePriorityAdjust(TrustedAI ai, Player player, List<Player> targets, WrappedCard card)
        {
            return ai.Number["huaiyi"];
        }
    }

    public class XianzhenCardAI : UseCard
    {
        public XianzhenCardAI() : base(XianzhenCard.ClassName)
        {}

        public override void OnEvent(TrustedAI ai, TriggerEvent triggerEvent, Player player, object data)
        {
            if (triggerEvent == TriggerEvent.CardTargetAnnounced && data is CardUseStruct use && ai.Self != player)
            {
                Player target = use.To[0];
                if (ai.GetPlayerTendency(target) != "unknown")
                    ai.UpdatePlayerRelation(player, target, false);
            }
        }

        public override double UsePriorityAdjust(TrustedAI ai, Player player, List<Player> targets, WrappedCard card)
        {
            WrappedCard max = ai.GetMaxCard(player, player.GetCards("h"), new List<string> { Peach.ClassName, Analeptic.ClassName, Slash.ClassName });
            int max_number = 0;
            if (max != null)
            {
                max_number = max.Number;
                if (ai.HasSkill("yingyang"))
                    max_number = Math.Min(13, max_number + 3);

                if (max_number >= 12) return 3.5;
            }

            return 2;
        }
        public override void Use(TrustedAI ai, Player player, ref CardUseStruct use, WrappedCard card)
        {
            List<Player> enemies = ai.GetPrioEnemies();
            ai.SortByDefense(ref enemies, false);
            Room room = ai.Room;
            WrappedCard max = ai.GetMaxCard(player, player.GetCards("h"), new List<string> { Peach.ClassName, Analeptic.ClassName, Slash.ClassName });
            int max_number = 0;
            if (max != null)
            {
                max_number = max.Number;
                if (ai.HasSkill("yingyang"))
                    max_number = Math.Min(13, max_number + 3);

                List<Player> known = new List<Player>();
                foreach (Player p in enemies)
                {
                    if (!RoomLogic.CanBePindianBy(room, p, player) || ai.GetKnownCards(p).Count != p.HandcardNum) continue;
                    known.Add(p);
                    WrappedCard max_enemy = ai.GetMaxCard(p);
                    if (max_enemy != null)
                    {
                        int enemy = max_enemy.Number;
                        if (ai.HasSkill("yingyang", p))
                            enemy = Math.Min(13, enemy + 3);

                        if (enemy >= max_number)
                            continue;
                    }

                    use.Card = card;
                    use.To.Add(p);
                    return;
                }

                if (max_number >= 12)
                {
                    foreach (Player p in enemies)
                    {
                        if (!RoomLogic.CanBePindianBy(room, p, player) || known.Contains(p)) continue;

                        WrappedCard max_enemy = ai.GetMaxCard(p);
                        if (max_enemy != null)
                        {
                            int enemy = max_enemy.Number;
                            if (ai.HasSkill("yingyang", p))
                                enemy = Math.Min(13, enemy + 3);

                            if (enemy >= max_number)
                                continue;
                        }

                        use.Card = card;
                        use.To.Add(p);
                        return;
                    }
                }

                if (max_number > 10)
                {
                    foreach (Player p in enemies)
                    {
                        if (!RoomLogic.CanBePindianBy(room, p, player) || known.Contains(p)) continue;

                        WrappedCard max_enemy = ai.GetMaxCard(p);
                        if (max_enemy != null)
                        {
                            int enemy = max_enemy.Number;
                            if (ai.HasSkill("yingyang", p))
                                enemy = Math.Min(13, enemy + 3);

                            if (enemy >= max_number)
                                continue;
                        }

                        use.Card = card;
                        use.To.Add(p);
                        return;
                    }
                }
                else if (ai.GetOverflow(player) > 0)
                {
                    foreach (Player p in enemies)
                    {
                        if (!RoomLogic.CanBePindianBy(room, p, player)) continue;
                        use.Card = card;
                        use.To.Add(p);
                        return;
                    }
                }
            }
        }
    }

    public class YuanJXAI : SkillEvent
    {
        public YuanJXAI() : base("enyuan_jx_yuan")
        {
        }

        public override bool OnSkillInvoke(TrustedAI ai, Player player, object data)
        {
            return data is Player target && !ai.IsFriend(target);
        }
    }
    public class EnJXAI : SkillEvent
    {
        public EnJXAI() : base("enyuan_jx_en")
        {
            key = new List<string> { "skillInvoke:enyuan_jx_en" };
        }

        public override void OnEvent(TrustedAI ai, TriggerEvent triggerEvent, Player player, object data)
        {
            if (triggerEvent == TriggerEvent.ChoiceMade && data is string str && player != ai.Self)
            {
                List<string> strs = new List<string>(str.Split(':'));
                if (strs[1] == Name && strs[2] == "yes")
                {
                    Player target = null;
                    foreach (Player p in ai.Room.GetAlivePlayers())
                    {
                        if (p.HasFlag("en_target"))
                        {
                            target = p;
                            break;
                        }
                    }
                    if (ai.GetPlayerTendency(target) != "unknown") ai.UpdatePlayerRelation(player, target, true);
                }
            }
        }

        public override bool OnSkillInvoke(TrustedAI ai, Player player, object data)
        {
            return data is Player target && ai.IsFriend(target);
        }
    }

    public class XuanhuoJXAI : SkillEvent
    {
        public XuanhuoJXAI() : base("xuanhuo_jx")
        {
        }

        public override List<Player> OnPlayerChosen(TrustedAI ai, Player player, List<Player> targets, int min, int max)
        {
            Player target = null;
            Room room = ai.Room;
            foreach (Player p in room.GetOtherPlayers(player))
            {
                if (p.HasFlag("xuanhuo_source"))
                {
                    target = p;
                    break;
                }
            }

            if (target == null)
            {
                List<Player> friends = ai.FriendNoSelf;
                if (friends.Count > 0)
                {
                    ai.SortByDefense(ref friends, false);
                    foreach (Player p in friends)
                    {
                        if (ai.HasSkill("tuntian", p))
                            return new List<Player> { p };
                    }

                    foreach (Player p in friends)
                    {
                        foreach (int id in p.GetEquips())
                        {
                            if (ai.GetKeepValue(id, p, Player.Place.PlaceEquip) < 0)
                                return new List<Player> { p };
                        }
                    }

                    return new List<Player> { friends[0] };
                }
            }
            else
            {
                return new List<Player> { targets[0] };
            }

            return new List<Player>();
        }

        public override CardUseStruct OnResponding(TrustedAI ai, Player player, string pattern, string prompt, object data)
        {
            CardUseStruct use = new CardUseStruct
            {
                From = player
            };
            string[] strs = prompt.Split(':');
            Room room = ai.Room;
            Player source = room.FindPlayer(strs[1], true);
            Player victim = room.FindPlayer(strs[2], true);
            if (!ai.IsFriend(source))
            {
                List<ScoreStruct> scores = ai.CaculateSlashIncome(player, null, new List<Player> { victim }, false);
                if (scores.Count > 0 && scores[0].Score >= 0 && scores[0].Card != null)
                {
                    use.Card = scores[0].Card;
                    use.To = scores[0].Players;
                }
            }

            return use;
        }
    }

    public class ZhimanJXAI : SkillEvent
    {
        public ZhimanJXAI() : base("zhiman_jx")
        {
            key = new List<string> { "skillInvoke:zhiman_jx", "cardChosen:zhiman_jx" };
        }

        public override void OnEvent(TrustedAI ai, TriggerEvent triggerEvent, Player player, object data)
        {
            if (triggerEvent == TriggerEvent.ChoiceMade && data is string str && player != ai.Self && ai is StupidAI _ai)
            {
                string[] strs = str.Split(':');
                Room room = ai.Room;
                if (strs[0] == "skillInvoke" && strs[1] == Name && room.GetTag("zhiman_data") is DamageStruct damage)
                {
                    Player target = damage.To;
                    if (ai.GetPlayerTendency(target) != "unknown")
                    {
                        bool good = target.JudgingArea.Count > 0;
                        if (!good)
                        {
                            foreach (int id in target.GetEquips())
                            {
                                if (ai.GetKeepValue(id, target, Place.PlaceEquip) < 0)
                                {
                                    good = true;
                                    break;
                                }
                            }
                        }
                        if (good && strs[2] == "yes")
                            return;
                        else if (_ai.NeedDamage(damage) && !good)
                        {
                            ai.UpdatePlayerRelation(player, target, strs[2] == "no");
                        }
                        else if (player.GetCards("ej").Count == 0 && strs[2] == "yes")
                            ai.UpdatePlayerRelation(player, target, true);
                    }
                }
                else if (strs[0] == "cardChosen" && strs[1] == Name)
                {
                    int card_id = int.Parse(strs[2]);
                    Player target = room.FindPlayer(strs[4]);

                    if (room.GetCardPlace(card_id) == Place.PlaceJudge)
                    {
                        if (room.GetCard(card_id).Name != Lightning.ClassName)
                            ai.UpdatePlayerRelation(player, target, true);
                        else
                        {
                            Player winner = ai.GetWizzardRaceWinner(room.GetCard(card_id).Name, target, target);
                            if (winner != null && ai.IsFriend(winner, target))
                                ai.UpdatePlayerRelation(player, target, false);
                            else
                                ai.UpdatePlayerRelation(player, target, true);
                        }
                    }
                    else if (room.GetCardPlace(card_id) == Place.PlaceEquip && ai.GetKeepValue(card_id, target, Place.PlaceEquip) > 0)
                    {
                        ai.UpdatePlayerRelation(player, target, true);
                    }
                }
            }
        }

        public override bool OnSkillInvoke(TrustedAI ai, Player player, object data)
        {
            Room room = ai.Room;
            if ((ai.WillShowForAttack() || ai.WillShowForDefence())
                && room.ContainsTag("zhiman_data") && room.GetTag("zhiman_data") is DamageStruct damage)
            {
                ScoreStruct get = ai.FindCards2Discard(player, damage.To, string.Empty, "ej", HandlingMethod.MethodGet);
                ScoreStruct score = ai.GetDamageScore(damage);

                //if (get.Score < score.Score)
                //{
                //    Debug.Assert(ai.IsEnemy(damage.To));
                //}

                return get.Score >= score.Score;
            }

            return false;
        }
    }

    public class AnxuAI : SkillEvent
    {
        public AnxuAI() : base("anxu")
        {
        }

        public override List<WrappedCard> GetTurnUse(TrustedAI ai, Player player)
        {
            if (!player.HasUsed(AnxuCard.ClassName))
                return new List<WrappedCard> { new WrappedCard(AnxuCard.ClassName) { Skill = Name } };

            return new List<WrappedCard>();
        }
    }

    public class FumianAI : SkillEvent
    {
        public FumianAI() : base("fumian") { }

        public override string OnChoice(TrustedAI ai, Player player, string choice, object data)
        {
            if (ai.WillSkipDrawPhase(player) || (player.GetMark(Name) > 0 && player.GetMark(Name) < 3)) return "target";
            if (ai.WillSkipPlayPhase(player)) return "draw";
            return "draw";
        }

        public override List<Player> OnPlayerChosen(TrustedAI ai, Player player, List<Player> targets, int min, int max)
        {
            Room room = ai.Room;
            List<Player> result = new List<Player>();
            if (room.GetTag(Name) is CardUseStruct use)
            {
                if (use.Card.Name == Slash.ClassName)
                {
                    List<ScoreStruct> scores = ai.CaculateSlashIncome(player, new List<WrappedCard> { use.Card }, targets, false);
                    if (scores.Count > 0)
                    {
                        foreach (ScoreStruct score in scores)
                        {
                            if (score.Score > 0)
                            {
                                bool contain = false;
                                foreach (Player p in score.Players)
                                {
                                    if (result.Contains(p))
                                    {
                                        contain = true;
                                        break;
                                    }
                                }
                                if (contain) continue;
                                int count = result.Count;
                                for (int i = 0; i < Math.Min(score.Players.Count, max - count); i++)
                                    result.Add(score.Players[i]);

                                if (result.Count >= max) break;
                            }
                        }
                    }
                }
                else if (use.Card.Name == Peach.ClassName || use.Card.Name == ExNihilo.ClassName)
                {
                    ai.SortByDefense(ref targets, false);
                    foreach (Player p in targets)
                        if (ai.IsFriend(p) && result.Count < max)
                            result.Add(p);
                }
                else if (use.Card.Name == Dismantlement.ClassName)
                {
                    List<Player> players = new List<Player>();
                    foreach (Player p in targets)
                        if (!ai.IsFriend(p)) players.Add(p);

                    foreach (Player p in players)
                        if (result.Count < max && ai.FindCards2Discard(player, p, Dismantlement.ClassName, "hej", HandlingMethod.MethodDiscard).Score > 0)
                            result.Add(p);
                }
                else if (use.Card.Name == Snatch.ClassName)
                {
                    List<Player> players = new List<Player>();
                    foreach (Player p in targets)
                        if (!ai.IsFriend(p)) players.Add(p);

                    foreach (Player p in players)
                        if (result.Count < max && ai.FindCards2Discard(player, p, Snatch.ClassName, "hej", HandlingMethod.MethodGet).Score > 0)
                            result.Add(p);
                }
            }

            return result;
        }
    }

    public class DaiyanAI : SkillEvent
    {
        public DaiyanAI() : base("daiyan") { }

        public override List<Player> OnPlayerChosen(TrustedAI ai, Player player, List<Player> targets, int min, int max)
        {
            List<Player> friends = ai.FriendNoSelf;
            if (friends.Count > 0)
            {
                ai.SortByDefense(ref friends, false);
                foreach (Player p in friends)
                    if (!player.ContainsTag(Name) || (string)player.GetTag(Name) != p.Name)
                        return new List<Player> { p };
            }
            else if (ai.IsSituationClear())
            {
                List<Player> enemis = ai.GetEnemies(player);
                ai.SortByDefense(ref enemis, false);
                Player target = null;
                if (player.ContainsTag(Name))
                {
                    target = ai.Room.FindPlayer((string)player.GetTag(Name), true);
                    if (!target.Alive)
                        target = null;
                }
                if (target == null)
                {
                    if (enemis.Count > 0)
                        return new List<Player> { enemis[0] };
                }
                else if (targets.Contains(target))
                    return new List<Player> { target };
            }

            return new List<Player>();
        }
    }

    public class QiaoshiAI : SkillEvent
    {
        public QiaoshiAI() : base("qiaoshi")
        {
            key = new List<string> { "skillInvoke:qiaoshi" };
        }

        public override void OnEvent(TrustedAI ai, TriggerEvent triggerEvent, Player player, object data)
        {
            if (triggerEvent == TriggerEvent.ChoiceMade && data is string str && player != ai.Self)
            {
                Room room = ai.Room;
                List<string> strs = new List<string>(str.Split(':'));
                if (strs[1] == Name && strs[2] == "yes")
                {
                    Player target = room.Current;
                    if (ai.GetPlayerTendency(target) != "unknown") ai.UpdatePlayerRelation(player, target, true);
                }
                else if (strs[1] == Name && strs[2] == "no" && ai is StupidAI _ai && ai.GetPlayerTendency(room.Current) != "unknown")
                {
                    Player to = room.Current;
                    if (_ai.GetPlayerTendency(to) == "loyalist" || to.GetRoleEnum() == PlayerRole.Lord)
                        _ai.UpdatePlayerIntention(player, "rebel", 60);
                    else
                        _ai.UpdatePlayerIntention(player, "loyalist", 60);
                }
            }
        }
        public override bool OnSkillInvoke(TrustedAI ai, Player player, object data)
        {
            return ai.IsFriend(ai.Room.Current);
        }
    }

    public class YanyuAI : SkillEvent
    {
        public YanyuAI() : base("yanyu") { key = new List<string> { "playerChosen:yanyu" }; }
        public override void OnEvent(TrustedAI ai, TriggerEvent triggerEvent, Player player, object data)
        {
            if (triggerEvent == TriggerEvent.ChoiceMade && data is string str && player != ai.Self)
            {
                string[] strs = str.Split(':');
                if (strs[1] == Name)
                {
                    Room room = ai.Room;
                    Player target = room.FindPlayer(strs[2]);
                    if (ai.GetPlayerTendency(target) != "unknown")
                            ai.UpdatePlayerRelation(player, target, true);
                }
            }
        }
        public override List<WrappedCard> GetTurnUse(TrustedAI ai, Player player)
        {
            Room room = ai.Room;
            List<int> ids = new List<int>();
            foreach (int id in player.GetCards("h"))
                if (room.GetCard(id).Name.Contains(Slash.ClassName))
                    ids.Add(id);


            if (ids.Count > 0)
            {
                bool male = false;
                foreach (Player p in ai.GetFriends(player))
                {
                    if (p.IsMale())
                    {
                        male = true;
                        break;
                    }
                }
                ai.SortByUseValue(ref ids, false);
                if (male)
                {
                    ai.Number[Name] = 5;
                    if (player.GetMark(Name) < 2 || ai.GetOverflow(player) > 0)
                    {
                        if (player.GetMark(Name) >= 2)
                            ai.Number[Name] = 0.1;

                        WrappedCard card = new WrappedCard(YanyuCard.ClassName) { Skill = Name };
                        card.AddSubCard(ids[0]);
                        return new List<WrappedCard> { card };
                    }
                }
                else if (ai.GetOverflow(player) > 0 || ids.Count > 0)
                {
                    ai.Number[Name] = 0.1;
                    WrappedCard card = new WrappedCard(YanyuCard.ClassName) { Skill = Name };
                    card.AddSubCard(ids[0]);
                    return new List<WrappedCard> { card };
                }
            }

            return new List<WrappedCard>();
        }

        public override List<Player> OnPlayerChosen(TrustedAI ai, Player player, List<Player> targets, int min, int max)
        {
            ai.Room.SortByActionOrder(ref targets);
            foreach (Player p in targets)
                if (ai.IsFriend(p) && !ai.WillSkipPlayPhase(p))
                    return new List<Player> { p };

            foreach (Player p in targets)
                if (ai.IsFriend(p))
                    return new List<Player> { p };

            return new List<Player>();
        }
    }

    public class YanyuCardAI : UseCard
    {
        public YanyuCardAI() : base(YanyuCard.ClassName) { }

        public override void Use(TrustedAI ai, Player player, ref CardUseStruct use, WrappedCard card)
        {
            use.Card = card;
        }

        public override double UsePriorityAdjust(TrustedAI ai, Player player, List<Player> targets, WrappedCard card)
        {
            return ai.Number["yanyu"];
        }
    }

    public class XiantuAI : SkillEvent
    {
        public XiantuAI() : base("xiantu")
        {
            key = new List<string> { "skillInvoke:xiantu" };
        }

        public override void OnEvent(TrustedAI ai, TriggerEvent triggerEvent, Player player, object data)
        {
            if (triggerEvent == TriggerEvent.ChoiceMade && data is string str && player != ai.Self)
            {
                Room room = ai.Room;
                List<string> strs = new List<string>(str.Split(':'));
                if (strs[1] == Name && strs[2] == "yes")
                {
                    Player target = room.Current;
                    if (ai.GetPlayerTendency(target) != "unknown") ai.UpdatePlayerRelation(player, target, true);
                }
                else if (strs[1] == Name && strs[2] == "no" && ai is StupidAI _ai && room.Current.GetRoleEnum() == PlayerRole.Lord)
                {
                    _ai.UpdatePlayerIntention(player, "rebel", 50);
                }
            }
        }

        public override bool OnSkillInvoke(TrustedAI ai, Player player, object data)
        {
            if (data is Player target)
                return ai.IsFriend(target);

            return false;
        }

        public override List<int> OnExchange(TrustedAI ai, Player player, string pattern, int min, int max, string pile)
        {
            List<int> result = new List<int>();
            List<int> ids = player.GetCards("he");
            List<double> values = ai.SortByKeepValue(ref ids, false);
            for (int i = 0; i < 2; i++)
                if (values[i] < 0)
                    result.Add(ids[i]);

            if (result.Count < 2)
            {
                ai.SortByUseValue(ref ids);
                foreach (int id in ids)
                {
                    if (!result.Contains(id))
                        result.Add(id);

                    if (result.Count >= 2)
                        break;
                }
            }

            return result;
        }
    }

    public class QiangzhiAI : SkillEvent
    {
        public QiangzhiAI() : base("qiangzhi") { }

        public override List<Player> OnPlayerChosen(TrustedAI ai, Player player, List<Player> targets, int min, int max)
        {
            List<Player> enemies = ai.GetEnemies(player);
            if (enemies.Count > 0)
            {
                ai.SortByDefense(ref enemies, false);
                foreach (Player p in enemies)
                    if (!p.IsKongcheng()) return new List<Player> { p };
            }

            return new List<Player> { targets[0] };
        }

        public override bool OnSkillInvoke(TrustedAI ai, Player player, object data)
        {
            return true;
        }
    }

    public class DangxianJXAI : SkillEvent
    {
        public DangxianJXAI() : base("dangxian_jx") { }

        public override bool OnSkillInvoke(TrustedAI ai, Player player, object data)
        {
            if (ai.GetKnownCardsNums(Slash.ClassName, "he", player) == 0 && player.Hp > 2)
                return true;

            return false;
        }
    }

    public class FuliAI : SkillEvent
    {
        public FuliAI() : base("fuli") { }

        public override bool OnSkillInvoke(TrustedAI ai, Player player, object data)
        {
            return true;
        }
    }

    public class DuliangAI : SkillEvent
    {
        public DuliangAI() : base("duliang") { }

        public override List<WrappedCard> GetTurnUse(TrustedAI ai, Player player)
        {
            if (!player.HasUsed(DuliangCard.ClassName))
                return new List<WrappedCard> { new WrappedCard(DuliangCard.ClassName) { Skill = Name } };

            return new List<WrappedCard>();
        }

        public override string OnChoice(TrustedAI ai, Player player, string choice, object data)
        {
            if (data is Player target)
            {
                if (ai.IsFriend(target) && ai.Room.GetNextAlive(player) != target)
                    return "view";
            }

            return "more";
        }
    }

    public class DuliangCardAI : UseCard
    {
        public DuliangCardAI() : base(DuliangCard.ClassName) { }

        public override void Use(TrustedAI ai, Player player, ref CardUseStruct use, WrappedCard card)
        {
            Room room = ai.Room;
            List<Player> enemies = ai.GetEnemies(player);
            if (enemies.Count > 0)
            {
                ai.SortByDefense(ref enemies, false);
                foreach (Player p in enemies)
                {
                    if (!p.IsKongcheng() && RoomLogic.CanGetCard(room, player, p, "h"))
                    {
                        use.Card = card;
                        use.To.Add(p);
                        return;
                    }
                }
            }

            Player target = room.GetNextAlive(player);
            if (target != null && target != player && ai.IsFriend(target) && !target.IsKongcheng() && RoomLogic.CanGetCard(room, player, target, "h"))
            {
                use.Card = card;
                use.To.Add(target);
            }
        }

        public override double UsePriorityAdjust(TrustedAI ai, Player player, List<Player> targets, WrappedCard card)
        {
            return 3.8;
        }
    }

    public class FuhunAI : SkillEvent
    {
        public FuhunAI() : base("fuhun")
        {
        }

        public override List<WrappedCard> GetTurnUse(TrustedAI ai, Player player)
        {
            List<int> ids = player.GetCards("h");
            ids.AddRange(player.GetHandPile());
            if (ids.Count >= 2)
            {
                ai.SortByUseValue(ref ids, false);
                double value = ai.GetUseValue(ids[0], player);
                value += ai.GetUseValue(ids[1], player);
                if (value < Engine.GetCardUseValue(Slash.ClassName))
                {
                    WrappedCard slash = new WrappedCard(Slash.ClassName)
                    {
                        Skill = Name,
                        ShowSkill = Name
                    };
                    slash.AddSubCard(ids[0]);
                    slash.AddSubCard(ids[1]);
                    slash = RoomLogic.ParseUseCard(ai.Room, slash);
                    return new List<WrappedCard> { slash };
                }
            }

            return null;
        }

        public override double UseValueAdjust(TrustedAI ai, Player player, List<Player> targets, WrappedCard card)
        {
            if (targets.Count > 0) return 0;

            Room room = ai.Room;

            if (player.Phase == PlayerPhase.Play && room.GetRoomState().GetCurrentCardUseReason() == CardUseStruct.CardUseReason.CARD_USE_REASON_PLAY)
            {
                return 2;
            }
            else
            {
                double value = 0;
                foreach (int id in card.SubCards)
                {
                    double _value = 0;
                    List<WrappedCard> cards = ai.GetViewAsCards(player, id);
                    foreach (WrappedCard c in cards)
                    {
                        double card_value = ai.GetUseValue(c, player, room.GetCardPlace(id));
                        if (card_value > _value)
                            _value = card_value;
                    }
                    value += _value;
                }

                return Math.Min(0, 4 - value);
            }
        }

        public override List<WrappedCard> GetViewAsCards(TrustedAI ai, string pattern, Player player)
        {
            if (pattern == Slash.ClassName)
            {
                List<int> ids = player.GetCards("h");
                ids.AddRange(player.GetHandPile());
                CardUseStruct.CardUseReason reason = ai.Room.GetRoomState().GetCurrentCardUseReason();
                if (reason != CardUseStruct.CardUseReason.CARD_USE_REASON_PLAY && reason != CardUseStruct.CardUseReason.CARD_USE_REASON_RESPONSE_USE)
                {
                    foreach (int id in ids)
                        if (ai.Room.GetCard(id).Name == Slash.ClassName)
                            return new List<WrappedCard>();
                }

                if (ids.Count >= 2)
                {
                    if (ai.Self == player)
                    {
                        bool will_use = false;
                        if (reason == CardUseStruct.CardUseReason.CARD_USE_REASON_PLAY)
                        {
                            ai.SortByUseValue(ref ids, false);
                            double value = ai.GetUseValue(ids[0], player);
                            value += ai.GetUseValue(ids[1], player);
                            if (value < Engine.GetCardUseValue(Slash.ClassName))
                                will_use = true;
                        }
                        else
                        {
                            ai.SortByKeepValue(ref ids, false);
                            double value = ai.GetKeepValue(ids[0], player);
                            value += ai.GetKeepValue(ids[1], player);
                            if (value < Engine.GetCardKeepValue(Slash.ClassName))
                                will_use = true;
                        }

                        if (will_use)
                        {
                            WrappedCard slash = new WrappedCard(Slash.ClassName)
                            {
                                Skill = Name,
                                ShowSkill = Name
                            };
                            slash.AddSubCard(ids[0]);
                            slash.AddSubCard(ids[1]);
                            slash = RoomLogic.ParseUseCard(ai.Room, slash);
                            return new List<WrappedCard> { slash };
                        }
                    }
                    else
                    {
                        WrappedCard slash = new WrappedCard(Slash.ClassName)
                        {
                            Skill = Name,
                            ShowSkill = Name
                        };
                        slash.AddSubCard(ids[0]);
                        slash.AddSubCard(ids[1]);
                        slash = RoomLogic.ParseUseCard(ai.Room, slash);
                        return new List<WrappedCard> { slash };
                    }
                }
            }

            return new List<WrappedCard>();
        }
    }

    public class WuyanAI : SkillEvent
    {
        public WuyanAI() : base("wuyan") { }

        public override double CardValue(TrustedAI ai, Player player, WrappedCard card, bool isUse, Place place)
        {
            if (ai.HasSkill(Name, player) && (card.Name == Duel.ClassName || card.Name == FireAttack.ClassName || card.Name == SavageAssault.ClassName || card.Name == ArcheryAttack.ClassName))
                return -2;

            return 0;
        }
    }

    public class JujianAI : SkillEvent
    {
        public JujianAI() : base("jujian")
        {
        }

        public override CardUseStruct OnResponding(TrustedAI ai, Player player, string pattern, string prompt, object data)
        {
            CardUseStruct use = new CardUseStruct(null, player, new List<Player>());
            List<int> ids = new List<int>();
            Room room = ai.Room;
            foreach (int id in player.GetCards("he"))
            {
                WrappedCard card = room.GetCard(id);
                if (!(Engine.GetFunctionCard(card.Name) is BasicCard) && RoomLogic.CanDiscard(room, player, player, id))
                    ids.Add(id);
            }

            if (ids.Count > 0)
            {
                List<Player> friends = ai.FriendNoSelf;
                if (friends.Count > 0)
                {
                    ai.SortByKeepValue(ref ids, false);
                    ai.SortByDefense(ref friends, false);
                    foreach (Player p in friends)
                    {
                        if (!p.FaceUp)
                        {
                            use.Card = new WrappedCard(JujianCard.ClassName) { Skill = Name };
                            use.Card.AddSubCard(ids[0]);
                            use.To.Add(p);
                            return use;
                        }
                    }

                    use.Card = new WrappedCard(JujianCard.ClassName) { Skill = Name };
                    use.Card.AddSubCard(ids[0]);
                    use.To.Add(friends[0]);
                    return use;
                }
            }

            return use;
        }

        public override string OnChoice(TrustedAI ai, Player player, string choice, object data)
        {
            if (!player.FaceUp)
                return "reset";
            else if (player.IsWounded() && ai.IsWeak(player))
                return "recover";

            return "draw";
        }
    }

    public class BenxiAI : SkillEvent
    {
        public BenxiAI() : base("benxi") { }

        public override string OnChoice(TrustedAI ai, Player player, string choice, object data)
        {
            if (data is CardUseStruct use)
            {
                Player to = use.To[0];
                if (use.Card.Name.Contains(Slash.ClassName))
                {
                    if (ai.IsFriend(to) && choice.Contains("draw")) return "draw";
                    if (choice.Contains("armor"))
                    {
                        bool armor_ignore = false;
                        if (to.ArmorIsNullifiedBy(player) || player.HasWeapon(QinggangSword.ClassName) || ai.HasSkill("jianchu"))
                            armor_ignore = true;
                        if (!armor_ignore && ((ai.HasArmorEffect(to, Vine.ClassName) && use.Card.Name == Slash.ClassName && !player.HasWeapon(Fan.ClassName))
                            || (ai.HasArmorEffect(to, RenwangShield.ClassName) && WrappedCard.IsBlack(use.Card.Suit))
                            || (ai.HasArmorEffect(to, SilverLion.ClassName) && (use.Drank > 0 || use.ExDamage > 0))))
                            return "armor";
                    }
                    if (choice.Contains("more"))
                    {
                        foreach (Player p in ai.GetEnemies(player))
                            if (!use.To.Contains(p) && ai.SlashIsEffective(use.Card, p).Score > 0)
                                return "more";
                    }
                    if (choice.Contains("nullified") && ((!ai.IsLackCard(to, Jink.ClassName) && !to.IsKongcheng()) || ai.HasArmorEffect(to, EightDiagram.ClassName)))
                    {
                        return "nullified";
                    }
                    if (choice.Contains("draw")) return "draw";
                }
                else if (use.Card.Name == Dismantlement.ClassName || use.Card.Name == Snatch.ClassName)
                {
                    if (choice.Contains("more")) return "more";
                    if (choice.Contains("nullified")) return "nullified";
                }
                else if (use.Card.Name == Duel.ClassName)
                {
                    if (ai.IsFriend(to) && choice.Contains("draw")) return "draw";
                    if (choice.Contains("more"))
                    {
                        foreach (Player p in ai.GetEnemies(player))
                            if (!use.To.Contains(p) && ai.GetDamageScore(new DamageStruct(use.Card, player, p)).Score > 0)
                                return "more";
                    }
                    if (choice.Contains("nullified") && !ai.IsLackCard(to, Slash.ClassName) && !to.IsKongcheng()) return "nullified";

                    if (choice.Contains("draw")) return "draw";
                }
                else if (use.Card.Name == FireAttack.ClassName)
                {
                    if (ai.IsFriend(to) && choice.Contains("draw")) return "draw";
                    if (choice.Contains("more")) return "more";
                    if (choice.Contains("draw")) return "draw";
                }
                else if (use.Card.Name == SavageAssault.ClassName || use.Card.Name == ArcheryAttack.ClassName)
                {
                    if (ai.HasArmorEffect(use.To[0], Vine.ClassName) && choice.Contains("armor"))
                        return "armor";
                    if (choice.Contains("nullified")) return "nullified";
                    if (choice.Contains("draw")) return "draw";
                }
                else if (use.Card.Name == IronChain.ClassName)
                {
                    if (choice.Contains("more")) return "more";
                    if (choice.Contains("nullified")) return "nullified";
                }
            }

            return "cancel";
        }

        public override List<Player> OnPlayerChosen(TrustedAI ai, Player player, List<Player> targets, int min, int max)
        {
            Room room = ai.Room;
            if (room.GetTag("extra_target_skill") is CardUseStruct use)
            {
                ai.SortByDefense(ref targets, false);
                List<Player> result = new List<Player>();
                if (use.Card.Name == ExNihilo.ClassName)
                {
                    foreach (Player p in targets)
                        if (ai.IsFriend(p))
                            return new List<Player> { p };
                }
                else if (use.Card.Name.Contains(Slash.ClassName))
                {
                    foreach (Player p in targets)
                    {
                        List<ScoreStruct> scores = ai.CaculateSlashIncome(player, new List<WrappedCard> { use.Card }, new List<Player> { p }, false);
                        if (scores.Count > 0 && scores[0].Score > 2)
                            return new List<Player> { p };
                    }
                }
                else if (use.Card.Name == Snatch.ClassName)
                {
                    foreach (Player p in targets)
                    {
                        if (ai.FindCards2Discard(player, p, use.Card.Name, "hej", HandlingMethod.MethodGet).Score > 0)
                            return new List<Player> { p };
                    }
                }
                else if (use.Card.Name == Dismantlement.ClassName)
                {
                    foreach (Player p in targets)
                    {
                        if (ai.FindCards2Discard(player, p, use.Card.Name, "hej", HandlingMethod.MethodDiscard).Score > 0)
                            return new List<Player> { p };
                    }
                }
                else if (use.Card.Name == Duel.ClassName)
                {
                    foreach (Player p in targets)
                    {
                        if (!ai.IsEnemy(p)) continue;
                        DamageStruct damage = new DamageStruct(use.Card, player, p);
                        if (ai.GetDamageScore(damage).Score > 0)
                            return new List<Player> { p };
                    }
                }
                else if (use.Card.Name == IronChain.ClassName)
                {
                    foreach (Player p in targets)
                        if (ai.IsFriend(p) && p.Chained)
                            return new List<Player> { p };

                    foreach (Player p in targets)
                        if (!ai.IsFriend(p) && !p.Chained)
                            return new List<Player> { p };
                }
                else if (use.Card.Name == FireAttack.ClassName)
                {
                    if (!ai.IsFriend(use.To[0]) || !ai.HasSkill("qianxun_jx", use.To[0]))
                    {
                        foreach (Player p in targets)
                        {
                            if (!ai.IsFriend(p))
                            {
                                DamageStruct damage = new DamageStruct(use.Card, player, p, 1, DamageStruct.DamageNature.Fire);
                                if (ai.GetDamageScore(damage).Score > 0)
                                    return new List<Player> { p };
                            }
                        }
                    }
                }
            }

            return new List<Player>();
        }
    }

    public class JujianCardAI : UseCard
    {
        public JujianCardAI() : base(JujianCard.ClassName)
        {
        }
        public override void OnEvent(TrustedAI ai, TriggerEvent triggerEvent, Player player, object data)
        {
            if (triggerEvent == TriggerEvent.CardTargetAnnounced && player != ai.Self && data is CardUseStruct use)
            {
                Player target = use.To[0];

                if (ai.GetPlayerTendency(target) != "unknown")
                    ai.UpdatePlayerRelation(player, target, true);
            }
        }
    }

    public class ZhanjueAI : SkillEvent
    {
        public ZhanjueAI() : base("zhanjue") { }

        public override List<WrappedCard> GetTurnUse(TrustedAI ai, Player player)
        {
            return base.GetTurnUse(ai, player);
        }
    }

    public class QinwangAI : SkillEvent
    {
        public QinwangAI() : base("qinwang")
        {
            key = new List<string> { "cardResponded%qinwang" };
        }

        public override void OnEvent(TrustedAI ai, TriggerEvent triggerEvent, Player player, object data)
        {
            if (triggerEvent == TriggerEvent.ChoiceMade && data is string choice)
            {
                Room room = ai.Room;
                string[] choices = choice.Split('%');
                if (choices[1] == Name && ai.GetPlayerTendency(player) == "unknown" && choices[4] != "_nil_")
                {
                    string prompt = choices[3];
                    Player cc = room.FindPlayer(choices[3].Split(':')[1]);
                    ai.UpdatePlayerRelation(player, cc, true);
                }
            }
        }

        public override double UsePriorityAdjust(TrustedAI ai, Player player, CardUseStruct use)
        {
            List<Player> friends = ai.FriendNoSelf;
            return friends.Count == 0 ? 1 : 0;
        }

        public override List<WrappedCard> GetViewAsCards(TrustedAI ai, string pattern, Player player)
        {
            List<WrappedCard> result = new List<WrappedCard>();

            return result;        // to do



            Room room = ai.Room;
            if (pattern != Slash.ClassName) return result;

            CardUseStruct.CardUseReason reason = room.GetRoomState().GetCurrentCardUseReason();
            if (reason == CardUseStruct.CardUseReason.CARD_USE_REASON_PLAY)
            {
                if (!Slash.IsAvailable(room, player) || player.HasFlag(string.Format("jijiang_activate_{0}", room.GetRoomState().GlobalActivateID)))
                    return result;
            }
            else
            {
                if (player.HasFlag(string.Format("jijiang_{0}", room.GetRoomState().GetCurrentResponseID())))
                    return result;
            }

            bool check = false;
            List<Player> friends = ai.FriendNoSelf;
            if (friends.Count > 0)
            {
                int count = 0;
                foreach (int id in player.GetCards("h"))
                    if (room.GetCard(id).Name.Contains(Slash.ClassName)) count++;
                foreach (int id in player.GetPile("wooden_ox"))
                    if (room.GetCard(id).Name.Contains(Slash.ClassName)) count++;

                foreach (Player p in friends)
                {
                    if (p.Kingdom == "shu" && ((ai.HasSkill("yajiao", p) && room.Current != p) || count == 0 || !ai.IsWeak(p)))
                    {
                        check = true;
                        break;
                    }
                }
            }
            else if (ai.FriendNoSelf.Count == 0)
            {
                foreach (Player p in room.GetOtherPlayers(player))
                {
                    if (p.Kingdom == "shu")
                    {
                        check = true;
                        break;
                    }
                }
            }

            if (check)
            {
                WrappedCard jj = new WrappedCard(QinwangCard.ClassName) { Skill = Name, Mute = true };
                WrappedCard slash = new WrappedCard(Slash.ClassName)
                {
                    UserString = RoomLogic.CardToString(room, jj)
                };
                result.Add(slash);
                if (reason == CardUseStruct.CardUseReason.CARD_USE_REASON_PLAY || reason == CardUseStruct.CardUseReason.CARD_USE_REASON_RESPONSE_USE)
                {
                    WrappedCard f_slash = new WrappedCard(FireSlash.ClassName)
                    {
                        UserString = RoomLogic.CardToString(room, jj)
                    };
                    result.Add(f_slash);

                    WrappedCard t_slash = new WrappedCard(ThunderSlash.ClassName)
                    {
                        UserString = RoomLogic.CardToString(room, jj)
                    };
                    result.Add(t_slash);
                }
            }

            return result;
        }

        public override List<WrappedCard> GetTurnUse(TrustedAI ai, Player player)
        {
            List<WrappedCard> result = new List<WrappedCard>();

            return result;                    //to do

            Room room = ai.Room;
            if (!Slash.IsAvailable(room, player) || player.HasFlag(string.Format("jijiang_activate_{0}", room.GetRoomState().GlobalActivateID)))
                return result;

            bool check = false;

            List<Player> friends = ai.FriendNoSelf;
            if (friends.Count > 0)
            {
                int count = 0;
                foreach (int id in player.GetCards("h"))
                    if (room.GetCard(id).Name.Contains(Slash.ClassName)) count++;
                foreach (int id in player.GetPile("wooden_ox"))
                    if (room.GetCard(id).Name.Contains(Slash.ClassName)) count++;

                foreach (Player p in friends)
                {
                    if (p.Kingdom == "shu" && ((ai.HasSkill("yajiao", p) && room.Current != p) || count == 0 || !ai.IsWeak(p)))
                    {
                        check = true;
                        break;
                    }
                }
            }
            else if (ai.FriendNoSelf.Count == 0)
            {
                foreach (Player p in room.GetOtherPlayers(player))
                {
                    if (p.Kingdom == "shu")
                    {
                        check = true;
                        break;
                    }
                }
            }

            if (check)
            {
                WrappedCard jj = new WrappedCard(JijiangCard.ClassName) { Skill = Name, Mute = true };
                WrappedCard slash = new WrappedCard(Slash.ClassName)
                {
                    UserString = RoomLogic.CardToString(room, jj)
                };
                result.Add(slash);
            }

            return result;
        }

        public override CardUseStruct OnResponding(TrustedAI ai, Player player, string pattern, string prompt, object data)
        {
            Room room = ai.Room;
            CardUseStruct use = new CardUseStruct(null, player, new List<Player>());
            if (prompt.StartsWith("@qinwang-target"))
            {
                Player liubei = room.FindPlayer(prompt.Split(':')[1], true);
                if (ai.IsFriend(liubei))
                {
                    List<Player> targets = new List<Player>();
                    foreach (string str in prompt.Split(':')[2].Split('+'))
                    {
                        Player target = room.FindPlayer(str);
                        if (target != null)
                            targets.Add(target);
                    }

                    if (targets.Count > 0)
                    {
                        List<ScoreStruct> scores = new List<ScoreStruct>();
                        foreach (WrappedCard slash in ai.GetCards(Slash.ClassName, player))
                        {
                            if (RoomLogic.IsCardLimited(room, player, slash, FunctionCard.HandlingMethod.MethodResponse)) continue;
                            foreach (Player enemy in targets)
                            {
                                if (ai.IsEnemy(enemy) && RoomLogic.IsProhibited(room, liubei, enemy, slash) == null
                                    && !ai.IsCancelTarget(slash, enemy, liubei) && ai.IsCardEffect(slash, enemy, liubei))
                                {
                                    ScoreStruct score = new ScoreStruct
                                    {
                                        Card = slash,
                                    };

                                    DamageStruct damage = new DamageStruct(slash, liubei, enemy);
                                    if (ai.HasArmorEffect(enemy, Vine.ClassName) && slash.Name == Slash.ClassName && liubei.HasWeapon(Fan.ClassName))
                                    {
                                        WrappedCard fan = new WrappedCard(FireSlash.ClassName);
                                        fan.AddSubCard(slash);
                                        fan = RoomLogic.ParseUseCard(room, fan);
                                        damage.Card = fan;
                                    }

                                    if (damage.Card.Name == FireSlash.ClassName)
                                        damage.Nature = DamageStruct.DamageNature.Fire;
                                    else if (damage.Card.Name == ThunderSlash.ClassName)
                                        damage.Nature = DamageStruct.DamageNature.Thunder;

                                    ScoreStruct damage_score = ai.GetDamageScore(damage);
                                    if (damage_score.Score > 0)
                                    {
                                        ScoreStruct effect = ai.SlashIsEffective(damage.Card, liubei, enemy);
                                        if (effect.Score > 0)
                                        {
                                            score.Score = effect.Score;
                                            if (effect.Rate > 0)
                                            {
                                                score.Score += Math.Min(1, effect.Rate) * damage_score.Score;
                                                scores.Add(score);
                                            }
                                        }
                                    }
                                }
                            }
                        }

                        if (scores.Count > 0)
                        {
                            scores.Sort((x, y) => { return x.Score > y.Score ? -1 : 1; });
                            double adjust = ai.HasSkill("yajiao") && room.Current != player ? 3 : 1;
                            if (scores[0].Score + adjust > 0)
                            {
                                use.Card = scores[0].Card;
                                return use;
                            }
                        }
                    }
                }
            }
            else
            {
                Player liubei = room.FindPlayer(prompt.Split(':')[1], true);
                if (ai.IsFriend(liubei))
                {
                    object reason = room.GetTag("current_Slash");
                    DamageStruct damage = new DamageStruct();
                    if (reason is CardEffectStruct effect)
                    {
                        damage.From = effect.From;
                        damage.To = effect.To;
                        damage.Card = effect.Card;
                        damage.Damage = 1 + effect.ExDamage;
                        damage.Nature = DamageStruct.DamageNature.Normal;
                    }

                    List<WrappedCard> slashs = ai.GetCards(Slash.ClassName, player);
                    ScoreStruct score = ai.GetDamageScore(damage);
                    if (score.Score < 0 && slashs.Count > 0)
                        use.Card = slashs[0];
                }
            }

            return use;
        }
    }

    public class XiansiAI : SkillEvent
    {
        public XiansiAI() : base("xiansi")
        {
            key = new List<string> { "cardChosen:xiansi" };
        }

        public override void OnEvent(TrustedAI ai, TriggerEvent triggerEvent, Player player, object data)
        {
            if (triggerEvent == TriggerEvent.ChoiceMade && data is string str && player != ai.Self)
            {
                Room room = ai.Room;
                List<string> strs = new List<string>(str.Split(':'));
                if (strs[1] == Name)
                {
                    int id = int.Parse(strs[2]);
                    Player target = room.FindPlayer(strs[3]);
                    if (ai.GetPlayerTendency(target) != "unknown")
                        ai.UpdatePlayerRelation(player, target, ai.GetKeepValue(id, target) > 0);
                }
            }
        }

        public override double CardValue(TrustedAI ai, Player player, WrappedCard card, bool isUse, Place place)
        {
            if (ai.HasSkill(Name, player) && card != null && card.Name == Vine.ClassName)
                return 3;

            return 0;
        }

        public override List<Player> OnPlayerChosen(TrustedAI ai, Player player, List<Player> targets, int min, int max)
        {
            List<ScoreStruct> scores = new List<ScoreStruct>();
            foreach (Player p in targets)
            {
                ScoreStruct score = ai.FindCards2Discard(player, p, Name, "he", HandlingMethod.MethodNone);
                score.Players = new List<Player> { p };
                scores.Add(score);
            }

            if (scores.Count > 1)
                scores.Sort((x, y) => { return x.Score > y.Score ? -1 : 1; });

            List<Player> players = new List<Player>();
            for (int i = 0; i < Math.Min(2, scores.Count); i++)
            {
                if (scores[i].Score > 0)
                    players.AddRange(scores[i].Players);
                else
                    break;
            }

            return players;
        }
    }

    public class XiansiVSAI : SkillEvent
    {
        public XiansiVSAI() : base("xiansivs") { }

        public override List<WrappedCard> GetTurnUse(TrustedAI ai, Player player)
        {
            Room room = ai.Room;
            List<WrappedCard> result = new List<WrappedCard>();
            Player lf = RoomLogic.FindPlayerBySkillName(room, "xiansi");
            if (lf != null)
            {
                List<int> ids = lf.GetPile("revolt");
                if (ids.Count > 1)
                {
                    WrappedCard xs = new WrappedCard(XiansiCard.ClassName);
                    xs.AddSubCard(ids[0]);
                    xs.AddSubCard(ids[1]);
                    WrappedCard slash = new WrappedCard(Slash.ClassName) { Skill = "_xiansi" };
                    slash.UserString = RoomLogic.CardToString(room, xs);
                    result.Add(slash);
                }
            }

            return result;
        }

        public override List<WrappedCard> GetViewAsCards(TrustedAI ai, string pattern, Player player)
        {
            Room room = ai.Room;
            List<WrappedCard> result = new List<WrappedCard>();
            if (pattern == Slash.ClassName && (room.GetRoomState().GetCurrentCardUseReason() == CardUseStruct.CardUseReason.CARD_USE_REASON_RESPONSE_USE
                    || room.GetRoomState().GetCurrentCardUseReason() == CardUseStruct.CardUseReason.CARD_USE_REASON_PLAY))
            {
                Player lf = RoomLogic.FindPlayerBySkillName(room, "xiansi");
                if (lf != null)
                {
                    List<int> ids = lf.GetPile("revolt");
                    if (ids.Count > 1)
                    {
                        WrappedCard xs = new WrappedCard(XiansiCard.ClassName);
                        xs.AddSubCard(ids[0]);
                        xs.AddSubCard(ids[1]);
                        WrappedCard slash = new WrappedCard(Slash.ClassName) { Skill = "_xiansi" };
                        slash.UserString = RoomLogic.CardToString(room, xs);
                        result.Add(slash);
                    }
                }
            }

            return result;
        }
    }

    public class LongyinAI : SkillEvent
    {
        public LongyinAI() : base("longyin")
        {
        }

        public override List<int> OnDiscard(TrustedAI ai, Player player, List<int> ids, int min, int max, bool option)
        {
            Room room = ai.Room;
            if (room.GetTag("longyin_data") is CardUseStruct use && ai.IsFriend(use.From) && use.From.Alive)
            {
                if (ids.Count > 0)
                {
                    bool red = WrappedCard.IsRed(use.Card.Suit);
                    List<double> values = ai.SortByKeepValue(ref ids, false);
                    if (values[0] < 0 || (values[0] < 4 && red))
                        return new List<int> { ids[0] };

                    if (ai.IsEnemy(use.To[0]))
                    {
                        if (use.From != player && use.From.HandcardNum + use.From.GetPile("wooden_ox").Count > 4 && values[0] < 3)
                            return new List<int> { ids[0] };

                        if (use.From == player)
                        {
                            int count = ai.GetKnownCardsNums(Slash.ClassName, "he", player);
                            if (count > 0)
                            {
                                values = ai.SortByKeepValue(ref ids, false);
                                if (values[0] < 4 && (!room.GetCard(ids[0]).Name.Contains(Slash.ClassName) || count > 1))
                                {
                                    return new List<int> { ids[0] };
                                }
                            }
                        }
                    }
                }
            }

            return new List<int>();
        }
    }

    public class WurongAI : SkillEvent
    {
        public WurongAI() : base("wurong") {}

        public override List<WrappedCard> GetTurnUse(TrustedAI ai, Player player)
        {
            if (!player.HasUsed(WurongCard.ClassName) && !player.IsKongcheng())
            {
                return new List<WrappedCard> { new WrappedCard(WurongCard.ClassName) { Skill = Name } };
            }

            return new List<WrappedCard>();
        }

        public override WrappedCard OnPindian(TrustedAI ai, Player requestor, List<Player> players)
        {
            Room room = ai.Room;
            List<int> ids = ai.Self.GetCards("h");
            if (ai.IsFriend(requestor))
            {
                foreach (int id in ids)
                {
                    WrappedCard card = room.GetCard(id);
                    if (card.Name != Jink.ClassName)
                        return card;
                }
            }
            else
            {
                foreach (int id in ids)
                {
                    WrappedCard card = room.GetCard(id);
                    if (card.Name == Jink.ClassName)
                        return card;
                }
            }

            return room.GetCard(ids[0]);
        }
    }

    public class WurongCardAI : UseCard
    {
        public WurongCardAI() : base(WurongCard.ClassName) { }
        public override void OnEvent(TrustedAI ai, TriggerEvent triggerEvent, Player player, object data)
        {
            if (triggerEvent == TriggerEvent.CardTargetAnnounced && data is CardUseStruct use && ai.Self != player)
            {
                Player target = use.To[0];
                if (ai.GetPlayerTendency(target) != "unknown")
                    ai.UpdatePlayerRelation(player, target, false);
            }
        }
        public override void Use(TrustedAI ai, Player player, ref CardUseStruct use, WrappedCard card)
        {
            List<Player> enemies = ai.GetEnemies(player);
            if (enemies.Count > 0)
            {
                Room room = ai.Room;
                ai.SortByDefense(ref enemies, false);
                List<Player> no_jink = new List<Player>(), has_jink = new List<Player>();

                int slash = -1, not_slash = -1;
                List<int> ids = player.GetCards("h");
                ai.SortByUseValue(ref ids, false);
                foreach (int id in ids)
                {
                    WrappedCard wrapped = room.GetCard(id);
                    if (RoomLogic.CanDiscard(room, player, player, id) && wrapped.Name.Contains(Slash.ClassName))
                    {
                        slash = id;
                        break;
                    }
                }

                foreach (int id in ids)
                {
                    WrappedCard wrapped = room.GetCard(id);
                    if (RoomLogic.CanDiscard(room, player, player, id))
                    {
                        not_slash = id;
                        break;
                    }
                }

                List<ScoreStruct> scores = new List<ScoreStruct>();
                foreach (Player p in enemies)
                {
                    if (!p.IsKongcheng())
                    {
                        bool lack = ai.IsLackCard(p, Jink.ClassName);
                        if (!lack)
                        {
                            lack = true;
                            foreach (int id in ai.GetKnownCards(p))
                            {
                                if (room.GetCard(id).Name == Jink.ClassName)
                                {
                                    lack = false;
                                    break;
                                }
                            }
                        }

                        if (lack && slash > -1)
                        {
                            DamageStruct damage = new DamageStruct("wurong", player, p);
                            ScoreStruct score = ai.GetDamageScore(damage);
                            score.Players = new List<Player> { p };
                            scores.Add(score);

                            no_jink.Add(p);
                        }
                        else if (RoomLogic.CanGetCard(room, player, p, "he"))
                        {
                            ScoreStruct score = ai.FindCards2Discard(player, p, "wurong", "he", HandlingMethod.MethodGet);
                            score.Players = new List<Player> { p };
                            scores.Add(score);
                            has_jink.Add(p);
                        }
                    }
                }

                if (scores.Count > 0)
                {
                    scores.Sort((x, y) => { return x.Score > y.Score ? -1 : 1; });
                    foreach (ScoreStruct score in scores)
                    {
                        if (score.Score > 2)
                        {
                            if (no_jink.Contains(score.Players[0]) && slash > -1)
                            {
                                card.AddSubCard(slash);
                                use.Card = card;
                                use.To = score.Players;
                                return;
                            }

                            if (has_jink.Contains(score.Players[0]) && not_slash > -1)
                            {
                                card.AddSubCard(not_slash);
                                use.Card = card;
                                use.To = score.Players;
                                return;
                            }
                        }
                    }
                }
            }
        }

        public override double UsePriorityAdjust(TrustedAI ai, Player player, List<Player> targets, WrappedCard card)
        {
            return 3.3;
        }
    }

    public class ZhongyongAI : SkillEvent
    {
        public ZhongyongAI() : base("zhongyong")
        {
            key = new List<string> { "playerChosen:zhongyong" };
        }
        public override void OnEvent(TrustedAI ai, TriggerEvent triggerEvent, Player player, object data)
        {
            if (triggerEvent == TriggerEvent.ChoiceMade && data is string str && player != ai.Self)
            {
                string[] strs = str.Split(':');
                if (strs[1] == Name)
                {
                    Room room = ai.Room;
                    Player target = room.FindPlayer(strs[2]);
                    if (ai.GetPlayerTendency(target) != "unknown")
                        ai.UpdatePlayerRelation(player, target, true);
                }
            }
        }

        public override List<Player> OnPlayerChosen(TrustedAI ai, Player player, List<Player> targets, int min, int max)
        {
            Room room = ai.Room;
            if (room.GetTag(Name) is string card_str)
            {
                string str = string.Format("{0}_{1}", Name, card_str);
                WrappedCard card = RoomLogic.ParseCard(room, card_str);
                bool red = false;
                List<int> slash = new List<int>(), jink = new List<int>();
                foreach (int id in card.SubCards)
                    if (room.GetCardPlace(id) == Place.DiscardPile) slash.Add(id);
                if (player.ContainsTag(str) && player.GetTag(str) is List<int> jinks)
                    foreach (int id in jinks)
                        if (room.GetCardPlace(id) == Place.DiscardPile) jink.Add(id);

                foreach (int id in slash)
                {
                    if (WrappedCard.IsRed(room.GetCard(id).Suit))
                    {
                        red = true;
                        break;
                    }
                }

                foreach (int id in jink)
                {
                    if (WrappedCard.IsRed(room.GetCard(id).Suit))
                    {
                        red = true;
                        break;
                    }
                }

                foreach (Player p in targets)
                {
                    if (ai.IsFriend(p) && ai.HasSkill("zishu", p) && p == room.Current)
                    {
                        return new List<Player> { p };
                    }
                }

                if (red)
                {
                    foreach (Player p in targets)
                    {
                        if (ai.IsFriend(p) && ai.HasSkill("liegong_jx|tieqi_jx|wushuang|jianchu|moukui|fuqi") || (ai.HasSkill("jiedao", p) && p.IsWounded()))
                        {
                            return new List<Player> { p };
                        }
                    }

                    ai.SortByHandcards(ref targets);
                    foreach (Player p in targets)
                    {
                        if (ai.IsFriend(p))
                        {
                            return new List<Player> { p };
                        }
                    }
                }

                ai.SortByDefense(ref targets, false);
                foreach (Player p in targets)
                {
                    if (ai.IsFriend(p) && ai.HasSkill("liegong_jx|tieqi_jx|wushuang|jianchu|moukui|fuqi|jiedao|longdan_jx|wusheng_jx"))
                        return new List<Player> { p };
                }

                foreach (Player p in targets)
                {
                    if (ai.IsFriend(p) && !ai.HasSkill("zishu", p))
                    {
                        return new List<Player> { p };
                    }
                }
            }

            return new List<Player>();
        }

        public override string OnChoice(TrustedAI ai, Player player, string choice, object data)
        {
            Room room = ai.Room;
            if (room.GetTag(Name) is string card_str)
            {
                string str = string.Format("{0}_{1}", Name, card_str);
                WrappedCard card = RoomLogic.ParseCard(room, card_str);
                List<int> slash = new List<int>(), jink = new List<int>();
                foreach (int id in card.SubCards)
                    if (room.GetCardPlace(id) == Place.DiscardPile) slash.Add(id);
                if (player.ContainsTag(str) && player.GetTag(str) is List<int> jinks)
                    foreach (int id in jinks)
                        if (room.GetCardPlace(id) == Place.DiscardPile) jink.Add(id);

                bool jink_red = false;
                foreach (int id in jink)
                {
                    if (WrappedCard.IsRed(room.GetCard(id).Suit))
                    {
                        jink_red = true;
                        break;
                    }
                }

                if (jink_red && jink.Count > slash.Count)
                    return "jink";
            }

            return "slash";
        }

        public override CardUseStruct OnResponding(TrustedAI ai, Player player, string pattern, string prompt, object data)
        {
            Room room = ai.Room;
            string[] strs = prompt.Split(':');

            CardUseStruct use = new CardUseStruct(null, player, new List<Player>());
            List<Player> targets = new List<Player>();
            foreach (Player p in room.GetOtherPlayers(player))
                if (p.HasFlag("SlashAssignee")) targets.Add(p);

            List<ScoreStruct> values = ai.CaculateSlashIncome(player, null, targets);
            if (values.Count > 0 && values[0].Score > 0)
            {
                use.Card = values[0].Card;
                use.To = values[0].Players;
            }

            return use;
        }
    }

    public class AnxuCardAI : UseCard
    {
        public AnxuCardAI() : base(AnxuCard.ClassName)
        {
        }

        public override void OnEvent(TrustedAI ai, TriggerEvent triggerEvent, Player player, object data)
        {
            if (triggerEvent == TriggerEvent.CardTargetAnnounced && player != ai.Self && data is CardUseStruct use)
            {
                Player target = use.To[0];
                if (use.To[0].HandcardNum > use.To[1].HandcardNum)
                    target = use.To[1];

                if (ai.GetPlayerTendency(target) != "unknown")
                    ai.UpdatePlayerRelation(player, target, true);
            }
        }

        public override double UsePriorityAdjust(TrustedAI ai, Player player, List<Player> targets, WrappedCard card)
        {
            return 9;
        }
        public override void Use(TrustedAI ai, Player player, ref CardUseStruct use, WrappedCard card)
        {
            Room room = ai.Room;
            List<Player> friends = ai.GetFriends(player);
            List<Player> enemies = ai.GetEnemies(player);
            ai.SortByDefense(ref friends, false);
            ai.SortByDefense(ref enemies, false);

            foreach (Player p in friends)
            {
                if (player == p) continue;
                foreach (Player e in enemies)
                {
                    if (e.HandcardNum > p.HandcardNum)
                    {
                        use.To = new List<Player> { p, e };
                        use.Card = card;
                        return;
                    }
                }
            }

            foreach (Player p in friends)
            {
                if (player == p) continue;
                foreach (Player e in room.GetOtherPlayers(p))
                {
                    if (player == e || ai.IsFriend(e)) continue;
                    if (e.HandcardNum > p.HandcardNum)
                    {
                        use.To = new List<Player> { p, e };
                        use.Card = card;
                        return;
                    }
                }
            }
        }
    }

    public class ZhuiyiAI : SkillEvent
    {
        public ZhuiyiAI() : base("zhuiyi") { }
        public override List<Player> OnPlayerChosen(TrustedAI ai, Player player, List<Player> targets, int min, int max)
        {
            List<Player> friends = ai.FriendNoSelf;
            if (friends.Count > 0)
            {
                ai.SortByDefense(ref friends, false);
                foreach (Player p in friends)
                    if (targets.Contains(p)) return new List<Player> { p };
            }

            return new List<Player>();
        }
    }

    public class ShenxingAI : SkillEvent
    {
        public ShenxingAI() : base("shenxing")
        {
        }

        public override List<WrappedCard> GetTurnUse(TrustedAI ai, Player player)
        {
            List<WrappedCard> result = new List<WrappedCard>();
            List<int> ids = player.GetCards("he");
            if (ids.Count >= 2)
            {
                Room room = ai.Room;
                List<int> red = new List<int>(), black = new List<int>(), adjust = new List<int>(), subs = new List<int>();
                foreach (int id in player.GetCards("h"))
                {
                    if (WrappedCard.IsBlack(room.GetCard(id).Suit))
                        black.Add(id);
                    else
                        red.Add(id);
                }

                if (black.Count > 0 && red.Count > 0)
                {
                    if (black.Count > red.Count)
                        adjust = red;
                    else
                        adjust = black;
                }

                List<double> values = ai.SortByKeepValue(ref ids, false);
                if (values[0] < 0)
                {
                    subs.Add(ids[0]);
                    foreach (int id in ids)
                    {
                        if (subs.Contains(id) || (adjust.Count > 0 && !adjust.Contains(id))) continue;
                        subs.Add(id);
                        if (subs.Count >= 2)
                        {
                            WrappedCard card = new WrappedCard(ShenxingCard.ClassName) { Skill = Name };
                            card.AddSubCards(subs);
                            result.Add(card);
                            return result;
                        }
                    }
                }

                if (adjust.Count > 0 || ai.GetOverflow(player) > 0)
                {
                    ai.SortByKeepValue(ref adjust, false);
                    foreach (int id in adjust)
                    {
                        subs.Add(id);
                        if (subs.Count >= 2)
                        {
                            WrappedCard card = new WrappedCard(ShenxingCard.ClassName) { Skill = Name };
                            card.AddSubCards(subs);
                            result.Add(card);
                            return result;
                        }
                    }

                    foreach (int id in ids)
                    {
                        if (subs.Contains(id)) continue;
                        subs.Add(id);
                        if (subs.Count >= 2)
                        {
                            WrappedCard card = new WrappedCard(ShenxingCard.ClassName) { Skill = Name };
                            card.AddSubCards(subs);
                            result.Add(card);
                            return result;
                        }
                    }
                }
            }

            return result;
        }
    }

    public class ShenxingCardAI : UseCard
    {
        public ShenxingCardAI() : base(ShenxingCard.ClassName)
        { }

        public override void Use(TrustedAI ai, Player player, ref CardUseStruct use, WrappedCard card)
        {
            use.Card = card;
        }

        public override double UsePriorityAdjust(TrustedAI ai, Player player, List<Player> targets, WrappedCard card)
        {
            return 0.5;
        }
    }

    public class BingyiAI : SkillEvent
    {
        public BingyiAI() : base("bingyi") { key = new List<string> { "playerChosen:bingyi" }; }
        public override void OnEvent(TrustedAI ai, TriggerEvent triggerEvent, Player player, object data)
        {
            if (triggerEvent == TriggerEvent.ChoiceMade && data is string str && player != ai.Self)
            {
                string[] strs = str.Split(':');
                if (strs[1] == Name)
                {
                    Room room = ai.Room;
                    List<Player> targets = new List<Player>();
                    foreach (string name in strs[2].Split('+'))
                        targets.Add(room.FindPlayer(name));

                    foreach (Player p in targets)
                        if (p != player && ai.GetPlayerTendency(p) != "unknown")
                            ai.UpdatePlayerRelation(player, p, true);
                }
            }
        }
        public override bool OnSkillInvoke(TrustedAI ai, Player player, object data)
        {
            List<int> red = new List<int>(), black = new List<int>();
            foreach (int id in player.GetCards("h"))
            {
                if (WrappedCard.IsBlack(ai.Room.GetCard(id).Suit))
                    black.Add(id);
                else
                    red.Add(id);
            }

            return black.Count == 0 || red.Count == 0;
        }

        public override List<Player> OnPlayerChosen(TrustedAI ai, Player player, List<Player> targets, int min, int max)
        {
            List<Player> friends = ai.GetFriends(player), result = new List<Player>();
            ai.SortByDefense(ref friends, false);

            for (int i = 0; i < Math.Min(friends.Count, max); i++)
                result.Add(friends[i]);

            return result;
        }
    }

    public class BuyiJXAI : SkillEvent
    {
        public BuyiJXAI() : base("buyi_jx")
        {
            key = new List<string> { "skillInvoke:buyi_jx" };
        }

        public override void OnEvent(TrustedAI ai, TriggerEvent triggerEvent, Player player, object data)
        {
            if (triggerEvent == TriggerEvent.ChoiceMade && data is string str && player != ai.Self)
            {
                List<string> strs = new List<string>(str.Split(':'));
                if (strs[1] == Name && strs[2] == "yes")
                {
                    Player target = null;
                    foreach (Player p in ai.Room.GetAlivePlayers())
                    {
                        if (p.HasFlag("Global_Dying"))
                        {
                            target = p;
                            break;
                        }
                    }
                    if (ai.GetPlayerTendency(target) != "unknown") ai.UpdatePlayerRelation(player, target, true);
                }
            }
        }
        public override bool OnSkillInvoke(TrustedAI ai, Player player, object data)
        {
            if (data is Player target)
            {
                if (ai.IsFriend(target)) return true;

                if (player.GetRoleEnum() == Player.PlayerRole.Renegade && ai.Room.GetAlivePlayers().Count > 2 && target.GetRoleEnum() == Player.PlayerRole.Lord)
                    return true;
            }

            return false;
        }
    }

    public  class AnguoAI : SkillEvent
    {
        public AnguoAI() : base("anguo") { }

        public override List<WrappedCard> GetTurnUse(TrustedAI ai, Player player)
        {
            if (!player.HasUsed(AnguoCard.ClassName))
                return new List<WrappedCard> { new WrappedCard(AnguoCard.ClassName) { Skill = Name } };

            return new List<WrappedCard>();
        }
    }

    public class AnguoCardAI : UseCard
    {
        public AnguoCardAI() : base(AnguoCard.ClassName) { }

        public override void OnEvent(TrustedAI ai, TriggerEvent triggerEvent, Player player, object data)
        {
            if (triggerEvent == TriggerEvent.CardTargetAnnounced && data is CardUseStruct use && player != ai.Self)
            {
                Room room = ai.Room;
                int count = 100, hp = 100, eq = 100;
                foreach (Player p in room.GetAllPlayers())
                {
                    if (p.HandcardNum < count)
                        count = p.HandcardNum;
                    if (p.Hp < hp)
                        hp = p.Hp;
                    if (p.GetEquips().Count < eq)
                        eq = p.GetEquips().Count;
                }

                Player target = use.To[0];
                if (ai.GetPlayerTendency(target) != "unknown" && (target.Hp == hp || (target.HandcardNum == count && target.GetEquips().Count == eq)))
                {
                    ai.UpdatePlayerRelation(player, target, true);
                }
            }
        }

        public override void Use(TrustedAI ai, Player player, ref CardUseStruct use, WrappedCard card)
        {
            Room room = ai.Room;
            int count = 100, hp = 100, eq = 100;
            foreach (Player p in room.GetAllPlayers())
            {
                if (p.HandcardNum < count)
                    count = p.HandcardNum;
                if (p.Hp < hp)
                    hp = p.Hp;
                if (p.GetEquips().Count < eq)
                    eq = p.GetEquips().Count;
            }

            List<Player> friends = ai.FriendNoSelf;
            if (friends.Count > 0)
            {
                ai.SortByDefense(ref friends, false);
                foreach (Player p in friends)
                {
                    int match = 0;
                    if (p.HandcardNum == count) match++;
                    if (p.Hp == hp) match++;
                    if (p.GetEquips().Count == eq) match++;

                    if (match == 3)
                    {
                        use.Card = card;
                        use.To.Add(p);
                        return;
                    }
                }

                foreach (Player p in friends)
                {
                    int match = 0;
                    if (p.HandcardNum == count) match++;
                    if (p.Hp == hp) match++;
                    if (p.GetEquips().Count == eq) match++;

                    if (match == 2)
                    {
                        use.Card = card;
                        use.To.Add(p);
                        return;
                    }
                }

                foreach (Player p in friends)
                {
                    if (p.Hp == hp)
                    {
                        use.Card = card;
                        use.To.Add(p);
                        return;
                    }
                }

                foreach (Player p in friends)
                {
                    int match = 0;
                    if (p.HandcardNum == count) match++;
                    if (p.Hp == hp) match++;
                    if (p.GetEquips().Count == eq) match++;

                    if (match > 0)
                    {
                        use.Card = card;
                        use.To.Add(p);
                        return;
                    }
                }
            }

            int self = 0;
            if (player.Hp == hp) self++;
            if (player.HandcardNum == count) self++;
            if (player.GetEquips().Count == eq) self++;
            if (self > 0)
            {
                foreach (Player p in room.GetAlivePlayers())
                {
                    int match = 0;
                    if (p.HandcardNum == count) match++;
                    if (p.Hp == hp) match++;
                    if (p.GetEquips().Count == eq) match++;

                    if (match == 0)
                    {
                        use.Card = card;
                        use.To.Add(p);
                        return;
                    }
                }

                foreach (Player p in room.GetAlivePlayers())
                {
                    int match = 0;
                    if (p.HandcardNum == count) match++;
                    if (p.Hp == hp) match++;
                    if (p.GetEquips().Count == eq) match++;

                    if (p.HandcardNum == count && match == 1 && (player.Hp == hp || player.GetEquips().Count == eq))
                    {
                        use.Card = card;
                        use.To.Add(p);
                        return;
                    }
                }
            }
        }

        public override double UsePriorityAdjust(TrustedAI ai, Player player, List<Player> targets, WrappedCard card)
        {
            return 7;
        }
    }

    public class WenguaAI : SkillEvent
    {
        public WenguaAI() : base("wengua") { }

        public override List<WrappedCard> GetTurnUse(TrustedAI ai, Player player)
        {
            if (!player.IsNude() && !player.HasUsed(WenguaCard.ClassName))
            {
                List<int> ids = player.GetCards("he");
                List<double> values = ai.SortByKeepValue(ref ids, false);
                ai.Number[Name] = 6;
                if (values[0] < 0)
                {
                    WrappedCard card = new WrappedCard(WenguaCard.ClassName) { Skill = Name };
                    card.AddSubCard(ids[0]);
                    return new List<WrappedCard> { card };
                }

                values = ai.SortByUseValue(ref ids, false);
                if (values[0] < 3.5)
                {
                    WrappedCard card = new WrappedCard(WenguaCard.ClassName) { Skill = Name };
                    card.AddSubCard(ids[0]);
                    return new List<WrappedCard> { card };
                }

                if (ai.GetOverflow(player) > 0)
                {
                    ai.Number[Name] = 0.4;
                    values = ai.SortByKeepValue(ref ids, false);
                    WrappedCard card = new WrappedCard(WenguaCard.ClassName) { Skill = Name };
                    card.AddSubCard(ids[0]);
                    return new List<WrappedCard> { card };
                }
            }

            return new List<WrappedCard>();
        }
        public override string OnChoice(TrustedAI ai, Player player, string choice, object data)
        {
            return "bottom";
        }
    }

    public class WenguaVSAI : SkillEvent
    {
        public WenguaVSAI() : base("wenguavs") { }

        public override List<WrappedCard> GetTurnUse(TrustedAI ai, Player player)
        {
            Room room = ai.Room;
            Player xushi = RoomLogic.FindPlayerBySkillName(room, "wengua");
            if (xushi != null && ai.IsFriend(xushi) && !player.HasUsed(WenguaCard.ClassName) && !player.IsNude())
            {
                List<int> ids = player.GetCards("he");
                List<double> values = ai.SortByKeepValue(ref ids, false);
                ai.Number["wengua"] = 6;
                if (values[0] < 0)
                {
                    WrappedCard card = new WrappedCard(WenguaCard.ClassName) { Skill = "wengua", Mute = true };
                    card.AddSubCard(ids[0]);
                    return new List<WrappedCard> { card };
                }

                values = ai.SortByUseValue(ref ids, false);
                if (values[0] < 4)
                {
                    WrappedCard card = new WrappedCard(WenguaCard.ClassName) { Skill = "wengua", Mute = true };
                    card.AddSubCard(ids[0]);
                    return new List<WrappedCard> { card };
                }

                if (ai.GetOverflow(player) > 0)
                {
                    ai.Number["wengua"] = 0.4;
                    values = ai.SortByKeepValue(ref ids, false);
                    WrappedCard card = new WrappedCard(WenguaCard.ClassName) { Skill = "wengua", Mute = true };
                    card.AddSubCard(ids[0]);
                    return new List<WrappedCard> { card };
                }
            }

            return new List<WrappedCard>();
        }

    }

    public class WenguaCardAI : UseCard
    {
        public WenguaCardAI() : base(WenguaCard.ClassName) { }

        public override void Use(TrustedAI ai, Player player, ref CardUseStruct use, WrappedCard card)
        {
            use.Card = card;
        }

        public override double UsePriorityAdjust(TrustedAI ai, Player player, List<Player> targets, WrappedCard card)
        {
            return ai.Number["wengua"];
        }
    }

    public class FuzhuAI : SkillEvent
    {
        public FuzhuAI() : base("fuzhu")
        {
            key = new List<string> { "skillInvoke:fuzhu" };
        }

        public override void OnEvent(TrustedAI ai, TriggerEvent triggerEvent, Player player, object data)
        {
            if (triggerEvent == TriggerEvent.ChoiceMade && data is string str && player != ai.Self)
            {
                List<string> strs = new List<string>(str.Split(':'));
                if (strs[1] == Name && strs[2] == "yes")
                {
                    Player target = ai.Room.Current;
                    if (ai.GetPlayerTendency(target) != "unknown") ai.UpdatePlayerRelation(player, target, false);
                }
            }
        }

        public override bool OnSkillInvoke(TrustedAI ai, Player player, object data)
        {
            if (data is Player target && ai.IsEnemy(target))
            {
                WrappedCard slash = new WrappedCard(Slash.ClassName);
                if (RoomLogic.IsProhibited(ai.Room, player, target, slash) == null)
                    return true;
            }

            return false;
        }
    }

    public class YaomingAI : SkillEvent
    {
        public YaomingAI() : base("yaoming")
        {
        }

        public override List<Player> OnPlayerChosen(TrustedAI ai, Player player, List<Player> targets, int min, int max)
        {
            foreach (Player p in targets)
            {
                if (ai.IsFriend(p) && p.HandcardNum >= player.HandcardNum)
                {
                    foreach (int id in p.GetEquips())
                    {
                        if (ai.GetKeepValue(id, p) < 0)
                            return new List<Player> { p };
                    }
                }
            }

            List<ScoreStruct> scores = new List<ScoreStruct>();
            foreach (Player p in targets)
            {
                if (ai.IsFriend(p) && p.HandcardNum < player.HandcardNum)
                {
                    ScoreStruct score = new ScoreStruct
                    {
                        Score = 1.5,
                        Players = new List<Player> { p }
                    };
                    if (ai.IsWeak(p))
                    {
                        score.Score += 0.1 * p.GetLostHp();
                        score.Score += (3 - p.HandcardNum) * 0.05;
                    }
                }
                else if (!ai.IsFriend(p) && p.HandcardNum > player.HandcardNum)
                {
                    scores.Add(ai.FindCards2Discard(player, p, Name, "he", HandlingMethod.MethodDiscard));
                }
            }
            if (scores.Count > 0)
            {
                scores.Sort((x, y) => { return x.Score > y.Score ? -1 : 1; });
                if (scores[0].Score > 0)
                    return scores[0].Players;
            }

            if (targets.Contains(player) && !player.IsKongcheng())
            {
                List<int> ids = player.GetCards("h");
                List<double> values = new List<double>();
                if (player.Phase != PlayerPhase.NotActive)
                    values = ai.SortByUseValue(ref ids, false);
                else
                    values = ai.SortByKeepValue(ref ids, false);

                if (values[0] < 3)
                    return new List<Player> { player };
            }

            return new List<Player>();
        }

        public override List<int> OnExchange(TrustedAI ai, Player player, string pattern, int min, int max, string pile)
        {
            List<int> ids = player.GetCards("he"), result = new List<int>();
            List<double> values = ai.SortByUseValue(ref ids, false);
            if (values[0] < 0) result.Add(ids[0]);

            if (player.Phase == PlayerPhase.NotActive)
            {
                for (int i = 0; i < ids.Count; i++)
                {
                    int id = ids[0];
                    if (result.Contains(id)) continue;
                    if (values[i] < 3) result.Add(id);
                    if (result.Count >= 2) break;
                }
                if (result.Count > 0)
                    return result;

            }
            else
            {
                values = ai.SortByUseValue(ref ids, false);
                for (int i = 0; i < ids.Count; i++)
                {
                    int id = ids[0];
                    if (result.Contains(id)) continue;
                    if (values[i] < 3) result.Add(id);
                    if (result.Count >= 2) break;
                }
            }

            return new List<int> { ids[0] };
        }
    }

    public class XuanfengAI : SkillEvent
    {
        public XuanfengAI() : base("xuanfeng") { }

        public override List<Player> OnPlayerChosen(TrustedAI ai, Player player, List<Player> targets, int min, int max)
        {
            List<ScoreStruct> scores = new List<ScoreStruct>();
            List<Player> players = new List<Player>();
            foreach (Player p in targets)
            {
                scores.Add(ai.FindCards2Discard(player, p, Name, "he", HandlingMethod.MethodDiscard));
            }
            if (scores.Count > 0)
            {
                scores.Sort((x, y) => { return x.Score > y.Score ? -1 : 1; });
                for (int i = 1; i < Math.Min(2, scores.Count); i++)
                    players.AddRange(scores[i].Players);

                if (players.Count == 1 && ai.IsFriend(players[0]))
                {
                    if (ai.FindCards2Discard(player, players[0], Name, "he", HandlingMethod.MethodDiscard, 2, true).Score <
                        ai.FindCards2Discard(player, players[0], Name, "he", HandlingMethod.MethodDiscard, 1).Score)
                        return new List<Player>();
                }
            }

            return players;
        }
    }

    public class FenliAI : SkillEvent
    {
        public FenliAI() : base("fenli") { }

        public override bool OnSkillInvoke(TrustedAI ai, Player player, object data)
        {
            if (data is string str)
            {
                string[] strs = str.Split(':');
                string phase = strs[3];
                if (phase == "discard")
                    return true;
                else if (phase == "draw")
                {
                    if (player.HandcardNum > 4 && ai.GetEnemies(player).Count > 1)
                        return true;
                }
                else if (phase == "play")
                {
                    double value = 0;
                    bool slash = false;
                    List<int> ids = player.GetCards("h");
                    ids.AddRange(player.GetHandPile());
                    Room room = ai.Room;
                    foreach (int id in ids)
                    {
                        WrappedCard card = room.GetCard(id);
                        FunctionCard fcard = Engine.GetFunctionCard(card.Name);
                        if (fcard.IsAvailable(room, player, card) && (!(fcard is Slash && slash) || ai.HasCrossbowEffect(player)))
                        {
                            value += ai.GetUseValue(id, player);
                            if (fcard is Slash)
                                slash = true;
                        }
                    }
                    if (value < 8 && ai.GetOverflow(player) == 0 || value < 4) return true;
                }
            }

            return false;
        }

        public override double CardValue(TrustedAI ai, Player player, WrappedCard card, bool isUse, Place place)
        {
            Room room = ai.Room;
            if (ai.HasSkill(Name, player) && !RoomLogic.IsVirtualCard(room, card))
            {
                FunctionCard fcard = Engine.GetFunctionCard(room.GetCard(card.Id).Name);
                if (fcard is EquipCard equip && (!isUse || player.GetEquip((int)equip.EquipLocation()) == -1))
                    return 1.8;
            }

            return 0;
        }
    }

    public class PinkouAI : SkillEvent
    {
        public PinkouAI() : base("pingkou")
        {
            key = new List<string> { "playerChosen:pingkou" };
        }
        public override void OnEvent(TrustedAI ai, TriggerEvent triggerEvent, Player player, object data)
        {
            if (triggerEvent == TriggerEvent.ChoiceMade && data is string str && player != ai.Self && ai is StupidAI _ai)
            {
                List<string> strs = new List<string>(str.Split(':'));
                if (strs[1] == Name)
                {
                    Room room = ai.Room;
                    foreach (string name in strs[2].Split('+'))
                    {
                        Player target = room.FindPlayer(name);
                        DamageStruct damage = new DamageStruct(Name, player, target);
                        if (ai.GetPlayerTendency(target) != "unknown" && !_ai.NeedDamage(damage))
                            ai.UpdatePlayerRelation(player, target, false);
                    }
                }
            }
        }

        public override List<Player> OnPlayerChosen(TrustedAI ai, Player player, List<Player> targets, int min, int max)
        {
            List<Player> result = new List<Player>();
            Dictionary<Player, double> points = new Dictionary<Player, double>();
            foreach (Player p in targets)
            {
                DamageStruct damage = new DamageStruct(Name, player, p);
                ScoreStruct score = ai.GetDamageScore(damage);
                points[p] = score.Score;
            }

            targets.Sort((x, y) => { return points[x] > points[y] ? -1 : 1; });
            for (int i =0;i< Math.Min(max, targets.Count);i++)
            {
                if (points[targets[i]] > 0)
                    result.Add(targets[i]);
                else
                    break;
            }

            return result;
        }
    }

    public class KuangbiAI : SkillEvent
    {
        public KuangbiAI() : base("kuangbi")
        {
        }

        public override List<WrappedCard> GetTurnUse(TrustedAI ai, Player player)
        {
            if (!player.HasUsed(KuangbiCard.ClassName))
                return new List<WrappedCard> { new WrappedCard(KuangbiCard.ClassName) { Skill = Name } };

            return new List<WrappedCard>();
        }

        public override List<int> OnExchange(TrustedAI ai, Player player, string pattern, int min, int max, string pile)
        {
            List<int> ids = player.GetCards("he"), result = new List<int>();
            List<double> values = ai.SortByKeepValue(ref ids, false);
            

            Room room = ai.Room;
            if (ai.IsFriend(room.Current))
            {
                for (int i = 0; i < Math.Min(3, ids.Count); i++)
                {
                    if (values[i] < 0)
                        result.Add(ids[i]);
                    else if ((values[i] < 2 && ai.GetOverflow(player) > i) || (values[i] < 4 && ai.GetOverflow(player) > i + 3))
                        result.Add(ids[i]);
                }
            }
            else
            {
                result.Add(ids[0]);
            }

            return result;
        }
    }

    public class KuangbiCardAI : UseCard
    {
        public KuangbiCardAI() : base(KuangbiCard.ClassName){}

        public override void Use(TrustedAI ai, Player player, ref CardUseStruct use, WrappedCard card)
        {
            Room room = ai.Room;
            List<Player> friends = ai.FriendNoSelf, enemies = ai.GetEnemies(player);
            ai.SortByDefense(ref friends, false);
            ai.SortByDefense(ref enemies, false);
            foreach (Player p in friends)
            {
                foreach (int id in p.GetEquips())
                {
                    if (ai.GetKeepValue(id, p) < 0)
                    {
                        use.Card = card;
                        use.To.Add(p);
                        return;
                    }
                }
            }
            if (!ai.IsWeak(player) || ai.MaySave(player))
            {
                foreach (Player p in friends)
                {
                    if (ai.WillSkipPlayPhase(p) && ai.GetOverflow(p) > 0)
                    {
                        use.Card = card;
                        use.To.Add(p);
                        return;
                    }
                }

                foreach (Player p in friends)
                {
                    if (ai.GetOverflow(p) > 0)
                    {
                        use.Card = card;
                        use.To.Add(p);
                        return;
                    }
                }
            }

            foreach (Player p in enemies)
            {
                if (!p.IsNude())
                {
                    bool lose = false;
                    foreach (int id in p.GetEquips())
                    {
                        if (ai.GetKeepValue(id, p) < 0)
                        {
                            lose = true;
                            break;
                        }
                    }

                    if (!lose)
                    {
                        use.Card = card;
                        use.To.Add(p);
                        return;
                    }
                }
            }
        }

        public override double UsePriorityAdjust(TrustedAI ai, Player player, List<Player> targets, WrappedCard card)
        {
            return 3;
        }
    }

    public class ZenhuiAI : SkillEvent
    {
        public ZenhuiAI() : base("zenhui")
        {
        }

        public override List<Player> OnPlayerChosen(TrustedAI ai, Player player, List<Player> targets, int min, int max)
        {
            Room room = ai.Room;
            if (room.GetTag("zenhui_data") is CardUseStruct use)
            {
                foreach (Player p in targets)
                {
                    if (ai.IsFriend(p))
                    {
                        foreach (int id in p.GetEquips())
                            if (ai.GetKeepValue(id, p) < 0)
                                return new List<Player> { p };
                    }
                }

                Player target = use.To[0];
                if ((use.Card.Name == Duel.ClassName || use.Card.Name.Contains(Slash.ClassName)) && ai.IsEnemy(target))
                {
                    foreach (Player p in targets)
                    {
                        if (ai.IsFriend(p) && p.IsNude())
                        {
                            if (use.Card.Name.Contains(Slash.ClassName) && (ai.HasSkill("lieong_jx|tieqi_jx|wushuang|jianchu", p) || player.GetMark("@luoyi") > 0))
                                return new List<Player> { p };

                            if (use.Card.Name.Contains(Duel.ClassName) && (ai.HasSkill("wushuang|jiang", p) || player.GetMark("@luoyi") > 0))
                                return new List<Player> { p };
                        }
                    }
                }

                if ((use.Card.Name == Duel.ClassName || use.Card.Name.Contains(Slash.ClassName)) && ai.IsEnemy(target) && (ai.GetPlayerTendency(target) != "rebel" || target.Hp > 1))
                {
                    foreach (Player p in targets)
                    {
                        if (!ai.IsFriend(p) && ai.IsCardEffect(use.Card, p, player))
                        {
                            DamageStruct damage = new DamageStruct(use.Card, p, target, 1 + use.Drank + use.ExDamage);
                            if (damage.Card.Name == FireSlash.ClassName)
                                damage.Nature = DamageStruct.DamageNature.Fire;
                            else if (damage.Card.Name == ThunderSlash.ClassName)
                                damage.Nature = DamageStruct.DamageNature.Thunder;

                            if (ai.GetDamageScore(damage).Score > 0)
                                return new List<Player> { p };
                        }
                    }
                }
                else if ((use.Card.Name == Snatch.ClassName || use.Card.Name == Dismantlement.ClassName) && target.GetEquips().Count == 0)
                {
                    foreach (Player p in targets)
                    {
                        if (!ai.IsFriend(p) && ai.IsCardEffect(use.Card, p, player))
                        {
                            return new List<Player> { p };
                        }
                    }
                }
            }

            return new List<Player>();
        }

        public override List<int> OnExchange(TrustedAI ai, Player player, string pattern, int min, int max, string pile)
        {
            Room room = ai.Room;
            List<int> ids = player.GetCards("he"), result = new List<int>();
            if (room.GetTag("zenhui_data") is CardUseStruct use)
            {
                List<double> values = ai.SortByKeepValue(ref ids, false);
                Player target = use.To[0];
                if (ai.IsFriend(target))
                {
                    if (ai.IsCardEffect(use.Card, use.From, player))
                    {
                        if (use.Card.Name == Duel.ClassName || use.Card.Name.Contains(Slash.ClassName))
                        {
                            DamageStruct damage = new DamageStruct(use.Card, player, target, 1 + use.Drank + use.ExDamage);
                            if (damage.Card.Name == FireSlash.ClassName)
                                damage.Nature = DamageStruct.DamageNature.Fire;
                            else if (damage.Card.Name == ThunderSlash.ClassName)
                                damage.Nature = DamageStruct.DamageNature.Thunder;

                            double good = ai.GetDamageScore(damage).Score;
                            if (good > 0)
                                return new List<int> { ids[0] };
                            else if (good < -4 && values[0] < 2)
                                return new List<int> { ids[0] };
                        }

                        if (use.Card.Name == Snatch.ClassName || use.Card.Name == Dismantlement.ClassName)
                        {
                            if (values[0] < 2)
                                return new List<int> { ids[0] };
                        }
                    }
                }
                else
                {
                    if (values[0] < 0)
                    {
                        return new List<int> { ids[0] };
                    }

                    if (ai.IsFriend(use.From))
                    {
                        KeyValuePair<Player, int> pair = ai.GetCardNeedPlayer(ids, new List<Player> { use.From });
                        if (pair.Key != null)
                            return new List<int> { pair.Value };
                    }

                    return new List<int> { ids[0] };
                }
            }

            return result;
        }
    }

    public class JiaojinAI : SkillEvent
    {
        public JiaojinAI() : base("jiaojin") { }

        public override List<int> OnExchange(TrustedAI ai, Player player, string pattern, int min, int max, string pile)
        {
            Room room = ai.Room;

            if (room.GetTag("jiaojin_data") is DamageStruct damage)
            {
                if (ai.GetDamageScore(damage, DamageStruct.DamageStep.Done).Score >= 0) return new List<int>();
                foreach (int id in player.GetCards("he"))
                {
                    FunctionCard fcard = Engine.GetFunctionCard(room.GetCard(id).Name);
                    if (fcard is OffensiveHorse || fcard is Weapon)
                        return new List<int> { id };
                }

                foreach (int id in player.GetCards("h"))
                {
                    FunctionCard fcard = Engine.GetFunctionCard(room.GetCard(id).Name);
                    if (fcard is Horse)
                        return new List<int> { id };
                }

                foreach (int id in player.GetCards("h"))
                {
                    FunctionCard fcard = Engine.GetFunctionCard(room.GetCard(id).Name);
                    if (fcard is EquipCard)
                        return new List<int> { id };
                }

                foreach (int id in player.GetCards("e"))
                {
                    FunctionCard fcard = Engine.GetFunctionCard(room.GetCard(id).Name);
                    if (fcard is Horse)
                        return new List<int> { id };
                }

                if (player.GetArmor())
                {
                    if (player.HasArmor(Vine.ClassName) && damage.Nature == DamageStruct.DamageNature.Fire)
                        return new List<int> { player.Armor.Key };

                    if (player.HasArmor(SilverLion.ClassName) && damage.Damage > 1)
                        return new List<int>();

                    if (player.Hp == 1) return new List<int> { player.Armor.Key };
                }
            }

            return new List<int>();
        }

        public override double CardValue(TrustedAI ai, Player player, WrappedCard card, bool isUse, Place place)
        {
            Room room = ai.Room;
            if (ai.HasSkill(Name, player) && !RoomLogic.IsVirtualCard(room, card))
            {
                FunctionCard fcard = Engine.GetFunctionCard(room.GetCard(card.Id).Name);
                if (fcard is EquipCard equip)
                    return 1;
            }

            return 0;
        }
    }

    public class ChunlaoAI : SkillEvent
    {
        public ChunlaoAI() : base("chunlao")
        {
        }

        public override List<int> OnExchange(TrustedAI ai, Player player, string pattern, int min, int max, string pile)
        {
            List<int> ids = new List<int>();
            foreach (int id in player.GetCards("h"))
                if (ai.Room.GetCard(id).Name.Contains(Slash.ClassName))
                    ids.Add(id);

            return ids;
        }
    }

    public class LihuoAI : SkillEvent
    {
        public LihuoAI() : base("lihuo")
        { }
        
        public override List<Player> OnPlayerChosen(TrustedAI ai, Player player, List<Player> target, int min, int max)
        {
            Room room = ai.Room;
            if (room.GetTag("extra_target_skill") is CardUseStruct use)
            {
                List<ScoreStruct> scores = new List<ScoreStruct>();
                foreach (Player p in target)
                {
                    ScoreStruct score = ai.SlashIsEffective(use.Card, p);
                    score.Players = new List<Player> { p };
                    scores.Add(score);
                }
                ai.CompareByScore(ref scores);
                if (scores[0].Score > 0)
                    return scores[0].Players;
            }

            return new List<Player>();
        }
    }

    public class DanshouAI : SkillEvent
    {
        public DanshouAI() : base("danshou")
        {
        }

        public override bool OnSkillInvoke(TrustedAI ai, Player player, object data)
        {
            if (player == ai.Room.Current)
            {
                if (player.IsWounded() && ai.GetKnownCardsNums(Peach.ClassName, "he", player) > 0) return false;
                if (ai.GetKnownCardsNums(ExNihilo.ClassName, "he", player) > 0) return false;
            }

            return true;
        }

        public override List<int> OnDiscard(TrustedAI ai, Player player, List<int> all, int min, int max, bool option)
        {
            Room room = ai.Room;
            double value = 0;
            Player target = room.Current;
            DamageStruct _damage = new DamageStruct(Name, player, target);
            ScoreStruct score = ai.GetDamageScore(_damage);

            List<int> ids = new List<int>(), discount = new List<int>();
            foreach (int id in player.GetCards("he"))
                if (RoomLogic.CanDiscard(room, player, player, id))
                    ids.Add(id);

            List<double> values = ai.SortByKeepValue(ref ids, false);
            for (int i = 0; i < target.HandcardNum; i++)
            {
                value += values[i];
                discount.Add(ids[i]);
            }
            if (value > 0) value /= 2;

            if (value < score.Score)
            {
                return discount;
            }

            return new List<int>();
        }
    }

    public class DuodaoAI : SkillEvent
    {
        public DuodaoAI() : base("duodao") {}

        public override List<int> OnDiscard(TrustedAI ai, Player player, List<int> cards, int min, int max, bool option)
        {
            Room room = ai.Room;
            List<int> ids = new List<int>();
            foreach (int id in player.GetCards("he"))
                if (RoomLogic.CanDiscard(room, player, player, id))
                    ids.Add(id);

            if (ids.Count > 0)
            {
                List<double> values = ai.SortByKeepValue(ref ids, false);
                if (values[0] < 0)
                    return new List<int> { ids[0] };

                if (values[0] < 3 && room.GetTag(Name) is DamageStruct damage && damage.To.Alive && ai.IsEnemy(damage.To) && damage.To.GetWeapon())
                    return new List<int> { ids[0] };
            }

            return new List<int>();
        }
    }

    public class AnjianAI : SkillEvent
    {
        public AnjianAI() : base("anjian") { }

        public override void DamageEffect(TrustedAI ai, ref DamageStruct damage, DamageStruct.DamageStep step)
        {
            Room room = ai.Room;
            if (damage.Card != null && damage.From != null && damage.Card.Name.Contains(Slash.ClassName) && ai.HasSkill(Name, damage.From) && damage.To != damage.From
                && !damage.Chain && !damage.Transfer && !RoomLogic.InMyAttackRange(room, damage.To, damage.From) && step <= DamageStruct.DamageStep.Caused)
                damage.Damage++;
        }
    }

    public class PojunAI : SkillEvent
    {
        public PojunAI() : base("pojun")
        {
        }

        public override bool OnSkillInvoke(TrustedAI ai, Player player, object data)
        {
            Room room = ai.Room;
            if (data is Player target && room.GetTag(Name) is CardUseStruct use)
            {
                if (ai.IsFriend(target))
                {
                    if (!player.ContainsTag(Name))
                    {
                        foreach (int id in target.GetEquips())
                            if (ai.GetKeepValue(id, target, Place.PlaceEquip) < 0)
                                return true;
                    }
                }
                else
                {
                    List<int> selected = player.ContainsTag(Name) ? (List<int>)player.GetTag(Name) : new List<int>();
                    List<int> cards = target.GetCards("h");
                    cards.RemoveAll(t => selected.Contains(t));
                    if (cards.Count == 0)
                    {
                        bool check = false;
                        foreach (int id in target.GetEquips())
                        {
                            if (!selected.Contains(id) && ai.GetKeepValue(id, target, Place.PlaceEquip) > 0)
                            {
                                check = true;
                                break;
                            }
                        }

                        if (!check) return false;
                    }

                    return true;
                }
            }

            return false;
        }

        public override List<int> OnCardsChosen(TrustedAI ai, Player from, Player to, string flags, int min, int max, List<int> disable_ids)
        {
            Room room = ai.Room;
            if (!ai.IsFriend(to) && room.GetTag(Name) is CardUseStruct use)
            {
                if (to.GetArmor() && (!ai.IsCardEffect(use.Card, to, from) || to.HasArmor(EightDiagram.ClassName)) && !disable_ids.Contains(to.Armor.Key))
                    return new List<int> { to.Armor.Key };

                List<int> cards = to.GetCards("h");
                cards.RemoveAll(t => disable_ids.Contains(t));
                if (cards.Count > 0)
                {
                    Shuffle.shuffle(ref cards);
                    return new List<int> { cards[0] };
                }
            }

            return base.OnCardsChosen(ai, from, to, flags, min, max, disable_ids);
        }
    }
}