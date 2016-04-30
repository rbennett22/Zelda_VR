using UnityEngine;
using UnityEngine.UI;
using System;
using System.Reflection;
using System.Text;

namespace Eyefluence.Utility
{
    public class DebugOptionsUI : MonoBehaviour
    {
        const string DefaultTextColor = "white";
        const string BoolFalseTextColor = "#ff5050ff";
        const string BoolTrueTextColor = "lime";
        const string EnumTextColor = "cyan";


        public Text keyBindingsText, optionsText, valuesText;


        void Update()
        {
            StringBuilder keysSB = new StringBuilder("Key\n", 50);
            StringBuilder optionsSB = new StringBuilder("Option\n", 200);
            StringBuilder valuesSB = new StringBuilder("Value\n", 50);

            foreach (DebugOption option in DebugOptions.Instance.Options)
            {
                KeyCode keyCode = option.Key;
                string keyStr = (keyCode == DebugOption.NullKeyCode) ? string.Empty : keyCode.ToString();

                keysSB.Append("\n" + keyStr);
                optionsSB.Append("\n" + option.Name);
                valuesSB.Append("\n");
                if (!option.IsTrigger)
                {
                    valuesSB.Append(GetRichTextForValue(option.IsActivated));
                }
            }

            keyBindingsText.text = keysSB.ToString();
            optionsText.text = optionsSB.ToString();
            valuesText.text = valuesSB.ToString();
        }


        static string GetRichTextForValue(object obj, string format = null)
        {
            string colorStr, objStr;

            // Determine text color
            if (obj is bool)
            {
                colorStr = (bool)obj ? BoolTrueTextColor : BoolFalseTextColor;
            }
            else if (obj is Enum)
            {
                colorStr = EnumTextColor;
            }
            else
            {
                colorStr = DefaultTextColor;
            }

            // Determine object string value
            if (obj is IFormattable)
            {
                objStr = ((IFormattable)obj).ToString(format, null);
            }
            else
            {
                Type[] types = { typeof(string) };
                if (HasMethod(obj, "ToString", types))
                {
                    objStr = (string)CallMethod(obj, "ToString", types, format);
                }
                else
                {
                    objStr = obj.ToString();
                }
            }

            if (obj is bool)
            {
                objStr = (bool)obj ? "On" : "Off";
            }

            return "<color=" + colorStr + ">" + objStr + "</color>";
        }

        static bool HasMethod(object objectToCheck, string methodName, Type[] types)
        {
            Type type = objectToCheck.GetType();
            return type.GetMethod(methodName, types) != null;
        }

        static object CallMethod(object target, string methodName, Type[] types, params object[] args)
        {
            Type t = target.GetType();
            MethodInfo m = t.GetMethod(methodName, types);
            return m.Invoke(target, args);
        }
    }
}