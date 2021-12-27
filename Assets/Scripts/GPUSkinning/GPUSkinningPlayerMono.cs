using System.Collections;
using System.Collections.Generic;
using UnityEngine;





[ExecuteInEditMode]
public class GPUSkinningPlayerMono : MonoBehaviour
{
    [HideInInspector]
    [SerializeField]
    private GPUSkinningAnimation    anim = null;

    [HideInInspector]
    [SerializeField]
    private Mesh                    mesh = null;

    [HideInInspector]
    [SerializeField]
    private Material                mtrl = null;

    [HideInInspector]
    [SerializeField]
    private Material                HightLight = null;


    [HideInInspector]
    [SerializeField]
    private TextAsset               textureRawData = null;

    [HideInInspector]
    [SerializeField]
    private int                     defaultPlayingClipIndex = 0;

    [HideInInspector]
    [SerializeField]
    private bool                    rootMotionEnabled = false;

    [HideInInspector]
    [SerializeField]
    private GPUSKinningCullingMode  cullingMode = GPUSKinningCullingMode.CullUpdateTransforms;

    public float _speed              = 1.0f;
    private static GPUSkinningPlayerMonoManager playerManager = new GPUSkinningPlayerMonoManager();
    private GPUSkinningPlayer       player = null;
    public GPUSkinningPlayer Player
    {
        get
        {
            return player;
        }
    }

    public float speed
    {
        set
        {
            _speed = value;
            if (player != null)
                player.speed = _speed;
        }
        get
        {
            return _speed;
        }
    }

    public void Init(GPUSkinningAnimation anim, Mesh mesh, Material mtrl, TextAsset textureRawData)
    {
        if (player != null)
        {
            return;
        }

        this.anim = anim;
        this.mesh = mesh;
        this.mtrl = mtrl;
        this.textureRawData = textureRawData;
        Init();
    }

    public void Init()
    {
        if (player != null)
        {
            return;
        }


        if (anim != null && mesh != null && mtrl != null && textureRawData != null)
        {
            GPUSkinningPlayerResources res = null;
            if (Application.isPlaying)
            {
                playerManager.Register(anim, mesh, mtrl, textureRawData, this, out res);
            }
            

            if( res == null )
            {
                res = new GPUSkinningPlayerResources();
                res.anim = anim;
                res.mesh = mesh;
                res.InitMaterial(mtrl, HideFlags.DontSaveInBuild | HideFlags.DontSaveInEditor);
                res.texture = GPUSkinningUtil.CreateTexture2D(textureRawData, anim);
                res.texture.hideFlags = HideFlags.DontSaveInBuild | HideFlags.DontSaveInEditor;
            }

            player = new GPUSkinningPlayer(gameObject, res, HightLight);
            player.RootMotionEnabled = Application.isPlaying ? rootMotionEnabled : false;
            player.CullingMode = cullingMode;
            player.Visible = true;
            if (anim != null && anim.clips != null && anim.clips.Length > 0)
            {
                player.Play(anim.clips[Mathf.Clamp(defaultPlayingClipIndex, 0, anim.clips.Length)].name);
            }
        }
    }


    public void DeletePlayer()
    {
        player = null;
    }

    public void UpdateAnimr(float deltaTime)
    {
        if (player != null && !Application.isPlaying)
        {
            player.Update_Internal(deltaTime);
        }
    }

    public void Play(string clipName)
    {
        if (player != null )
        {
            player.Play(clipName);
        }
    }

    public GPUSkinningAnimationClip GetCurrentAnimatorStateInfo()
    {
        return player.GetCurrentAnimatorStateInfo();
    }

    public bool HasState( int name )
    {
        return anim.HasAnimationClip(name);
    }


    private void Awake()
    {
        
    }

    private void Start()
    {
        Init();
        #if UNITY_EDITOR
        UpdateAnimr(0);
        #endif
    }

    private void Update()
    {
        if (player != null)
        {
            
            if (Application.isPlaying)
            {
                player.Update_Internal(Time.deltaTime);
            }
            else
            {
                player.Update_Internal(0);
            }
        }
    }

    private void OnDestroy()
    {
        player          = null;
        anim            = null;
        mesh            = null;
        mtrl            = null;
        textureRawData  = null;

        if (Application.isPlaying)
        {
            playerManager.Unregister(this);
        }
    }
}

