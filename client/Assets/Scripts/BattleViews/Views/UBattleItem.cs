using System;
using GameLogic.Game.Elements;
using Google.Protobuf;
using Proto;
using UnityEngine;
using Vector3 = UnityEngine.Vector3;
using UGameTools;

public class UBattleItem : UElementView, IBattleItem
{
    public Proto.PlayerItem Item;

    public string AccountId;
    public int TeamIndex;
    public int GroupIndex;

    private void Awake()
    {
        var box = this.gameObject.AddComponent<BoxCollider>();
        box.isTrigger = true;
        box.size = Vector3.one * 1;
        box.center = Vector3.one * .5f;

    }

    private void OnTriggerEnter(Collider other)
    {
        var ch = other.GetComponent<UCharacterView>();
        if (ch) ch.OnItemTrigger?.Invoke();
    }

    public override IMessage ToInitNotify()
    {
        return new Notify_Drop
        {
            GroupIndex = GroupIndex,
            Item = Item,
            Pos = this.transform.position.ToPVer3(),
            TeamIndex = TeamIndex
        };
    }
}
