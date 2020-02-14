using UnityEngine;
using GameLogic.Game.Elements;
using System.Collections.Generic;
using GameLogic;
using Quaternion = UnityEngine.Quaternion;
using Proto;
using Vector3 = UnityEngine.Vector3;
using UVector3 = UnityEngine.Vector3;
using UGameTools;
using EngineCore.Simulater;
using Google.Protobuf;
using System;
using System.Linq;
using UnityEngine.AI;
using GameLogic.Game;

[
	BoneName("Top","__Top"),
	BoneName("Bottom","__Bottom"),
	BoneName("Body","__Body"),
	//BoneName("HandLeft","bn_handleft"),
	//BoneName("HandRight","bn_handright")
]
public class UCharacterView : UElementView,IBattleCharacter
{

    private class HpChangeTip
    {
        public int id = -1;
        public float hideTime;
        public int hp;
        public Vector3 pos;
    }

    private readonly List<HpChangeTip> _tips = new List<HpChangeTip>();

    public string AccoundUuid = string.Empty;
	private  const string SpeedStr ="Speed";
    private const string TopBone = "Top";
    private const string BodyBone = "Body";
    private const string BottomBone = "Bottom";
    private const string Die_Motion = "Die";
    private Animator CharacterAnimator;
	private bool IsStop = true;
    private int hpBar = -1;
    private int nameBar=-1;
    private float showHpBarTime =0;
    private int max;
    private int cur;
    private readonly Dictionary<int, HeroMagicData> MagicCds = new Dictionary<int, HeroMagicData>();

    void Update()
    {

        if (Vector3.Distance(this.transform.position, ThridPersionCameraContollor.Current.LookPos) < 10)
        {
            if (_tips.Count > 0)
            {
                _tips.RemoveAll(t => t.hideTime < Time.time);
                foreach (var i in _tips)
                {
                    i.id = UUITipDrawer.Singleton.DrawHPNumber(i.id,
                        i.hp,
                        UUIManager.Singleton.OffsetInUI(i.pos));
                }
            }

            if (showHpBarTime > Time.time)
            {
                hpBar = UUITipDrawer.S.DrawUUITipHpBar(hpBar,
                        cur, max,
                        UUIManager.S.OffsetInUI(GetBoneByName(TopBone).position)
                    );
            }


            if (ShowName && !IsDead && ThridPersionCameraContollor.Current)
            {

                if (ThridPersionCameraContollor.Current.InView(this.transform.position))
                {
                    nameBar = UUITipDrawer.S.DrawUUITipNameBar(nameBar,
                                this.Name,
                                UUIManager.S.OffsetInUI(GetBoneByName(BottomBone).position)
                            );
                }

            }
        }


        LookQuaternion = Quaternion.Lerp(LookQuaternion, targetLookQuaternion, Time.deltaTime * this.damping);
        
        if (!Agent) return;
        if (MoveForward.HasValue)
        {
            Agent.isStopped = true;
            var v = MoveForward.Value * Agent.speed;
            Agent.Move(v * Time.deltaTime);
            targetLookQuaternion = Quaternion.LookRotation(MoveForward.Value);
            PlaySpeed(v.magnitude);
        }
        else
        {
            PlaySpeed(Agent.velocity.magnitude);
        }

        if (lockRotationTime < Time.time && !IsStop && Agent.velocity.magnitude > 0)
        {
            targetLookQuaternion = Quaternion.LookRotation(Agent.velocity, Vector3.up);
        }
    }
    private void PlaySpeed(float speed)
    {
        if (CharacterAnimator == null) return;
        CharacterAnimator.SetFloat(SpeedStr, speed);
    }
    void Awake()
    {
        Agent = this.gameObject.AddComponent<NavMeshAgent>();
        Agent.updateRotation = false;
        Agent.updatePosition = true;
        Agent.acceleration = 20;
        Agent.radius = 0.1f;
        Agent.baseOffset = 0;//-0.15f;
        Agent.obstacleAvoidanceType =ObstacleAvoidanceType.MedQualityObstacleAvoidance;
        Agent.speed = Speed;
    }
    public int ConfigID { internal set; get; }
    public int TeamId { get; internal set; }
    public int Level { get; internal set; }
    public float Speed
    {
        get
        {
            if (!Agent) return 0;
            return Agent.speed;
        }
        set
        {
            if (!Agent) return; Agent.speed = value;
        }
    }
    public string Name { get; internal set; }
    private UnityEngine.AI.NavMeshAgent Agent;
    public string lastMotion =string.Empty;
    private float last = 0;
	private readonly Dictionary<string ,Transform > bones = new Dictionary<string, Transform>();
    private Vector3? targetPos;
    private bool IsDead = false;
    public float damping  = 5;
    public Quaternion targetLookQuaternion;
    public Quaternion LookQuaternion
    {
        set
        {
            //Debug.Log($"look:{value} euler:{value.eulerAngles}");
            if (ViewRoot) ViewRoot.transform.rotation = value;
        }
        get
        {
            if (ViewRoot) return ViewRoot.transform.rotation;
            return Quaternion.identity;
        }
    }
    public int hp = -1;

