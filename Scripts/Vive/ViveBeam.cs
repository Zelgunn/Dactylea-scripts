using UnityEngine;
using System.Collections;

public class ViveBeam : MonoBehaviour
{
    private ViveWand m_viveWand;

    [SerializeField] private MeshFilter m_outlinedPrefab;
    [Header("Beams")]
    [SerializeField] private LineRenderer m_leftBeam;
    [SerializeField] private LineRenderer m_rightBeam;
    [SerializeField] private Transform m_centralBeamTarget;

    private float m_offset = 0;
    private Vector3 m_leftTarget;
    private Vector3 m_rightTarget;
    private Vector3 m_centralTarget;

    private MeshFilter m_outlinedItem;
    private VivePickable m_targetVivePickable;

    private float m_holdingTime = 0;
    private float m_timeSinceSelection = 0;

	private void Awake ()
	{
        m_leftTarget = - m_leftBeam.transform.localPosition;
        m_rightTarget = - m_rightBeam.transform.localPosition;
        m_centralTarget = m_centralBeamTarget.transform.localPosition;

        m_viveWand = GetComponentInParent<ViveWand>();
    }
	
	private void Update ()
	{
        m_centralBeamTarget.localPosition = m_centralTarget * m_holdingTime * 5;

        float trigger = m_viveWand.controller.GetAxis(Valve.VR.EVRButtonId.k_EButton_SteamVR_Trigger).x;

        m_offset += Time.deltaTime * 2;
        if (m_offset >= 1) m_offset -= 1;

        m_rightBeam.material.SetTextureOffset("_MainTex", new Vector2(m_offset, 0));
        m_leftBeam.material.SetTextureOffset("_MainTex", new Vector2(m_offset, 0));

        bool hasHit = UpdateBeam(m_rightBeam, m_rightTarget);
        hasHit |= UpdateBeam(m_leftBeam, m_leftTarget);
        Vector3 deltaToTarget = m_centralBeamTarget.transform.position - m_centralBeamTarget.transform.parent.position;

        if (!hasHit)
        {
            if(trigger >= 1)
            {
                RaycastHit hit;
                Ray ray = new Ray(m_centralBeamTarget.transform.parent.position, deltaToTarget);

                if (Physics.Raycast(ray, out hit, deltaToTarget.magnitude, 1 << 8))
                {
                    m_rightBeam.SetPosition(2, m_rightBeam.transform.InverseTransformPoint(hit.point));
                    m_leftBeam.SetPosition(2, m_leftBeam.transform.InverseTransformPoint(hit.point));

                    SelectItem(hit.transform);
                }

                m_holdingTime += Time.deltaTime;
            }
            else
            {
                m_holdingTime = 0;
            }
        }

        if (trigger == 0)
        {
            RemoveSelection();
        }

        m_timeSinceSelection += Time.deltaTime;
        if (m_targetVivePickable)
        {
            Vector3 delta = m_rightBeam.transform.parent.position - m_targetVivePickable.transform.position;
            m_targetVivePickable.transform.position = m_targetVivePickable.transform.position + delta * Mathf.Min(m_timeSinceSelection / 10, 0.2f);
        }
	}

    private bool UpdateBeam(LineRenderer beam, Vector3 target)
    {
        float trigger = m_viveWand.controller.GetAxis(Valve.VR.EVRButtonId.k_EButton_SteamVR_Trigger).x;

        RaycastHit hit;
        Vector3 deltaToCenter = beam.transform.parent.position - beam.transform.position;
        Ray ray = new Ray(beam.transform.position, deltaToCenter * trigger);
        float lenght = trigger;
        bool hasHit = false;

        if (Physics.Raycast(ray, out hit, m_rightTarget.magnitude, 1 << 8))
        {
            lenght = (hit.point - m_rightBeam.transform.position).magnitude / deltaToCenter.magnitude * trigger;
            hasHit = true;
        }

        beam.SetPosition(1, Vector3.Lerp(Vector3.zero, target, lenght));

        if(hasHit || (trigger < 1))
        {
            beam.SetVertexCount(2);
        }
        else
        {
            beam.SetVertexCount(3);
            beam.SetPosition(2, beam.transform.InverseTransformPoint(m_centralBeamTarget.position));
        }

        return hasHit;
    }

    public void SelectItem(Transform item)
    {
        if (m_outlinedItem) return;

        MeshFilter meshFilter = item.gameObject.GetComponent<MeshFilter>();

        m_outlinedItem = Instantiate<MeshFilter>(m_outlinedPrefab);
        m_outlinedItem.mesh = meshFilter.mesh;
        m_outlinedItem.transform.SetParent(item, false);

        m_targetVivePickable = item.gameObject.GetComponent<VivePickable>();
        if(m_targetVivePickable)
        {
            m_targetVivePickable.Grip(m_viveWand);
        }

        m_timeSinceSelection = 0;
    }

    public void RemoveSelection()
    {
        if (!m_outlinedItem) return;

        if (m_targetVivePickable)
        {
            m_targetVivePickable.Release();
            m_targetVivePickable = null;
        }

        Destroy(m_outlinedItem.gameObject);
        m_outlinedItem = null;
    }
}
