using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine.UI;

public class HUDComponent : MonoBehaviour
{
	/// <summary>
	/// 进度条占领队伍相关信息
	/// </summary>
    Node                    hostNode = null;
	List<Color>             m_clr = new List<Color>();
    List<float>             m_p   = new List<float>();


    private GameObject      mHudRoot;
    public GameObject       TXT;
    public Image            image;
    public Image            back;

    private List<Vector3>   mHudPoses;
    private List<Text>      mHudLabel;
    private Color           enemyColor      = new Color32(0xff, 0, 0, 0xff);
    private Color           TempTeamColor   = new Color32(0x99, 0x99, 0x99, 0x00);


    public HUDComponent()
    {
        mHudLabel           = new List<Text>();
        mHudPoses           = new List<Vector3>();
    }

    public void SetNode( Node node )
    {
        hostNode                         = node;
        mHudRoot                         = new GameObject();
        mHudRoot.name                    = "HudRoot";
        mHudRoot.transform.parent        = transform;
        mHudRoot.transform.localPosition = Vector3.zero;
        mHudRoot.transform.localScale    = Vector3.one;
    }


    public void DisPlayLable()
    {
        for (int i = 0; i < mHudLabel.Count; ++i)
        {
            mHudLabel[i].gameObject.SetActive(false);
        }
    }


    public void ShowPopulationProcess(List<Team> teamArray, List<float> HPArray, float hpMax = 100f)
    {
        if( teamArray.Count != HPArray.Count )
        {
            return;
        }
        if (1 == teamArray.Count) 
		{
			// 未开发星球
			Color color = teamArray [0].color;
			color.a     = 0x33 / 255.0f;
			m_clr.Add (color);
			color.a     = 1.0f; 
			m_clr.Add (color);


			m_p.Add (1f);
			m_p.Add (HPArray [0] / hpMax);

			SetColor (m_clr, m_p);
			if (HPArray [0] >= 1000.0f) 
			{
				ShutHalo ();
			} 
		} 
		else if (2 <= teamArray.Count ) 
		{
			float HP = 0.0f;
			// 对战
			for (int i = 0; i < HPArray.Count; ++i) 
			{
				HP += HPArray [i];
			}

			// 战斗过程中把所有飞船移走
			if (0 == HP) 
			{
				ShutHalo ();
			}

			for (int i = 0; i < HPArray.Count; ++i) 
			{
				if (HPArray [i] > 0) 
				{
					Color color = teamArray [i].color;
                    if (teamArray[i].team == TEAM.Neutral)
                        color   = TempTeamColor;
					
                    color.a     = 1.0f;
					m_clr.Add (color);
					m_p.Add (HPArray [i] / HP);
					if (HPArray [i] / HP == 1) 
					{
						ShutHalo ();
					} 
				}
			}
			SetColor (m_clr, m_p);
		} 
		m_clr.Clear ();
		m_p.Clear ();
    }

	/// <summary>
	/// Shows the progress.
	/// </summary>
    
	public void ShowProgress(Team[] teamArray, float[] HPArray, float hpMax = 100f )
	{
		if(teamArray.Length != HPArray.Length)
		{
			Debug.LogError ("teamArr != HPArr");
			return;
		}

        if (1 == teamArray.Length) 
		{
			// 未开发星球
			Color color = teamArray [0].color;
			color.a = 0x33 / 255.0f;
			m_clr.Add (color);
			color.a = 1.0f; 
			m_clr.Add (color);


			m_p.Add (1f);
			m_p.Add (HPArray [0] / hpMax);

			SetColor (m_clr, m_p);

			if (HPArray [0] >= 1000.0f) 
			{
				ShutHalo ();
			} 
		} 
		else if (2 <= teamArray.Length) 
		{
			float HP = 0.0f;
			// 对战
			for (int i = 0; i < HPArray.Length; ++i) 
			{
				HP += HPArray [i];
			}

			// 战斗过程中把所有飞船移走
			if (0 == HP) 
			{
				ShutHalo ();
			}

			for (int i = 0; i < HPArray.Length; ++i) 
			{
				if (HPArray [i] > 0) 
				{
					Color color = teamArray [i].color;
                    if (teamArray[i].team == TEAM.Neutral)
                        color   = TempTeamColor;
					
                    color.a     = 1.0f;
					m_clr.Add (color);
					m_p.Add (HPArray [i] / HP);
					if (HPArray [i] / HP == 1) 
					{
						ShutHalo ();
					} 
				}
			}
			SetColor (m_clr, m_p);
		} 
		m_clr.Clear ();
		m_p.Clear ();
	}


