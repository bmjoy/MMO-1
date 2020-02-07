﻿using System;
using System.Collections.Generic;
using BehaviorTree;
using EConfig;
using EngineCore.Simulater;
using ExcelConfig;
using GameLogic.Game.Elements;
using GameLogic.Game.Perceptions;
using Layout.AITree;

namespace GameLogic.Game.AIBehaviorTree
{
    public class AITreeRoot :  ITreeRoot
    {

        public const string SELECT_MAGIC_ID = "MagicID";
        public const string TRAGET_INDEX = "TargetIndex";
        public const string ACTION_MESSAGE = "Action_Message";


        public AITreeRoot(ITimeSimulater timeSimulater, BattleCharacter userstate, Composite root,
                          TreeNode nodeRoot)
        {
            TimeSimulater = timeSimulater;
            UserState = userstate;
            Character = userstate;
            Root = root;
            NodeRoot = nodeRoot;
        }

        public bool GetDistanceByValueType(DistanceValueOf type, float value, out float outValue)
        {
            outValue = value;
            switch (type)
            {
                case DistanceValueOf.BlackboardMaigicRangeMax:
                    {
                        var data = this[SELECT_MAGIC_ID];
                        if (data == null)
                        {
                            return false;
                        }
                        var magic = ExcelToJSONConfigManager
                                               .Current
                                               .GetConfigByID<CharacterMagicData>((int)data);
                        if (magic == null)
                        {
                            return false;
                        }
                        outValue = magic.ReleaseRangeMax;
                    }
                    break;
                case DistanceValueOf.BlackboardMaigicRangeMin:
                    {
                        var data = this[SELECT_MAGIC_ID];
                        if (data == null)
                        {
                            return false;
                        }
                        var magic =ExcelToJSONConfigManager.Current.GetConfigByID<CharacterMagicData>((int)data);
                        if (magic == null)
                        {
                            return false;
                        }
                        outValue = magic.ReleaseRangeMin;
                    }
                    break;
                case DistanceValueOf.ViewDistance:
                    outValue = (float)this.Character[Proto.HeroPropertyType.ViewDistance].FinalValue / 100f;
                    break;
                case DistanceValueOf.Value:
                    break;
            }
            return true;
        }

        public TreeNode NodeRoot { private set; get; }

        public ITimeSimulater TimeSimulater { private set; get; }


        public BattlePerception Perception { get { return Character.Controllor.Perception as BattlePerception; } }

        public object UserState
        {
            get;
            private set;
        }

        public BattleCharacter Character { get; }

        public Composite Root { private set; get; }

        private bool NeedBreak = false;

        public void Tick()
        {
            if (Current == null)
            {
                Current = Root;
            }

            if (next != null)
            {
                Current.Stop(this);
                Current = next;
                next = null;
                Current.Start(this);
            }

            if (NeedBreak)
            {
                NeedBreak = false;
                Current.Stop(this);
                Current.Start(this);
            }

            if (Current.LastStatus == null)
            {
                Current.Start(this);
            }

            Current.Tick(this);
            if (Current.LastStatus.HasValue
                && Current.LastStatus.Value != BehaviorTree.RunStatus.Running)
            {
                Current.Stop(this);
                //重新从根执行
                Current = Root;
                Current.Start(this);
            }
        }

        private Composite next;

        public void Chanage(Composite cur)
        {
            next = cur;
        }

        public void BreakTree()
        {
            NeedBreak = true;
        }

        public void SetInt(string key, int value)
        {

            this[key] = value;
        }

        public int GetInt(string key)
        {
            var v = this[key];
            if (v == null) return 0;
            return (int)v;
        }


        public float Time
        {
            get
            {
                return TimeSimulater.Now.Time;
            }
        }

        public bool IsDebug { set; get; }

        private Composite Current;

		private readonly Dictionary<string, object> _blackbroad = new Dictionary<string, object>();

		public object this[string key] 
        { 
			set {
                if (value == null)
                {
                    _blackbroad.Remove(key);
                    return;
                }
				if (_blackbroad.ContainsKey(key)) _blackbroad[key] = value;
				else
					_blackbroad.Add(key, value);
			}
			get {
				object v;
				if (_blackbroad.TryGetValue(key, out v)) return v;
				return null;
			}
		}

	}
}

