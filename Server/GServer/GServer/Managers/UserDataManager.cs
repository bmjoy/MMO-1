using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using EConfig;
using ExcelConfig;
using GateServer;
using Google.Protobuf.Collections;
using MongoDB.Bson.Serialization.IdGenerators;
using MongoDB.Driver;
using Proto;
using Proto.MongoDB;
using ServerUtility;
using XNet.Libs.Net;
using XNet.Libs.Utility;
using static GateServer.DataBase;
using static Proto.ItemsShop.Types;

namespace GServer.Managers
{


    /// <summary>
    /// 管理用户的数据 并且管理持久化
    /// </summary>
    public class UserDataManager :XSingleton<UserDataManager>
    {

        private async Task<bool> CostGold(GamePlayerEntity player, int gold)
        {
            if (gold <= 0) return false;
            var id = Builders<GamePlayerEntity>.Filter.Eq(t => t.Uuid, player.Uuid);
            var fgold = Builders<GamePlayerEntity>.Filter.Eq(t => t.Gold, player.Gold);
            var filter = Builders<GamePlayerEntity>.Filter.And(id, fgold);
            var update = Builders<GamePlayerEntity>.Update.Set(t => t.Gold, player.Gold - gold);

            var re = await DataBase.S.Playes.UpdateOneAsync(filter, update);
            return re.ModifiedCount > 0;
        }

        public async Task<GameHeroEntity> FindHeroByPlayerId(string player_uuid)
        {
           
            var filter = Builders<GameHeroEntity>.Filter.Eq(t => t.PlayerUuid, player_uuid);
            var query = await  DataBase.S.Heros.FindAsync(filter);

            return query.Single();
        }

        public async Task<GameHeroEntity> FindHeroByAccountId(string accountID)
        {
            var player = await FindPlayerByAccountId(accountID);
            return await FindHeroByPlayerId(player.Uuid);
        }

        internal async Task<string> ProcessBattleReward(string account_uuid, RepeatedField<PlayerItem> items, int exp, int level, int gold)
        {
            var player = await FindPlayerByAccountId(account_uuid);
            if (player == null) return null;
            var pupdate = Builders<GamePlayerEntity>.Update.Set(t =>t.Gold, gold);
            var pfilter = Builders<GamePlayerEntity>.Filter.Eq(t => t.Uuid, player.Uuid);
            await DataBase.S.Playes.UpdateOneAsync(pfilter, pupdate);
            await ProcessRewardItem(player.Uuid, items);
            var hero = await FindHeroByPlayerId(player.Uuid);
            var update = Builders<GameHeroEntity>.Update.Set(t => t.Exp, exp)
                .Set(t => t.Level, level);
            var filter = Builders<GameHeroEntity>.Filter.Eq(t => t.Uuid, hero.Uuid);
            await DataBase.S.Heros.UpdateOneAsync(filter, update);
            return  player.Uuid;
        }

        internal async Task SyncToClient(Client userClient, string playerUuid, bool packageOnly = false)
        {
            
            var player = await FindPlayerById(playerUuid);
            var p = await FindPackageByPlayerID(playerUuid);

            var pack = new Task_G2C_SyncPackage
            {
                Coin = player.Coin,
                Gold = player.Gold,
                Package = p.ToPackage()
            };

            userClient.CreateTask(pack).Send();
            if (packageOnly) return;
            var h = (await FindHeroByPlayerId(playerUuid)).ToDhero(p);
            var hTask = new Task_G2C_SyncHero
            {
                Hero = h
            };
            userClient.CreateTask(hTask).Send();

        }

