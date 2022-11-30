using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditorInternal;
using System.Text.RegularExpressions;

namespace ComponentsBind
{
    [CustomEditor(typeof(UIComponentsBind))]
    public class UIComponentsBindInspector : Editor
    {
        private readonly int singleItemHeight = 53;
        private readonly int singlePropertyHeight = 20; 

        SerializedProperty elements;
        ReorderableList reorderableList;
        UIComponentsBind componentsBind;

        private void OnEnable()
        {
            elements = serializedObject.FindProperty("elements");
            reorderableList = new ReorderableList(serializedObject, elements, true, true, true, true);
        }

        public override void OnInspectorGUI()
        {
            componentsBind = target as UIComponentsBind;

            reorderableList.elementHeight = singleItemHeight;

            reorderableList.drawHeaderCallback = DrawHeader;
            reorderableList.drawElementCallback = DrawElement;
            reorderableList.onAddCallback = DrawOnAddCallBack;
            reorderableList.onRemoveCallback = DrawOnRemoveCallBack;

            DragControl();

            serializedObject.Update();
            reorderableList.DoLayoutList();
            serializedObject.ApplyModifiedProperties();

            WriteLuaCode();
        }

        private void OnDisable() 
        {
            if (componentsBind)
            {
                EditorUtility.SetDirty(componentsBind);
            }
        }

        void DragControl() 
        {
            EditorGUILayout.Space();

            GUI.color = Color.green;
            var are = GUILayoutUtility.GetRect(0, 50, GUILayout.ExpandWidth(true));
            GUIContent des = new GUIContent("\n拖拽组件对象到此可快速绑定(可多选)");
            GUI.Box(are, des);

            if (are.Contains(Event.current.mousePosition)) 
            {
                DragAndDrop.AcceptDrag();
                DragAndDrop.visualMode = DragAndDropVisualMode.Copy;

                switch (Event.current.type)
                {
                    case EventType.DragPerform:
                        if (are.Contains(Event.current.mousePosition))
                        {
                            for (int i = 0; i < DragAndDrop.objectReferences.Length; ++i)
                            {
                                GameObject reference = DragAndDrop.objectReferences[i] as GameObject;
                                AddElementsToList(reference);
                            }
                        }
                        break;
                    default:
                        break;
                }
            }
           
            GUI.color = Color.white;
            EditorGUILayout.Space();
        }
    
        void DrawHeader(Rect rect)
        {
            string name = "componts";
            EditorGUI.LabelField(rect, name);
        }
    
        void DrawElement(Rect rect, int index, bool isActive, bool isFocused)
        {
            SerializedProperty element = reorderableList.serializedProperty.GetArrayElementAtIndex(index);
            SerializedProperty fieldName = element.FindPropertyRelative("fieldName");
            SerializedProperty compont = element.FindPropertyRelative("selectedComponent");
    
            Rect rectFieldName = new Rect(rect.x, rect.y + 3, rect.width, singlePropertyHeight);
            Rect rectCompont = new Rect(rect.x, rect.y + singlePropertyHeight + 6, rect.width/2, singlePropertyHeight);
            Rect rectCompontEnum = new Rect(rect.x + rect.width/2 + 5, rect.y + singlePropertyHeight + 6, rect.width/2 - 4, singlePropertyHeight);
    
            EditorGUI.PropertyField(rectFieldName, fieldName, GUIContent.none);
            EditorGUI.PropertyField(rectCompont, compont, GUIContent.none);
    
            Element curElement = componentsBind.elements[index];
            Component curElementSelectedComponent = curElement.selectedComponent;
            if (curElementSelectedComponent)
            {
                Component[] allComponent = curElement.GetElementAllComponents();
                List<string> tempCompontNames = GetElementAllComponentsStringList(allComponent);

                int newSelectIndex = EditorGUI.Popup(rectCompontEnum, curElement.selectedComponentIndex, tempCompontNames.ToArray());
                if (newSelectIndex != curElement.selectedComponentIndex)
                {
                    curElement.SetSelectedComponentIndex(newSelectIndex);
                    curElement.fieldName = curElement.selectedComponent.name + tempCompontNames[curElement.selectedComponentIndex];
                }
                else if(curElement.fieldName == string.Empty)
                {
                    curElement.fieldName = curElement.selectedComponent.name + tempCompontNames[curElement.selectedComponentIndex];
                }

                if (curElement.selectedComponentIndex >= allComponent.Length)
                {
                    curElement.SetSelectedComponentIndex(allComponent.Length - 1);
                }
                else
                {
                    curElement.SetSelectedComponentIndex(curElement.selectedComponentIndex);
                }

                curElement.selectedComponent = allComponent[curElement.selectedComponentIndex];
            }
            else
            {
                curElement.fieldName = string.Empty;
            }
        }

        // string GetComponentName(Element curElement,List<string> tempCompontNames)
        // {
        //     if (curElement.selectedComponent.name == "aa")
        //     {
                
        //     }
        //     return curElement.selectedComponent.name + tempCompontNames[curElement.selectedComponentIndex];
        // }

        List<string> GetElementAllComponentsStringList(Component[] allComponent)
        {
            List<string> tempCompontNames = new List<string>();

            foreach (Component item in allComponent)
            {
                string compontType = Regex.Replace(item.ToString(), @"(.*\()(.*)(\).*)", "$2");
                string[] temp = compontType.Split('.');
                if (temp.Length > 0)
                    tempCompontNames.Add(temp[temp.Length - 1]);
                else
                    tempCompontNames.Add(compontType);
            }

            return tempCompontNames;
        }

        void DrawOnAddCallBack(ReorderableList list)
        {
            componentsBind.elements.Add(new Element());
        }

        void DrawOnRemoveCallBack(ReorderableList list)
        {
            componentsBind.elements.RemoveAt(list.index);
        }

        void AddElementsToList(GameObject component) 
        {
            if (componentsBind.elements == null)
            {
                componentsBind.elements = new List<Element>();
            }

            for (int i = 0; i < componentsBind.elements.Count; i++)
            {
                Element element = componentsBind.elements[i];
                // Debug.LogError(string.Format("i:  {0}    ele"));
                if (element.selectedComponent == null)
                {
                    element.selectedComponent = component.GetComponent<Component>();
                    return;
                }
            }

            componentsBind.elements.Add(new Element(component.GetComponent<Component>()));
        }

        void WriteLuaCode()
        {
            EditorGUILayout.Space();

            GUI.color = Color.green;

            if (GUILayout.Button("copy code for lua init"))
            {
                LuaCodeExport.WriteLuaInit(componentsBind);
            }

            GUI.color = Color.cyan;
            if (GUILayout.Button("copy code for lua unInit"))
            {
                LuaCodeExport.WriteLuaUnInit(componentsBind);
            }

            GUI.color = Color.green;
            if (GUILayout.Button("copy code for lua buttonFunction"))
            {
                LuaCodeExport.WriteLuaButtonFunction(componentsBind);
            }
        }
    }
}