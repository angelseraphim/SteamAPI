using LiteDB;
using System;

namespace SteamSusAcc.DataBase
{
    public class Data
    {
        [Serializable]
        public class PlayerInfo
        {
            [BsonId]
            public string userId { get; set; }
        }
    }
}