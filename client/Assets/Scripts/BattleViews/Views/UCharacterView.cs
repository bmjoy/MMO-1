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
using System;
using EngineCore.Simulater;

[
    BoneName("Top", "__Top"),
    BoneName("Bottom", "__Bottom"),
    BoneName("Body", "__Body"),
//BoneName("HandLeft","bn_handleft"),
//BoneName("HandRight","bn_handright")
]
public class UCharacterView : UElementView, IBattleCharacter
{
    #region Move 
    public class Empty :CharacterMoveState
    {
        public Empty(UCharacterView v) : base(v) { }
        public override bool Tick(GTime gTime)
        {
            return false;
        }
    }

    public class PushMove : CharacterMoveState
    {
        //private readonly UCharacterView view;
        private Vector3 speed;
        private float time;

        public PushMove(UCharacterView view, Vector3 speed, float pushLeftTime):base(view)
        {
            this.speed = speed;
            time = pushLeftTime;
        }

        public override bool Tick(GTime gTime)
        {
            time -= gTime.DeltaTime;
            if (time < 0) return true;
            View.Agent.Move(speed * gTime.DeltaTime);
            return false;
        }
        public override void Exit()
        {
            OnExit?.Invoke();
        }

        public override Vector3 Velocity => speed;

        public Action OnExit;
    }

    public class ForwardMove : CharacterMoveState
    {
        public ForwardMove(UCharacterView view, Vector3 forward):base(view)
        {
            Forward = forward;
        }

        public Vector3? Forward { get; private set; }

        public void ChangeDir(Vector3 dir)
        {
            if (dir.magnitude < 0.001f) { this.Forward = null; return; }
            this.Forward = dir;
        }

        public override bool Tick(GTime gTime)
        {
            if (Forward == null) return true;
            View.Agent.Move(Forward.Value * gTime.DeltaTime * View.Speed);
            return false;
        }

        public override Vector3 Velocity => (Forward ?? Vector3.zero) * View.Speed;
    }

    public class DestinationMove : CharacterMoveState
    {
        public DestinationMove(UCharacterView view) : base(view)
        {
        }

        public Vector3? Target { get; private set; }

        private float stopDis;


        public override bool Tick(GTime gTime)
        {
            return  !Target.HasValue || Vector3.Distance(View.transform.position, Target.Value) < stopDis+ 0.02f;
        }

        private bool MoveTo(Vector3 target)
        {
            if (!View.Agent) return false;
            Target = null;
            View.Agent.isStopped = false;
            NavMeshPath path = new NavMeshPath();
            if (!View.Agent.CalculatePath(target, path)) return false;
            Vector3? wrapTar = path.corners.LastOrDefault();
            if (Vector3.Distance(wrapTar.Value, View.transform.position) < stopDis)
            {
                return  false;
            }
            View.Agent.stoppingDistance = stopDis;
            View.Agent.SetDestination(wrapTar.Value);
            Target = wrapTar;
            return true;
        }

        public Vector3? ChangeTarget(Vector3 target, float dis)
        {
            stopDis = dis; MoveTo(target);
            return Target;
        }

        public override void Exit()
        {
            View.Agent.velocity = Vector3.zero;
            View.Agent.ResetPath();
            View.Agent.isStopped = true;
        }

        public override Vector3 Velocity => View.Agent.velocity;
    }
    #endregion

    public string AccoundUuid = string.Empty;
	public  const string SpeedStr ="Speed";
    public const string TopBone = "Top";
    public const string BodyBone = "Body";
    public const string BottomBone = "Bottom";
    public const string Die_Motion = "Die";
    private Animator CharacterAnimator;
    private int nameBar=-1;
    private float showHpBarTime =0;
    private int maxHp;
    private int curHp;
    private readonly Dictionary<int, HeroMagicData> MagicCds = new Dictionary<int, HeroMagicData>();

