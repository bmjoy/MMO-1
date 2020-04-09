
/*
#############################################
       
       *此代码为工具自动生成
       *请勿单独修改该文件

#############################################
*/
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ExcelConfig;
namespace EConfig
{

    /// <summary>
    /// 战斗关卡地图表
    /// </summary>
    [ConfigFile("BattleLevelData.json","BattleLevelData")]
    [global::System.Serializable]
    public class BattleLevelData:JSONConfigBase    {
        
        /// <summary>
        /// 名称
        /// </summary>
        [ExcelConfigColIndex(1)]
        public String Name { set; get; }
        
        /// <summary>
        /// 地图ID
        /// </summary>
        [ExcelConfigColIndex(2)]
        public int MapID { set; get; }
        
        /// <summary>
        /// 关卡类型
        /// </summary>
        [ExcelConfigColIndex(3)]
        public int MapType { set; get; }
        
        /// <summary>
        /// 最大进入等级
        /// </summary>
        [ExcelConfigColIndex(4)]
        public int LimitLevel { set; get; }
        
        /// <summary>
        /// 图标
        /// </summary>
        [ExcelConfigColIndex(5)]
        public String Icon { set; get; }
        
        /// <summary>
        /// 怪物刷新组
        /// </summary>
        [ExcelConfigColIndex(6)]
        public String MonsterGroupID { set; get; }
        
        /// <summary>
        /// Boss刷新组
        /// </summary>
        [ExcelConfigColIndex(7)]
        public String BossGroupID { set; get; }
        
        /// <summary>
        /// 最大刷怪间隔时间(秒)
        /// </summary>
        [ExcelConfigColIndex(8)]
        public float MaxRefrshTime { set; get; }
        
        /// <summary>
        /// 怪物最大数量
        /// </summary>
        [ExcelConfigColIndex(9)]
        public int MaxMonster { set; get; }
        
        /// <summary>
        /// boss出现需求杀怪数
        /// </summary>
        [ExcelConfigColIndex(10)]
        public int BossNeedKilledNumber { set; get; }
        
        /// <summary>
        /// 描述
        /// </summary>
        [ExcelConfigColIndex(11)]
        public String Description { set; get; }

    }

    /// <summary>
    /// 怪物刷新组
    /// </summary>
    [ConfigFile("MonsterGroupData.json","MonsterGroupData")]
    [global::System.Serializable]
    public class MonsterGroupData:JSONConfigBase    {
        
        /// <summary>
        /// 掉落ID
        /// </summary>
        [ExcelConfigColIndex(1)]
        public int DropID { set; get; }
        
        /// <summary>
        /// 怪物ID
        /// </summary>
        [ExcelConfigColIndex(2)]
        public String MonsterID { set; get; }
        
        /// <summary>
        /// 出现概率
        /// </summary>
        [ExcelConfigColIndex(3)]
        public String Pro { set; get; }
        
        /// <summary>
        /// 最少刷怪数
        /// </summary>
        [ExcelConfigColIndex(4)]
        public int MonsterNumberMin { set; get; }
        
        /// <summary>
        /// 最大刷怪数
        /// </summary>
        [ExcelConfigColIndex(5)]
        public int MonsterNumberMax { set; get; }
        
        /// <summary>
        /// 站位
        /// </summary>
        [ExcelConfigColIndex(6)]
        public int StandType { set; get; }
        
        /// <summary>
        /// 参数
        /// </summary>
        [ExcelConfigColIndex(0)]
        public List<float> StandParams { set; get; }

    }

    /// <summary>
    /// 掉落组
    /// </summary>
    [ConfigFile("DropGroupData.json","DropGroupData")]
    [global::System.Serializable]
    public class DropGroupData:JSONConfigBase    {
        
        /// <summary>
        /// 名称
        /// </summary>
        [ExcelConfigColIndex(1)]
        public String Name { set; get; }
        
        /// <summary>
        /// 万分比掉落
        /// </summary>
        [ExcelConfigColIndex(2)]
        public int DropPro { set; get; }
        
        /// <summary>
        /// 掉落种类小
        /// </summary>
        [ExcelConfigColIndex(3)]
        public int DropMinNum { set; get; }
        
        /// <summary>
        /// 最大
        /// </summary>
        [ExcelConfigColIndex(4)]
        public int DropMaxNum { set; get; }
        
