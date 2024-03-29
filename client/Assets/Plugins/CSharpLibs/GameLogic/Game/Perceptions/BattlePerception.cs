﻿using System;
using EngineCore.Simulater;
using GameLogic.Game.Elements;
using GameLogic.Game.Controllors;
using Layout.LayoutElements;
using System.Collections.Generic;
using GameLogic.Game.LayoutLogics;
using Layout;
using Layout.AITree;
using GameLogic.Game.AIBehaviorTree;
using Proto;
using Layout.LayoutEffects;
using EConfig;
using UVector3 = UnityEngine.Vector3;
using UnityEngine;
using System.Linq;


namespace GameLogic.Game.Perceptions
{
    /// <summary>
    /// 战斗感知器
    /// </summary>
	public class BattlePerception : GPerception
    {
        public class EmptyControllor : GControllor
        {
            public EmptyControllor(BattlePerception p) : base(p) { }
            public override GAction GetAction(GTime time, GObject current)
            {
                return GAction.Empty;
            }
        }

        public static float Distance(BattleCharacter c1, BattleCharacter c2)
        {
            var size = c2.Radius + c1.Radius;
            return Math.Max(0, (c1.Position - c2.Position).magnitude - size);
        }

        public static float Distance(BattleCharacter c1, UVector3 c2)
        {
            return Math.Max(0, (c1.Position - c2).magnitude - c1.Radius);
        }

        public static bool InviewSide(BattleCharacter ower , BattleCharacter target, float viewDistance, float angle)
        {
            if (Distance(ower, target) > viewDistance) return false;
            var forward = target.Position - ower.Position;
            if (angle / 2 < UVector3.Angle(forward, ower.Forward)) return false;
            return true;
        }

        public BattlePerception(GState state, IBattlePerception view) : base(state)
        {
            View = view;
            Empty = new EmptyControllor(this);
            ReleaserControllor = new MagicReleaserControllor(this);
            BattleMissileControllor = new BattleMissileControllor(this);
            AIControllor = new AIControllor(this);
            BattleItemControllor = new BattleItemControllor(this);
        }

        public IBattlePerception View { private set; get; }


        #region controllor
        public BattleMissileControllor BattleMissileControllor { private set; get; }
        public MagicReleaserControllor ReleaserControllor { private set; get; }
        public AIControllor AIControllor { private set; get; }
        public BattleItemControllor BattleItemControllor { private set; get; }
        public EmptyControllor Empty { private set; get; }
        #endregion

        #region create Elements 
        public MagicReleaser CreateReleaser(string key, BattleCharacter owner, IReleaserTarget target, ReleaserType ty, ReleaserModeType RMType, float durtime)
        {
            var magic = View.GetMagicByKey(key);
            if (magic == null)
            {
                Debug.LogError($"{key} no found!");
                return null;
            }
            var releaser = CreateReleaser(key, owner, magic, target, ty, RMType, durtime);
            return releaser;
        }

        public MagicReleaser CreateReleaser(string key, BattleCharacter owner, MagicData magic, IReleaserTarget target, ReleaserType ty, ReleaserModeType RMType, float durtime)
        {
            if (magic.unique)
            {
                bool have = false;
                State.Each<MagicReleaser>(t =>
                {
                    if (t.Releaser.Index == owner.Index)
                    {
                        if (t.MagicKey == key)
                        {
                            have = true;
                            return true;
                        }
                    }
                    return false;
                });
                if (have) return null;
            }

            var view = View.CreateReleaserView(target.Releaser.Index,target.ReleaserTarget.Index, key,target.TargetPosition.ToPV3(),RMType);
            var mReleaser = new MagicReleaser(key, magic,owner, target, this.ReleaserControllor, view, ty, durtime);
            if(RMType == ReleaserModeType.RmtMagic) owner.FireEvent(BattleEventType.Skill, mReleaser);
            if(RMType == ReleaserModeType.RmtNormalAttack) owner.FireEvent(BattleEventType.NomarlAttack, mReleaser);
            this.JoinElement(mReleaser);
            return mReleaser;
        }

