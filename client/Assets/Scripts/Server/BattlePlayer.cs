using System;
using System.Collections.Generic;
using System.Linq;
using EConfig;
using ExcelConfig;
using GameLogic.Game.Elements;
using Proto;
using UnityEngine;
using XNet.Libs.Net;

public class BattlePlayer
{

    #region Property

    private readonly DHero Hero;
    private readonly PlayerPackage Package;
    public BattleCharacter HeroCharacter { set; get; }
    public int Gold { get; private set; }

    #endregion


    public Client Client{set;get;}

    public DHero GetHero() { return Hero; }

    public string AccountId {  private set; get; }

    public GameServerInfo GateServer { set; get; }
   
    public BattlePlayer(string account, PlayerPackage package, DHero hero, Client client, GameServerInfo info)
    {
        Package = package;
        Hero = hero;
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
            result.Items.Add(i.Key ,i.Value);
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
            Package.Items.Add(item.GUID, item);
        }
        else 
        {
            foreach (var i in Package.Items)
            {
                if (i.Value.Locked) continue;
                if (i.Value.ItemID == item.ItemID)
                {
                    if (i.Value.Num == config.MaxStackNum) continue;
                    int maxNum = config.MaxStackNum - i.Value.Num;
                    if (maxNum >= item.Num)
                    {
                        i.Value.Num += item.Num;
                        item.Num = 0;
                        break;
                    }
                    else
                    {
                        i.Value.Num += maxNum;
                        item.Num -= maxNum;
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
                Package.Items.Add(playitem.GUID, playitem);
            }
        }
        Dirty = true;
        return true;
    }

    public bool ConsumeItem(int item, int num)
    {
        int have = 0;
        foreach (var i in Package.Items)
        {
            if (i.Value.Locked) continue;
            if (i.Value.ItemID == item) have += i.Value.Num;

        }
        if (have < num) return false;
        HashSet<string> needRemoves = new HashSet<string>();
        foreach (var i in Package.Items)
        {
            if (i.Value.ItemID != item) continue;
            if (i.Value.Locked) continue;

            var left = num-i.Value.Num;
            if (left < 0)
            {
                i.Value.Num -= num;
                num = 0;
                break;
            }
            needRemoves.Add(i.Key);
            num = left;
        }
        foreach (var i in needRemoves)  Package.Items.Remove(i);
        Dirty = true;
        return true;
    }

    public bool Dirty { get; private set; } = false;
    public IDictionary<string,PlayerItem> Items { get { return Package.Items; } }

    internal PlayerItem GetEquipByGuid(string gUID)
    {
        if (Package.Items.TryGetValue(gUID, out PlayerItem item)) return item;   
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
            exExp = exExp - herolevel.NeedExprices;
            if (exExp > 0)
            {
                AddExp(exExp, exLevel, out exLevel, out exExp);
            }
        }
        return true;
    }


    public bool AddExp(int exp,out int oldLevel, out int newLevel)
    {
        
        oldLevel = newLevel = Hero.Level;
        if (exp <= 0) return false;
        if (AddExp(exp+Hero.Exprices, Hero.Level, out int level, out int exLimit))
        {
            Hero.Level = level;
            Hero.Exprices = exLimit;
            newLevel = Hero.Level;
        }
        Dirty = true;
        return true;
    }
}

