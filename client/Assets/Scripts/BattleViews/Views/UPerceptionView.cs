using UnityEngine;
using GameLogic.Game.Perceptions;
using Layout.LayoutElements;
using GameLogic.Game.Elements;
using GameLogic.Game.LayoutLogics;
using System.Collections.Generic;
using EngineCore.Simulater;
using Layout.AITree;
using Quaternion = UnityEngine.Quaternion;
using Layout;
using GameLogic;
using Google.Protobuf;
using Proto;
using System;
using ExcelConfig;
using EConfig;
using UGameTools;
using GameLogic.Game.AIBehaviorTree;

public class UPerceptionView : MonoBehaviour, IBattlePerception, ITimeSimulater, IViewBase
{
    public UGameScene UScene;
    public bool UseCache = true;

    private TreeNode LoadTreeXml(string pathTree)
    {
        var xml = ResourcesManager.Singleton.LoadText(pathTree);
        var root = XmlParser.DeSerialize<TreeNode>(xml);
        return root;
    }

    public int OwnerIndex { set; get; } = -1;

    public int OwerTeamIndex { set; get; } = -1;

    void Awake()
    {
        UScene = FindObjectOfType<UGameScene>();
        _magicData = new Dictionary<string, MagicData>();
        _timeLines = new Dictionary<string, TimeLine>();
#if !UNITY_SERVER
        GPUBillboardBuffer.S.Init();
        GPUBillboardBuffer.S.SetupBillboard(1000);
        GPUBillboardBuffer.S.SetDisappear(1);
        GPUBillboardBuffer.S.SetScaleParams(0f, 0.5f, 0.5f, 1f, 1f);

        param = new DisplayNumerInputParam()
        {
            RandomXInitialSpeedMin = 0f,
            RandomXInitialSpeedMax = 0f,

            RandomYInitialSpeedMin = 1f,
            RandomYInitialSpeedMax = 2f,

            RandomXaccelerationMin = 0,
            RandomXaccelerationMax = 0,

            RandomYaccelerationMin = 1,
            RandomYaccelerationMax = 3,
            NormalTime =.2f,
            FadeTime = .5f,

        };
#endif
    }

    private DisplayNumerInputParam param;

    public void ShowHPCure(UnityEngine.Vector3 pos, int hp)
    {
#if !UNITY_SERVER
        GPUBillboardBuffer.S.DisplayNumberRandom($"{hp}", new Vector2(.2f, .2f), pos, Color.green, true, param);
#endif
    }

    internal void ShowMPCure(UnityEngine.Vector3 pos, int mp)
    {
#if !UNITY_SERVER
        GPUBillboardBuffer.S.DisplayNumberRandom($"{mp}", new Vector2(.2f, .2f), pos, Color.blue, true, param);
#endif
    }

    public T GetViewByIndex<T>(int releaseIndex) where T: UElementView
    {
        if (AttachElements.TryGetValue(releaseIndex, out UElementView vi))
            return vi as T;
        return null;
    }

    void Update()
    {
        now = new GTime(Time.time, Time.deltaTime);
    }

	public int timeLineCount = 0;
	public int magicCount =0;

    private Dictionary<string,TimeLine> _timeLines;
    private Dictionary<string ,MagicData> _magicData;

    private readonly Queue<IMessage> _notify = new Queue<IMessage>();

    private readonly Dictionary<int, UElementView> AttachElements = new Dictionary<int, UElementView>();

    private readonly Dictionary<int, UMagicReleaserView> OwnerReleasers = new Dictionary<int, UMagicReleaserView>();

    public void DeAttachView(UElementView battleElement)
    {
        AttachElements.Remove(battleElement.Index);
        if (battleElement is UMagicReleaserView r)
        {
            if (r.CharacterReleaser.Index == OwnerIndex)
            {
                OwnerReleasers.Remove(r.Index);//, r);
            }
        }
    }

    public void AttachView(UElementView battleElement)
    {
        AttachElements.Add(battleElement.Index, battleElement);
        if (battleElement is UMagicReleaserView r)
        {
            if (r.CharacterReleaser.Index == OwnerIndex)
            {
                OwnerReleasers.Add(r.Index, r);
            }
        }
    }

    public bool HaveOwnerKey(string key)
    {
        foreach (var i in OwnerReleasers)
        {
            if (i.Value.Key == key) return true;
        }
        return false;
    }

    