        internal async Task<G2C_BuyPackageSize> BuyPackageSize(Client client,string accountUuid, int size)
        {
            var player = await FindPlayerByAccountId(accountUuid);
            var package = await FindPackageByPlayerID(player.Uuid);
            if (package.PackageSize != size) return  new G2C_BuyPackageSize { Code = ErrorCode.Error };
            if (player.Coin < Application.Constant.PACKAGE_BUY_COST) return  new G2C_BuyPackageSize { Code = ErrorCode.NoEnoughtCoin };
            if (package.PackageSize + Application.Constant.PACKAGE_BUY_SIZE >
                Application.Constant.PACKAGE_SIZE_LIMIT) return  new G2C_BuyPackageSize { Code = ErrorCode.PackageSizeLimit };
            {
                var filter = Builders<GamePlayerEntity>.Filter.Eq(t => t.Uuid, player.Uuid);
                var update = Builders<GamePlayerEntity>.Update
                    .Set(t => t.Coin, player.Coin - Application.Constant.PACKAGE_BUY_COST);
                 await DataBase.S.Playes.UpdateOneAsync(filter, update);
            }
            {
                var filter = Builders<GamePackageEntity>.Filter.Eq(t => t.Uuid, package.Uuid);
                var update = Builders<GamePackageEntity>.Update.Set(t => t.PackageSize,
                    package.PackageSize + Application.Constant.PACKAGE_BUY_SIZE);
                await DataBase.S.Packages.UpdateOneAsync(filter, update);
            }

            await SyncToClient(client, player.Uuid, true);

            return new G2C_BuyPackageSize
            {
                Code = ErrorCode.Ok,
                OldCount = size,
                PackageCount = package.PackageSize + Application.Constant.PACKAGE_BUY_SIZE
            };
        }

        public async Task<GamePlayerEntity> FindPlayerById(string player_uuid)
        {
            var filter = Builders<GamePlayerEntity>.Filter.Eq(t => t.Uuid, player_uuid);
            var query = await DataBase.S.Playes.FindAsync(filter);
            return query.SingleOrDefault();
        }

        public async Task<GamePlayerEntity> FindPlayerByAccountId(string account_uuid)
        {
            var filter = Builders<GamePlayerEntity>.Filter.Eq(t => t.AccountUuid, account_uuid);
            var query = await DataBase.S.Playes.FindAsync(filter);
            return query.SingleOrDefault();
        }

        public async Task<GamePackageEntity> FindPackageByPlayerID(string player_uuid)
        {
            var filter = Builders<GamePackageEntity>.Filter.Eq(t => t.PlayerUuid, player_uuid);
            var query = await DataBase.S.Packages.FindAsync(filter);
            return query.Single();
        }

        public async Task<string> TryToCreateUser(string userID, int heroID, string heroName)
        {

            if (heroName.Length > 12) return  string.Empty;

            var character = ExcelToJSONConfigManager
                .Current.FirstConfig<CharacterPlayerData>(t => t.CharacterID == heroID);
            if (character == null) return string.Empty;

            var fiter = Builders<GamePlayerEntity>.Filter.Eq(t => t.AccountUuid, userID);
            var fiterHero = Builders<GameHeroEntity>.Filter.Eq(t => t.HeroName, heroName);
            //var heros = db.GetCollection<GameHeroEntity>(Hero);
            

            if(
               /*user create*/ (await DataBase.S.Playes.FindAsync(Builders<GamePlayerEntity>.Filter.Eq(t => t.AccountUuid, userID))).Any() ||
               /*hero name  */ (await DataBase.S.Heros.FindAsync(Builders<GameHeroEntity>.Filter.Eq(t => t.HeroName, heroName))).Any()
            )
            {
                
                return string.Empty;
            }

            var player = new GamePlayerEntity
            {
                AccountUuid = userID,
                Coin = Application.Constant.PLAYER_COIN,
                Gold = Application.Constant.PLAYER_GOLD,
                LastIp = string.Empty
            };
            await DataBase.S.Playes.InsertOneAsync(player);
            
            var hero = new GameHeroEntity
            {
                Exp = 0,
                HeroName = heroName,
                Level = 1,
                PlayerUuid = player.Uuid,
                HeroId = heroID
            };

            await DataBase.S.Heros.InsertOneAsync(hero);

            var package = new GamePackageEntity
            {
                PackageSize = Application.Constant.PACKAGE_SIZE,
                PlayerUuid = player.Uuid
            };

            
            await DataBase.S.Packages.InsertOneAsync(package);

            return player.Uuid;

        }



