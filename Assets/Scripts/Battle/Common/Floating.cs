using UnityEngine;
using System.Collections;




public class Floating : MonoBehaviour 
{
    float radian        = 0;
    float perRadian     = 0.03f;
    float radius        = 0.8f;

    Vector3 oldPos      = Vector3.zero;
	private Transform   _cacheTransform;
	
	void Start () {

        oldPos          = transform.position;
        _cacheTransform = transform;
    }
	
	void Update () 
    {
        radian      += perRadian;
        float dy    = Mathf.Cos(radian) * 0.1f;
        _cacheTransform.position = oldPos + new Vector3(0, dy, 0);
    }
}
