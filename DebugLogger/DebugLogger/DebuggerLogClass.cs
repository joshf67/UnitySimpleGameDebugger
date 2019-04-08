using System;
using System.IO;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace DL
{
    //store message and message colour data
    internal struct DebugType
    {

        public string message;
        public string messageColour;

        public DebugType(string _msg, string _msgColour)
        {
            message = _msg;
            messageColour = _msgColour;
        }

    }

    public class DebugBox : MonoBehaviour
    {
        //store basic variables of a debugBox
        private string BoxName;
        internal List<DebugType> textLines;

        private Vector2 virtualSize;
        private int maxLines;
        private float currentScroll;
        private int offset;

        private Rect windowRect;
        private int GUIID;

        private bool showGUI;
        private bool writeToDisk;

        public DebugBox(string _boxName, Rect _rect, int _maxStoredLines, int _GUIID, bool _writeToDisk = true)
        {
            //setup basic variables required
            BoxName = _boxName;
            showGUI = false;
            writeToDisk = _writeToDisk;

            offset = 0;
            currentScroll = maxLines;

            GUIID = _GUIID;
            maxLines = _maxStoredLines;
            textLines = new List<DebugType>();

            //used to scale GUI at a later date
            windowRect = _rect;
            virtualSize = new Vector2(1920, 1080);

            if (_writeToDisk)
            {

                //Setup debug text folder
                Debug.SetupDirectory();

                //Setup debug text file
                StreamWriter writer = new StreamWriter("mods/Debug/" + BoxName + ".txt", false);
                writer.Close();

            }
        }

        internal void AddText(string _msg, string _msgColour)
        {

            textLines.Insert(0, new DebugType(_msg, _msgColour));

            //debug info to file
            if (writeToDisk)
            {
                StreamWriter writer = new StreamWriter("mods/Debug/" + BoxName + ".txt", true);
                writer.WriteLine("Message: " + _msg);
                writer.Close();
            }

            if (textLines.Count > maxLines)
            {
                textLines.RemoveAt(maxLines);
            }

        }

        //toggle GUI on/off
        internal void ToggleGUI(bool _enabled)
        {
            showGUI = _enabled;
        }

        //Clear all text
        internal void ClearText()
        {
            textLines.Clear();
        }

        //Draw GUI
        public void OnGUI()
        {

            //scale UI to current resolution
            GUI.matrix = Matrix4x4.TRS(Vector3.zero, Quaternion.identity, new Vector3(Screen.width / virtualSize.x, Screen.height / virtualSize.y, 1));

            if (showGUI)
            {
                //create debugWindow
                windowRect = GUI.Window(GUIID, windowRect, WindowFunction, BoxName);
            }

        }

        internal void WindowFunction(int windowID)
        {

            //allow window to be dragged
            GUI.DragWindow(new Rect(0, 0, windowRect.width, 20));

            //begin scroll area
            GUILayout.BeginArea(new Rect(windowRect.width - 25, 30, 20, windowRect.height - 40));

            //display offset scroll to screen
            currentScroll = GUILayout.VerticalScrollbar(currentScroll, 10, maxLines + 10, 24, GUILayout.Height(windowRect.height - 40));
            offset = (int)(maxLines - currentScroll);
            GUILayout.EndArea();

            //begin text area
            GUILayout.BeginArea(new Rect(10, 30, windowRect.width - 50, windowRect.height - 40));

            //setup gui style for colours/rich txt
            GUIStyle style = new GUIStyle();
            style.richText = true;

            //setup text and loop through all available text
            string output = "";

            for (int a = offset; a < textLines.Count; a++)
            {
                //check if output is past a certain length to stop the
                //text getting cut off mid way
                if (output.Length > 1500)
                {
                    break;
                }
                //add colour + message + newline to text
                output += "<color=#" + textLines[a].messageColour + ">" + textLines[a].message + "</color>";
                output += Environment.NewLine;
            }

            //print text and then end the area
            GUILayout.TextArea(output, 2000, style, GUILayout.Height(windowRect.height - 40));
            GUILayout.EndArea();

        }

    }

    //static class to call debug functions
    public static class Debug
    {
        //store debug box variables
        private static Dictionary<string, DebugBox> debugPanels = new Dictionary<string, DebugBox>();
        private static int currentGUIID = 0;
        private static GameObject m_obj;

        //setup base folder
        internal static void SetupDirectory()
        {
            if (!Directory.Exists("mods/Debug"))
            {
                Directory.CreateDirectory("mods/Debug");
            }
        }

        //setup base game object
        internal static void SetupObject()
        {
            if (m_obj == null)
            {
                m_obj = new GameObject("DebugLogger");
                GameObject.DontDestroyOnLoad(m_obj);
            }
        }

        //toggle the panel UI on/off
        public static void ToggleUI(string _panel, bool _enabled, bool _writeToDisk = true)
        {
            DebugBox temp = Find(_panel, _writeToDisk, true);
            if (temp != null)
            {
                temp.ToggleGUI(_enabled);
            }
        }

        //try to return existing DebugBox
        public static DebugBox Find(string _panel = "Default", bool _writeToDisk = true, bool justSearching = false)
        {

            DebugBox ret;

            //check if debugPanel exists
            if (debugPanels.TryGetValue(_panel, out ret))
            {
                return ret;
            }

            if (justSearching)
            {
                return null;
            }

            //return newly created debug
            return Create(new Rect(400, 400, 400, 400), _panel, _writeToDisk);

        }

        //create debug box
        public static DebugBox Create(Rect _rect, string _panel = "Default", bool _writeToDisk = true, bool _enabledOnCreation = true)
        {
            DebugBox ret;

            //try to see if panel exists
            if (debugPanels.TryGetValue(_panel, out ret))
            {
                return ret;
            }

            //check if master object is needed
            SetupObject();

            //create an object and add DebugBox component to it
            GameObject DebugObj = new GameObject("DebugObj");
            GameObject.DontDestroyOnLoad(DebugObj);
            DebugObj.transform.parent = m_obj.transform;
            ret = DebugObj.AddComponent<DebugBox>(new DebugBox(_panel, _rect, 100, currentGUIID++, _writeToDisk));
            ret.ToggleGUI(_enabledOnCreation);


            //add panel then return
            debugPanels.Add(_panel, ret);
            return ret;
        }

        //Log text in white
        public static void Log(object _obj, string _panel = "Default", bool _writeToDisk = true)
        {
            LogColour(_obj, "ffffffff", _panel, _writeToDisk);
        }

        //log text in yellow
        public static void Warning(object _obj, string _panel = "Default", bool _writeToDisk = true)
        {
            LogColour(_obj, "ffff00ff", _panel, _writeToDisk);
        }

        //log text in red
        public static void Error(object _obj, string _panel = "Default", bool _writeToDisk = true)
        {
            LogColour(_obj, "ff0000ff", _panel, _writeToDisk);
        }

        //log text in colour given
        public static void LogColour(object _obj, string _colour, string _panel = "Default", bool _writeToDisk = true)
        {

            DebugBox _chatPanel = Find(_panel, _writeToDisk);
            _chatPanel.AddText(_obj.ToString(), _colour);
        }
    }
}
