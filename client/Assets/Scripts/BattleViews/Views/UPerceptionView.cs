using UnityEngine;
using GameLogic.Game.Perceptions;
using Layout.LayoutElements;
using GameLogic.Game.Elements;
using GameLogic.Game.LayoutLogics;
using Vector3 = UnityEngine.Vector3;
using System.Collections.Generic;
using EngineCore.Simulater;
using Layout.AITree;
using Quaternion = UnityEngine.Quaternion;
using Layout;
using GameLogic;
using UVector3 = UnityEngine.Vector3;
using Google.Protobuf;
using Proto;
using System;
using ExcelConfig;
using EConfig;

public class UPerceptionView : MonoBehaviour, IBattlePerception , ITimeSimulater, IViewBase
{
    public UGameScene UScene;
    public bool UseCache = true;

	void Awake()
	{
        UScene = FindObjectOfType<UGameScene>();
		_magicData = new Dictionary<string, MagicData> ();
		_timeLines = new Dictionary<string, TimeLine> ();
		var  magics = ResourcesManager.Singleton.LoadAll<TextAsset> ("Magics");
		foreach (var i in magics) 
        {           
			var m = XmlParser.DeSerialize<Layout.MagicData> (i.text);
			_magicData.Add (m.key, m);
		}
		magicCount = _magicData.Count;
		var timeLines = ResourcesManager.Singleton.LoadAll<TextAsset> ("Layouts");
		foreach (var i in timeLines) 
		{
			var line = XmlParser.DeSerialize<TimeLine> (i.text);
			_timeLines.Add ("Layouts/" + i.name+".xml",line);
		}
		timeLineCount = _timeLines.Count;

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
        AddNotify(battleElement.ToInitNotify());
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
#if UNITY_SERVER//UNITY_SERVER
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

    #region IBattlePerception implementation

    bool IBattlePerception.ProcessDamage(int owner, int target, int damage, bool isMissed)
    {
        AddNotify(new Notify_DamageResult
        {
            Index = owner,
            TargetIndex = target,
            Damage = damage,
            IsMissed = isMissed
        });
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
        int config, int teamId, Proto.Vector3 pos, Proto.Vector3 forward, int level, string name, float speed)
	{
        var data = ExcelToJSONConfigManager.Current.GetConfigByID<CharacterData>(config);
		var character = ResourcesManager.Singleton.LoadResourcesWithExName<GameObject> (data.ResourcesPath);
		var qu = Quaternion.Euler (forward.X, forward.Y, forward.Z);
		var ins = Instantiate(character) as GameObject;
        var root = new GameObject(data.ResourcesPath);
        root.transform.SetParent(this.transform, false);
		root.transform.position = pos.ToUV3();
		root.transform.rotation = Quaternion.identity;
		ins.transform.parent = root.transform;
		ins.transform.localPosition = Vector3.zero;
		ins.transform.localRotation =  Quaternion.identity;
		ins.name = "Character";
		var view= root.AddComponent<UCharacterView> ();
        view.SetPrecpetion(this);
		view.targetLookQuaternion = qu;
		view.SetCharacter(ins);
        view.TeamId = teamId;
        view.Level = level;
        view.Speed = speed;
        view.ConfigID = config;
        view.AccoundUuid = account_id;
        view.Name = name;
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
        
    IBattleMissile IBattlePerception.CreateMissile (int releaseIndex, string res, Proto.Vector3 offset , string fromBone, string toBone, float speed)
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
        missile.releaserIndex = releaseIndex;
        missile.res = res;
        missile.SetPrecpetion(this);
		return missile;
	}

    IParticlePlayer IBattlePerception.CreateParticlePlayer(int releaser,
        string path,int fromTarget,bool bind ,string fromBone, string toBone, int destoryType, float destoryTime)
    {
        AddNotify(new Notify_LayoutPlayParticle {
             Bind =bind, DestoryTime = destoryTime,DestoryType = destoryType,
            FromBoneName = fromBone, FromTarget = fromTarget, Path = path, ReleaseIndex = releaser,
            ToBoneName =toBone
        });
        //var res = layout.path;
        var obj = ResourcesManager.Singleton.LoadResourcesWithExName<GameObject> (path);
        GameObject ins;
        if (obj == null)
        {
            return null;
        } else {
            ins =Instantiate (obj);
        }

        var viewRelease = GetViewByIndex(releaser) as UMagicReleaserView;
        var viewTarget = viewRelease.CharacterTarget as UCharacterView;
        var characterView = viewRelease.CharacterReleaser as UCharacterView;
       // var trans = (GTransform)characterView.Transform ;
        var form = (TargetType)fromTarget == Layout.TargetType.Releaser ? characterView : viewTarget;
        if (bind)
        {
            ins.transform.SetParent(form.GetBoneByName(fromBone),false);
        }
        else
        {
            ins.transform.SetParent(this.transform, false);
            ins.transform.position = form.GetBoneByName(toBone).position;
            ins.transform.rotation = Quaternion.identity;
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

        var view = ins.AddComponent<UParticlePlayer>();
       // view.SetPrecpetion(this);
        return view;
    }
        
    ITimeSimulater IBattlePerception.GetTimeSimulater ()
	{
		return this;
	}

    TreeNode IBattlePerception.GetAITree (string pathTree)
	{
		var xml = ResourcesManager.Singleton.LoadText (pathTree);
		var root = XmlParser.DeSerialize<TreeNode> (xml);
		return root;
	}

    IBattlePerception IViewBase.Create(ITimeSimulater simulater)
    {
        return this;
    }

#endregion

}
