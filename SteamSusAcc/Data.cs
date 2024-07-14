using Exiled.API.Features;
using LiteDB;
using System;
using static SteamSusAcc.Data;

namespace SteamSusAcc
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

    public static class Extensions
    {
        public static ILiteCollection<PlayerInfo> PlayerInfoCollection => Plugin.plugin.db.GetCollection<PlayerInfo>("SteamAPI");

        public static void InsertPlayer(string UserId)
        {
            PlayerInfo insert = new PlayerInfo()
            {
                userId = UserId,
            };
            PlayerInfoCollection.Insert(insert);
        }

        public static bool GetPlayer(string id, out PlayerInfo info)
        {
            info = PlayerInfoCollection.FindById(id);
            return info != null;
        }

        public static void DeletePlayer(string playerId)
        {
            if (!GetPlayer(playerId, out PlayerInfo info))
                return;

            PlayerInfoCollection.Delete(playerId);
        }
    }
}