    void Update()
    {
#if !UNITY_SERVER
        if (Vector3.Distance(this.transform.position, ThridPersionCameraContollor.Current.LookPos) < 10)
        {
            //player
            if ((showHpBarTime >Time.time || ShowName || TeamId == PerView.OwerTeamIndex ) 
                && !IsDeath 
                && ThridPersionCameraContollor.Current)
            {
                if (ThridPersionCameraContollor.Current.InView(this.transform.position))
                {
                    nameBar = UUITipDrawer.S.DrawUUITipNameBar(nameBar, Name, Level, curHp, maxHp, MP,mpMax,
                        TeamId == PerView.OwerTeamIndex,
                        GetBoneByName(TopBone).position + Vector3.up * .2f,ThridPersionCameraContollor.Current.CurrenCamera);
                }
            }
        }

        if (hideTime < Time.time)
        {
            if (range && range.activeSelf)
            {
                range.SetActive(false);
            }
        }
#endif
        LookQuaternion = Quaternion.Lerp(LookQuaternion, targetLookQuaternion, Time.deltaTime * this.damping);

        if (State ==null || State?.Tick(PerView.GetTime()) == true)
        {
            GoToEmpty();
        }

        PlaySpeed(State?.Velocity.magnitude ?? 0);
       
        if (lockRotationTime < Time.time && State?.Velocity.magnitude > 0.1f)
        {
            targetLookQuaternion = Quaternion.LookRotation(State.Velocity, Vector3.up);
        }
    }

    private CharacterMoveState State;

    private T ChangeState<T>(T s) where T : CharacterMoveState
    {
        State?.Exit();
        State = s;
        State?.Enter();
        return s;
    }

    private void GoToEmpty()
    {
        ChangeState(new Empty(this));
    }

    public float vSpeed = 0;

