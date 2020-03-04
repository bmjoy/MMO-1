using System;
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

        public bool IsDebug { set; get; }
        public object UserState { get { return Character; } }
        private Composite Current;

        private readonly Dictionary<string, object> _blackbroad = new Dictionary<string, object>();

        public string TreePath { private set; get; }

        public AITreeRoot(ITimeSimulater timeSimulater, BattleCharacter userstate,
            Composite root,TreeNode nodeRoot, string path)
        {
            this.TreePath = path;
            TimeSimulater = timeSimulater;
            Character = userstate;
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
                        outValue = magic.RangeMax;
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
                        outValue = magic.RangeMin;
                    }
                    break;
                case DistanceValueOf.ViewDistance:
                    outValue = Character[Proto.HeroPropertyType.ViewDistance].FinalValue / 100f;
                    break;
                case DistanceValueOf.Value:
                    break;
            }
            return true;
        }

        public TreeNode NodeRoot { private set; get; }

        public ITimeSimulater TimeSimulater { private set; get; }

        public BattlePerception Perception { get { return Character.Controllor.Perception as BattlePerception; } }

        public BattleCharacter Character { get; private set; }

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
                if (_blackbroad.TryGetValue(key, out object v)) return v;
                return null;
			}
		}


        public bool TryGet<T>(string key, out T v)
        {
            var t = this[key];
            v = default;
            if (!(t is T val)) return false;
            v = val;
            if (v == null) return false;
            return true;
        }


        public bool TryGetTarget(out BattleCharacter target)
        {
            target = null;
            if (!TryGet(TRAGET_INDEX, out int index)) return false;
            target = Perception.FindTarget(index);
            return target;
        }

        public bool TryGetAction<T>(out T action)
        {
            if (!TryGet(ACTION_MESSAGE, out action)) return false;
            this[ACTION_MESSAGE] = null;
            return true;
        }

        internal bool TryGetMagic(out CharacterMagicData magicData)
        {
            magicData = null;
            if (!TryGet(SELECT_MAGIC_ID, out int id))
            {
                return false;
            }

            magicData = ExcelToJSONConfigManager.Current.GetConfigByID<CharacterMagicData>(id);
            return magicData != null;

        }

        internal void Stop()
        {
            Current?.Stop(this);
        }

        public override string ToString()
        {
            return $"{TreePath}";
        }
    }
}

