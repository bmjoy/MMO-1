using System;
using Proto;
using Proto.GateServerTask;
using XNet.Libs.Net;

[TaskHandler(typeof(IGateServerTask))]
public class GateServerTaskHandler : TaskHandler, IGateServerTask
{
    public Task_CoinAndGold CoinAndGold(Task_CoinAndGold req)
    {
        var gata = UApplication.G<GMainGate>();
        gata.Coin = req.Coin;
        gata.Gold = req.Gold;
        UUIManager.S.UpdateUIData();
        return req;
    }

    public Task_G2C_JoinBattle JoinBattle(Task_G2C_JoinBattle req)
    {
        return req;
    }

    public Task_ModifyItem ModifyItem(Task_ModifyItem req)
    {
        var gata = UApplication.G<GMainGate>();
        foreach (var i in req.ModifyItems)
        {
            if (gata.package.Items.TryGetValue(i.GUID, out PlayerItem item))
            {
                gata.package.Items.Remove(i.GUID);
            }

            gata.package.Items.Add(i.GUID, i);
        }
        foreach(var i in req.RemoveItems) gata.package.Items.Remove(i.GUID);

        UUIManager.S.UpdateUIData();

        return req;
    }

    public Task_PackageSize PackageSize(Task_PackageSize req)
    {
        var gata = UApplication.G<GMainGate>();
        gata.package.MaxSize = req.Size;
        UUIManager.S.UpdateUIData();
        return req;
    }

    public Task_G2C_SyncHero SyncHero(Task_G2C_SyncHero req)
    {
        var gata = UApplication.G<GMainGate>();
        gata.hero = req.Hero;
        
        gata.ReCreateHero(req.Hero.HeroID, req.Hero.Name);
        UUIManager.S.UpdateUIData();
        return req;
    }

    public Task_G2C_SyncPackage SyncPackage(Task_G2C_SyncPackage req)
    {
        var gata = UApplication.G<GMainGate>();
        gata.Coin = req.Coin;
        gata.Gold = req.Gold;
        gata.package = req.Package;
        UUIManager.S.UpdateUIData();
        return req;
    }
}
