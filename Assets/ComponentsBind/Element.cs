using System;
using System.Text.RegularExpressions;
using UnityEngine;

namespace ComponentsBind
{
    
    [Serializable]
    public class Element
    {
        public Element(){}
        public Element(Component comp)
        {
            selectedComponent = comp;
        }

        public string fieldName = string.Empty;
        public Component selectedComponent;
        public int selectedComponentIndex = 0;
        


        public Component[] GetElementAllComponents()
        {
            if (selectedComponent != null) 
                return selectedComponent.GetComponents<Component>();
            else
                return null;
        }

    }
}
