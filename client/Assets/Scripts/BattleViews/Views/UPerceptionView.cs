﻿using UnityEngine;
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
        var magics = ResourcesManager.S.LoadAll<TextAsset>("Magics");
        foreach (var i in magics)
        {
            try
            {
                var m = XmlParser.DeSerialize<MagicData>(i.text);
                _magicData.Add(i.name, m);
            }
            catch (Exception ex)
            {
                Debug.LogError($"File : {i.name}");
                Debug.LogException(ex);
            }
        }
        magicCount = _magicData.Count;
        var timeLines = ResourcesManager.Singleton.LoadAll<TextAsset>("Layouts");
        foreach (var i in timeLines)
        {
            var line = XmlParser.DeSerialize<TimeLine>(i.text);
            _timeLines.Add("Layouts/" + i.name + ".xml", line);
        }
        timeLineCount = _timeLines.Count;

#if !UNITY_SERVER
        GPUBillboardBuffer.Instance.Init();
        GPUBillboardBuffer.Instance.SetupBillboard(1000);
        GPUBillboardBuffer.Instance.SetDisappear(1);
        GPUBillboardBuffer.Instance.SetScaleParams(0f, 0.5f, 0.5f, 1f, 1f);

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
        GPUBillboardBuffer.Instance.DisplayNumberRandom($"{hp}", new Vector2(.2f, .2f), pos, Color.green, true, param);