    /// <summary>
    /// 关闭
    /// </summary>
    public void ShutHalo( )
    {
        image.color         = Color.white;
        back.color          = Color.white;
    }


    /// <summary>
    /// 画颜色圈 Color[] 和 float[] 必需长度相同
    /// </summary>
    /// <param name="clrArray">Clr array.</param>
    /// <param name="phaseArray">Phase array.</param>
    public void SetColor(List<Color> clrArray, List<float> phaseArray)
    {
        if (clrArray.Count != phaseArray.Count)
        {
            return;
        }

        switch (clrArray.Count)
        {
            case 2:
                {
                    image.fillAmount    = phaseArray[1];
                    image.fillClockwise = false;
                    back.color          = clrArray[0];
                    image.color         = clrArray[1];

                    back.fillClockwise  = true;
                    back.fillAmount     = phaseArray[0];
                }
                break;
            default:
                {
                    back.color           = Color.white;
                    image.color          = clrArray[0];
                    image.fillClockwise  = false;
                    image.fillAmount     = phaseArray[0];
                }
                break;
        }
    }


    private void CalculateHudPose()
    {
        if (Camera.main != null && Camera.main.gameObject != null)
        {
            if (transform.rotation != Camera.main.transform.rotation)
            {
                transform.rotation = Camera.main.transform.rotation;
            }

            //Vector3 pos             = hostNode.entity.go.transform.position;
            //Vector3 screenPoint     = Camera.main.WorldToScreenPoint(pos);

            //Vector2 localPoint;
            //RectTransform canvas    = UISystem.Get().mUIParent;
            //Camera uiCamera         = UISystem.Get().UICamera;
            //RectTransformUtility.ScreenPointToLocalPointInRectangle(canvas, screenPoint, uiCamera, out localPoint);
            //transform.localPosition = localPoint;
        }
    }

    public void ShowTeamLable(List<Team> teamArray, List<float> HPArray )
    {
        CalculateHudPose();
        bool needMapPosition = false;
        if (teamArray.Count != mHudLabel.Count )
        {
            CalculateHudPoses(teamArray.Count);
            needMapPosition = true;
        }


        GameObject go = null;
        for( int i = mHudLabel.Count; i < teamArray.Count; ++i )
        {
            go          = GameObject.Instantiate(TXT);
            mHudLabel.Add(go.GetComponent<Text>());
        }

        // TODO(hou):: 这里有空指针报错，限制一下
        for (int i = 0; i < mHudLabel.Count; ++i)
        {
            if (mHudLabel[i] == null)
            {
                continue;
            }
            mHudLabel[i].gameObject.SetActive(true);
            if (needMapPosition && i < teamArray.Count)
            {
                mHudLabel[i].transform.localPosition = mHudPoses[i];
            }
        }

        // show node team info
        Color c         = Color.blue;
        for (int i = 0; i < teamArray.Count; ++i)
        {
            Team t = teamArray[i];
            if (teamArray[i].team == TEAM.Neutral)
                c = TempTeamColor;

            c.a = 1.0f;
            if (/*t.hidePopution &&*/ t.team != BattleSystem.Instance.battleData.currentTeam)
            {
                mHudLabel[i].text = string.Empty;
            }
            else
            {
                StringBuilder build = new StringBuilder();
                build.Append(HPArray[i]);
                mHudLabel[i].text = build.ToString();
                build = null;
            }
            mHudLabel[i].color = c;

            if (HPArray[i] <= 0 && mHudLabel[i].gameObject.activeSelf )
            {
                mHudLabel[i].gameObject.SetActive(false);
            }
        }
    }


    private void CalculateHudPoses(int teamCount)
    {
        for (int i = mHudPoses.Count; i < teamCount; ++i)
        {
            mHudPoses.Add(Vector3.zero);
        }
        Vector3 pos = hostNode.GetPosition();
        if (teamCount == 1)
        {
            mHudPoses[0] = new Vector3(0, 90, 0);
        }
        else if (teamCount == 2)
        {
            mHudPoses[0] = new Vector3(0,  90, 0);
            mHudPoses[1] = new Vector3(0, -90, 0);
        }
        else
        {
            // 多边形
            float length        = 90;
            float anglePer      = Mathf.PI * 2 / teamCount;
            float angleStart    = Mathf.PI / 2;
            float angle         = 0;
            for (int i = 0; i < teamCount; ++i)
            {
                angle = angleStart + anglePer * i;
                mHudPoses[i] = new Vector3(Mathf.Cos(angle) * length, Mathf.Sin(angle) * length, 0);
            }
        }
    }

}
