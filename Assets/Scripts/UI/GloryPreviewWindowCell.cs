using System;
using UnityEngine;
using Solarmax;






public class GloryPreviewWindowCell : MonoBehaviour
{

    public UILabel          gloryName;
    public UILabel          gloryLevel;
	private LadderConfig    data;




	public void SetInfo (LadderConfig config)
	{	
		data			= config;
        gloryLevel.text = string.Format ("{0}阶竞技场", data.ladderlevel);
        gloryName.text  = data.laddername;
        
    }

    public void OnItemClick(GameObject go)
	{
		string[] mark = go.transform.parent.name.Split ('-');
		int itemId = int.Parse (mark [1]);

		Debug.Log ("click on item : " + itemId);
	}

	public void OnRewardButtonClick()
	{

		Debug.Log ("click on reward button");
	}
}
