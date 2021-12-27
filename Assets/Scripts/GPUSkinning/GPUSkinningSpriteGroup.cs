using System;
using System.Collections.Generic;
using UnityEngine;




/// <summary>
/// 管理一小队内的角色移动和动画状态
/// </summary>
public class GPUSkinningSpriteGroup : MonoBehaviour
{
    public List<SrInfo>             spriteRendLists;
    private List<GameObject>        spriteLists = new List<GameObject>();
    public class SrInfo
    {
        public GPUSkinningPlayerMono player;
        public MeshRenderer         sprite;
        public Transform            Parent;
        public Vector3              StartPos;
        public Vector3              TargetPos;
        public float                RunTime;
        public float                RunTimeMax = 1;
        public bool                 Die = false;
    }

    public bool                     bDie;
    private bool                    _isRun;

    
    public float                    DispersedRange = 1.2f;
    public float                    DispersedTimeMin = 0.5f;
    public float                    DispersedTimeMax = 1.5f;
    public float                    DispersedSpeed = 3;

    private int                     _count = 0;
    private static int              shaderPropID_DyncAlpha = -1;
    private void Awake()
    {
        if( shaderPropID_DyncAlpha != -1 )
        {
            shaderPropID_DyncAlpha = Shader.PropertyToID("_DyncAlpha");
        }
    }

    public void Init()
    {
        spriteRendLists = new List<SrInfo>();
        if (transform.childCount > 0)
        {
            var ani = transform.GetComponentsInChildren<GPUSkinningPlayerMono>();
            if (ani != null)
            {
                for (int i = 0; i < ani.Length; i++)
                {
                    var tr          = ani[i];
                    tr.gameObject.SetActive(true);
                    ani[i].Init();
                    var sri         = new SrInfo();
                    sri.player      = ani[i];
                    sri.player.Player.animationOffsetenable = true;
                    sri.sprite      = sri.player.Player.Render;
                    sri.Parent      = tr.transform;
                    sri.RunTimeMax  = 0f;
                    sri.StartPos    = tr.transform.localPosition;
                    spriteRendLists.Add(sri);
                }
            }

            foreach( var obj in ani)
            {
                var sprites = obj.transform.Find("slg_b_yinying");
                if( sprites != null )
                {
                    spriteLists.Add(sprites.gameObject);
                    sprites.gameObject.SetActive(true);
                }
            }
        }
        bDie            = false;
    }

    public void SetAlpha(float alpha)
    {
        foreach (var it in spriteRendLists)
        {
            if (it.player != null && it.player.Player != null)
                it.player.Player.SetAlpha(alpha);
        }

        if (alpha <= 0.1f)
        {
            foreach (var sp in spriteLists)
            {
                sp.SetActive(false);
            }
        }
    }

    public void SetHihgtLight( bool bActive )
    {
        foreach (var it in spriteRendLists)
        {
            if (it.player != null && it.player.Player != null)
                it.player.Player.SetHightLight(bActive);
        }
    }


    public void SetHp(double hp, double hpMax)
    {
        var v = (hp / hpMax) / (1f / spriteRendLists.Count);
        var count = (int)Math.Ceiling(v);
        var hide = spriteRendLists.Count - count;
        while (hide > _count)
        {
            var hideIndex = UnityEngine.Random.Range(0, spriteRendLists.Count);
            if (spriteRendLists[hideIndex].Die == false)
            {
                OnDie(spriteRendLists[hideIndex]);
                _count++;
            }
        }
    }
    public void OnPlay( string name , float speed, bool isRun = false )
    {
        foreach (var it in spriteRendLists)
        {
            it.player.speed = speed;
            it.player.Play(name);
        }
        _isRun = isRun;
    }

    public void Speed(float speed )
    {
        foreach (var it in spriteRendLists)
        {
            it.player.speed = speed;
        }
    }

    void CloseAnimationOffSet()
    {
        foreach( var it in spriteRendLists )
        {
            it.player.Player.animationOffsetenable = false;
        }
    }


    void OnDie( SrInfo sr )
    {
        sr.Die = true;
        if (sr.Parent.childCount == 2)
            sr.Parent.GetChild(1).gameObject.SetActive(false);
    }


    public void ReSet()
    {
        foreach (var srs in spriteRendLists)
        {
            if (srs.Parent != null)
            {
                srs.Parent.localPosition = srs.StartPos;
                srs.Die = false;
                if (srs.Parent.childCount == 2)
                {
                    srs.Parent.GetChild(1).gameObject.SetActive(true);
                }
            }
        }
    }

    private void SetSr(SrInfo sr)
    {
        sr.RunTimeMax = UnityEngine.Random.Range(DispersedTimeMin, DispersedTimeMax);
        var p = sr.Parent.position;
        sr.Parent.localPosition = sr.StartPos;
        sr.Parent.position = sr.Parent.position +
            new Vector3(UnityEngine.Random.Range(-DispersedRange, DispersedRange), 0,
            UnityEngine.Random.Range(-DispersedRange, DispersedRange));
        sr.TargetPos = sr.Parent.localPosition;
        sr.Parent.position = p;
        sr.RunTime = 0;
    }

    void Update()
    {
        if (bDie || !_isRun )
            return;

        for (int j = 0; j < spriteRendLists.Count; j++)
        {
            var srs = spriteRendLists[j];
            if (_isRun && srs.Parent != null)
            {
                if (srs.RunTime >= srs.RunTimeMax)
                {
                    SetSr(srs);
                }
                srs.Parent.localPosition +=
                    (srs.TargetPos - srs.Parent.localPosition).normalized *
                    Time.deltaTime * DispersedSpeed;
                srs.RunTime += Time.deltaTime;
            }
        }
    }
}
