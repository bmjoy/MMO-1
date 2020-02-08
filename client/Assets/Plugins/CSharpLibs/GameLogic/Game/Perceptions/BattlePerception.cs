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

        /// <summary>
        /// Distance the specified c1 and c2.
        /// </summary>
        /// <param name="c1">C1.</param>
        /// <param name="c2">C2.</param>
        public static float Distance(BattleCharacter c1, BattleCharacter c2)
        {
            return Math.Max(0, (c1.Position - c2.Position).magnitude - 1);
        }

        public BattlePerception(GState state, IBattlePerception view) : base(state)
        {
            View = view;
            BattleCharacterControllor = new BattleCharacterControllor(this);
            ReleaserControllor = new MagicReleaserControllor(this);
            BattleMissileControllor = new BattleMissileControllor(this);
            AIControllor = new BattleCharacterAIBehaviorTreeControllor(this);
        }

        public IBattlePerception View { private set; get; }


        #region controllor
        //初始化游戏中的控制器 保证唯一性
        public BattleCharacterControllor BattleCharacterControllor { private set; get; }
        public BattleMissileControllor BattleMissileControllor { private set; get; }


        public MagicReleaserControllor ReleaserControllor { private set; get; }
        public BattleCharacterAIBehaviorTreeControllor AIControllor { private set; get; }
        #endregion


        #region create Elements 
        public MagicReleaser CreateReleaser(string key, IReleaserTarget target, ReleaserType ty)
        {
            var magic = View.GetMagicByKey(key);
            if (magic == null)
            {
                Debug.LogError($"{key} no found!");
                return null;
            }
            var releaser = CreateReleaser(key, magic, target, ty);
            return releaser;
        }

        public MagicReleaser CreateReleaser(string key, MagicData magic, IReleaserTarget target, ReleaserType ty)
        {
            var view = View.CreateReleaserView(target.Releaser.Index,
                                               target.ReleaserTarget.Index,
                                               key,
                                               target.TargetPosition.ToPV3());
            var mReleaser = new MagicReleaser(magic, target, this.ReleaserControllor, view, ty);
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
            List<CharacterMagicData> magics,
            int teamIndex,
            UVector3 position,
            UVector3 forward, string accountUuid, string name)
        {

            var view = View.CreateBattleCharacterView(accountUuid, data.ID,
                teamIndex, position.ToPV3(), forward.ToPV3(), level, name,
                data.MoveSpeed, magics.Select(t=>t.ID).ToList());

            var battleCharacter = new BattleCharacter(data.ID, magics, this.BattleCharacterControllor, view, accountUuid);
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
            battleCharacter.TeamIndex = teamIndex;
            battleCharacter.Speed = data.MoveSpeed;
            view.SetPriorityMove(data.PriorityMove);
            battleCharacter.Init();

            this.JoinElement(battleCharacter);
            return battleCharacter;
        }

        internal IParticlePlayer CreateParticlePlayer(MagicReleaser relaser, ParticleLayout layout)
        {
            var p = View.CreateParticlePlayer(relaser.Index, layout.path, (int)layout.fromTarget,
                layout.Bind, layout.fromBoneName, layout.toBoneName, (int)layout.destoryType, layout.destoryTime);
            return p;
        }


        internal void ProcessDamage(BattleCharacter sources, BattleCharacter effectTarget, DamageResult result)
        {
            View.ProcessDamage(sources.Index, effectTarget.Index, result.Damage, result.IsMissed);
            if (result.IsMissed) return;
            CharacterSubHP(effectTarget, result.Damage);
        }

        public void CharacterSubHP(BattleCharacter effectTarget, int lostHP)
        {
            effectTarget.SubHP(lostHP);
        }

        public void CharacterAddHP(BattleCharacter effectTarget, int addHp)
        {
            effectTarget.AddHP(addHp);
        }


        public AITreeRoot ChangeCharacterAI(string pathTree, BattleCharacter character)
        {
            TreeNode ai = View.GetAITree(pathTree);
            return ChangeCharacterAI(ai, character);
        }

        public AITreeRoot ChangeCharacterAI(TreeNode ai, BattleCharacter character)
        {
            try
            {
                var comp = AITreeParse.CreateFrom(ai);
                //var state = State as BattleState;
                var root = new AITreeRoot(View.GetTimeSimulater(), character, comp, ai);
                character.SetAITree(root);
                character.SetControllor(AIControllor);
                return root;
            }
            catch(Exception ex) {
                Debug.LogException(ex);
                return null;
            }
        }

      

        #endregion

        //获取一个非本阵营目标
        public BattleCharacter GetSingleTargetUseRandom(BattleCharacter owner)
        {
            BattleCharacter target = null;

            this.State.Each<BattleCharacter>((t) =>
            {
                if (t.TeamIndex != owner.TeamIndex)
                {
                    target = t;
                    return true;
                }
                return false;
            });

            return target;
        }

        /// <summary>
        /// 获取目标
        /// </summary>
        /// <param name="target"></param>
        /// <returns></returns>
        public BattleCharacter FindTarget(int target)
        {
            return this.State[target] as BattleCharacter;
        }

        /// <summary>
        /// Finds the target.
        /// </summary>
        /// <returns>The target.</returns>
        /// <param name="character">Character.</param>
        /// <param name="fitler">Fitler.</param>
        /// <param name="damageType">Damage type.</param>
        /// <param name="radius">Radius.</param>
        /// <param name="angle">Angle.</param>
        /// <param name="offsetAngle">Offset angle.</param>
        /// <param name="offset">Offset.</param>
        /// <param name="teamIndex">team</param>
        public List<BattleCharacter> FindTarget(
            BattleCharacter character,
            FilterType fitler,
            Layout.LayoutElements.DamageType damageType,
            float radius, float angle, float offsetAngle,
            UVector3 offset, int teamIndex)
        {
            var sqrRadius = radius * radius;
            switch (damageType)
            {
                case Layout.LayoutElements.DamageType.Single://单体直接对目标
                    return new List<BattleCharacter> { character };
                case Layout.LayoutElements.DamageType.Rangle:
                    {
                        var orgin = character.Position + offset;
                        var forward = character.Forward;

                        var q = Quaternion.Euler(0, angle, 0);

                        forward = q * forward;

                        var list = new List<BattleCharacter>();
                        State.Each<BattleCharacter>((t) =>
                        {

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
                            //不在目标区域内
                            if ((orgin - t.Position).sqrMagnitude > sqrRadius) return false;
                            if (angle < 360)
                            {
                                if (UVector3.Angle(forward, t.Forward) > (angle / 2)) return false;
                            }
                            list.Add(t);
                            return false;
                        });
                        return list;
                    }
            }

            return new List<BattleCharacter>();
        }

        public void StopAllReleaserByCharacter(BattleCharacter character)
        {
            State.Each<MagicReleaser>(t =>
            {
                if (t.ReleaserTarget.Releaser == character)
                {
                    t.SetState(ReleaserStates.Ended);//防止AI错误
                    GObject.Destory(t);
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

    }
}

