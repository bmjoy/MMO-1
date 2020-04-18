using System.Collections;
using System.Collections.Generic;
using Proto;
using UnityEngine;

public interface IBattleGate
{
    float TimeServerNow { get; }
    UPerceptionView PreView { get; }
    Texture LookAtView { get; }
    UCharacterView Owner { get; }
    PlayerPackage Package { get; }
    DHero Hero { get; }
    void ReleaseSkill(HeroMagicData data);
    void Exit();
    void MoveDir(UnityEngine.Vector3 dir);
    void TrySendLookForward(bool force);
    void DoNormalAttack();
    bool SendUseItem(ItemType type);
    bool IsHpFull();
    bool IsMpFull();
}