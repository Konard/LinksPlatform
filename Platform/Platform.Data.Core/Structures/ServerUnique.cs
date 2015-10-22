using System;

namespace Platform.Links.DataBase.CoreUnsafe.Structures
{
    public static class ServerUnique
    {
        private static ServerType CurrentServerType = ServerType.Slave;
        private static int CurrentServerId = 0;
        private static uint CurrentItemCounter = 0;

        /// <summary>
        /// Создаёт семя серверного Юника (Unique).
        /// </summary>
        /// <param name="serverType">Тип сервера (Мастер|Раб).</param>
        /// <param name="serverId">Id сервера.</param>
        /// <param name="itemId">Id элемента.</param>
        /// <returns>Сформированное в ulong семя серверного Юника (Unique).</returns>
        /// <remarks>
        /// Seed Format For Server Unique
        /// 1 bit : Server Type (Master | Slave)
        /// 31 bit : Server Id (Cluster Server Address)
        /// 32 bit : Unique Id (Item Id, Link Id, Object Id & ...)
        /// </remarks>
        public static ulong CreateSeed(ServerType serverType, int serverId, uint itemId)
        {
            if (serverId <= 0)
                throw new ArgumentOutOfRangeException("serverId", "Invalid Server Id.");

            ulong seed = 0;

            if (serverType == ServerType.Master)
                return seed &= 0x8000000000000000;

            seed &= (uint) serverId;
            seed &= (ulong) itemId >> 32;

            return seed;
        }

        public static void ExtractSeed(ulong seed, out ServerType serverType, out int serverId, out ulong itemId)
        {
            throw new NotImplementedException();
        }

        public static Unique CreateUnique()
        {
            throw new NotImplementedException();
        }

        public static void SetServerType(ServerType serverType)
        {
            CurrentServerType = serverType;
        }

        public static void SetCurrentServerId(int serverId)
        {
            CurrentServerId = serverId;
        }
    }
}