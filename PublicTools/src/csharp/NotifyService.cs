
using Proto;
using Google.Protobuf;
using Google.Protobuf.WellKnownTypes;
using Proto.PServices;
using System.Threading.Tasks;
namespace Proto.NotifyService
{

    /// <summary>
    /// 1
    /// </summary>    
    [API(1)]
    public class CharacterAlpha:APIBase<Void, Notify_CharacterAlpha> 
    {
        private CharacterAlpha() : base() { }
        public  static CharacterAlpha CreateQuery(){ return new CharacterAlpha();}
    }
    

    /// <summary>
    /// 2
    /// </summary>    
    [API(2)]
    public class CharacterPosition:APIBase<Void, Notify_CharacterSetPosition> 
    {
        private CharacterPosition() : base() { }
        public  static CharacterPosition CreateQuery(){ return new CharacterPosition();}
    }
    

    /// <summary>
    /// 3
    /// </summary>    
    [API(3)]
    public class CreateBattleCharacter:APIBase<Void, Notify_CreateBattleCharacter> 
    {
        private CreateBattleCharacter() : base() { }
        public  static CreateBattleCharacter CreateQuery(){ return new CreateBattleCharacter();}
    }
    

    /// <summary>
    /// 4
    /// </summary>    
    [API(4)]
    public class CreateMissile:APIBase<Void, Notify_CreateMissile> 
    {
        private CreateMissile() : base() { }
        public  static CreateMissile CreateQuery(){ return new CreateMissile();}
    }
    

    /// <summary>
    /// 5
    /// </summary>    
    [API(5)]
    public class CreateReleaser:APIBase<Void, Notify_CreateReleaser> 
    {
        private CreateReleaser() : base() { }
        public  static CreateReleaser CreateQuery(){ return new CreateReleaser();}
    }
    

    /// <summary>
    /// 6
    /// </summary>    
    [API(6)]
    public class DamageResult:APIBase<Void, Notify_DamageResult> 
    {
        private DamageResult() : base() { }
        public  static DamageResult CreateQuery(){ return new DamageResult();}
    }
    

    /// <summary>
    /// 7
    /// </summary>    
    [API(7)]
    public class Drop:APIBase<Void, Notify_Drop> 
    {
        private Drop() : base() { }
        public  static Drop CreateQuery(){ return new Drop();}
    }
    

    /// <summary>
    /// 8
    /// </summary>    
    [API(8)]
    public class ElementExitState:APIBase<Void, Notify_ElementExitState> 
    {
        private ElementExitState() : base() { }
        public  static ElementExitState CreateQuery(){ return new ElementExitState();}
    }
    

    /// <summary>
    /// 11
    /// </summary>    
    [API(11)]
    public class HPChange:APIBase<Void, Notify_HPChange> 
    {
        private HPChange() : base() { }
        public  static HPChange CreateQuery(){ return new HPChange();}
    }
    

    /// <summary>
    /// 14
    /// </summary>    
    [API(14)]
    public class LookAtCharacter:APIBase<Void, Notify_LookAtCharacter> 
    {
        private LookAtCharacter() : base() { }
        public  static LookAtCharacter CreateQuery(){ return new LookAtCharacter();}
    }
    

    /// <summary>
    /// 15
    /// </summary>    
    [API(15)]
    public class MPChange:APIBase<Void, Notify_MPChange> 
    {
        private MPChange() : base() { }
        public  static MPChange CreateQuery(){ return new MPChange();}
    }
    

    /// <summary>
    /// 16
    /// </summary>    
    [API(16)]
    public class PlayerJoinState:APIBase<Void, Notify_PlayerJoinState> 
    {
        private PlayerJoinState() : base() { }
        public  static PlayerJoinState CreateQuery(){ return new PlayerJoinState();}
    }
    

    /// <summary>
    /// 17
    /// </summary>    
    [API(17)]
    public class PropertyValue:APIBase<Void, Notify_PropertyValue> 
    {
        private PropertyValue() : base() { }
        public  static PropertyValue CreateQuery(){ return new PropertyValue();}
    }
    

    /// <summary>
    /// 19
    /// </summary>    
    [API(19)]
    public class CharacterSetForword:APIBase<Void, Notify_CharacterSetForword> 
    {
        private CharacterSetForword() : base() { }
        public  static CharacterSetForword CreateQuery(){ return new CharacterSetForword();}
    }
    

    /// <summary>
    /// 20
    /// </summary>    
    [API(20)]
    public class CharacterMoveTo:APIBase<Void, Notify_CharacterMoveTo> 
    {
        private CharacterMoveTo() : base() { }
        public  static CharacterMoveTo CreateQuery(){ return new CharacterMoveTo();}
    }
    

    /// <summary>
    /// 21
    /// </summary>    
    [API(21)]
    public class CharacterStopMove:APIBase<Void, Notify_CharacterStopMove> 
    {
        private CharacterStopMove() : base() { }
        public  static CharacterStopMove CreateQuery(){ return new CharacterStopMove();}
    }
    

