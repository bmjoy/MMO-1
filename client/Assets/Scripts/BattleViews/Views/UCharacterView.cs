using UnityEngine;
using GameLogic.Game.Elements;
using System.Collections.Generic;
using GameLogic;
using Quaternion = UnityEngine.Quaternion;
using Proto;
using Vector3 = UnityEngine.Vector3;
using UGameTools;
using Google.Protobuf;
using System.Linq;
using UnityEngine.AI;
using static Proto.Notify_CharacterAttachMagic.Types;



[
    BoneName("Top", "__Top"),
    BoneName("Bottom", "__Bottom"),
    BoneName("Body", "__Body"),
//BoneName("HandLeft","bn_handleft"),
//BoneName("HandRight","bn_handright")
]
public class UCharacterView : UElementView, IBattleCharacter
{
    public enum MoveCategory
    {
        NONE,
        Destination,
        Forward,
        Push
    }

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
    public MoveCategory MCategory = MoveCategory.NONE;
    private string NameInfo;

    private Vector3 pushSpeed= Vector3.zero;//speed
    private float pushLeftTime = -1;

    void Update()
    {
#if !UNITY_SERVER
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
                                NameInfo,
                                UUIManager.S.OffsetInUI(GetBoneByName(BottomBone).position)
                            );
                }

            }
        }
