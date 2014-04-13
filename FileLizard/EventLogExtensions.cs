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
using System.Diagnostics;
using System.Text;

namespace FileLizard
{
    /// <summary>
    /// Enumerated type to codify error codes for the EventLog.
    /// </summary>
    internal enum ServiceEventIds
    {
        Configuration_Invalid = 1000,
        Initialization_Failure = 2000,
        Initialization_Dir_NotExist = 2010,
        Initialization_Dir_NoAccess = 2020,
        Initialization_Dir_OtherError = 2030,
        SMTP_Failure = 3000,
        Start_Failure = 4000,
        Stop_Failure = 5000,
        Watcher_Error = 6000,
        Watcher_BufferOverflow = 6010
    }

    /// <summary>
    /// A class to contain extension methods for working with EventLogs.
    /// </summary>
    public static class EventLogExtensions
    {
        /// <summary>
        /// A method to write details about an Exception to the EventLog.
        /// With thanks to Mike Christian (for more see: 
        /// http://thecurlybrace.blogspot.ca/2010/12/how-to-create-c-windows-service.html)
        /// </summary>
        /// <param name="log"></param>
        /// <param name="message"></param>
        /// <param name="type"></param>
        /// <param name="eventId"></param>
        /// <param name="ex"></param>
        public static void WriteEntry(this EventLog log, string message, EventLogEntryType type, int eventId, Exception ex)
        {
            if (String.IsNullOrWhiteSpace(message))
                throw new ArgumentException("message is null or empty.", "message");

            if (ex == null)
                throw new ArgumentNullException("ex", "ex is null.");

            string separator = new String('-', 50);
            StringBuilder builder = new StringBuilder(message);

            // Write each of the inner exception messages.
            Exception parentException = ex;
            bool isInnerException = false;
            do
            {
                builder.AppendLine();
                builder.AppendLine(separator);
                if (isInnerException)
                    builder.Append("INNER ");
                builder.AppendLine("EXCEPTION DETAILS");
                builder.AppendLine();
                builder.Append("EXCEPTION TYPE:\t");
                builder.AppendLine(parentException.GetType().ToString());
                builder.AppendLine();
                builder.Append("EXCEPTION MESSAGE:\t");
                builder.AppendLine(parentException.Message);
                builder.AppendLine();
                builder.AppendLine("STACK TRACE:");
                builder.AppendLine(parentException.StackTrace);
                if (parentException.InnerException != null)
                {
                    parentException = parentException.InnerException;
                    isInnerException = true;
                }
                else
                    parentException = null;
            }
            while (parentException != null);

            log.WriteEntry(builder.ToString(), type, eventId);
        }
    }
}
