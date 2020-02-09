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

namespace GameLogic.Game.Elements
{

    public class BattleCharacter:BattleElement<IBattleCharacter>
	{

        private readonly Dictionary<int, ReleaseHistory> _history = new Dictionary<int, ReleaseHistory>();
        private readonly Dictionary<P, ComplexValue> Properties = new Dictionary<P, ComplexValue>();
        public Dictionary<int,CharacterMagicData> Magics { private set; get; }
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
                //500  - 20 *100
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
        public ActionLock Lock { private set; get; }
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
            Lock.OnStateOnchanged += (s, e) => {
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

        public void MoveTo(UVector3 target,float stopDis =0f)
        {
            View.MoveTo(View.Transform.position.ToPV3(), target.ToPV3(), stopDis);
        }

        public void MoveForward(UVector3 forward)
        {
            if (forward.magnitude > 0.1f)
            {
                View.SetMoveDir(View.Transform.position.ToPV3(), forward.ToPV3());
            }
            else {
                StopMove();
            }
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
        public void Init()
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
            var data = ExcelConfig.ExcelToJSONConfigManager
                                      .Current.GetConfigByID<CharacterMagicData>(magicID);
            if (!_history.TryGetValue(magicID, out ReleaseHistory history))
            {
                history = new ReleaseHistory
                {
                    MagicDataID = magicID,
                    CdTime = Math.Max(AttackSpeed, data.TickTime),
                    LastTime = now
                };
                _history.Add(magicID, history);

            }
            history.LastTime = now;
            View.AttachMagic(data.ID, history.LastTime + history.CdTime);
        }


        public CharacterMagicData GetMagicById(int id)
        {

            Magics.TryGetValue(id, out CharacterMagicData datat);

            return datat;
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

        public void ModifyValue(HeroPropertyType property, AddType addType, float resultValue)
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

        public void IncreaseAttackCount()
        {
            AttackCount++;
        }

        public void ResetAttackCount()
        {
            AttackCount = 0;
        }
    }
}

