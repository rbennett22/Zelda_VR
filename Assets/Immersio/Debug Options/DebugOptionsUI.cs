using System;
using System.Reflection;
using System.Text;
using UnityEngine;
using UnityEngine.UI;

namespace Eyefluence.Utility
{
    public class DebugOptionsUI : MonoBehaviour
    {
        const string TEXT_COLOR_DEFAULT = "white";
        const string TEXT_COLOR_BOOL_FALSE = "#ff5050ff";
        const string TEXT_COLOR_BOOL_TRUE = "lime";
        const string TEXT_COLOR_ENUM = "cyan";


        [SerializeField]
        Text _keyBindingsText, _optionsText, _valuesText;


        StringBuilder _keysSB, _optionsSB, _valuesSB;


        void Awake()
        {
            _keysSB = new StringBuilder("Key\n", 50);
            _optionsSB = new StringBuilder("Option\n", 200);
            _valuesSB = new StringBuilder("Value\n", 50);
        }


        void Update()
        {
            // TODO: Only Update UI when values change (Observer)

            _keysSB.Remove(0, _keysSB.Length);
            _optionsSB.Remove(0, _optionsSB.Length);
            _valuesSB.Remove(0, _valuesSB.Length);

            foreach (DebugOption option in DebugOptions.Instance.Options)
            {
                KeyCode keyCode = option.Key;
                string keyStr = (keyCode == DebugOption.NullKeyCode) ? string.Empty : keyCode.ToString();

                _keysSB.Append("\n" + keyStr);
                _optionsSB.Append("\n" + option.Name);
                _valuesSB.Append("\n");
                if (!option.IsTrigger)
                {
                    _valuesSB.Append(GetRichTextForValue(option.IsActivated));
                }
            }

            _keyBindingsText.text = _keysSB.ToString();
            _optionsText.text = _optionsSB.ToString();
            _valuesText.text = _valuesSB.ToString();
        }


        static string GetRichTextForValue(object obj, string format = null)
        {
            string objStr = GetValueStringForObject(obj, format);
            string colorStr = GetColorStringForObject(obj);

            return "<color=" + colorStr + ">" + objStr + "</color>";
        }
        static string GetValueStringForObject(object obj, string format = null)
        {
            string s;

            if (obj is bool)
            {
                s = (bool)obj ? "On" : "Off";
            }
            else if (obj is IFormattable)
            {
                s = ((IFormattable)obj).ToString(format, null);
            }
            else
            {
                Type[] types = { typeof(string) };
                if (HasMethod(obj, "ToString", types))
                {
                    s = (string)CallMethod(obj, "ToString", types, format);
                }
                else
                {
                    s = obj.ToString();
                }
            }
            
            return s;
        }
        static string GetColorStringForObject(object obj)
        {
            string s = null;

            if (obj is bool)
            {
                s = (bool)obj ? TEXT_COLOR_BOOL_TRUE : TEXT_COLOR_BOOL_FALSE;
            }
            else if (obj is Enum)
            {
                s = TEXT_COLOR_ENUM;
            }
            else
            {
                s = TEXT_COLOR_DEFAULT;
            }

            return s;
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