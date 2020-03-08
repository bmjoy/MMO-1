using EngineCore.Simulater;
using Layout.LayoutEffects;
using GameLogic.Game.AIBehaviorTree;
using System;
using System.Collections.Generic;
using Proto;
using GameLogic.Game.Perceptions;
using EConfig;
using UVector3 = UnityEngine.Vector3;
using P = Proto.HeroPropertyType;
using ExcelConfig;
using Layout.AITree;
using static Proto.Notify_CharacterAttachMagic.Types;
using UnityEngine;

namespace GameLogic.Game.Elements
{
    public class BattleCharacterMagic
    {
        public MagicType Type { private set; get; }

        public CharacterMagicData Config { private set; get; }

        public int ConfigId { get { return Config.ID; } }

        public BattleCharacterMagic(MagicType type, CharacterMagicData config)
        {
            Type = type;
            Config = config;
        }

        public float LastTime { set; get; }
        public float CdTime { get { return Config.TickTime; } }

        public bool IsCoolDown(float time)
        {
            return time > LastTime + CdTime;
        }

        public float TimeToCd(float time)
        {
            return Math.Max(0, (LastTime + CdTime) - time);
        }

    }

    public delegate bool EachWithBreak(BattleCharacterMagic item);

    public class PushMove
    {
        private UVector3 dir;

        private readonly float distance;

        private float speed;

        public PushMove(Quaternion rotation, float dis, float speed)
        {
            dir = rotation *UVector3.forward ;
            distance = dis;
            this.speed = speed;
        }

        public Action FinishCall;

        public UVector3 Length { get { return dir * distance; } }

        public UVector3 Speed { get { return dir * speed; } }
    }

    public class BattleCharacter:BattleElement<IBattleCharacter>
	{
        private readonly Dictionary<P, ComplexValue> Properties = new Dictionary<P, ComplexValue>();
        private Dictionary<int, BattleCharacterMagic> Magics {  set; get; }
        private float LastNormalAttackTime = 0;
        private object tempObj;


        public TreeNode DefaultTree { get; set; }
        public string DefaultTreePath { set; get; }
        public event Action<BattleEventType, object> OnBattleEvent;
        public string AcccountUuid { private set; get; }
        public int AttackCount { private set; get; }
        public HeroCategory Category { set; get; }
        public DefanceType TDefance { set; get; }
        public DamageType TDamage { set; get; }
        public UVector3 BronPosition { private set; get; }
        public int MaxHP
        {
            get
            {
                return this[P.MaxHp].FinalValue + (int)(this[P.Force].FinalValue * BattleAlgorithm.FORCE_HP);
            }
        }
        public int MaxMP
        {
            get
            {
                var maxMP = this[P.MaxMp].FinalValue + (int)(this[P.Knowledge].FinalValue * BattleAlgorithm.KNOWLEGDE_MP);
                return maxMP;
            }
        }
        public float AttackSpeed
        {
            get
            {
                var time = this[P.MagicWaitTime].FinalValue
                    - BattleAlgorithm.AGILITY_SUBWAITTIME
                    * this[P.Agility].FinalValue;
                return BattleAlgorithm.Clamp(time / 1000, BattleAlgorithm.ATTACK_MIN_WAIT / 1000f, 100);
            }
        }
        public string Name { set; get; }
        public int TeamIndex { set; get; }
        public int Level { set; get; }
        public HanlderEvent OnDead;
        public int ConfigID { private set; get; }
        private ActionLock Lock {  set; get; }
        private float _speed;
        public float Speed
        {
            set
            {
                _speed = value;
                View.SetSpeed(Speed);
            }
            get
            {
                var speed = this[P.Agility].FinalValue * BattleAlgorithm.AGILITY_ADDSPEED + _speed;
                return Math.Min(BattleAlgorithm.MAX_SPEED, speed);
            }
        }
       
        public int HP { private set; get; }
        public int MP { private set; get; }
        public bool IsDeath
        {
            get
            {
                return HP == 0;
            }
        }
        private AITreeRoot _AiRoot;
        public AITreeRoot AiRoot
        {
            private set
            {
                _AiRoot = value;
                Debug.Log($"{this}->{value}");
            }
            get { return _AiRoot; }
        }
        public UVector3 Position
        {
            get
            {
                var t = View?.Transform;
                if (!t) return UVector3.zero;
                return t.position;
            }
            set
            {
                var tart = View?.Transform;
                if (!tart) return;
                View.SetPosition(value.ToPV3());
            }
        }
        public UVector3 Forward {
            get
            {
                var t = View?.Transform;
                if (!t) return UVector3.zero;
                return t.forward;
            }
        }
        public bool IsMoving { get { return View.IsMoving; } }
        public Quaternion Rototion
        {
            get
            {
                return View.Rotation;
            }
        }
        public Transform Transform { get { return this.View.RootTransform; } }
        //property
        public ComplexValue this[P type]
        {
            get { return Properties[type]; }
        }

