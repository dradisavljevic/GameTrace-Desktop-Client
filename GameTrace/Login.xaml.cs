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

namespace GameTrace
{
    /// <summary>
    /// Interaction logic for Login.xaml
    /// </summary>
    public partial class Login : Window
    {
        public string connstr = "Data Source= XE;User Id=GameTrace;Password=ftn;";
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
            string cmdtxt =
         @"select COUNT(*) from GT_USER WHERE UNAME = '" + user + "' AND PWORD = '" + pass+"'";

            OracleConnection conn = new OracleConnection();
            conn.ConnectionString = connstr;
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
                        Debug.WriteLine("PRB"+sadrzaj);
                    }

                }

            }
            Debug.WriteLine("PRBBB" + sadrzaj);
            if (sadrzaj > 0)
            {
                MainWindow.logged = user;
                conn.Close();
                this.Close();
            }
            else
            {
                MessageBoxResult result = MessageBox.Show("Username or password combination does not exist.", "Invalid Login Credentials", MessageBoxButton.OK, MessageBoxImage.Exclamation);
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
