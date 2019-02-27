﻿using System;
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
using System.Windows.Shapes;
using System.Management;
using System.Diagnostics;
using System.Runtime.InteropServices;
using Oracle.DataAccess.Client;
using System.Windows.Threading;
using System.ComponentModel;

namespace GameTrace
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    /// 
    public static class ExtensionMethods
    {
        private static Action EmptyDelegate = delegate() { };

        public static void Refresh(this UIElement uiElement)
        {
            uiElement.Dispatcher.Invoke(DispatcherPriority.Render, EmptyDelegate);
        }
    }
    public partial class MainWindow : Window, INotifyPropertyChanged
    {

        public static Dictionary<int, string> igre = new Dictionary<int, string>();
        public static Dictionary<int, string> igreNaziv = new Dictionary<int, string>();
        public string connstr = "Data Source= XE;User Id=GameTrace;Password=ftn;";
        public static string logged = null;
        public static string running = null;
        public static int startSeconds = -1;
        public static int startMinutes = -1;
        public static int startHours = -1;
        public static int seconds = -1;
        public static int minutes = -1;
        public static int hours = -1;
        public static int days = 0;
        public static int endSeconds = -1;
        public static int endMinutes = -1;
        public static int endHours = -1;
        public static int pocetak = -1;
        public static int kraj = -1;
        public static string game;
        public static MainWindow wind = null;
        public static string _dateStart;
        public static int gameId;


        public Dictionary<int, string> Igre
        {
            get { return igre; }
            set { igre = value; }
        }

        public Dictionary<int, string> IgreNaziv
        {
            get { return igreNaziv; }
            set { igreNaziv = value; }
        }

        public string DateStart
        {
            get { return _dateStart; }
            set
            {
                _dateStart = value;
                updateDate(_dateStart);
                NotifyPropertyChanged("DateStart");
            }
        }

        public string Game
        {
            get { return game; }
            set
            {
                if (game != value)
                {
                    game = value;
                    updateGameName(game);
                    NotifyPropertyChanged("Game");
                }
            }
        }

        public string Logged
        {
            get { return logged; }
            set
            {
                if (logged != value)
                {
                    logged = value;
                    updateLoggedName(logged);
                    NotifyPropertyChanged("Logged");
                }
            }
        }

        protected void NotifyPropertyChanged(String propertyName)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }

        public event PropertyChangedEventHandler PropertyChanged;


        public void updateGameName(string newText)
        {


            game = newText;
            NotifyPropertyChanged("Game");

        }

        public void updateLoggedName(string newText)
        {


            logged = newText;
            NotifyPropertyChanged("Logged");

        }

        public void updateDate(string date)
        {
            _dateStart = date;
            NotifyPropertyChanged("DateStart");
        }

        public MainWindow()
        {

            InitializeComponent();
            DataContext = this;

            this.Hide();

            string cmdtxt =
          @"select GAMEID, GAMENAME,GAMEDR from GAME";


            OracleConnection conn = new OracleConnection();
            conn.ConnectionString = connstr;

            using (OracleCommand cmd = new OracleCommand(cmdtxt, conn))
            {

                conn.Open();

                // reader is IDisposable and should be closed
                using (OracleDataReader dr = cmd.ExecuteReader())
                {


                    while (dr.Read())
                    {
                        string thexml = (string)dr.GetValue(2);
                        igre.Add(Convert.ToInt32(dr.GetValue(0)), thexml);
                        igreNaziv.Add(Convert.ToInt32(dr.GetValue(0)), (string)dr.GetValue(1));
                    }

                }

            }

            ConsoleManager.Show();
            ConsoleManager.Toggle();
            EventWatcherAsync eventWatcher = new EventWatcherAsync();


            Process[] processlist = Process.GetProcesses();

            foreach (Process theprocess in processlist)
            {
                string proces = theprocess.ProcessName + ".exe";
                foreach (KeyValuePair<int, string> entry in igre)
                {
                    if (proces.ToUpper().Equals(entry.Value.ToUpper()))
                    {
                        running = entry.Value;
                        Debug.WriteLine("Caught!" + running);
                        startSeconds = (int)System.DateTime.Now.Second;
                        startMinutes = (int)System.DateTime.Now.Minute;
                        startHours = (int)System.DateTime.Now.Hour;
                        pocetak = (int)System.DateTime.Now.Day;
                        this.gameName.Content = MainWindow.igreNaziv[entry.Key];
                        this.startDate.Content = System.DateTime.Now.ToString();
                        game = MainWindow.igreNaziv[entry.Key];
                        gameId = entry.Key;
                        String sql = "UPDATE GAME_USER SET PLAYING_GAME_ID = " + entry.Key + ", PLAYING= 1 WHERE UNAME= '" + MainWindow.logged.ToString() + "'";

                        using (OracleCommand cmd = new OracleCommand(sql, conn))
                        {
                            conn.Open();

                            // reader is IDisposable and should be closed
                            cmd.ExecuteNonQuery();

                        }

                        conn.Close();
                    }
                }

            }

            var s = new Login();
            s.ShowDialog();
            if (logged == null)
            {
                this.Close();
            }
            else
            {
                Debug.WriteLine(logged);
                if (game == null || game == " ")
                {
                    this.updateGameName("Nothing");
                    this.updateDate("N/A");
                }
                conn.Close();
                this.Show();
                this.updateLoggedName(logged.ToString());
                wind = this;
            }


        }

        private void Logout_Click(object sender, RoutedEventArgs e)
        {
            if (game != " ")
            {
                if (logged != null)
                {
                    if (game != null)
                    {
                        if (game != "Nothing")
                        {

                            if (gameId != 0)
                            {
                                if (logged != " ")
                                {

                                    MainWindow.endSeconds = (int)System.DateTime.Now.Second;
                                    MainWindow.endMinutes = (int)System.DateTime.Now.Minute;
                                    MainWindow.endHours = (int)System.DateTime.Now.Hour;
                                    MainWindow.kraj = (int)System.DateTime.Now.Day;
                                    if (MainWindow.kraj == MainWindow.pocetak)
                                    {
                                        if (MainWindow.endSeconds < MainWindow.startSeconds)
                                        {
                                            MainWindow.endSeconds = MainWindow.endSeconds + 60;
                                            MainWindow.endMinutes = MainWindow.endMinutes - 1;
                                        }
                                        MainWindow.seconds = MainWindow.endSeconds - MainWindow.startSeconds;
                                        if (MainWindow.endMinutes < MainWindow.startMinutes)
                                        {
                                            MainWindow.endMinutes = MainWindow.endMinutes + 60;
                                            MainWindow.endHours = MainWindow.endHours - 1;
                                        }
                                        MainWindow.minutes = MainWindow.endMinutes - MainWindow.startMinutes;
                                        MainWindow.hours = MainWindow.endHours - MainWindow.startHours;
                                        while (MainWindow.hours > 24)
                                        {
                                            MainWindow.days = MainWindow.days + 1;
                                            MainWindow.hours = MainWindow.hours - 24;
                                        }
                                    }
                                    else if (MainWindow.kraj > MainWindow.pocetak)
                                    {
                                        MainWindow.endHours = MainWindow.endHours + (MainWindow.kraj - MainWindow.pocetak) * 24;

                                        if (MainWindow.endSeconds < MainWindow.startSeconds)
                                        {
                                            MainWindow.endSeconds = MainWindow.endSeconds + 60;
                                            MainWindow.endMinutes = MainWindow.endMinutes - 1;
                                        }
                                        MainWindow.seconds = MainWindow.endSeconds - MainWindow.startSeconds;
                                        if (MainWindow.endMinutes < MainWindow.startMinutes)
                                        {
                                            MainWindow.endMinutes = MainWindow.endMinutes + 60;
                                            MainWindow.endHours = MainWindow.endHours - 1;
                                        }
                                        MainWindow.minutes = MainWindow.endMinutes - MainWindow.startMinutes;
                                        MainWindow.hours = MainWindow.endHours - MainWindow.startHours;
                                        while (MainWindow.hours > 24)
                                        {
                                            MainWindow.days = MainWindow.days + 1;
                                            MainWindow.hours = MainWindow.hours - 24;
                                        }
                                    }
                                    else
                                    {
                                    }



                                    string connstr = "Data Source= XE;User Id=GameTrace;Password=ftn;";
                                    string cmdtxt =
                          @"INSERT INTO PLAYS VALUES ('" + MainWindow.logged + "', " + MainWindow.gameId + ", " + MainWindow.seconds + ", " + MainWindow.minutes + ", " + MainWindow.hours + ", " + MainWindow.days + ", DEFAULT, DEFAULT)";
                                    Debug.WriteLine(cmdtxt);
                                    OracleConnection conn = new OracleConnection();
                                    conn.ConnectionString = connstr;
                                    using (OracleCommand cmd = new OracleCommand(cmdtxt, conn))
                                    {
                                        conn.Open();

                                        // reader is IDisposable and should be closed
                                        cmd.ExecuteNonQuery();

                                    }

                                    conn.Close();

                                    String sql = "UPDATE GAME_USER SET PLAYING_GAME_ID = NULL, PLAYING= 0 WHERE UNAME= '" + MainWindow.logged.ToString() + "'";

                                    using (OracleCommand cmd = new OracleCommand(sql, conn))
                                    {
                                        conn.Open();

                                        // reader is IDisposable and should be closed
                                        cmd.ExecuteNonQuery();

                                    }

                                    conn.Close();

                                    MainWindow.minutes = -1;
                                    MainWindow.seconds = -1;
                                    MainWindow.days = 0;
                                    MainWindow.hours = -1;
                                    MainWindow.startHours = -1;
                                    MainWindow.startMinutes = -1;
                                    MainWindow.startSeconds = -1;
                                    MainWindow.endHours = -1;
                                    MainWindow.endMinutes = -1;
                                    MainWindow.endSeconds = -1;
                                    this.updateGameName("Nothing");
                                    this.updateDate("N/A");
                                }
                            }
                        }
                    }
                }
            }
            MainWindow.logged = " ";
            MainWindow.gameId = 0;
            MainWindow.wind.updateLoggedName(" ");
            game = " ";
            this.updateGameName("Nothing");
            this.updateDate("N/A");
            this.Hide();

            var s = new Login();
            s.ShowDialog();
            if (logged == null || logged == " ")
            {
                this.Close();
            }
            else
            {

                Process[] processlist = Process.GetProcesses();

                foreach (Process theprocess in processlist)
                {
                    string proces = theprocess.ProcessName + ".exe";
                    foreach (KeyValuePair<int, string> entry in igre)
                    {
                        if (proces.ToUpper().Equals(entry.Value.ToUpper()))
                        {
                            running = entry.Value;
                            Debug.WriteLine("Caught!" + running);
                            startSeconds = (int)System.DateTime.Now.Second;
                            startMinutes = (int)System.DateTime.Now.Minute;
                            startHours = (int)System.DateTime.Now.Hour;
                            pocetak = (int)System.DateTime.Now.Day;
                            String sql = "UPDATE GAME_USER SET PLAYING_GAME_ID = "+entry.Key+", PLAYING= 1 WHERE UNAME= '"+MainWindow.logged.ToString()+"'";
                            OracleConnection conn = new OracleConnection();
                            conn.ConnectionString = connstr;
                            using (OracleCommand cmd = new OracleCommand(sql, conn))
                            {
                                conn.Open();

                                // reader is IDisposable and should be closed
                                cmd.ExecuteNonQuery();

                            }

                            conn.Close();
                            game = MainWindow.igreNaziv[entry.Key];
                            gameId = entry.Key;
                        }
                    }

                }

                this.Show();
                this.InitializeComponent();
                this.updateLoggedName(logged.ToString());


            }
        }

        protected override void OnClosing(System.ComponentModel.CancelEventArgs e)
        {
            if (game != " ")
            {
                if (logged != null)
                {
                    if (game != null)
                    {
                        if (game != "Nothing")
                        {
                            if (gameId != 0)
                            {
                                if (logged != " ")
                                {

                                    MainWindow.endSeconds = (int)System.DateTime.Now.Second;
                                    MainWindow.endMinutes = (int)System.DateTime.Now.Minute;
                                    MainWindow.endHours = (int)System.DateTime.Now.Hour;
                                    MainWindow.kraj = (int)System.DateTime.Now.Day;
                                    if (MainWindow.kraj == MainWindow.pocetak)
                                    {
                                        if (MainWindow.endSeconds < MainWindow.startSeconds)
                                        {
                                            MainWindow.endSeconds = MainWindow.endSeconds + 60;
                                            MainWindow.endMinutes = MainWindow.endMinutes - 1;
                                        }
                                        MainWindow.seconds = MainWindow.endSeconds - MainWindow.startSeconds;
                                        if (MainWindow.endMinutes < MainWindow.startMinutes)
                                        {
                                            MainWindow.endMinutes = MainWindow.endMinutes + 60;
                                            MainWindow.endHours = MainWindow.endHours - 1;
                                        }
                                        MainWindow.minutes = MainWindow.endMinutes - MainWindow.startMinutes;
                                        MainWindow.hours = MainWindow.endHours - MainWindow.startHours;
                                        while (MainWindow.hours > 24)
                                        {
                                            MainWindow.days = MainWindow.days + 1;
                                            MainWindow.hours = MainWindow.hours - 24;
                                        }
                                    }
                                    else if (MainWindow.kraj > MainWindow.pocetak)
                                    {
                                        MainWindow.endHours = MainWindow.endHours + (MainWindow.kraj - MainWindow.pocetak) * 24;

                                        if (MainWindow.endSeconds < MainWindow.startSeconds)
                                        {
                                            MainWindow.endSeconds = MainWindow.endSeconds + 60;
                                            MainWindow.endMinutes = MainWindow.endMinutes - 1;
                                        }
                                        MainWindow.seconds = MainWindow.endSeconds - MainWindow.startSeconds;
                                        if (MainWindow.endMinutes < MainWindow.startMinutes)
                                        {
                                            MainWindow.endMinutes = MainWindow.endMinutes + 60;
                                            MainWindow.endHours = MainWindow.endHours - 1;
                                        }
                                        MainWindow.minutes = MainWindow.endMinutes - MainWindow.startMinutes;
                                        MainWindow.hours = MainWindow.endHours - MainWindow.startHours;
                                        while (MainWindow.hours > 24)
                                        {
                                            MainWindow.days = MainWindow.days + 1;
                                            MainWindow.hours = MainWindow.hours - 24;
                                        }
                                    }
                                    else
                                    {
                                    }


                                    string connstr = "Data Source= XE;User Id=GameTrace;Password=ftn;";
                                    string cmdtxt =
                          @"INSERT INTO PLAYS VALUES ('" + MainWindow.logged + "', " + MainWindow.gameId + ", " + MainWindow.seconds + ", " + MainWindow.minutes + ", " + MainWindow.hours + ", " + MainWindow.days + ", DEFAULT, DEFAULT)";
                                    Debug.WriteLine(cmdtxt);
                                    OracleConnection conn = new OracleConnection();
                                    conn.ConnectionString = connstr;
                                    using (OracleCommand cmd = new OracleCommand(cmdtxt, conn))
                                    {
                                        conn.Open();

                                        // reader is IDisposable and should be closed
                                        cmd.ExecuteNonQuery();

                                    }

                                    conn.Close();

                                    String sql = "UPDATE GAME_USER SET PLAYING_GAME_ID = NULL, PLAYING= 0 WHERE UNAME= '" + MainWindow.logged.ToString() + "'";

                                    using (OracleCommand cmd = new OracleCommand(sql, conn))
                                    {
                                        conn.Open();

                                        // reader is IDisposable and should be closed
                                        cmd.ExecuteNonQuery();

                                    }

                                    conn.Close();

                                    MainWindow.minutes = -1;
                                    MainWindow.seconds = -1;
                                    MainWindow.days = 0;
                                    MainWindow.hours = -1;
                                    MainWindow.startHours = -1;
                                    MainWindow.startMinutes = -1;
                                    MainWindow.startSeconds = -1;
                                    MainWindow.endHours = -1;
                                    MainWindow.endMinutes = -1;
                                    MainWindow.endSeconds = -1;
                                }
                            }
                        }
                    }
                }
            }
        }
    }

    public class EventWatcherAsync
    {
        private void WmiEventHandler(object sender, EventArrivedEventArgs e)
        {
            
                Debug.WriteLine("TargetInstance.Handle :    " + ((ManagementBaseObject)e.NewEvent.Properties["TargetInstance"].Value)["Handle"]);
                Debug.WriteLine("TargetInstance.Name :      " + ((ManagementBaseObject)e.NewEvent.Properties["TargetInstance"].Value)["Name"]);

                foreach (KeyValuePair<int, string> entry in MainWindow.igre)
                {
                    if ((entry.Value.ToUpper()).Equals(((string)((ManagementBaseObject)e.NewEvent.Properties["TargetInstance"].Value)["Name"]).ToUpper()))
                    {
                        if (MainWindow.running == null)
                        {
                            MainWindow.running = entry.Value;
                            Debug.WriteLine("Caught!" + MainWindow.running);
                            MainWindow.startSeconds = (int)System.DateTime.Now.Second;
                            MainWindow.startMinutes = (int)System.DateTime.Now.Minute;
                            MainWindow.startHours = (int)System.DateTime.Now.Hour;
                            MainWindow.pocetak = (int)System.DateTime.Now.Day;
                            MainWindow.game = MainWindow.igreNaziv[entry.Key];
                            MainWindow.gameId = entry.Key;
                            
                            if (MainWindow.wind != null)
                            {
                                MainWindow.wind.updateGameName(MainWindow.igreNaziv[entry.Key]);
                                MainWindow.wind.updateDate(System.DateTime.Now.ToString());
                                String sql = "UPDATE GAME_USER SET PLAYING_GAME_ID = " + entry.Key + ", PLAYING= 1 WHERE UNAME= '" + MainWindow.logged.ToString() + "'";
                                OracleConnection conn = new OracleConnection();
                                conn.ConnectionString = MainWindow.wind.connstr;
                                using (OracleCommand cmd = new OracleCommand(sql, conn))
                                {
                                    conn.Open();

                                    // reader is IDisposable and should be closed
                                    cmd.ExecuteNonQuery();

                                }

                                conn.Close();
                            }
                        }
                    }
                
            }
            

        }

        private void WmiEventHandlerExit(object sender, EventArrivedEventArgs e)
        {
            if (MainWindow.logged != null )
            {
                Debug.WriteLine("TargetInstance.Handle :    " + ((ManagementBaseObject)e.NewEvent.Properties["TargetInstance"].Value)["Handle"]);
                Debug.WriteLine("TargetInstance.Name EXITED :      " + ((ManagementBaseObject)e.NewEvent.Properties["TargetInstance"].Value)["Name"]);

                if (MainWindow.running != null && MainWindow.gameId != 0)
                {

                    if ((MainWindow.running.ToUpper()).Equals(((string)((ManagementBaseObject)e.NewEvent.Properties["TargetInstance"].Value)["Name"]).ToUpper()))
                    {

                        MainWindow.running = null;
                        MainWindow.endSeconds = (int)System.DateTime.Now.Second;
                        MainWindow.endMinutes = (int)System.DateTime.Now.Minute;
                        MainWindow.endHours = (int)System.DateTime.Now.Hour;
                        MainWindow.kraj = (int)System.DateTime.Now.Day;
                        if (MainWindow.kraj == MainWindow.pocetak)
                        {
                            if (MainWindow.endSeconds < MainWindow.startSeconds)
                            {
                                MainWindow.endSeconds = MainWindow.endSeconds + 60;
                                MainWindow.endMinutes = MainWindow.endMinutes - 1;
                            }
                            MainWindow.seconds = MainWindow.endSeconds - MainWindow.startSeconds;
                            if (MainWindow.endMinutes < MainWindow.startMinutes)
                            {
                                MainWindow.endMinutes = MainWindow.endMinutes + 60;
                                MainWindow.endHours = MainWindow.endHours - 1;
                            }
                            MainWindow.minutes = MainWindow.endMinutes - MainWindow.startMinutes;
                            MainWindow.hours = MainWindow.endHours - MainWindow.startHours;
                            while (MainWindow.hours > 24)
                            {
                                MainWindow.days = MainWindow.days + 1;
                                MainWindow.hours = MainWindow.hours - 24;
                            }
                        }
                        else if (MainWindow.kraj > MainWindow.pocetak)
                        {
                            MainWindow.endHours = MainWindow.endHours + (MainWindow.kraj - MainWindow.pocetak) * 24;

                            if (MainWindow.endSeconds < MainWindow.startSeconds)
                            {
                                MainWindow.endSeconds = MainWindow.endSeconds + 60;
                                MainWindow.endMinutes = MainWindow.endMinutes - 1;
                            }
                            MainWindow.seconds = MainWindow.endSeconds - MainWindow.startSeconds;
                            if (MainWindow.endMinutes < MainWindow.startMinutes)
                            {
                                MainWindow.endMinutes = MainWindow.endMinutes + 60;
                                MainWindow.endHours = MainWindow.endHours - 1;
                            }
                            MainWindow.minutes = MainWindow.endMinutes - MainWindow.startMinutes;
                            MainWindow.hours = MainWindow.endHours - MainWindow.startHours;
                            while (MainWindow.hours > 24)
                            {
                                MainWindow.days = MainWindow.days + 1;
                                MainWindow.hours = MainWindow.hours - 24;
                            }
                        }
                        else
                        {
                        }
                        MainWindow.game = " ";
                        if (MainWindow.wind != null)
                        {
                            MainWindow.wind.updateGameName("Nothing");
                            MainWindow.wind.updateDate("N/A");
                        }
                        string connstr = "Data Source= XE;User Id=GameTrace;Password=ftn;";
                        string cmdtxt =
              @"INSERT INTO PLAYS VALUES ('" + MainWindow.logged + "', " + MainWindow.gameId + ", " + MainWindow.seconds + ", " + MainWindow.minutes + ", " + MainWindow.hours + ", " + MainWindow.days + ", DEFAULT, DEFAULT)";
                        Debug.WriteLine(cmdtxt);
                        OracleConnection conn = new OracleConnection();
                        conn.ConnectionString = connstr;
                        using (OracleCommand cmd = new OracleCommand(cmdtxt, conn))
                        {
                            conn.Open();

                            // reader is IDisposable and should be closed
                            cmd.ExecuteNonQuery();

                        }

                        conn.Close();

                        String sql = "UPDATE GAME_USER SET PLAYING_GAME_ID = NULL, PLAYING= 0 WHERE UNAME= '" + MainWindow.logged.ToString() + "'";

                        using (OracleCommand cmd = new OracleCommand(sql, conn))
                        {
                            conn.Open();

                            // reader is IDisposable and should be closed
                            cmd.ExecuteNonQuery();

                        }

                        conn.Close();

                        Debug.WriteLine("Hours: " + MainWindow.hours + " Minutes: " + MainWindow.minutes + " Seconds: " + MainWindow.seconds);

                    }
                }
            }
        }

        public EventWatcherAsync()
        {
            try
            {
                string ComputerName = "localhost";
                string WmiQuery;
                ManagementEventWatcher Watcher;
                ManagementScope Scope;

                Scope = new ManagementScope(String.Format("\\\\{0}\\root\\CIMV2", ComputerName), null);
                Scope.Connect();

                WmiQuery = "Select * From __InstanceCreationEvent Within 1 " +
                "Where TargetInstance ISA 'Win32_Process' ";

                Watcher = new ManagementEventWatcher(Scope, new EventQuery(WmiQuery));
                Watcher.EventArrived += new EventArrivedEventHandler(this.WmiEventHandler);
                Watcher.Start();
                Console.Read();
                Watcher.Stop();
            }
            catch (Exception e)
            {
                Debug.WriteLine("Exception {0} Trace {1}", e.Message, e.StackTrace);
            }

            try
            {
                string ComputerName = "localhost";
                string WmiQuery2;
                ManagementEventWatcher Watcher2;
                ManagementScope Scope;

                Scope = new ManagementScope(String.Format("\\\\{0}\\root\\CIMV2", ComputerName), null);
                Scope.Connect();

                WmiQuery2 = "Select * From __InstanceDeletionEvent Within 1 " +
                "Where TargetInstance ISA 'Win32_Process' ";

                Watcher2 = new ManagementEventWatcher(Scope, new EventQuery(WmiQuery2));
                Watcher2.EventArrived += new EventArrivedEventHandler(this.WmiEventHandlerExit);
                Watcher2.Start();
                Console.Read();
                Watcher2.Stop();
            }
            catch (Exception e)
            {
                Debug.WriteLine("Exception {0} Trace {1}", e.Message, e.StackTrace);
            }

        }

    }
}
