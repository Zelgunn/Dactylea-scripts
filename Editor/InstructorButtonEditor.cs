using UnityEngine;
using UnityEditor;
using System.Collections;

[CustomEditor(typeof(InstructorButton))]
public class InstructorButtonEditor : Editor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        InstructorButton instructorButton = target as InstructorButton;

        if(instructorButton.type == InstructorButton.InstructorButtonType.Loop)
        {
            instructorButton.onTexture = EditorGUILayout.ObjectField("Loop On (stop icon)", instructorButton.onTexture, typeof(Texture), false) as Texture;
        }

        if (instructorButton.type == InstructorButton.InstructorButtonType.PlayPause)
        {
            instructorButton.onTexture = EditorGUILayout.ObjectField("Playing (stop icon)", instructorButton.onTexture, typeof(Texture), false) as Texture;
        }
    }
}