        public async Task<G2C_EquipmentLevelUp> EquipLevel(Client client, string account_uuid, string item_uuid, int level)
        {
            var player = await FindPlayerByAccountId(account_uuid);
            var player_uuid = player.Uuid;
            var p_filter = Builders<GamePackageEntity>.Filter.Eq(t => t.PlayerUuid, player_uuid);
            var package = await FindPackageByPlayerID(player_uuid);

            if (package == null) return new G2C_EquipmentLevelUp { Code = ErrorCode.Error };

            if (!package.Items.TryGetValue(item_uuid, out PackageItem item))
                return new G2C_EquipmentLevelUp { Code = ErrorCode.NofoundItem };

            var itemconfig = ExcelToJSONConfigManager.Current.GetConfigByID<ItemData>(item.Id);
            if ((ItemType)itemconfig.ItemType != ItemType.ItEquip)
                return new G2C_EquipmentLevelUp { Code = ErrorCode.Error };

            //装备获取失败
            var equipconfig = ExcelToJSONConfigManager.
                Current.GetConfigByID<EquipmentData>(int.Parse(itemconfig.Params[0]));
            if (equipconfig == null)
                return new G2C_EquipmentLevelUp { Code = ErrorCode.Error };

            //等级不一样
            if (item.Level != level)
                return new G2C_EquipmentLevelUp { Code = ErrorCode.Error };


            var levelconfig = ExcelToJSONConfigManager
                .Current
                .FirstConfig<EquipmentLevelUpData>(t =>
                {
                    return t.Level == level + 1 && t.Quality == equipconfig.Quality;
                });

            if (levelconfig == null)
                return new G2C_EquipmentLevelUp { Code = ErrorCode.Error }; ;

            var filter = Builders<GamePlayerEntity>.Filter.Eq(t => t.Uuid, player_uuid);


            if (levelconfig.CostGold > 0 && levelconfig.CostGold > player.Gold )
                return new G2C_EquipmentLevelUp { Code = ErrorCode.NoEnoughtGold };

            if (levelconfig.CostCoin > 0 && levelconfig.CostCoin > player.Coin)
                return new G2C_EquipmentLevelUp { Code = ErrorCode.NoEnoughtCoin };

            if (levelconfig.CostGold > 0)
            {
                player.Gold -= levelconfig.CostGold;
                var update = Builders<GamePlayerEntity>.Update.Set(t => t.Gold, player.Gold);
                DataBase.S.Playes.UpdateOne(filter, update);
            }

            if (levelconfig.CostCoin > 0)
            {
                player.Coin -= levelconfig.CostCoin;
                var update = Builders<GamePlayerEntity>.Update.Set(t => t.Coin, player.Coin);
                DataBase.S.Playes.UpdateOne(filter, update);
            }

            if (GRandomer.Probability10000(levelconfig.Pro))
            {
                item.Level += 1;
                var update = Builders<GamePackageEntity>.Update.Set(t => t.Items, package.Items);
                DataBase.S.Packages.UpdateOne(p_filter, update);
            }

            await SyncToClient(client, player_uuid);

            return new G2C_EquipmentLevelUp { Code = ErrorCode.Ok, Level = item.Level };
        }


        internal async Task<G2C_BuyGold> BuyGold(Client client, string accountUuid, int shopId)
        {
            var item = ExcelToJSONConfigManager.Current.GetConfigByID<GoldShopData>(shopId);
            if (item == null) return new G2C_BuyGold { Code = ErrorCode.NofoundItem };
            var player = await FindPlayerByAccountId(accountUuid);
            if (player.Coin < item.Prices) return new G2C_BuyGold { Code = ErrorCode.NoEnoughtCoin };
            var update = Builders<GamePlayerEntity>.Update
                .Set(t => t.Coin, player.Coin - item.Prices)
                .Set(t => t.Gold, player.Gold + item.ReceiveGold);
            var idFilter = Builders<GamePlayerEntity>.Filter.Eq(t => t.Uuid, player.Uuid);
            await DataBase.S.Playes.UpdateOneAsync(idFilter, update);

            await SyncToClient(client, player.Uuid, true);

            return new G2C_BuyGold
            {
                Code = ErrorCode.Ok,
                Coin = player.Coin - item.Prices,
                Gold = player.Gold + item.ReceiveGold,
                ReceivedGold = item.ReceiveGold
            };
        }

