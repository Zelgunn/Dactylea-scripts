using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class EndSimulationUI : MonoBehaviour
{
    static private EndSimulationUI s_singleton;

    [Header("Cercles")]
    [SerializeField] private RawImage m_globalCircle;
    [SerializeField] private RawImage m_timeCircle;
    [SerializeField] private RawImage m_snakeCircle;
    [SerializeField] private RawImage m_wasteCircle;

    [Header("Notes")]
    [SerializeField] private Text m_globalRating;
    [SerializeField] private Text m_timeRating;
    [SerializeField] private Text m_snakeRating;
    [SerializeField] private Text m_wasteRating;

	private void Awake ()
	{
        s_singleton = this;
        Hide();
	}
	
    private void Show()
    {
        foreach(Transform t in transform)
        {
            t.gameObject.SetActive(true);
        }
    }

    private void Hide()
    {
        foreach (Transform t in transform)
        {
            t.gameObject.SetActive(false);
        }
    }

    private void _ShowRating(float time, float snakeSize, float waste)
    {
        float global = (time + snakeSize + waste) / 3;

        m_globalCircle.color = RatingToColor(global);
        m_timeCircle.color = RatingToColor(time);
        m_snakeCircle.color = RatingToColor(snakeSize);
        m_wasteCircle.color = RatingToColor(waste);

        global =    Mathf.Round(global      * 10) / 10;
        time =      Mathf.Round(time        * 10) / 10;
        snakeSize = Mathf.Round(snakeSize   * 10) / 10;
        waste =     Mathf.Round(waste       * 10) / 10;

        m_globalRating.text     = global.ToString();
        m_timeRating.text       = time.ToString();
        m_snakeRating.text      = snakeSize.ToString();
        m_wasteRating.text      = waste.ToString();



        Show();
    }

    private Color RatingToColor(float rating)
    {
        if(rating == 10)
        {
            return Color.cyan;
        }
        else if(rating > 6)
        {
            return Color.green;
        }
        else if(rating > 4)
        {
            return Color.yellow;
        }
        else if(rating > 2.5f)
        {
            return new Color(1, 0.5f, 0);
        }
        else
        {
            return Color.red;
        }
    }

    static public void ShowRatings(float time, float snakeSize, float waste)
    {
        s_singleton._ShowRating(time, snakeSize, waste);
    }
}
