using System;
using System.Collections.Generic;
using System.Linq;
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
            if (usernameBox.Text.Equals("user") && passwordBox.Password.Equals("user"))
            {
                MessageBox.Show("Login sucess! Welcome to LDS Command Application");
                this.Hide();
            }
            else
            {
                MessageBox.Show("Invalid Login, please provide a valid username and password");
                usernameBox.Text = "";
                passwordBox.Password = "";
            }
            //var client = HttpClient();   
        }
    }
}