        internal async Task<G2C_MagicLevelUp> MagicLevelUp(Client client, int magicId, int level, string accountUuid)
        {
            var player = await FindPlayerByAccountId(accountUuid);
            var hero = await FindHeroByPlayerId(player.Uuid);
            var config = ExcelToJSONConfigManager.Current.GetConfigByID<CharacterMagicData>(magicId);
            if (config.CharacterID != hero.HeroId) return new G2C_MagicLevelUp { Code = ErrorCode.Error };

            var levelConfig = ExcelToJSONConfigManager
                .Current.FirstConfig<MagicLevelUpData>(t => t.Level == level && t.MagicID == magicId);

            if (levelConfig.NeedLevel > hero.Level) return new G2C_MagicLevelUp { Code = ErrorCode.NeedHeroLevel };
            if (levelConfig.NeedGold > player.Gold) return new G2C_MagicLevelUp { Code = ErrorCode.NoEnoughtGold };

            if (!hero.Magics.TryGetValue(magicId, out DBHeroMagic magic))
            {
                magic = new DBHeroMagic { Actived = true, Exp = 0, Level = 0 };
                hero.Magics.Add(magicId, magic);
            }

            if(levelConfig.NeedGold > 0)
            {
                var filter = Builders<GamePlayerEntity>.Filter.Eq(t => t.Uuid, player.Uuid);
                player.Gold -= levelConfig.NeedGold;
                var update = Builders<GamePlayerEntity>.Update.Set(t => t.Gold, player.Gold);
                await DataBase.S.Playes.UpdateOneAsync(filter, update);
            }

            magic.Level += 1;
            {
                var filter = Builders<GameHeroEntity>.Filter.Eq(t => t.Uuid, hero.Uuid);
                var update = Builders<GameHeroEntity>.Update.Set(t => t.Magics, hero.Magics);
                await DataBase.S.Heros.UpdateOneAsync(filter, update);
            }


            await SyncToClient(client, player.Uuid);

            return new G2C_MagicLevelUp
            {
                Code = ErrorCode.Ok
            };
        }

        internal async Task<ErrorCode> BuyItem(Client c,string acount_id, ShopItem item)
        {
            var player =  await FindPlayerByAccountId(acount_id);

            var id = Builders<GamePlayerEntity>.Filter.Eq(t => t.Uuid , player.Uuid);
            bool update = false;
            if (item.CType == CoinType.Coin)
            {
                if (item.Prices > player.Coin) return ErrorCode.NoEnoughtCoin;
                var coin = Builders<GamePlayerEntity>.Filter.Eq(t => t.Coin, player.Coin);
                var query_player = Builders<GamePlayerEntity>.Filter.And(id, coin);
                var update_player = Builders<GamePlayerEntity>.Update.Set(t => t.Coin ,player.Coin -  item.Prices);
                var res = await  DataBase.S.Playes.UpdateOneAsync(query_player, update_player);
                update = res.ModifiedCount > 0;
            }
            else if (item.CType == CoinType.Gold)
            {
                if (item.Prices > player.Gold) return ErrorCode.NoEnoughtGold;
                var gold = Builders<GamePlayerEntity>.Filter.Eq(t => t.Gold, player.Gold);
                var query_player = Builders<GamePlayerEntity>.Filter.And(id, gold);
                var update_player = Builders<GamePlayerEntity>.Update.Set(t => t.Gold, player.Gold - item.Prices);
                var res = await DataBase.S.Playes.UpdateOneAsync(query_player, update_player);
                update = res.ModifiedCount > 0;
            }

            if (!update) return ErrorCode.Error;

            if (await AddItems(player.Uuid, new PlayerItem { ItemID = item.ItemId, Num = item.PackageNum }))
            {
                await SyncToClient(c, player.Uuid, true);
                return ErrorCode.Ok;
            }
            return ErrorCode.Error;
        }

        internal async Task<int> HeroGetExprise(string uuid, int exp)
        {

            var hero = await FindHeroByPlayerId(uuid);
            if (AddExp(exp+hero.Exp, hero.Level, out int level, out int exExp))
            {
                var filter = Builders<GameHeroEntity>.Filter.Eq(t => t.Uuid, hero.Uuid);
                var update = Builders<GameHeroEntity>.Update.Set(t => t.Level, level).Set(t => t.Exp, exExp);

                await DataBase.S.Heros.UpdateOneAsync(filter,update);
            }

            return level;

        }


