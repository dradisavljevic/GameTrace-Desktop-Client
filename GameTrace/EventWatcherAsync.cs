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

            foreach (KeyValuePair<int, string> entry in MainWindow.games)
            {

                XmlDocument xmldoc = new XmlDocument();
                xmldoc.LoadXml(entry.Value);

                XmlNodeList detectionRuleProcesses = xmldoc.GetElementsByTagName("proc");

                foreach (XmlNode node in detectionRuleProcesses)
                {
                    // TODO: Remove hideous try-catch
                    try
                    {
                        if ((node.InnerText.ToUpper()).Equals(((string)((ManagementBaseObject)e.NewEvent.Properties["TargetInstance"].Value)["Name"]).ToUpper()))
                        {
                            string processNameExtension = (string)((ManagementBaseObject)e.NewEvent.Properties["TargetInstance"].Value)["Name"];
                            string processName = processNameExtension.Split('.')[0];
                            Process[] processes = Process.GetProcessesByName(processName);
                            Process theprocess = processes[0];
                            string fullPath = GetMainModuleFilepath(theprocess.Id);
                            string[] runtimeName = fullPath.Split('\\');
                            string runtimeProcessName = runtimeName[runtimeName.Length - 1];
                            if (MainWindow.running == null && runtimeProcessName.Equals(node.InnerText))
                            {
                                MainWindow.parseXML(node, fullPath, entry, theprocess);
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

                        Debug.WriteLine("Hours: " + MainWindow.hours + " Minutes: " + MainWindow.minutes + " Seconds: " + MainWindow.seconds);


                        Process[] processlist = Process.GetProcesses();

                        MainWindow.iterateProcessList(MainWindow.games, processlist);
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
