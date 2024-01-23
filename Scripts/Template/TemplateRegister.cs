using System;
using UnityEngine;

namespace TS.TSEffect.Template
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
    public class TemplateRegister : Attribute
    {
        public bool Enable;
        public string Path;
        public Color IconColor;
        public Type TargetType;
        
        public TemplateRegister(bool enable, string path, string color_code, Type target_type)
        {
            Enable = enable;
            if (Enable)
            {
                Path = path;
                Color color;
                ColorUtility.TryParseHtmlString(color_code, out color);
                IconColor = color;
            }
            TargetType = target_type;
        }
    }
}
