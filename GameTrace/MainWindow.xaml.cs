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

        public static Dictionary<int, string> igre = new Dictionary<int, string>();
        public static Dictionary<int, string> igreNaziv = new Dictionary<int, string>();
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
            foreach (Process theprocess in processlist)
            {
                string proces = theprocess.ProcessName;

                foreach (KeyValuePair<int, string> entry in igre)
                {
                    XmlDocument xmldoc = new XmlDocument();
                    xmldoc.LoadXml(entry.Value);

                    XmlNodeList procesi = xmldoc.GetElementsByTagName("proc");

                    foreach (XmlNode nod in procesi)
                    {
                        string tekst = nod.InnerText.Split('.')[0];
                        // TODO: Remove hideous try-catch
                        try
                        {
                            if (proces.ToUpper().Equals(tekst.ToUpper()))
                            {
                                string putanja = EventWatcherAsync.GetMainModuleFilepath(theprocess.Id);
                                string[] runtimeIme = putanja.Split('\\');
                                string nazivProcesa = runtimeIme[runtimeIme.Length - 1];
                                if (nazivProcesa.Equals(nod.InnerText))
                                {
                                    parseXML(nod, putanja, entry, theprocess);
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

        public static void parseXML(XmlNode nod, string putanja, KeyValuePair<int, string> entry, Process theprocess)
        {

            XmlNode roditelj = nod.ParentNode;
            XmlNodeList srodnici = roditelj.ChildNodes;
            Boolean nasao = false;
            Boolean nijeNasao = true;
            Boolean argument = false;
            Boolean imaArg = false;

            foreach (XmlNode srodnik in srodnici)
            {
                if (srodnik.Name.Equals("file") && nasao == false)
                {
                    string fajl = putanja;

                    string dodatak = srodnik.InnerText;
                    fajl = fajl.Substring(0, fajl.Length - nod.InnerText.Length);
                    while (dodatak.Substring(0, 2).Equals(".."))
                    {
                        string[] fajlDeljen = fajl.Split('\\');
                        string direktorijumPoslednji = fajlDeljen[fajlDeljen.Length - 2];
                        fajl = fajl.Substring(0, fajl.Length - direktorijumPoslednji.Length - 1);
                        dodatak = dodatak.Substring(3, dodatak.Length - 3);
                    }
                    fajl = fajl + dodatak;
                    if (System.IO.File.Exists(fajl))
                    {
                        nasao = true;

                    }
                }
                if (srodnik.Name.Equals("no") && nijeNasao == true)
                {
                    string fajl = putanja;
                    string dodatak = srodnik.InnerText;

                    fajl = fajl.Substring(0, fajl.Length - nod.InnerText.Length);
                    while (dodatak.Substring(0, 2).Equals(".."))
                    {
                        string[] fajlDeljen = fajl.Split('\\');
                        string direktorijumPoslednji = fajlDeljen[fajlDeljen.Length - 2];
                        fajl = fajl.Substring(0, fajl.Length - direktorijumPoslednji.Length - 1);
                        dodatak = dodatak.Substring(3, dodatak.Length - 3);

                    }
                    fajl = fajl + dodatak;
                    if (System.IO.File.Exists(fajl))
                    {
                        nijeNasao = false;
                    }
                }

                if (srodnik.Name.Equals("arg") && argument == false)
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
                    imaArg = true;

                    if (commandLine.ToString().Contains(srodnik.InnerText))
                    {
                        argument = true;
                    }
                }

            }
            if (imaArg == false)
            {
                argument = true;
            }


            if (nasao == true && nijeNasao == true && argument == true)
            {


                running = nod.InnerText;
                Debug.WriteLine("Caught!" + running);

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

                MainWindow.game = MainWindow.igreNaziv[entry.Key];
                MainWindow.gameId = entry.Key;

            }
        }

        public static void trackTime()
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
                        igre.Add(Convert.ToInt32(dr.GetValue(0)), thexml);
                        igreNaziv.Add(Convert.ToInt32(dr.GetValue(0)), (string)dr.GetValue(1));
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

                iterateProcessList(igre, processlist);
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
                iterateProcessList(igre, processlist);


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
