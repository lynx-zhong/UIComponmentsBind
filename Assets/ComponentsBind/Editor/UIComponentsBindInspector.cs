using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditorInternal;
using System;
using System.Text;
using System.Text.RegularExpressions;
using System.Linq;

namespace ComponentsBind
{
    [CustomEditor(typeof(UIComponentsBind))]
    public class UIComponentsBindInspector : Editor
    {
        private readonly int singleItemHeight = 53;
        private readonly int singlePropertyHeight = 20;
        private static Dictionary<string, string> commonCScriptToLuaScript = new Dictionary<string, string>()
        {
            {"UnityEngine.UI.Text","UIText"},
            {"UnityEngine.UI.Image","UIImage"},
            {"UnityEngine.UI.Button","UIButton"},
        };

        private static Dictionary<string, string> customCScriptToLuaScript = new Dictionary<string, string>()
        {
            {"UnityEngine.UI.Text","UIText"},
            {"UnityEngine.UI.Image","UIImage"},
            {"UnityEngine.UI.Button","UIButton"},
        };
    
        SerializedProperty elements;
        ReorderableList reorderableList;
        UIComponentsBind UIComponentsBind;



        private void OnEnable()
        {
            elements = serializedObject.FindProperty("elements");
            reorderableList = new ReorderableList(serializedObject, elements, true, true, true, true);
            UIComponentsBind = target as UIComponentsBind;

            reorderableList.elementHeight = singleItemHeight;

            reorderableList.drawHeaderCallback = DrawHeader;
            reorderableList.drawElementCallback = DrawElement;
            reorderableList.onAddCallback = DrawOnAddCallBack;
            reorderableList.onRemoveCallback = DrawOnRemoveCallBack;

            // copyLabel.fontSize = 20;
            // copyLabel.normal.textColor = new Color(46f/256f, 163f/256f, 256f/256f, 256f/256f);
        }

        public override void OnInspectorGUI()
        {
            DragControl();

            serializedObject.Update();
            reorderableList.DoLayoutList();
            serializedObject.ApplyModifiedProperties();

            WriteCode();
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
    
            Element curElement = UIComponentsBind.elements[index];
            Component curElementSelectedComponent = curElement.selectedComponent;
            if (curElementSelectedComponent)
            {
                Component[] allComponent = curElement.GetElementAllComponents();
                List<string> tempCompontNames = GetElementAllComponentsStringList(allComponent);

                int newSelectIndex = EditorGUI.Popup(rectCompontEnum, curElement.selectedComponentIndex, tempCompontNames.ToArray());
                if (newSelectIndex != curElement.selectedComponentIndex)
                {
                    curElement.selectedComponentIndex = newSelectIndex;
                    curElement.fieldName = curElement.selectedComponent.name + tempCompontNames[curElement.selectedComponentIndex];
                }
                else if(curElement.fieldName == string.Empty)
                {
                    curElement.fieldName = curElement.selectedComponent.name + tempCompontNames[curElement.selectedComponentIndex];
                }

                curElement.selectedComponent = allComponent[curElement.selectedComponentIndex];
            }
            else
            {
                curElement.fieldName = string.Empty;
            }
        }

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
            UIComponentsBind.elements.Add(new Element());
        }

        void DrawOnRemoveCallBack(ReorderableList list)
        {
            UIComponentsBind.elements.RemoveAt(list.index);
        }

        void AddElementsToList(GameObject component) 
        {
            for (int i = 0; i < UIComponentsBind.elements.Count; i++)
            {
                Element element = UIComponentsBind.elements[i];
                // Debug.LogError(string.Format("i:  {0}    ele"));
                if (element.selectedComponent == null)
                {
                    element.selectedComponent = component.GetComponent<Component>();
                    return;
                }
            }


            UIComponentsBind.elements.Add(new Element(component.GetComponent<Component>()));
        }

        void WriteCode()
        {
            EditorGUILayout.Space();

            GUI.color = Color.green;

            if (GUILayout.Button("copy code for lua init"))
            {
                WriteLuaInit();
            }

            GUI.color = Color.cyan;
            if (GUILayout.Button("copy code for lua unInit"))
            {
                WriteLuaUnInit();
            }
        }


        #region 写lua
        void WriteLuaInit()
        {
            StringBuilder luaCode = new StringBuilder();

            for (int i = 0; i < UIComponentsBind.elements.Count; i++)
            {
                Element element = UIComponentsBind.elements[i];
                WriteLuaInitPathLine(element, luaCode);
            }

            luaCode.AppendLine();

            for (int i = 0; i < UIComponentsBind.elements.Count; i++)
            {
                Element element = UIComponentsBind.elements[i];
                WriteLuaInitLine(element, luaCode);
            }

            EditorGUIUtility.systemCopyBuffer = luaCode.ToString();
        }

        void WriteLuaInitPathLine(Element element,StringBuilder luaCode) 
        {
            if (!element.selectedComponent)
                return;

            string filedStr = element.fieldName == string.Empty ? "Undefined" : FirstCharToLower(element.fieldName);
            StringBuilder nodePath = new StringBuilder();
            nodePath.Append(element.selectedComponent.gameObject.name);
            GetNodePath(element.selectedComponent,nodePath);

            luaCode.AppendLine(string.Format("self._{0}Path = \"{1}\"", filedStr,nodePath.ToString()));
        }

        void GetNodePath(Component component,StringBuilder nodePath)
        {
            UIComponentsBind parentBind = component.transform.parent.GetComponent<UIComponentsBind>();
            if (parentBind == null)
            {
                nodePath.Insert(0,component.gameObject.name + "/");
                GetNodePath(component.transform.parent,nodePath);
                return;
            }
        }

        void WriteLuaInitLine(Element element,StringBuilder luaCode) 
        {
            if (!element.selectedComponent)
                return;

            string filedStr = element.fieldName == string.Empty ? "Undefined" : FirstCharToLower(element.fieldName);
            string componentFullName = Regex.Replace(element.selectedComponent.ToString(), @"(.*\()(.*)(\).*)", "$2");
            
            if (commonCScriptToLuaScript.ContainsKey(componentFullName))
                luaCode.AppendLine(string.Format("self._{0} = self:AddCompont({1},self._{2}Path)", filedStr,commonCScriptToLuaScript[componentFullName],filedStr));

            else if(customCScriptToLuaScript.ContainsKey(componentFullName))
                luaCode.AppendLine(string.Format("self._{0} = {1}", filedStr,customCScriptToLuaScript[componentFullName]));

            else
                luaCode.AppendLine(string.Format("self._{0} = \"未定义该类型的代码的获取方式\"", filedStr));
        }

        void WriteLuaUnInit()
        {
            StringBuilder luaCode = new StringBuilder();

            for (int i = 0; i < UIComponentsBind.elements.Count; i++)
            {
                Element element = UIComponentsBind.elements[i];
                WriteLuaUnInitLine(element, luaCode);
            }

            EditorGUIUtility.systemCopyBuffer = luaCode.ToString();
        }

        void WriteLuaUnInitLine(Element element,StringBuilder luaCode)
        {

        }

        #endregion

        #region 公用

        string FirstCharToLower(string input)
        {
            if (String.IsNullOrEmpty(input))
                return input;

            string str = input.First().ToString().ToLower() + input.Substring(1);
            return str;
        }
        #endregion
    }
}