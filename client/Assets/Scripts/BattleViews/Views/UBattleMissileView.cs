using GameLogic.Game.Elements;
using UGameTools;
using EngineCore.Simulater;
using Google.Protobuf;
using GameLogic;
using Proto;
using UnityEngine;
using Vector3 = UnityEngine.Vector3;

public class UBattleMissileView : UElementView ,IBattleMissile
{
	
    Transform IBattleMissile.Transform 
	{
		get
		{
            return transform;
		}
	}


    private void Start()
    {
        UElementView releaser = PerView.GetViewByIndex(releaserIndex);
        var viewRelease = releaser as UMagicReleaserView;
        var viewTarget = viewRelease.CharacterTarget as UCharacterView;
        var characterView = viewRelease.CharacterReleaser as UCharacterView;
        var trans = characterView.transform;
        transform.position = characterView.GetBoneByName(fromBone).position + trans.rotation * offset;
        transform.rotation = Quaternion.identity;
        var path = GetComponent<MissileFollowPath>();
        if (path) path.SetTarget(viewTarget.GetBoneByName(toBone), speed);
    }

    public string res;
    public float speed;

    public string fromBone;
    public string toBone;
    public int releaserIndex;
    public Vector3 offset;

    public override IMessage ToInitNotify()
    {
        //var missile = this.Element as BattleMissile;
        var createNotify = new Notify_CreateMissile
        {
            Index = Index,
            ResourcesPath = res,
            Speed = speed,
            ReleaserIndex = releaserIndex,
            FromBone = fromBone,
            ToBone = toBone,
            Offset = offset.ToPVer3()
        };
        return createNotify;
    }

}
