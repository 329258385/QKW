using UnityEngine;
using System.Collections;

public class NodeEvent : MonoBehaviour {

	/// <summary>
	/// 当前星球
	/// </summary>
	public Node node;

	/// <summary>
	/// 地图编辑主逻辑类
	/// </summary>
	public MapEdit mapEdit;

	// Use this for initialization
	void Start () {
		mapEdit = gameObject.GetComponentInParent<MapEdit> ();
	}

	float clicktime;
	void OnMouseDown()
	{
		OnPress (true);
	}

	void OnMouseUp()
	{
	}

	void OnMouseDrag()
	{
		OnDrag (Vector2.zero);
	}

	void OnPress(bool isPressed)
	{
        Debug.Log("OnPress");
		if (isPressed && enabled) {
			mapEdit.menuRoot.SetActive (true);
			mapEdit.nodeAttribute.SetNode (node);
			mapEdit.UpdateMenu (node.GetPosition());
			clicktime = 0f;
		} 
	}

	void OnDrag(Vector2 delta)
	{
		if (!enabled)
			return;

		clicktime += Time.deltaTime;
		if (clicktime < 0.5f)
			return;
		node.SetPosition (Camera.main.ScreenToWorldPoint (Input.mousePosition));
		mapEdit.nodeAttribute.UpdatePosition (node);
		mapEdit.UpdateMenu (node.GetPosition());
	}

}
