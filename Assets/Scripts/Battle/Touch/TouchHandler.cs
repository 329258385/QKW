using UnityEngine;
using UnityEngine.EventSystems;
using System.Collections;
using Solarmax;
using TouchInput;

public class TouchHandler : MonoBehaviour
{
    private Node                    mNode = null;
    [HideInInspector]
    public static Node              currentNode;
    [HideInInspector]
    public static Node              currentSelect;

    [HideInInspector]
    public static SpriteRenderer    beginHalo;
    [HideInInspector]
    public static SpriteRenderer    endHalo;
    [HideInInspector]
    public static LineRenderer      line;

	public static bool              IsPressGuid = false;
    
	static Vector3                  eulerAngle = Vector3.zero;
	static int                      isWarning = 0;       // 0 白色、1 红色、2绿色
	

	public static void Clean()
	{
		currentNode     = null;
		currentSelect   = null;
        Debug.Log("currentSelect4 == null ");
	}

	public void SetWarning(int warning)
	{
		isWarning = warning;

        if (isWarning == 1 )
        {
			beginHalo.color = Color.red;
			endHalo.color   = Color.red;
			line.startColor = Color.red;
            line.endColor   = Color.red;
		} 
        else if(isWarning == 0 )
        {
			beginHalo.color = Color.white;
			endHalo.color   = Color.white;
			line.startColor = Color.white;
            line.endColor   = Color.white;
		}
        else if(isWarning == 2 )
        {
			beginHalo.color = Color.green;
			endHalo.color   = Color.green;
			line.endColor   = Color.green;
            line.startColor = Color.green;
		}
	}


	public void SetNode(Node node)
	{
		mNode           = node;
        beginHalo       = BuildTypeManager.Instance.beginHalo;
        endHalo         = BuildTypeManager.Instance.endHalo;
        line            = BuildTypeManager.Instance.line;
		SetWarning (0);
	} 


    public static void SelectHeroNode( Node curentNode )
    {
        beginHalo           = BuildTypeManager.Instance.beginHalo;
        endHalo             = BuildTypeManager.Instance.endHalo;
        line                = BuildTypeManager.Instance.line;

        beginHalo.transform.parent = curentNode.GetGO().transform;
        beginHalo.gameObject.transform.localPosition = Vector3.zero;
        beginHalo.gameObject.transform.localEulerAngles = new Vector3(90, 90, 0);
        beginHalo.gameObject.transform.localScale = new Vector3(14, 14, 0);
        beginHalo.gameObject.SetActive(true);
        beginHalo.enabled   = true;
        Color color         = curentNode.currentTeam.color;
        color.a             = 1.0f;
        beginHalo.color     = color;
        currentNode         = curentNode;
        currentSelect       = null;
    }
		