    public IMessage[] GetAndClearNotify()
    {
        if (_notify.Count > 0)
        {
            var list = _notify.ToArray();
            _notify.Clear();
            return list;
        }
        else
            return new IMessage[0];
    }

    public IMessage[] GetInitNotify()
    {
        var list = new List<IMessage>();
        foreach (var i in AttachElements)
        {
            if (i.Value is ISerializerableElement sElement)
            {
                list.Add(sElement.ToInitNotify());
            }
        }
        return list.ToArray();
    }

    public void AddNotify(IMessage notify)
    {
#if UNITY_SERVER || UNITY_EDITOR
        _notify.Enqueue(notify);
#endif
    }

    private GTime now;

    public GTime GetTime() { return now; }

    public static UPerceptionView Create()
    {
        var go = new GameObject("PreView");
        return go.AddComponent<UPerceptionView>();
    }

    GTime ITimeSimulater.Now => GetTime();

    private TimeLine TryToLoad(string path)
    {
        var lineAsset = ResourcesManager.S.LoadText(path);
        if (string.IsNullOrEmpty(lineAsset)) return null;

        var line = XmlParser.DeSerialize<TimeLine> (lineAsset);
        if (UseCache) 
        {
            _timeLines.Add (path, line);
        } 
        return line;
    }

    private MagicData TryLoadMagic(string key)
    {
        if (!_magicData.TryGetValue(key, out _))
        {
            var asset = ResourcesManager.S.LoadText($"Magics/{key}.xml");
            if (string.IsNullOrEmpty(asset)) return null;
            MagicData magic = XmlParser.DeSerialize<MagicData>(asset);
            if (UseCache) _magicData.Add(key, magic);
            return magic;
        }
        return null;
    }


    public void Each<T>(Func<T, bool> invoke) where T : UElementView
    {
        foreach (var i in AttachElements)
        {
            if (!i.Value) continue;
            if (!(i.Value is T t)) continue;
            if (invoke?.Invoke(t) == true) return;
        }
    }
    #region IBattlePerception implementation

    bool IBattlePerception.ProcessDamage(int owner, int target, int damage, bool isMissed, int crtmult)
    {
#if UNITY_SERVER|| UNITY_EDITOR
        AddNotify(new Notify_DamageResult
        {
            Index = owner,
            TargetIndex = target,
            Damage = damage,
            IsMissed = isMissed,
            CrtMult = crtmult
        });
#endif

#if !UNITY_SERVER
        UCharacterView chDisplay = GetViewByIndex<UCharacterView>(isMissed ? owner : target);
        if (chDisplay)
        {
            var bone = chDisplay.GetBoneByName(UCharacterView.TopBone);
            if (bone)
            {
                var pos = bone.transform.position;
                GPUBillboardBuffer.S.
                    DisplayNumberRandom((isMissed ? "MISS" : $"{damage}"), new Vector2(.2f, .2f) * crtmult, pos, Color.red, true, param);
            }
        }
#endif
        return true;
    }

    TimeLine IBattlePerception.GetTimeLineByPath(string path)
    {
        if (UseCache && _timeLines.TryGetValue(path, out TimeLine line)) return line;
        line = TryToLoad(path);
        if (line == null)
        {
            Debug.LogError($"Nofound:{path}");
        }
        return line;
    }

    MagicData IBattlePerception.GetMagicByKey(string key)
    {
        MagicData magic;
        if (UseCache)
        {
            if (_magicData.TryGetValue(key, out magic))
            {
                return magic;
            }
        }
        magic = TryLoadMagic(key);
        if (magic == null) Debug.LogError("No found magic by key:" + key);
        return magic;
    }

    bool IBattlePerception.ExistMagicKey (string key)
	{
        TryLoadMagic(key);
        return _magicData.ContainsKey (key);
	}

