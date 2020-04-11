using EConfig;
using EngineCore.Simulater;
using GameLogic;
using GameLogic.Game.AIBehaviorTree;
using GameLogic.Game.Elements;
using GameLogic.Game.Perceptions;
using GameLogic.Game.States;
using Layout;
using Layout.AITree;
using Proto;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using P = Proto.HeroPropertyType;
using CM = ExcelConfig.ExcelToJSONConfigManager;
using UnityEngine.SceneManagement;
using UGameTools;
using Google.Protobuf;
using GameLogic.Game.LayoutLogics;
using UnityEngine.AddressableAssets;

namespace Server
{

    public class LevelSimulaterAttribute : Attribute
    {
        public MapType MType { set; get; }
    }

   // [LevelSimulater(MType = MapType.MtNone)]
    public class BattleLevelSimulater : IStateLoader, IAIRunner
    {
        #region AI RUN
        private BattleCharacter aiAttach;
        AITreeRoot IAIRunner.RunAI(TreeNode ai)
        {
            if (aiAttach == null)
            {
                Debug.LogError($"Need attach a battlecharacter");
                return null;
            }

            if (this.State.Perception is BattlePerception p)
            {
                var root = p.ChangeCharacterAI(ai, this.aiAttach);
                root.IsDebug = true;
                return root;
            }

            return null;
        }

        bool IAIRunner.IsRuning(Layout.EventType eventType)
        {
            return false;
        }

        bool IAIRunner.ReleaseMagic(MagicData data)
        {
            return false;
        }

        void IAIRunner.Attach(BattleCharacter character)
        {
            aiAttach = character;
            if (character.AiRoot == null) return;
            character.AiRoot.IsDebug = true;
        }

        #endregion

        public BattleLevelSimulater(BattleLevelData data)
        {
            LevelData = data;
        }
        void IStateLoader.Load(GState state)
        {

        }
        ITimeSimulater timeSimulater;
        public UPerceptionView PerView { private set; get; }
        public BattleLevelData LevelData { get; private set; }
        public MapData MapConfig { get; private set; }
        public BattleState State { private set; get; }
        public GTime GetTime() { return timeSimulater.Now; }
        public MonsterGroupPosition[] MonsterGroup { private set; get; }
        private PlayerBornPosition[] playerBornPositions;

        public GTime TimeNow { get { return GetTime(); } }

        public IEnumerator Start()
        {
            AIRunner.Current = this;
            MapConfig = CM.Current.GetConfigByID<MapData>(LevelData.MapID);
            yield return Addressables.LoadSceneAsync($"Assets/Levels/{MapConfig.LevelName}.unity");
            yield return new WaitForEndOfFrame();

            PerView = UPerceptionView.Create();
            timeSimulater = PerView as ITimeSimulater;
            MonsterGroup = GameObject.FindObjectsOfType<MonsterGroupPosition>();
            playerBornPositions = GameObject.FindObjectsOfType<PlayerBornPosition>();
            yield return new WaitForEndOfFrame();
            State = new BattleState(PerView, this, PerView);
            State.Start(this.GetTime());
          

        }

        public bool TryGetElementByIndex<T>(int index, out T el) where T : GObject
        {
            if (this.State[index] is T e)
            {
                el = e;
                return true;
            }
            el = null;
            return false;
        }

        protected virtual int PlayerTeamIndex  { get; } = 1;

        public BattleCharacter CreateUser(BattlePlayer user)
        {
            BattleCharacter character = null;
            State.Each<BattleCharacter>(t =>
            {
                if (!t.Enable) return false;
                if (t.AcccountUuid == user.AccountId)
                {
                    character = t;
                    return true;
                }
                return false;
            });
            if (character != null) return character;
            var per = State.Perception as BattlePerception;
            var data = CM.Current.GetConfigByID<CharacterData>(user.GetHero().HeroID);
            var magic = user.GetHero().CreateHeroMagic();
            var appendProperties = new Dictionary<P, int>();
            foreach (var i in user.GetHero().Equips)
            {
                var equip = user.GetEquipByGuid(i.GUID);
                if (equip == null)
                {
                    Debug.LogError($"No found equip {i.GUID}");
                    continue;
                }
                var ps = equip.GetProperties();
                foreach (var p in ps)
                {
                    if (appendProperties.ContainsKey(p.Key))
                    {
                        appendProperties[p.Key] += p.Value.FinalValue;
                    }
                    else
                    {
                        appendProperties.Add(p.Key, p.Value.FinalValue);
                    }
                }
            }
            var pos = GRandomer.RandomArray(playerBornPositions).transform;//.position;        
            character = per.CreateCharacter(user.GetHero().Level, data,
                magic, appendProperties,
                PlayerTeamIndex, pos.position, pos.rotation.eulerAngles, user.AccountId, user.GetHero().Name);
            per.ChangeCharacterAI(data.AIResourcePath, character);
            return character;
        }

        public void Stop()
        {
            State?.Stop(TimeNow);
        }

        public IMessage[] GetInitNotify()
        {
            return PerView.GetInitNotify();
        }

        public IMessage[] Tick()
        {
            if (State == null) return new IMessage[0];
            OnTick();
            GState.Tick(State, TimeNow);
            return PerView.GetAndClearNotify();
        }

        protected virtual void OnTick() { }

        internal MagicReleaser CreateReleaser(string key, BattleCharacter heroCharacter, ReleaseAtTarget rTarget, ReleaserType Rt, int dur)
        {
            if (State.Perception is BattlePerception per)
            {
               return  per.CreateReleaser(key, heroCharacter, rTarget, Rt, dur);
            }
            return null;
        }

        private static readonly Dictionary<MapType, Type> Types = new Dictionary<MapType, Type>();

        static BattleLevelSimulater()
        {
            var t = typeof(BattleLevelSimulater);
            var types =t .Assembly.GetTypes();
            foreach (var i in types)
            {
                if (!i.IsSubclassOf(t)) continue;
                var atts = i.GetCustomAttributes(typeof(LevelSimulaterAttribute), false) as LevelSimulaterAttribute[];
                if (atts == null || atts.Length == 0) continue;
                Types.Add(atts[0].MType, i);
            }
        }

        public static BattleLevelSimulater Create(int levelID )
        {
            var level = CM.Current.GetConfigByID<BattleLevelData>(levelID);
            var MType = (MapType)level.MapType;

            if (Types.TryGetValue(MType, out Type t))
            {
                return Activator.CreateInstance(t, level) as BattleLevelSimulater;
            }

            return null;
        }
    }
}