#endif
        LookQuaternion = Quaternion.Lerp(LookQuaternion, targetLookQuaternion, Time.deltaTime * this.damping);

        if (!Agent) return;
        switch (MCategory)
        {

            case MoveCategory.Forward:
                {
                    Agent.isStopped = true;
                    var v = MoveForward.Value * Agent.speed;
                    Agent.Move(v * Time.deltaTime);
                    targetLookQuaternion = Quaternion.LookRotation(MoveForward.Value);
                    PlaySpeed(v.magnitude);
                }
                break;
            case MoveCategory.Push:
                if (pushLeftTime > 0)
                {
                    pushLeftTime -= Time.deltaTime;
                    Agent.Move(pushSpeed * Time.deltaTime);
                    targetLookQuaternion = Quaternion.LookRotation(pushSpeed);
                    PlaySpeed(pushSpeed.magnitude);
                }
                else
                {
                    EndPush();
                    MCategory = MoveCategory.NONE;
                }
                break;
            case MoveCategory.Destination:
            default:
                {
                    PlaySpeed(Agent.velocity.magnitude);
                }
                break;
        }

        if (lockRotationTime < Time.time && !IsStop && Agent.velocity.magnitude > 0)
        {
            targetLookQuaternion = Quaternion.LookRotation(Agent.velocity, Vector3.up);
        }

        if (hideTime < Time.time)
        {
            if (range && range.activeSelf)
            {
                range.SetActive(false);
            }
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
        Agent.obstacleAvoidanceType =ObstacleAvoidanceType.NoObstacleAvoidance;
        Agent.speed = Speed;
        var r =this.gameObject.AddComponent<Rigidbody>();
        r.isKinematic = true;
        r.useGravity = false;
    }


    private void OnTriggerEnter(Collider other)
    {
        var view = other.GetComponent<UCharacterView>();
        if (view == null) return;
        if (this.GElement is BattleCharacter o)
        {
            if (o == null) return;
            if (view.GElement is BattleCharacter ot)
            {
                if (ot == null) return;
                o.HitOther(ot);
            }
        }
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
    private NavMeshAgent Agent;
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

    private GameObject ViewRoot;

    private GameObject range;
    private float hideTime = 0f;


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
        var c = this.gameObject.AddComponent<CapsuleCollider>();
        c.radius = collider.radius;
        c.height = collider.height;
        c.center = collider.center;
        c.direction = collider.direction;
        c.isTrigger = true;

#if UNITY_SERVER
        Destroy(character);
#else
        CharacterAnimator = character.GetComponent<Animator>();
#endif

        string color = TeamId ==1?"{0}" :"<color=red>{0}</color>";

        NameInfo = string.Format(color, $"{Name} Lv:{Level}");

    }

    private float lockRotationTime = -1f;

    private void LookAt(Transform target)
    {
        if (target == null) return;
        var look = target.position - this.transform.position;
        if (look.magnitude <= 0.01f) return;
        look.y = 0;
        lockRotationTime = Time.time + 0.3f;
        LookQuaternion = targetLookQuaternion = Quaternion.LookRotation(look, Vector3.up); ;
    }

    private void StopMove()
    {
        MCategory = MoveCategory.NONE;
        pushSpeed = Vector3.zero;
        pushLeftTime = -1;
        MoveForward = null;
        IsStop = true;
        if (!Agent ||!Agent.enabled) return;
        Agent.velocity = Vector3.zero;
        Agent.ResetPath();
        Agent.isStopped = true;
        targetPos = null;
    }

    public bool ShowName { set; get; } = true;

    public bool TryGetMagicData(int magicID, out HeroMagicData data)
    {
        if (MagicCds.TryGetValue(magicID, out data)) return true;
        return false;
    }

    public IList<HeroMagicData> Magics { get { return MagicCds.Values.ToList() ; } }

    void IBattleCharacter.SetForward(Proto.Vector3 forward)
    {
        if (!this) return;
        var f = forward.ToUV3();
        this.LookQuaternion = targetLookQuaternion = Quaternion.LookRotation(f);
#if UNITY_SERVER || UNITY_EDITOR
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
        if (!this) return ;
        this.Agent.Warp(pos.ToUV3());
#if UNITY_SERVER || UNITY_EDITOR
        CreateNotify(new Notify_CharacterSetPosition { Index = Index, Position = pos });
#endif
    }

    void IBattleCharacter.LookAtTarget(int target)
    {
        if (!this) return;
#if UNITY_SERVER || UNITY_EDITOR
        CreateNotify(new Notify_LookAtCharacter { Index = Index, Target = target });
#endif
        var v = PerView.GetViewByIndex(target);
        if (!v) return;
        LookAt(v.transform);
    }

    void IBattleCharacter.PropertyChange(HeroPropertyType type, int finalValue)
    {
        if (!this) return;
#if UNITY_SERVER || UNITY_EDITOR
        CreateNotify(new Notify_PropertyValue { Index = Index, Type = type, FinallyValue = finalValue });
#endif
    }

    void IBattleCharacter.SetAlpha(float alpha)
    {
        if (!this) return;
#if UNITY_SERVER || UNITY_EDITOR
        CreateNotify(new Notify_CharacterAlpha { Index = Index, Alpha = alpha });
#endif
       //do nothing
    }

    void IBattleCharacter.PlayMotion(string motion)
    {
        if (!this) return;
#if UNITY_SERVER || UNITY_EDITOR
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

        if (!this) return false;
#if UNITY_SERVER || UNITY_EDITOR
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
        MCategory = MoveCategory.Destination;
        return true;
    }

    bool IBattleCharacter.IsMoving
    {
        get
        {
            if (MoveForward.HasValue) return true;
            if (!this) return false;
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
        if (!this) return;
        if (Vector3.Distance(transform.localPosition, pos.ToUV3()) > 0.5f)
        {
            transform.position = pos.ToUV3();
        }
        StopMove();
#if UNITY_SERVER || UNITY_EDITOR
        CreateNotify(new Notify_CharacterStopMove { Position = pos, Index = Index });
#endif
	}

    void IBattleCharacter.Death ()
	{
        if (!this) return;
        var view = this as IBattleCharacter;
		view.PlayMotion (Die_Motion);
        StopMove();
        showHpBarTime = -1;
		if(Agent)  Agent.enabled = false;
		IsDead = true;
		//MoveDown.BeginMove (ViewRoot, 1, 1, 5);
#if UNITY_SERVER || UNITY_EDITOR
        CreateNotify(new Notify_CharacterDeath { Index = Index });
#endif
	}

    void IBattleCharacter.SetSpeed(float speed)
    {
        if (!this) return;
        this.Speed = speed;
#if UNITY_SERVER || UNITY_EDITOR
        CreateNotify(new Notify_CharacterSpeed { Index = Index, Speed = speed });
#endif
    }

    void IBattleCharacter.SetPriorityMove (float priorityMove)
    {
        if (!this) return;
        Agent.avoidancePriority = (int)priorityMove;
#if UNITY_SERVER || UNITY_EDITOR
        CreateNotify(new Notify_CharacterPriorityMove { Index = Index, PriorityMove = priorityMove });
#endif
    }

    void IBattleCharacter.SetScale(float scale)
    {
        if (!this) return;
        this.gameObject.transform.localScale = Vector3.one * scale;
#if UNITY_SERVER || UNITY_EDITOR
        CreateNotify(new Notify_CharacterSetScale { Index = Index, Scale = scale });
#endif
    }

    void IBattleCharacter.ShowHPChange(int hp,int cur,int max)
    {
        if (!this) return;
#if UNITY_SERVER || UNITY_EDITOR
        CreateNotify(new Notify_HPChange { Index = Index, Cur = cur, Hp = hp, Max = max });
#endif
        if (IsDead)  return;
        this.cur = cur;
        this.max = max;

#if !UNITY_SERVER
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
#endif
    }

    void IBattleCharacter.ShowMPChange(int mp, int cur, int maxMP)
    {
        if (!this) return;
#if UNITY_SERVER || UNITY_EDITOR
        CreateNotify(new Notify_MPChange { Cur = cur, Index = Index, Max = max, Mp = mp });
#endif
    }

    void IBattleCharacter.AttachMagic(MagicType type, int magicID, float cdCompletedTime)
    {
        if (!this) return;
#if UNITY_SERVER || UNITY_EDITOR
        CreateNotify(new Notify_CharacterAttachMagic
        {
            Index = Index,
            MagicId = magicID,
            CompletedTime = cdCompletedTime,
            MType = type
        });
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
        if (!this) return;
        TryToSetPosition(pos.ToUV3());
        MoveForward = forward.ToUV3();
        MCategory = MoveCategory.Forward;
#if UNITY_SERVER || UNITY_EDITOR
        CreateNotify(new Notify_CharacterMoveForward { Forward = forward, Index = Index, Position = pos });
#endif
    }


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
        return createNotity;
    }

    private int LockValue = 0;

    void IBattleCharacter.SetLock(int lockValue)
    {
        if (!this) return;
        LockValue = lockValue;
#if UNITY_SERVER || UNITY_EDITOR
        CreateNotify(new Notify_CharacterLock { Index = Index, Lock = lockValue });
#endif
    }

    private Vector3? MoveForward;

    public bool IsLock(ActionLockType type)
    {
        return (LockValue &(1 << (int)type )) > 0;
    }

    public void ShowRange(float r)
    {
        if (range == null)
        {
            range = Instantiate( Resources.Load<GameObject>("Range"), this.GetBoneByName(BottomBone));
           // range.transform.SetParent(this.GetBoneByName(BottomBone),false);
            range.transform.RestRTS();
        }

        range.SetActive(  true);
        hideTime = Time.time + .5f;
        range.transform.localScale = Vector3.one * r;
        
    }

    void IBattleCharacter.Push(Proto.Vector3 length, Proto.Vector3 speed)
    {
        if (!this) return;
#if UNITY_SERVER || UNITY_EDITOR
        CreateNotify(new Notify_CharacterPush { Index = Index,  Speed =speed, Length = length});
#endif
        pushSpeed = speed.ToUV3();
        pushLeftTime = length.ToUV3().magnitude / pushSpeed.magnitude;
        MCategory = MoveCategory.Push;
    }

    private void EndPush()
    {
        if (GElement is BattleCharacter c)
        {
            c.EndPush();
        }
    }
}
