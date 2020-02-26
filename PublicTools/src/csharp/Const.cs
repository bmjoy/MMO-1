// Generated by the protocol buffer compiler.  DO NOT EDIT!
// source: const.proto
#pragma warning disable 1591, 0612, 3021
#region Designer generated code

using pb = global::Google.Protobuf;
using pbc = global::Google.Protobuf.Collections;
using pbr = global::Google.Protobuf.Reflection;
using scg = global::System.Collections.Generic;
namespace Proto {

  /// <summary>Holder for reflection information generated from const.proto</summary>
  public static partial class ConstReflection {

    #region Descriptor
    /// <summary>File descriptor for const.proto</summary>
    public static pbr::FileDescriptor Descriptor {
      get { return descriptor; }
    }
    private static pbr::FileDescriptor descriptor;

    static ConstReflection() {
      byte[] descriptorData = global::System.Convert.FromBase64String(
          string.Concat(
            "Cgtjb25zdC5wcm90bxIFUHJvdG8iBgoEVm9pZCqeAwoJRXJyb3JDb2RlEgkK",
            "BUVycm9yEAASBgoCT0sQARIQCgxMb2dpbkZhaWx1cmUQAhIUChBSZWdFeGlz",
            "dFVzZXJOYW1lEAMSFwoTUmVnSW5wdXRFbXB0eU9yTnVsbBAEEhQKEE5vR2Ft",
            "ZVBsYXllckRhdGEQBRIOCgpOb0hlcm9JbmZvEAYSEwoPTk9Gb3VuZFNlcnZl",
            "cklEEAcSFgoSTk9GcmVlQmF0dGxlU2VydmVyEAgSFAoQUGxheWVySXNJbkJh",
            "dHRsZRAJEh0KGUJhdHRsZVNlcnZlckhhc0Rpc2Nvbm5lY3QQChIdChlOT0Zv",
            "dW5kVXNlck9uQmF0dGxlU2VydmVyEAsSGwoXTk9Gb3VuZFVzZXJCYXR0bGVT",
            "ZXJ2ZXIQDBIPCgtOT0ZvdW5kSXRlbRANEhAKDE5PRW5vdWdoSXRlbRAOEhAK",
            "DElzV2Vhck9uSGVybxAPEhEKDU5vRW5vdWdodEdvbGQQEBIUChBOb0ZyZWVH",
            "YXRlU2VydmVyEBESGwoXTmFtZU9yUHdkTGVnaHRJbmNvcnJlY3QQEmIGcHJv",
            "dG8z"));
      descriptor = pbr::FileDescriptor.FromGeneratedCode(descriptorData,
          new pbr::FileDescriptor[] { },
          new pbr::GeneratedClrTypeInfo(new[] {typeof(global::Proto.ErrorCode), }, new pbr::GeneratedClrTypeInfo[] {
            new pbr::GeneratedClrTypeInfo(typeof(global::Proto.Void), global::Proto.Void.Parser, null, null, null, null)
          }));
    }
    #endregion

  }
  #region Enums
  /// <summary>
  ///错误码 考虑平台问题 不要尝试串码
  /// </summary>
  public enum ErrorCode {
    /// <summary>
    ///VersionError=9999;//协议版本异常
    /// </summary>
    [pbr::OriginalName("Error")] Error = 0,
    /// <summary>
    ///处理成功
    /// </summary>
    [pbr::OriginalName("OK")] Ok = 1,
    /// <summary>
    ///登陆失败
    /// </summary>
    [pbr::OriginalName("LoginFailure")] LoginFailure = 2,
    /// <summary>
    ///用户名重复
    /// </summary>
    [pbr::OriginalName("RegExistUserName")] RegExistUserName = 3,
    /// <summary>
    ///输入为空
    /// </summary>
    [pbr::OriginalName("RegInputEmptyOrNull")] RegInputEmptyOrNull = 4,
    /// <summary>
    ///没有游戏角色信息
    /// </summary>
    [pbr::OriginalName("NoGamePlayerData")] NoGamePlayerData = 5,
    /// <summary>
    ///英雄数据异常
    /// </summary>
    [pbr::OriginalName("NoHeroInfo")] NoHeroInfo = 6,
    /// <summary>
    ///没有对应的serverID
    /// </summary>
    [pbr::OriginalName("NOFoundServerID")] NofoundServerId = 7,
    /// <summary>
    ///没有空闲的战斗服务器
    /// </summary>
    [pbr::OriginalName("NOFreeBattleServer")] NofreeBattleServer = 8,
    /// <summary>
    ///玩家已经在战斗中
    /// </summary>
    [pbr::OriginalName("PlayerIsInBattle")] PlayerIsInBattle = 9,
    /// <summary>
    ///战斗服务器已经断开连接
    /// </summary>
    [pbr::OriginalName("BattleServerHasDisconnect")] BattleServerHasDisconnect = 10,
    /// <summary>
    ///没有申请战斗服务器
    /// </summary>
    [pbr::OriginalName("NOFoundUserOnBattleServer")] NofoundUserOnBattleServer = 11,
    /// <summary>
    ///没有战斗服务器
    /// </summary>
    [pbr::OriginalName("NOFoundUserBattleServer")] NofoundUserBattleServer = 12,
    /// <summary>
    ///没有道具
    /// </summary>
    [pbr::OriginalName("NOFoundItem")] NofoundItem = 13,
    /// <summary>
    ///道具数量不足
    /// </summary>
    [pbr::OriginalName("NOEnoughItem")] NoenoughItem = 14,
    /// <summary>
    ///穿戴中
    /// </summary>
    [pbr::OriginalName("IsWearOnHero")] IsWearOnHero = 15,
    /// <summary>
    ///金币不足
    /// </summary>
    [pbr::OriginalName("NoEnoughtGold")] NoEnoughtGold = 16,
    /// <summary>
    ///没有空闲网关服务器
    /// </summary>
    [pbr::OriginalName("NoFreeGateServer")] NoFreeGateServer = 17,
    /// <summary>
    ///账号或者密码长度不好
    /// </summary>
    [pbr::OriginalName("NameOrPwdLeghtIncorrect")] NameOrPwdLeghtIncorrect = 18,
  }

