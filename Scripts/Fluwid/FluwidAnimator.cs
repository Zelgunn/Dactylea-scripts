using UnityEngine;
using System.Collections;

public class FluwidAnimator : MonoBehaviour
{
    private Renderer m_renderer;
    [SerializeField] private float m_sinDeltaSpeed = 1;

	private void Start ()
    {
        m_renderer = GetComponent<Renderer>();
    }

    private void Update ()
    {
        float sinDelta = m_renderer.material.GetFloat("_CurveSinDelta");
        m_renderer.material.SetFloat("_CurveSinDelta", sinDelta + Time.deltaTime * m_sinDeltaSpeed);
    }
}
