using System;
using UnityEditor;
using UnityEngine;

namespace FavouritesEd
{
    public class TextInputWindow : EditorWindow
    {
        private static GUIStyle BottomBarStyle;
        private bool accepted;
        private Action<TextInputWindow> callback;
        private string label;
        private bool lostFocus;
        public string Text { get; private set; }
        public object[] Args { get; private set; }

        private void Update()
        {
            if (lostFocus) Close();
            if (accepted && callback != null) callback(this);
        }

        private void OnGUI()
        {
            if (BottomBarStyle == null)
                BottomBarStyle = new GUIStyle(GUI.skin.FindStyle("ProjectBrowserBottomBarBg"))
                {
                    padding = new RectOffset(3, 3, 8, 8), stretchHeight = false, stretchWidth = true, fixedHeight = 0,
                    fixedWidth = 0, richText = false
                };

            EditorGUILayout.Space();
            GUILayout.Label(label);
            Text = EditorGUILayout.TextField(Text);
            GUILayout.FlexibleSpace();
            EditorGUILayout.BeginHorizontal(BottomBarStyle);
            {
                GUILayout.FlexibleSpace();
                if (GUILayout.Button("Accept", GUILayout.Width(80))) accepted = true;
                GUILayout.Space(5);
                if (GUILayout.Button("Cancel", GUILayout.Width(80))) Close();
                GUILayout.FlexibleSpace();
            }
            EditorGUILayout.EndHorizontal();
        }

        private void OnFocus()
        {
            lostFocus = false;
        }

        private void OnLostFocus()
        {
            lostFocus = true;
        }

        public static void ShowWindow(string title, string label, string currText, Action<TextInputWindow> callback,
            object[] args, float width = 250)
        {
            var win = GetWindow<TextInputWindow>(true, title, true);
            win.label = label;
            win.Text = currText;
            win.callback = callback;
            win.Args = args;
            win.minSize = win.maxSize = new Vector2(width, 100);
            win.ShowUtility();
        }
    }
}