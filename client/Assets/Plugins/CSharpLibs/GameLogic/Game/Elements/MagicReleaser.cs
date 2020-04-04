﻿using System;
using Layout;
using GameLogic.Game.LayoutLogics;
using EngineCore.Simulater;
using System.Collections.Generic;
using GameLogic.Game.Perceptions;
using Layout.LayoutEffects;
using Proto;
using Layout.LayoutElements;
using Layout.AITree;

namespace GameLogic.Game.Elements
{
    public enum ReleaserStates
    {
        NOStart,
        Starting,
        Releasing,
        ToComplete,
        Completing,
        Ended
    }

    public enum ReleaserType
    {
        Magic,
        Buff
    }

    public class RevertActionLock
    {
        public BattleCharacter target;
        public ActionLockType type;
    }

    public class RevertData
    {
        public BattleCharacter target;
        public HeroPropertyType property;
        public AddType addtype;
        public float addValue;
    }

    public class MagicReleaser : BattleElement<IMagicReleaser>,ICharacterWatcher
    {
        public float LastTickTime = -1;
        public float tickStartTime = -1;
        private readonly List<RevertActionLock> actionReverts = new List<RevertActionLock>();
        private readonly List<RevertData> reverts = new List<RevertData>();
        
        public MagicReleaser(
            string key,
            MagicData magic,
            BattleCharacter owner,
            IReleaserTarget target,
            GControllor controllor,
            IMagicReleaser view,
            ReleaserType type,float durtime)
            : base(controllor, view)
        {
            MagicKey = key;
            Owner = owner;
            ReleaserTarget = target;
            Magic = magic;
            RType = type;
            OnExitedState = ReleaseAll;
            this.Durtime = type == ReleaserType.Buff? durtime:-1;
        }

        public string MagicKey { private set; get; }

        public BattleCharacter Owner { set; private get; }

        public float Durtime { set; get; }

        public void SetParam(params string[] parms)
        {
            Params = parms;
        }

        private string[] Params;

        public string this[int paramIndex]
        {
            get
            {
                if (Params == null) return string.Empty;
                if (paramIndex < 0 || paramIndex >= Params.Length) return string.Empty;
                return Params[paramIndex];
            }
        }

        public ReleaserType RType { private set; get; }

        public MagicData Magic { private set; get; }

        public IReleaserTarget ReleaserTarget { private set; get; }

        public ReleaserStates State { private set; get; }

        public int UnitCount { get { return this._objs.Count; } }

        public void SetState(ReleaserStates state)
        {
            State = state;
        }

        public void OnEvent(EventType eventType, BattleCharacter target = null)
        {
            target = target??ReleaserTarget.ReleaserTarget;
            var per = this.Controllor.Perception as BattlePerception;
            LastEvent = eventType;

            for (var index = 0; index < Magic.Containers.Count; index++)
            {
                var i = Magic.Containers[index];
                if (i.type == eventType)
                {
                    var timeLine = i.line??per.View.GetTimeLineByPath(i.layoutPath);
                    if (timeLine == null) continue;
                    var player = new TimeLinePlayer(timeLine, this, i, target);
                    _players.AddLast(player);

                    if (i.line == null)
                        View.PlayTimeLine(i.layoutPath);//for runtime
                    else
                        View.PlayTest(i.line);//for editor

                    if (i.type == EventType.EVENT_START)
                    {
                        if (startLayout != null)
                        {
                            throw new Exception("Start layout must only one!");
                        }
                        startLayout = player;
                    }
                }
            } 
        }

        private TimeLinePlayer startLayout;

        public class AttachedElement
        {
            public GObject Element;
            public float time;
            public bool HaveLeftTime;
            public bool Managed = false;
        }

        private readonly Dictionary<int, AttachedElement> _objs = new Dictionary<int, AttachedElement>();

        public void AttachElement(GObject el, bool onlyWatch = false, float time = -1f)
        {
            if (_objs.ContainsKey(el.Index))
            {
                return;
            }
            var att = new AttachedElement()
            {
                time = time,
                Element = el,
                HaveLeftTime = time >= 0f,
                Managed = !onlyWatch
            };
            _objs.Add(el.Index, att); ;
        }

        internal void Cancel()
        {
            if (!IsLayoutStartFinish) StopAllPlayer();
            SetState(ReleaserStates.ToComplete);
        }

        private readonly LinkedList<TimeLinePlayer> _players = new LinkedList<TimeLinePlayer>();
        private readonly Queue<long> _removeTemp = new Queue<long>();

        public void Tick(GTime time)
        {
           
            var current = _players.First;
            while (current != null)
            {
                if (current.Value.Tick(time))
                {
                    current.Value.Destory();
                    _players.Remove(current);
                }
                current = current.Next;
            }

            foreach (var i in _objs)
            {
                if (!i.Value.Managed) continue;
                if (i.Value.Element.IsAliveAble)
                {
                    if (i.Value.HaveLeftTime)
                    {
                        i.Value.time -= time.DeltaTime;
                        if (i.Value.Element is BattleCharacter character)
                        {
                            if (i.Value.time <= 0)
                            {
                                character.SubHP(character.MaxHP,out _);
                            }
                        }
                    }
                    continue;
                }
                else
                {
                    _removeTemp.Enqueue(i.Key);
                    OnEvent(EventType.EVENT_UNIT_DEAD);
                }
            }

        }

