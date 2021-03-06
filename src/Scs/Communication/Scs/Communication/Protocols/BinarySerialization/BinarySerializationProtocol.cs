﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters;
using System.Runtime.Serialization.Formatters.Binary;
using System.Security.Cryptography;
using Hik.Communication.Scs.Communication.Messages;
using Ionic.Zlib;

namespace Hik.Communication.Scs.Communication.Protocols.BinarySerialization
{
    /// <summary>
    ///     Default communication protocol between server and clients to send and receive a message.
    ///     It uses .NET binary serialization to write and read messages.
    ///     A Message format:
    ///     [Message Length (4 bytes)][Serialized Message Content]
    ///     If a message is serialized to byte array as N bytes, this protocol
    ///     adds 4 bytes size information to head of the message bytes, so total length is (4 + N) bytes.
    ///     This class can be derived to change serializer (default: BinaryFormatter). To do this,
    ///     SerializeMessage and DeserializeMessage methods must be overrided.
    /// </summary>
    public class BinarySerializationProtocol : IScsWireProtocol
    {
        private Aes _aes;

        #region Constructor

        /// <summary>
        ///     Creates a new instance of BinarySerializationProtocol.
        /// </summary>
        public BinarySerializationProtocol()
        {
            _ms = new MemoryStream();
        }

        #endregion

        #region Nested classes

        /// <summary>
        ///     This class is used in deserializing to allow deserializing objects that are defined
        ///     in assemlies that are load in runtime (like PlugIns).
        /// </summary>
        protected sealed class DeserializationAppDomainBinder : SerializationBinder
        {
            /// <summary>
            ///     BindToType
            /// </summary>
            /// <param name="assemblyName"></param>
            /// <param name="typeName"></param>
            /// <returns></returns>
            public override Type BindToType(string assemblyName, string typeName)
            {
                var toAssemblyName = assemblyName.Split(',')[0];
                return (from assembly in AppDomain.CurrentDomain.GetAssemblies()
                        where assembly.FullName.Split(',')[0] == toAssemblyName
                        select assembly.GetType(typeName)).FirstOrDefault();
            }
        }

        #endregion

        #region Private fields

        /// <summary>
        ///     Maximum length of a message.
        /// </summary>
        private const int _maxMessageLength = 128 * 1024 * 1024; //128 Megabytes.

        /// <summary>
        ///     This MemoryStream object is used to collect receiving bytes to build messages.
        /// </summary>
        private MemoryStream _ms;

        #endregion

        #region IScsWireProtocol implementation

        /// <summary>
        ///     Serializes a message to a byte array to send to remote application.
        ///     This method is synchronized. So, only one thread can call it concurrently.
        /// </summary>
        /// <param name="message">Message to be serialized</param>
        /// <exception cref="CommunicationException">
        ///     Throws CommunicationException if message is bigger than maximum allowed
        ///     message length.
        /// </exception>
        public byte[] GetBytes(IScsMessage message)
        {
            //Serialize the message to a byte array
            var serializedMessage = SerializeMessage(message);

            //Check for message length
            var msgLength = serializedMessage.Length;
            if (msgLength > _maxMessageLength)
            {
                throw new CommunicationException("Message is too big (" + msgLength +
                                                 " bytes). Max allowed length is " + _maxMessageLength + " bytes.");
            }

            //Create a byte array including the length of the message (4 bytes) and serialized message content
            var bytes = new byte[msgLength + 4];
            WriteInt32(bytes, 0, msgLength);
            Array.Copy(serializedMessage, 0, bytes, 4, msgLength);

            //Return serialized message by this protocol
            return bytes;
        }

        /// <summary>
        ///     Builds messages from a byte array that is received from remote application.
        ///     The Byte array may contain just a part of a message, the protocol must
        ///     cumulate bytes to build messages.
        ///     This method is synchronized. So, only one thread can call it concurrently.
        /// </summary>
        /// <param name="buffer">Received bytes from remote application</param>
        /// <param name="length">Length of received bytes from remote application</param>
        /// <returns>
        ///     List of messages.
        ///     Protocol can generate more than one message from a byte array.
        ///     Also, if received bytes are not sufficient to build a message, the protocol
        ///     may return an empty list (and save bytes to combine with next method call).
        /// </returns>
        public IEnumerable<IScsMessage> CreateMessages(byte[] buffer, int length)
        {
            //Write all received bytes to the _receiveMemoryStream
            _ms.Write(buffer, 0, length);
            //Create a list to collect messages
            var messages = new List<IScsMessage>();
            //Read all available messages and add to messages collection
            while (ReadSingleMessage(messages))
            {
            }
            //Return message list
            return messages;
        }

