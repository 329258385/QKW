using UnityEngine;
using System.Collections;
using UnityEditor;




[CustomEditor(typeof(HexMap))]
public class HexMapEditor : Editor
{
    public override void OnInspectorGUI()
    {
        HexMap myTarget                     = ( HexMap )target;
        int size = 96;
        DrawDefaultInspector();
        EditorGUILayout.LabelField("AREA");
        myTarget.area.tilesInX              = EditorGUILayout.IntField("      Tiles in X: ", myTarget.area.tilesInX);
        myTarget.area.tilesInZ              = EditorGUILayout.IntField("      Tiles in Z: ", myTarget.area.tilesInZ);
        myTarget.area.height                = EditorGUILayout.FloatField("    Height    : ", myTarget.area.height);
        myTarget.area.tileSize              = EditorGUILayout.IntField("      Tiles Size: ", myTarget.area.tileSize);


        EditorGUILayout.LabelField("");
        EditorGUILayout.LabelField("GENERAL OPTIONS");
        myTarget.inclinationMax             = EditorGUILayout.FloatField("      Inclination Max : ", myTarget.inclinationMax);
        EditorGUILayout.LabelField("");
        EditorGUILayout.LabelField("GIZMOS");
        myTarget.showGizmo                  = EditorGUILayout.Toggle("      Enabled : ", myTarget.showGizmo);
        if (myTarget.showGizmo)
        {
            myTarget.colorLinks             = EditorGUILayout.ColorField("         Color Node : ", myTarget.colorLinks);
        }

        if (GUI.changed)
        {
            EditorUtility.SetDirty(myTarget);
        }
    }
}
