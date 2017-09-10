using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class MagneticAgitator : MonoBehaviour
{
    [Header("Elements")]
    [SerializeField] private Transform m_center;
    [SerializeField] private GameObject m_bidule;
    [SerializeField] private Potentiometer m_speedButton;

    [Header("Parameters")]
    [SerializeField] private float m_maxSpeed;

    private Rigidbody m_biduleRigidbody;
    private CapsuleCollider m_biduleCollider;
    private bool m_biduleIsClose = false;

    private float m_biduleMovementFactor = 0.1f;
    private List<HeatableObject> m_agitatedObjects;

	private void Awake ()
	{
        m_biduleRigidbody = m_bidule.GetComponent<Rigidbody>();
        m_biduleCollider = m_bidule.GetComponent<CapsuleCollider>();
        m_agitatedObjects = new List<HeatableObject>();
	}
	
	private void Update ()
	{
	    if(!m_biduleIsClose)
        {
            m_biduleMovementFactor = 0.1f;
            return;
        }


        foreach (HeatableObject agitatedObject in m_agitatedObjects)
        {
            agitatedObject.agitation = m_speedButton.value;
        }

        if(m_speedButton.value == 0)
        {
            m_biduleMovementFactor = 0.1f;
            return;
        }

        m_biduleRigidbody.useGravity = false;
        m_biduleRigidbody.isKinematic = true;
        m_biduleCollider.isTrigger = true;

        float speed = m_speedButton.value * m_maxSpeed;

        m_biduleMovementFactor = m_biduleMovementFactor * (1 + m_biduleMovementFactor * m_speedButton.value);
        m_biduleMovementFactor = Mathf.Min(1, m_biduleMovementFactor);

        Vector3 positionDeltaToCenter = m_center.transform.position - m_bidule.transform.position;
        if (positionDeltaToCenter.magnitude < 0.001f)
        {
            m_biduleRigidbody.transform.position = m_center.transform.position;
            m_biduleRigidbody.transform.Rotate(new Vector3(0, 360f * Time.deltaTime * speed, 0), Space.World);
        }
        else
        {
            m_biduleRigidbody.transform.position = Vector3.Lerp(m_bidule.transform.position, m_center.transform.position, m_biduleMovementFactor);
        }
	}

    private void OnTriggerEnter(Collider other)
    {
        if(other.gameObject == m_bidule)
        {
            m_biduleIsClose = true;
            return;
        }

        HeatableObject agitatedObject = other.gameObject.GetComponent<HeatableObject>();
        if(agitatedObject != null)
        {
            m_agitatedObjects.Add(agitatedObject);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject == m_bidule)
        {
            m_biduleIsClose = false;
            m_biduleRigidbody.useGravity = true;
            m_biduleRigidbody.isKinematic = false;
            m_biduleCollider.isTrigger = false;
            return;
        }

        HeatableObject agitatedObject = other.gameObject.GetComponent<HeatableObject>();
        if (agitatedObject != null)
        {
            agitatedObject.agitation = 0;
            m_agitatedObjects.Remove(agitatedObject);
        }
    }
}