        /// <summary>
        ///     This method is called when connection with remote application is reset (connection is renewing or first
        ///     connecting).
        ///     So, wire protocol must reset itself.
        /// </summary>
        public void Reset()
        {
            if (_ms.Length > 0)
            {
                _ms.Dispose();
                _ms = new MemoryStream();
            }
        }

        /// <summary>
        /// SetAes
        /// </summary>
        /// <param name="aes"></param>
        public void SetAes(Aes aes)
        {
            _aes = aes;
        }

        #endregion

        #region Proptected virtual methods

        /// <summary>
        ///     This method is used to serialize a IScsMessage to a byte array.
        ///     This method can be overrided by derived classes to change serialization strategy.
        ///     It is a couple with DeserializeMessage method and must be overrided together.
        /// </summary>
        /// <param name="message">Message to be serialized</param>
        /// <returns>
        ///     Serialized message bytes.
        ///     Does not include length of the message.
        /// </returns>
        protected virtual byte[] SerializeMessage(IScsMessage message)
        {
            using (var memoryStream = new MemoryStream())
            {
                new BinaryFormatter().Serialize(memoryStream, message);
                var data = memoryStream.ToArray();
                var compData = ZlibCodecCompress(data);

                if (_aes != null)
                {
                    //using (var msEncrypt = new MemoryStream())
                    //{
                    using (var encryptor = _aes.CreateEncryptor(_aes.Key, _aes.IV))
                    {
                        return encryptor.TransformFinalBlock(compData, 0, compData.Length);
                    }
                    //using (var csEncrypt = new CryptoStream(msEncrypt, encryptor, CryptoStreamMode.Write))
                    //{
                    //    csEncrypt.Write(compData, 0, compData.Length);
                    //}
                    //return msEncrypt.ToArray();
                    //}
                }
                return compData;
            }
        }


        private static byte[] ZlibCodecCompress(byte[] uncompressedBytes)
        {
            const int bufferSize = 1024;
            ZlibCodec compressor = new ZlibCodec();

            using (var ms = new MemoryStream())
            {
                int rc = compressor.InitializeDeflate(CompressionLevel.BestCompression, false);
                if (rc != ZlibConstants.Z_OK) throw new ZlibException("Cannot initialize for deflate.");
                compressor.InputBuffer = uncompressedBytes;
                compressor.NextIn = 0;
                compressor.AvailableBytesIn = uncompressedBytes.Length;

                compressor.OutputBuffer = new byte[bufferSize];

                // pass 1: deflate 
                do
                {
                    compressor.NextOut = 0;
                    compressor.AvailableBytesOut = bufferSize;
                    rc = compressor.Deflate(FlushType.None);

                    if (rc != ZlibConstants.Z_OK && rc != ZlibConstants.Z_STREAM_END)
                        throw new Exception("deflating: " + compressor.Message);

                    ms.Write(compressor.OutputBuffer, 0, bufferSize - compressor.AvailableBytesOut);
                }
                while (compressor.AvailableBytesIn != 0 || compressor.AvailableBytesOut == 0);

                // pass 2: finish and flush
                do
                {
                    compressor.NextOut = 0;
                    compressor.AvailableBytesOut = bufferSize;
                    rc = compressor.Deflate(FlushType.Finish);

                    if (rc != ZlibConstants.Z_STREAM_END && rc != ZlibConstants.Z_OK)
                        throw new Exception("deflating: " + compressor.Message);

                    if (bufferSize - compressor.AvailableBytesOut != 0)
                        ms.Write(compressor.OutputBuffer, 0, bufferSize - compressor.AvailableBytesOut);
                }
                while (compressor.AvailableBytesIn != 0 || compressor.AvailableBytesOut == 0);

                compressor.EndDeflate();
                return ms.ToArray();
            }
        }

