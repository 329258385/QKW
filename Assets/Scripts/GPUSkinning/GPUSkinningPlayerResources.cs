using System.Collections;
using System.Collections.Generic;
using UnityEngine;




public class GPUSkinningPlayerResources
{
    public GPUSkinningAnimation         anim = null;
    public Mesh                         mesh = null;
    public Texture2D                    texture = null;
    private CullingGroup                cullingGroup = null;
    private GPUSkinningMaterial         mtrl = null;
    private GPUSkinningExecutePerFrame  executeOncePerFrame = new GPUSkinningExecutePerFrame();


    public List<GPUSkinningPlayerMono>              players = new List<GPUSkinningPlayerMono>();
    private GPUSkinningBetterList<BoundingSphere>   cullingBounds = new GPUSkinningBetterList<BoundingSphere>(100);
    
    private float time = 0;
    
    public float Time
    {
        get
        {
            return time;
        }
        set
        {
            time = value;
        }
    }

    /// <summary>
    /// 与shader 通信
    /// </summary>
    private static int shaderPropID_GPUSkinning_TextureMatrix = -1;
    private static int shaderPropID_GPUSkinning_TextureSize_NumPixelsPerFrame = 0;
    private static int shaderPorpID_GPUSkinning_FrameIndex_PixelSegmentation = 0;
    public  static int shaderPorpID_GPUSkinning_ExposureAndGray = 0;


    public GPUSkinningPlayerResources()
    {
        if (shaderPropID_GPUSkinning_TextureMatrix == -1)
        {
            shaderPropID_GPUSkinning_TextureMatrix                  = Shader.PropertyToID("_GPUSkinning_TextureMatrix");
            shaderPropID_GPUSkinning_TextureSize_NumPixelsPerFrame  = Shader.PropertyToID("_GPUSkinning_TextureSize_NumPixelsPerFrame");
            shaderPorpID_GPUSkinning_FrameIndex_PixelSegmentation   = Shader.PropertyToID("_GPUSkinning_FrameIndex_PixelSegmentation");
            shaderPorpID_GPUSkinning_ExposureAndGray                = Shader.PropertyToID("_GPUSkinning_ExposureAndGray");
        }
    }

    public void Destroy()
    {
        anim = null;
        mesh = null;
        if (cullingBounds != null)
        {
            cullingBounds.Release();
            cullingBounds = null;
        }

        DestroyCullingGroup();
        if (mtrl != null)
        {
            mtrl.Destroy();
            mtrl = null;
        }

        if (texture != null)
        {
            Object.DestroyImmediate(texture);
            texture = null;
        }

        if (players != null)
        {
            players.Clear();
            players = null;
        }
    }

    public void AddCullingBounds()
    {
        if (cullingGroup == null)
        {
            cullingGroup = new CullingGroup();
            cullingGroup.targetCamera = Camera.main;
            //cullingGroup.SetBoundingDistances(anim.lodDistances);
            cullingGroup.SetDistanceReferencePoint(Camera.main.transform);
            cullingGroup.onStateChanged = OnLodCullingGroupOnStateChangedHandler;
        }

        cullingBounds.Add(new BoundingSphere());
        cullingGroup.SetBoundingSpheres(cullingBounds.buffer);
        cullingGroup.SetBoundingSphereCount(players.Count);
    }

    public void RemoveCullingBounds(int index)
    {
        cullingBounds.RemoveAt(index);
        cullingGroup.SetBoundingSpheres(cullingBounds.buffer);
        cullingGroup.SetBoundingSphereCount(players.Count);
    }

    
    private void OnLodCullingGroupOnStateChangedHandler(CullingGroupEvent evt)
    {
        GPUSkinningPlayerMono player = players[evt.index];
        if (evt.isVisible)
        {
            player.Player.Visible = true;
        }
        else
        {
            player.Player.Visible = false;
        }
    }

    private void DestroyCullingGroup()
    {
        if (cullingGroup != null)
        {
            cullingGroup.Dispose();
            cullingGroup = null;
        }
    }

    public void Update(float deltaTime, GPUSkinningMaterial mtrl, float speed = 1.0f )
    {
        if (executeOncePerFrame.CanBeExecute())
        {
            executeOncePerFrame.MarkAsExecuted();
            time += ( deltaTime * speed );
        }

        if (mtrl.executeOncePerFrame.CanBeExecute())
        {
            mtrl.executeOncePerFrame.MarkAsExecuted();
            mtrl.material.SetTexture(shaderPropID_GPUSkinning_TextureMatrix, texture);
            mtrl.material.SetVector(shaderPropID_GPUSkinning_TextureSize_NumPixelsPerFrame,new Vector4(anim.textureWidth, anim.textureHeight, anim.bones.Length * 3, 0));
        }
    }

    public void UpdatePlayingData(MaterialPropertyBlock mpb, GPUSkinningAnimationClip playingClip, int frameIndex)
    {
        mpb.SetVector(shaderPorpID_GPUSkinning_FrameIndex_PixelSegmentation, new Vector4(frameIndex, playingClip.pixelSegmentation, 0, 0));
    }


    public void UpdateEffect(MaterialPropertyBlock mpb, Vector4 colorVal)
    {
        mpb.SetVector(shaderPorpID_GPUSkinning_ExposureAndGray, colorVal);
    }

    public GPUSkinningMaterial GetMaterial()
    {
        return mtrl;
    }

    public void InitMaterial(Material originalMaterial, HideFlags hideFlags)
    {
        if (mtrl != null)
        {
            return;
        }

        mtrl = new GPUSkinningMaterial() { material = new Material(originalMaterial) };
        mtrl.HashName           = originalMaterial.name.GetHashCode();
        mtrl.material.name      = "ROOTOFF_BLENDOFF";
        mtrl.material.hideFlags = hideFlags;
        mtrl.material.enableInstancing = true;
        mtrl.material.EnableKeyword("ROOTOFF_BLENDOFF");
    }
}
