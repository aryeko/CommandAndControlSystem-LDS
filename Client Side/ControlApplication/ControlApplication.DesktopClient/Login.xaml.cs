using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using ControlApplication.Core.Networking;

namespace ControlApplication.DesktopClient
{
    /// <summary>
    /// Interaction logic for Login.xaml
    /// </summary>
    public partial class Login : Window
    {
        public Login()
        {
            InitializeComponent();
            usernameBox.Focus();
        }

        private void LoginWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            Application.Current.Shutdown();
        }

        private void cancelBtn_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }

        private void loginBtn_Click(object sender, RoutedEventArgs e)
        {
            var loginSuccess = ServerConnectionManager.Instance.Login(usernameBox.Text, passwordBox.Password);
            if (loginSuccess)
            {
                MessageBox.Show("Login sucess! Welcome to LDS Command Application");
                Hide();
            }
            else
            {
                MessageBox.Show("Invalid Login, please provide a valid username and password");
                usernameBox.Text = "";
                usernameBox.Focus();
                passwordBox.Password = "";
            }
        }
    }
}
