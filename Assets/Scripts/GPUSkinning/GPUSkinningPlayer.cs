using UnityEngine;
using System.Collections;
using System.Collections.Generic;





public class GPUSkinningPlayer
{
    private GameObject                  go = null;
    private Transform                   transform = null;
    private MeshRenderer                mr = null;
    private MeshFilter                  mf = null;

    private Material                    normaterial = null;
    private Material                    hihgtLightmaterial = null;

  
    private float                       time = 0;
    private float                       timeDiff = 0;

    private int                         lastPlayingFrameIndex = -1;
    private GPUSkinningAnimationClip    lastPlayedClip = null;
    private GPUSkinningAnimationClip    lastPlayingClip = null;
    private GPUSkinningAnimationClip    playingClip = null;
    private GPUSkinningPlayerResources  res = null;

    private MaterialPropertyBlock       mpb = null;
    private bool                        rootMotionEnabled = false;
    public bool                         animationOffsetenable = false;
    private Vector4                     colorsetter = new Vector4(1, 0, 1, 0);


    public MeshRenderer Render
    {
        get
        {
            return mr;
        }
    }

    public bool RootMotionEnabled
    {
        get
        {
            return rootMotionEnabled;
        }
        set
        {
            rootMotionEnabled = value;
        }
    }

    private GPUSKinningCullingMode cullingMode = GPUSKinningCullingMode.CullUpdateTransforms;
    public GPUSKinningCullingMode CullingMode
    {
        get
        {
            return Application.isPlaying ? cullingMode : GPUSKinningCullingMode.AlwaysAnimate;
        }
        set
        {
            cullingMode = value;
        }
    }

    private bool visible = false;
    public bool Visible
    {
        get
        {
            return Application.isPlaying ? visible : true;
        }
        set
        {
            visible = value;
        }
    }


    private bool isPlaying = false;
    public bool IsPlaying
    {
        get
        {
            return isPlaying;
        }
    }

    public Vector3 Position
    {
        get
        {
            return transform == null ? Vector3.zero : transform.position;
        }
    }

    private float _playspeed = 1.0f;
    public float speed
    {
        get
        {
            return _playspeed;
        }
        set
        {
            _playspeed = value;
        }
    }

    public GPUSkinningWrapMode WrapMode
    {
        get
        {
            return playingClip == null ? GPUSkinningWrapMode.Once : playingClip.wrapMode;
        }
    }

    public bool IsTimeAtTheEndOfLoop
    {
        get
        {
            if (playingClip == null)
            {
                return false;
            }
            else
            {
                return GetFrameIndex() == ((int)(playingClip.length * playingClip.fps) - 1);
            }
        }
    }

    public float NormalizedTime
    {
        get
        {
            if (playingClip == null)
            {
                return 0;
            }
            else
            {
                return (float)GetFrameIndex() / (float)((int)(playingClip.length * playingClip.fps) - 1);
            }
        }
        set
        {
            if (playingClip != null)
            {
                float v = Mathf.Clamp01(value);
                if (WrapMode == GPUSkinningWrapMode.Once)
                {
                    this.time = v * playingClip.length;
                }
                else if (WrapMode == GPUSkinningWrapMode.Loop)
                {
                    if (playingClip.individualDifferenceEnabled)
                    {
                        res.Time = playingClip.length + v * playingClip.length - this.timeDiff;
                    }
                    else
                    {
                        res.Time = v * playingClip.length;
                    }
                }
                else
                {
                    throw new System.NotImplementedException();
                }
            }
        }
    }

    public GPUSkinningPlayer(GameObject attachToThisGo, GPUSkinningPlayerResources res, Material hlmat )
    {
        go              = attachToThisGo;
        transform       = go.transform;
        this.res        = res;
        hihgtLightmaterial = hlmat;
        if (hihgtLightmaterial != null)
        {
            hihgtLightmaterial.enableInstancing = true;
            hihgtLightmaterial.EnableKeyword("ROOTOFF_BLENDOFF");
        }

        mr              = go.GetComponent<MeshRenderer>();
        if (mr == null)
        {
            mr          = go.AddComponent<MeshRenderer>();
        }
        mf              = go.GetComponent<MeshFilter>();
        if (mf == null)
        {
            mf          = go.AddComponent<MeshFilter>();
        }

        GPUSkinningMaterial mtrl = GetCurrentMaterial();
        mr.sharedMaterial        = mtrl == null ? null : mtrl.material;
        mf.sharedMesh            = res.mesh;
        mpb                      = new MaterialPropertyBlock();
        normaterial              = mtrl.material;
    }

