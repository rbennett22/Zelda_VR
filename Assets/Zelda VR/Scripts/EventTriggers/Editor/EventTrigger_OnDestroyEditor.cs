using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(EventTrigger_OnDestroy), false)]

public class EventTrigger_OnDestroyEditor : Editor
{
    const string DELEGATES_PROPERTY_NAME = "_delegates";
    const string EVENT_NAME = "OnDestroy";


    SerializedProperty m_DelegatesProperty;

    GUIContent m_EventIDName;


    protected virtual void OnEnable()
    {
        m_DelegatesProperty = serializedObject.FindProperty(DELEGATES_PROPERTY_NAME);
        m_EventIDName = new GUIContent(EVENT_NAME);
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        m_DelegatesProperty.arraySize = 1;

        for (int i = 0; i < m_DelegatesProperty.arraySize; ++i)
        {
            SerializedProperty delegateProperty = m_DelegatesProperty.GetArrayElementAtIndex(i);

            EditorGUILayout.PropertyField(delegateProperty, m_EventIDName);
        }

        serializedObject.ApplyModifiedProperties();
    }
}