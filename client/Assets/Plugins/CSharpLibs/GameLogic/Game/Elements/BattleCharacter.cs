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

namespace GameLogic.Game.Elements
{

    public class BattleCharacter:BattleElement<IBattleCharacter>
	{

        private readonly Dictionary<int, ReleaseHistory> _history = new Dictionary<int, ReleaseHistory>();
        private readonly Dictionary<P, ComplexValue> Properties = new Dictionary<P, ComplexValue>();
        public Dictionary<int,CharacterMagicData> Magics { private set; get; }
        public CharacterMagicData NormalAttack { private set; get; }
        public CharacterMagicData NormalAppend { private set; get; }
        public event Action<BattleEventType,object> OnBattleEvent;
        private float LastNormalAttackTime = 0;

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
        public AITreeRoot AIRoot { private set; get; }
        //position
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
        //forward
        public UVector3 Forward {
            get
            {
                var t = View?.Transform;
                if (!t) return UVector3.zero;
                return t.forward;
            }
        }

        public bool IsMoving { get { return View.IsMoving; } }

        public UnityEngine.Quaternion Rototion
        {
            get
            {
                return View.Rotation;
            }
        }

        public UnityEngine.Transform Transform { get { return this.View.RootTransform; } }
        //property
        public ComplexValue this[P type]
        {
            get { return Properties[type]; }
        }

        public BattleCharacter (
            int configID,
            List<CharacterMagicData> magics,
            GControllor controllor, 
            IBattleCharacter view, 
            string account_uuid):base(controllor,view)
		{
            AcccountUuid = account_uuid;
			HP = 0;
			ConfigID = configID;
            Magics = new Dictionary<int, CharacterMagicData>();
            foreach (var i in magics)
            {
                if (Magics.ContainsKey(i.ID)) continue;
                Magics.Add(i.ID, i);
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
                    case ActionLockType.Inhiden:
                        view.SetAlpha(e.IsLocked ? 0.5f: 1);
                        break;
                }
            };
            BronPosition = Position;
		}

        public bool AddMagic(CharacterMagicData data)
        {
            if (Magics.ContainsKey(data.ID)) return false;
            Magics.Add(data.ID, data);
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
			if (hp <= 0)  return false;
			if (HP == 0) return true;
			HP -= hp;
			if (HP <= 0) HP = 0;
			var dead = HP == 0;//is dead
            View.ShowHPChange(-hp, HP, this.MaxHP);
            if (dead) OnDeath();
			return dead;
		}

        public void AddHP(int hp)
        {
            var maxHP = MaxHP;
            if (hp <= 0) return;
            if (HP >= maxHP) return;
            HP += hp;
            if (HP >= maxHP) HP = maxHP;
            View.ShowHPChange(hp, HP, maxHP);
        }

        public bool SubMP(int mp)
        {
            if (mp <= 0)
                return false;
            if (MP - mp < 0) return false;
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

        public void SetAITree(AITreeRoot root)
        {
            AIRoot = root;
        }

        internal void LookAt(BattleCharacter releaserTarget)
        {
            View.LookAtTarget(releaserTarget.Index);
        }

        internal void Init()
		{
            HP = MaxHP;
            MP = MaxMP;
			_history.Clear();
		}

		protected void OnDeath()
		{
			View.Death();
            OnDead?.Invoke(this);
            var per = this.Controllor.Perception as BattlePerception;
            per.StopAllReleaserByCharacter(this);
			Destory(this, 5.5f);
		}

        public void AttachMagicHistory(int magicID, float now)
        {
            var data = ExcelToJSONConfigManager
                                      .Current.GetConfigByID<CharacterMagicData>(magicID);
            if (!_history.TryGetValue(magicID, out ReleaseHistory history))
            {
                history = new ReleaseHistory
                {
                    MagicDataID = magicID,
                    CdTime = data.TickTime,
                    LastTime = now
                };
                _history.Add(magicID, history);

            }
            history.LastTime = now;
            View.AttachMagic(data.ID, history.LastTime + history.CdTime);
        }

        internal bool IsLock(ActionLockType type)
        {
            return Lock.IsLock(type);
        }

        public CharacterMagicData GetMagicById(int id)
        {

            Magics.TryGetValue(id, out CharacterMagicData datat);

            return datat;
        }

        public void AddNormalAttack(int att, int append)
        {
            NormalAttack = ExcelToJSONConfigManager.Current.GetConfigByID<CharacterMagicData>(att);
            NormalAppend = ExcelToJSONConfigManager.Current.GetConfigByID<CharacterMagicData>(append);
        }

		public bool IsCoolDown(int magicID, float now, bool autoAttach = false)
		{
            bool isOK = true;
            if (_history.TryGetValue(magicID, out ReleaseHistory h))
			{ 
				isOK = h.IsCoolDown(now); 
			}
			if (autoAttach)
			{
				AttachMagicHistory(magicID, now);
			}
			return isOK;
		}

        public float GetCoolDwon(int magicID)
        {
            if (_history.TryGetValue(magicID, out ReleaseHistory h))
            {
                return h.CdTime;
            }
            return 0;
        }

        public void ModifyValue(P property, AddType addType, float resultValue)
        {
            var value = this[property];
            switch (addType)
            {
                case AddType.Append:
                    {
                        value.SetAppendValue((int)resultValue);
                    }
                    break;
                case AddType.Base:
                    {
                        value.SetBaseValue((int)resultValue);
                    }
                    break;
                case AddType.Rate:
                    {
                        value.SetRate((int)resultValue);
                    }
                    break;
            }
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

        public bool TryGetNormalAtt(float now, out CharacterMagicData att, out bool isAppend)
        {
            att = null;
            isAppend = false;
            if (LastNormalAttackTime + this.AttackSpeed > now) return false;

            if (AttackCount >2 && NormalAppend != null)
            {
                isAppend = true;
                att = NormalAppend;
            }
            else
            {
                att = NormalAttack;
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
    }
}

