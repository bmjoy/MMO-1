using System.Collections.Generic;
using EConfig;
using Proto;
using Proto.MongoDB;
using static GateServer.DataBase;
using static Proto.ItemsShop.Types;

namespace GateServer
{
    public static class ProtoExtends
    {
        public static DHero ToDhero(this GameHeroEntity entity, GamePackageEntity package)
        {
            var h = new DHero
            {
                Exprices = entity.Exp,
                HeroID = entity.HeroId,
                Level = entity.Level,
                Name = entity.HeroName
            };


            foreach (var i in entity.Magics)
            {
                h.Magics.Add(new Proto.HeroMagic { MagicKey = i.Key, Level = i.Value.Level });
            }

            foreach (var i in entity.Equips)
            {
                if (package.Items.TryGetValue(i.Value, out ItemNum item))
                {
                    h.Equips.Add(new WearEquip
                    {
                        Part = (EquipmentType)i.Key,
                        GUID = i.Value,
                        ItemID = item.Id
                    });
                }
            }

            return h;
        }

        public static PlayerPackage ToPackage(this GamePackageEntity entity)
        {
            var p = new PlayerPackage { MaxSize = entity.PackageSize };
            if (entity.Items != null)
            {
                foreach (var i in entity.Items)
                {
                    p.Items.Add(i.Key,new PlayerItem
                    {
                        GUID = i.Key,
                        ItemID = i.Value.Id,
                        Locked = i.Value.IsLock,
                        Num = i.Value.Num,
                        Level = i.Value.Level
                    });
                }
            }

            return p;
        }

        public static IList<int> SplitToInt(this string str, char sKey = '|')
        {
            var arrs = str.Split(sKey);
            var list = new List<int>();
            foreach (var i in arrs) list.Add(int.Parse(i));
            return list;
        }


        public static ItemsShop ToItemShop(this ItemShopData config)
        {
            var shop = new ItemsShop { ShopId = config.ID };
            var items = config.ItemIds.SplitToInt();
            var nums = config.ItemNums.SplitToInt();
            var prices = config.ItemPrices.SplitToInt();
            var coinType = config.CoinTypes.SplitToInt();
            for (var index = 0; index < items.Count; index++)
            {
                shop.Items.Add(new ShopItem
                {
                    CType = (CoinType)coinType[index],
                    ItemId = items[index],
                    PackageNum = nums[index],
                    Prices = prices[index]
                });
            }

            return shop;
        }
    }
}