        public BattleMissile CreateMissile(MissileLayout layout, MagicReleaser releaser)
        {
            var view = this.View.CreateMissile(releaser.Index,
                layout.resourcesPath, layout.offset.ToV3(), layout.fromBone, layout.toBone, layout.speed);
            var missile = new BattleMissile(BattleMissileControllor, releaser, view, layout);
            this.JoinElement(missile);
            return missile;
        }

        #endregion

        #region Character
        public BattleCharacter CreateCharacter(
            int level,
            CharacterData data,
            IList<BattleCharacterMagic> magics,
            Dictionary<HeroPropertyType, int > appendProperties,
            int teamIndex,
            UVector3 position,
            UVector3 forward,
            string accountUuid,
            string name,int ownerIndex = -1, int hp = -1,int mp =-1)
        {
            
            var now = this.View.GetTimeSimulater().Now.Time;
            var cds = magics.Select(t => t.ToHeroMagic(now)) .ToList();
            var view = View.CreateBattleCharacterView(accountUuid, data.ID,
                teamIndex, position.ToPV3(), forward.ToPV3(), level, name,
                data.MoveSpeed, data.HPMax, data.HPMax,data.MPMax, data.MPMax,cds, ownerIndex );

            var battleCharacter = new BattleCharacter(data,magics,data.MoveSpeed, this.AIControllor,
                view, accountUuid,teamIndex, ownerIndex);

            battleCharacter[HeroPropertyType.MaxHp].SetBaseValue(data.HPMax);
            battleCharacter[HeroPropertyType.MaxMp].SetBaseValue(data.MPMax);
            battleCharacter[HeroPropertyType.Defance].SetBaseValue(data.Defance);
            battleCharacter[HeroPropertyType.DamageMin].SetBaseValue(data.DamageMin);
            battleCharacter[HeroPropertyType.DamageMax].SetBaseValue(data.DamageMax);
            battleCharacter[HeroPropertyType.Agility].SetBaseValue(data.Agility + (int)(level * data.AgilityGrowth));
            battleCharacter[HeroPropertyType.Force].SetBaseValue(data.Force + (int)(level * data.ForceGrowth));
            battleCharacter[HeroPropertyType.Knowledge].SetBaseValue(data.Knowledge + (int)(level * data.KnowledgeGrowth));
            battleCharacter[HeroPropertyType.MagicWaitTime].SetBaseValue((int)(data.AttackSpeed * 1000));
            battleCharacter[HeroPropertyType.ViewDistance].SetBaseValue((int)(data.ViewDistance * 100));
            battleCharacter.Level = level;
            battleCharacter.TDamage = (Proto.DamageType)data.DamageType;
            battleCharacter.TDefance = (DefanceType)data.DefanceType;
            battleCharacter.Category = (HeroCategory)data.Category;
            battleCharacter.Name = data.Name;

            if (appendProperties != null)
            {
                foreach (var i in appendProperties)
                {
                    var p = battleCharacter[i.Key];
                    p.SetBaseValue(p.BaseValue + i.Value);
                }
            }
            battleCharacter.ResetHPMP(hp, mp);
            this.JoinElement(battleCharacter);
            view.SetPriorityMove(data.PriorityMove);
            return battleCharacter;
        }

        internal void ProcessDamage(BattleCharacter sources, BattleCharacter effectTarget, DamageResult result)
        {
            View.ProcessDamage(sources.Index, effectTarget.Index, result.Damage, result.IsMissed, result.CrtMult);
            NotifyHurt(effectTarget);
            if (result.IsMissed) return;
            effectTarget.AttachDamage(sources.Index, result.Damage, View.GetTimeSimulater().Now.Time);
            if (effectTarget.SubHP(result.Damage,out bool dead))
            {
                if (dead) sources.FireEvent(BattleEventType.Killed, effectTarget);
            }
        }



        public BattleItem CreateItem(UVector3 ps, PlayerItem item, int groupIndex, int teamIndex)
        {
            var view = View.CreateDropItem(ps.ToPV3(), item, teamIndex, groupIndex);
            var ditem = new BattleItem(this.BattleItemControllor, view, item);
            JoinElement(ditem);
            return ditem;
        }