	/// <summary>
	/// 按下事件，isDown代表按下还是弹起
	/// </summary>
	void OnPress(bool isDown)
	{

		if (isDown) {

            TouchInputController.Instance.IsSelectedNode           = true;
            IsPressGuid = true;
			if (mNode == null)
				return;

            if ( mNode.nodeIsHide && 
                 mNode.currentTeam.team != BattleSystem.Instance.battleData.currentTeam)
                return;

            beginHalo.transform.parent                      = mNode.GetGO().transform;
			beginHalo.gameObject.transform.localPosition    = Vector3.zero;
            beginHalo.gameObject.transform.localEulerAngles = new Vector3(90, 90, 0);
            beginHalo.gameObject.transform.localScale       = new Vector3(14, 14, 0);
			beginHalo.gameObject.SetActive (true);
            beginHalo.enabled   = true;
            Color color         = mNode.currentTeam.color;
            color.a             = 1.0f;
            beginHalo.color     = color;
			currentNode         = mNode;
			currentSelect       = null;
		}
        else 
        {

            TouchInputController.Instance.IsSelectedNode = false;
            if (mNode == null)
				return;

			if (mNode != currentNode)
				return;

            if (currentNode != currentSelect && currentNode.GetAttribute(NodeAttr.Ice) <= 0) 
			{
				//if (isWarning == 0 || isWarning == 2)
                {
					if (currentNode != null && currentSelect != null) 
                    {
                        BattleSystem.Instance.OnPlayerMove(currentNode, currentSelect, BattleSystem.Instance.battleData.BattleTeamID);
					}
				}
			}

			if(currentSelect != null)
				currentSelect.ShowRange (false);

			if(line != null)line.gameObject.SetActive (false);
			currentNode     = null;
			currentSelect   = null;
            SetWarning (0);
            OnFadeOutHalo();
            IsPressGuid = false;
        }
	}

   
	/// <summary>
	/// 拖动事件
	/// </summary>
	/// <param name="pos">Position.</param>
	void OnDrag(Vector2 pos)
	{
        // Debug.Log("TouchHandler OnDrag---------------");
        if (mNode == null)
        {
           // Debug.Log("TouchHandler OnDrag step 2");
            return;
        }

        if (currentNode == null)
        {
            //Debug.Log("TouchHandler OnDrag---------------y");
            return;
        }


        if (currentSelect == currentNode)
        {
            //line.width = 0;
            line.transform.parent = null;
            line.gameObject.SetActive(false);
            //Debug.Log("TouchHandler OnDrag---------------1");
            return;
        }

        //起始与终点, 暂时先统一使用NGUI坐标
        Vector3 beginPos    = currentNode.GetPosition();
        Vector3 endPos      = currentSelect == null ? Screen2NGUI(pos) : currentSelect.GetPosition();
        if (currentSelect != null && !currentSelect.CanBeTarget())
        {
            Debug.Log("TouchHandler OnDrag---------------2");
            return;
        }

        if (currentSelect != null && currentSelect.nodeIsHide &&
            currentSelect.currentTeam.team != BattleSystem.Instance.battleData.currentTeam)
        {
            Debug.Log("TouchHandler OnDrag---------------3");
            return;
        }

        //确定是否还在拖拽范围中
        //是－>不显示线
        float dist = Vector3.Distance(beginPos, endPos);
        float noderange = currentNode.GetDist();
        if (dist < noderange )
        {
			line.gameObject.SetActive (false);
            line.transform.parent = null;
            return;
		}


        //显示线
        float nodewidth     = 5f;
        beginPos            = Vector3.MoveTowards(beginPos, endPos, nodewidth);
        endPos              = Vector3.MoveTowards( endPos, beginPos, nodewidth);
        line.positionCount  = 2;
        line.SetPosition(0, beginPos);
        line.SetPosition(1, endPos);
        line.startWidth     = 1f;
        line.endWidth       = 1f;

        //float distance      = Vector3.Distance(beginPos, endPos);
        //line.sharedMaterial.mainTextureScale    = new Vector2(distance / 8, 1);
        //line.sharedMaterial.mainTextureOffset   -= new Vector2(Time.deltaTime * 3, 0);

        //确定是否是冰冻状态
        if (currentNode.GetAttribute(NodeAttr.Ice) > 0)
		{
			//设置颜色
			SetWarning (1);
		}
		else 
		{
			//确定是否是传送门
			if (currentNode.CanWarp ()) {
				//设置颜色
				SetWarning (0);
			} else if (currentNode.team != BattleSystem.Instance.battleData.currentTeam
				&& currentNode.GetShipCount ((int)BattleSystem.Instance.battleData.currentTeam) == 0) {
				SetWarning (1);
			}
			else
			{
				//确定是否经过阻挡线
				//设置颜色
                bool bInterCircle  = BattleSystem.Instance.sceneManager.IsFixedPortal(beginPos, endPos);
                bool bIntersection = BattleSystem.Instance.sceneManager.GetIntersection(beginPos, endPos);
                if( bIntersection )
                {
                    SetWarning(1);
                }
                else
                {
                    SetWarning(0);
                }

                if (bInterCircle)
                  SetWarning( 2);
			}
		}

        line.transform.parent = BuildTypeManager.Instance.SceneRoot;
		line.gameObject.SetActive (true);
	}

