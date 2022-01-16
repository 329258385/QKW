using System.Collections;
using UnityEngine;
using UnityEngine.UI;






public class FloatingText : MonoBehaviour
{
    private float           timer = 0f;

    /// <summary>
    /// How fast text moves
    /// </summary>
    public  float           speed = 3f;

    /// <summary>
    /// How long the text is visible
    /// </summary>
    public  float           fadeOutTime = 1f;

    /// <summary>
    /// 
    /// </summary>
    private BattleMember    mEntity = null;

    void Update()
    {
        timer += Time.deltaTime;
        float fade = (fadeOutTime - timer) / fadeOutTime;

        if (mEntity != null)
        {
            Vector3 local       = mEntity.GetPosition();
            transform.position  = local + Vector3.up * speed;
        }

        transform.rotation = Camera.main.transform.rotation;
        if (fade <= 0)
            Destroy(this.gameObject);
    }

    IEnumerator AnimateOutwardsText(Vector3 TargetPosition)
    {
        float t = 0;
        Vector3 m_TextPos = Vector3.zero;
        float RandomYPosition = Random.Range(0, 8f);
        RandomYPosition = Mathf.Round(RandomYPosition * 10f) / 10;
        float r = 1.0f;
        while ((t / 1) < 1)
        {
            t += Time.deltaTime;
            if (r > 0.5f)
            {
                r = 1.0f - (t * 4 / 1);
            }

            m_TextPos = TargetPosition + new Vector3(0, -r * 2 + Mathf.Sin(r + RandomYPosition * Mathf.PI) + 2.0f, 0);
            transform.position = m_TextPos;
            transform.rotation = Camera.main.transform.rotation;
            yield return null;
        }

        Destroy(this.gameObject);
    }

    /// <summary>
    /// Called when floating text is created
    /// </summary>
    public void Init(BattleMember monster, float v)
    {
        mEntity                 = monster;
        Text uiText             = this.GetComponent<Text>();
        uiText.color            = Color.red;
        uiText.text             = "-" + Mathf.Round(v).ToString();

        Vector3 startPosition   = Vector3.zero;
        if (mEntity != null)
        {
            startPosition = mEntity.GetPosition();
        }

        StartCoroutine(AnimateOutwardsText(startPosition));
    }
}

