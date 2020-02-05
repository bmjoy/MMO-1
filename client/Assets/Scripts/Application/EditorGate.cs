using GameLogic;
using EngineCore.Simulater;
using UnityEngine;
using GameLogic.Game.Perceptions;
using GameLogic.Game.Elements;
using Layout;
using ExcelConfig;
using System.Linq;
using GameLogic.Game.AIBehaviorTree;
using UGameTools;
using GameLogic.Game.States;
using EConfig;
using Google.Protobuf;
using UVector3 = UnityEngine.Vector3;

#if UNITY_EDITOR
public class EditorGate:UGate
{
	private class StateLoader : IStateLoader
	{

		public StateLoader(EditorGate gate)
		{
			Gate = gate;
		}

		private EditorGate Gate { set; get; }

		#region IStateLoader implementation
		public void Load(GState state)
		{
			var releaserData = ExcelToJSONConfigManager.Current.GetConfigByID<CharacterData>(1);
			var targetData = ExcelToJSONConfigManager.Current.GetConfigByID<CharacterData>(2);

			var releaserMagics = ExcelToJSONConfigManager
				.Current.GetConfigs<CharacterMagicData>(t => t.CharacterID == releaserData.ID).ToList();

			var targetMagics = ExcelToJSONConfigManager
				.Current.GetConfigs<CharacterMagicData>(t => t.CharacterID == targetData.ID).ToList();
			//throw new NotImplementedException ();
			var per = state.Perception as BattlePerception;
			var scene = (per.View as UPerceptionView).UScene;
			var releaser = per.CreateCharacter(1, releaserData, releaserMagics, 1,
				scene.startPoint.position,
				new UVector3(0, 90, 0), string.Empty,"releaser");
			var target = per.CreateCharacter(1, targetData, targetMagics, 2, scene.enemyStartPoint.position,
				new UVector3(0, -90, 0), string.Empty,"target");
			Gate.releaser = releaser;
			Gate.target = target;
		}
		#endregion

	}

	public const string EDITOR_LEVEL_NAME ="EditorReleaseMagic";

	#region implemented abstract members of UGate


	public UPerceptionView PerView { private set; get; }

    private GTime Now
	{
		get
		{
			var sim = PerView as ITimeSimulater;
			return sim.Now;
		}
	}
	
	protected override void JoinGate ()
	{
		PerView = UPerceptionView.Create();
		curState = new BattleState(PerView, new StateLoader(this), PerView);
		curState.Init ();
		curState.Start (Now);
		PerView.UseCache = false;
	}

	private GState curState;

	protected override void ExitGate ()
	{
		curState.Stop (Now);
	}

	protected override void Tick ()
    {
        if (curState != null)
        {
            GState.Tick(curState, Now);
        }
    }

	#endregion

	public MagicReleaser currentReleaser;

	public BattleCharacter releaser;
	public BattleCharacter target;

    public bool EnableTap = false;

	public void ReleaseMagic(MagicData magic)
	{
		Resources.UnloadUnusedAssets();
		if (currentReleaser != null)
		{
			GObject.Destory(currentReleaser);
		}
		var per = curState.Perception as BattlePerception;
		this.currentReleaser = per.CreateReleaser(magic,
			new GameLogic.Game.LayoutLogics.ReleaseAtTarget(this.releaser, this.target),
			ReleaserType.Magic);

	}
        
	public void ReplaceRelease(CharacterData data,bool stay, bool ai)
	{
        var magics = ExcelToJSONConfigManager
            .Current.GetConfigs<CharacterMagicData>(t => t.CharacterID == data.ID).ToList();

		if (!stay) this.releaser.SubHP (this.releaser.HP);
		var per = curState.Perception as BattlePerception;
		var scene = PerView.UScene;
        var releaser = per.CreateCharacter(1,data, magics, 1,
            scene.startPoint.position,new UVector3(0,90,0) , string.Empty,"Releaser");
		if(ai)
		per.ChangeCharacterAI (data.AIResourcePath, releaser);
		this.releaser = releaser;
	}

	public void ReplaceTarget(CharacterData data,bool stay, bool ai)
	{
        var magics = ExcelToJSONConfigManager
            .Current.GetConfigs<CharacterMagicData>(t => t.CharacterID == data.ID).ToList();
		if (!stay)  this.target.SubHP (this.target.HP);
		var per = curState.Perception as BattlePerception;
		var scene = PerView.UScene;
        var target =per.CreateCharacter(1,data,magics, 2,scene.enemyStartPoint.position,
			new UVector3(0,-90,0),string.Empty,"target");;
		if(ai)per.ChangeCharacterAI (data.AIResourcePath, target);
		this.target = target;
	}

	public void DoAction(IMessage action)
	{
		if (this.releaser == null) return;
		if (this.releaser.AIRoot == null) return;
		this.releaser.AIRoot[AITreeRoot.ACTION_MESSAGE] = action;
		this.releaser.AIRoot.BreakTree();
	}
}
#endif
