using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;







public class BattleFogManager : MonoBehaviour
{
    public float            screenwidth  = 512;
    public float            screenheight = 512;

    public Material         fogMaterial = null;

    private Camera          myCamera;
    private BattleFogMask   Fogmask;
    private bool            bStartRenderFog = false;

    public Camera           camera
    {
        get
        {
            if (myCamera == null)
                myCamera = GetComponent<Camera>();
            return myCamera;
        }
    }

    private Transform       cameraTransform;
    public Transform camerTransform
    {
        get
        {
            if (cameraTransform == null)
                cameraTransform = camera.transform;
            return cameraTransform;
        }
    }


    private void OnEnable()
    {
        //bStartRenderFog = true;
        camera.depthTextureMode |= DepthTextureMode.Depth;
    }

    private void OnDisable()
    {
        bStartRenderFog = false;
    }

    //[ImageEffectOpaque]
    //private void OnRenderImage(RenderTexture source, RenderTexture destination)
    //{
    //    if (!bStartRenderFog) return;
    //    if (fogMaterial != null )
    //    {
    //        Matrix4x4 frustumCorners = Matrix4x4.identity;
    //        float fov       = camera.fieldOfView;
    //        float near      = camera.nearClipPlane;
    //        float far       = camera.farClipPlane;
    //        float aspect    = camera.aspect;

    //        float halfHeight = near * Mathf.Tan(fov * 0.5f * Mathf.Deg2Rad);
    //        Vector3 toRight  = camerTransform.right * halfHeight * aspect;
    //        Vector3 toTop    = camerTransform.up * halfHeight;
                
    //        Vector3 topLeft  = camerTransform.forward * near - toRight + toTop;
    //        float   scale    = topLeft.magnitude * far / near;
    //        topLeft.Normalize();
    //        topLeft         *= scale;

    //        Vector3 topRight = camerTransform.forward * near + toRight + toTop;
    //        topRight.Normalize();
    //        topRight        *= scale;

    //        Vector3 botLeft  = camerTransform.forward * near - toRight - toTop;
    //        botLeft.Normalize();
    //        botLeft         *= scale;

    //        Vector3 botRight = camerTransform.forward * near + toRight - toTop;
    //        botRight.Normalize();
    //        botRight         *= scale;

    //        frustumCorners.SetRow(0, topLeft);
    //        frustumCorners.SetRow(1, topRight);
    //        frustumCorners.SetRow(2, botRight);
    //        frustumCorners.SetRow(3, botLeft);

    //        fogMaterial.SetFloat("_worldWidth", screenwidth);
    //        fogMaterial.SetFloat("_worldHeight", screenheight);
    //        if( Fogmask != null )
    //        {
    //            fogMaterial.SetTexture("_EdgeTex", Fogmask.MaskTex);
    //        }
    //        CustomGraphicsBlit(source, destination, fogMaterial, 0);
    //    }
    //}

    void CustomGraphicsBlit(RenderTexture source, RenderTexture dest, Material fxMaterial, int passNr)
    {
        RenderTexture.active = dest;

        fxMaterial.SetTexture("_MainTex", source);

        GL.PushMatrix();
        GL.LoadOrtho();

        fxMaterial.SetPass(passNr);

        GL.Begin(GL.QUADS);

        GL.MultiTexCoord2(0, 0.0f, 0.0f);
        GL.Vertex3(0.0f, 0.0f, 3.0f); // BL

        GL.MultiTexCoord2(0, 1.0f, 0.0f);
        GL.Vertex3(1.0f, 0.0f, 2.0f); // BR

        GL.MultiTexCoord2(0, 1.0f, 1.0f);
        GL.Vertex3(1.0f, 1.0f, 1.0f); // TR

        GL.MultiTexCoord2(0, 0.0f, 1.0f);
        GL.Vertex3(0.0f, 1.0f, 0.0f); // TL

        GL.End();
        GL.PopMatrix();
    }

    void CheckSupport( bool needDepth )
    {
        if( !SystemInfo.supportsImageEffects )
        {
            NotSupported();
            return;
        }

        if( needDepth && !SystemInfo.SupportsRenderTextureFormat(RenderTextureFormat.Depth))
        {
            NotSupported();
            return;
        }

        if (!SystemInfo.supports3DTextures)
        {
            NotSupported();
            return;
        }
    }

    void NotSupported()
    {
        enabled = false;
        return;
    }
}
