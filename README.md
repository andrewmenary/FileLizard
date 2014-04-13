# FileLizard README

## What is FileLizard?

A windows service to monitor a directory (and subdirectories if desired) 
and report via an email alert when a new file is added, or an existing file 
is changed, or deleted.

This project was created in Microsoft Visual Studio Express 2013 for Windows 
Desktop and uses the .NET 4.5.1 Framework.

This code is protected under the *MIT* license (see *COPYING*).

## How to Install

* Clone the repository locally.
* Build the project using the *Release* configuration.
* Copy the files in the *"FileLizard\bin\Release"* folder to *"C:\Program Files\FileLizard"*.
* Modify the *FileLizard.exe.config* file as appropriate.
* Open an *Administrator Command Prompt* window and *cd* to *"C:\Program Files\FileLizard"*.
* Run *install.bat* to install the windows service.
* Open *services.msc*, locate the FileLizard service and start it.
* If desired, change the Startup Type to *Automatic* from *Manual* to have the 
service survive a system restart.
