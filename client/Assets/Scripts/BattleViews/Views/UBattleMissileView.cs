using GameLogic.Game.Elements;
using UGameTools;
using EngineCore.Simulater;
using Google.Protobuf;
using GameLogic;
using Proto;
using UnityEngine;
using Vector3 = UnityEngine.Vector3;
using System;
using System.Collections;

public class UBattleMissileView : UElementView ,IBattleMissile
{
	
    Transform IBattleMissile.Transform 
	{
		get
		{
            return transform;
		}
	}


    private IEnumerator Start()
    {
        var viewRelease = PerView.GetViewByIndex<UMagicReleaserView>(releaserIndex);
        var viewTarget = viewRelease.CharacterTarget as UCharacterView;
        var characterView = viewRelease.CharacterReleaser as UCharacterView;
        var rotation = (characterView as IBattleCharacter).Rotation;
        transform.position = characterView.GetBoneByName(fromBone).position +  rotation* offset;
        transform.rotation = Quaternion.identity;
       
        yield return ResourcesManager.Singleton.LoadResourcesWithExName<GameObject>(res, (obj) =>
        {
            if (obj == null) return;
            var ins = Instantiate(obj);
            ins.transform.SetParent(this.transform, false);
            ins.transform.RestRTS();
            var path = ins.GetComponent<MissileFollowPath>();
            if (path) path.SetTarget(viewTarget.GetBoneByName(toBone), speed);
        });
    }

    public string res;
    public float speed;

    public string fromBone;
    public string toBone;
    public int releaserIndex;
    public Vector3 offset;

    public override IMessage ToInitNotify()
    {
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
