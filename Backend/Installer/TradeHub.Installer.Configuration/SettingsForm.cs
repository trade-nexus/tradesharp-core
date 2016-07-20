using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace TradeHub.Installer.Configuration
{
    public partial class SettingsForm : Form
    {
        public SettingsForm()
        {
            InitializeComponent();
        }

        private void OnTestButtonClick(object sender, EventArgs e)
        {
           TestConnection();
        }

        private void OnDoneButtonClick(object sender, EventArgs e)
        {
            if (MySqlConfiguration.TestConnection(usernameTextBox.Text, passwordTextBox.Text))
            {
                
                MySqlConfiguration.DeployScript(usernameTextBox.Text,passwordTextBox.Text);
                string filePath = Path.GetFullPath(@"~\..\..\TradeSharp User Interface\SpringConfig\SpringDao.xml");
                MySqlConfiguration.ChangeSpringConfigFile(usernameTextBox.Text, passwordTextBox.Text, filePath);

                //Display message to user about success.
                MessageBox.Show("Application has been configured, now you can start the application");

                //Close the application
                this.Close();
            }
            else
            {
                MessageBox.Show("Please verify connection first");
            }
        }

        /// <summary>
        /// Test Connection
        /// </summary>
        private void TestConnection()
        {
            if (MySqlConfiguration.TestConnection(usernameTextBox.Text, passwordTextBox.Text))
            {
                MessageBox.Show("Connection Successfull");
            }
            else
            {
                MessageBox.Show("Invalid credentials");
            }
        }
    }
}
