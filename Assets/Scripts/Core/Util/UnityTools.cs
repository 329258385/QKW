using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace Solarmax
{
    public class UnityTools
    {
        public static GameObject AddChild(GameObject parent, GameObject prefab)
        {
            GameObject go = GameObject.Instantiate(prefab) as GameObject;

            if (null != go && null != parent)
            {
                Transform t = go.transform;
                t.SetParent(parent.transform);
                t.localPosition = Vector3.zero;
                t.localRotation = Quaternion.identity;
                t.localScale = Vector3.one;
                go.layer = parent.layer;
            }

            return go;
        }


        public static Vector3 CalculateFireVector(float speedModifier, Vector3 targetPosition, Vector3 firePosition, float launchAngle)
        {
            Vector3 target          = targetPosition;
            target.y                = firePosition.y;
            Vector3 toTarget        = target - firePosition;
            float targetDistance    = toTarget.magnitude;
            float shootingAngle     = launchAngle;
            float grav              = Mathf.Abs(Physics.gravity.y);
            grav                    *= speedModifier;// 重力速度修正
            float relativeY         = firePosition.y - targetPosition.y;

            float theta             = Mathf.Deg2Rad * shootingAngle;
            float cosTheta          = Mathf.Cos(theta);
            float num               = targetDistance * Mathf.Sqrt(grav) * Mathf.Sqrt(1 / cosTheta);
            float denom             = Mathf.Sqrt(2 * targetDistance * Mathf.Sin(theta) + 2 * relativeY * cosTheta);
            float v                 = num / denom;

            if (targetDistance == 0)
                targetDistance = 1.0f;

            Vector3 aimVector       = toTarget / targetDistance;
            aimVector.y             = 0;
            Vector3 rotAxis         = Vector3.Cross(aimVector, Vector3.up);
            Quaternion rotation     = Quaternion.AngleAxis(shootingAngle, rotAxis);
            aimVector               = rotation * aimVector.normalized;

           
            return aimVector * v;
        }
    }

}
