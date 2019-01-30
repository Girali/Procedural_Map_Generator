using UnityEngine;
using System.Collections.Generic;
using UnityEditor;

[CustomEditor(typeof (GenerateTestMap))]
public class MapGeneratorEditor : Editor {

    public override void OnInspectorGUI()
    {
        GenerateTestMap mapGen = (GenerateTestMap)target;

        if (DrawDefaultInspector())
        {
            if (mapGen.autoUpdate)
            {
                mapGen.GenerateTest();
            }
        }

        if (GUILayout.Button("Generate"))
        {
            mapGen.GenerateTest();
        }

        if (GUILayout.Button("Load"))
        {
            mapGen.LoadOctave();
        }
        
        if (GUILayout.Button("Save"))
        {
            mapGen.SaveGenerated();
        }
    }

}