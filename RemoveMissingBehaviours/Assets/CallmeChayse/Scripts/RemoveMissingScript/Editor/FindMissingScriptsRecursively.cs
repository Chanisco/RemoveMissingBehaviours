using UnityEngine;
using System.Collections.Generic;
using UnityEditor;

public class FindMissingScriptsRecursively: EditorWindow {
    static int go_count = 0, components_count = 0, missing_count = 0,removed_count = 0;
    static string currentGameobject;


    Vector2 scrollPos;

    static List<string> missingComponents = new List<string>();


    [MenuItem("Window/CallmeChayse/RemoveMissingElements/FindMissingPop-up")]
    public static void ShowWindow() {
        EditorWindow.GetWindow(typeof(FindMissingScriptsRecursively));
    }
    
    public void OnGUI() {
        GUILayout.Label(missing_count + " x Amount of errors in " + currentGameobject);

        if (GUILayout.Button("Find Missing Scripts in",GUILayout.Width(Screen.width))) {
            missingComponents.Clear();
            FindInSelected();
        }
        if (missing_count > 0) {
            EditorGUILayout.BeginVertical();
            scrollPos = EditorGUILayout.BeginScrollView(scrollPos,true,true,GUILayout.MaxWidth(200),GUILayout.MaxHeight(100),GUILayout.MinHeight(10));
            for (int i = 0;i < missingComponents.Count;i++) {
                GUILayout.Label("Component missing at: " + missingComponents[i]);
            }
            EditorGUILayout.EndScrollView();
            if (GUILayout.Button("Remove missing objects",GUILayout.Width(Screen.width))) {
                  RemoveMissingScripts();
            }
        }

    }

    private static void RemoveMissingScripts() {
        GameObject[] go = Selection.gameObjects;
        currentGameobject = go[0].name;
        go_count = 0;
        components_count = 0;
        missing_count = 0;
        foreach (GameObject g in go) {
            RemoveInGo(g);
        }
        Debug.Log("<color=darkblue> removed " + missing_count + " missing scripts from this object , Pass the love</color>");

    }

    private static void FindInSelected() {
        GameObject[] go = Selection.gameObjects;
        currentGameobject = go[0].name;
        go_count = 0;
        components_count = 0;
        missing_count = 0;
        foreach (GameObject g in go) {
            FindInGO(g);
        }
        //Debug.Log();
    }

    private static void FindInGO(GameObject g) {
        go_count++;
        Component[] components = g.GetComponents<Component>();
        for (int i = 0;i < components.Length;i++) {
            components_count++;
            if (components[i] == null) {
                missing_count++;
                string s = g.name;
                Transform t = g.transform;
                while (t.parent != null) {
                    s = t.parent.name + "/" + s;
                    t = t.parent;
                }
                //temp.path = string.Format("Searched {0} GameObjects, {1} components, found {2} missing",go_count,components_count,missing_count);
                missingComponents.Add(s);
                //Debug.Log(s + " has an empty script attached in position: " + i,g);
            }
        }
        // Now recurse through each child GO (if there are any):
        foreach (Transform childT in g.transform) {
            //Debug.Log("Searching " + childT.name  + " " );
            FindInGO(childT.gameObject);
        }
    }

    private static void RemoveInGo(GameObject g) {
        Component[] components = g.GetComponents<Component>();
        removed_count = 0;
        for (int i = 0;i < components.Length;i++) {
            var serializedObject = new SerializedObject(g);
            var prop = serializedObject.FindProperty("m_Component");
            if (components[i] == null) {

                prop.DeleteArrayElementAtIndex(i - removed_count);
                removed_count++;
            }
            serializedObject.ApplyModifiedProperties();
        }       

        foreach (Transform childT in g.transform) {
            RemoveInGo(childT.gameObject);
        }
     }
    
    [MenuItem("Window/CallmeChayse/RemoveMissingElements/SelectMissingScripts-InScene")]
    static void SelectMissing(MenuCommand command)
    {
        Transform[] ts = FindObjectsOfType<Transform>();
        List<GameObject> selection = new List<GameObject>();
        foreach (Transform t in ts) {
            Component[] cs = t.gameObject.GetComponents<Component>();
            foreach (Component c in cs) {
                if (c == null) {
                    selection.Add(t.gameObject);
                }
            }
        }
        Selection.objects = selection.ToArray();
    }


    [MenuItem("Window/CallmeChayse/RemoveMissingElements/Cleanup Missing-Scripts in scene")]
    static void CleanupMissingScripts() {
        removed_count = 0;
        for (int i = 0;i < Selection.gameObjects.Length;i++) {
            var gameObject = Selection.gameObjects[i];
            var components = gameObject.GetComponents<Component>();
            var serializedObject = new SerializedObject(gameObject);
            var prop = serializedObject.FindProperty("m_Component");
            int r = 0;
            for (int j = 0;j < components.Length;j++) {
                if (components[j] == null) {
                    prop.DeleteArrayElementAtIndex(j - r);
                    r++;
                    removed_count++;
                }
            }

            serializedObject.ApplyModifiedProperties();
        }
        Debug.Log("<color=darkblue> removed missing scripts from the scene (removed " + removed_count + ")</color>");
    }
     
}