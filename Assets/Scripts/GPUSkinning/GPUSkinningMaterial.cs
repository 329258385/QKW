/*
-----------------------------------------------------------------------------------------------------
    骨骼动画---记录动画
    log:  add by ljp 2021--3-29
-----------------------------------------------------------------------------------------------------
*/
using UnityEngine;



public class GPUSkinningMaterial
{
    public int HashName = -1;
    public Material material = null;
    public GPUSkinningExecutePerFrame executeOncePerFrame = new GPUSkinningExecutePerFrame();

    public void Destroy()
    {
        if (material != null)
        {
            Object.Destroy(material);
            material = null;
        }
    }
}


