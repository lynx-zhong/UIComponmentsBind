using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditorInternal;
using System;
using System.Text;
using System.Text.RegularExpressions;
using System.Linq;
using NUnit;

namespace ComponentsBind
{
    public class LuaCodeExport : CodeExportBase
    {
        readonly static int codeDefultLen = 115;

        private static Dictionary<string, string> commonCScriptToLuaScript = new Dictionary<string, string>()
        {
            {"UnityEngine.UI.Text","UIText"},
            {"UnityEngine.UI.Image","UIImage"},
            {"UnityEngine.UI.Button","UIButton"},
            {"UnityEngine.UI.Slider","UISlider"},
            {"TMPro.TextMeshProUGUI","UITextMeshProUGUI"},
            {"Extends.UI.UIAnimation","UIAnimation"},
            {"Extends.UI.ExToggle","UIExToggle"},
            {"UnityEngine.UI.Dropdown","UIDragdown"},
        };

        private static List<string> commonDestoryCScriptToLuaScript = new List<string>()
        {
            {"Extends.UI.LoopHorizontalScrollRect"},
            {"Extends.UI.LoopVerticalScrollRect"},
        };

        public static StringBuilder WriteLuaInit(UIComponentsBind componentsBind,bool isCopyBuffer = true)
        {
            StringBuilder luaCode = new StringBuilder();

            for (int i = 0; i < componentsBind.elements.Count; i++)
            {
                Element element = componentsBind.elements[i];
                WriteLuaInitPathLine(componentsBind,element, luaCode);
            }

            luaCode.AppendLine();

            for (int i = 0; i < componentsBind.elements.Count; i++)
            {
                Element element = componentsBind.elements[i];
                WriteLuaInitLine(element, luaCode);
            }

            luaCode.AppendLine();

            for (int i = 0; i < componentsBind.elements.Count; i++)
            {
                Element element = componentsBind.elements[i];
                WriteLuaButtonAction(element, luaCode);
            }

            if (isCopyBuffer)
            {
                EditorGUIUtility.systemCopyBuffer = luaCode.ToString();
                EditorUtility.DisplayDialog("", "已将代码复制到剪切板", "确定");
            }

            return luaCode;
        }

        public static void WriteLuaInitPathLine(UIComponentsBind componentsBind,Element element,StringBuilder luaCode) 
        {
            if (!element.selectedComponent)
                return;

            string filedStr = element.fieldName == string.Empty ? "Undefined" : FirstCharToLower(element.fieldName);
            StringBuilder nodePath = new StringBuilder();
            nodePath.Append(element.selectedComponent.gameObject.name);
            GetNodePath(element.selectedComponent,nodePath);

            string result = nodePath.ToString();
            int strIndex1 = result.IndexOf(componentsBind.name + "/");
            int strIndex2 = componentsBind.name.Length + 1;

            if (strIndex1 == -1)
                result = string.Empty;
            else
                result = result.Substring(strIndex1 + strIndex2);

            luaCode.AppendLine(string.Format("\tself._{0}Path = \"{1}\"", filedStr, result));
        }

        public static void GetNodePath(Component component,StringBuilder nodePath)
        {
            Transform parentBind = component.transform.parent;
            if (parentBind != null)
            {
                nodePath.Insert(0,component.transform.parent.gameObject.name + "/");
                GetNodePath(component.transform.parent,nodePath);
                return;
            }
        }

