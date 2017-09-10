using UnityEngine;
using System.Collections;

public class PenDistributerTrigger : MonoBehaviour
{
    private PenDistributor m_penDistributor;

	private void Awake ()
	{
        m_penDistributor = GetComponentInParent<PenDistributor>();
	}
	
    private void OnTriggerEnter(Collider other)
    {
        if(other.GetComponent<ViveWand>())
        {
            m_penDistributor.CreatePen();
        }
    }
}
