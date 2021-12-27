using UnityEngine;
using System.Collections;

public class MouseEvent : MonoBehaviour {

	/// <summary>
	/// 创建菜单对象
	/// </summary>
	public GameObject nodeMenu;

	/// <summary>
	/// 当前摄像机
	/// </summary>
	public Camera uiCamera;

	/// <summary>
	/// 战斗节点
	/// </summary>
	public GameObject battlteObject;

	/// <summary>
	/// The node attribute.
	/// </summary>
	public GameObject nodeAttribute;

	void LateUpdate()
	{
		if (battlteObject == null)
			return;
		if (!battlteObject.activeSelf)
			return;
		
		//鼠标右键
		if (Input.GetMouseButtonUp (1)) {

			Vector3 position = Input.mousePosition;
			position.z = 0;
			position = uiCamera.ScreenToWorldPoint (position);
			nodeMenu.transform.position = position;
			nodeMenu.SetActive (true);
		}
	}

	void OnClick ()
	{
		float time = Time.realtimeSinceStartup - nodeAttribute.GetComponent<NodeAttribute> ().clickTime;
		if (time < 0.1f)
			return;
		nodeAttribute.SetActive (false);
		nodeMenu.SetActive (false);
	}

	void OnMouseDown()
	{
	}
}
