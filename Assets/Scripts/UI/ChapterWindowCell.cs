using System;
using UnityEngine;
using Solarmax;

public class ChapterWindowCell : MonoBehaviour
{
	public UIWidget widget;
	public UIEventListener eventListener;
	public UILabel chapterName;
	public UILabel starNum;
	public UILabel desc;
	public UILabel clearLevel;
	public UILabel lockLabel;
	public UISprite lockPic;

	private ChapterInfo data;

	public void SetInfo (ChapterInfo info)
	{
		data = info;

		ChapterConfig config = ChapterConfigProvider.Instance.GetData (info.id);
		if (config == null) 
		{
			Debug.LogError ("error read config " + info.id);
			return;
		}

		chapterName.text = config.name;
		desc.text = config.describe;

		if (info.unLock) {
			widget.alpha = 1f;
			starNum.text = info.star.ToString ();
			clearLevel.text = string.Format ("{0}/{1}", info.achieveLevels, info.GetLevelCount ());
			starNum.transform.parent.gameObject.SetActive (true);
			clearLevel.gameObject.SetActive (true);
			lockPic.gameObject.SetActive (false);
			lockLabel.gameObject.SetActive (false);
		} else {
			widget.alpha = 0.7f;
			lockLabel.text = DictionaryDataProvider.Format (1001, config.needStar);
			starNum.transform.parent.gameObject.SetActive (false);
			clearLevel.gameObject.SetActive (false);
			lockPic.gameObject.SetActive (true);
			lockLabel.gameObject.SetActive (true);
		}

		eventListener.onClick = OnClickItem;
	}

	private void OnClickItem (GameObject go)
	{
		Debug.Log ("Click chapter :" + data.id);

		if (LevelDataHandler.Instance.BeUnlockChapter (data.id)) {
			LevelDataHandler.Instance.SetSelectChapter (data.id);
		}
		else {
			Tips.Make ("章节未开启");
		}
	}
}

