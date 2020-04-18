using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EConfig;
using GameLogic;
using GameLogic.Game.Elements;
using Proto;
using UnityEngine;
using XNet.Libs.Utility;
using Vector3 = UnityEngine.Vector3;

namespace Server
{
    [LevelSimulater(MType = Proto.MapType.Boss)]
    public class BossLevelSimulater:BattleLevelSimulater
    {
        private BattleMosterCreator MonsterCreator { set; get; }

        public BossLevelSimulater(BattleLevelData data) : base(data)
        {
           
        }

        protected override void OnLoadCompleted()
        {
            base.OnLoadCompleted();
            MonsterCreator = new BattleMosterCreator(this.LevelData,this.MonsterGroup,this.Per)
            {
                OnDrop = (it) =>
                {
                    DoDrop(it.Pos, it.MDate, it.DataConfig, it.OwnerIndex, it.TeamIndex, it.Owner);
                }
            };
        }


        private void DoDrop(Vector3 pos, MonsterData monster, DropGroupData drop, int groupIndex, int teamIndex, BattleCharacter owner)
        {
            BattlePlayer player = null;
            if (owner && BattleSimulater.S.TryGetPlayer(owner.AcccountUuid, out player))
            {
                var exp = player.GetHero().Exprices;
                int expNew = player.AddExp(monster.Exp, out int old, out int newLevel);
                if (newLevel != old)
                {
                    player.HeroCharacter.SetLevel(newLevel);
                    player.HeroCharacter.ResetHPMP();//full mp and hp
                }
                var expNotify = new Notify_CharacterExp { Exp = expNew, Level = newLevel, OldExp = exp, OldLeve = old };
                player.Client.SendMessage(expNotify.ToNotityMessage());
            }

            if (drop == null) return;
            if (!GRandomer.Probability10000(drop.DropPro)) return;
            var items = drop.DropItem.SplitToInt();
            var pors = drop.Pro.SplitToInt();
            var nums = drop.DropNum.SplitToInt();
            if (owner)
            {
                var gold = GRandomer.RandomMinAndMax(drop.GoldMin, drop.GoldMax);
                if (gold > 0)
                {
                    if (player != null)
                    {
                        player.AddGold(gold);
                        var notify = new Notify_DropGold { Gold = gold, TotalGold = player.Gold };
                        player.Client.SendMessage(notify.ToNotityMessage());
                    }
                }
            }

            var count = GRandomer.RandomMinAndMax(drop.DropMinNum, drop.DropMaxNum);
            while (count > 0)
            {
                count--;
                var offset = new Vector3(UnityEngine.Random.Range(-1f, 1f), 0, UnityEngine.Random.Range(-1f, 1f));
                var index = GRandomer.RandPro(pors.ToArray());
                var item = new PlayerItem { ItemID = items[index], Num = nums[index] };
               this.Per.CreateItem(pos + offset, item, groupIndex, teamIndex);
            }
        }
        protected override void OnTick()
        {
            base.OnTick();
            MonsterCreator?.TryCreateMonster(this.TimeNow.Time);
        }
    }
}
