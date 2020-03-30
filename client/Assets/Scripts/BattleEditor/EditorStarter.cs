using UnityEngine;
using System.Collections;
using System.Linq;
using ExcelConfig;
using UnityEngine.SceneManagement;
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
using GameLogic.Game.LayoutLogics;
//#if UNITY_EDITOR

public class EditorStarter : XSingleton<EditorStarter> , IAIRunner, IStateLoader
{
	

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
		curState = new BattleState(PerView, this, PerView);
		curState.Init();
		curState.Start(Now);
		PerView.UseCache = false;
		UUIManager.S.CreateWindowAsync<Windows.UUIBattleEditor>(ui=>ui.ShowWindow());
		
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
		this.currentReleaser =per.CreateReleaser(string.Empty,this.releaser, magic,
            new ReleaseAtTarget(this.releaser, this.target),
			ReleaserType.Magic,0);

	}

	public void ReplaceRelease(int level,CharacterData data, bool stay, bool ai)
	{
		
		if (!stay && this.releaser)
            this.releaser.SubHP(this.releaser.HP,out _);
		var per = curState.Perception as BattlePerception;
		var scene = PerView.UScene;
		var magics = per.CreateHeroMagic(data.ID);
		var releaser = per.CreateCharacter(level, data, magics,null, 1,
			scene.startPoint.position + (UVector3.right * distanceCharacter / 2)
            , new UVector3(0, 90, 0), string.Empty, data.Name);
		if (ai) per.ChangeCharacterAI(data.AIResourcePath, releaser);
		this.releaser = releaser;
		tcamera.SetLookAt(releaser.Transform);
	}

	public void ReplaceTarget(int level,CharacterData data, bool stay, bool ai)
	{
		
		if (!stay&&this.target)
            this.target.SubHP(this.target.HP,out _);
		var per = curState.Perception as BattlePerception;
		var scene = PerView.UScene;
		var magics = per.CreateHeroMagic(data.ID);
		var target = per.CreateCharacter(level, data, magics,null, 2, scene.enemyStartPoint.position + (UVector3.left * distanceCharacter / 2),
			new UVector3(0, -90, 0), string.Empty, data.Name); ;
		if (ai) per.ChangeCharacterAI(data.AIResourcePath, target);
		this.target = target;
	}

    


	public void DoAction(IMessage action)
	{
		if (this.releaser == null) return;
		if (this.releaser.AiRoot == null) return;
		this.releaser?.AiRoot?.PushAction(action);
	}

    private bool isStarted = false;

	private ThridPersionCameraContollor tcamera;

	// Update is called once per frame
	void Update()
	{
		if (!isStarted) return;
		Tick();
		tcamera.rotationX =  slider_y;
		tcamera.distance = distance;
		tcamera.rotationY = ry;
		if (isChanged)
		{
			var position = PerView.UScene.startPoint.position;
			var left = position + (UVector3.left * distanceCharacter / 2);
			var right = position + (UVector3.right * distanceCharacter / 2);
			releaser.Position = left;
			target.Position = right;
			isChanged = false;
		}
	}

	public bool isChanged = false;

	public float slider_y = 1f;
	public float distance = 5;
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
		if (character.AiRoot != null) character.AiRoot.IsDebug = true;
    }

    public void Load(GState state)
    {
		var configs = ExcelToJSONConfigManager.Current.GetConfigs<CharacterData>();
		var releaserData = configs[0];
		var targetData = configs[1];
		curState = state;

		ReplaceRelease(1, releaserData, false, true);
		ReplaceTarget(1, targetData, false, true);
    }
}
//#endif
