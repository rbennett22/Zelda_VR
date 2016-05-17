using System;
using System.Reflection;
using UnityEditor;

public class ConsoleClear
{
    [MenuItem("Custom/ConsoleClear &%c")]
    public static void ClearConsole()
    {
        Assembly assembly = Assembly.GetAssembly(typeof(SceneView));
        Type type = assembly.GetType("UnityEditorInternal.LogEntries");
        MethodInfo method_info = type.GetMethod("Clear");
        method_info.Invoke(new object(), null);
    }
}