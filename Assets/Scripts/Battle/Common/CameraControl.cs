using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using Solarmax;



public class CameraControl : MonoBehaviour
{
    public float                    mYawAngle = 45f;
    [HideInInspector]
    public float                    mZoomAngle = 1f;

    [Header("Header Tips")]
    public float                    stickMinZoom;
    public float                    stickMaxZoom;

    public float                    minFovDegree;
    public float                    maxFovDegree;

    public float                    minCameraHeight;
    public float                    maxCameraHeight;
    protected float                 cameraHeight;

    private float                   zoomDelta = 0f;
    private float                   zoomAccumulation = 0f;

    [HideInInspector]
    public Camera                   mainCamera;
    public static bool              Drag = false;
    public static bool              Zoom = false;

    /// <summary>
    /// 滑动事件监听器
    /// </summary>
    public Vector4                  mapRect;

    /// <summary>
    /// 
    /// </summary>
    public static CameraControl     Instance = null;
    public bool                     IsSelectedNode = false;

    private void Awake()
    {
        Instance                    = this;
        cameraHeight                = maxCameraHeight;
        mainCamera                  = GetComponent<Camera>();
    }

    private void OnDestroy()
    {
        Clear();
    }


    /// <summary>
    /// 
    /// </summary>
    public void Init()
    {
        EventHandlerGroup.Get().addEventHandler((int)EventTypeGroup.On1TouchMove, On1TouchMove);
        EventHandlerGroup.Get().addEventHandler((int)EventTypeGroup.On1TouchUp,   On1TouchUp);
        EventHandlerGroup.Get().addEventHandler((int)EventTypeGroup.On2TouchMove, On2TouchMove);
        EventHandlerGroup.Get().addEventHandler((int)EventTypeGroup.On1TouchDown, On1TouchDown);
    }

    /// <summary>
    /// 
    /// </summary>
    public void Clear()
    {
        EventHandlerGroup.Get().delEventHandler((int)EventTypeGroup.On1TouchMove, On1TouchMove);
        EventHandlerGroup.Get().delEventHandler((int)EventTypeGroup.On1TouchUp,   On1TouchUp);
        EventHandlerGroup.Get().delEventHandler((int)EventTypeGroup.On2TouchMove, On2TouchMove);
        EventHandlerGroup.Get().delEventHandler((int)EventTypeGroup.On1TouchDown, On1TouchDown);
    }


    /// <summary>
    /// 更新滑动
    /// </summary>
    public virtual void Update()
    {
        if (IsSelectedNode) return;

        if(zoomDelta > 0 && zoomAccumulation < zoomDelta )
        {
            zoomAccumulation += (Time.deltaTime * 0.5f);
            mZoomAngle        = Mathf.Clamp01(mZoomAngle + Time.deltaTime);
        } 
        
        if(zoomDelta < 0 && zoomAccumulation > zoomDelta )
        {
            zoomAccumulation -= (Time.deltaTime * 0.5f);
            mZoomAngle        = Mathf.Clamp01(mZoomAngle - Time.deltaTime);
        }

        float distance  = Mathf.Lerp(minCameraHeight, maxCameraHeight, mZoomAngle);
        cameraHeight    = distance;

        var xpos        = mainCamera.transform.localPosition.x;
        var zpos        = mainCamera.transform.localPosition.z;
        mainCamera.transform.position = new Vector3(xpos, distance, zpos);

        var fov         = Mathf.Lerp(maxFovDegree, minFovDegree, mZoomAngle);
        mainCamera.fieldOfView = fov;

        var angle       = Mathf.Lerp(stickMaxZoom, stickMinZoom, mZoomAngle );
        mainCamera.transform.localEulerAngles = new Vector3(angle, mYawAngle, 0);
    }


    public virtual void On2TouchMove(object sender, EventArgs e)
    {
        var args        = e as EventArgs_SinVal<float>;
        if (args != null)
        {
            zoomDelta        = args.Val;
            zoomAccumulation = 0f;
            //Debug.LogError("Zoom = " + zoomDelta.ToString());
        }
    }

    public virtual void On1TouchDown(object sender, EventArgs e)
    {
        
    }

    public virtual void On1TouchUp(object sender, EventArgs e)
    {

    }

    public virtual void On1TouchMove(object sender, EventArgs e)
    {
        if (Drag == false) return;
        var args = e as EventArgs_SinVal<Vector2>;
        if (args != null)
        {
            Vector3 pos = new Vector3(transform.position.x + args.Val.x, transform.position.y + args.Val.y, transform.position.z);
            transform.position = FilterPostion(pos);
        }
    }

    public virtual Vector3 FilterPostion( Vector3 v3 )
    {
        float offsetX = 0;// mainCamera.orthographicSize * 0.5f;
        float offsetY = 0;// mainCamera.orthographicSize * 0.5f;

        if (v3.x < mapRect.x + offsetX)
        {
            v3.x = mapRect.x + offsetX;
        }

        float aspect = mainCamera.aspect;
        if (v3.x > mapRect.z - offsetX)
        {
            v3.x = mapRect.z - offsetX;
        }

        if (v3.y < minCameraHeight)
        {
            v3.y = minCameraHeight;
        }

        if (v3.y > maxCameraHeight)
        {
            v3.y = maxCameraHeight;
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

    public static List<RaycastResult> IsPointerOverGameObject()
    {
        PointerEventData eventData  = new PointerEventData(UnityEngine.EventSystems.EventSystem.current);
        eventData.pressPosition     = Input.mousePosition;
        eventData.position          = Input.mousePosition;
        List<RaycastResult> list    = new List<RaycastResult>();
        if (UnityEngine.EventSystems.EventSystem.current != null)
            UnityEngine.EventSystems.EventSystem.current.RaycastAll(eventData, list);
        return list;

    }
}
