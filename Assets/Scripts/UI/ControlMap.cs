using UnityEngine;
using System.Collections;
using System.Collections.Generic;






public class ControlMap : MonoBehaviour
{
    /// <summary>
    /// 
    /// </summary>
    public UIEventListener          moveTrigger;


    private Vector2                 _vector2;
   

    private void Awake()
    {
        moveTrigger.onDragStart     = OnTriggerDragStart;
        moveTrigger.onDrag          = OnTriggerDrag;
        moveTrigger.onDragEnd       = OnTriggerDragEnd;
    }


    /// ----------------------------------------------------------------------------------------------------
    /// <summary>
    /// 开始拖动
    /// </summary>
    /// ----------------------------------------------------------------------------------------------------
    private void OnTriggerDragStart( GameObject go )
    {
        _vector2                    = new Vector2(Input.mousePosition.x, Input.mousePosition.y);
    }


    /// ----------------------------------------------------------------------------------------------------
    /// <summary>
    /// 拖动中
    /// </summary>
    /// ----------------------------------------------------------------------------------------------------
    private void OnTriggerDrag( GameObject go, Vector2 delta )
    {
        if (CameraControl.Instance.IsSelectedNode)
            return;

        var mousePos                = new Vector2(Input.mousePosition.x, Input.mousePosition.y);
        var dis                     = Vector2.Distance(_vector2, mousePos);
        if (dis > 0.1f) 
        {
            EventHandlerGroup.Get().fireEvent((int)EventTypeGroup.On1TouchMove, this, new EventArgs_SinVal<Vector2>((_vector2 - mousePos) / 50f));
            _vector2                = mousePos;
        }
    }


    /// ----------------------------------------------------------------------------------------------------
    /// <summary>
    /// 拖动结束
    /// </summary>
    /// ----------------------------------------------------------------------------------------------------
    private void OnTriggerDragEnd( GameObject go )
    {
        _vector2                    = Vector2.zero;
        EventHandlerGroup.Get().fireEvent((int)EventTypeGroup.On1TouchUp, this, null);
    }
}