        public BattleCharacter (
            int configID,
            List<CharacterMagicData> magics,
            float speed,
            GControllor controllor, 
            IBattleCharacter view, 
            string account_uuid):base(controllor,view)
		{
            AcccountUuid = account_uuid;
			HP = 0;
            _speed = speed;
			ConfigID = configID;
            Magics = new Dictionary<int, BattleCharacterMagic>();
            foreach (var i in magics)
            {
                if (Magics.ContainsKey(i.ID)) continue;
                Magics.Add(i.ID, new BattleCharacterMagic(MagicType.MtMagic, i));
            }
            var enums = Enum.GetValues(typeof(P));
            foreach (var i in enums)
            {
                var pr = (P)i;
                var value = new ComplexValue();
                Properties.Add(pr,value );
            }
            Lock = new ActionLock();
            Lock.OnStateOnchanged += (s, e) =>
            {
                switch (e.Type)
                {
                    case ActionLockType.NoMove:
                        if (e.IsLocked)
                        {
                            StopMove();
                        }
                        break;
                    case ActionLockType.NoInhiden:
                        view.SetAlpha(e.IsLocked ? 0.5f: 1);
                        break;
                    case ActionLockType.NoAi:
                        this.AiRoot?.Stop();
                        break;

                }
            };
            BronPosition = Position;
		}

        public bool AddMagic(CharacterMagicData data)
        {
            if (Magics.ContainsKey(data.ID)) return false;
            Magics.Add(data.ID, new BattleCharacterMagic(MagicType.MtMagic, data));
            return true;
        }

        public bool RemoveMaic(int id)
        {
           return  Magics.Remove(id);
        }

        internal void PlayMotion(string motionName)
        {
            View.PlayMotion(motionName);
        }

        public bool MoveTo(UVector3 target,float stopDis =0f)
        {
            if (IsLock(ActionLockType.NoMove)) return false;
            return View.MoveTo(View.Transform.position.ToPV3(), target.ToPV3(), stopDis);
        }

        private Action<BattleCharacter,object> launchHitCallback;

        internal void BeginLauchSelf(Quaternion rototion, float distance, float speed, Action<BattleCharacter,object> hitCallback, MagicReleaser releaser)
        {
            if (TryStartPush(rototion, distance, speed,out PushMove push))
            {
                push.FinishCall = () =>
                {
                    launchHitCallback = null;
                    releaser.DeAttachElement(this);
                };
                releaser.AttachElement(this, true);
                tempObj = releaser;
                launchHitCallback = hitCallback;
            }
        }

        public void HitOther(BattleCharacter character)
        {
            launchHitCallback?.Invoke(character, tempObj);
        }

        public bool MoveForward(UVector3 forward, UVector3 posNext)
        {
            if (IsLock(ActionLockType.NoMove)) return false;
            if (forward.magnitude > 0.001f)
            {
                View.SetMoveDir(posNext.ToPV3(), forward.ToPV3());
            }
            else {
                StopMove();
            }
            return true;
        }

        public void StopMove()
        {
            View.StopMove(Position.ToPV3());
        }

        public bool SubHP(int hp)
        {
            if (hp <= 0) return false;
            if (HP == 0) return true;
            HP -= hp;
            if (HP <= 0) HP = 0;
            var dead = HP == 0;//is dead
            View.ShowHPChange(-hp, HP, this.MaxHP);
            if (dead) OnDeath();
            return dead;
        }

        private PushMove currentPush;
        internal bool TryStartPush(Quaternion rototion, float distance, float speed, out PushMove push)
        {
            push = null;
            if (currentPush != null) return false;
            push = new PushMove(rototion, distance, speed);
            View.Push(push.Length.ToPV3(), push.Speed.ToPV3());
            currentPush = push;
            return true;
        }

        public void EndPush()
        {
            this.View.StopMove(this.Position.ToPV3());
            currentPush?.FinishCall?.Invoke();
            currentPush = null;
        }

        public void AddHP(int hp)
        {
            var maxHP = MaxHP;
            if (hp <= 0 || HP >= maxHP) return;
            HP += hp;
            if (HP >= maxHP) HP = maxHP;
            View.ShowHPChange(hp, HP, maxHP);
        }

        public bool SubMP(int mp)
        {
            if (mp <= 0 || MP - mp < 0) return false;
            MP -= mp;
            View.ShowMPChange(-mp, MP, this.MaxMP);
            return true;
        }

