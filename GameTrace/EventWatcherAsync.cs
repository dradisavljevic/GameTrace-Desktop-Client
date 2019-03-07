using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using System.Management;
using System.Diagnostics;
using Oracle.DataAccess.Client;

namespace GameTrace
{
    class EventWatcherAsync
    {

        public static string GetMainModuleFilepath(int processId)
        {
            string wmiQueryString = QueryStrings.selectExecPathWhereProcessId + processId;
            using (var searcher = new ManagementObjectSearcher(wmiQueryString))
            {
                using (var results = searcher.Get())
                {
                    ManagementObject mo = results.Cast<ManagementObject>().FirstOrDefault();
                    if (mo != null)
                    {
                        return (string)mo["ExecutablePath"];
                    }
                }
            }
            return null;
        }

        private void WmiEventHandler(object sender, EventArrivedEventArgs e)
        {

            Debug.WriteLine("TargetInstance.Handle :    " + ((ManagementBaseObject)e.NewEvent.Properties["TargetInstance"].Value)["Handle"]);
            Debug.WriteLine("TargetInstance.Name :      " + ((ManagementBaseObject)e.NewEvent.Properties["TargetInstance"].Value)["Name"]);

            foreach (KeyValuePair<int, string> entry in MainWindow.igre)
            {

                XmlDocument xmldoc = new XmlDocument();
                xmldoc.LoadXml(entry.Value);

                XmlNodeList procesi = xmldoc.GetElementsByTagName("proc");

                foreach (XmlNode nod in procesi)
                {
                    // TODO: Remove hideous try-catch
                    try
                    {
                        if ((nod.InnerText.ToUpper()).Equals(((string)((ManagementBaseObject)e.NewEvent.Properties["TargetInstance"].Value)["Name"]).ToUpper()))
                        {
                            string imeProcesa = (string)((ManagementBaseObject)e.NewEvent.Properties["TargetInstance"].Value)["Name"];
                            string samoIme = imeProcesa.Split('.')[0];
                            Process[] processes = Process.GetProcessesByName(samoIme);
                            Process theprocess = processes[0];
                            string putanja = GetMainModuleFilepath(theprocess.Id);
                            string[] runtimeIme = putanja.Split('\\');
                            string nazivProcesa = runtimeIme[runtimeIme.Length - 1];
                            if (MainWindow.running == null)
                            {
                                MainWindow.parseXML(nod, putanja, entry, theprocess);
                            }
                        }
                    }
                    catch (Exception)
                    {
                    }
                }

            }


        }

        private void WmiEventHandlerExit(object sender, EventArrivedEventArgs e)
        {
            if (MainWindow.logged != null)
            {
                Debug.WriteLine("TargetInstance.Handle :    " + ((ManagementBaseObject)e.NewEvent.Properties["TargetInstance"].Value)["Handle"]);
                Debug.WriteLine("TargetInstance.Name EXITED :      " + ((ManagementBaseObject)e.NewEvent.Properties["TargetInstance"].Value)["Name"]);

                if (MainWindow.running != null && MainWindow.gameId != 0)
                {

                    if ((MainWindow.running.ToUpper()).Equals(((string)((ManagementBaseObject)e.NewEvent.Properties["TargetInstance"].Value)["Name"]).ToUpper()))
                    {
                        MainWindow.running = null;
                        MainWindow.trackTime();
                        MainWindow.game = " ";
                        if (MainWindow.wind != null)
                        {

                            MainWindow.wind.updateGameName("Nothing");
                            MainWindow.wind.updateDate("N/A");
                        }

                        Debug.WriteLine("Sati: " + MainWindow.hours + " Minuta: " + MainWindow.minutes + " Sekundi: " + MainWindow.seconds);


                        Process[] processlist = Process.GetProcesses();

                        MainWindow.iterateProcessList(MainWindow.igre, processlist);
                    }
                }
            }
        }

        public void QueryWmi(string query, string type)
        {
            string ComputerName = "localhost";
            string WmiQuery = query;
            ManagementEventWatcher Watcher;
            ManagementScope Scope;

            Scope = new ManagementScope(String.Format("\\\\{0}\\root\\CIMV2", ComputerName), null);
            Scope.Connect();

            Watcher = new ManagementEventWatcher(Scope, new EventQuery(WmiQuery));
            if (type == "start")
            {
                Watcher.EventArrived += new EventArrivedEventHandler(this.WmiEventHandler);
            }
            else
            {
                Watcher.EventArrived += new EventArrivedEventHandler(this.WmiEventHandlerExit);
            }
            Watcher.Start();
            Console.Read();
            Watcher.Stop();
        }

        public EventWatcherAsync()
        {
            try
            {
                QueryWmi(QueryStrings.instanceCreation, "start");
            }
            catch (Exception e)
            {
                Debug.WriteLine("Exception {0} Trace {1}", e.Message, e.StackTrace);
            }
            try
            {
                QueryWmi(QueryStrings.instanceDeletion, "exit");
            }
            catch (Exception e)
            {
                Debug.WriteLine("Exception {0} Trace {1}", e.Message, e.StackTrace);
            }

        }
    }
}
