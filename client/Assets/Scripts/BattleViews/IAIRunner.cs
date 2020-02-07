using System;
using GameLogic.Game.AIBehaviorTree;
using GameLogic.Game.Elements;
using Layout;
using Layout.AITree;

public interface IAIRunner
{
    AITreeRoot RunAI(TreeNode ai);
    bool IsRuning(Layout.EventType eventType);
    bool ReleaseMagic(MagicData data);
    void Attach(BattleCharacter character);
}

public abstract class AIRunner
{
     public static IAIRunner Current { set; get; } 
}

