using System.Collections.Generic;
using UnityEngine;
using UnityEditor;


namespace ComponentsBind
{
    public class UIComponentsBindHierarchy
    {
        // [InitializeOnLoadMethod]
        // private static void UIComponentsBindHierachyOnLoad()
        // {
        //     UnityEditor.EditorApplication.hierarchyWindowItemOnGUI += OnHierarchyWindowItemOnGUI;
        // }

        // private static void OnHierarchyWindowItemOnGUI(int instanceID, Rect selectionRect)
        // {
        //     if (Application.isPlaying)
        //         return;

        //     GameObject go = UnityEditor.EditorUtility.InstanceIDToObject(instanceID) as GameObject;
        //     if (go == null)
        //         return;

        //     UIComponentsBind componentsBind = go.GetComponent<UIComponentsBind>();
        //     if (componentsBind != null)
        //     {
        //         List<Element> elements = componentsBind.elements;
        //         if (elements != null && elements.Count > 0)
        //         {
        //             foreach (Element item in elements)
        //             {
        //                 Debug.LogError("elementName:  " + item.selectedComponent.gameObject.name + "     go:  " + go.name);
        //                 if (item.selectedComponent.gameObject.GetInstanceID() == instanceID)
        //                 {
        //                     Debug.LogError("相等");

        //                     var r = new Rect(selectionRect);
        //                     r.x = 34;
        //                     r.width = 80;
        //                     GUIStyle style = new GUIStyle();
        //                     style.normal.textColor = Color.yellow;
        //                     style.active.textColor = Color.red;
        //                     if (style != null && go != null)
        //                     {
        //                         GUI.Label(r, "★", style);
        //                     }
        //                 }
        //             }
        //         }
        //     }
        // }

        // void ShowChildItem(GameObject go, UIComponentsBind componentsBind) 
        // {
        //     if (go.transform.childCount > 0)
        //     {
        //         //ShowChildItem();
        //     }
        // }
    }
}