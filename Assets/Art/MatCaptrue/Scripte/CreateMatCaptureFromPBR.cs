using UnityEngine;
using System.Collections;
using System;
using System.IO;
#if UNITY_EDITOR
using UnityEditor;
#endif





public class CreateMatCaptureFromPBR : MonoBehaviour
{
    public Camera       screenshotCamera;
    public Material     preview;

    [Tooltip("View the saved PNG in the file browser on save")]
    public bool revealOnSave = true;
}


#if UNITY_EDITOR
[CustomEditor(typeof(CreateMatCaptureFromPBR))]
public class CreateMatCaptureFromEditor : Editor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        GUILayout.Space(10);
        if (GUILayout.Button("Save MatCap", GUILayout.Height(40)))
        {
            bool reveal = ((CreateMatCaptureFromPBR)target).revealOnSave;
            CaptureScreen("MatCap", reveal);
        }
    }

    void CaptureScreen(string filename, bool reveal)
    {
        var screenshot = ((CreateMatCaptureFromPBR)target).screenshotCamera;
            
        Graphics.SetRenderTarget(screenshot.targetTexture);
        Texture2D tex = new Texture2D(screenshot.targetTexture.width, screenshot.targetTexture.height, TextureFormat.ARGB32, false);
        tex.ReadPixels(new Rect(0, 0, screenshot.targetTexture.width, screenshot.targetTexture.height), 0, 0, false);
        tex.Apply();
        
        string path = EditorUtility.SaveFilePanelInProject("保存材质捕捉纹理", "MatCap", "png", "");
        if (!string.IsNullOrEmpty(path))
        {
            // Save the PNG file
            File.WriteAllBytes(path, tex.EncodeToPNG());
            AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceSynchronousImport);
            if (reveal)
            {
                EditorUtility.RevealInFinder(path);
            }

            var textureImporter                 = (TextureImporter)AssetImporter.GetAtPath(path);
            textureImporter.textureCompression  = TextureImporterCompression.Uncompressed;
            textureImporter.wrapMode            = TextureWrapMode.Clamp;

            var previewMaterial                 = ((CreateMatCaptureFromPBR)target).preview;
            if (previewMaterial != null)
            {
                var generatedTexture = AssetDatabase.LoadAssetAtPath<Texture2D>(path);
                if (generatedTexture != null)
                {
                    previewMaterial.SetTexture("_MatCap", generatedTexture);
                }
            }
        }
        DestroyImmediate(tex);
    }
}
#endif

