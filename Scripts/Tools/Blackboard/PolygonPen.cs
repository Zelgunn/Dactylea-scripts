using UnityEngine;
using System.Collections;

[ExecuteInEditMode]
public class PolygonPen : MonoBehaviour
{
    [SerializeField] private Color32 m_color;
    [SerializeField] private SphereCollider[] m_points;
    private Transform[] m_pointsTransform;

	private void Awake ()
	{
        m_pointsTransform = new Transform[m_points.Length];

        for (int i = 0; i < m_points.Length; i++)
        {
            m_pointsTransform[i] = m_points[i].transform;
        }
	}
	
	private void Update ()
	{
        if(Application.isEditor)
        {
            for (int i = 0; i < m_points.Length; i++)
            {
                Debug.DrawLine(m_points[i].transform.position, m_points[(i + 1) % m_points.Length].transform.position);
            }
        }

	}

    public Transform[] points
    {
        get { return m_pointsTransform; }
    }

    public Color32 color
    {
        get { return m_color; }
    }
}