        public AITreeRoot ChangeCharacterAI(string pathTree, BattleCharacter character)
        {
            var ai = View.GetAITree(pathTree);
            return ChangeCharacterAI(ai, character,pathTree);
        }

        public AITreeRoot ChangeCharacterAI(TreeNode ai, BattleCharacter character, string path = null)
        {
            var comp = AITreeParse.CreateFrom(ai,View);
            var root = new AITreeRoot(View.GetTimeSimulater(), character, comp, ai,path);
            character.SetAITreeRoot(root);
            character.SetControllor(AIControllor);
            return root;
        }

        #endregion

        public BattleCharacter FindTarget(int target)
        {
            return this.State[target] as BattleCharacter;
        }

        public BattleCharacter FindTarget(BattleCharacter character, TargetTeamType type,
            float distance,
            float view,
            bool igDead = true,
            TargetSelectType sType = TargetSelectType.Nearest,
            TargetFilterType filterType = TargetFilterType.None)
        {

            var list = new List<BattleCharacter>();
            State.Each<BattleCharacter>(t =>
            {
                //隐身的不进入目标查找
                if (t.IsLock(ActionLockType.NoInhiden)) return false;
                if (igDead && t.IsDeath) return false;
                switch (type)
                {
                    case TargetTeamType.Enemy:
                        if (character.TeamIndex == t.TeamIndex)
                            return false;
                        break;
                    case TargetTeamType.OwnTeam:
                        if (character.TeamIndex != t.TeamIndex)
                            return false;
                        break;
                    case TargetTeamType.OwnTeamWithOutSelf:
                        if (character.Index == t.Index) return false;
                        if (character.TeamIndex != t.TeamIndex)
                            return false;
                        break;
                    case TargetTeamType.Own:
                        {
                            if (character.Index != t.Index) return false;
                        }
                        break;
                    case TargetTeamType.All:
                        break;
                    default:
                        return false;
                }

                if (!InviewSide(character, t, distance, view)) return false;
                switch (filterType)
                {
                    case TargetFilterType.Hurt:
                        if (t.HP == t.MaxHP) return false;
                        break;
                }
                list.Add(t);
                return false;
            });

            BattleCharacter target = null;

            if (list.Count > 0)
            {
                switch (sType)
                {
                    case TargetSelectType.Nearest:
                        {
                            target = list[0];
                            var d = Distance(target, character);
                            foreach (var i in list)
                            {
                                var temp = Distance(i, character);
                                if (temp < d)
                                {
                                    d = temp;
                                    target = i;
                                }
                            }
                        }
                        break;
                    case TargetSelectType.ForwardNearest:
                        {
                            var forward = character.Forward;
                            var dis = float.MaxValue;
                            //var angle = float.MaxValue;//  UVector3.Angle(target.Position - character.Position, forward);
                            foreach (var i in list)
                            {
                                var temp = UVector3.Angle(i.Position - character.Position, forward);
                                if (temp < 15)
                                {
                                    if (dis < Distance(i, character)) continue;
                                    dis = Distance(i, character);
                                    target = i;
                                }
                            }
                            //no nearest
                            if (target == null)
                            {
                                target = list[0];
                                var d = Distance(target, character);
                                foreach (var i in list)
                                {
                                    var temp = Distance(i, character);
                                    if (temp < d)
                                    {
                                        d = temp;
                                        target = i;
                                    }
                                }
                            }
                        }
                        break;
                    case TargetSelectType.Random:
                        target = GRandomer.RandomList(list);
                        break;
                    case TargetSelectType.HPMax:
                        {
                            target = list[0];
                            var d = target.HP;
                            foreach (var i in list)
                            {
                                var temp = i.HP;
                                if (temp > d)
                                {
                                    d = temp;
                                    target = i;
                                }
                            }
                        }
                        break;
                    case TargetSelectType.HPMin:
                        {
                            target = list[0];
                            var d = target.HP;
                            foreach (var i in list)
                            {
                                var temp = i.HP;
                                if (temp < d)
                                {
                                    d = temp;
                                    target = i;
                                }
                            }
                        }
                        break;
                    case TargetSelectType.HPRateMax:
                        {
                            target = list[0];
                            var d = (float)target.HP / target.MaxHP;
                            foreach (var i in list)
                            {
                                var temp = (float)i.HP / i.MaxHP; ;
                                if (temp > d)
                                {
                                    d = temp;
                                    target = i;
                                }
                            }
                        }
                        break;
                    case TargetSelectType.HPRateMin:
                        {
                            target = list[0];
                            var d = (float)target.HP / target.MaxHP;
                            foreach (var i in list)
                            {
                                var temp = (float)i.HP / i.MaxHP;
                                if (temp < d)
                                {
                                    d = temp;
                                    target = i;
                                }
                            }
                        }
                        break;
                }
            }

            return target;

        }