    public Transform GetBoneByName(string name)
    {
        if (!transform) return null;
        if (bones.TryGetValue(name, out Transform bone))
        {
            return bone;
        }
        return transform;
    }

    //public GameObject Character{ private set; get; }

    private GameObject ViewRoot;

    public void SetCharacter(GameObject root, GameObject character)
    {
        ViewRoot = root;
        //this.Character = character;

        var collider = character.GetComponent<CapsuleCollider>();

        var gameTop = new GameObject("__Top");
        gameTop.transform.SetParent(this.transform);
        gameTop.transform.localPosition = new Vector3(0, collider.height, 0);
        bones.Add(TopBone, gameTop.transform);

        var bottom = new GameObject("__Bottom");
        bottom.transform.SetParent(this.transform, false);
        bottom.transform.localPosition = new Vector3(0, 0, 0);
        bones.Add(BottomBone, bottom.transform);

        var body = new GameObject("__Body");
        body.transform.SetParent(this.transform, false);
        body.transform.localPosition = new Vector3(0, collider.height / 2, 0);
        bones.Add(BodyBone, body.transform);    
        Agent.radius = collider.radius;
        Agent.height = collider.height;

#if UNITY_SERVER
        Destroy(character);
#else
        CharacterAnimator = character.GetComponent<Animator>();
#endif

    }

    private float lockRotationTime = -1f;

    private void LookAt(Transform target)
    {
        if (target == null) return;
        var look = target.position - this.transform.position;
        if (look.magnitude <= 0.01f) return;
        look.y = 0;
        lockRotationTime = Time.time + 0.3f;
        var qu = Quaternion.LookRotation(look, Vector3.up);
        LookQuaternion = targetLookQuaternion = qu;
    }

    private void StopMove()
    {
        MoveForward = null;
        IsStop = true;
        if (!Agent ||!Agent.enabled) return;
        Agent.velocity = Vector3.zero;
        Agent.ResetPath();
        Agent.isStopped = true;// ();
        targetPos = null;
    }

    public bool ShowName { set; get; } = true;

    public float GetCdTime(int magicKey)
    {
        if (TryGetMagicData(magicKey, out HeroMagicData cd)) return cd.CDTime;
        return 0;
    }

    public bool TryGetMagicData(int magicID, out HeroMagicData data)
    {
        if (MagicCds.TryGetValue(magicID, out data)) return true;
        return false;
    }

    public IList<HeroMagicData> Magics { get { return MagicCds.Values.ToList() ; } }

#region impl

    void IBattleCharacter.SetForward(Proto.Vector3 forward)
    {
        var f = forward.ToUV3();
        this.LookQuaternion = Quaternion.LookRotation(f);
#if UNITY_SERVER||UNITY_EDITOR
        CreateNotify(new Notify_CharacterSetForword
        {
            Forward = forward,
            Index = Index
        });
#endif
    }

    Transform IBattleCharacter.Transform
    {
        get
        {
            if (ViewRoot)
                return ViewRoot.transform;
            return null;
        }
    }

    Transform IBattleCharacter.RootTransform
    {
        get {
            if (this) return transform;
            return null;
        }
    }