        internal void ShowDamageRange(DamageLayout layout)
        {
            this.View.ShowDamageRanger(layout);
        }

        public bool IsCompleted
        {
            get
            {

                if (State == ReleaserStates.NOStart)
                    return false;

                var current = _players.First;
                while (current != null)
                {
                    if (!current.Value.IsFinshed) return false;
                    current = current.Next;
                }

                if (_objs.Count > 0)
                {
                    foreach (var i in _objs)
                    {
                        if (i.Value.Element.Enable) return false;
                    }
                }
                return true;
            }
        }

        public float GetLayoutTimeByPath(string path)
        {
            foreach (var i in _players)
            {
                if (i.TypeEvent.layoutPath == path) return i.PlayTime;
            }
            return -1f;
        }

        public EventType? LastEvent { get; private set; }

        public bool IsLayoutStartFinish
        {
            get
            {
                if (State == ReleaserStates.NOStart) return false;
                if (State == ReleaserStates.Starting && startLayout != null)
                    return startLayout.IsFinshed;
                return true;
            }
        }

        public BattleCharacter Releaser { get { return ReleaserTarget.Releaser; } }

        public BattleCharacter Target { get { return ReleaserTarget.ReleaserTarget; } }

        public int DisposeValue { get; internal set; } = 0;

        public void StopAllPlayer()
        {
            foreach (var i in _players)
            {
                i.Destory();
            }
            _players.Clear();
        }

        private readonly HashSet<int> hitList = new HashSet<int>();

        internal bool TryHit(BattleCharacter hit)
        {
            if (hitList.Contains(hit.Index)) return false;
            hitList.Add(hit.Index);
            return true;
        }

        private void ReleaseAll(GObject el)
        {
            foreach (var i in reverts)
            {
                if (i.target.Enable)
                {
                    i.target.ModifyValueMinutes(i.property, i.addtype, i.addValue);
                }
            }

            foreach (var i in actionReverts)
            {
                if (i.target.Enable)
                {
                    i.target.UnLockAction(i.type);
                }
            }

            actionReverts.Clear();
            reverts.Clear();

            foreach (var i in _objs)
            {
                if (!i.Value.Managed) continue;
                Destroy(i.Value.Element);
            }

            _objs.Clear();
            foreach (var i in _players)
            {
                i.Destory();
            }

            _players.Clear();

        }


        internal void DeAttachElement(BattleCharacter battleCharacter)
        {
            _objs.Remove(battleCharacter.Index);
        }

        public bool IsRuning(EventType type)
        {
            foreach (var i in _players)
            {
                if (i.IsFinshed) continue;
                if (i.TypeEvent.type == type) return true;
            }

            return false;
        }

        internal RevertData RevertProperty(BattleCharacter effectTarget, HeroPropertyType property, AddType addType, float addValue)
        {
            var rP = new RevertData { addtype = addType, addValue = addValue, property = property, target = effectTarget };
            reverts.Add(rP);
            return rP;

        }

        public RevertActionLock RevertLock(BattleCharacter effectTarget, ActionLockType lockType)
        {
            var rLock = new RevertActionLock { target = effectTarget, type = lockType };
            actionReverts.Add(rLock);
            return rLock;
        }

        protected override void OnJoinState()
        {
            base.OnJoinState();
            Releaser.AddEventWatcher(this);
        }

        protected override void OnExitState()
        {
            base.OnExitState();
            Releaser.RemoveEventWathcer(this);
        }

        void ICharacterWatcher.OnFireEvent(BattleEventType eventType, object args)
        {
            if (this.RType == ReleaserType.Buff)
            {
                switch (eventType)
                {
                    case BattleEventType.Skill:
                        if ((DisposeValue & (int)DisposeType.SKILL )>0)
                        {
                            this.ToCompleted();
                        }
                        break;
                    case BattleEventType.Move:
                        if ((DisposeValue & (int)DisposeType.MOVE) > 0)
                        {
                            this.ToCompleted();
                        }
                        break;
                    case BattleEventType.Hurt:
                        if ((DisposeValue & (int)DisposeType.HURT) > 0)
                        {
                            this.ToCompleted();
                        }
                        break;
                }
            }
        }

        private void ToCompleted()
        {
            if (State == ReleaserStates.Completing || State == ReleaserStates.Ended || State == ReleaserStates.ToComplete) return;
            State = ReleaserStates.ToComplete;
        }

        internal int TryGetParams(GetValueFrom vF)
        {
            int index =(int) vF - 1;
            if (this.Params.Length <= index) return 0;
            if (index < 0) return 0;
            if (int.TryParse(this.Params[index], out int v)) return v;
            return 0;
        }
    }
}

