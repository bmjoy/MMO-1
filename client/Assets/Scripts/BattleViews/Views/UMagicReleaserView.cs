using UnityEngine;
using System.Collections;
using GameLogic.Game.Elements;
using Google.Protobuf;
using Proto;
using EngineCore.Simulater;

public class UMagicReleaserView :UElementView,IMagicReleaser
{

    public void SetCharacter(IBattleCharacter releaser, IBattleCharacter target)
    {
        CharacterTarget = target;
        CharacterReleaser = releaser;
    }

	public IBattleCharacter CharacterTarget{private set; get;}
	public IBattleCharacter CharacterReleaser{private set; get; }

    public string Key { get; internal set; }

    public override IMessage ToInitNotify()
    {
        var createNotify = new Notify_CreateReleaser
        {
            Index = Index,
            ReleaserIndex = CharacterTarget.Index,
            TargetIndex = CharacterReleaser.Index,
            MagicKey = Key
        };
        return createNotify;
    }
}