        /// <summary>
        ///     This method is used to deserialize a IScsMessage from it's bytes.
        ///     This method can be overrided by derived classes to change deserialization strategy.
        ///     It is a couple with SerializeMessage method and must be overrided together.
        /// </summary>
        /// <param name="bytes">
        ///     Bytes of message to be deserialized (does not include message length. It consist
        ///     of a single whole message)
        /// </param>
        /// <returns>Deserialized message</returns>
        protected virtual IScsMessage DeserializeMessage(byte[] bytes)
        {
            if (_aes != null)
            {
                //using (var msDecrypt = new MemoryStream(bytes))
                using (var decryptor = _aes.CreateDecryptor(_aes.Key, _aes.IV))
                {
                    bytes = decryptor.TransformFinalBlock(bytes, 0, bytes.Length);
                }
                //using (var csDecrypt = new CryptoStream(msDecrypt, decryptor, CryptoStreamMode.Read))
                /*using (var ms = new MemoryStream())
                {
                    var buffer = new byte[1024];
                    var br = csDecrypt.Read(buffer, 0, 1024);
                    while (br > 0)
                    {
                        ms.Write(buffer, 0, br);
                        br = csDecrypt.Read(buffer, 0, 1024);
                    }
                    bytes = ms.ToArray();
                }*/
            }

            //Create a MemoryStream to convert bytes to a stream
            using (var deserializeMemoryStream = ZlibCodecDecompress(bytes))
            //using (var deserializeMemoryStream = new MemoryStream(bytes))
            {
                //Go to head of the stream
                deserializeMemoryStream.Position = 0;
                //Debug.WriteLine($"Decompressed {bytes.Length} to {deserializeMemoryStream.Length}");
                //Deserialize the message
                var binaryFormatter = new BinaryFormatter
                {
                    AssemblyFormat = FormatterAssemblyStyle.Simple,
                    Binder = new DeserializationAppDomainBinder()
                };

                //Return the deserialized message
                return (IScsMessage)binaryFormatter.Deserialize(deserializeMemoryStream);
            }
        }

        private static MemoryStream ZlibCodecDecompress(byte[] compressedBytes)
        {
            const int bufferSize = 1024;
            ZlibCodec decompressor = new ZlibCodec();

            MemoryStream ms = new MemoryStream();

            int rc = decompressor.InitializeInflate(false);
            if (rc != ZlibConstants.Z_OK) throw new ZlibException("Cannot initialize for inflate.");

            decompressor.InputBuffer = compressedBytes;
            decompressor.NextIn = 0;
            decompressor.AvailableBytesIn = compressedBytes.Length;

            decompressor.OutputBuffer = new byte[bufferSize];

            // pass 1: inflate 
            do
            {
                decompressor.NextOut = 0;
                decompressor.AvailableBytesOut = bufferSize;
                rc = decompressor.Inflate(FlushType.None);

                if (rc != ZlibConstants.Z_OK && rc != ZlibConstants.Z_STREAM_END)
                    throw new Exception("inflating: " + decompressor.Message);

                ms.Write(decompressor.OutputBuffer, 0, bufferSize - decompressor.AvailableBytesOut);
            }
            while (decompressor.AvailableBytesIn != 0 || decompressor.AvailableBytesOut == 0);

            // pass 2: finish and flush
            do
            {
                decompressor.NextOut = 0;
                decompressor.AvailableBytesOut = bufferSize;
                rc = decompressor.Inflate(FlushType.Finish);

                if (rc != ZlibConstants.Z_STREAM_END && rc != ZlibConstants.Z_OK)
                    throw new Exception("inflating: " + decompressor.Message);

                if (bufferSize - decompressor.AvailableBytesOut != 0)
                    ms.Write(decompressor.OutputBuffer, 0, bufferSize - decompressor.AvailableBytesOut);
            }
            while (decompressor.AvailableBytesIn != 0 || decompressor.AvailableBytesOut == 0);

            decompressor.EndInflate();

            return ms;
        }

        #endregion

        #region Private methods

