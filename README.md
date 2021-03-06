# TaskBar AppID Adjuster

Automatically adjust the Application ID of any Process with a visible TaskBar entry.  Which processes to adjust and what action to take is configurable.  This software runs in the background and continues to watch the task bar to update any new windows that match a configuration. 

## Status

- Alpha!
- Working for the basic happy path.

## Reason

Some applications group on the task bar when they shouldn't.  Windows doesn't let you specifically disable grouping for certain applications.  7+ TaskBar Tweaker lets you get most of the way there but it didn't automate the process.

## Install

- [Download the latest release](https://github.com/halsafar/TaskBarAppIdAdjuster/releases/latest).
- Unzip to any location.

## Usage
- Launch TaskBarIdAdjuster.exe
- On first launch a config file named `config.json` will be created with an example entry.
- A simple tray icon is the only indication it is running.  No need to clutter the taskbar. 
- The config file is checked for changes every iteration of the main loop.  You can edit while it is running.

### Basic Steps
* Open config.json
* Use the default `notepad` entry as inspiration.
* Name: Add the basename of the process without extension.  So `C:\Foo\Bar.exe` becomes `Bar`
* Action:
  * 0 = Ungroup, this will forcefully ungroup on the taskbar any apps that match the process name.
  * 1 = Group, currently unsupported.
 
## Details

The Windows task bar decides how to group application based on their ApplicationID. This ID usually comes from the application itself.  The developer decides whether to set the ID the same for each Window to cause grouping or to give it a unique ID to forcefully ungroup it.  
Using the WindowsCodePack.Shell API we can communicate with the TaskBar easily from C#. By fetching a list of processes by name we can change the ID on a per process basis for each Window that process has spawned.

This tool simply wakes up every X seconds, checks to see if any new Windows need to have their IDs adjusted.

