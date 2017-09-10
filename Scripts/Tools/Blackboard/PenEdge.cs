using UnityEngine;
using System.Collections;

public class PenEdge : MonoBehaviour
{
    private Material m_penColorMaterial;

    [SerializeField] private int m_width = 3;
    [SerializeField] private Transform m_corkPosition;

    //private VivePickable m_penCork;
    private bool m_hasCork;

    private void Awake()
    {
        m_penColorMaterial = GetComponentInParent<Renderer>().materials[0];
    }

    private void OnTriggerEnter(Collider other)
    {
        //if (m_penCork)
        //{
        //    return;
        //}

        //VivePickable penCork = other.GetComponent<VivePickable>();

        //if (penCork && (penCork.itemType == XperItem.PenCork))
        //{
        //    m_penCork = penCork;
        //    StartCoroutine(PutCorkCoroutine());
        //}

        if (!m_corkPosition) return;

        ViveCork viveCork = other.GetComponent<ViveCork>();

        if (viveCork && !m_hasCork)
        {
            viveCork.AttachTo(transform, m_corkPosition.localPosition);
            m_hasCork = true;
            //PlayPopSound(m_openingSound);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        //if (m_penCork && (other.gameObject == m_penCork.gameObject))
        //{
        //    StopCoroutine(PutCorkCoroutine());

        //    m_penCork.transform.SetParent(null);
        //    m_penCork.rigidbody.isKinematic = false;
        //    m_penCork.rigidbody.useGravity = true;
        //    m_penCork.GetComponent<Collider>().isTrigger = false;

        //    m_penCork = null;
        //}

        if (!m_corkPosition) return;

        ViveCork viveCork = other.GetComponent<ViveCork>();

        if (viveCork)
        {
            if (!viveCork.picked) viveCork.Release();
            else viveCork.CancelAttach();
            m_hasCork = false;
            //PlayPopSound(m_openingSound);
        }
    }

    public Color32 color
    {
        get { return m_penColorMaterial.color; }
    }

    public int width
    {
        get { return m_width; }
    }

    public bool hasCork
    {
        get { return m_hasCork; }
    }
}