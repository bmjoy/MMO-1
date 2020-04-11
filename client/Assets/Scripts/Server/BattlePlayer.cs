using System;
using System.Collections.Generic;
using System.Linq;
using EConfig;
using ExcelConfig;
using GameLogic.Game.Elements;
using Proto;
using Server;
using UnityEngine;
using XNet.Libs.Net;

public class BattlePlayer
{

    #region Property

    private readonly DHero Hero;
    public  BattlePackage Package { private set; get; }
    public BattleCharacter HeroCharacter { set; get; }

    public int Gold
    {
        get { return baseGold + DiffGold; }
        private set
        {
            DiffGold = value - baseGold;        
        }
    }

    #endregion


    public Client Client{set;get;}

    public DHero GetHero() { return Hero; }

    public string AccountId {  private set; get; }

    public GameServerInfo GateServer { set; get; }


    private readonly int baseGold = 0;

    public BattlePlayer(string account, PlayerPackage package, DHero hero, int gold, Client client, GameServerInfo info)
    {
        Package = new BattlePackage( package);
        Hero = hero;
        baseGold = gold;
        this.AccountId = account;
        this.Client = client;
        GateServer = info;
    }


    public bool SubGold(int gold)
    {
        if (gold <= 0) return false;
        if (Gold - gold < 0) return false;
        Gold -= gold;
        Dirty = true;
        return true;
    }

    public bool AddGold(int gold)
    {
        if (gold <= 0) return false;
        Gold += gold;
        Dirty = true;
        return true;
    }

    public Notify_PlayerJoinState GetNotifyPackage()
    {
        var notify = new Notify_PlayerJoinState()
        {
            AccountUuid = AccountId,
            Gold = Gold,
            Package = GetCompletedPackage(),
            Hero = Hero
        };
        return notify;

    }

    private PlayerPackage GetCompletedPackage()
    {
        var result = new PlayerPackage();
        foreach (var i in Package.Items)
        {
            result.Items.Add(i.Key ,i.Value.Item);
            result.MaxSize = Package.MaxSize;
        }
        return result;
    }

    public int CurrentSize { get { return Package.Items.Count; } }

    public bool AddDrop(PlayerItem item)
    {
        var config = ExcelToJSONConfigManager.Current.GetConfigByID<ItemData>(item.ItemID);
        if (config.MaxStackNum == 1)
        {
            item.GUID =CreateUUID();
            if (CurrentSize >= Package.MaxSize) return false;
            Package.Items.Add(item.GUID, new BattlePlayerItem(item,true));
        }
        else 
        {
            foreach (var i in Package.Items)
            {
                if (i.Value.Item.Locked) continue;
                if (i.Value.Item.ItemID == item.ItemID)
                {
                    if (i.Value.Item.Num == config.MaxStackNum) continue;
                    int maxNum = config.MaxStackNum - i.Value.Item.Num;
                    if (maxNum >= item.Num)
                    {
                        i.Value.Item.Num += item.Num;
                        item.Num = 0;
                        i.Value.SetDrity();
                        break;
                    }
                    else
                    {
                        i.Value.Item.Num += maxNum;
                        item.Num -= maxNum;
                        i.Value.SetDrity();
                    }
                }
            }
            if (CurrentSize >= Package.MaxSize) return true;
            var needSize = item.Num / Mathf.Max(1, config.MaxStackNum);
            if (needSize + CurrentSize >= Package.MaxSize) return true;
            while (item.Num > 0)
            {
                var num = Mathf.Min(config.MaxStackNum, item.Num);
                item.Num -= num;
                var playitem = new PlayerItem { GUID = CreateUUID(), ItemID = item.ItemID, Level = item.Level, Num = num };
                Package.Items.Add(playitem.GUID, new BattlePlayerItem(playitem, true));
            }
        }
        Dirty = true;
        return true;
    }

    internal int GetItemCount(int itemId,bool ignoreLocked = true)
    {
        int have = 0;
        foreach (var i in Package.Items)
        {
            if (i.Value.Item.Locked && ignoreLocked) continue;
            if (i.Value.Item.ItemID == itemId) have += i.Value.Item.Num;
        }
        return have;
    }

    public bool ConsumeItem(int item, int num = 1)
    {
        int have = GetItemCount(item);
        if (have < num) return false;
        HashSet<string> needRemoves = new HashSet<string>();
        foreach (var i in Package.Items)
        {
            if (i.Value.Item.ItemID != item) continue;
            if (i.Value.Item.Locked) continue;
            var left = num - i.Value.Item.Num;
            if (left < 0)
            {
                i.Value.Item.Num -= num;
                i.Value.SetDrity();
                num = 0;
                break;
            }
            i.Value.SetDrity();
            needRemoves.Add(i.Key);
            num = left;
        }
        foreach (var i in needRemoves)
        {
            Package.RemoveItem(i);
        }
        Dirty = true;
        return true;
    }

    public bool Dirty { get; private set; } = false;
    
    public int DiffGold { get; private set; }

    internal PlayerItem GetEquipByGuid(string gUID)
    {
        if (Package.Items.TryGetValue(gUID, out BattlePlayerItem item)) return item.Item;   
        return null;
    }

    public  string CreateUUID()
    {
        var sererid = BattleSimulater.S.ServerID;
        return $"{sererid}-{DateTime.Now.Ticks}{Guid.NewGuid().ToString()}";
    }

    private bool AddExp(int totalExp, int level, out int exLevel, out int exExp)
    {
        exLevel = level;
        exExp = totalExp;
        var herolevel = ExcelToJSONConfigManager.Current.FirstConfig<CharacterLevelUpData>(t => t.Level == level + 1);
        if (herolevel == null) return false;

        if (exExp >= herolevel.NeedExprices)
        {
            exLevel += 1;
            exExp -= herolevel.NeedExprices;
            if (exExp > 0)
            {
                AddExp(exExp, exLevel, out exLevel, out exExp);
            }
        }
        return true;
    }


    public int AddExp(int exp,out int oldLevel, out int newLevel)
    {
        
        oldLevel = newLevel = Hero.Level;
        if (exp <= 0) return  Hero.Exprices;
        if (AddExp(exp+Hero.Exprices, Hero.Level, out int level, out int exLimit))
        {
            Hero.Level = level;
            Hero.Exprices = exLimit;
            newLevel = Hero.Level;
        }
        Dirty = true;
        return Hero.Exprices;
    }

   
}