        private bool AddExp(int totalExp, int level, out int exLevel, out int exExp)
        {
            exLevel = level;
            exExp = totalExp;
            var herolevel = ExcelToJSONConfigManager.Current.FirstConfig<CharacterLevelUpData>(t => t.Level == level + 1);
            if (herolevel == null) return false;

            if (exExp >= herolevel.NeedExprices)
            {
                exLevel += 1;
                exExp -= herolevel.NeedExprices;
                if (exExp > 0)
                {
                    AddExp(exExp, exLevel, out exLevel, out exExp);
                }
            }
            return true;
        }


        public async Task<G2C_SaleItem> SaleItem(Client client, string account, IList<C2G_SaleItem.Types.SaleItem> items)
        {
            var pl = await FindPlayerByAccountId(account);

            var fiterHero = Builders<GameHeroEntity>.Filter.Eq(t => t.PlayerUuid, pl.Uuid);
            var fiterPackage = Builders<GamePackageEntity>.Filter.Eq(t => t.PlayerUuid, pl.Uuid);

            var h = (await DataBase.S.Heros.FindAsync(fiterHero)).Single();
            var p = (await DataBase.S.Packages.FindAsync(fiterPackage)).Single();
           

            foreach (var i in items)
            {
                foreach (var e in h.Equips)
                {
                    if (i.Guid == e.Value)  return new G2C_SaleItem { Code = ErrorCode.IsWearOnHero };
                }

                if (!p.Items.TryGetValue(i.Guid, out PackageItem item))
                    return new G2C_SaleItem { Code = ErrorCode.NofoundItem };
                if (item.Num < i.Num)
                    return new G2C_SaleItem { Code = ErrorCode.NoenoughItem };
                if (item.IsLock)
                    return new G2C_SaleItem { Code = ErrorCode.Error };
            }

            var total = 0;
            foreach (var i in items)
            {
                if (p.Items.TryGetValue(i.Guid, out PackageItem item))
                {
                    var config = ExcelToJSONConfigManager.Current.GetConfigByID<ItemData>(item.Id);
                    if (config.SalePrice > 0) total += config.SalePrice * i.Num;
                    item.Num -= i.Num;
                    if (item.Num == 0)
                    {
                        p.Items.Remove(i.Guid);
                    }
                }
            }

            pl.Gold += total;

            var u_player = Builders<GamePlayerEntity>.Update.Set(t => t.Gold, pl.Gold);
            var u_package = Builders<GamePackageEntity>.Update.Set(t => t.Items, p.Items);
            var u_filter = Builders<GamePlayerEntity>.Filter.Eq(t => t.Uuid, pl.Uuid);
            await DataBase.S.Playes.UpdateOneAsync(u_filter, u_player);
            await DataBase.S.Packages.UpdateOneAsync(fiterPackage, u_package);
            await SyncToClient(client, pl.Uuid);

            return new G2C_SaleItem { Code = ErrorCode.Ok, Coin = pl.Coin, Gold = pl.Gold };

        }

        internal async Task<bool> OperatorEquip(string player_uuid, string equip_uuid, EquipmentType part, bool isWear)
        {
            var h_filter = Builders<GameHeroEntity>.Filter.Eq(t => t.PlayerUuid, player_uuid);

            var hero = (await DataBase.S.Heros.FindAsync(h_filter)).SingleOrDefault();
            if (hero == null) return false;

            var pa_filter = Builders<GamePackageEntity>.Filter.Eq(t => t.PlayerUuid, player_uuid);
            var package = (await DataBase.S.Packages.FindAsync(pa_filter)).Single();

            if (!package.Items.TryGetValue(equip_uuid, out PackageItem item)) return false;

            var config = ExcelToJSONConfigManager.Current.GetConfigByID<ItemData>(item.Id);
            if (config == null) return false;

            var equipConfig = ExcelToJSONConfigManager.Current.GetConfigByID<EquipmentData>(int.Parse( config.Params[0]));
            if (equipConfig == null) return false;
            if (equipConfig.PartType != (int)part) return false;

            hero.Equips.Remove((int)part);
            if (isWear)
            {
                hero.Equips.Add((int)part, item.Uuid);
            }

            var update = Builders<GameHeroEntity>.Update.Set(t => t.Equips, hero.Equips);
            await DataBase.S.Heros.UpdateOneAsync(h_filter, update);

            return true;
        }