        // 在这里定义某个组件的获取方式
        public static void WriteLuaInitLine(Element element,StringBuilder luaCode) 
        {
            if (!element.selectedComponent)
                return;

            string filedStr = element.fieldName == string.Empty ? "Undefined" : FirstCharToLower(element.fieldName);
            string componentFullName = Regex.Replace(element.selectedComponent.ToString(), @"(.*\()(.*)(\).*)", "$2");
            if (componentFullName == null)
                return;

            if (commonCScriptToLuaScript.ContainsKey(componentFullName))
            {
                string info = string.Format("\tself._{0} = self:AddComponent({1},self._{2}Path)", filedStr,commonCScriptToLuaScript[componentFullName],filedStr);
                luaCode.AppendLine(string.Format("{0}        ---@type {1}",info.PadRight(codeDefultLen),commonCScriptToLuaScript[componentFullName]));
            }
            else if(componentFullName == "UnityEngine.RectTransform" )
            {
                luaCode.AppendLine(string.Format("\tself._{0} = self.transform:Find(self._{1}Path).gameObject", filedStr,filedStr));
            }
            else if (componentFullName == "Extends.UI.LoopHorizontalScrollRect" || componentFullName == "Extends.UI.LoopVerticalScrollRect")
            {
                string info = string.Format("\tself._{0} = self:AddComponent(UILoopList,self._{1}Path,Bind(self,self.OnCellValid))", filedStr,filedStr);
                luaCode.AppendLine(string.Format("{0}        ---@type UILoopList",info.PadRight(codeDefultLen)));
            }
            else
            {
                luaCode.AppendLine(string.Format("\tself._{0} = self.transform:Find(self._{1}Path):GetComponent(typeof(CS.{2}))", filedStr,filedStr,componentFullName));
                Debug.LogError("未定义的类型，请检测是否采用此Find方法：  " + componentFullName);
            }
        }

        public static void WriteLuaButtonAction(Element element, StringBuilder luaCode)
        {
            if (!element.selectedComponent)
                return;

            string filedStr = element.fieldName == string.Empty ? "Undefined" : FirstCharToLower(element.fieldName);
            string componentFullName = Regex.Replace(element.selectedComponent.ToString(), @"(.*\()(.*)(\).*)", "$2");
            if (componentFullName == "UnityEngine.UI.Button")
            {
                luaCode.AppendLine(String.Format("\tself._{0}:SetOnClick(self,self.On{1}Click)", filedStr, FirstCharToUpper(filedStr)));
            }
        }

        public static StringBuilder WriteLuaUnInit(UIComponentsBind componentsBind,bool isCopyBuffer = true)
        {
            StringBuilder luaCode = new StringBuilder();

            for (int i = 0; i < componentsBind.elements.Count; i++)
            {
                Element element = componentsBind.elements[i];
                if (element.selectedComponent != null)
                {
                    string componentFullName = Regex.Replace(element.selectedComponent.ToString(), @"(.*\()(.*)(\).*)", "$2");
                    string filedStr = element.fieldName == string.Empty ? "Undefined" : FirstCharToLower(element.fieldName);
                    if (commonCScriptToLuaScript.ContainsKey(componentFullName) || commonDestoryCScriptToLuaScript.Contains(componentFullName))
                    {
                        luaCode.AppendLine(String.Format("\tself._{0}:OnDestroy()", filedStr));
                    }
                    else
                    {
                        luaCode.AppendLine(String.Format("\tself._{0} = nil", filedStr));
                    }
                }
            }

            if (isCopyBuffer)
            {
                EditorGUIUtility.systemCopyBuffer = luaCode.ToString();
                EditorUtility.DisplayDialog("", "已将代码复制到剪切板", "确定");
            }

            return luaCode;
        }


        public static StringBuilder WriteLuaButtonFunction(UIComponentsBind componentsBind,bool isCopyBuffer = true) 
        {
            StringBuilder luaCode = new StringBuilder();

            for (int i = 0; i < componentsBind.elements.Count; i++)
            {
                Element element = componentsBind.elements[i];

                if (!element.selectedComponent)
                    continue;

                string filedStr = element.fieldName == string.Empty ? "Undefined" : FirstCharToLower(element.fieldName);
                string componentFullName = Regex.Replace(element.selectedComponent.ToString(), @"(.*\()(.*)(\).*)", "$2");
                if (componentFullName != "UnityEngine.UI.Button")
                    continue;

                luaCode.AppendLine();
                luaCode.AppendLine(String.Format("function {0}View:On{1}Click()", componentsBind.gameObject.name, FirstCharToUpper(filedStr)));
                luaCode.AppendLine();
                luaCode.AppendLine("end");
            }

            if (isCopyBuffer)
            {
                EditorGUIUtility.systemCopyBuffer = luaCode.ToString();
                EditorUtility.DisplayDialog("", "已将代码复制到剪切板", "确定");
            }

            return luaCode;
        }
    }
}