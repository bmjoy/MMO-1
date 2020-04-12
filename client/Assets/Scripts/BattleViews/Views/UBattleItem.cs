using System;
using GameLogic.Game.Elements;
using Google.Protobuf;
using Proto;
using UnityEngine;
using Vector3 = UnityEngine.Vector3;
using UGameTools;
using EConfig;

public class UBattleItem : UElementView, IBattleItem
{
    public PlayerItem Item { private set; get; }
    public int TeamIndex { private set; get; }
    public int GroupIndex { private set; get; }

    int IBattleItem.TeamIndex { get { return TeamIndex; } }

    int IBattleItem.GroupIndex { get { return GroupIndex; } }

    public ItemData config;

    private void Start()
    {
#if !UNITY_SERVER
        ResourcesManager.S.LoadModel(config,(res)=> {
            var go = Instantiate(res, transform);
            go.transform.RestRTS();
        });
#endif
    }

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
        if (ch) ch.OnItemTrigger?.Invoke(this);
    }

    private int id = -1;

    private void Update()
    {
#if !UNITY_SERVER
        if (Vector3.Distance(this.transform.position, ThridPersionCameraContollor.Current.LookPos) < 10)
        {
            var owner = false;
            owner = IsOwner(PerView.OwnerIndex);
            id = UUITipDrawer.S.DrawItemName(id, config.Name, owner, this.transform.position +Vector3.up *.8f, ThridPersionCameraContollor.Current.CurrenCamera);
        }
#endif
    }

    public override IMessage ToInitNotify()
    {
        return new Notify_Drop
        {
            Index = Index,
            GroupIndex = GroupIndex,
            Item = Item,
            Pos = this.transform.position.ToPVer3(),
            TeamIndex = TeamIndex
        };
    }

    internal void SetInfo(PlayerItem item, int teamIndex, int groupId)
    {
        this.GroupIndex = groupId;
        this.TeamIndex = teamIndex;
        this.Item = item;
        config = ExcelConfig.ExcelToJSONConfigManager.Current.GetConfigByID<ItemData>(item.ItemID);
    }

    void IBattleItem.ChangeGroupIndex(int groupIndex)
    {
#if UNITY_SERVER || UNITY_EDITOR
        CreateNotify(new Notify_BattleItemChangeGroupIndex { GroupIndex = groupIndex, Index = Index });
#endif
        GroupIndex = groupIndex;
    }

    public bool IsOwner(int index)
    {
        return GroupIndex == index || GroupIndex < 0;
    }
}
