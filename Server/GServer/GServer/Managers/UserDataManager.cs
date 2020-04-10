using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using EConfig;
using ExcelConfig;
using GateServer;
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

        public async Task<string> ProcessBattleReward(string account_uuid,IList<PlayerItem> modifyItems,  IList<PlayerItem> RemoveItems, int exp, int level, int gold)
        {
            var player = await FindPlayerByAccountId(account_uuid);
            if (player == null) return null;
            var pupdate = Builders<GamePlayerEntity>.Update.Inc(t =>t.Gold, gold);
            await DataBase.S.Playes.UpdateOneAsync(t => t.Uuid== player.Uuid, pupdate);
            var (ms,rs) = await ProcessRewardItem(player.Uuid, modifyItems,  RemoveItems);
            var hero = await FindHeroByPlayerId(player.Uuid);
            var update = Builders<GameHeroEntity>.Update.Set(t => t.Exp, exp).Set(t => t.Level, level);
            var filter = Builders<GameHeroEntity>.Filter.Eq(t => t.Uuid, hero.Uuid);
            await DataBase.S.Heros.UpdateOneAsync(filter, update);
            return  player.Uuid;
        }

        internal async Task SyncToClient(Client userClient, string playerUuid, bool syncPlayer = false, bool syncPackage = false)
        {
            var player = await FindPlayerById(playerUuid);
            var p = await FindPackageByPlayerID(playerUuid);
            if (syncPackage)
            {
                //var p = await FindPackageByPlayerID(playerUuid);
                var pack = new Task_G2C_SyncPackage
                {
                    Coin = player.Coin,
                    Gold = player.Gold,
                    Package = p.ToPackage()
                };
                userClient.CreateTask(pack).Send();
            }

            if (syncPlayer)
            {
                var h = (await FindHeroByPlayerId(playerUuid)).ToDhero(p);
                var hTask = new Task_G2C_SyncHero
                {
                    Hero = h
                };
                userClient.CreateTask(hTask).Send();
            }
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

                 await DataBase.S.Playes.FindOneAndUpdateAsync(t => t.Uuid== player.Uuid,
                     Builders<GamePlayerEntity>.Update.Inc(t => t.Coin, - Application.Constant.PACKAGE_BUY_COST));
                SyncCoinAndGold(client, player.Coin - Application.Constant.PACKAGE_BUY_COST, player.Gold);
            }

            {
                await DataBase.S.Packages.FindOneAndUpdateAsync(t => t.Uuid== package.Uuid,
                    Builders<GamePackageEntity>.Update.Inc(t=>t.PackageSize, Application.Constant.PACKAGE_BUY_SIZE)
                    );
            }

            client
                .CreateTask(new Task_PackageSize { Size = package.PackageSize + Application.Constant.PACKAGE_BUY_SIZE })
                .Send();

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

            if (!package.TryGetItem(item_uuid, out PackageItem item))
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
                var update = Builders<GamePlayerEntity>.Update.Inc(t => t.Gold, -levelconfig.CostGold);
                await DataBase.S.Playes.FindOneAndUpdateAsync(filter, update);
            }

            if (levelconfig.CostCoin > 0)
            {
                player.Coin -= levelconfig.CostCoin;
                var update = Builders<GamePlayerEntity>.Update.Inc(t => t.Coin, -levelconfig.CostCoin);
                await DataBase.S.Playes.FindOneAndUpdateAsync(filter, update);
            }

            if (GRandomer.Probability10000(levelconfig.Pro))
            {
                item.Level += 1;
                var update = Builders<GamePackageEntity>.Update.Set(t=>t.Items[-1].Level, item.Level);
                var f = Builders<GamePackageEntity>.Filter.Eq(t => t.PlayerUuid, player.Uuid)
                    & Builders<GamePackageEntity>.Filter.ElemMatch(t => t.Items, x => x.Uuid == item.Uuid);
                await DataBase.S.Packages.FindOneAndUpdateAsync(f, update);
                SyncModifyItems(client, new [] { item.ToPlayerItem() });
            }

            SyncCoinAndGold(client, player.Coin, player.Gold);
            return new G2C_EquipmentLevelUp { Code = ErrorCode.Ok, Level = item.Level };
        }

      

        private void SyncModifyItems(Client userClient, PlayerItem[] modifies, PlayerItem[] removes =null)
        {
            var task = new Task_ModifyItem();
            if (modifies != null)
            {
                foreach (var i in modifies)
                {
                    task.ModifyItems.Add(i);
                }
            }
            if (removes != null)
            {
                foreach (var i in removes)
                {
                    task.RemoveItems.Add(i);
                }
            }
            userClient.CreateTask(task).Send();
        }

        private void SyncCoinAndGold(Client userClient, int coin, int gold)
        {
            var task = new Task_CoinAndGold { Coin = coin, Gold = gold };
            userClient.CreateTask(task).Send();
        }

        internal async Task<G2C_BuyGold> BuyGold(Client client, string accountUuid, int shopId)
        {
            var item = ExcelToJSONConfigManager.Current.GetConfigByID<GoldShopData>(shopId);
            if (item == null) return new G2C_BuyGold { Code = ErrorCode.NofoundItem };
            var player = await FindPlayerByAccountId(accountUuid);
            if (player.Coin < item.Prices) return new G2C_BuyGold { Code = ErrorCode.NoEnoughtCoin };

            var update = Builders<GamePlayerEntity>.Update
                .Inc(t => t.Coin, - item.Prices)
                .Inc(t => t.Gold,  item.ReceiveGold);
            await DataBase.S.Playes.FindOneAndUpdateAsync(t=>t.Uuid == player.Uuid, update);

            SyncCoinAndGold(client, player.Coin - item.Prices, player.Gold + item.ReceiveGold);

            return new G2C_BuyGold
            {
                Code = ErrorCode.Ok,
                Coin = player.Coin - item.Prices,
                Gold = player.Gold + item.ReceiveGold,
                ReceivedGold = item.ReceiveGold
            };
        }
        //todo
        internal async Task<G2C_ActiveMagic> ActiveMadic(Client client, string accountUuid, int magicId)
        {
            var player = await FindPlayerByAccountId(accountUuid);
            var hero = await FindHeroByPlayerId(player.Uuid);
            var config = ExcelToJSONConfigManager.Current.GetConfigByID<CharacterMagicData>(magicId);
            if (config.CharacterID != hero.HeroId) return new G2C_ActiveMagic { Code = ErrorCode.Error };
            if (!hero.Magics.TryGetValue(magicId, out DBHeroMagic magic))
            {
                magic = new DBHeroMagic { Actived = true, Exp = 0, Level = 1 };
                hero.Magics.Add(magicId, magic);
                var update = Builders<GameHeroEntity>.Update.Set(t => t.Magics, hero.Magics);
                await DataBase.S.Heros.UpdateOneAsync(t => t.Uuid == hero.Uuid, update);
                await SyncToClient(client, player.Uuid, true);
                return new G2C_ActiveMagic { Code = ErrorCode.Ok };
            }
            else {
                return new G2C_ActiveMagic { Code = ErrorCode.Error };
            }

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

           
            var models = new List<WriteModel<GameHeroEntity>>();

            if (!hero.Magics.TryGetValue(magicId, out DBHeroMagic magic))
            {
                // magic = new DBHeroMagic { Actived = true, Exp = 0, Level = 0 };
                //hero.Magics.Add(magicId, magic);
                return new G2C_MagicLevelUp { Code = ErrorCode.MagicNoActicted };
            }

            if (levelConfig.NeedGold > 0)
            {
                player.Gold -= levelConfig.NeedGold;
                var update = Builders<GamePlayerEntity>.Update.Inc(t => t.Gold, -levelConfig.NeedGold);
                await DataBase.S.Playes.UpdateOneAsync(t => t.Uuid == player.Uuid, update);
                SyncCoinAndGold(client, player.Coin, player.Gold);
            }

            magic.Level += 1;

            {
               
                var update = Builders<GameHeroEntity>.Update.Set(t=>t.Magics, hero.Magics);
                await DataBase.S.Heros.UpdateOneAsync(t=>t.Uuid == hero.Uuid, update);
            }

            await SyncToClient(client, player.Uuid,true);

            return new G2C_MagicLevelUp
            {
                Code = ErrorCode.Ok
            };
        }

        internal async Task<ErrorCode> BuyItem(Client c, string acount_id, ShopItem item)
        {
            var player = await FindPlayerByAccountId(acount_id);
            var id = Builders<GamePlayerEntity>.Filter.Eq(t => t.Uuid, player.Uuid);
   
            if (item.CType == CoinType.Coin)
            {
                if (item.Prices > player.Coin) return ErrorCode.NoEnoughtCoin;

                await DataBase.S.Playes.FindOneAndUpdateAsync(t => t.Uuid == player.Uuid,
                    Builders<GamePlayerEntity>.Update.Inc(t => t.Coin, -item.Prices));
                player.Coin -= item.Prices;
            }
            else if (item.CType == CoinType.Gold)
            {
                if (item.Prices > player.Gold) return ErrorCode.NoEnoughtGold;
                await DataBase.S.Playes.FindOneAndUpdateAsync(t => t.Uuid == player.Uuid,
                  Builders<GamePlayerEntity>.Update.Inc(t => t.Gold, -item.Prices));
                player.Gold -= item.Prices;
            }
            
            var (modifies, add) = await AddItems(player.Uuid, new PlayerItem { ItemID = item.ItemId, Num = item.PackageNum });

            var items = new List<PlayerItem>();
            foreach (var i in modifies) items.Add(i.ToPlayerItem());
            foreach(var i in  add) items.Add(i.ToPlayerItem());

            SyncModifyItems(c, items.ToArray());
            SyncCoinAndGold(c, player.Coin, player.Gold);
            return ErrorCode.Ok;

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

                if (!p.TryGetItem(i.Guid, out PackageItem item))
                    return new G2C_SaleItem { Code = ErrorCode.NofoundItem };
                if (item.Num < i.Num)
                    return new G2C_SaleItem { Code = ErrorCode.NoenoughItem };
                if (item.IsLock)
                    return new G2C_SaleItem { Code = ErrorCode.Error };
            }

            var total = 0;
            var removes = new List<PackageItem>();
            var modify = new List<PackageItem>();

            var models = new List<WriteModel<GamePackageEntity>>();

            foreach (var i in items)
            {
                if (p.TryGetItem(i.Guid, out PackageItem item))
                {
                    var config = ExcelToJSONConfigManager.Current.GetConfigByID<ItemData>(item.Id);
                    if (config.SalePrice > 0) total += config.SalePrice * i.Num;
                    item.Num -= i.Num;
                    if (item.Num == 0)
                    {
                        removes.Add(item);
                        models.Add(new UpdateOneModel<GamePackageEntity>(
                            Builders<GamePackageEntity>.Filter.Eq(t=>t.Uuid, p.Uuid),
                            Builders<GamePackageEntity>.Update.PullFilter(t=>t.Items,x=>x.Uuid ==item.Uuid))
                            );
                    }
                    else
                    {
                        modify.Add(item);
                        models.Add(new UpdateOneModel<GamePackageEntity>(
                        Builders<GamePackageEntity>.Filter.Eq(t => t.Uuid, p.Uuid) &
                        Builders<GamePackageEntity>.Filter.ElemMatch(t => t.Items, x => x.Uuid == item.Uuid),
                        Builders<GamePackageEntity>.Update.Set(t => t.Items[-1].Num, item.Num))
                        );
                    }
                }
            }

            pl.Gold += total;


            var u_player = Builders<GamePlayerEntity>.Update.Inc(t => t.Gold,total);
            await DataBase.S.Playes.FindOneAndUpdateAsync(t=>t.Uuid == pl.Uuid, u_player);
            await DataBase.S.Packages.BulkWriteAsync(models);

            SyncModifyItems(client, modify.Select(t=>t.ToPlayerItem()).ToArray(), removes.Select(t => t.ToPlayerItem()).ToArray());
            SyncCoinAndGold(client, pl.Coin, pl.Gold);
            return new G2C_SaleItem { Code = ErrorCode.Ok, Coin = pl.Coin, Gold = pl.Gold };

        }

        internal async Task<bool> OperatorEquip(string player_uuid, string equip_uuid, EquipmentType part, bool isWear)
        {
            var h_filter = Builders<GameHeroEntity>.Filter.Eq(t => t.PlayerUuid, player_uuid);

            var hero = (await DataBase.S.Heros.FindAsync(h_filter)).SingleOrDefault();
            if (hero == null) return false;

            var pa_filter = Builders<GamePackageEntity>.Filter.Eq(t => t.PlayerUuid, player_uuid);
            var package = (await DataBase.S.Packages.FindAsync(pa_filter)).Single();

            if (!package.TryGetItem(equip_uuid, out PackageItem item)) return false;

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

        private async Task<Tuple<IList<PlayerItem>, IList<PlayerItem>>> ProcessRewardItem(string player_uuid, IList<PlayerItem> modify, IList<PlayerItem> removes)
        {
            var pa_filter = Builders<GamePackageEntity>.Filter.Eq(t => t.PlayerUuid, player_uuid);
            var package = (await DataBase.S.Packages.FindAsync(pa_filter)).Single();

            var models = new List<WriteModel<GamePackageEntity>>();
            foreach (var i in modify)
            {
                if (package.TryGetItem(i.GUID, out PackageItem item))
                {
                    var m = i.ToPackageItem();
                    models.Add(new UpdateOneModel<GamePackageEntity>(
                        Builders<GamePackageEntity>.Filter.Eq(t => t.Uuid, package.Uuid)
                        & Builders<GamePackageEntity>.Filter.ElemMatch(t => t.Items, x => x.Uuid == item.Uuid),
                        Builders<GamePackageEntity>.Update.Set(t => t.Items[-1], m)
                        ));
                }
                else 
                {
                    models.Add(new UpdateOneModel<GamePackageEntity>(
                       Builders<GamePackageEntity>.Filter.Eq(t => t.Uuid, package.Uuid),
                       Builders<GamePackageEntity>.Update.Push(t => t.Items, i.ToPackageItem())
                       ));
                }
            }

            foreach (var i in removes)
            {
                models.Add(new UpdateOneModel<GamePackageEntity>(
                          Builders<GamePackageEntity>.Filter.Eq(t => t.Uuid, package.Uuid),
                          Builders<GamePackageEntity>.Update.PullFilter(t => t.Items, t=>t.Uuid == i.GUID)));
            }

            await DataBase.S.Packages.BulkWriteAsync(models);

            return Tuple.Create(modify,removes);
        }

        private PackageItem GetCanStackItem(int itemID, GamePackageEntity package)
        {
            var it = ExcelToJSONConfigManager.Current.GetConfigByID<ItemData>(itemID);
            foreach (var i in package.Items)
            {
                if (i.Id == itemID)
                {
                    if (i.Num < it.MaxStackNum) return i;
                }
            }
            return null;
        }

        public async Task<Tuple<List<PackageItem>,List< PackageItem>>> AddItems(string uuid, PlayerItem i)
        {
            if (i.Num <= 0) return null;
            GamePackageEntity package = await FindPackageByPlayerID(uuid);
            var it = ExcelToJSONConfigManager.Current.GetConfigByID<ItemData>(i.ItemID);
            if (it == null) return null;
            var num = i.Num;
            var modifies = new List<PackageItem>();
            var adds = new List<PackageItem>();
           
            while (num > 0)
            {
                var cItem = GetCanStackItem(i.ItemID, package);
                if (cItem != null)
                {
                    var remainNum = it.MaxStackNum - cItem.Num;
                    var add = Math.Min(remainNum, num);
                    cItem.Num += add;
                    num -= add;
                    modifies.Add(cItem);
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
                    adds.Add(itemNum);
                }
            }


            var models = new List<WriteModel<GamePackageEntity>>();
            foreach (var a in adds) 
            {
                var push = new UpdateOneModel<GamePackageEntity>(
                    Builders<GamePackageEntity>.Filter.Eq(t => t.PlayerUuid, uuid),
                    Builders<GamePackageEntity>.Update.Push(t => t.Items, a));
                models.Add(push);
            }

            foreach (var m in modifies)
            {
                var b = Builders<GamePackageEntity>.Filter;
                models.Add(new UpdateOneModel<GamePackageEntity>(
                    b.Eq(t => t.PlayerUuid, uuid) & b.ElemMatch(t => t.Items, x => x.Uuid == m.Uuid),
                    Builders<GamePackageEntity>.Update.Set(t => t.Items[-1], m))
                    );
            }

            await DataBase.S.Packages.BulkWriteAsync(models);

            return  Tuple.Create(modifies, adds);
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

            if (!package.TryGetItem(equipUuid, out PackageItem equip)) return new G2C_RefreshEquip { Code = ErrorCode.NofoundItem };

            var config = ExcelToJSONConfigManager.Current.GetConfigByID<ItemData>(equip.Id);
            if (config == null) return new G2C_RefreshEquip { Code = ErrorCode.Error };
            var refreshData = ExcelToJSONConfigManager.Current.GetConfigByID<EquipRefreshData>(config.Quality);
            int refreshCount = equip.EquipData?.RefreshCount ?? 0;
            if (refreshData.MaxRefreshTimes < refreshCount) return new G2C_RefreshEquip { Code = ErrorCode.RefreshTimeLimit };
            if (refreshData.NeedItemCount > customItem.Count) return new G2C_RefreshEquip { Code = ErrorCode.NoenoughItem };
            Dictionary<HeroPropertyType, int> values = new Dictionary<HeroPropertyType, int>();
            var removes = new List<PackageItem>();
            foreach (var i in customItem)
            {
                if (!package.TryGetItem(i, out PackageItem custom)) return new G2C_RefreshEquip { Code = ErrorCode.NofoundItem };
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
                removes.Add(custom);
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

            equip.EquipData.RefreshCount++;

            var models = new List<WriteModel<GamePackageEntity>>();

            var modify = new UpdateOneModel<GamePackageEntity>(
                     Builders<GamePackageEntity>.Filter.Eq(t => t.Uuid, package.Uuid)
                     & Builders<GamePackageEntity>.Filter.ElemMatch(t => t.Items, c=>c.Uuid == equip.Uuid),
                      Builders<GamePackageEntity>.Update.Set(t => t.Items[-1], equip)
                );
            models.Add(modify);

            foreach (var i in removes)
            {
                var delete = new UpdateOneModel<GamePackageEntity>(
                    Builders<GamePackageEntity>.Filter.Eq(t => t.Uuid, package.Uuid),
                    Builders<GamePackageEntity>.Update.PullFilter(t => t.Items, t=>t.Uuid == i.Uuid)
                    );
                models.Add(delete);
            }


            await DataBase.S.Packages.BulkWriteAsync(models);
            await DataBase.S.Playes.FindOneAndUpdateAsync(p => p.Uuid == player.Uuid,
                 Builders<GamePlayerEntity>.Update.Inc(t => t.Gold, -refreshData.CostGold));

            SyncCoinAndGold(client, player.Coin, player.Gold - refreshData.CostGold);
            SyncModifyItems(client, new[] { equip.ToPlayerItem() }, removes.Select(t => t.ToPlayerItem()).ToArray());
            return new G2C_RefreshEquip { Code = ErrorCode.Ok };
        }
    }
}