        /// <summary>
        /// 道具
        /// </summary>
        [ExcelConfigColIndex(5)]
        public String DropItem { set; get; }
        
        /// <summary>
        /// 掉落数
        /// </summary>
        [ExcelConfigColIndex(6)]
        public String DropNum { set; get; }
        
        /// <summary>
        /// 掉落概率
        /// </summary>
        [ExcelConfigColIndex(7)]
        public String Pro { set; get; }
        
        /// <summary>
        /// 掉落金币
        /// </summary>
        [ExcelConfigColIndex(8)]
        public int GoldMin { set; get; }
        
        /// <summary>
        /// 掉落金币大
        /// </summary>
        [ExcelConfigColIndex(9)]
        public int GoldMax { set; get; }

    }

    /// <summary>
    /// 怪物表
    /// </summary>
    [ConfigFile("MonsterData.json","MonsterData")]
    [global::System.Serializable]
    public class MonsterData:JSONConfigBase    {
        
        /// <summary>
        /// 名称
        /// </summary>
        [ExcelConfigColIndex(1)]
        public String NamePrefix { set; get; }
        
        /// <summary>
        /// 角色ID
        /// </summary>
        [ExcelConfigColIndex(2)]
        public int CharacterID { set; get; }
        
        /// <summary>
        /// 杀死获得经验
        /// </summary>
        [ExcelConfigColIndex(3)]
        public int Exp { set; get; }
        
        /// <summary>
        /// 等级
        /// </summary>
        [ExcelConfigColIndex(4)]
        public int Level { set; get; }
        
        /// <summary>
        /// 生命修正
        /// </summary>
        [ExcelConfigColIndex(5)]
        public int HPMax { set; get; }
        
        /// <summary>
        /// 伤害修正
        /// </summary>
        [ExcelConfigColIndex(6)]
        public int DamageMin { set; get; }
        
        /// <summary>
        /// 伤害修正
        /// </summary>
        [ExcelConfigColIndex(7)]
        public int DamageMax { set; get; }
        
        /// <summary>
        /// 力量
        /// </summary>
        [ExcelConfigColIndex(8)]
        public int Force { set; get; }
        
        /// <summary>
        /// 敏捷
        /// </summary>
        [ExcelConfigColIndex(9)]
        public int Agility { set; get; }
        
        /// <summary>
        /// 智力
        /// </summary>
        [ExcelConfigColIndex(10)]
        public int Knowledeg { set; get; }

    }

    /// <summary>
    /// 英雄升级经验表
    /// </summary>
    [ConfigFile("CharacterLevelUpData.json","CharacterLevelUpData")]
    [global::System.Serializable]
    public class CharacterLevelUpData:JSONConfigBase    {
        
        /// <summary>
        /// 等级
        /// </summary>
        [ExcelConfigColIndex(1)]
        public int Level { set; get; }
        
        /// <summary>
        /// 需要经验
        /// </summary>
        [ExcelConfigColIndex(2)]
        public int NeedExprices { set; get; }
        
        /// <summary>
        /// 需要总计经验
        /// </summary>
        [ExcelConfigColIndex(3)]
        public int NeedTotalExprices { set; get; }

    }

    /// <summary>
    /// 角色数据表
    /// </summary>
    [ConfigFile("CharacterData.json","CharacterData")]
    [global::System.Serializable]
    public class CharacterData:JSONConfigBase    {
        
        /// <summary>
        /// 名称
        /// </summary>
        [ExcelConfigColIndex(1)]
        public String Name { set; get; }
        
        /// <summary>
        /// 资源目录
        /// </summary>
        [ExcelConfigColIndex(2)]
        public String ResourcesPath { set; get; }
        
        /// <summary>
        /// AI路径
        /// </summary>
        [ExcelConfigColIndex(3)]
        public String AIResourcePath { set; get; }
        
        /// <summary>
        /// 大小
        /// </summary>
        [ExcelConfigColIndex(4)]
        public float ViewSize { set; get; }
        
        /// <summary>
        /// 视野(m)
        /// </summary>
        [ExcelConfigColIndex(5)]
        public float ViewDistance { set; get; }
        
        /// <summary>
        /// 攻击速度(间隔秒)
        /// </summary>
        [ExcelConfigColIndex(6)]
        public float AttackSpeed { set; get; }
        
        /// <summary>
        /// 移动速度（m/s）
        /// </summary>
        [ExcelConfigColIndex(7)]
        public float MoveSpeed { set; get; }
        