    /// <summary>
    /// 22
    /// </summary>    
    [API(22)]
    public class CharacterDeath:APIBase<Void, Notify_CharacterDeath> 
    {
        private CharacterDeath() : base() { }
        public  static CharacterDeath CreateQuery(){ return new CharacterDeath();}
    }
    

    /// <summary>
    /// 23
    /// </summary>    
    [API(23)]
    public class CharacterPriorityMove:APIBase<Void, Notify_CharacterPriorityMove> 
    {
        private CharacterPriorityMove() : base() { }
        public  static CharacterPriorityMove CreateQuery(){ return new CharacterPriorityMove();}
    }
    

    /// <summary>
    /// 24
    /// </summary>    
    [API(24)]
    public class CharacterSetScale:APIBase<Void, Notify_CharacterSetScale> 
    {
        private CharacterSetScale() : base() { }
        public  static CharacterSetScale CreateQuery(){ return new CharacterSetScale();}
    }
    

    /// <summary>
    /// 25
    /// </summary>    
    [API(25)]
    public class CharacterAttachMagic:APIBase<Void, Notify_CharacterAttachMagic> 
    {
        private CharacterAttachMagic() : base() { }
        public  static CharacterAttachMagic CreateQuery(){ return new CharacterAttachMagic();}
    }
    

    /// <summary>
    /// 26
    /// </summary>    
    [API(26)]
    public class CharacterMoveForward:APIBase<Void, Notify_CharacterMoveForward> 
    {
        private CharacterMoveForward() : base() { }
        public  static CharacterMoveForward CreateQuery(){ return new CharacterMoveForward();}
    }
    

    /// <summary>
    /// 27
    /// </summary>    
    [API(27)]
    public class CharacterSpeed:APIBase<Void, Notify_CharacterSpeed> 
    {
        private CharacterSpeed() : base() { }
        public  static CharacterSpeed CreateQuery(){ return new CharacterSpeed();}
    }
    

    /// <summary>
    /// 28
    /// </summary>    
    [API(28)]
    public class CharacterLock:APIBase<Void, Notify_CharacterLock> 
    {
        private CharacterLock() : base() { }
        public  static CharacterLock CreateQuery(){ return new CharacterLock();}
    }
    

    /// <summary>
    /// 29
    /// </summary>    
    [API(29)]
    public class CharcaterPush:APIBase<Void, Notify_CharacterPush> 
    {
        private CharcaterPush() : base() { }
        public  static CharcaterPush CreateQuery(){ return new CharcaterPush();}
    }
    

    /// <summary>
    /// 30
    /// </summary>    
    [API(30)]
    public class CharacterRelive:APIBase<Void, Notify_CharacterRelive> 
    {
        private CharacterRelive() : base() { }
        public  static CharacterRelive CreateQuery(){ return new CharacterRelive();}
    }
    

    /// <summary>
    /// 31
    /// </summary>    
    [API(31)]
    public class BattleItemChangeGroupIndex:APIBase<Void, Notify_BattleItemChangeGroupIndex> 
    {
        private BattleItemChangeGroupIndex() : base() { }
        public  static BattleItemChangeGroupIndex CreateQuery(){ return new BattleItemChangeGroupIndex();}
    }
    

    /// <summary>
    /// 32
    /// </summary>    
    [API(32)]
    public class DropGold:APIBase<Void, Notify_DropGold> 
    {
        private DropGold() : base() { }
        public  static DropGold CreateQuery(){ return new DropGold();}
    }
    

    /// <summary>
    /// 33
    /// </summary>    
    [API(33)]
    public class SyncServerTime:APIBase<Void, Notify_SyncServerTime> 
    {
        private SyncServerTime() : base() { }
        public  static SyncServerTime CreateQuery(){ return new SyncServerTime();}
    }
    

    /// <summary>
    /// 35
    /// </summary>    
    [API(35)]
    public class PlayTimeLine:APIBase<Void, Notify_PlayTimeLine> 
    {
        private PlayTimeLine() : base() { }
        public  static PlayTimeLine CreateQuery(){ return new PlayTimeLine();}
    }
    

    /// <summary>
    /// 36
    /// </summary>    
    [API(36)]
    public class CharacterRotation:APIBase<Void, Notify_CharacterRotation> 
    {
        private CharacterRotation() : base() { }
        public  static CharacterRotation CreateQuery(){ return new CharacterRotation();}
    }
    

