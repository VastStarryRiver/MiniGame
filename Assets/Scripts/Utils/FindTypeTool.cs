using System;
using System.Collections.Generic;
using UnityEngine.UI;



public static class FindTypeTool
{
    private static Dictionary<string, string> m_nameDictionary = null;



    public static Type GetComponentType(string componentName)
    {
        if (m_nameDictionary == null)
        {
            InitNameDictionary();
        }

        if (m_nameDictionary.ContainsKey(componentName))
        {
            string className = m_nameDictionary[componentName];
            return Type.GetType(className);
        }

        return null;
    }

    private static void InitNameDictionary()
    {
        m_nameDictionary = new Dictionary<string, string>
        {
            { "RawImage", typeof(RawImage).AssemblyQualifiedName },
            { "Image", typeof(Image).AssemblyQualifiedName },
            { "Text", typeof(Text).AssemblyQualifiedName },
            { "Button", typeof(Button).AssemblyQualifiedName },
            { "ScrollRect", typeof(ScrollRect).AssemblyQualifiedName },
            { "Dropdown", typeof(Dropdown).AssemblyQualifiedName },
            { "Toggle", typeof(Toggle).AssemblyQualifiedName },
            { "Slider", typeof(Slider).AssemblyQualifiedName },
            { "Scrollbar", typeof(Scrollbar).AssemblyQualifiedName },
            { "InputField", typeof(InputField).AssemblyQualifiedName },
            { "GridLayoutGroup", typeof(GridLayoutGroup).AssemblyQualifiedName },
            { "HorizontalLayoutGroup", typeof(HorizontalLayoutGroup).AssemblyQualifiedName },
            { "VerticalLayoutGroup", typeof(VerticalLayoutGroup).AssemblyQualifiedName },
            { "Mask", typeof(Mask).AssemblyQualifiedName },
            { "RectMask2D", typeof(RectMask2D).AssemblyQualifiedName }
        };
    }
}