        /// <summary>
        /// 避让优先级
        /// </summary>
        [ExcelConfigColIndex(8)]
        public float PriorityMove { set; get; }
        
        /// <summary>
        /// 魔法
        /// </summary>
        [ExcelConfigColIndex(9)]
        public int MPMax { set; get; }
        
        /// <summary>
        /// 血量
        /// </summary>
        [ExcelConfigColIndex(10)]
        public int HPMax { set; get; }
        
        /// <summary>
        /// 伤害小
        /// </summary>
        [ExcelConfigColIndex(11)]
        public int DamageMin { set; get; }
        
        /// <summary>
        /// 伤害大
        /// </summary>
        [ExcelConfigColIndex(12)]
        public int DamageMax { set; get; }
        
        /// <summary>
        /// 防御力
        /// </summary>
        [ExcelConfigColIndex(13)]
        public int Defance { set; get; }
        
        /// <summary>
        /// 力量
        /// </summary>
        [ExcelConfigColIndex(14)]
        public int Force { set; get; }
        
        /// <summary>
        /// 智力
        /// </summary>
        [ExcelConfigColIndex(15)]
        public int Knowledge { set; get; }
        
        /// <summary>
        /// 敏捷
        /// </summary>
        [ExcelConfigColIndex(16)]
        public int Agility { set; get; }
        
        /// <summary>
        /// 力量成长
        /// </summary>
        [ExcelConfigColIndex(17)]
        public float ForceGrowth { set; get; }
        
        /// <summary>
        /// 智力成长
        /// </summary>
        [ExcelConfigColIndex(18)]
        public float KnowledgeGrowth { set; get; }
        
        /// <summary>
        /// 敏捷成长
        /// </summary>
        [ExcelConfigColIndex(19)]
        public float AgilityGrowth { set; get; }
        
        /// <summary>
        /// 种类
        /// </summary>
        [ExcelConfigColIndex(20)]
        public int Category { set; get; }
        
        /// <summary>
        /// 防御类型
        /// </summary>
        [ExcelConfigColIndex(21)]
        public int DefanceType { set; get; }
        
        /// <summary>
        /// 攻击类型
        /// </summary>
        [ExcelConfigColIndex(22)]
        public int DamageType { set; get; }

    }

    /// <summary>
    /// 角色技能
    /// </summary>
    [ConfigFile("CharacterMagicData.json","CharacterMagicData")]
    [global::System.Serializable]
    public class CharacterMagicData:JSONConfigBase    {
        
        /// <summary>
        /// 名称
        /// </summary>
        [ExcelConfigColIndex(1)]
        public String Name { set; get; }
        
        /// <summary>
        /// 所属角色
        /// </summary>
        [ExcelConfigColIndex(2)]
        public int CharacterID { set; get; }
        
        /// <summary>
        /// 魔法图标
        /// </summary>
        [ExcelConfigColIndex(3)]
        public String IconKey { set; get; }
        
        /// <summary>
        /// 魔法Key
        /// </summary>
        [ExcelConfigColIndex(4)]
        public String MagicKey { set; get; }
        
        /// <summary>
        /// 需求MP
        /// </summary>
        [ExcelConfigColIndex(5)]
        public int MPCost { set; get; }
        
        /// <summary>
        /// 释放最小距离(m)
        /// </summary>
        [ExcelConfigColIndex(6)]
        public float RangeMin { set; get; }
        
        /// <summary>
        /// 释放最大距离(m)
        /// </summary>
        [ExcelConfigColIndex(7)]
        public float RangeMax { set; get; }
        
        /// <summary>
        /// 释放类型
        /// </summary>
        [ExcelConfigColIndex(8)]
        public int ReleaseType { set; get; }
        
        /// <summary>
        /// 释放参数
        /// </summary>
        [ExcelConfigColIndex(9)]
        public int AITargetType { set; get; }
        
        /// <summary>
        /// CoolDown(s)
        /// </summary>
        [ExcelConfigColIndex(10)]
        public float TickTime { set; get; }
        
        /// <summary>
        /// 描述
        /// </summary>
        [ExcelConfigColIndex(11)]
        public String Description { set; get; }

    }

    /// <summary>
    /// 玩家角色表
    /// </summary>
    [ConfigFile("CharacterPlayerData.json","CharacterPlayerData")]
    [global::System.Serializable]
    public class CharacterPlayerData:JSONConfigBase    {
        
