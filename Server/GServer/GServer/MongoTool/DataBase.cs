using System.Collections.Generic;
using Google.Protobuf.Collections;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson.Serialization.IdGenerators;
using MongoDB.Bson.Serialization.Options;
using MongoDB.Bson.Serialization.Serializers;
using MongoDB.Driver;
using Proto.MongoDB;
using ServerUtility;

namespace GateServer
{
    public class DataBase : XSingleton<DataBase>
    {

        public class GamePackageEntity
        {
            [BsonId(IdGenerator = typeof(StringObjectIdGenerator))]
            [BsonElement("id")]
            public string Uuid { set; get; }
            [BsonDictionaryOptions(DictionaryRepresentation.ArrayOfDocuments)]
            [BsonElement("item")]
            public Dictionary<string, ItemNum> Items { set; get; }
            [BsonElement("size")]
            public int PackageSize { set; get; }
            [BsonElement("puuid")]
            public string PlayerUuid { set; get; }

            public GamePackageEntity()
            {
                Items = new Dictionary<string, ItemNum>();
            }

        }

        public class GameHeroEntity
        {
            [BsonId(IdGenerator = typeof(StringObjectIdGenerator))]
            [BsonElement("_id")]
            public string Uuid { set; get; }
            public string PlayerUuid { set; get; }
            public int Exp { set; get; }
            public int Level { set; get; }
            [BsonDictionaryOptions(DictionaryRepresentation.ArrayOfDocuments)]
            public Dictionary<int, HeroMagic> Magics { set; get; }
            [BsonDictionaryOptions(DictionaryRepresentation.ArrayOfDocuments)]
            public Dictionary<int, string> Equips { set; get; }
            public string HeroName { set; get; }
            public int HeroId { set; get; }
            public GameHeroEntity()
            {
                Magics = new Dictionary<int, HeroMagic>();
                Equips = new Dictionary<int, string>();
            }
        }

        public class GameWareroom
        {
            public int Size { set; get; }
            [BsonId(IdGenerator = typeof(StringObjectIdGenerator))]
            [BsonElement("id")]
            public string Uuid { set; get; }
            [BsonDictionaryOptions(DictionaryRepresentation.ArrayOfDocuments)]
            [BsonElement("item")]
            public Dictionary<string, ItemNum> Items { set; get; }
            [BsonElement("puuid")]
            public string PlayerUuid { set; get; }
        }

        public DataBase()
        {
            BsonClassMap.RegisterClassMap<GamePlayerEntity>(
            (cm) =>
            {
                cm.AutoMap();
                cm.MapIdMember(c => c.Uuid).SetIdGenerator(StringObjectIdGenerator.Instance);
            });
         
        }

        public const string PLAYER = "Player";
        public const string HERO = "Hero";
        public const string PACKAGE = "Package";
        public const string WEARROOM = "Wareroom";

        public IMongoCollection<GamePlayerEntity> Playes { get; private set; }
        public IMongoCollection<GameHeroEntity> Heros { get; private set; }
        public IMongoCollection<GamePackageEntity> Packages { get; private set; }
        public IMongoCollection<GameWareroom> Warerooms { get; private set; }

        public void Init(string connectString, string dbName)
        {
            var mongo = new MongoClient(connectString);
            var db = mongo.GetDatabase(dbName);
            Playes = db.GetCollection<GamePlayerEntity>(PLAYER);
            Heros = db.GetCollection<GameHeroEntity>(HERO);
            Packages = db.GetCollection<GamePackageEntity>(PACKAGE);
            Warerooms = db.GetCollection<GameWareroom>(WEARROOM);
        }
    }
}