        public List<BattleCharacter> DamageFindTarget(BattleCharacter deTarget,
            UVector3 target,
            Quaternion rototion,
            FilterType fitler,
            Layout.LayoutElements.DamageType damageType,
            float radius, float angle, float offsetAngle,
            UVector3 offset, int teamIndex, bool igDeath = true)
        {
            switch (damageType)
            {

                case Layout.LayoutElements.DamageType.Rangle:
                    {
                        var orgin = target + rototion * offset;
                        var q = Quaternion.Euler(0, offsetAngle, 0);
                        var forward = q * rototion * UVector3.forward;
                        var list = new List<BattleCharacter>();
                        State.Each<BattleCharacter>((t) =>
                        {
                            if (igDeath && t.IsDeath) return false;//ig
                            //过滤
                            switch (fitler)
                            {
                                case FilterType.Alliance:
                                case FilterType.OwnerTeam:
                                    if (teamIndex != t.TeamIndex) return false;
                                    break;
                                case FilterType.EmenyTeam:
                                    if (teamIndex == t.TeamIndex) return false;
                                    break;

                            }
                            if (Distance(t, orgin) > radius) return false;
                            var len = t.Position - orgin;
                            if (angle < 360)
                            {
                                var an = UVector3.Angle(len, forward);
                                if (an > angle / 2) return false;
                            }
                            list.Add(t);
                            return false;
                        });
                        return list;
                    }

                case Layout.LayoutElements.DamageType.Single:
                default:
                    return new List<BattleCharacter> { deTarget };

            }
        }

        public void StopAllReleaserByCharacter(BattleCharacter character)
        {
            State.Each<MagicReleaser>(t =>
            {
                if (t.ReleaserTarget.Releaser == character)
                {
                    t.SetState(ReleaserStates.Ended);//防止AI错误
                    GObject.Destroy(t);
                }
                return false;
            });
        }

        public void BreakReleaserByCharacter(BattleCharacter character, BreakReleaserType type)
        {
            State.Each<MagicReleaser>(t =>
            {
                if (t.ReleaserTarget.Releaser == character)
                {
                    switch (type)
                    {
                        case BreakReleaserType.InStartLayoutMagic:
                            {
                                if (t.RType == ReleaserType.Magic)
                                {
                                    if (!t.IsLayoutStartFinish)
                                    {
                                        t.StopAllPlayer();
                                    }
                                    t.SetState(ReleaserStates.ToComplete);
                                }
                            }
                            break;
                        case BreakReleaserType.Buff:
                            {
                                if (t.RType == ReleaserType.Buff)
                                {
                                    t.SetState(ReleaserStates.ToComplete);
                                }
                            }
                            break;
                        case BreakReleaserType.ALL:
                            {
                                t.SetState(ReleaserStates.ToComplete);
                            }
                            break;
                    }


                }
                return false;
            });
        }

        public void NotifyHurt(BattleCharacter sources)
        {
            State.Each<BattleCharacter>((c) => 
            {
                if (c.IsDeath) return false;
                if (c.TeamIndex == sources.TeamIndex)
                {
                    if (Distance(c, sources) < BattleAlgorithm.HURT_NOTIFY_R)
                    {
                        c.FireEvent(BattleEventType.TeamBeAttack, sources);
                    }
                }
                return false;
            });
        }
    }
}

