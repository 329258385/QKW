using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;






/// <summary>
/// Helper class to display the HealthBar and FloatingText Canvas UI in WorldSpace
/// </summary>
public class WorldCanvasController : MonoSingleton<WorldCanvasController>
{
    public GameObject           uiWorld;
    public GameObject           floatingTextPrefab;
    public GameObject           healthBarPrefab;

    [HideInInspector]
    public AICombatTextData     m_AICombatTextData;

    void Start()
    {
        uiWorld.SetActive(false);
        m_AICombatTextData = (AICombatTextData)Resources.Load("combattextdata") as AICombatTextData;
    }

    /// <summary>
    /// For Creating a new FloatingText
    /// </summary>
    /// <param name="position"></param>
    /// <param name="v"></param>
    public void AddDamageText(BattleMember monster, float v)
    {
        CreateCombatText((int)v, monster.GetPosition(), false, false, false);
    }


    /// <summary>
    /// For Creating a new FloatingText
    /// </summary>
    /// <param name="position"></param>
    /// <param name="v"></param>
    public void AddDamageText(Vector3 pos, float v)
    {
        CreateCombatText((int)v, pos, false, false, true);
    }


    /// <summary>
    /// For Creating a new HealthBar
    /// </summary>
    /// <param name="position"></param>
    /// <param name="v"></param>
    public void AddHealthBar(BattleMember monster)
    {
        GameObject go = Instantiate(healthBarPrefab);
        go.transform.SetParent(gameObject.transform);
    }

    public void CreateCombatText(int amount, Vector3 TextPosition, bool CriticalHit, bool HealingText, bool PlayerTakingDamage)
    {
        GameObject go = Instantiate(floatingTextPrefab);
        go.transform.SetParent(transform);
        go.transform.position = Vector3.zero;
        Text m_Text = go.GetComponent<Text>();
        m_Text.text = amount.ToString();
        m_Text.fontSize = m_AICombatTextData.FontSize;

        Outline m_OutLine = go.GetComponent<Outline>();
        if (m_AICombatTextData.OutlineEffect == AICombatTextData.OutlineEffectEnum.Enabled)
        {
            m_OutLine.enabled = true;
        }
        else if (m_AICombatTextData.OutlineEffect == AICombatTextData.OutlineEffectEnum.Disabled)
        {
            m_OutLine.enabled = false;
        }

        StartCoroutine(AnimateOutwardsText(m_Text, m_AICombatTextData.PlayerTextColor, m_AICombatTextData.PlayerCritTextColor, TextPosition, CriticalHit, HealingText, PlayerTakingDamage));
    }


    /// <summary>
    /// 冒血动画
    /// </summary>
    IEnumerator AnimateOutwardsText(Text m_Text, Color RegularTextColor, Color CritTextColor, Vector3 TargetPosition, bool CriticalHit, bool HealingText, bool PlayerTakingDamage)
    {

        AICombatTextData combatText = WorldCanvasController.Instance.m_AICombatTextData;
        if (CriticalHit)
        {
            m_Text.color = CritTextColor;
        }
        else if (!CriticalHit)
        {
            m_Text.color = RegularTextColor;
        }

        if (HealingText)
        {
            m_Text.color = combatText.HealingTextColor;
            m_Text.text = "+" + m_Text.text;
        }

        if (PlayerTakingDamage)
        {
            m_Text.color = combatText.PlayerTakeDamageTextColor;
            m_Text.text = "-" + m_Text.text;

            if (CriticalHit)
            {
                m_Text.color = CritTextColor;
            }
        }

        float t = 0;
        float m_TextFade = 0;
        float RandomXPosition = Random.Range(-1f, 1.1f);
        RandomXPosition = Mathf.Round(RandomXPosition * 10f) / 10;
        Vector3 m_TextPos = Vector3.zero;
        float r = 1.0f;
        float AnimateSmaller = 0;
        float AnimateLarger = 0;

        while ((t / 1) < 1)
        {
            t += Time.deltaTime;
            if (r > 0.5f)
            {
                r = 1.0f - (t * 4 / 1);
            }

            m_TextPos = TargetPosition + new Vector3(Mathf.Lerp(0, RandomXPosition, t), r * 2 + Mathf.Sin(t * Mathf.PI), 0);
            m_Text.transform.position = m_TextPos;

            if (combatText.UseAnimateFontSize == AICombatTextData.UseAnimateFontSizeEnum.Enabled)
            {
                if (t <= 0.15f)
                {
                    AnimateLarger += Time.deltaTime * 8;
                    m_Text.fontSize = (int)Mathf.Lerp((float)combatText.FontSize, (float)combatText.FontSize + combatText.MaxFontSize, AnimateLarger);
                }
                else if (t > 0.15f && t <= 0.3f)
                {
                    AnimateSmaller += Time.deltaTime * 6;
                    m_Text.fontSize = (int)Mathf.Lerp((float)combatText.FontSize + combatText.MaxFontSize, (float)combatText.FontSize, AnimateSmaller);
                }
            }

            if (t > 0.5f)
            {
                m_TextFade += Time.deltaTime;
                m_Text.color = new Color(m_Text.color.r, m_Text.color.g, m_Text.color.b, 1 - (m_TextFade * 2));
            }
            m_Text.transform.rotation = Camera.main.transform.rotation;
            yield return null;
        }

        GameObject.DestroyImmediate(m_Text.gameObject);
    }
}