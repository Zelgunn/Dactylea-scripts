using UnityEngine;
using UnityEditor;
using UnityEditor.AnimatedValues;
using System;
using System.Collections;

[CustomEditor(typeof(XperManager))]
public class XperManagerEditor : Editor
{
    AnimBool m_showActionFields;
    AnimBool m_showRangeFields;

    private void OnEnable()
    {
        m_showActionFields = new AnimBool(true);
        m_showActionFields.valueChanged.AddListener(Repaint);

        m_showRangeFields = new AnimBool(true);
        m_showRangeFields.valueChanged.AddListener(Repaint);
    }

    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        XperManager xperManager = target as XperManager;
        

        XperItem[] items = Enum.GetValues(typeof(XperItem)) as XperItem[];

        m_showActionFields.target = EditorGUILayout.ToggleLeft("Actions", m_showActionFields.target);

        if (EditorGUILayout.BeginFadeGroup(m_showActionFields.faded))
        {
            foreach (XperItem item in items)
            {
                xperManager.SetAttributionsOf(item, (XperAction)EditorGUILayout.EnumMaskField(item.ToString(), xperManager.AttributionsOf(item)));
            }
        }

        EditorGUILayout.EndFadeGroup();
        EditorGUILayout.Separator();

        m_showRangeFields.target = EditorGUILayout.ToggleLeft("Portées", m_showRangeFields.target);

        if (EditorGUILayout.BeginFadeGroup(m_showRangeFields.faded))
        {
            foreach (XperItem item in items)
            {
                xperManager.SetRangeModeOf(item, (XperRange)EditorGUILayout.EnumMaskField(item.ToString(), xperManager.RangeModeOf(item)));
            }

        }

        EditorGUILayout.EndFadeGroup();
    }
}
