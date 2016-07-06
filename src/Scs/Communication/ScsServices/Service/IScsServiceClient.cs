﻿using System;
using Hik.Communication.Scs.Communication;
using Hik.Communication.Scs.Communication.EndPoints;

namespace Hik.Communication.ScsServices.Service
{
    /// <summary>
    ///     Represents a client that uses a SDS service.
    /// </summary>
    public interface IScsServiceClient
    {
        /// <summary>
        ///     Unique identifier for this client.
        /// </summary>
        string ClientId { get; }

        /// <summary>
        ///     Gets endpoint of remote application.
        /// </summary>
        ScsEndPoint RemoteEndPoint { get; }

        /// <summary>
        ///     Gets the communication state of the Client.
        /// </summary>
        CommunicationStates CommunicationState { get; }

        /// <summary>
        ///     This event is raised when client is disconnected from service.
        /// </summary>
        event EventHandler Disconnected;

        /// <summary>
        ///     Closes client connection.
        /// </summary>
        void Disconnect();

        /// <summary>
        ///     Gets the client proxy interface that provides calling client methods remotely.
        /// </summary>
        /// <typeparam name="T">Type of client interface</typeparam>
        /// <returns>Client interface</returns>
        T GetClientProxy<T>() where T : class;
    }
}