        /// <summary>
        /// 名称
        /// </summary>
        [ExcelConfigColIndex(1)]
        public int CharacterID { set; get; }
        
        /// <summary>
        /// 普通攻击技能
        /// </summary>
        [ExcelConfigColIndex(2)]
        public int NormalAttack { set; get; }
        
        /// <summary>
        /// 创建角色动作
        /// </summary>
        [ExcelConfigColIndex(3)]
        public String Motion { set; get; }
        
        /// <summary>
        /// 角色说明
        /// </summary>
        [ExcelConfigColIndex(4)]
        public String Description { set; get; }

    }

    /// <summary>
    /// 游戏常量表
    /// </summary>
    [ConfigFile("ConstantValue.json","ConstantValue")]
    [global::System.Serializable]
    public class ConstantValue:JSONConfigBase    {
        
        /// <summary>
        /// 用户初始化钻石
        /// </summary>
        [ExcelConfigColIndex(1)]
        public int PLAYER_COIN { set; get; }
        
        /// <summary>
        /// 初始化金币
        /// </summary>
        [ExcelConfigColIndex(2)]
        public int PLAYER_GOLD { set; get; }
        
        /// <summary>
        /// 初始化数据 
        /// </summary>
        [ExcelConfigColIndex(3)]
        public int PACKAGE_SIZE { set; get; }
        
        /// <summary>
        /// 购买消耗
        /// </summary>
        [ExcelConfigColIndex(4)]
        public int PACKAGE_BUY_COST { set; get; }
        
        /// <summary>
        /// 购买消耗增容量
        /// </summary>
        [ExcelConfigColIndex(5)]
        public int PACKAGE_BUY_SIZE { set; get; }
        
        /// <summary>
        /// 购买消耗增容量上限
        /// </summary>
        [ExcelConfigColIndex(6)]
        public int PACKAGE_SIZE_LIMIT { set; get; }

    }

    /// <summary>
    /// 装备升级表
    /// </summary>
    [ConfigFile("EquipmentLevelUpData.json","EquipmentLevelUpData")]
    [global::System.Serializable]
    public class EquipmentLevelUpData:JSONConfigBase    {
        
        /// <summary>
        /// 品质
        /// </summary>
        [ExcelConfigColIndex(1)]
        public int Quality { set; get; }
        
        /// <summary>
        /// 装备级别
        /// </summary>
        [ExcelConfigColIndex(2)]
        public int Level { set; get; }
        
        /// <summary>
        /// 附加比例万分比
        /// </summary>
        [ExcelConfigColIndex(3)]
        public int AppendRate { set; get; }
        
        /// <summary>
        /// 成功概率
        /// </summary>
        [ExcelConfigColIndex(4)]
        public int Pro { set; get; }
        
        /// <summary>
        /// 消耗金币
        /// </summary>
        [ExcelConfigColIndex(5)]
        public int CostGold { set; get; }
        
        /// <summary>
        /// 消耗钻石
        /// </summary>
        [ExcelConfigColIndex(6)]
        public int CostCoin { set; get; }

    }

    /// <summary>
    /// 装备数据表
    /// </summary>
    [ConfigFile("EquipmentData.json","EquipmentData")]
    [global::System.Serializable]
    public class EquipmentData:JSONConfigBase    {
        
        /// <summary>
        /// 名称
        /// </summary>
        [ExcelConfigColIndex(1)]
        public String Name { set; get; }
        
        /// <summary>
        /// 品质
        /// </summary>
        [ExcelConfigColIndex(2)]
        public int Quality { set; get; }
        
        /// <summary>
        /// 装备类型
        /// </summary>
        [ExcelConfigColIndex(3)]
        public int PartType { set; get; }
        
        /// <summary>
        /// 属性
        /// </summary>
        [ExcelConfigColIndex(4)]
        public String Properties { set; get; }
        
        /// <summary>
        /// 属性值
        /// </summary>
        [ExcelConfigColIndex(5)]
        public String PropertyValues { set; get; }

    }

    /// <summary>
    /// 装备刷新
    /// </summary>
    [ConfigFile("EquipRefreshData.json","EquipRefreshData")]
    [global::System.Serializable]
    public class EquipRefreshData:JSONConfigBase    {
        
