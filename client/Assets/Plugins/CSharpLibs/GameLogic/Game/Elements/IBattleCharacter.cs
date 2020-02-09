using GameLogic.Utility;
using Proto;

namespace GameLogic.Game.Elements
{
    public interface IBattleCharacter : IBattleElement
    {
        UnityEngine.Transform Transform { get; }
        bool IsMoving { get; }

        [NeedNotify(typeof(Notify_CharacterSetPosition), "Position")]
        void SetPosition(Vector3 pos);//set position of the character
        [NeedNotify(typeof(Notify_CharacterSetForword), "Forward")]
        void SetForward(Vector3 forward);//forward use lookup
        [NeedNotify(typeof(Notify_LookAtCharacter), "Target")]
        void LookAtTarget(int target); //target for look
        [NeedNotify(typeof(Notify_LayoutPlayMotion), "Motion")]
        void PlayMotion(string motion);//play motion
        [NeedNotify(typeof(Notify_CharacterMoveTo), "Position", "Target","StopDis")]
        void MoveTo(Vector3 position, Vector3 target,float stopDis);//move to target
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
        
        [NeedNotify(typeof(Notify_CharacterAttachMagic), "MagicId", "CompletedTime")]
        void AttachMagic(int magicID, float cdCompletedTime);//magic
        [NeedNotify(typeof(Notify_CharacterAlpha), "Alpha")]
        void SetAlpha(float alpha);//alpha
        [NeedNotify(typeof(Notify_CharacterMoveForward),"Position","Forward")]
        void SetMoveDir(Vector3 pos, Vector3 forward);
    }
}

