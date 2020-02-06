using EngineCore.Simulater;
using Layout.LayoutEffects;
using GameLogic.Game.AIBehaviorTree;
using System;
using System.Collections.Generic;
using Proto;
using GameLogic.Game.Perceptions;
using EConfig;

namespace GameLogic.Game.Elements
{

    public class BattleCharacter:BattleElement<IBattleCharacter>
	{


        
        private readonly Dictionary<int, ReleaseHistory> _history = new Dictionary<int, ReleaseHistory>();
        private readonly Dictionary<HeroPropertyType, ComplexValue> Properties = new Dictionary<HeroPropertyType, ComplexValue>();
        private float _speed;

        public List<CharacterMagicData> Magics { private set; get; }
        public string AcccountUuid { private set; get; }
        public HeroCategory Category { set; get; }
        public DefanceType TDefance { set; get; }
        public DamageType TDamage { set; get; }
        public int MaxHP
        {
            get
            {
                return this[HeroPropertyType.MaxHp].FinalValue + (int)(this[HeroPropertyType.Force].FinalValue * BattleAlgorithm.FORCE_HP);
            }
        }
        public int MaxMP
        {
            get
            {
                var maxMP = this[HeroPropertyType.MaxMp].FinalValue + (int)(this[HeroPropertyType.Knowledge].FinalValue * BattleAlgorithm.KNOWLEGDE_MP);
                return maxMP;
            }
        }
        public float AttackSpeed
        {
            get
            {
                //500  - 20 *100
                var time = this[HeroPropertyType.MagicWaitTime].FinalValue - BattleAlgorithm.AGILITY_SUBWAITTIME * this[HeroPropertyType.Agility].FinalValue;
                return BattleAlgorithm.Clamp(time / 1000, BattleAlgorithm.ATTACK_MIN_WAIT / 1000f, 100);
            }
        }
        public string Name { set; get; }
        public int TeamIndex { set; get; }
        public int Level { set; get; }
        public HanlderEvent OnDead;
        public int ConfigID { private set; get; }
        public ActionLock Lock { private set; get; }
        public float Speed
        {
            set
            {
                _speed = value;
                View.SetSpeed(Speed);
            }
            get
            {
                var speed = this[HeroPropertyType.Agility].FinalValue * BattleAlgorithm.AGILITY_ADDSPEED + _speed;
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

        public ComplexValue this[HeroPropertyType type]
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
            Magics = magics;
            var enums = Enum.GetValues(typeof(HeroPropertyType));
            foreach (var i in enums)
            {
                var pr = (HeroPropertyType)i;
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
		}

        public void MoveTo(UnityEngine.Vector3 target)
        {
            View.MoveTo(View.Transform.position.ToPV3(), target.ToPV3());
        }
        public void MoveForward(UnityEngine.Vector3 forward)
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
            View.StopMove(View.Transform.position.ToPV3());
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

        public bool HasMagicKey(string key)
        {
            foreach (var i in Magics)
            {
                if (i.MagicKey == key) return true;
            }
            return false;
        }

        public CharacterMagicData GetMagicByKey(string key)
        {
            foreach (var i in Magics)
            {
                if (i.MagicKey == key) return i;
            }
            return null;
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
    }
}

