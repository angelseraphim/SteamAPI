using Exiled.API.Features;
using LiteDB;
using static SteamSusAcc.DataBase.Data;

namespace SteamSusAcc.DataBase
{
    public static class Extensions
    {
        public static ILiteCollection<PlayerInfo> PlayerInfoCollection => Plugin.plugin.db.GetCollection<PlayerInfo>($"SteamAPI{Server.Port}");

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