#endif
    }


    public UElementView GetViewByIndex(int releaseIndex)
    {
        if (AttachElements.TryGetValue(releaseIndex, out UElementView vi)) return vi;
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

    public void DeAttachView(UElementView battleElement)
    {
        AttachElements.Remove(battleElement.Index);
    }

    public void AttachView(UElementView battleElement)
    {
        AttachElements.Add(battleElement.Index, battleElement);
        //AddNotify(battleElement.ToInitNotify());
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
        var lineAsset = ResourcesManager.Singleton.LoadText(path);
        if (string.IsNullOrEmpty(lineAsset))
            return null;

        var line = XmlParser.DeSerialize<TimeLine> (lineAsset);
        if (UseCache) 
        {
            _timeLines.Add (path, line);
        } 
        return line;
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

    bool IBattlePerception.ProcessDamage(int owner, int target, int damage, bool isMissed,int crtmult)
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
        if (GetViewByIndex(target) is UCharacterView ch)
        {
            if (ch != null)
            {
                var bone = ch.GetBoneByName(UCharacterView.TopBone);
                if (bone)
                {
                    var pos = bone.transform.position;
                    GPUBillboardBuffer.Instance.
                        DisplayNumberRandom((isMissed ? "MISS" : $"{damage}"),new Vector2(.2f, .2f) * crtmult, pos, Color.red,true, param);
                }
            }
        }
#endif
        return true;
    }
        
    TimeLine IBattlePerception.GetTimeLineByPath (string path)
	{
		if (UseCache)
		{
            if (_timeLines.TryGetValue(path, out TimeLine line))
            {
                return line;
            }
            Debug.LogError ("No found timeline by path:" + path);
		} 
		return TryToLoad (path);
	}
        
    MagicData IBattlePerception.GetMagicByKey (string key)
	{
        if (_magicData.TryGetValue(key, out MagicData magic))
        {
            return magic;
        }
        Debug.LogError ("No found magic by key:"+key);
		return null;
	}

    bool IBattlePerception.ExistMagicKey (string key)
	{
		return _magicData.ContainsKey (key);
	}

    IBattleCharacter IBattlePerception.CreateBattleCharacterView(string account_id,
        int config, int teamId, Proto.Vector3 pos, Proto.Vector3 forward,
        int level, string name, float speed,int hp, int hpMax,int mp,int mpMax ,IList<HeroMagicData> cds)
    {
        var data = ExcelToJSONConfigManager.Current.GetConfigByID<CharacterData>(config);
        var character = ResourcesManager.Singleton.LoadResourcesWithExName<GameObject>(data.ResourcesPath);
        var qu = Quaternion.Euler(forward.X, forward.Y, forward.Z);
        var ins = Instantiate(character) as GameObject;
        var root = new GameObject(data.ResourcesPath);
        root.transform.SetParent(this.transform, false);
        root.transform.position = pos.ToUV3();
        root.transform.rotation = Quaternion.identity;
        var body = new GameObject("__VIEW__");
        body.transform.SetParent(root.transform, false);
        body.transform.RestRTS();
        ins.transform.SetParent(body.transform);
        ins.transform.RestRTS();

        ins.name = "VIEW";

        var view = root.AddComponent<UCharacterView>();
        view.SetPrecpetion(this);
        view.LookQuaternion = view.targetLookQuaternion = qu;
        view.TeamId = teamId;
        view.Level = level;
        view.Speed = speed;
        view.ConfigID = config;
        view.AccoundUuid = account_id;
        view.Name = name;
        view.SetHpMp(hp, hpMax,mp, mpMax);

        if (cds != null) { foreach (var i in cds) view.AddMagicCd(i.MagicID, i.CDTime, i.MType); }
        view.SetCharacter(body, ins);
        return view;
    }

    IMagicReleaser IBattlePerception.CreateReleaserView(int releaser, int target, string magicKey, Proto.Vector3 targetPos)
    {
        var obj = new GameObject("MagicReleaser");
        obj.transform.SetParent(this.transform, false);
        var view = obj.AddComponent<UMagicReleaserView>();
        if (AttachElements.TryGetValue(releaser, out UElementView r))
        {
            if (AttachElements.TryGetValue(target, out UElementView t))
            {
                view.SetCharacter(r as IBattleCharacter, t as IBattleCharacter);
            }
        }
        view.Key = magicKey;
        view.SetPrecpetion(this);
        return view;
    }
        
    IBattleMissile IBattlePerception.CreateMissile(int releaseIndex, string res, Proto.Vector3 offset , string fromBone, string toBone, float speed)
	{
        var obj = ResourcesManager.Singleton.LoadResourcesWithExName<GameObject> (res);
        GameObject ins;
        if (obj == null)
        {
            ins = new GameObject("Missile");
        }
        else
        {
            ins = Instantiate(obj);
        }
        ins.transform.SetParent(this.transform, false);
		var missile = ins.AddComponent<UBattleMissileView> (); //NO
        missile.fromBone = fromBone;
        missile.toBone = toBone;
        missile.speed = speed;
        missile.offset = offset.ToUV3();
        missile.res = res;
        missile.SetPrecpetion(this);

        missile.releaserIndex = releaseIndex;

        return missile;
	}

    IParticlePlayer IBattlePerception.CreateParticlePlayer(int releaser,
        string path,int fromTarget,bool bind ,string fromBone, string toBone, int destoryType, float destoryTime)
    {
#if UNITY_SERVER||UNITY_EDITOR
        AddNotify(new Notify_LayoutPlayParticle
        {
            Bind = bind,
            DestoryTime = destoryTime,
            DestoryType = destoryType,
            FromBoneName = fromBone ?? string.Empty,
            FromTarget = fromTarget,
            Path = path,
            ReleaseIndex = releaser,
            ToBoneName = toBone??string.Empty
        });
#endif

        var viewRoot = new GameObject(path);
        var view = viewRoot.AddComponent<UParticlePlayer>();

#if !UNITY_SERVER
        var obj = ResourcesManager.Singleton.LoadResourcesWithExName<GameObject> (path);
        GameObject ins;
        if (obj == null)
        {
            return null;
        } else
        {
            ins =Instantiate (obj);
            ins.transform.SetParent(viewRoot.transform);
            ins.transform.RestRTS();
        }
        var viewRelease = GetViewByIndex(releaser) as UMagicReleaserView;
        var viewTarget = viewRelease.CharacterTarget as UCharacterView;
        var characterView = viewRelease.CharacterReleaser as UCharacterView;
      
        var form = (TargetType)fromTarget == TargetType.Releaser ? characterView : viewTarget;
        var bone = form.GetBoneByName(fromBone);
        if (bind)
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

        switch ((ParticleDestoryType)destoryType)
        {
            case  ParticleDestoryType.Time:
                Destroy(ins, destoryTime);
                break;
            case ParticleDestoryType.Normal:
                Destroy(ins, 3);
                break;
            case ParticleDestoryType.LayoutTimeOut:
                Destroy(ins, 1);
                break;
        }
#endif
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

#if !UNITY_SERVER
       
        var res = ResourcesManager.S.LoadModel(config);
        var go =Instantiate(res,root.transform);
        go.transform.RestRTS();
#endif
        var bi = root.AddComponent<UBattleItem>();
        bi.SetInfo(item, teamIndex, groupId);
        bi.SetPrecpetion(this);
        return bi;
    }
    #endregion

}
