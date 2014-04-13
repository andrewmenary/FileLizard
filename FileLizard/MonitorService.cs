//The MIT License (MIT)

//Copyright (c) 2014 Andrew Menary

//Permission is hereby granted, free of charge, to any person obtaining a copy
//of this software and associated documentation files (the "Software"), to deal
//in the Software without restriction, including without limitation the rights
//to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
//copies of the Software, and to permit persons to whom the Software is
//furnished to do so, subject to the following conditions:

//The above copyright notice and this permission notice shall be included in
//all copies or substantial portions of the Software.

//THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
//IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
//FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
//AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
//LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
//OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
//THE SOFTWARE.

using System;
using System.Configuration;
using System.Diagnostics;
using System.ServiceProcess;
using System.Text;

namespace FileLizard
{
    /// <summary>
    /// A Windows Service which monitors a specific file system directory for
    /// changes and sends email notifications.
    /// </summary>
    public class MonitorService: ServiceBase
    {
        private FileSystemMonitor monitor;

        /// <summary>
        /// Custom constructor which also initializes service
        /// </summary>
        public MonitorService(): base()
        {
            InitializeComponent();
        }

        /// <summary>
        /// This method is called when the service is sent a start command
        /// </summary>
        /// <param name="args"></param>
        protected override void OnStart(string[] args)
        {
            base.OnStart(args);
            EventLog.WriteEntry("Monitor Service is starting.");
            try
            {
                MonitorSettings settings = LoadMonitorSettings();
                monitor = new FileSystemMonitor(this.EventLog, settings);
            }
            catch (Exception ex)
            {
                EventLog.WriteEntry("Monitor Service failed to start.", EventLogEntryType.Error, (int)ServiceEventIds.Start_Failure, ex);
                this.Stop();
            }
        }

        /// <summary>
        /// This method is called when the service is sent a stop command
        /// </summary>
        protected override void OnStop()
        {
            if (monitor != null)
            {
                monitor.Stop();
                monitor = null;
            }
            base.OnStop();
            EventLog.WriteEntry("Monitor Service is stopped.");
        }

        /// <summary>
        /// This method makes sure the service is stopped before it is discarded
        /// </summary>
        /// <param name="disposing"></param>
        protected override void Dispose(bool disposing)
        {
            if (monitor != null)
            {
                monitor.Stop();
                monitor = null;
            }
            base.Dispose(disposing);
        }

        /// <summary>
        /// Method to load configuration settings on service startup.
        /// </summary>
        /// <returns>A MonitorSettings data object.</returns>
        private MonitorSettings LoadMonitorSettings()
        {
            MonitorSettings settings = new MonitorSettings();
            bool hasRequiredSettings = true;

            settings.PathToMonitor = LoadStringSetting("PathToMonitor", ref hasRequiredSettings);
            settings.SmtpServer = LoadStringSetting("SmtpServer", ref hasRequiredSettings);
            settings.SendFrom = LoadStringSetting("SentFrom", ref hasRequiredSettings);
            settings.SendTo = LoadStringSetting("SendTo", ref hasRequiredSettings);
            settings.IncludeSubDirectories = LoadBooleanSetting("IncludeSubDirectories", ref hasRequiredSettings);
            settings.NotifyOnChange = LoadBooleanSetting("NotifyOnChange", ref hasRequiredSettings);
            settings.NotifyOnDelete = LoadBooleanSetting("NotifyOnDelete", ref hasRequiredSettings);
            settings.NotifyOnNew = LoadBooleanSetting("NotifyOnNew", ref hasRequiredSettings);
            settings.NotifyOnRename = LoadBooleanSetting("NotifyOnRename", ref hasRequiredSettings);

            if (!hasRequiredSettings)
                throw new ConfigurationErrorsException("Required configuration settings are not available.");

            return settings;
        }

        /// <summary>
        /// Get a string valued configuration setting, or write an error to the
        /// EventLog if the setting cannot be loaded.
        /// </summary>
        /// <param name="settingName"></param>
        /// <param name="hasRequiredSettings"></param>
        /// <returns></returns>
        private string LoadStringSetting(string settingName, ref bool hasRequiredSettings)
        {
            string value = "*** missing ***";
            try
            {
                value = ConfigurationManager.AppSettings[settingName].Trim();
                if (String.IsNullOrWhiteSpace(value))
                {
                    EventLog.WriteEntry(String.Format("Required setting \"{0}\" was not found in the service configuration file.", settingName), EventLogEntryType.Error, (int)ServiceEventIds.Configuration_Invalid);
                    hasRequiredSettings = false;
                }
            }
            catch (ConfigurationErrorsException)
            {
                EventLog.WriteEntry(String.Format("Unable to read service configuration file for required setting \"{0}\".", settingName), EventLogEntryType.Error, (int)ServiceEventIds.Configuration_Invalid);
                hasRequiredSettings = false;
            }
            return value;
        }

        /// <summary>
        /// Get a boolean valued configuration setting, or write an error to the
        /// EventLog if the setting cannot be loaded.
        /// </summary>
        /// <param name="settingName"></param>
        /// <param name="hasRequiredSettings"></param>
        /// <returns></returns>
        private bool LoadBooleanSetting(string settingName, ref bool hasRequiredSettings)
        {
            bool value = false;
            try
            {
                string s = ConfigurationManager.AppSettings[settingName].Trim();
                if (String.IsNullOrWhiteSpace(s))
                {
                    EventLog.WriteEntry(String.Format("Required setting \"{0}\" was not found in the service configuration file.", settingName), EventLogEntryType.Error, (int)ServiceEventIds.Configuration_Invalid);
                    hasRequiredSettings = false;
                }
                else
                {
                    value = (s.ToLower() == "true");
                }
            }
            catch (ConfigurationErrorsException)
            {
                EventLog.WriteEntry(String.Format("Unable to read service configuration file for required setting \"{0}\".", settingName), EventLogEntryType.Error, (int)ServiceEventIds.Configuration_Invalid);
                hasRequiredSettings = false;
            }

            return value;
        }

        /// <summary>
        /// Method to initialize service property values.
        /// </summary>
        private void InitializeComponent()
        {
            this.ServiceName = "FileLizardService";
            this.CanStop = true;
            this.AutoLog = false;
            this.EventLog.Log = "Application";
            this.EventLog.Source = "FileLizard Service";
        }
    }
}
