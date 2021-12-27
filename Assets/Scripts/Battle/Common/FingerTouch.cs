using UnityEngine;
using UnityEngine.EventSystems;

public class FingerTouch : MonoBehaviour
{
    public float    minDistance = 20f;
    public float    maxDistance = 60f;
    public float    scaleSmooth = 0.1f;

    private Touch   oldTouch1;
    private Touch   oldTouch2;

    public Camera scalCamera;
    void Start()
    {
        Input.multiTouchEnabled = true;//开启多点触碰
        scalCamera              = Camera.main;
    }


    void Update()
    {
        if( Input.touchCount <= 0 )
        {
            return;
        }

        // 水平上下移动
        if( Input.touchCount == 1 && Input.GetTouch(0).phase == TouchPhase.Moved )
        {
            if (!EventSystem.current.IsPointerOverGameObject(Input.GetTouch(0).fingerId))
            {
                var deltaposition = Input.GetTouch(0).deltaPosition;
                transform.Translate(-deltaposition.x * 0.1f, 0f, -deltaposition.y * 0.1f);
            }
        }

        UpdateCamerFOV();
    }

    void UpdateCamerFOV()
    {

        //多点触摸, 放大缩小  
        Touch newTouch1 = Input.GetTouch(0);
        Touch newTouch2 = Input.GetTouch(1);

        //第2点刚开始接触屏幕, 只记录，不做处理  
        if (newTouch2.phase == TouchPhase.Began)
        {
            oldTouch2 = newTouch2;
            oldTouch1 = newTouch1;
            return;
        }

        //计算老的两点距离和新的两点间距离，变大要放大模型，变小要缩放模型  
        float oldDistance       = Vector2.Distance(oldTouch1.position, oldTouch2.position);
        float newDistance       = Vector2.Distance(newTouch1.position, newTouch2.position);

        //两个距离之差，为正表示放大手势， 为负表示缩小手势  
        float offset = newDistance - oldDistance;
        scalCamera.fieldOfView  += offset * scaleSmooth;
        scalCamera.fieldOfView  = Mathf.Clamp(scalCamera.fieldOfView, minDistance, maxDistance);
        //记住最新的触摸点，下次使用  
        oldTouch1 = newTouch1;
        oldTouch2 = newTouch2;
    }
}

