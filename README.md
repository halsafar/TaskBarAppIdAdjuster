# TaskBar AppID Adjuster

Automatically adjust the Application ID of any Process with a visible TaskBar entry.  Which processes to adjust and what action to take is configurable.

## Install

- Download the latest release here.
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

