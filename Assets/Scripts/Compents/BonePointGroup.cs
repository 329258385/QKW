using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;





public class BonePointGroup
{
    public enum BonePointType
    {
        Action,
        Custom
    }

    public BonePointType                    PointType = BonePointType.Action;


    public Dictionary<string, BonePoint>    Points = new Dictionary<string, BonePoint>();


    public void InitBone( GameObject go )
    {
        var bps = go.GetComponentsInChildren<BonePoint>();
        for( int i = 0; i < bps.Length; i++ )
        {
            Points.Add(bps[i].name, bps[i]);
        }
    }

    public Transform GetBonePoint(string bpName)
    {
        if (!string.IsNullOrEmpty(bpName))
        {
            if (Points.ContainsKey(bpName))
            {
                return Points[bpName].transform;
            }
        }

        return null;
    }
}

