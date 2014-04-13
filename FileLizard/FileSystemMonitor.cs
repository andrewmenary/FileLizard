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
using System.Diagnostics;
using System.IO;
using System.Net.Mail;

namespace FileLizard
{
    /// <summary>
    /// A class which implements a solution for monitoring a file system
    /// directory for changes.  When a subscribed event occurs, an email
    /// alert is generated.
    /// </summary>
    public class FileSystemMonitor
    {
        private FileSystemWatcher fsWatcher;
        private EventLog log;
        private MonitorSettings settings;

        /// <summary>
        /// Custom constructor which receives an eventLog parameter and 
        /// a settings parameter via dependency injection.
        /// </summary>
        /// <param name="eventLog"></param>
        public FileSystemMonitor(EventLog eventLog, MonitorSettings settings)
        {
            //Guard clause for eventLog
            if (eventLog == null)
                throw new ArgumentNullException("eventLog");
            this.log = eventLog;

            //Guard clause for settings
            if (settings == null)
                throw new ArgumentNullException("settings");
            this.settings = settings;

            Start();
        }

        /// <summary>
        /// Called to setup and begin monitoring a directory for changes
        /// </summary>
        private void Start()
        {
            //TODO: Make sure directory to be watched exists before trying to watch it
            //Create an instance of FileSystemWatcher
            fsWatcher = new FileSystemWatcher();

            //Set properties for FileSystemWatcher
            fsWatcher.Path = this.settings.PathToMonitor;
            fsWatcher.IncludeSubdirectories = this.settings.IncludeSubDirectories;
            fsWatcher.NotifyFilter = NotifyFilters.FileName | NotifyFilters.LastWrite;

            //Add event handlers
            if (this.settings.NotifyOnNew)
                fsWatcher.Created += new FileSystemEventHandler(FileCreated);
            if (this.settings.NotifyOnDelete)
                fsWatcher.Deleted += new FileSystemEventHandler(FileDeleted);
            if (this.settings.NotifyOnChange)
                fsWatcher.Changed += new FileSystemEventHandler(FileChanged);
            if (this.settings.NotifyOnRename)
                fsWatcher.Renamed += new RenamedEventHandler(FileRenamed);
            fsWatcher.Error += new ErrorEventHandler(OnError);

            //Enable monitoring
            fsWatcher.EnableRaisingEvents = true;
            log.WriteEntry(String.Concat("Began watching for changes to \"", fsWatcher.Path, "\"."));
        }

        /// <summary>
        /// An event handler that is called when a new file is created.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void FileCreated(object sender, FileSystemEventArgs e)
        {
            log.WriteEntry(String.Concat("New file \"", e.Name, "\" detected.  Full path is \"", e.FullPath, "\"."));
            SendNotification(String.Concat("File \"", e.Name, "\" has been created."),
                String.Concat("New file detected at \"", e.FullPath, "\"."));
        }

        /// <summary>
        /// An event handler that is called when a file is deleted.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void FileDeleted(object sender, FileSystemEventArgs e)
        {
            log.WriteEntry(String.Concat("Existing file \"", e.Name, "\" deleted.  Full path is \"", e.FullPath, "\"."));
            SendNotification(String.Concat("File \"", e.Name, "\" has been deleted."),
                String.Concat("File deleted from \"", e.FullPath, "\"."));
        }

        /// <summary>
        /// An event handler that is called when a file is changed.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void FileChanged(object sender, FileSystemEventArgs e)
        {
            //TODO: Check that the e.FullPath is not a directory before sending notification
            log.WriteEntry(String.Concat("File \"", e.Name, "\" has changed.  Full path is \"", e.FullPath, "\"."));
            SendNotification(String.Concat("File \"", e.Name, "\" has been changed."),
                String.Concat("File at \"", e.FullPath, "\" has been changed."));
        }

        /// <summary>
        /// An event handler that is called when a file is renamed.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void FileRenamed(object sender, RenamedEventArgs e)
        {
            log.WriteEntry(String.Format("File \"{0}\" has been renamed to \"{1}\".", e.OldName, e.Name));
            SendNotification(String.Format("File \"{0}\" has been renamed.", e.OldName),
                String.Concat("File \"{0}\" has been renamed to \"{1}\".", e.OldFullPath, e.FullPath));
        }

        /// <summary>
        /// This method is called when the FileSystemWatcher detects an error.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnError(object sender, ErrorEventArgs e)
        {
            log.WriteEntry("The Monitor Service has detected an error.", EventLogEntryType.Error, (int)ServiceEventIds.Watcher_Error);
            if (e.GetException().GetType() == typeof(InternalBufferOverflowException))
            {
                log.WriteEntry("The file system watcher has experienced an internal buffer overflow: " + e.GetException().Message, EventLogEntryType.Error, (int)ServiceEventIds.Watcher_BufferOverflow);
            }
        }

        /// <summary>
        /// A method to send an email notification.
        /// </summary>
        /// <param name="subject"></param>
        /// <param name="body"></param>
        private void SendNotification(string subject, string body)
        {
            try
            {
                //TODO: Allow sending email to multiple recipients
                MailMessage msg = new MailMessage(
                    this.settings.SendFrom,
                    this.settings.SendTo,
                    subject,
                    body);

                SmtpClient smtp = new SmtpClient(this.settings.SmtpServer);
                smtp.Send(msg);
            }
            catch (Exception e)
            {
                log.WriteEntry("The Monitor Service encountered an error sending notification: " + e.Message, EventLogEntryType.Error, (int)ServiceEventIds.SMTP_Failure);
            }
        }

        /// <summary>
        /// A method to tell the system to stop listening for file system
        /// change events.  Should normally only be invoked by the MonitorService.
        /// </summary>
        public void Stop()
        {
            if (fsWatcher != null)
            {
                fsWatcher.EnableRaisingEvents = false;
                fsWatcher.Dispose();
            }
            log.WriteEntry(String.Concat("Stopped watching for changes to \"", fsWatcher.Path, "\"."));
        }
    }
}