        private async Task<bool> ProcessRewardItem(string player_uuid, IList<PlayerItem> diff)
        {
            var pa_filter = Builders<GamePackageEntity>.Filter.Eq(t => t.PlayerUuid, player_uuid);
            var package = (await DataBase.S.Packages.FindAsync(pa_filter)).Single();
            package.Items.Clear();
            foreach (var i in diff)
            {
                package.Items.Add(i.GUID, new PackageItem { Id= i.ItemID, IsLock = i.Locked, Level =i.Level, Num = i.Num, Uuid = i.GUID });
            }
            var update = Builders<GamePackageEntity>.Update.Set(t => t.Items, package.Items);
            await DataBase.S.Packages.UpdateOneAsync(pa_filter, update);

            return true;
        }

        private PackageItem GetCanStackItem(int itemID, GamePackageEntity package)
        {
            var it = ExcelToJSONConfigManager.Current.GetConfigByID<ItemData>(itemID);
            foreach (var i in package.Items)
            {
                if (i.Value.Id == itemID)
                {
                    if (i.Value.Num < it.MaxStackNum) return i.Value;
                }
            }

            return null;
        }

        /// <summary>
        /// add item new
        /// </summary>
        /// <param name="uuid"></param>
        /// <param name="i"></param>
        /// <returns></returns>
        public async Task<bool> AddItems(string uuid, PlayerItem i)
        {
            if (i.Num <= 0) return false;
            GamePackageEntity package = await FindPackageByPlayerID(uuid);
            var it = ExcelToJSONConfigManager.Current.GetConfigByID<ItemData>(i.ItemID);
            if (it == null) return false;
            var num = i.Num;
            while (num > 0)
            {
                var cItem = GetCanStackItem(i.ItemID, package);
                if (cItem != null)
                {
                    var remainNum = it.MaxStackNum - cItem.Num;
                    var add = Math.Min(remainNum, num);
                    cItem.Num += add;
                    num -= add;
                }
                else
                {
                    var add = Math.Min(num, it.MaxStackNum);
                    num -= add;
                    var itemNum = new PackageItem
                    {
                        Uuid = Guid.NewGuid().ToString(),
                        Id = i.ItemID,
                        IsLock = i.Locked,
                        Level = i.Level,
                        Num = add
                    };
                    package.Items.Add(itemNum.Uuid, itemNum);
                }
            }

            var pa_filter = Builders<GamePackageEntity>.Filter.Eq(t => t.PlayerUuid, uuid);
            var update = Builders<GamePackageEntity>.Update.Set(t => t.Items, package.Items);
            await DataBase.S.Packages.UpdateOneAsync(pa_filter, update);
            return true;
        }

        public async Task<bool> AddGoldAndCoin(string player_uuid, int coin, int gold)
        {
            var filter = Builders<GamePlayerEntity>.Filter.Eq(t => t.Uuid, player_uuid);
            var player = (await DataBase.S.Playes.FindAsync(filter)).Single();
            var up = Builders<GamePlayerEntity>.Update;
            UpdateDefinition<GamePlayerEntity> update = null;

            if (coin > 0)
            {
                update = up.Set(t => t.Coin, player.Coin + coin);
            }

            if (gold > 0)
            {
                update = up.Set(t => t.Gold, player.Gold + gold);
            }

            var result = await DataBase.S.Playes.UpdateOneAsync(filter, update);
            return result.ModifiedCount > 0;
        }

