using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(RuleSetData)), CanEditMultipleObjects]
public class RuleSetEditor : Editor
{
    public SerializedProperty
        myRule_Prop,
        gravity_Prop,
        punchForce_Prop;
    private void OnEnable()
    {
        myRule_Prop = serializedObject.FindProperty("MyRule");
        gravity_Prop = serializedObject.FindProperty("Gravity");
        punchForce_Prop = serializedObject.FindProperty("PunchForce");
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();
        EditorGUILayout.PropertyField(myRule_Prop);
        RuleType myRule = (RuleType)myRule_Prop.enumValueIndex;

        switch (myRule)
        {
            case RuleType.Gravity:
                EditorGUILayout.PropertyField(gravity_Prop, new GUIContent("Gravity"));
                break;
            case RuleType.Punch:
                EditorGUILayout.PropertyField(punchForce_Prop, new GUIContent("Punch Force"));
                break;
        }
        serializedObject.ApplyModifiedProperties();
    }
}
