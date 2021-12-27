using UnityEngine;
using System.Collections;

public class Active : MonoBehaviour {

	/// <summary>
	/// 是否显示
	/// </summary>
	/// <param name="go">Go.</param>
	public void SetActive(GameObject go)
	{
		if (go == null)
			return;
		gameObject.SetActive (!go.activeSelf);
	}
}
