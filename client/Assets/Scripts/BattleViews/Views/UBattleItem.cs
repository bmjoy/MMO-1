using System;
using GameLogic.Game.Elements;
using Google.Protobuf;
using Proto;

public class UBattleItem : UElementView, IBattleItem
{
    public Proto.PlayerItem Item;

    public string AccountId;
    public int TeamIndex;
    public int GroupIndex;

    public override IMessage ToInitNotify()
    {
        //Notify_Drop

        return new Notify_Drop { };
    }
}