  #endregion

  #region Messages
  /// <summary>
  ///empty text
  /// </summary>
  public sealed partial class Void : pb::IMessage<Void> {
    private static readonly pb::MessageParser<Void> _parser = new pb::MessageParser<Void>(() => new Void());
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public static pb::MessageParser<Void> Parser { get { return _parser; } }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public static pbr::MessageDescriptor Descriptor {
      get { return global::Proto.ConstReflection.Descriptor.MessageTypes[0]; }
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    pbr::MessageDescriptor pb::IMessage.Descriptor {
      get { return Descriptor; }
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public Void() {
      OnConstruction();
    }

    partial void OnConstruction();

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public Void(Void other) : this() {
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public Void Clone() {
      return new Void(this);
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public override bool Equals(object other) {
      return Equals(other as Void);
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public bool Equals(Void other) {
      if (ReferenceEquals(other, null)) {
        return false;
      }
      if (ReferenceEquals(other, this)) {
        return true;
      }
      return true;
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public override int GetHashCode() {
      int hash = 1;
      return hash;
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public override string ToString() {
      return pb::JsonFormatter.ToDiagnosticString(this);
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public void WriteTo(pb::CodedOutputStream output) {
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public int CalculateSize() {
      int size = 0;
      return size;
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public void MergeFrom(Void other) {
      if (other == null) {
        return;
      }
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public void MergeFrom(pb::CodedInputStream input) {
      uint tag;
      while ((tag = input.ReadTag()) != 0) {
        switch(tag) {
          default:
            input.SkipLastField();
            break;
        }
      }
    }

  }

  #endregion

}

#endregion Designer generated code
