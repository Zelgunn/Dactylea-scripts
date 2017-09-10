using UnityEngine;
using System.Collections;

public class SpriteAnimator : MonoBehaviour
{
    private Renderer m_renderer;
    private float m_xOffset, m_yOffset;
    [SerializeField] private int m_xCount = 8;
    [SerializeField] private int m_yCount = 4;
    private int m_xIndex = 0, m_yIndex = 1;

    private void Start()
    {
        m_renderer = GetComponent<Renderer>();

        m_xOffset = 1f / m_xCount;
        m_yOffset = 1f / m_yCount;

        m_renderer.material.mainTextureScale = new Vector2(m_xOffset, m_yOffset);
        m_renderer.material.mainTextureOffset = Vector2.zero;
    }

    private void FixedUpdate()
    {
        m_xIndex++;

        if(m_xIndex >= m_xCount)
        {
            m_xIndex = 0;
            m_yIndex = (m_yIndex + 1) % m_yCount;
        }

        m_renderer.material.mainTextureOffset = new Vector2(m_xIndex * m_xOffset, 1 - m_yIndex * m_yOffset);
    }
}
