
using Proto;
using Google.Protobuf;
using Google.Protobuf.WellKnownTypes;
using Proto.PServices;
using System.Threading.Tasks;
namespace Proto.NotifyService
{

    /// <summary>
    /// 10001
    /// </summary>    
    [API(10001)]
    public class CharacterAlpha:APIBase<Void, Notify_CharacterAlpha> 
    {
        private CharacterAlpha() : base() { }
        public  static CharacterAlpha CreateQuery(){ return new CharacterAlpha();}
    }
    

    /// <summary>
    /// 10002
    /// </summary>    
    [API(10002)]
    public class CharacterPosition:APIBase<Void, Notify_CharacterSetPosition> 
    {
        private CharacterPosition() : base() { }
        public  static CharacterPosition CreateQuery(){ return new CharacterPosition();}
    }
    

    /// <summary>
    /// 10003
    /// </summary>    
    [API(10003)]
    public class CreateBattleCharacter:APIBase<Void, Notify_CreateBattleCharacter> 
    {
        private CreateBattleCharacter() : base() { }
        public  static CreateBattleCharacter CreateQuery(){ return new CreateBattleCharacter();}
    }
    

    /// <summary>
    /// 10004
    /// </summary>    
    [API(10004)]
    public class CreateMissile:APIBase<Void, Notify_CreateMissile> 
    {
        private CreateMissile() : base() { }
        public  static CreateMissile CreateQuery(){ return new CreateMissile();}
    }
    

    /// <summary>
    /// 10005
    /// </summary>    
    [API(10005)]
    public class CreateReleaser:APIBase<Void, Notify_CreateReleaser> 
    {
        private CreateReleaser() : base() { }
        public  static CreateReleaser CreateQuery(){ return new CreateReleaser();}
    }
    

    /// <summary>
    /// 10006
    /// </summary>    
    [API(10006)]
    public class DamageResult:APIBase<Void, Notify_DamageResult> 
    {
        private DamageResult() : base() { }
        public  static DamageResult CreateQuery(){ return new DamageResult();}
    }
    

    /// <summary>
    /// 10007
    /// </summary>    
    [API(10007)]
    public class Drop:APIBase<Void, Notify_Drop> 
    {
        private Drop() : base() { }
        public  static Drop CreateQuery(){ return new Drop();}
    }
    

    /// <summary>
    /// 10008
    /// </summary>    
    [API(10008)]
    public class ElementExitState:APIBase<Void, Notify_ElementExitState> 
    {
        private ElementExitState() : base() { }
        public  static ElementExitState CreateQuery(){ return new ElementExitState();}
    }
    

    /// <summary>
    /// 10009
    /// </summary>    
    [API(10009)]
    public class HPChange:APIBase<Void, Notify_HPChange> 
    {
        private HPChange() : base() { }
        public  static HPChange CreateQuery(){ return new HPChange();}
    }
    

    /// <summary>
    /// 10010
    /// </summary>    
    [API(10010)]
    public class LayoutPlayMotion:APIBase<Void, Notify_LayoutPlayMotion> 
    {
        private LayoutPlayMotion() : base() { }
        public  static LayoutPlayMotion CreateQuery(){ return new LayoutPlayMotion();}
    }
    

    /// <summary>
    /// 10011
    /// </summary>    
    [API(10011)]
    public class LayoutPlayParticle:APIBase<Void, Notify_LayoutPlayParticle> 
    {
        private LayoutPlayParticle() : base() { }
        public  static LayoutPlayParticle CreateQuery(){ return new LayoutPlayParticle();}
    }
    

    /// <summary>
    /// 10012
    /// </summary>    
    [API(10012)]
    public class LookAtCharacter:APIBase<Void, Notify_LookAtCharacter> 
    {
        private LookAtCharacter() : base() { }
        public  static LookAtCharacter CreateQuery(){ return new LookAtCharacter();}
    }
    

    /// <summary>
    /// 10013
    /// </summary>    
    [API(10013)]
    public class MPChange:APIBase<Void, Notify_MPChange> 
    {
        private MPChange() : base() { }
        public  static MPChange CreateQuery(){ return new MPChange();}
    }
    

