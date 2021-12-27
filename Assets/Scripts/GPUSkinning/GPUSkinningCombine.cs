using System;
using System.Collections.Generic;
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;



[ExecuteInEditMode]
public class GPUSkinningCombine : MonoBehaviour
{
    [Header("如果报错，修改贴图的Read/Write enable = true , 贴图的压缩格式先取消")]
    public int TextureAtlasWidth = 1024;
    public int TextureAtlasHeight = 512;

    private void Start()
    {
        
    }

    [ContextMenu("CombineSkinMesh")]
    public void CombineSkinMesh()
    {
        Material material = null;
        int uvCount = 0;
        

        List<CombineInstance>   combineInstances = new List<CombineInstance>();
        List<Transform>         bones       = new List<Transform>();
        List<Texture2D>         textures    = new List<Texture2D>();
        List<Vector2[]>         uvList      = new List<Vector2[]>();

        SkinnedMeshRenderer[] skinMeshs     = GetComponentsInChildren<SkinnedMeshRenderer>();
        foreach( var mesh in skinMeshs )
        {
            if( material == null )
            {
                material = mesh.sharedMaterial;
            }

            // 合并网格
            CombineInstance combine  = new CombineInstance();
            combine.mesh             = mesh.sharedMesh;
            combine.transform        = mesh.transform.localToWorldMatrix;
            combineInstances.Add(combine);

            //取得mesh相对应名称的骨骼列表
            int startBoneIndex      = bones.Count;
            foreach( var bone in mesh.bones )
            {
                bones.Add(bone);
            }

            // 储存网格纹理坐标
            uvList.Add(mesh.sharedMesh.uv);
            uvCount += mesh.sharedMesh.uv.Length;

            // 暂时不考虑submesh了，现在的制作方式应该不会有
            // 合并贴图
            if( mesh.sharedMaterial.mainTexture != null )
            {
                textures.Add(mesh.GetComponent<Renderer>().sharedMaterial.mainTexture as Texture2D);
            }
            mesh.gameObject.SetActive(false);
        }

        // 贴图合并
        Texture2D skinnedMeshAtlas  = new Texture2D(TextureAtlasWidth, TextureAtlasHeight, TextureFormat.RGB24, false );
        Rect[] packingResult        = skinnedMeshAtlas.PackTextures(textures.ToArray(), 0);

        //网格纹理坐标合并
        Vector2[] atlasUVs = new Vector2[uvCount];
        int j = 0;
        for (int i = 0; i < uvList.Count; i++)
        {
            foreach (Vector2 uv in uvList[i])
            {
                atlasUVs[j].x = Mathf.Lerp(packingResult[i].xMin, packingResult[i].xMax, uv.x);
                atlasUVs[j].y = Mathf.Lerp(packingResult[i].yMin, packingResult[i].yMax, uv.y);
                j++;
            }
        }

        Texture newTex = SaveCombineTexture(skinnedMeshAtlas, material, gameObject.name );

        //蒙皮网格渲染器
        GameObject go = new GameObject();
        go.name = "skinmesh";
        go.transform.parent = transform;
        SkinnedMeshRenderer r = go.AddComponent<SkinnedMeshRenderer>();
        r.sharedMesh = new Mesh();
        r.sharedMesh.CombineMeshes(combineInstances.ToArray(), true, false);
        r.bones = bones.ToArray();
        r.sharedMaterial = material;
        r.sharedMaterial.mainTexture = newTex;
        r.sharedMesh.uv = atlasUVs; 
    }

    private Texture SaveCombineTexture( Texture2D combine, Material material, string fileName )
    {
        #if UNITY_EDITOR
       
        string filePath     = Application.dataPath + "/GPUSkin/" + fileName + "_combine.PNG";
        string assetPath    = "Assets/GPUSkin/" + fileName + "_combine.PNG";
        if (System.IO.File.Exists(filePath))
        {
            System.IO.FileAttributes newPathAttrs = System.IO.File.GetAttributes(filePath);
            newPathAttrs &= ~System.IO.FileAttributes.ReadOnly;
            System.IO.File.SetAttributes(filePath, newPathAttrs);
        }

        byte[] bytes = combine.EncodeToPNG();
        System.IO.File.WriteAllBytes(filePath, bytes);
        AssetDatabase.ImportAsset(filePath, ImportAssetOptions.ForceUpdate);

        //Modification de la Texture 
        TextureImporter TextureI = AssetImporter.GetAtPath(assetPath) as TextureImporter;
        TextureI.isReadable = true;
        TextureI.anisoLevel = 9;
        TextureI.mipmapEnabled = false;
        TextureI.wrapMode = TextureWrapMode.Clamp;
        AssetDatabase.Refresh();
        AssetDatabase.ImportAsset(assetPath, ImportAssetOptions.ForceUpdate);

        Texture test = (Texture)AssetDatabase.LoadAssetAtPath(assetPath, typeof(Texture));
        AssetDatabase.Refresh();
        return test;
#endif

        return null;
    }
}

