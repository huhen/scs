using System;
using System.IO;
using System.Reflection;
using System.Text;
using System.Xml.Serialization;

namespace ProxyConfig
{
    /// <summary>
    /// ProxyConfig
    /// </summary>
    [Serializable]
    public class ProxyConfig
    {
        /// <summary>
        /// ProxyType
        /// </summary>
        public string ProxyType;
        /// <summary>
        /// ProxyAddress
        /// </summary>
        public string ProxyAddress;
        /// <summary>
        /// ProxyPort
        /// </summary>
        public ushort ProxyPort;
        /// <summary>
        /// ProxyUserName
        /// </summary>
        public string ProxyUserName;
        /// <summary>
        /// ProxyPassword
        /// </summary>
        public string ProxyPassword;
        /// <summary>
        /// ProxyEnable
        /// </summary>
        public bool ProxyEnable;
        /// <summary>
        /// ProxyPasswordEncrypted
        /// </summary>
        public bool ProxyPasswordEncrypted;

        private const string _proxyConfigName = "proxy.xml";
        private const byte _xorConst = 0xAD;

        /// <summary>
        /// Load Config
        /// </summary>
        /// <returns></returns>
        public static ProxyConfig GetConfig()
        {
            var config = new ProxyConfig() { ProxyType = string.Empty, ProxyAddress = string.Empty, ProxyPort = 0, ProxyUserName = string.Empty, ProxyPassword = string.Empty, ProxyEnable = false, ProxyPasswordEncrypted = false };
            try
            {
                var proxyConfigPath = Path.Combine(Path.GetDirectoryName(new Uri(Assembly.GetEntryAssembly().EscapedCodeBase).LocalPath), _proxyConfigName);
                if (!File.Exists(proxyConfigPath)) return config;
                var serializer = new XmlSerializer(typeof(ProxyConfig));
                using (var reader = new StreamReader(proxyConfigPath))
                {
                    config = (ProxyConfig)serializer.Deserialize(reader);
                    reader.Close();
                }
            }
            catch { }

            if (config.ProxyPasswordEncrypted)
            {
                config.ProxyPassword = Decrypt(config.ProxyPassword);
            }else
            {
                SetConfig(config);
            }

            return config;
        }

        private static string Decrypt(string s)
        {
            try
            {
                var encBytes = Convert.FromBase64String(s);
                for (var i = 0; i < encBytes.Length; i++)
                    encBytes[i] ^= _xorConst;
                return Encoding.UTF8.GetString(encBytes);
            }
            catch
            {
                return s;
            }
        }

        private static string Encrypt(string s)
        {
            try
            {
                var encBytes = Encoding.UTF8.GetBytes(s);
                for (var i = 0; i < encBytes.Length; i++)
                    encBytes[i] ^= _xorConst;
                return Convert.ToBase64String(encBytes);
            }
            catch
            {
                return s;
            }
        }

        /// <summary>
        /// Save config
        /// </summary>
        /// <param name="proxyConfig"></param>
        /// <returns></returns>
        public static bool SetConfig(ProxyConfig proxyConfig)
        {
            if (proxyConfig == null) return false;
            try
            {
                var proxyConfigPath = Path.Combine(Path.GetDirectoryName(new Uri(Assembly.GetEntryAssembly().EscapedCodeBase).LocalPath), _proxyConfigName);
                var serializer = new XmlSerializer(typeof(ProxyConfig));
                using (var writer = new StreamWriter(proxyConfigPath))
                {
                    proxyConfig.ProxyPasswordEncrypted = true;
                    var tmp = proxyConfig.ProxyPassword;
                    proxyConfig.ProxyPassword = Encrypt(proxyConfig.ProxyPassword);

                    serializer.Serialize(writer, proxyConfig);

                    proxyConfig.ProxyPassword = tmp;

                    writer.Close();
                }
                return true;
            }
            catch { return false; }
        }
    }
}