    public interface INotifyService
    {
        [API(36)]Notify_CharacterRotation CharacterRotation(Void req);
        [API(35)]Notify_PlayTimeLine PlayTimeLine(Void req);
        [API(33)]Notify_SyncServerTime SyncServerTime(Void req);
        [API(32)]Notify_DropGold DropGold(Void req);
        [API(31)]Notify_BattleItemChangeGroupIndex BattleItemChangeGroupIndex(Void req);
        [API(30)]Notify_CharacterRelive CharacterRelive(Void req);
        [API(29)]Notify_CharacterPush CharcaterPush(Void req);
        [API(28)]Notify_CharacterLock CharacterLock(Void req);
        [API(27)]Notify_CharacterSpeed CharacterSpeed(Void req);
        [API(26)]Notify_CharacterMoveForward CharacterMoveForward(Void req);
        [API(25)]Notify_CharacterAttachMagic CharacterAttachMagic(Void req);
        [API(24)]Notify_CharacterSetScale CharacterSetScale(Void req);
        [API(23)]Notify_CharacterPriorityMove CharacterPriorityMove(Void req);
        [API(22)]Notify_CharacterDeath CharacterDeath(Void req);
        [API(21)]Notify_CharacterStopMove CharacterStopMove(Void req);
        [API(20)]Notify_CharacterMoveTo CharacterMoveTo(Void req);
        [API(19)]Notify_CharacterSetForword CharacterSetForword(Void req);
        [API(17)]Notify_PropertyValue PropertyValue(Void req);
        [API(16)]Notify_PlayerJoinState PlayerJoinState(Void req);
        [API(15)]Notify_MPChange MPChange(Void req);
        [API(14)]Notify_LookAtCharacter LookAtCharacter(Void req);
        [API(11)]Notify_HPChange HPChange(Void req);
        [API(8)]Notify_ElementExitState ElementExitState(Void req);
        [API(7)]Notify_Drop Drop(Void req);
        [API(6)]Notify_DamageResult DamageResult(Void req);
        [API(5)]Notify_CreateReleaser CreateReleaser(Void req);
        [API(4)]Notify_CreateMissile CreateMissile(Void req);
        [API(3)]Notify_CreateBattleCharacter CreateBattleCharacter(Void req);
        [API(2)]Notify_CharacterSetPosition CharacterPosition(Void req);
        [API(1)]Notify_CharacterAlpha CharacterAlpha(Void req);

    }
   

    public abstract class NotifyService
    {
        [API(36)]public abstract Task<Notify_CharacterRotation> CharacterRotation(Void request);
        [API(35)]public abstract Task<Notify_PlayTimeLine> PlayTimeLine(Void request);
        [API(33)]public abstract Task<Notify_SyncServerTime> SyncServerTime(Void request);
        [API(32)]public abstract Task<Notify_DropGold> DropGold(Void request);
        [API(31)]public abstract Task<Notify_BattleItemChangeGroupIndex> BattleItemChangeGroupIndex(Void request);
        [API(30)]public abstract Task<Notify_CharacterRelive> CharacterRelive(Void request);
        [API(29)]public abstract Task<Notify_CharacterPush> CharcaterPush(Void request);
        [API(28)]public abstract Task<Notify_CharacterLock> CharacterLock(Void request);
        [API(27)]public abstract Task<Notify_CharacterSpeed> CharacterSpeed(Void request);
        [API(26)]public abstract Task<Notify_CharacterMoveForward> CharacterMoveForward(Void request);
        [API(25)]public abstract Task<Notify_CharacterAttachMagic> CharacterAttachMagic(Void request);
        [API(24)]public abstract Task<Notify_CharacterSetScale> CharacterSetScale(Void request);
        [API(23)]public abstract Task<Notify_CharacterPriorityMove> CharacterPriorityMove(Void request);
        [API(22)]public abstract Task<Notify_CharacterDeath> CharacterDeath(Void request);
        [API(21)]public abstract Task<Notify_CharacterStopMove> CharacterStopMove(Void request);
        [API(20)]public abstract Task<Notify_CharacterMoveTo> CharacterMoveTo(Void request);
        [API(19)]public abstract Task<Notify_CharacterSetForword> CharacterSetForword(Void request);
        [API(17)]public abstract Task<Notify_PropertyValue> PropertyValue(Void request);
        [API(16)]public abstract Task<Notify_PlayerJoinState> PlayerJoinState(Void request);
        [API(15)]public abstract Task<Notify_MPChange> MPChange(Void request);
        [API(14)]public abstract Task<Notify_LookAtCharacter> LookAtCharacter(Void request);
        [API(11)]public abstract Task<Notify_HPChange> HPChange(Void request);
        [API(8)]public abstract Task<Notify_ElementExitState> ElementExitState(Void request);
        [API(7)]public abstract Task<Notify_Drop> Drop(Void request);
        [API(6)]public abstract Task<Notify_DamageResult> DamageResult(Void request);
        [API(5)]public abstract Task<Notify_CreateReleaser> CreateReleaser(Void request);
        [API(4)]public abstract Task<Notify_CreateMissile> CreateMissile(Void request);
        [API(3)]public abstract Task<Notify_CreateBattleCharacter> CreateBattleCharacter(Void request);
        [API(2)]public abstract Task<Notify_CharacterSetPosition> CharacterPosition(Void request);
        [API(1)]public abstract Task<Notify_CharacterAlpha> CharacterAlpha(Void request);

    }

}