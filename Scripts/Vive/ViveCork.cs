using UnityEngine;
using System.Collections;

public class ViveCork : VivePickable
{
    private bool m_released;
    private bool m_attaching = false;
    private Collider[] m_colliders;

    new protected void Awake()
    {
        base.Awake();
        m_colliders = GetComponents<Collider>();
    }

    protected override void OnActionDown()
    {
        if (!picked)
        {
            VivePickable itemToGrip = this;

            if (!m_released && transform.parent && transform.parent.parent)
            {
                VivePickable corkParent = transform.parent.parent.GetComponent<VivePickable>();
                if (corkParent && !corkParent.picked)
                {
                    itemToGrip = corkParent;
                }
            }

            itemToGrip.Grip(m_interactionWand);
            ReportInteraction();
        }
    }

    protected override void OnActionUp()
    {
        if (picked)
        {
            if(m_attaching)
            {
                m_picked = false;
            }
            else
            {
                Release();
            }
        }
        else
        {
            if (transform.parent && transform.parent.parent)
            {
                VivePickable corkParent = transform.parent.parent.GetComponent<VivePickable>();
                if (corkParent && corkParent.picked)
                {
                    corkParent.Release();
                }
            }
        }
    }

    override public void Release()
    {
        CancelAttach();

        base.Release();

        m_rigidbody.isKinematic = false;
        m_rigidbody.useGravity = true;
        
        foreach(Collider collider in m_colliders)
        {
            collider.isTrigger = false;
        }

        m_released = true;
        m_attaching = false;
    }

    public void CancelAttach()
    {
        m_attaching = false;
        StopAllCoroutines();
    }

    public void AttachTo(Transform item, Vector3 localAttachPoint)
    {
        m_attaching = true;
        StartCoroutine(AttachToCoroutine(item, localAttachPoint));
    }

    private IEnumerator AttachToCoroutine(Transform item, Vector3 localAttachPoint)
    {
        #region WaitForRelease
        while (m_picked)
        {
            yield return null;
        }
        #endregion
        #region Phys update
        m_rigidbody.isKinematic = true;
        m_rigidbody.useGravity = false;

        foreach (Collider collider in m_colliders)
        {
            collider.isTrigger = true;
        }

        m_released = false;
        transform.SetParent(item);
        #endregion
        #region Timed move [OFF]
        //float t = 0;
        //float timeToPut = 0.2f;

        //Quaternion baseRotation = transform.localRotation;
        //Vector3 basePosition = transform.localPosition;

        //while (t < timeToPut)
        //{
        //    transform.localRotation = Quaternion.Slerp(baseRotation, Quaternion.identity, t / timeToPut);
        //    transform.localPosition = Vector3.Lerp(basePosition, localAttachPoint, t / timeToPut);

        //    t += Time.deltaTime;
        //    yield return new WaitForEndOfFrame();
        //}

        #endregion

        transform.localPosition = localAttachPoint;
        transform.localRotation = Quaternion.identity;
        m_attaching = false;
    }

    public bool released
    {
        get { return m_released; }
    }
}