        /// <summary>
        /// 品质
        /// </summary>
        [ExcelConfigColIndex(1)]
        public int Quality { set; get; }
        
        /// <summary>
        /// 金币消耗
        /// </summary>
        [ExcelConfigColIndex(2)]
        public int CostGold { set; get; }
        
        /// <summary>
        /// 成功概率万分比
        /// </summary>
        [ExcelConfigColIndex(3)]
        public int Pro { set; get; }
        
        /// <summary>
        /// 需求装备品质
        /// </summary>
        [ExcelConfigColIndex(4)]
        public int NeedQuality { set; get; }
        
        /// <summary>
        /// 最大可刷新次数
        /// </summary>
        [ExcelConfigColIndex(5)]
        public int MaxRefreshTimes { set; get; }
        
        /// <summary>
        /// 需要消耗道具
        /// </summary>
        [ExcelConfigColIndex(6)]
        public int NeedItemCount { set; get; }
        
        /// <summary>
        /// 属性添加值
        /// </summary>
        [ExcelConfigColIndex(7)]
        public int PropertyAppendMin { set; get; }
        
        /// <summary>
        /// 属性添加上限
        /// </summary>
        [ExcelConfigColIndex(8)]
        public int PropertyAppendMax { set; get; }
        
        /// <summary>
        /// 属性添加数量
        /// </summary>
        [ExcelConfigColIndex(9)]
        public int PropertyAppendCountMin { set; get; }
        
        /// <summary>
        /// 属性添加数量
        /// </summary>
        [ExcelConfigColIndex(10)]
        public int PropertyAppendCountMax { set; get; }

    }

    /// <summary>
    /// 属性参数对照
    /// </summary>
    [ConfigFile("RefreshPropertyValueData.json","RefreshPropertyValueData")]
    [global::System.Serializable]
    public class RefreshPropertyValueData:JSONConfigBase    {
        
        /// <summary>
        /// 属性单位值
        /// </summary>
        [ExcelConfigColIndex(1)]
        public int Value { set; get; }

    }

    /// <summary>
    /// 道具表
    /// </summary>
    [ConfigFile("ItemData.json","ItemData")]
    [global::System.Serializable]
    public class ItemData:JSONConfigBase    {
        
        /// <summary>
        /// 名称
        /// </summary>
        [ExcelConfigColIndex(1)]
        public String Name { set; get; }
        
        /// <summary>
        /// 售卖价格
        /// </summary>
        [ExcelConfigColIndex(2)]
        public int SalePrice { set; get; }
        
        /// <summary>
        /// 类别
        /// </summary>
        [ExcelConfigColIndex(3)]
        public int ItemType { set; get; }
        
        /// <summary>
        /// 品质
        /// </summary>
        [ExcelConfigColIndex(4)]
        public int Quality { set; get; }
        
        /// <summary>
        /// 模型
        /// </summary>
        [ExcelConfigColIndex(5)]
        public String ResModel { set; get; }
        
        /// <summary>
        /// 图标
        /// </summary>
        [ExcelConfigColIndex(6)]
        public String Icon { set; get; }
        
        /// <summary>
        /// 是否可堆叠
        /// </summary>
        [ExcelConfigColIndex(9)]
        public int Unique { set; get; }
        
        /// <summary>
        /// 最大堆叠数
        /// </summary>
        [ExcelConfigColIndex(10)]
        public int MaxStackNum { set; get; }
        
        /// <summary>
        /// 描述
        /// </summary>
        [ExcelConfigColIndex(11)]
        public String Description { set; get; }
        
        /// <summary>
        /// 参数1
        /// </summary>
        [ExcelConfigColIndex(0)]
        public List<String> Params { set; get; }

    }

    /// <summary>
    /// 商店表
    /// </summary>
    [ConfigFile("ItemShopData.json","ItemShopData")]
    [global::System.Serializable]
    public class ItemShopData:JSONConfigBase    {
        
        /// <summary>
        /// 商店ID
        /// </summary>
        [ExcelConfigColIndex(1)]
        public int ShopId { set; get; }
        
        /// <summary>
        /// 名称
        /// </summary>
        [ExcelConfigColIndex(2)]
        public String Name { set; get; }
        
        /// <summary>
        /// 商店道具
        /// </summary>
        [ExcelConfigColIndex(3)]
        public String ItemIds { set; get; }
        
