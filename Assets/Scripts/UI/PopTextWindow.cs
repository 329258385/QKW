using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Plugin;

public enum POP_TYPE
{
	POP_NORMAL,
	POP_UP,
	POP_DOWN,
}

public class PopItem
{
	public POP_TYPE type;
	public float poptime;
	public string poptext;
	public GameObject popobj;
}

public class PopTextWindow : BaseWindow
{
	public GameObject popTemp;
	public GameObject popCenter;
	public UILabel popCenterText;
	
	public List<PopItem> popMap = new List<PopItem>();
	Queue<GameObject> objList = new Queue<GameObject>();
	float centerTipTime;

	public override bool Init ()
	{
		RegisterEvent (EventId.OnSkillPop);

		return true;
	}

	public override void OnShow ()
	{
		for (int i = 0; i < 20; i++) 
		{
			GameObject obj = GameObject.Instantiate (popTemp);
			obj.transform.parent = popTemp.transform.parent;
			obj.transform.localScale = Vector3.one;
			objList.Enqueue (obj);
		}
	}

	public override void OnHide()
	{
		
	}
	
	// Update is called once per frame
	void Update ()
	{
		for (int i = 0; i < popMap.Count;) 
		{
			PopItem item = popMap [i];
			item.poptime -= Time.deltaTime;
			if (item.poptime < 0) 
			{
				item.popobj.SetActive (false);
				objList.Enqueue (item.popobj);
				popMap.Remove (item);
			} 
			else
			{
				switch (item.type) 
				{
				case POP_TYPE.POP_UP:
					{
						Vector3 pos = item.popobj.transform.localPosition;
						pos.y += Time.deltaTime * 50f;
						item.popobj.transform.localPosition = pos;
						break;
					}
				case POP_TYPE.POP_DOWN:
					{
						Vector3 pos = item.popobj.transform.localPosition;
						pos.y -= Time.deltaTime * 50f;
						item.popobj.transform.localPosition = pos;
						break;
					}
				}

				i++;
			}
		}

		if (centerTipTime > 0) 
		{
			centerTipTime -= Time.deltaTime;
			Vector3 pos = popCenter.transform.localPosition;
			pos.y += Time.deltaTime * 50f;
			popCenter.transform.localPosition = pos;
			if (centerTipTime < 0)
				popCenter.SetActive (false);
		}
	}

	public override void OnUIEventHandler (EventId eventId, params object[] args)
	{
		if (eventId == EventId.OnSkillPop) 
		{
			if (BattleSystem.Instance.battleData.silent)
				return;

			POP_TYPE type = (POP_TYPE)args [0];
			string text = (string)args [1];
			Vector3 pos = Vector3.zero;
			if (type != POP_TYPE.POP_NORMAL)
			{
				pos = (Vector3)args [2];

				PopItem item = new PopItem ();
				item.poptext = text;
				item.poptime = 2f;
				item.type = type;
				if (objList.Count > 0)
					item.popobj = objList.Dequeue ();
				else
					item.popobj = GameObject.Instantiate (popTemp);
				item.popobj.transform.parent = popTemp.transform.parent;
				item.popobj.transform.localScale = Vector3.one;
				Vector3 sceenpos = Camera.main.WorldToScreenPoint (pos);
				Vector3 finalpos = UICamera.mainCamera.ScreenToWorldPoint (sceenpos);
				finalpos.z = 0;
				//Debug.Log (string.Format ("Node pos:{0},UI pos:{1}",pos ,finalpos));
				item.popobj.transform.position = finalpos;
				item.popobj.SetActive (true);
				UILabel label = item.popobj.GetComponentInChildren<UILabel> ();
				label.text = item.poptext;
				popMap.Add (item);
			}
			else
			{
				popCenter.SetActive(true);
				popCenterText.text = text;
				centerTipTime = 2f;
				popCenter.transform.localPosition = new Vector3(0,362f,0);
			}
		}
	}
}