    IBattleCharacter IBattlePerception.CreateBattleCharacterView(string account_id,
        int config, int teamId, Proto.Vector3 pos, Proto.Vector3 forward,
        int level, string name, float speed,int hp, int hpMax,int mp,int mpMax ,IList<HeroMagicData> cds,int owner)
    {
        var data = ExcelToJSONConfigManager.Current.GetConfigByID<CharacterData>(config);
        
        var qu = Quaternion.Euler(forward.X, forward.Y, forward.Z);
       
        var root = new GameObject(data.ResourcesPath);
        root.transform.SetParent(this.transform, false);
        root.transform.position = pos.ToUV3();
        root.transform.rotation = Quaternion.identity;
        var body = new GameObject("__VIEW__");
        body.transform.SetParent(root.transform, false);
        body.transform.RestRTS();

        var view = root.AddComponent<UCharacterView>();
        view.SetPrecpetion(this);
        view.LookQuaternion = view.targetLookQuaternion = qu;
        view.TeamId = teamId;
        view.Level = level;
        view.Speed = speed;
        view.ConfigID = config;
        view.AccoundUuid = account_id;
        view.Name = name;
        view.OwnerIndex  = owner;
        if (cds != null) { foreach (var i in cds) view.AddMagicCd(i.MagicID, i.CDTime, i.MType); }
        if (view is IBattleCharacter ch) ch.SetHpMp(hp, hpMax, mp, mpMax);
        view.SetCharacter(body, data.ResourcesPath);
        return view;
    }

    IMagicReleaser IBattlePerception.CreateReleaserView(int releaser, int target, string magicKey, Proto.Vector3 targetPos)
    {
        var obj = new GameObject($"Rleaser:{magicKey}");
        obj.transform.SetParent(this.transform, false);
        var view = obj.AddComponent<UMagicReleaserView>();

        view.Key = magicKey;
        view.SetPrecpetion(this);
        view.SetCharacter(releaser, target);
        return view;
    }
        
    IBattleMissile IBattlePerception.CreateMissile(int releaseIndex, string res, Proto.Vector3 offset , string fromBone, string toBone, float speed)
	{
       
        var root = new GameObject(res);
        var missile = root.AddComponent<UBattleMissileView> (); //NO
        missile.fromBone = fromBone;
        missile.toBone = toBone;
        missile.speed = speed;
        missile.offset = offset.ToUV3();
        missile.res = res;
        missile.SetPrecpetion(this);
        missile.releaserIndex = releaseIndex;
        return missile;
	}

    public IParticlePlayer CreateParticlePlayer(IMagicReleaser releaser, ParticleLayout layout)
    {
        var viewRoot = new GameObject(layout.path);
        var view = viewRoot.AddComponent<UParticlePlayer>();
        view.Path = layout.path;
        var viewRelease = releaser as UMagicReleaserView;
        var viewTarget = viewRelease.CharacterTarget as UCharacterView;
        var characterView = viewRelease.CharacterReleaser as UCharacterView;
        var form = layout.fromTarget == TargetType.Releaser ? characterView : viewTarget;
        var bone = form.GetBoneByName(layout.fromBoneName);
        if (layout.Bind)
        {
            if (bone) viewRoot.transform.SetParent(bone, false);
            viewRoot.transform.RestRTS();
        }
        else
        {
            viewRoot.transform.SetParent(transform, false);
            viewRoot.transform.RestRTS();
            if (bone) viewRoot.transform.position = bone.position;
        }

        viewRoot.transform.rotation =( form as IBattleCharacter).Rotation*  Quaternion.Euler(layout.rotation.ToUV3());
        viewRoot.transform.position += viewRoot.transform.rotation * layout.offet.ToUV3();
        viewRoot.transform.localScale =  UnityEngine.Vector3 .one* layout.localsize;
        return view;
    }
        
    ITimeSimulater IBattlePerception.GetTimeSimulater ()
	{
		return this;
	}

    TreeNode IBattlePerception.GetAITree (string pathTree)
	{
        return LoadTreeXml(pathTree);
    }

    IBattlePerception IViewBase.Create(ITimeSimulater simulater)
    {
        return this;
    }

    TreeNode ITreeLoader.Load(string path)
    {
        return LoadTreeXml(path);
    }
    IBattleItem IBattlePerception.CreateDropItem(Proto.Vector3 pos, PlayerItem item, int teamIndex, int groupId)
    {
        var config = ExcelToJSONConfigManager.Current.GetConfigByID<ItemData>(item.ItemID);
        var root = new GameObject(config.Name);
        root.transform.SetParent(this.transform);
        root.transform.RestRTS();
        root.transform.position = pos.ToUV3();
        var bi = root.AddComponent<UBattleItem>();
        bi.SetInfo(item, teamIndex, groupId);
        bi.SetPrecpetion(this);
        return bi;
    }


    #endregion

}
