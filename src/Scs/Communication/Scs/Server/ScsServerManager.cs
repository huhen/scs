namespace Hik.Communication.Scs.Server
{
    /// <summary>
    ///     Provides some functionality that are used by servers.
    /// </summary>
    internal static class ScsServerManager
    {
        /// <summary>
        ///     Used to set an auto incremential unique identifier to clients.
        /// </summary>
        private static ulong _lastClientId = 1;

        // ReSharper disable once FieldCanBeMadeReadOnly.Local
        private static object _lockIncrement = new object();

        /// <summary>
        ///     Gets an unique number to be used as idenfitier of a client.
        /// </summary>
        /// <returns></returns>
        public static ulong GetClientId()
        {
            ulong clientId;
            lock (_lockIncrement)
            {
                if (_lastClientId == ulong.MaxValue) _lastClientId = 1;
                clientId = _lastClientId;
                _lastClientId++;
            }
            return clientId;
        }
    }
}