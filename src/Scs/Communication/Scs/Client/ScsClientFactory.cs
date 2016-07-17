using Hik.Communication.Scs.Communication.EndPoints;

namespace Hik.Communication.Scs.Client
{
    /// <summary>
    ///     This class is used to create SCS Clients to connect to a SCS server.
    /// </summary>
    public static class ScsClientFactory
    {
        /// <summary>
        ///     SSL
        /// </summary>
        /// <param name="endpoint"></param>
        /// <param name="publicKey"></param>
        /// <returns></returns>
        public static IScsClient CreateSecureClient(ScsEndPoint endpoint, byte[] publicKey)
        {
            return endpoint.CreateSecureClient(publicKey);
        }
    }
}