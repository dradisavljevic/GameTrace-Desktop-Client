using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Collections.ObjectModel;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Net;
using System.Runtime.InteropServices;
using System.Reflection;
using System.Threading;
using System.Diagnostics;
using Oracle.DataAccess.Client;
using System.Security.Cryptography;

namespace GameTrace
{
    /// <summary>
    /// Interaction logic for Login.xaml
    /// </summary>
    public partial class Login : Window
    {
        public int sadrzaj = 0;

        public Login()
        {
            InitializeComponent();
        }



        private void Registruj_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void Uloguj_Click(object sender, RoutedEventArgs e)
        {
            string user = Ime.Text;
            string pass = Pass.Password;
            SHA256 sha256Hash = SHA256.Create();
            byte[] bytes = sha256Hash.ComputeHash(Encoding.UTF8.GetBytes(pass));
            StringBuilder builder = new StringBuilder();
            for (int i = 0; i < bytes.Length; i++)
            {
                builder.Append(bytes[i].ToString("x2"));
            }
            string hashedPassword = builder.ToString();
            string cmdtxt = QueryStrings.selectUserUname + user + QueryStrings.selectUserPword + hashedPassword + QueryStrings.selectUserUtype;

            OracleConnection conn = new OracleConnection();
            conn.ConnectionString = QueryStrings.connstr;
            using (OracleCommand cmd = new OracleCommand(cmdtxt, conn))
            {
                try
                {
                    conn.Open();
                }
                catch (Oracle.ManagedDataAccess.Client.OracleException ex)
                {
                    string errorMessage = "Source: " + ex.Source + "\n" +
                                          "Message: " + ex.Message;
                    Debug.WriteLine(errorMessage);
                    Console.WriteLine("An exception occurred. Please contact your system administrator.");
                }
                // reader is IDisposable and should be closed
                using (OracleDataReader dr = cmd.ExecuteReader())
                {


                    while (dr.Read())
                    {
                        sadrzaj = Convert.ToInt32(dr.GetValue(0));
                        Debug.WriteLine("PRB" + sadrzaj);
                    }

                }

            }
            if (sadrzaj > 0)
            {
                MainWindow.logged = user;
                conn.Close();
                this.Close();
            }
            else
            {
                MessageBoxResult result = MessageBox.Show("Username and password combination does not exist.", "Invalid Login Credentials", MessageBoxButton.OK, MessageBoxImage.Exclamation);
                return;
            }

        }

        protected override void OnClosing(System.ComponentModel.CancelEventArgs e)
        {
            BinaryFormatter formatter = new BinaryFormatter();
            FileStream stream = null;

            try
            {
            }
            catch
            {
                // 
            }
            finally
            {
                if (stream != null)
                    stream.Dispose();
            }
        }



    }
}
