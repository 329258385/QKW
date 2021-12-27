using System;
using UnityEngine;


public class WorldCameraControl : CameraControl
{
    /// <summary>
    /// 滑动
    /// </summary>
    private Vector3     moveTarget      = Vector3.zero;
    private Vector3     currentVelocity = Vector3.zero;

    private bool        OnMouseDown = false;
    private bool        OnMouseMove = false;


    public override void Update()
    {
        if (IsSelectedNode) return;
        if (Input.GetMouseButtonDown(0))
        {
            {
                OnMouseDown = true;
            }
        }

        if (OnMouseMove)
        {
            transform.position = Vector3.SmoothDamp(transform.position, moveTarget, ref currentVelocity, 0.2f);
            //Debug.LogError("Pos = " + transform.position.ToString());
            if (Vector3.Distance(transform.position, moveTarget) < 0.01f)
            {
                OnMouseMove     = false;
                currentVelocity = Vector3.zero;
                moveTarget      = transform.position;
            }
        }
        base.Update();
    }


    public override void On1TouchMove(object sender, EventArgs e)
    {
        var args = e as EventArgs_SinVal<Vector2>;
        if (args != null)
        {
            OnMouseMove     = true;
            float scale     = Screen.dpi;
            float ap        = (float)Screen.width * 1.0f / Screen.height;
            Vector3 pos     = new Vector3(args.Val.x * scale, 0, args.Val.y * scale * ap);
            Vector3 move    = mainCamera.transform.TransformPoint(pos);
            moveTarget      = FilterPostion(move);
        }
    }

    public override void On1TouchDown(object sender, EventArgs e)
    {
        base.On1TouchDown(sender, e);
    }

    public override void On1TouchUp(object sender, EventArgs e)
    {
        OnMouseMove         = false;
        moveTarget          = transform.position;
    }

    public override Vector3 FilterPostion(Vector3 v3)
    {
        float offsetX = 0;// mainCamera.orthographicSize * 0.5f;
        float offsetY = 0;// mainCamera.orthographicSize * 0.5f;

        v3.y = cameraHeight;
        if (v3.x < mapRect.x + offsetX)
        {
            v3.x = mapRect.x + offsetX;
        }

        if (v3.x > mapRect.z - offsetX)
        {
            v3.x = mapRect.z - offsetX;
        }

        if (v3.z < mapRect.y + offsetY)
        {
            v3.z = mapRect.y + offsetY;
        }

        if (v3.z > mapRect.w - offsetY)
        {
            v3.z = mapRect.w - offsetY;
        }
        return v3;
    }

    public Vector3 GetIntersectWithLineAndPlane( Vector3 point, Vector3 direct, Vector3 planeNormal, Vector3 planePoint )
    {
        float d = Vector3.Dot(planePoint - point, planeNormal) / Vector3.Dot(direct.normalized, planeNormal);
        return d * direct.normalized + point;
    }
}