	/// <summary>
	/// 进入物体
	/// </summary>
	void OnDragOver(GameObject go)
	{
        //Debug.Log("TouchHandler OnDragOver---------------");
        if (mNode == null || currentNode == null )
			return;

		if (currentNode == mNode)
			return;

        if (mNode != null && mNode.nodeIsHide && 
            mNode.currentTeam.team != BattleSystem.Instance.battleData.currentTeam)
            return;

        if (currentNode != null && currentNode.nodeIsHide && 
            currentNode.currentTeam.team != BattleSystem.Instance.battleData.currentTeam)
            return;

        currentSelect   = mNode;
        endHalo.transform.parent                        = currentSelect.GetGO().transform;
        endHalo.gameObject.transform.localPosition      = Vector3.zero;
        endHalo.gameObject.transform.localEulerAngles   = new Vector3(90, 90, 0);
        endHalo.gameObject.transform.localScale         = new Vector3(12, 12, 0);
		endHalo.gameObject.SetActive (true);
        endHalo.enabled = true;
        Color color     = mNode.currentTeam.color;
        color.a         = 1.0f;
        endHalo.color   = color;

    }

	/// <summary>
	/// 退出物体
	/// </summary>
	void OnDragOut(GameObject go)
	{
        //Debug.Log("TouchHandler OnDragOut---------------");
        if (mNode == null)
			return;

        if (endHalo != null && IsPressGuid )
        {
            endHalo.gameObject.SetActive(false);
        }

		mNode.ShowRange (false);
		if(currentSelect != null)
			currentSelect.ShowRange (false);

        //if(!IsPressGuid )
        currentSelect = null;
        //Debug.Log("TouchHandler OnDragOut---------------");
    }


    private Vector3 Screen2NGUI(Vector2 pos)
    {
        //Ray ray = Camera.main.ScreenPointToRay( new Vector3( pos.x, pos.y, 0 ) );
        //RaycastHit hit;
        //bool IsHit = Physics.Raycast(ray, out hit, 1000f );
        //if( IsHit )
        //{
        //    return hit.point;
        //}
        Vector3 Temp          = Camera.main.ScreenToWorldPoint(new Vector3(pos.x, pos.y, 85f));
        //Temp.z = 0;
        //Temp                  = BuildTypeManager.Instance.SceneRoot.worldToLocalMatrix.MultiplyPoint(Temp);
        //Temp                  = transform.parent.worldToLocalMatrix.MultiplyPoint(Temp);

        //Debug.LogError("His is " + Temp.ToString() );
        return Temp;
    }

    /// <summary>
    /// beginHalo 和 endHalo 淡出，但是缩放方向相反
    /// </summary>
    private void OnFadeOutHalo()
    {
        if (beginHalo != null && beginHalo.gameObject.activeSelf)
        {
            
            TweenAlpha al = beginHalo.gameObject.GetComponent<TweenAlpha>();
            if (al == null)
            {
                al = beginHalo.gameObject.AddComponent<TweenAlpha>();
            }

            al.ResetToBeginning();
            al.from = 1.0f;
            al.to = 0;
            al.duration = 0.2f;
            al.SetOnFinished(() =>
            {
                beginHalo.transform.parent = null;
                beginHalo.enabled = false;
            });
            al.Play(true);
        }


        if (endHalo != null && endHalo.gameObject.activeSelf)
        {
            TweenAlpha al = endHalo.gameObject.GetComponent<TweenAlpha>();
            if (al == null)
            {
                al = endHalo.gameObject.AddComponent<TweenAlpha>();
            }

            al.ResetToBeginning();
            al.from = 1.0f;
            al.to = 0;
            al.duration = 0.2f;
            al.SetOnFinished(() =>
            {
                endHalo.transform.parent = null;
                endHalo.enabled = false;
            });
            al.Play(true);
        }
    }
}


