using System;
using Proto;
using Proto.GateServerTask;
using XNet.Libs.Net;

[TaskHandler(typeof(IGateServerTask))]
public class GateServerTaskHandler : TaskHandler, IGateServerTask
{
    public Task_G2C_JoinBattle JoinBattle(Task_G2C_JoinBattle req)
    {
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