        /// <summary>
        ///     This method tries to read a single message and add to the messages collection.
        /// </summary>
        /// <param name="messages">Messages collection to collect messages</param>
        /// <returns>
        ///     Returns a boolean value indicates that if there is a need to re-call this method.
        /// </returns>
        /// <exception cref="CommunicationException">
        ///     Throws CommunicationException if message is bigger than maximum allowed
        ///     message length.
        /// </exception>
        private bool ReadSingleMessage(ICollection<IScsMessage> messages)
        {
            //Go to the begining of the stream
            _ms.Seek(0, SeekOrigin.Begin);

            //If stream has less than 4 bytes, that means we can not even read length of the message
            //So, return false to wait more bytes from remore application.
            if (_ms.Length < 4)
            {
                _ms.Seek(0, SeekOrigin.End);
                return false;
            }

            //Read length of the message
            var messageLength = ReadInt32(_ms);
            if (messageLength > _maxMessageLength)
            {
                throw new Exception("Message is too big (" + messageLength + " bytes). Max allowed length is " +
                                    _maxMessageLength + " bytes.");
            }

            //If message is zero-length (It must not be but good approach to check it)
            if (messageLength == 0)
            {
                //if no more bytes, return immediately
                if (_ms.Length == 4)
                {
                    _ms.Dispose();
                    _ms = new MemoryStream(); //Clear the stream
                    return false;
                }

                //Create a new memory stream from current except first 4-bytes.
                var bytes = _ms.ToArray();
                _ms.Dispose();
                _ms = new MemoryStream();
                _ms.Write(bytes, 4, bytes.Length - 4);
                return true;
            }

            //If all bytes of the message is not received yet, return to wait more bytes
            if (_ms.Length < 4 + messageLength)
            {
                _ms.Seek(0, SeekOrigin.End);
                return false;
            }

            //Read bytes of serialized message and deserialize it
            var serializedMessageBytes = ReadByteArray(_ms, messageLength);

            messages.Add(DeserializeMessage(serializedMessageBytes));

            //Read remaining bytes to an array

            var remainingBytes = ReadByteArray(_ms, (int)(_ms.Length - (4 + messageLength)));

            //Re-create the receive memory stream and write remaining bytes
            _ms.Dispose();
            _ms = new MemoryStream();
            _ms.Write(remainingBytes, 0, remainingBytes.Length);

            //Return true to re-call this method to try to read next message
            return remainingBytes.Length > 4;
        }

        /// <summary>
        ///     Writes a int value to a byte array from a starting index.
        /// </summary>
        /// <param name="buffer">Byte array to write int value</param>
        /// <param name="startIndex">Start index of byte array to write</param>
        /// <param name="number">An integer value to write</param>
        private static void WriteInt32(byte[] buffer, int startIndex, int number)
        {
            buffer[startIndex] = (byte)((number >> 24) & 0xFF);
            buffer[startIndex + 1] = (byte)((number >> 16) & 0xFF);
            buffer[startIndex + 2] = (byte)((number >> 8) & 0xFF);
            buffer[startIndex + 3] = (byte)(number & 0xFF);
        }

        /// <summary>
        ///     Deserializes and returns a serialized integer.
        /// </summary>
        /// <returns>Deserialized integer</returns>
        private static int ReadInt32(Stream stream)
        {
            var buffer = ReadByteArray(stream, 4);
            return (buffer[0] << 24) |
                   (buffer[1] << 16) |
                   (buffer[2] << 8) | buffer[3];
        }

        /// <summary>
        ///     Reads a byte array with specified length.
        /// </summary>
        /// <param name="stream">Stream to read from</param>
        /// <param name="length">Length of the byte array to read</param>
        /// <returns>Read byte array</returns>
        /// <exception cref="EndOfStreamException">Throws EndOfStreamException if can not read from stream.</exception>
        private static byte[] ReadByteArray(Stream stream, int length)
        {
            var buffer = new byte[length];
            var totalRead = 0;
            while (totalRead < length)
            {
                var read = stream.Read(buffer, totalRead, length - totalRead);
                if (read <= 0)
                {
                    throw new EndOfStreamException("Can not read from stream! Input stream is closed.");
                }

                totalRead += read;
            }

            return buffer;
        }

        #endregion
    }
}