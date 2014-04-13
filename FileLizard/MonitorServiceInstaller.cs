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

//NOTE: Some parts of this code file derived from code written by Mike Christian
//and published at http://thecurlybrace.blogspot.ca/2010/12/how-to-create-c-windows-service.html

using System;
using System.ServiceProcess;
using System.ComponentModel;
using System.Configuration.Install;

namespace FileLizard
{
    [RunInstaller(true)]
    public class MonitorServiceInstaller: Installer
    {
        private ServiceProcessInstaller serviceProcessInstaller;
        private ServiceInstaller serviceInstaller;
        private const String serviceName = "FileLizardService";

        public MonitorServiceInstaller() : base()
        {
            this.AfterInstall += new InstallEventHandler(MonitorServiceInstaller_AfterInstall);
            this.BeforeInstall += new InstallEventHandler(MonitorServiceInstaller_BeforeInstall);
            this.BeforeUninstall += new InstallEventHandler(MonitorServiceInstaller_BeforeUninstall);

            InitializeComponent();
        }

        private void InitializeComponent()
        {
            this.serviceProcessInstaller = new ServiceProcessInstaller();
            this.serviceProcessInstaller.Account = ServiceAccount.LocalSystem;
            this.serviceProcessInstaller.Username = null;
            this.serviceProcessInstaller.Password = null;
            this.serviceInstaller = new ServiceInstaller();
            this.serviceInstaller.Description = "A service to monitor a directory for changes and send alerts.";
            this.serviceInstaller.DisplayName = "FileLizard Service";
            this.serviceInstaller.ServiceName = serviceName;
            this.serviceInstaller.StartType = ServiceStartMode.Manual;
            this.Installers.AddRange(new Installer[] { this.serviceProcessInstaller, this.serviceInstaller });
        }

        private void MonitorServiceInstaller_BeforeInstall(object sender, InstallEventArgs e)
        {
            // Before installation, make sure existing service is stopped.
            SetServiceStatus(false);
        }

        private void MonitorServiceInstaller_AfterInstall(object sender, InstallEventArgs e)
        {
            // After installation, optionally start the service.
            SetServiceStatus(false);
        }

        private void MonitorServiceInstaller_BeforeUninstall(object sender, InstallEventArgs e)
        {
            // Before uninstalling stop the service.
            SetServiceStatus(false);
        }

        private void SetServiceStatus(bool startService)
        {
            ServiceController serviceController;

            // Determine the desired state
            ServiceControllerStatus setStatus = startService ? ServiceControllerStatus.Running : ServiceControllerStatus.Stopped;

            try
            {
                serviceController = new ServiceController(serviceName);

                // If the service exists and current status is not the desired status then either start or stop it
                if (serviceController != null && serviceController.Status != setStatus)
                {
                    if (startService)
                        serviceController.Start();
                    else
                        serviceController.Stop();
                    serviceController.WaitForStatus(setStatus, new TimeSpan(0, 0, 30));
                }
            }
            catch (Exception)
            {
                //Eat any error.
                //throw;
            }
        }
    }
}