    private void PlaySpeed(float speed)
    {
        vSpeed = speed;
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
            if (!Agent) return 0;return Agent.speed;
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


        if (curHp == 0) { (this as IBattleCharacter).PlayMotion(Die_Motion); IsDeath = true; };
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

    internal void SetHpMp(int hp, int hpMax,int mp ,int mpMax)
    {
        curHp = hp; maxHp = hpMax;
        MP = mp;  this.mpMax = mpMax;
    }

   

    public bool ShowName { set; get; } = false;
    public int MP { get; private set; }

    public bool IsFullMp { get { return MP == mpMax; } }
    public bool IsFullHp { get { return curHp == maxHp; } }

    public bool TryGetMagicData(int magicID, out HeroMagicData data)
    {
        if (MagicCds.TryGetValue(magicID, out data)) return true;
        return false;
    }

    public bool TryGetMagicByType(MagicType type, out HeroMagicData data)
    {
        data = null;
        foreach (var i in MagicCds)
        {
            if (i.Value.MType == type)
            {
                data = i.Value;
                return true;
            }
        }
        return false;
    }

    public bool TryGetMagicsType(MagicType type, out IList<HeroMagicData> data)
    {
        data =  new List<HeroMagicData>();
        foreach (var i in MagicCds)
        {
            if (i.Value.MType == type)
            {
                data .Add(i.Value);
                
            }
        }
        return  data.Count >0;
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
    }

    void IBattleCharacter.PlayMotion(string motion)
    {
        if (!this) return;
        var an = CharacterAnimator;
        if (an == null) return;
        if (motion == "Hit") { if (last + 0.3f > Time.time) return; }
        if (IsDeath) return;
        if (!string.IsNullOrEmpty(lastMotion) && lastMotion != motion)
        {
            an.ResetTrigger(lastMotion);
        }
        lastMotion = motion;
        last = Time.time;
        an.SetTrigger(motion);
    }

   
    Quaternion IBattleCharacter.Rotation {
        get
        {
            if (ViewRoot)
                return ViewRoot.transform.rotation;
            return Quaternion.identity;
        }
    }

    float IBattleCharacter.Radius
    {
        get
        {
            if (ViewRoot) return Agent.radius;
            return 0;
        }
    }

    public bool IsDeath { get; private set; } = false;

    public Action<UBattleItem> OnItemTrigger;


    void IBattleCharacter.Death ()
	{
        if (!this) return;
        var view = this as IBattleCharacter;
		view.PlayMotion (Die_Motion);
        GoToEmpty();
        showHpBarTime = -1;
		IsDeath = true;
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
        if (IsDeath)  return;
        this.curHp = cur;
        this.maxHp = max;
#if !UNITY_SERVER
        if (hp > 0)  this.PerView.ShowHPCure(this.GetBoneByName(BodyBone).position, hp);
        else showHpBarTime = Time.time + 3;
#endif
    }

    private int mpMax;

    void IBattleCharacter.ShowMPChange(int mp, int cur, int maxMP)
    {
        if (!this) return;
        mpMax = maxMP;
        MP = cur;
#if UNITY_SERVER || UNITY_EDITOR
        CreateNotify(new Notify_MPChange { Cur = cur, Index = Index, Max = maxMP, Mp = mp });
#endif
#if !UNITY_SERVER
        if (mp > 0) this.PerView.ShowMPCure(this.GetBoneByName(BodyBone).position, mp);
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
        AddMagicCd(magicID, cdCompletedTime,type);
    }

    public void AddMagicCd(int id, float cdTime, MagicType type)
    {
        if (MagicCds.ContainsKey(id))
        {
            MagicCds[id].CDTime = cdTime;
        }
        else
        {
            MagicCds.Add(id, new HeroMagicData { MType = type, MagicID = id, CDTime = cdTime });
        }
    }

    public override IMessage ToInitNotify()
    {
        var createNotity = new Notify_CreateBattleCharacter
        {
            Index = Index,
            AccountUuid = this.AccoundUuid,
            ConfigID = ConfigID,
            Position = transform.position.ToPVer3(),
            Forward = LookQuaternion.eulerAngles.ToPVer3(),
            Level = Level,
            Name = Name,
            TeamIndex = TeamId,
            Speed = Speed,
            Hp = curHp,
            MaxHp = maxHp,
            Mp = MP,
            MpMax = mpMax
        };
        foreach (var i in MagicCds)
            createNotity.Cds.Add(new HeroMagicData { MType = i.Value.MType, CDTime = i.Value.CDTime, MagicID = i.Value.MagicID });

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


    public bool IsLock(ActionLockType type)
    {
        return (LockValue &(1 << (int)type )) > 0;
    }

    public void ShowRange(float r)
    {
        if (range == null)
        {
            range = Instantiate(Resources.Load<GameObject>("Range"), this.GetBoneByName(BottomBone));
            range.transform.RestRTS();
        }
        range.SetActive(true);
        hideTime = Time.time + .2f;
        range.transform.localScale = Vector3.one * r;
    }

    void IBattleCharacter.SetMoveDir(Proto.Vector3 pos, Proto.Vector3 forward)
    {
        if (!this) return;
        TryToSetPosition(pos.ToUV3());
        if (State is ForwardMove m) m.ChangeDir(forward.ToUV3());
        else
        {
            if (forward.ToUV3().magnitude > 0.01f)
                ChangeState(new ForwardMove(this, forward.ToUV3()));
            else return;//no notity
        }
#if UNITY_SERVER || UNITY_EDITOR
        CreateNotify(new Notify_CharacterMoveForward { Forward = forward, Index = Index, Position = pos });
#endif
    }

    void IBattleCharacter.Push(Proto.Vector3 length, Proto.Vector3 speed)
    {
        if (!this) return;
#if UNITY_SERVER || UNITY_EDITOR
        CreateNotify(new Notify_CharacterPush { Index = Index, Speed = speed, Length = length });
#endif
        var pushSpeed = speed.ToUV3();
        var pushLeftTime = length.ToUV3().magnitude / pushSpeed.magnitude;
        ChangeState(new PushMove(this, pushSpeed, pushLeftTime))
            .OnExit = () =>
            {
                if (GElement == null) return;
                if (GElement is BattleCharacter c) c.EndPush();
            };
    }

    void IBattleCharacter.StopMove(Proto.Vector3 pos)
    {
        if (!this) return;
        TryToSetPosition(pos.ToUV3());
        GoToEmpty();
#if UNITY_SERVER || UNITY_EDITOR
        CreateNotify(new Notify_CharacterStopMove { Position = pos, Index = Index });
#endif
    }

    bool IBattleCharacter.IsForwardMoving { get { return State is ForwardMove; } }

    public bool IsCanForwardMoving { get { return !(State is PushMove);} }

    bool IBattleCharacter.IsMoving
    {
        get
        {
            return !(State is Empty);
        }
    }

    Vector3? IBattleCharacter.MoveTo(Proto.Vector3 position, Proto.Vector3 target, float stopDis)
    {
        if (!this) return null;
#if UNITY_SERVER || UNITY_EDITOR
        CreateNotify(new Notify_CharacterMoveTo
        {
            Index = Index,
            Position = position,
            Target = target,
            StopDis = stopDis
        });
#endif
        TryToSetPosition(position.ToUV3());
        if (State is DestinationMove m)
        {
            return m.ChangeTarget(target.ToUV3(), stopDis);
        }
        else
        {
            return ChangeState(new DestinationMove(this))
                .ChangeTarget(target.ToUV3(), stopDis);//.Target;
        }
    }

    void IBattleCharacter.Relive()
    {
        IsDeath = false;
        if (this.CharacterAnimator)
        {
            this.CharacterAnimator.SetTrigger("Idle");
            
        }
    }
}
