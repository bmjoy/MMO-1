using UnityEngine;
using System.Collections;
using System.Linq;
using ExcelConfig;
using UnityEngine.SceneManagement;
using UGameTools;
using EConfig;
using GameLogic;
using EngineCore.Simulater;
using GameLogic.Game.Perceptions;
using UVector3 = UnityEngine.Vector3;
using GameLogic.Game.AIBehaviorTree;
using Google.Protobuf;
using GameLogic.Game.Elements;
using Layout;
using GameLogic.Game.States;
using XNet.Libs.Utility;
using Layout.AITree;
//#if UNITY_EDITOR

public class EditorStarter : XSingleton<EditorStarter> , IAIRunner
{
	private class StateLoader : IStateLoader
	{

		public StateLoader(EditorStarter gate)
		{
			Gate = gate;
		}

		private EditorStarter Gate { set; get; }

		#region IStateLoader implementation
		public void Load(GState state)
		{
			var configs = ExcelToJSONConfigManager.Current.GetConfigs<CharacterData>();
			var releaserData = configs[0];
			var targetData = configs[1];

			var releaserMagics = ExcelToJSONConfigManager
				.Current.GetConfigs<CharacterMagicData>(t => t.CharacterID == releaserData.ID).ToList();

			var targetMagics = ExcelToJSONConfigManager
				.Current.GetConfigs<CharacterMagicData>(t => t.CharacterID == targetData.ID).ToList();
			//throw new NotImplementedException ();
			var per = state.Perception as BattlePerception;
			var scene = (per.View as UPerceptionView).UScene;
			var releaser = per.CreateCharacter(1, releaserData, releaserMagics, 1,
				scene.startPoint.position,
				new UVector3(0, 90, 0), string.Empty, "releaser");
			var target = per.CreateCharacter(1, targetData, targetMagics, 2, scene.enemyStartPoint.position,
				new UVector3(0, -90, 0), string.Empty, "target");
			Gate.releaser = releaser;
			Gate.target = target;
		}
		#endregion

	}

	public const string EDITOR_LEVEL_NAME = "EditorReleaseMagic";

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

	private  IEnumerator  Start()
	{
		AIRunner.Current = this;
		new ExcelToJSONConfigManager(ResourcesManager.S);
		isStarted = false;
		Debuger.Loger = new UnityLoger();
		yield return SceneManager.LoadSceneAsync("Welcome", LoadSceneMode.Additive);

		//UApplication.IsEditorMode = true;
		tcamera = FindObjectOfType<ThridPersionCameraContollor>();
		isStarted = true;
		PerView = UPerceptionView.Create();
		curState = new BattleState(PerView, new StateLoader(this), PerView);
		curState.Init();
		curState.Start(Now);
		PerView.UseCache = false;
		UUIManager.S.CreateWindow<Windows.UUIBattleEditor>().ShowWindow();
	}

	private GState curState;

    private void OnDestroy()
    {
		curState.Stop(Now);
	}

	protected  void Tick()
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

		if (!target.Enable || !releaser.Enable)
		{
			Debug.LogError("No found target !");
			return;
        }
		var per = curState.Perception as BattlePerception;
		this.currentReleaser =per.CreateReleaser(string.Empty,magic,
            new GameLogic.Game.LayoutLogics.ReleaseAtTarget(this.releaser, this.target),
			ReleaserType.Magic);

	}

	public void ReplaceRelease(CharacterData data, bool stay, bool ai)
	{
		var magics = ExcelToJSONConfigManager
			.Current.GetConfigs<CharacterMagicData>(t => t.CharacterID == data.ID).ToList();

		if (!stay) this.releaser.SubHP(this.releaser.HP);
		var per = curState.Perception as BattlePerception;
		var scene = PerView.UScene;
		var releaser = per.CreateCharacter(1, data, magics, 1,
			scene.startPoint.position, new UVector3(0, 90, 0), string.Empty, "Releaser");
		if (ai)
			per.ChangeCharacterAI(data.AIResourcePath, releaser);
		this.releaser = releaser;
	}

	public void ReplaceTarget(CharacterData data, bool stay, bool ai)
	{
		var magics = ExcelToJSONConfigManager
			.Current.GetConfigs<CharacterMagicData>(t => t.CharacterID == data.ID).ToList();
		if (!stay) this.target.SubHP(this.target.HP);
		var per = curState.Perception as BattlePerception;
		var scene = PerView.UScene;
		var target = per.CreateCharacter(1, data, magics, 2, scene.enemyStartPoint.position,
			new UVector3(0, -90, 0), string.Empty, "target"); ;
		if (ai) per.ChangeCharacterAI(data.AIResourcePath, target);
		this.target = target;
	}

	public void DoAction(IMessage action)
	{
		if (this.releaser == null) return;
		if (this.releaser.AIRoot == null) return;
		this.releaser.AIRoot[AITreeRoot.ACTION_MESSAGE] = action;
		//this.releaser.AIRoot.BreakTree();
	}

    private bool isStarted = false;

	private ThridPersionCameraContollor tcamera;

	// Update is called once per frame
	void Update()
	{
		if (!isStarted) return;
		Tick();
		//tcamera.forward.y = -1.08f + slider_y;
		//tcamera.Distance = 22 - distance;
		//tcamera.rotationY = ry;

		var midd = tcamera.lookAt;
		if (isChanged)
		{
			var position = midd.position;
			var left = position + (UVector3.left * distanceCharacter / 2);
			var right = position + (UVector3.right * distanceCharacter / 2);
			releaser.Position = left;
			target.Position = right;
			isChanged = false;
		}
	}

	public bool isChanged = false;

	public float slider_y = 1f;
	public float distance = -5f;
	public float ry =0;
	public float distanceCharacter = 8;


	AITreeRoot IAIRunner.RunAI(TreeNode ai)
	{
		if (curState.Perception is BattlePerception p)
		{
			var root = p.ChangeCharacterAI(ai, releaser);
			root.IsDebug = true;
			return root;
		}
		return null;
	}

    bool IAIRunner.IsRuning(Layout.EventType eventType)
    {
		return currentReleaser?.IsRuning(eventType) == true;
    }

	bool IAIRunner.ReleaseMagic(MagicData data)
	{
		ReleaseMagic(data);
		return true;
	}

    void IAIRunner.Attach(BattleCharacter character)
    {
		releaser = character;
		if (character.AIRoot != null) character.AIRoot.IsDebug = true;
    }
}
//#endif