    private Vector3 TryToSetPosition(Vector3 pos)
    {
        if (Vector3.Distance(pos, transform.position) > 3f)
        {
            this.Agent.Warp(pos);
        }
        return this.transform.position;
    }

    void IBattleCharacter.SetPosition(Proto.Vector3 pos)
    {
        this.Agent.Warp(pos.ToUV3());
#if UNITY_SERVER||UNITY_EDITOR
        CreateNotify(new Notify_CharacterSetPosition { Index = Index, Position = pos });
#endif
    }

    void IBattleCharacter.LookAtTarget(int target)
    {
#if UNITY_SERVER||UNITY_EDITOR
        CreateNotify(new Notify_LookAtCharacter { Index = Index, Target = target });
#endif
        var v = PerView.GetViewByIndex(target);
        if (v == null) return;
        this.LookAt(v.transform);
    }

    void IBattleCharacter.PropertyChange(HeroPropertyType type, int finalValue)
    {
#if UNITY_SERVER||UNITY_EDITOR
        CreateNotify(new Notify_PropertyValue { Index = Index, Type = type, FinallyValue = finalValue });
#endif
    }

    void IBattleCharacter.SetAlpha(float alpha)
    {
#if UNITY_SERVER||UNITY_EDITOR
        CreateNotify(new Notify_CharacterAlpha { Index = Index, Alpha = alpha });
#endif
       //do nothing
    }

    void IBattleCharacter.PlayMotion(string motion)
    {
#if UNITY_SERVER||UNITY_EDITOR
        CreateNotify(new Notify_LayoutPlayMotion { Index = Index, Motion = motion });
#endif
        var an = CharacterAnimator;
        if (an == null) return;

        if (motion == "Hit") { if (last + 0.3f > Time.time) return; }
        if (IsDead) return;

        if (!string.IsNullOrEmpty(lastMotion) && lastMotion != motion)
        {
            an.ResetTrigger(lastMotion);
        }
        lastMotion = motion;
        last = Time.time;
        an.SetTrigger(motion);
    }

    bool IBattleCharacter.MoveTo(Proto.Vector3 position, Proto.Vector3 target, float stopDis)
    {

#if UNITY_SERVER||UNITY_EDITOR
        CreateNotify(new Notify_CharacterMoveTo
        {
            Index = Index,
            Position = position,
            Target = target,
            StopDis = stopDis
        });
#endif

        if (!Agent || !Agent.enabled)
            return false;
        IsStop = false;

        TryToSetPosition(position.ToUV3());
        this.Agent.isStopped = false;
        if (!NavMesh.SamplePosition(target.ToUV3(), out NavMeshHit hit, 10000, this.Agent.areaMask)) return false;

        targetPos = hit.position;

        if (Vector3.Distance(targetPos.Value, this.transform.position) < 0.2f + stopDis)
        {
            StopMove();
            return false;
        }
        this.Agent.stoppingDistance = stopDis;
        this.Agent.SetDestination(targetPos.Value);
        return true;
    }

    bool IBattleCharacter.IsMoving
    {
        get
        {
            if (MoveForward.HasValue) return true;
            if (!this.transform) return false;
            return targetPos.HasValue && Vector3.Distance(targetPos.Value, this.transform.position) > 0.2f;
        }
    }

    Quaternion IBattleCharacter.Rotation {
        get
        {
            if (ViewRoot)
                return ViewRoot.transform.rotation;
            return Quaternion.identity;
        }
    }

    void IBattleCharacter.StopMove(Proto.Vector3 pos)
    {
        if (!transform) return;
        if (Vector3.Distance(transform.localPosition, pos.ToUV3()) > 0.5f)
        {
            transform.position = pos.ToUV3();
        }
        StopMove();
#if UNITY_SERVER||UNITY_EDITOR
        CreateNotify(new Notify_CharacterStopMove { Position = pos, Index = Index });
#endif
	}

    void IBattleCharacter.Death ()
	{
        var view = this as IBattleCharacter;
		view.PlayMotion (Die_Motion);
        StopMove();
        showHpBarTime = -1;
		if(Agent)  Agent.enabled = false;
		IsDead = true;
		MoveDown.BeginMove (ViewRoot, 1, 1, 5);
#if UNITY_SERVER||UNITY_EDITOR
        CreateNotify(new Notify_CharacterDeath { Index = Index });
#endif
	}

