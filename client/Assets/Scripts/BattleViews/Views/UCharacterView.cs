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

    //public Dictionary<Proto.HeroPropertyType,int> Properties = new Dictionary<HeroPropertyType, int>();

	private  const string SpeedStr ="Speed";
	private Animator CharacterAnimator;
	private bool IsStop = true;
    private const string TopBone ="Top";

    public int hpBar = -1;
    private float showHpBarTime =0;
    private int max;
    private int cur;

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
        if (CharacterAnimator != null) CharacterAnimator.SetFloat(SpeedStr, Agent.velocity.magnitude);

        if (!Agent) return;
        {
            if (MoveForward.HasValue)
            {
                Agent.Move(MoveForward.Value * Agent.speed * Time.deltaTime);
                lookQuaternion = Quaternion.LookRotation(MoveForward.Value);
            }

            if (lockRotationTime < Time.time && !IsStop && Agent.velocity.magnitude > 0)
            {
                targetLookQuaternion = Quaternion.LookRotation(Agent.velocity, Vector3.up);
            }
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
    public float Speed { get; internal set; }
    public string Name { get; internal set; }

    private UnityEngine.AI.NavMeshAgent Agent;
    public string lastMotion =string.Empty;
    private float last = 0;
	private readonly Dictionary<string ,Transform > bones = new Dictionary<string, Transform>();
    private Vector3? targetPos;

    public int hp;
    private bool IsDead = false;

    public float damping  = 5;

    public Quaternion targetLookQuaternion;

    public Quaternion lookQuaternion = Quaternion.identity;

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



    public Dictionary<int, HeroMagicData> MagicCds = new Dictionary<int, HeroMagicData>();

    public float GetCdTime(int magicKey)
    {
        if (MagicCds.TryGetValue(magicKey, out HeroMagicData cd))
            return cd.CDTime;
        return 0;
    }

    #region impl

    void IBattleCharacter.SetForward(Proto.Vector3 forward)
    {
        var f = forward.ToUV3();
        this.lookQuaternion = Quaternion.LookRotation(f);
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
        if (Vector3.Distance(pos, transform.localPosition) > 0.1f)
        {
            this.Agent.Warp(pos);
        }
        return this.transform.localPosition;
    }

    void IBattleCharacter.SetPosition(Proto.Vector3 pos)
    {
        TryToSetPosition(pos.ToUV3());
    }



    void IBattleCharacter.LookAtTarget(int target)
    {
        var v = PerView.GetViewByIndex(target);
        if (v == null) return;
        this.LookAt(v.transform);
    }


    void IBattleCharacter.PropertyChange(HeroPropertyType type, int finalValue)
    {

    }

    void IBattleCharacter.SetAlpha(float alpha)
    {
        
       //do nothing
    }

    void IBattleCharacter.PlayMotion (string motion)
	{
		
		var an = CharacterAnimator;
		if (an == null)
			return;
        
		if (motion == "Hit") {
			if (last + 0.3f > Time.time)
				return;
		}
		if (IsDead)
			return;
        
        if (!string.IsNullOrEmpty(lastMotion)&& lastMotion != motion) {
			an.ResetTrigger (lastMotion);
		}
		lastMotion = motion;
		last = Time.time;
		an.SetTrigger (motion);

	}


    void IBattleCharacter.MoveTo(Proto.Vector3 position, Proto.Vector3 target)
    {
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
            transform.localPosition = pos.ToUV3();
        }
        StopMove();
	}

    void IBattleCharacter.Death ()
	{
        var view = this as IBattleCharacter;
		view.PlayMotion ("Die");
        StopMove();
        showHpBarTime = -1;
		if(Agent)
		 Agent.enabled = false;
		IsDead = true;
		MoveDown.BeginMove (this.Character, 1, 1, 5);
	}


    void IBattleCharacter.SetSpeed(float speed)
    {
        this.Agent.speed = speed;
    }

    void IBattleCharacter.SetPriorityMove (float priorityMove)
    {
        Agent.avoidancePriority = (int)priorityMove;
    }

    void IBattleCharacter.SetScale(float scale)
    {
        this.gameObject.transform.localScale = Vector3.one * scale;
    }


    void IBattleCharacter.ShowHPChange(int hp,int cur,int max)
    {
        if (IsDead)
            return;

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
        //throw new System.NotImplementedException();
    }

    void IBattleCharacter.AttachMagic(int magicID, float cdCompletedTime)
    {
        if (MagicCds.ContainsKey(magicID))
        {
            MagicCds[magicID].CDTime = cdCompletedTime;
        }
        else
        {
            MagicCds.Add(magicID, new HeroMagicData{ MagicID = magicID, CDTime = cdCompletedTime});
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

        return createNotity;
    }

    private Vector3? MoveForward;

    void IBattleCharacter.SetMoveDir(Proto.Vector3 pos, Proto.Vector3 forward)
    {
        TryToSetPosition(pos.ToUV3());

        MoveForward = forward.ToUV3().normalized;
    }
}
