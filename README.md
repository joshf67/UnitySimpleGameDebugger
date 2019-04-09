# OLogger

## OLogger Example
```csharp

//All of these functions are called from the static class OLogger

//Here is the main Log function:
OLogger.Log(object _obj, string _color = "ffffffff", string _panel = "Default")

//_obj is the object you want to display, it will be turned into a string in the log function.
//_color is the color that the text will be, which by default will be white.
//_panel is the panel that the text will be output to.

//Other functions:
//This isn't actually needed as .Log will create a panel, however, this give you control over the writeToDisk/enabledOnCreation
OLogger.CreateLog(Rect _rect, string _panel = "Default", bool _writeToDisk = true, bool _enabledOnCreation = true);

//_writeToDisk is whether you want to output any text in this panel to a file in "mods/Debug/'PanelName'.txt"
//treat writing to disk in an update loop the same as calling Debug.Log.

OLogger.SetUIPanelEnabled(string _panel, bool _enabled); //this will set the panel "_panel" to "enabled"
OLogger.SetPanelWriteToDisk(string _panel, bool _writeToDisk) //this will set writeToDisk to "_writeToDisk"

OLogger.ClearUIPanel(string _panel); //this will clear the text in the "_panel" panel
OLogger.DestroyUIPanel(string _panel); //this will destroy the "_panel" panel;

OLogger.Warning(object _obj, string _panel = "Default"); //this will output yellow text to the "_panel" panel
OLogger.Error(object _obj, string _panel = "Default"); //this will output red text to the "_panel" panel

//Example Turn Unity Debug Into OLogger Debug:
public void Update()
{
	
	//First setup "Default Unity Compiler" panel at location: (X:400,Y:400)
	//	size: (W:400,H:400) and have it log to file (mods/Debug/"PanelName".txt) and be enabled on start
	OLogger.CreateLog(new Rect(400, 400, 400, 400), "Default Unity Compiler", true, true);
	
	//If you want to also debug Unity's stack trace then call this
	OLogger.CreateLog(new Rect(400, 400, 400, 400), "Default Unity Stack Trace", true, true);

	//Add ignores to OLogger ignore list (will filter out from Unity's Debug calls)
	OLogger.ignoreList.AddToIgnore("Internal", "Failed to create agent"
								  , "is registered with more than one LODGroup"
								  , "No AudioManager"); 
	//ignores can also be removed by calling OLogger.ignoreList.RemoveFromIgnore()

	//Finally hook OLogger onto logMessageReceived to receive Unity's Debug calls
	Application.logMessageReceived += OLogger.DebugMethodHook;

}
```