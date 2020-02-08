using System;
using System.Threading;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.IdGenerators;
using MongoDB.Driver;
using Proto.LoginBattleGameServerService;
using Proto.MongoDB;
using ServerUtility;
using XNet.Libs.Net;
using XNet.Libs.Utility;
using Xunit;

namespace ServerUnitTest
{
    public class MongTest
    {

        static MongTest()
        {
           
        }

        private readonly string url = @"mongodb+srv://dbuser:54249636@cluster0-us8pa.gcp.mongodb.net/test?retryWrites=true&w=majority";

        [Fact]
        public  void TestAsync()
        {
            
        }


        [Theory]
        [InlineData("admin","123456")]
        [InlineData("test", "123456")]
        public void InsertEntity(string user, string passw)
        {
           
        }

        [Fact]
        public void RequestLoginServer()
        {
           
        }
    }
}
