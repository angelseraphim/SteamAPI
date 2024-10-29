using LiteDB;
using System;
using System.Collections.Generic;

namespace SteamSusAcc.DataBase
{
    public class Data
    {
        [Serializable]
        public class PlayerInfo
        {
            [BsonId]
            public string UserId { get; set; }
            public List<string> Nicknames { get; set; }
            public List<string> IPs { get; set; }
        }
    }
}