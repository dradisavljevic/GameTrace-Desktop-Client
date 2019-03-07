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
using System.Windows.Shapes;
using System.Management;
using System.Diagnostics;
using System.Runtime.InteropServices;
using Oracle.DataAccess.Client;
using System.Windows.Threading;
using System.ComponentModel;
using System.Xml;
using System.Threading;


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

        public static Dictionary<int, string> games = new Dictionary<int, string>();
        public static Dictionary<int, string> gameNames = new Dictionary<int, string>();
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
        public static int startDay = -1;
        public static int endDay = -1;
        public static string game;
        public static MainWindow wind = null;
        public static string _dateStart;
        public static int gameId;


        public Dictionary<int, string> Games
        {
            get { return games; }
            set { games = value; }
        }

        public Dictionary<int, string> GameNames
        {
            get { return gameNames; }
            set { gameNames = value; }
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

        protected void annulateMinutes()
        {
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

        public static void iterateProcessList(Dictionary<int, string> igre, Process[] processlist)
        {
            foreach (Process startedProcess in processlist)
            {
                string processName = startedProcess.ProcessName;

                foreach (KeyValuePair<int, string> entry in igre)
                {
                    XmlDocument xmldoc = new XmlDocument();
                    xmldoc.LoadXml(entry.Value);

                    XmlNodeList detectionRuleProcesses = xmldoc.GetElementsByTagName("proc");

                    foreach (XmlNode node in detectionRuleProcesses)
                    {
                        string nodeInnerProcessName = node.InnerText.Split('.')[0];
                        // TODO: Remove hideous try-catch
                        try
                        {
                            if (processName.ToUpper().Equals(nodeInnerProcessName.ToUpper()))
                            {
                                string mainModuleFilePath = EventWatcherAsync.GetMainModuleFilepath(startedProcess.Id);
                                string[] runtimeName = mainModuleFilePath.Split('\\');
                                string runtimeProcessName = runtimeName[runtimeName.Length - 1];
                                if (runtimeProcessName.Equals(node.InnerText))
                                {
                                    parseXML(node, mainModuleFilePath, entry, startedProcess);
                                }
                            }

                        }
                        catch (NullReferenceException)
                        {
                        }
                    }
                }
            }
        }

        public static void parseXML(XmlNode nod, string fullPath, KeyValuePair<int, string> entry, Process theprocess)
        {

            XmlNode parent = nod.ParentNode;
            XmlNodeList siblings = parent.ChildNodes;
            Boolean foundFile = false;
            Boolean fileAbsent = true;
            Boolean argument = false;
            Boolean argumentPresent = false;

            foreach (XmlNode sibling in siblings)
            {
                if (sibling.Name.Equals("file") && foundFile == false)
                {
                    string path = fullPath;

                    string subdirectories = sibling.InnerText;
                    path = path.Substring(0, path.Length - nod.InnerText.Length);
                    while (subdirectories.Substring(0, 2).Equals(".."))
                    {
                        string[] splitPath = path.Split('\\');
                        string lastDirectory = splitPath[splitPath.Length - 2];
                        path = path.Substring(0, path.Length - lastDirectory.Length - 1);
                        subdirectories = subdirectories.Substring(3, subdirectories.Length - 3);
                    }
                    path = path + subdirectories;
                    if (System.IO.File.Exists(path))
                    {
                        foundFile = true;

                    }
                }
                if (sibling.Name.Equals("no") && fileAbsent == true)
                {
                    string path = fullPath;
                    string subdirectories = sibling.InnerText;

                    path = path.Substring(0, path.Length - nod.InnerText.Length);
                    while (subdirectories.Substring(0, 2).Equals(".."))
                    {
                        string[] splitPath = path.Split('\\');
                        string lastDirectory = splitPath[splitPath.Length - 2];
                        path = path.Substring(0, path.Length - lastDirectory.Length - 1);
                        subdirectories = subdirectories.Substring(3, subdirectories.Length - 3);

                    }
                    path = path + subdirectories;
                    if (System.IO.File.Exists(path))
                    {
                        fileAbsent = false;
                    }
                }

                if (sibling.Name.Equals("arg") && argument == false)
                {
                    var commandLine = new StringBuilder(EventWatcherAsync.GetMainModuleFilepath(theprocess.Id));

                    commandLine.Append(" ");
                    using (var searcher = new ManagementObjectSearcher(QueryStrings.selectCMDWhereProcessId + theprocess.Id))
                    {
                        foreach (var @object in searcher.Get())
                        {
                            commandLine.Append(@object["CommandLine"]);
                            commandLine.Append(" ");
                        }
                    }
                    argumentPresent = true;

                    if (commandLine.ToString().Contains(sibling.InnerText))
                    {
                        argument = true;
                    }
                }

            }
            if (argumentPresent == false)
            {
                argument = true;
            }


            if (foundFile == true && fileAbsent == true && argument == true)
            {


                running = nod.InnerText;
                Debug.WriteLine("Caught!" + running);

                MainWindow.startSeconds = (int)System.DateTime.Now.Second;
                MainWindow.startMinutes = (int)System.DateTime.Now.Minute;
                MainWindow.startHours = (int)System.DateTime.Now.Hour;
                MainWindow.startDay = (int)System.DateTime.Now.Day;
                MainWindow.game = MainWindow.gameNames[entry.Key];
                MainWindow.gameId = entry.Key;

                if (MainWindow.wind != null)
                {
                    MainWindow.wind.updateGameName(MainWindow.gameNames[entry.Key]);
                    MainWindow.wind.updateDate(System.DateTime.Now.ToString());
                    String sql = QueryStrings.updateGameUserPlaying1 + entry.Key + QueryStrings.updateGameUserPlaying2 + MainWindow.logged.ToString() + "'";
                    OracleConnection conn = new OracleConnection();
                    conn.ConnectionString = QueryStrings.connstr;
                    using (OracleCommand cmd = new OracleCommand(sql, conn))
                    {
                        conn.Open();

                        // reader is IDisposable and should be closed
                        cmd.ExecuteNonQuery();
                    }

                    conn.Close();
                }

                MainWindow.game = MainWindow.gameNames[entry.Key];
                MainWindow.gameId = entry.Key;

            }
        }

        public static void trackTime()
        {
            MainWindow.endSeconds = (int)System.DateTime.Now.Second;
            MainWindow.endMinutes = (int)System.DateTime.Now.Minute;
            MainWindow.endHours = (int)System.DateTime.Now.Hour;
            MainWindow.endDay = (int)System.DateTime.Now.Day;
            if (MainWindow.endDay == MainWindow.startDay)
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
            else if (MainWindow.endDay > MainWindow.startDay)
            {
                MainWindow.endHours = MainWindow.endHours + (MainWindow.endDay - MainWindow.startDay) * 24;

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
            string cmdtxt = QueryStrings.playsInsert1 + MainWindow.logged + "', " + MainWindow.gameId + ", " + MainWindow.seconds + ", " + MainWindow.minutes + ", " + MainWindow.hours + ", " + MainWindow.days + QueryStrings.playsInsert2;
            Debug.WriteLine(cmdtxt);
            OracleConnection conn = new OracleConnection();
            conn.ConnectionString = QueryStrings.connstr;
            using (OracleCommand cmd = new OracleCommand(cmdtxt, conn))
            {
                conn.Open();

                // reader is IDisposable and should be closed
                cmd.ExecuteNonQuery();

            }

            conn.Close();

            String sql = QueryStrings.updateGameUserNotPlaying + MainWindow.logged.ToString() + "'";

            using (OracleCommand cmd = new OracleCommand(sql, conn))
            {
                conn.Open();

                // reader is IDisposable and should be closed
                cmd.ExecuteNonQuery();

            }

            conn.Close();

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

            string cmdtxt = QueryStrings.selectGames;


            OracleConnection conn = new OracleConnection();
            conn.ConnectionString = QueryStrings.connstr;

            using (OracleCommand cmd = new OracleCommand(cmdtxt, conn))
            {
                conn.Open();

                // reader is IDisposable and should be closed
                using (OracleDataReader dr = cmd.ExecuteReader())
                {
                    while (dr.Read())
                    {
                        string thexml = (string)dr.GetValue(2);
                        games.Add(Convert.ToInt32(dr.GetValue(0)), thexml);
                        gameNames.Add(Convert.ToInt32(dr.GetValue(0)), (string)dr.GetValue(1));
                    }
                }
            }

            ConsoleManager.Show();
            ConsoleManager.Toggle();
            EventWatcherAsync eventWatcher = new EventWatcherAsync();

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
                Process[] processlist = Process.GetProcesses();

                iterateProcessList(games, processlist);
            }
        }



        private void Logout_Click(object sender, RoutedEventArgs e)
        {
            if (game != " " && game != null && game != "Nothing" && logged != null && logged != " " && gameId != 0)
            {
                trackTime();
                annulateMinutes();
                this.updateGameName("Nothing");
                this.updateDate("N/A");
            }
            MainWindow.logged = " ";
            MainWindow.gameId = 0;
            MainWindow.wind.updateLoggedName(" ");
            game = " ";
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
                iterateProcessList(games, processlist);


                this.Show();
                this.InitializeComponent();
                this.updateLoggedName(logged.ToString());


            }
        }

        protected override void OnClosing(System.ComponentModel.CancelEventArgs e)
        {
            if (game != " " && game != null && game != "Nothing" && logged != null && logged != " " && gameId != 0)
            {
                trackTime();
                annulateMinutes();
                this.updateGameName("Nothing");
                this.updateDate("N/A");
            }
        }
    }
}