    void IBattleCharacter.SetSpeed(float speed)
    {
        this.Speed = speed;
#if UNITY_SERVER || UNITY_EDITOR
        CreateNotify(new Notify_CharacterSpeed { Index = Index, Speed = speed });
#endif
    }

    void IBattleCharacter.SetPriorityMove (float priorityMove)
    {
        if (!Agent) return;
        Agent.avoidancePriority = (int)priorityMove;
#if UNITY_SERVER|| UNITY_EDITOR
        CreateNotify(new Notify_CharacterPriorityMove { Index = Index, PriorityMove = priorityMove });
#endif
    }

    void IBattleCharacter.SetScale(float scale)
    {
        this.gameObject.transform.localScale = Vector3.one * scale;
#if UNITY_SERVER|| UNITY_EDITOR
        CreateNotify(new Notify_CharacterSetScale { Index = Index, Scale = scale });
#endif
    }

    void IBattleCharacter.ShowHPChange(int hp,int cur,int max)
    {
#if UNITY_SERVER||UNITY_EDITOR
        CreateNotify(new Notify_HPChange { Index = Index, Cur = cur, Hp = hp, Max = max });
#endif
        if (IsDead)  return;
        this.cur = cur;
        this.max = max;
        if (hp < 0)
        {
            if (Vector3.Distance(this.transform.position, ThridPersionCameraContollor.Current.LookPos) < 10)
            {
                _tips.Add(new HpChangeTip
                {
                    id = -1,
                    hp = hp,
                    hideTime = Time.time + 3,
                    pos = GetBoneByName(TopBone).position
                });
            }
        }
        showHpBarTime = Time.time + 3;
       
    }

    void IBattleCharacter.ShowMPChange(int mp, int cur, int maxMP)
    {
#if UNITY_SERVER||UNITY_EDITOR
        CreateNotify(new Notify_MPChange { Cur = cur, Index = Index, Max = max, Mp = mp });
#endif
        //throw new System.NotImplementedException();
    }

    void IBattleCharacter.AttachMagic(int magicID, float cdCompletedTime)
    {
#if UNITY_SERVER || UNITY_EDITOR
        CreateNotify(new Notify_CharacterAttachMagic { Index = Index,
            MagicId = magicID, CompletedTime = cdCompletedTime });
#endif
        AddMagicCd(magicID, cdCompletedTime);
    }

    public void AddMagicCd(int id, float cdTime)
    {
        if (MagicCds.ContainsKey(id))
        {
            MagicCds[id].CDTime = cdTime;
        }
        else
        {
            MagicCds.Add(id, new HeroMagicData { MagicID = id, CDTime = cdTime });
        }
    }

    void IBattleCharacter.SetMoveDir(Proto.Vector3 pos, Proto.Vector3 forward)
    {
        TryToSetPosition(pos.ToUV3());
        MoveForward = forward.ToUV3();
#if UNITY_SERVER|| UNITY_EDITOR
        CreateNotify(new Notify_CharacterMoveForward { Forward = forward, Index = Index, Position = pos });
#endif
    }

#endregion

    public override IMessage ToInitNotify()
    {
        
        var createNotity = new Notify_CreateBattleCharacter
        {
            Index =Index,
            AccountUuid = this.AccoundUuid,
            ConfigID = ConfigID,
            Position = transform.position.ToPVer3(),
            Forward =  LookQuaternion.eulerAngles.ToPVer3(),
            Level = Level,
            Name = Name,
            TeamIndex = TeamId,
            Speed = Speed
        };

        foreach (var i in MagicCds) createNotity.MagicId.Add(i.Key);

        return createNotity;
    }

    private int LockValue = 0;

    void IBattleCharacter.SetLock(int lockValue)
    {
        LockValue = lockValue;
#if UNITY_SERVER|| UNITY_EDITOR
        CreateNotify(new Notify_CharacterLock { Index = Index, Lock = lockValue });
#endif
    }

    private Vector3? MoveForward;

    public bool IsLock(ActionLockType type)
    {
        return (LockValue &(1 << (int)type )) > 0;
    }
}
