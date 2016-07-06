using System;
using System.Windows.Forms;

namespace ProxyConfig
{
    /// <summary>
    /// Form for config proxy
    /// </summary>
    public partial class FrmProxyConfig : Form
    {
        private ushort _proxyPort;

        /// <summary>
        /// Constructor
        /// </summary>
        public FrmProxyConfig()
        {
            InitializeComponent();
        }

        private void btCancel_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
        }

        private void btOk_Click(object sender, EventArgs e)
        {
            var proxyConfig = new ProxyConfig
            {
                ProxyEnable = cbEnableProxy.Checked,
                ProxyAddress = tbProxyAddress.Text,
                ProxyPort = _proxyPort,
                ProxyUserName = tbProxyUserName.Text,
                ProxyPassword = tbProxyUserPassword.Text
            };
            if (rbHttp.Checked) proxyConfig.ProxyType = rbHttp.Text;
            if (rbSocks4.Checked) proxyConfig.ProxyType = rbSocks4.Text;
            if (rbSocks5.Checked) proxyConfig.ProxyType = rbSocks5.Text;
            ProxyConfig.SetConfig(proxyConfig);
            DialogResult = DialogResult.OK;
            Close();
        }

        private void FrmProxyConfig_Load(object sender, EventArgs e)
        {
            var proxyConfig = ProxyConfig.GetConfig();
            cbEnableProxy.Checked = proxyConfig.ProxyEnable;
            gbEnableProxy.Enabled = proxyConfig.ProxyEnable;
            switch (proxyConfig.ProxyType)
            {
                case "HTTP":
                    rbHttp.Checked = true;
                    break;
                case "SOCKS4":
                    rbSocks4.Checked = true;
                    break;
                case "SOCKS5":
                    rbSocks5.Checked = true;
                    break;
            }
            tbProxyAddress.Text = proxyConfig.ProxyAddress;
            tbProxyPort.Text = proxyConfig.ProxyPort.ToString();
            tbProxyUserName.Text = proxyConfig.ProxyUserName;
            tbProxyUserPassword.Text = proxyConfig.ProxyPassword;
        }

        private void tbProxyPort_TextChanged(object sender, EventArgs e)
        {
            if (!ushort.TryParse(tbProxyPort.Text, out _proxyPort))
            {
                _proxyPort = 0;
                tbProxyPort.Text = _proxyPort.ToString();
            }
        }

        private void cbEnableProxy_CheckedChanged(object sender, EventArgs e)
        {
            gbEnableProxy.Enabled = cbEnableProxy.Checked;
        }
    }
}
