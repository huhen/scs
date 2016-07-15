﻿using Hik.Communication.Scs.Communication.EndPoints;

namespace Hik.Communication.Scs.Client
{
    /// <summary>
    ///     This class is used to create SCS Clients to connect to a SCS server.
    /// </summary>
    public static class ScsClientFactory
    {
        /// <summary>
        ///     Creates a new client to connect to a server using an end point.
        /// </summary>
        /// <param name="endpoint">End point of the server to connect it</param>
        /// <returns>Created TCP client</returns>
        public static IScsClient CreateClient(ScsEndPoint endpoint)
        {
            return endpoint.CreateClient();
        }

        /// <summary>
        ///     Creates a new client to connect to a server using an end point.
        /// </summary>
        /// <param name="endpointAddress">End point address of the server to connect it</param>
        /// <returns>Created TCP client</returns>
        public static IScsClient CreateClient(string endpointAddress)
        {
            return CreateClient(ScsEndPoint.CreateEndPoint(endpointAddress));
        }


        /// <summary>
        ///     SSL
        /// </summary>
        /// <param name="endpoint"></param>
        /// <param name="nombreServerCert"></param>
        /// <returns></returns>
        public static IScsClient CreateSecureClient(ScsEndPoint endpoint, string nombreServerCert)
        {
            return endpoint.CreateSecureClient(nombreServerCert);
        }

        /// <summary>
        ///     SSL
        /// </summary>
        /// <param name="endpointAddress"></param>
        /// <returns></returns>
        public static IScsClient CreateSecureClient(string endpointAddress)
        {
            return CreateClient(ScsEndPoint.CreateEndPoint(endpointAddress));
        }
    }
}