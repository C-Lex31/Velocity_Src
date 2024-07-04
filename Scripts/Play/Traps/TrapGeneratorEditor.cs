using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(TrapGenerator))]
public class TrapGeneratorEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        TrapGenerator trapGenerator = (TrapGenerator)target;

        if (trapGenerator.traps.Count > 0)
        {
            trapGenerator.selectedTrapIndex = EditorGUILayout.IntSlider("Selected Trap Index", trapGenerator.selectedTrapIndex, 0, trapGenerator.traps.Count - 1);

            TrapInfo selectedTrap = trapGenerator.traps[trapGenerator.selectedTrapIndex];
            selectedTrap.positionOffset = EditorGUILayout.Vector3Field("Position Offset", selectedTrap.positionOffset);
            selectedTrap.rotationOffset = EditorGUILayout.Vector3Field("Rotation Offset", selectedTrap.rotationOffset);
            selectedTrap.scaleOffset = EditorGUILayout.Vector3Field("Scale Offset", selectedTrap.scaleOffset);
        }
    }
}
