using UnityEngine;
using System.Collections;

public class PenDistributor : MonoBehaviour
{
    [SerializeField] private Transform m_redHandle;
    [SerializeField] private Transform m_greenHandle;
    [SerializeField] private Transform m_blueHandle;

    [SerializeField] private Material m_penPreviewMaterial;
    [SerializeField] private GameObject m_pen;

    private Material m_redHandleMaterial;
    private Material m_greenHandleMaterial;
    private Material m_blueHandleMaterial;

	private void Awake ()
	{
        Renderer redRenderer = m_redHandle.GetComponent<Renderer>();
        Renderer greenRenderer = m_greenHandle.GetComponent<Renderer>();
        Renderer blueRenderer = m_blueHandle.GetComponent<Renderer>();

        m_redHandleMaterial = redRenderer.materials[1];
        m_blueHandleMaterial = blueRenderer.materials[1];
        m_greenHandleMaterial = greenRenderer.materials[1];

        redRenderer.materials[2].color = Color.red;
        greenRenderer.materials[2].color = Color.green;
        blueRenderer.materials[2].color = Color.blue;
    }
	
	private void Update ()
	{
        CheckHandleY(m_redHandle);
        CheckHandleY(m_greenHandle);
        CheckHandleY(m_blueHandle);

        float red = (m_redHandle.localPosition.y - 1.65f) / 0.8f;
        float green = (m_greenHandle.localPosition.y - 1.65f) / 0.8f;
        float blue = (m_blueHandle.localPosition.y - 1.65f) / 0.8f;

        m_redHandleMaterial.color = new Color(red, 0, 0);
        m_greenHandleMaterial.color = new Color(0, green, 0);
        m_blueHandleMaterial.color = new Color(0, 0, blue);

        m_penPreviewMaterial.color = new Color(red, green, blue);
	}

    private void CheckHandleY(Transform t)
    {
        Vector3 pos = t.localPosition;
        if (pos.y > 2.45f) pos.y = 2.45f;
        if (pos.y < 1.65f) pos.y = 1.65f;
        t.localPosition = pos;
    }

    public void CreatePen()
    {
        float red = (m_redHandle.localPosition.y - 1.65f) / 0.8f;
        float green = (m_greenHandle.localPosition.y - 1.65f) / 0.8f;
        float blue = (m_blueHandle.localPosition.y - 1.65f) / 0.8f;

        Color penColor = new Color(red, green, blue);

        GameObject pen = Instantiate(m_pen);
        pen.transform.position = m_pen.transform.position;
        pen.GetComponent<Renderer>().material.color = penColor;
        pen.SetActive(true);

        pen.GetComponentInChildren<ViveCork>().GetComponent<Renderer>().material.color = penColor;
    }
}
