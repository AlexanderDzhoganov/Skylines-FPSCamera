using System;
using UnityEngine;

namespace FPSCamera
{

    public class Debugger
    {

        private static Debugger instance = null;

        public static void Initialize()
        {
            instance = new Debugger();
        }

        public static void Log(string s)
        {
            instance.LogInternal(s);    
        }

        public DebugConsole console = null;
        private GameObject gameObject = null;

        public void LogInternal(string s)
        {
            if (console == null)
            {
                gameObject = new GameObject();
                console = gameObject.AddComponent<DebugConsole>();
                console.debugger = this;
            }

            console.Log(s);
        }

    }

    public class DebugConsole : MonoBehaviour
    {

        public Debugger debugger;
        private Rect windowRect = new Rect(16, 16, 380, 500);
        private Vector2 scrollPosition = Vector2.zero;
        private string text = "";
        private bool showConsole = true;

        private void OnGUI()
        {
            if (!showConsole)
            {
                return;
            }
            
            windowRect = GUI.Window(12521, windowRect, MainWindowFunc, "Console");
        }

        private void MainWindowFunc(int windowID)
        {
            GUI.DragWindow();
            scrollPosition = GUILayout.BeginScrollView(scrollPosition);
            GUILayout.Label(text);
            GUILayout.EndScrollView();
        }

        public void Log(string s)
        {
            text = String.Format("{0} * {1}\n", text, s);
        }

        void OnDestroy()
        {
            debugger.console = null;
        }

        void Update()
        {
            if (Input.GetKey(KeyCode.LeftControl) && Input.GetKeyDown(KeyCode.C))
            {
                showConsole = !showConsole;
            }
        }

    }

}