        public bool AddMP(int mp)
        {
            if (mp <= 0) return false;
            MP += mp;
            if (MP >= MaxMP) MP = MaxMP;
            View.ShowMPChange(mp, MP, MaxMP);
            return true;
        }

        private readonly Queue<AITreeRoot> _next = new Queue<AITreeRoot>();

        internal void SetAITreeRoot(AITreeRoot root, bool force = false)
        {
            if (force) _next.Clear();
            _next.Enqueue(root);
        }


        internal void TickAi()
        {
            if (_next.Count > 0)
            {
                AiRoot?.Stop();
                AiRoot = _next.Dequeue();
            }
            if (Lock.IsLock(ActionLockType.NoAi)) return;
            AiRoot?.Tick();
        }

        internal void LookAt(BattleCharacter releaserTarget)
        {
            View.LookAtTarget(releaserTarget.Index);
        }

        internal void Init()
		{
            HP = MaxHP;
            MP = MaxMP;
		}

		protected void OnDeath()
		{
			View.Death();
            OnDead?.Invoke(this);
            var per = this.Controllor.Perception as BattlePerception;
            per.StopAllReleaserByCharacter(this);
			//Destory(this, 5.5f);
		}

        public void AttachMagicHistory(int magicID, float now)
        {
            if (Magics.TryGetValue(magicID, out BattleCharacterMagic magic))
            {
                magic.LastTime = now;
                View.AttachMagic(magic.Type, magic.ConfigId, magic.LastTime + magic.CdTime);
            }
        }

        internal bool IsLock(ActionLockType type)
        {
            return Lock.IsLock(type);
        }

        public CharacterMagicData GetMagicById(int id)
        {
            if (!Magics.TryGetValue(id, out BattleCharacterMagic datat)) return null;
            return datat.Config;
        }

        public void AddNormalAttack(int att, int append)
        {
            var natt = ExcelToJSONConfigManager.Current.GetConfigByID<CharacterMagicData>(att);
            var nattapp = ExcelToJSONConfigManager.Current.GetConfigByID<CharacterMagicData>(append);
      
            Magics.Add(natt.ID, new BattleCharacterMagic(MagicType.MtNormal, natt));
            if (nattapp != null) Magics.Add(nattapp.ID, new BattleCharacterMagic(MagicType.MtNormalAppend, nattapp));
        }

        public bool IsCoolDown(int magicID, float now, bool autoAttach = false)
        {
            bool isOK = true;
            if (Magics.TryGetValue(magicID, out BattleCharacterMagic h)) isOK = h.IsCoolDown(now);
            if (autoAttach) AttachMagicHistory(magicID, now);
            return isOK;
        }

        public void ModifyValue(P property, AddType addType, float resultValue)
        {
            var value = this[property];
            value.ModifyValue(addType, resultValue);
            View.PropertyChange(property, value.FinalValue);
        }

        public void Reset()
        {
            Init();
        }

        public void IncreaseNormalAttack(float time)
        {
            LastNormalAttackTime = time;
            AttackCount++;
        }

        public void ResetNormalAttack(float time)
        {
            LastNormalAttackTime = time;
            AttackCount = 0;
        }

        private bool TryGetMaigcByType(MagicType magicType, out BattleCharacterMagic magic)
        {
            foreach (var i in Magics)
            {
                if (i.Value.Type == magicType)
                {
                    magic = i.Value;
                    return true;
                }
            }
            magic = null;
            return false;
        }

        public void EachActiveMagicByType(MagicType ty, float time, EachWithBreak call)
        {
            foreach (var i in Magics)
            {
                if (i.Value.Type != ty) continue;
                if (!i.Value.IsCoolDown(time)) continue;
                if (call?.Invoke(i.Value) == true) break;
            }
        }

        public bool TryGetNormalAtt(float now, out CharacterMagicData att, out bool isAppend)
        {
            att = null;
            isAppend = false;
            if (LastNormalAttackTime + this.AttackSpeed > now) return false;
            if (AttackCount > 2 && TryGetMaigcByType(MagicType.MtNormal, out BattleCharacterMagic m))
            {
                isAppend = true;
                att = m.Config;
            }
            else if (TryGetMaigcByType(MagicType.MtNormalAppend, out BattleCharacterMagic n))
            {
                att = n.Config;
            }
            return att != null;
        }

        public void LockAction(ActionLockType type)
        {
            Lock.Lock(type);
            View.SetLock(Lock.Value);
        }

        public void UnLockAction(ActionLockType type)
        {
            Lock.Unlock(type);
            View.SetLock(Lock.Value);
        }

        public void FireEvent(BattleEventType ev, object args)
        {
            OnBattleEvent?.Invoke(ev, args);
        }

        public override string ToString()
        {
            return $"[{Index}]{Name}";
        }
    }
}

