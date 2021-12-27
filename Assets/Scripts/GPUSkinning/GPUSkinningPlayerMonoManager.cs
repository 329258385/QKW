using System.Collections;
using System.Collections.Generic;
using UnityEngine;





public class GPUSkinningPlayerMonoManager
{
    private List<GPUSkinningPlayerResources> mapInstances = new List<GPUSkinningPlayerResources>();
   

    public void Register(GPUSkinningAnimation anim, Mesh mesh, Material originalMtrl, TextAsset textureRawData, GPUSkinningPlayerMono player, out GPUSkinningPlayerResources resources)
    {
        resources = null;
        if (anim == null || originalMtrl == null || textureRawData == null || player == null)
        {
            return;
        }

        GPUSkinningPlayerResources item = null;
        int numItems = mapInstances.Count;
        int srcMatHashCode = originalMtrl.name.GetHashCode();
        for (int i = 0; i < numItems; ++i)
        {
            int resHashColde = mapInstances[i].GetMaterial().HashName;
            if (mapInstances[i].anim.guid == anim.guid && srcMatHashCode == resHashColde )
            {
                item = mapInstances[i];
                break;
            }
        }

        if (item == null)
        {
            item = new GPUSkinningPlayerResources();
            mapInstances.Add(item);
        }

        if (item.anim == null)
        {
            item.anim = anim;
        }

        if (item.mesh == null)
        {
            item.mesh = mesh;
        }

        item.InitMaterial(originalMtrl, HideFlags.None);

        if (item.texture == null)
        {
            item.texture = GPUSkinningUtil.CreateTexture2D(textureRawData, anim);
        }

        if (!item.players.Contains(player))
        {
            item.players.Add(player);
            //item.AddCullingBounds();
        }

        resources = item;
    }

    public void Unregister(GPUSkinningPlayerMono player)
    {
        if (player == null)
        {
            return;
        }

        int numItems = mapInstances.Count;
        for (int i = 0; i < numItems; ++i)
        {
            int playerIndex = mapInstances[i].players.IndexOf(player);
            if (playerIndex != -1)
            {
                mapInstances[i].players.RemoveAt(playerIndex);
                //mapInstances[i].RemoveCullingBounds(playerIndex);
                if (mapInstances[i].players.Count == 0)
                {
                    mapInstances[i].Destroy();
                    mapInstances.RemoveAt(i);
                }
                break;
            }
        }
    }
}

