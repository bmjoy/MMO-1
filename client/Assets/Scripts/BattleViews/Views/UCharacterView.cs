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
    private const string Die_Motion = "Die";
    private Animator CharacterAnimator;
	private bool IsStop = true;
    public int hpBar = -1;
    private float showHpBarTime =0;
    private int max;
    private int cur;

    private readonly Dictionary<int, HeroMagicData> MagicCds = new Dictionary<int, HeroMagicData>();

    // Update is called once per frame
    void Update()
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
            hpBar = UUITipDrawer.Singleton.DrawUUITipHpBar(hpBar,
                    cur, max,
                    UUIManager.Singleton.OffsetInUI(GetBoneByName(TopBone).position)
                );
        }
        lookQuaternion = Quaternion.Lerp(lookQuaternion, targetLookQuaternion, Time.deltaTime * this.damping);
        Character.transform.localRotation = lookQuaternion;
        if (!Agent) return;
        if (MoveForward.HasValue)
        {
            Agent.isStopped = true;
            Agent.Move(MoveForward.Value * Agent.speed * Time.deltaTime);
            targetLookQuaternion = Quaternion.LookRotation(MoveForward.Value);
            CharacterAnimator.SetFloat(SpeedStr, Agent.speed);
        }
        else
        {
            CharacterAnimator.SetFloat(SpeedStr, Agent.velocity.magnitude);
        }

        if (lockRotationTime < Time.time && !IsStop && Agent.velocity.magnitude > 0)
        {
            targetLookQuaternion = Quaternion.LookRotation(Agent.velocity, Vector3.up);
        }
    }


    void Awake()
    {
        Agent = this.gameObject.AddComponent<UnityEngine.AI.NavMeshAgent>();
        Agent.updateRotation = false;
        Agent.updatePosition = true;
        Agent.acceleration = 20;
        Agent.radius = 0.1f;
        Agent.obstacleAvoidanceType = UnityEngine.AI.ObstacleAvoidanceType.NoObstacleAvoidance;
        Agent.speed = Speed;
    }

    public int ConfigID { internal set; get; }
    public int TeamId { get; internal set; }
    public int Level { get; internal set; }
    public float Speed { get { return Agent.speed; } set { Agent.speed = value; } }
    public string Name { get; internal set; }
    private UnityEngine.AI.NavMeshAgent Agent;
    public string lastMotion =string.Empty;
    private float last = 0;
	private readonly Dictionary<string ,Transform > bones = new Dictionary<string, Transform>();
    private Vector3? targetPos;
    private bool IsDead = false;
    public float damping  = 5;
    public Quaternion targetLookQuaternion;
    public Quaternion lookQuaternion = Quaternion.identity;
    public int hp = -1;

    public Transform GetBoneByName(string name)
    {
        if (bones.TryGetValue(name, out Transform bone))
        {
            return bone;
        }
        return transform;
    }

    public GameObject Character{ private set; get; }

    public void SetCharacter(GameObject character)
    {
        this.Character = character;

        var collider = this.Character.GetComponent<CapsuleCollider> ();
        var gameTop = new GameObject ("__Top");
        gameTop.transform.SetParent(this.transform);
        gameTop.transform.localPosition =  new Vector3(0,collider.height,0);
        bones.Add ("Top", gameTop.transform);

        var bottom = new GameObject ("__Bottom");
        bottom.transform.SetParent( this.transform,false);
        bottom.transform.localPosition =  new Vector3(0,0,0);
        bones.Add ("Bottom", bottom.transform);

        var body = new GameObject ("__Body");
        body.transform.SetParent( this.transform,false);
        body.transform.localPosition =  new Vector3(0,collider.height/2,0);
        bones.Add ("Body", body.transform);

        CharacterAnimator= Character. GetComponent<Animator> ();
        Agent.radius = collider.radius;
    }

    private float lockRotationTime = -1f;
    void LookAt(Transform target)
    {
        if (target == null) return;
        var look = target.position - this.transform.position;
        if (look.magnitude <= 0.01f) return;
        look.y = 0;
        lockRotationTime = Time.time + 0.3f;
        var qu = Quaternion.LookRotation(look, Vector3.up);
        lookQuaternion = targetLookQuaternion = qu;
        
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
        this.lookQuaternion = Quaternion.LookRotation(f);
        CreateNotify(new Notify_CharacterSetForword
        {
            Forward = forward,
            Index = Index
        });
    }

    Transform IBattleCharacter.Transform
    {
        get
        {
            return transform;
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
        TryToSetPosition(pos.ToUV3());
        CreateNotify(new Notify_CharacterSetPosition { Index = Index, Position = pos });
    }



    void IBattleCharacter.LookAtTarget(int target)
    {
        CreateNotify(new Notify_LookAtCharacter { Index = Index, Target = target });
        var v = PerView.GetViewByIndex(target);
        if (v == null) return;
        this.LookAt(v.transform);
    }


    void IBattleCharacter.PropertyChange(HeroPropertyType type, int finalValue)
    {
        CreateNotify(new Notify_PropertyValue { Index = Index, Type = type, FinallyValue = finalValue });
    }

    void IBattleCharacter.SetAlpha(float alpha)
    {
        CreateNotify(new Notify_CharacterAlpha { Index = Index, Alpha = alpha });
       //do nothing
    }

    void IBattleCharacter.PlayMotion(string motion)
    {
        CreateNotify(new Notify_LayoutPlayMotion { Index = Index, Motion = motion });
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


    void IBattleCharacter.MoveTo(Proto.Vector3 position, Proto.Vector3 target)
    {
        CreateNotify(new Notify_CharacterMoveTo { Index = Index, Position = position, Target = target });
        if (!Agent || !Agent.enabled)
            return;
        IsStop = false;

        TryToSetPosition(position.ToUV3());
        this.Agent.isStopped = false;
        if (UnityEngine.AI.NavMesh.SamplePosition(target.ToUV3(),
            out UnityEngine.AI.NavMeshHit hit, 10000, this.Agent.areaMask))
        {
            targetPos = hit.position;
        }
        else
        {
            return;
        }


        if (Vector3.Distance(targetPos.Value, this.transform.position) < 0.2f)
        {
            StopMove();
            return;
        }
        this.Agent.SetDestination(targetPos.Value);
    }

    bool IBattleCharacter.IsMoving
    {
        get
        {
            return targetPos.HasValue && Vector3.Distance(targetPos.Value, this.transform.position) > 0.2f;
        }
    }

    void IBattleCharacter.StopMove(Proto.Vector3 pos)
    {
        if (Vector3.Distance(transform.localPosition, pos.ToUV3()) > 0.1f)
        {
            transform.position = pos.ToUV3();
        }
        StopMove();
        CreateNotify(new Notify_CharacterStopMove { Position = pos, Index = Index });
	}

    void IBattleCharacter.Death ()
	{
        var view = this as IBattleCharacter;
		view.PlayMotion (Die_Motion);
        StopMove();
        showHpBarTime = -1;
		if(Agent)  Agent.enabled = false;
		IsDead = true;
		MoveDown.BeginMove (this.Character, 1, 1, 5);
        CreateNotify(new Notify_CharacterDeath { Index = Index });
	}


    void IBattleCharacter.SetSpeed(float speed)
    {
        this.Speed = speed;
        CreateNotify(new Notify_CharacterSpeed { Index = Index, Speed = speed });
    }

    void IBattleCharacter.SetPriorityMove (float priorityMove)
    {
        Agent.avoidancePriority = (int)priorityMove;
        CreateNotify(new Notify_CharacterPriorityMove { Index = Index, PriorityMove = priorityMove });
    }

    void IBattleCharacter.SetScale(float scale)
    {
        this.gameObject.transform.localScale = Vector3.one * scale;
        CreateNotify(new Notify_CharacterSetScale { Index = Index, Scale = scale });
    }


    void IBattleCharacter.ShowHPChange(int hp,int cur,int max)
    {
        CreateNotify(new Notify_HPChange { Index = Index, Cur = cur, Hp = hp, Max = max });
        if (IsDead)  return;
        this.cur = cur;
        this.max = max;
        if (hp < 0)
        {
            _tips.Add(new HpChangeTip
                { 
                    id = -1, hp = hp, hideTime = Time.time + 3, pos = GetBoneByName(TopBone).position
                });
        }
        showHpBarTime = Time.time + 3;
       
    }

    void IBattleCharacter.ShowMPChange(int mp, int cur, int maxMP)
    {
        CreateNotify(new Notify_MPChange { Cur = cur, Index = Index, Max = max, Mp = mp });
        //throw new System.NotImplementedException();
    }

    void IBattleCharacter.AttachMagic(int magicID, float cdCompletedTime)
    {
        CreateNotify(new Notify_CharacterAttachMagic { Index = Index,
            MagicId = magicID, CompletedTime = cdCompletedTime });
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

    #endregion

    public override IMessage ToInitNotify()
    {
        
        var createNotity = new Notify_CreateBattleCharacter
        {
            Index =Index,
            AccountUuid = this.AccoundUuid,
            ConfigID = ConfigID,
            Position = transform.position.ToPVer3(),
            Forward = transform.forward.ToPVer3(),
            Level = Level,
            Name = Name,
            TeamIndex = TeamId,
            Speed = Speed
        };

        foreach (var i in MagicCds)
            createNotity.MagicId.Add(i.Key);

        return createNotity;
    }

    private Vector3? MoveForward;

    void IBattleCharacter.SetMoveDir(Proto.Vector3 pos, Proto.Vector3 forward)
    {
        TryToSetPosition(pos.ToUV3());
        MoveForward = forward.ToUV3().normalized;
        CreateNotify(new Notify_CharacterMoveForward { Forward = forward, Index = Index, Position = pos });
    }
}