    public void Play(string clipName)
    {
        GPUSkinningAnimationClip[] clips = res.anim.clips;
        int numClips = clips == null ? 0 : clips.Length;
        string lower = clipName.ToLower();
        for (int i = 0; i < numClips; ++i)
        {
            if (clips[i].name == clipName || clips[i].name == lower )
            {
                if (playingClip != clips[i] ||
                    (playingClip != null && playingClip.wrapMode == GPUSkinningWrapMode.Once && IsTimeAtTheEndOfLoop) ||
                    (playingClip != null && !isPlaying))
                {
                    SetNewPlayingClip(clips[i]);
                }
                return;
            }
        }
    }


    public GPUSkinningAnimationClip GetCurrentAnimatorStateInfo()
    {
        return playingClip;
    }

    
    public void Stop()
    {
        isPlaying = false;
    }

    public void Resume()
    {
        if (playingClip != null)
        {
            isPlaying = true;
        }
    }


    private void SetNewPlayingClip(GPUSkinningAnimationClip clip)
    {
        lastPlayedClip          = playingClip;

        isPlaying               = true;
        playingClip             = clip;
        time                    = 0;
        /// 设置动作不同的时间偏移
        timeDiff                = Random.Range(0, playingClip.length);
    }

    public void Update_Internal(float timeDelta)
    {
        if (!isPlaying || playingClip == null)
        {
            return;
        }

        GPUSkinningMaterial currMtrl = GetCurrentMaterial();
        if (currMtrl == null)
        {
            return;
        }

        if (playingClip.wrapMode == GPUSkinningWrapMode.Loop)
        {
            UpdateMaterial(timeDelta, currMtrl);
        }
        else if (playingClip.wrapMode == GPUSkinningWrapMode.Once)
        {
            if (time >= playingClip.length)
            {
                time = playingClip.length;
                UpdateMaterial(timeDelta, currMtrl);
            }
            else
            {
                UpdateMaterial(timeDelta, currMtrl);
                time += (timeDelta * _playspeed);
                if (time > playingClip.length)
                {
                    time = playingClip.length;
                }
            }
        }
        else
        {
            throw new System.NotImplementedException();
        }
    }

    private void UpdateMaterial(float deltaTime, GPUSkinningMaterial currMtrl)
    {
        int frameIndex = GetFrameIndex();
        if (lastPlayingClip == playingClip && lastPlayingFrameIndex == frameIndex)
        {
            // 切换不同的动作，才通知shader更新参数
            res.Update(deltaTime, currMtrl, _playspeed );
            res.UpdateEffect(mpb, colorsetter);
            mr.SetPropertyBlock(mpb);
            return;
        }
        lastPlayingClip             = playingClip;
        lastPlayingFrameIndex       = frameIndex;

        if (Visible || CullingMode == GPUSKinningCullingMode.AlwaysAnimate)
        {
            res.Update(deltaTime, currMtrl, _playspeed);
            res.UpdatePlayingData(mpb, playingClip, frameIndex);
            res.UpdateEffect(mpb, colorsetter);
            mr.SetPropertyBlock(mpb);
        }
    }

    
    public void SetExposureAngGrayVector(Vector4 value)
    {
        colorsetter.x = value.x;
        colorsetter.y = value.y;
        colorsetter.z = value.z;
    }

    public void SetHightLight( bool bActive )
    {
        if (hihgtLightmaterial == null) return;
        GPUSkinningMaterial currMtrl = GetCurrentMaterial();
        if (currMtrl == null)
        {
            return;
        }
        
        if (bActive)
        {
            currMtrl.material           = hihgtLightmaterial;
            mr.sharedMaterial           = hihgtLightmaterial;
        }
        else
        {
            currMtrl.material           = normaterial;
            mr.sharedMaterial           = normaterial;
        }
    }


    public void SetAlpha(float f )
    {
        colorsetter.z = f;
    }

    private GPUSkinningMaterial GetCurrentMaterial()
    {
        if (res == null)
        {
            return null;
        }
        return res.GetMaterial();
    }

    private float GetCurrentTime()
    {
        float time = 0;
        if (WrapMode == GPUSkinningWrapMode.Once)
        {
            time = this.time;
        }
        else if (WrapMode == GPUSkinningWrapMode.Loop)
        {
            time = res.Time + (animationOffsetenable ? this.timeDiff : 0);
        }
        else
        {
            throw new System.NotImplementedException();
        }
        return time;
    }

    private int GetFrameIndex()
    {
        float time = GetCurrentTime();
        if (playingClip.length == time)
        {
            return GetTheLastFrameIndex_WrapMode_Once(playingClip);
        }
        else
        {
            return GetFrameIndex_WrapMode_Loop(playingClip, time);
        }
    }
    
    private int GetTheLastFrameIndex_WrapMode_Once(GPUSkinningAnimationClip clip)
    {
        return (int)(clip.length * clip.fps) - 1;
    }

    private int GetFrameIndex_WrapMode_Loop(GPUSkinningAnimationClip clip, float time)
    {
        return (int)(time * clip.fps) % (int)(clip.length * clip.fps);
    }
}