    /// <summary>
    /// 10014
    /// </summary>    
    [API(10014)]
    public class PlayerJoinState:APIBase<Void, Notify_PlayerJoinState> 
    {
        private PlayerJoinState() : base() { }
        public  static PlayerJoinState CreateQuery(){ return new PlayerJoinState();}
    }
    

    /// <summary>
    /// 10015
    /// </summary>    
    [API(10015)]
    public class PropertyValue:APIBase<Void, Notify_PropertyValue> 
    {
        private PropertyValue() : base() { }
        public  static PropertyValue CreateQuery(){ return new PropertyValue();}
    }
    

    /// <summary>
    /// 10016
    /// </summary>    
    [API(10016)]
    public class CharacterSetForword:APIBase<Void, Notify_CharacterSetForword> 
    {
        private CharacterSetForword() : base() { }
        public  static CharacterSetForword CreateQuery(){ return new CharacterSetForword();}
    }
    

    /// <summary>
    /// 10017
    /// </summary>    
    [API(10017)]
    public class CharacterMoveTo:APIBase<Void, Notify_CharacterMoveTo> 
    {
        private CharacterMoveTo() : base() { }
        public  static CharacterMoveTo CreateQuery(){ return new CharacterMoveTo();}
    }
    

    /// <summary>
    /// 10018
    /// </summary>    
    [API(10018)]
    public class CharacterStopMove:APIBase<Void, Notify_CharacterStopMove> 
    {
        private CharacterStopMove() : base() { }
        public  static CharacterStopMove CreateQuery(){ return new CharacterStopMove();}
    }
    

    /// <summary>
    /// 10019
    /// </summary>    
    [API(10019)]
    public class CharacterDeath:APIBase<Void, Notify_CharacterDeath> 
    {
        private CharacterDeath() : base() { }
        public  static CharacterDeath CreateQuery(){ return new CharacterDeath();}
    }
    

    /// <summary>
    /// 10020
    /// </summary>    
    [API(10020)]
    public class CharacterPriorityMove:APIBase<Void, Notify_CharacterPriorityMove> 
    {
        private CharacterPriorityMove() : base() { }
        public  static CharacterPriorityMove CreateQuery(){ return new CharacterPriorityMove();}
    }
    

    /// <summary>
    /// 10021
    /// </summary>    
    [API(10021)]
    public class CharacterSetScale:APIBase<Void, Notify_CharacterSetScale> 
    {
        private CharacterSetScale() : base() { }
        public  static CharacterSetScale CreateQuery(){ return new CharacterSetScale();}
    }
    

    /// <summary>
    /// 10022
    /// </summary>    
    [API(10022)]
    public class CharacterAttachMagic:APIBase<Void, Notify_CharacterAttachMagic> 
    {
        private CharacterAttachMagic() : base() { }
        public  static CharacterAttachMagic CreateQuery(){ return new CharacterAttachMagic();}
    }
    

    /// <summary>
    /// 10023
    /// </summary>    
    [API(10023)]
    public class CharacterMoveForward:APIBase<Void, Notify_CharacterMoveForward> 
    {
        private CharacterMoveForward() : base() { }
        public  static CharacterMoveForward CreateQuery(){ return new CharacterMoveForward();}
    }
    

    /// <summary>
    /// 10024
    /// </summary>    
    [API(10024)]
    public class CharacterSpeed:APIBase<Void, Notify_CharacterSpeed> 
    {
        private CharacterSpeed() : base() { }
        public  static CharacterSpeed CreateQuery(){ return new CharacterSpeed();}
    }
    

