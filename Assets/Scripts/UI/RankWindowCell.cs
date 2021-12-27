using UnityEngine;
using System.Collections;

public class RankWindowCell : MonoBehaviour {

	public UISprite bg;
	public UILabel id;
	public UISprite idBg;
    public UISprite icon;
	public UILabel nameLabel;
	public UILabel score;
	public UISprite line;

	private Color top3Color = new Color (1, 1, 1, 1);
	private Color otherColor = new Color (1, 1, 1, 0.50f);

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	  
	}

	public void SetCell(int rankid, string rankname, int rankscore, string sprite )
	{
		id.text = rankid.ToString ();
		nameLabel.text = rankname;
		score.text = rankscore.ToString ();

		if (rankid <= 3) {
			// 前三名
			id.color = top3Color;
			idBg.gameObject.SetActive (true);
			nameLabel.color = top3Color;
			score.color = top3Color;
		} else {
			// 后续排名
			id.color = otherColor;
			idBg.gameObject.SetActive (false);
			nameLabel.color = otherColor;
			score.color = otherColor;
		}

        if (!string.IsNullOrEmpty(sprite))
        {
            icon.spriteName = sprite;
        }
		line.gameObject.SetActive (true);

		bg.gameObject.SetActive (rankid % 2 == 1);
	}

	/*
	public void SetSelf(int rankid, string rankname, int rankscore)
	{
		id.text = rankid.ToString ();
		nameLabel.text = rankname;
		score.text = rankscore.ToString ();

		id.color = new Color (1, 1, 1, 0.3f);
		idBg.gameObject.SetActive (false);
		nameLabel.color = Color.white;
		score.color = Color.white;
		line.gameObject.SetActive (false);
	}*/
}
