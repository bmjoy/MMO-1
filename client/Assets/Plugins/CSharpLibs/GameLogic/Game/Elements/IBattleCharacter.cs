﻿using GameLogic.Utility;
using Proto;
using UnityEngine;

namespace GameLogic.Game.Elements
{
    public interface IBattleCharacter : IBattleElement
    {
        Transform Transform { get; }
        bool IsMoving { get; }
        Quaternion Rotation { get; }
        Transform RootTransform { get; }
        float Radius { get; }

        [NeedNotify(typeof(Notify_CharacterRelive))]
        void Relive();

        [NeedNotify(typeof(Notify_CharacterSetPosition), "Position")]
        void SetPosition(Proto.Vector3 pos);//set position of the character
        [NeedNotify(typeof(Notify_CharacterSetForword), "Forward")]
        void SetForward(Proto.Vector3 forward);//forward use lookup
        [NeedNotify(typeof(Notify_LookAtCharacter), "Target")]
        void LookAtTarget(int target); //target for look
        [NeedNotify(typeof(Notify_LayoutPlayMotion), "Motion")]
        void PlayMotion(string motion);//play motion
        [NeedNotify(typeof(Notify_CharacterMoveTo), "Position", "Target", "StopDis")]
        bool MoveTo(Proto.Vector3 position, Proto.Vector3 target, float stopDis);//move to target
        [NeedNotify(typeof(Notify_CharacterStopMove), "Position")]
        void StopMove(Proto.Vector3 pos);//stop move
        [NeedNotify(typeof(Notify_CharacterDeath))]
        void Death();//death
        [NeedNotify(typeof(Notify_CharacterSpeed), "Speed")]
        void SetSpeed(float speed);//move speed
        [NeedNotify(typeof(Notify_CharacterPriorityMove), "PriorityMove")]
        void SetPriorityMove(float priorityMove);//move priority
        [NeedNotify(typeof(Notify_CharacterSetScale), "Scale")]
        void SetScale(float scale);//scale
        [NeedNotify(typeof(Notify_HPChange), "Hp", "Cur", "Max")]
        void ShowHPChange(int hp, int cur, int max); //hp changed
        [NeedNotify(typeof(Notify_MPChange), "Mp", "Cur", "Max")]
        void ShowMPChange(int mp, int cur, int maxMP);//mp changed
        [NeedNotify(typeof(Notify_PropertyValue), "Type", "FinallyValue")]
        void PropertyChange(HeroPropertyType type, int finalValue);//property changed
        [NeedNotify(typeof(Notify_CharacterAttachMagic), "MType", "MagicId", "CompletedTime")]
        void AttachMagic(MagicType mType, int magicID, float cdCompletedTime);//magic
        [NeedNotify(typeof(Notify_CharacterAlpha), "Alpha")]
        void SetAlpha(float alpha);//alpha
        [NeedNotify(typeof(Notify_CharacterMoveForward), "Position", "Forward")]
        void SetMoveDir(Proto.Vector3 pos, Proto.Vector3 forward);
        [NeedNotify(typeof(Notify_CharacterLock), "Lock")]
        void SetLock(int lockValue);
        [NeedNotify(typeof(Notify_CharacterPush), "Length", "Speed")]
        void Push(Proto.Vector3 length, Proto.Vector3 speed);
  
    }
}