        internal async Task<G2C_RefreshEquip> RefreshEquip(Client client, string accountId, string equipUuid, IList<string> customItem)
        {
            var player = await FindPlayerByAccountId(accountId);
            var package = await FindPackageByPlayerID(player.Uuid);

            if (!package.Items.TryGetValue(equipUuid, out PackageItem equip)) return new G2C_RefreshEquip { Code = ErrorCode.NofoundItem };

            var config = ExcelToJSONConfigManager.Current.GetConfigByID<ItemData>(equip.Id);
            if (config == null) return new G2C_RefreshEquip { Code = ErrorCode.Error };
            var refreshData = ExcelToJSONConfigManager.Current.GetConfigByID<EquipRefreshData>(config.Quality);
            int refreshCount = equip.EquipData?.RefreshCount ?? 0;
            if (refreshData.MaxRefreshTimes < refreshCount) return new G2C_RefreshEquip { Code = ErrorCode.RefreshTimeLimit };
            if (refreshData.NeedItemCount > customItem.Count) return new G2C_RefreshEquip { Code = ErrorCode.NoenoughItem };


            Dictionary<HeroPropertyType, int> values = new Dictionary<HeroPropertyType, int>();

            foreach (var i in customItem)
            {
                if (!package.Items.TryGetValue(i, out PackageItem custom)) return new G2C_RefreshEquip { Code = ErrorCode.NofoundItem };
                var itemConfig = ExcelToJSONConfigManager.Current.GetConfigByID<ItemData>(custom.Id);
                if ((ItemType)itemConfig.ItemType != ItemType.ItEquip) return new G2C_RefreshEquip { Code = ErrorCode.NofoundItem };
                if (custom.Uuid == equipUuid) return new G2C_RefreshEquip { Code = ErrorCode.NofoundItem };
                if (itemConfig.Quality < refreshData.NeedQuality) return new G2C_RefreshEquip { Code = ErrorCode.NeedItemQuality };
                var equipConfig = ExcelToJSONConfigManager.Current.GetConfigByID<EquipmentData>(int.Parse(itemConfig.Params[0]));
                if (equipConfig == null) return new G2C_RefreshEquip { Code = ErrorCode.Error };
                var pre = equipConfig.Properties.SplitToInt();
                var vals = equipConfig.PropertyValues.SplitToInt();

                for (var index = 0; index < pre.Count; index++)
                {
                    var p = (HeroPropertyType)pre[index];
                    var d = ExcelToJSONConfigManager.Current.GetConfigByID<RefreshPropertyValueData>(pre[index]);
                    if (d == null) continue;
                    if (values.ContainsKey(p))
                    {
                        values[p] += vals[index];
                    }
                    else
                    {
                        values.Add(p, vals[index]);
                    }
                }
            }

            if (values.Count == 0) return new G2C_RefreshEquip { Code = ErrorCode.NoPropertyToRefresh };

            if (refreshData.CostGold > player.Gold) return new G2C_RefreshEquip { Code = ErrorCode.NoEnoughtGold };
            var appendCount = GRandomer.RandomMinAndMax(refreshData.PropertyAppendCountMin, refreshData.PropertyAppendCountMax);

            while (appendCount > 0)
            {
                appendCount--;
                var property = GRandomer.RandomMinAndMax(refreshData.PropertyAppendMin, refreshData.PropertyAppendMax);
                var selected = GRandomer.RandomList(values.Keys.ToList());
                var val = ExcelToJSONConfigManager.Current.GetConfigByID<RefreshPropertyValueData>((int)selected);
                var appendValue = val.Value * property;
                if (equip.EquipData.Properties.ContainsKey(selected))
                {
                    equip.EquipData.Properties[selected] += appendValue;
                }
                else
                {
                    equip.EquipData.Properties.Add(selected, appendValue);
                }
            }

            if (!(await CostGold(player, refreshData.CostGold))) return new G2C_RefreshEquip { Code = ErrorCode.NoEnoughtGold };

            foreach (var i in customItem)
            {
                package.Items.Remove(i);
            }

            var filter = Builders<GamePackageEntity>.Filter.Eq(t => t.Uuid, package.Uuid);
            var update = Builders<GamePackageEntity>.Update.Set(t => t.Items, package.Items);
            await DataBase.S.Packages.UpdateOneAsync(filter, update);

            await SyncToClient(client, player.Uuid);
            return new G2C_RefreshEquip { Code = ErrorCode.Ok };
        }


    }
}

