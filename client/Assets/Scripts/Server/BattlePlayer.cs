﻿using System;
using System.Collections.Generic;
using System.Linq;
using EConfig;
using ExcelConfig;
using GameLogic.Game.Elements;
using Proto;
using XNet.Libs.Net;

public class BattlePlayer
{

    #region Property

    private readonly DHero Hero;
    private readonly PlayerPackage Package;
    private readonly Dictionary<int, int> dropItems = new Dictionary<int, int>();
    private readonly Dictionary<int, int> consumeItems = new Dictionary<int, int>();

    public BattleCharacter HeroCharacter { set; get; }

    public int Gold { get; private set; }

    private int DifGold = 0;

    #endregion


    public Client Client{set;get;}

    public DHero GetHero() { return Hero; }

    public string AccountId {  private set; get; }

    public GameServerInfo GateServer { set; get; }
   
    public BattlePlayer(string account, PlayerPackage package, DHero hero, Client client, GameServerInfo info)
    {
        Package = package;
        CurrentSize = package.Items.Count;
        Hero = hero;
        this.AccountId = account;
        this.Client = client;
        GateServer = info;

    }

    public bool SubGold(int gold)
    {
        if (Gold - (DifGold + gold) < 0) return false;
        DifGold += gold;
        Dirty = true;
        return true;
    }

    public bool AddGold(int gold)
    {
        if (gold <= 0) return false;
        DifGold -= gold;
        Dirty = true;
        return true;
    }

    public Notify_PlayerJoinState GetNotifyPackage()
    {
        var notify = new Notify_PlayerJoinState()
        {
            AccountUuid = AccountId,
            Gold = Gold + DifGold,
            Package = GetCompletedPackage()
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

    public int CurrentSize { private set; get; }

    public bool AddDrop(int item, int num)
    {
        if (CurrentSize >= Package.MaxSize) return false;

        var config = ExcelToJSONConfigManager.Current.GetConfigByID<ItemData>(item);
        if (config == null) return false;

        if (dropItems.ContainsKey(item))
        {
            dropItems[item] += num;
        }
        else
        {
            if (config.Unique == 0)
            {
                CurrentSize += 1;
            }
            else
            {
                bool have = false;
                foreach (var i in Package.Items)
                {
                    if (i.Value.ItemID == item)
                    {
                        have = true;
                    }
                }

                if (!have)
                {
                    CurrentSize += 1;
                }
            }
            dropItems.Add(item, num);
        }
        Dirty = true;
        return true;
    }

    public bool ConsumeItem(int item, int num)
    {
        consumeItems.TryGetValue(item, out int consumeNum);
        //是否足够
        {
            bool enough = false;
            foreach (var i in this.Package.Items)
            {
                if (i.Value.ItemID == item)
                {
                    if (i.Value.Num < consumeNum + num) return false;
                    else
                    {
                        enough = true;
                        break;
                    }
                }
            }
            if (!enough) return false;
        }
        if (consumeItems.ContainsKey(item))
        {
            consumeItems[item] += num;
        }
        else
        {
            consumeItems.Add(item, num);
        }
        Dirty = true;

        return true;
    }

    public List<PlayerItem> DropItems
    {
        get
        {
            return dropItems.Select(t => new PlayerItem { ItemID = t.Key, Num = t.Value }).ToList();
        }
    }

    public List<PlayerItem> ConsumeItems
    {
        get
        {
            return this.consumeItems.Select(t => new PlayerItem { ItemID = t.Key, Num = t.Value }).ToList();

        }
    }

    public bool Dirty { get; private set; } = false;

    internal PlayerItem GetEquipByGuid(string gUID)
    {
        if (Package.Items.TryGetValue(gUID, out PlayerItem item)) return item;
              
        return null;
    }
}