        /// <summary>
        /// 道具数量
        /// </summary>
        [ExcelConfigColIndex(4)]
        public String ItemNums { set; get; }
        
        /// <summary>
        /// 价格
        /// </summary>
        [ExcelConfigColIndex(5)]
        public String ItemPrices { set; get; }
        
        /// <summary>
        /// 货币类型
        /// </summary>
        [ExcelConfigColIndex(6)]
        public String CoinTypes { set; get; }

    }

    /// <summary>
    /// 金币商店
    /// </summary>
    [ConfigFile("GoldShopData.json","GoldShopData")]
    [global::System.Serializable]
    public class GoldShopData:JSONConfigBase    {
        
        /// <summary>
        /// 名称
        /// </summary>
        [ExcelConfigColIndex(1)]
        public String Name { set; get; }
        
        /// <summary>
        /// 图片
        /// </summary>
        [ExcelConfigColIndex(2)]
        public String Icon { set; get; }
        
        /// <summary>
        /// 价格
        /// </summary>
        [ExcelConfigColIndex(3)]
        public int Prices { set; get; }
        
        /// <summary>
        /// 
        /// </summary>
        [ExcelConfigColIndex(4)]
        public int ReceiveGold { set; get; }
        
        /// <summary>
        /// 每个钻石多少金币
        /// </summary>
        [ExcelConfigColIndex(5)]
        public float CoinOfGold { set; get; }

    }

    /// <summary>
    /// 钻石商店
    /// </summary>
    [ConfigFile("GemShopData.json","GemShopData")]
    [global::System.Serializable]
    public class GemShopData:JSONConfigBase    {
        
        /// <summary>
        /// 名称
        /// </summary>
        [ExcelConfigColIndex(1)]
        public String Name { set; get; }
        
        /// <summary>
        /// 图片
        /// </summary>
        [ExcelConfigColIndex(2)]
        public String Icon { set; get; }
        
        /// <summary>
        /// 商店Key
        /// </summary>
        [ExcelConfigColIndex(3)]
        public String BundleId { set; get; }
        
        /// <summary>
        /// 价格
        /// </summary>
        [ExcelConfigColIndex(4)]
        public int Prices { set; get; }
        
        /// <summary>
        /// 获得钻石
        /// </summary>
        [ExcelConfigColIndex(5)]
        public int ReceiveCoin { set; get; }
        
        /// <summary>
        /// 价值兑换
        /// </summary>
        [ExcelConfigColIndex(6)]
        public float OneOfValue { set; get; }

    }

    /// <summary>
    /// 英雄数据表
    /// </summary>
    [ConfigFile("MagicLevelUpData.json","MagicLevelUpData")]
    [global::System.Serializable]
    public class MagicLevelUpData:JSONConfigBase    {
        
        /// <summary>
        /// ID
        /// </summary>
        [ExcelConfigColIndex(1)]
        public int MagicID { set; get; }
        
        /// <summary>
        /// 需要等级
        /// </summary>
        [ExcelConfigColIndex(2)]
        public int NeedLevel { set; get; }
        
        /// <summary>
        /// 需求金币
        /// </summary>
        [ExcelConfigColIndex(3)]
        public int NeedGold { set; get; }
        
        /// <summary>
        /// 等级
        /// </summary>
        [ExcelConfigColIndex(4)]
        public int Level { set; get; }
        
        /// <summary>
        /// 描述
        /// </summary>
        [ExcelConfigColIndex(10)]
        public String Description { set; get; }
        
        /// <summary>
        /// 参数1
        /// </summary>
        [ExcelConfigColIndex(0)]
        public List<String> Param { set; get; }

    }

    /// <summary>
    /// 等级表
    /// </summary>
    [ConfigFile("MapData.json","MapData")]
    [global::System.Serializable]
    public class MapData:JSONConfigBase    {
        
        /// <summary>
        /// 名称
        /// </summary>
        [ExcelConfigColIndex(1)]
        public String Name { set; get; }
        
        /// <summary>
        /// 资源名称
        /// </summary>
        [ExcelConfigColIndex(2)]
        public String LevelName { set; get; }
        
        /// <summary>
        /// 刷怪点
        /// </summary>
        [ExcelConfigColIndex(3)]
        public String MonsterPos { set; get; }
        
        /// <summary>
        /// Boss刷怪点
        /// </summary>
        [ExcelConfigColIndex(4)]
        public String BossPos { set; get; }

    }

 }