    public interface INotifyService
    {
        [API(10024)]Notify_CharacterSpeed CharacterSpeed(Void req);
        [API(10023)]Notify_CharacterMoveForward CharacterMoveForward(Void req);
        [API(10022)]Notify_CharacterAttachMagic CharacterAttachMagic(Void req);
        [API(10021)]Notify_CharacterSetScale CharacterSetScale(Void req);
        [API(10020)]Notify_CharacterPriorityMove CharacterPriorityMove(Void req);
        [API(10019)]Notify_CharacterDeath CharacterDeath(Void req);
        [API(10018)]Notify_CharacterStopMove CharacterStopMove(Void req);
        [API(10017)]Notify_CharacterMoveTo CharacterMoveTo(Void req);
        [API(10016)]Notify_CharacterSetForword CharacterSetForword(Void req);
        [API(10015)]Notify_PropertyValue PropertyValue(Void req);
        [API(10014)]Notify_PlayerJoinState PlayerJoinState(Void req);
        [API(10013)]Notify_MPChange MPChange(Void req);
        [API(10012)]Notify_LookAtCharacter LookAtCharacter(Void req);
        [API(10011)]Notify_LayoutPlayParticle LayoutPlayParticle(Void req);
        [API(10010)]Notify_LayoutPlayMotion LayoutPlayMotion(Void req);
        [API(10009)]Notify_HPChange HPChange(Void req);
        [API(10008)]Notify_ElementExitState ElementExitState(Void req);
        [API(10007)]Notify_Drop Drop(Void req);
        [API(10006)]Notify_DamageResult DamageResult(Void req);
        [API(10005)]Notify_CreateReleaser CreateReleaser(Void req);
        [API(10004)]Notify_CreateMissile CreateMissile(Void req);
        [API(10003)]Notify_CreateBattleCharacter CreateBattleCharacter(Void req);
        [API(10002)]Notify_CharacterSetPosition CharacterPosition(Void req);
        [API(10001)]Notify_CharacterAlpha CharacterAlpha(Void req);

    }
   

    public abstract class NotifyService
    {
        [API(10024)]public abstract Task<Notify_CharacterSpeed> CharacterSpeed(Void request);
        [API(10023)]public abstract Task<Notify_CharacterMoveForward> CharacterMoveForward(Void request);
        [API(10022)]public abstract Task<Notify_CharacterAttachMagic> CharacterAttachMagic(Void request);
        [API(10021)]public abstract Task<Notify_CharacterSetScale> CharacterSetScale(Void request);
        [API(10020)]public abstract Task<Notify_CharacterPriorityMove> CharacterPriorityMove(Void request);
        [API(10019)]public abstract Task<Notify_CharacterDeath> CharacterDeath(Void request);
        [API(10018)]public abstract Task<Notify_CharacterStopMove> CharacterStopMove(Void request);
        [API(10017)]public abstract Task<Notify_CharacterMoveTo> CharacterMoveTo(Void request);
        [API(10016)]public abstract Task<Notify_CharacterSetForword> CharacterSetForword(Void request);
        [API(10015)]public abstract Task<Notify_PropertyValue> PropertyValue(Void request);
        [API(10014)]public abstract Task<Notify_PlayerJoinState> PlayerJoinState(Void request);
        [API(10013)]public abstract Task<Notify_MPChange> MPChange(Void request);
        [API(10012)]public abstract Task<Notify_LookAtCharacter> LookAtCharacter(Void request);
        [API(10011)]public abstract Task<Notify_LayoutPlayParticle> LayoutPlayParticle(Void request);
        [API(10010)]public abstract Task<Notify_LayoutPlayMotion> LayoutPlayMotion(Void request);
        [API(10009)]public abstract Task<Notify_HPChange> HPChange(Void request);
        [API(10008)]public abstract Task<Notify_ElementExitState> ElementExitState(Void request);
        [API(10007)]public abstract Task<Notify_Drop> Drop(Void request);
        [API(10006)]public abstract Task<Notify_DamageResult> DamageResult(Void request);
        [API(10005)]public abstract Task<Notify_CreateReleaser> CreateReleaser(Void request);
        [API(10004)]public abstract Task<Notify_CreateMissile> CreateMissile(Void request);
        [API(10003)]public abstract Task<Notify_CreateBattleCharacter> CreateBattleCharacter(Void request);
        [API(10002)]public abstract Task<Notify_CharacterSetPosition> CharacterPosition(Void request);
        [API(10001)]public abstract Task<Notify_CharacterAlpha> CharacterAlpha(Void request);

    }

}