using UnityEngine;
using System.Collections;

public class VivePickable : ViveInteractible
{
    protected Rigidbody m_rigidbody;
    protected bool m_picked = false;

    private Vector3 m_lastPosition;
    private Vector3 m_lastEulerAngle;
    private Vector3 m_velocity;
    private Vector3 m_eulerVelocity;

    protected void Awake ()
    {
        m_rigidbody = GetComponent<Rigidbody>();
        m_lastPosition = transform.position;
        m_lastEulerAngle = transform.eulerAngles;
    }

    new private void Update()
    {
        base.Update();

        m_velocity = (transform.position - m_lastPosition) / Time.deltaTime;
        m_lastPosition = transform.position;

        m_eulerVelocity = (transform.eulerAngles - m_lastEulerAngle) / Time.deltaTime;
        m_lastEulerAngle = transform.eulerAngles;

        //if(m_interactionInProgress)
        //{
        //    if(m_interactionWand.transform != transform.parent)
        //    {
        //        Grip(m_interactionWand);
        //    }
        //}
        //else if(picked)
        //{

        //}
    }

    #region Fonctions "Overrided"

    protected override void OnActionDown()
    {
        if(!picked)
        {
            VivePickable itemToGrip = this;

            if(m_itemType == XperItem.Erlenmeyer)
            {
                SmartFluwid smartFluwid = GetComponentInChildren<SmartFluwid>();
                FluwidContainerEntry fluwidContainerEntry = smartFluwid.GetComponentInChildren<FluwidContainerEntry>();

                if(fluwidContainerEntry.hasCork && fluwidContainerEntry.viveCork.picked)
                {
                    itemToGrip = fluwidContainerEntry.viveCork;
                }
            }

            itemToGrip.Grip(m_interactionWand);
            ReportInteraction();
        }
    }

    protected override void OnActionUp()
    {
        if(picked)
        {
            Release();
        }
        else
        {
            ViveCork cork = GetComponent<ViveCork>();

            if(cork && cork.transform.parent && cork.transform.parent.parent)
            {
                VivePickable corkParent = cork.transform.parent.parent.GetComponent<VivePickable>();
                if(corkParent && corkParent.picked)
                {
                    corkParent.Release();
                }
            }
        }
    }

    #endregion

    public void Grip(ViveWand wand)
    {
        transform.SetParent(wand.transform);

        m_rigidbody.useGravity = false;
        m_rigidbody.isKinematic = true;

        m_picked = true;
    }

    virtual public void Release()
    {
        transform.SetParent(null);

        m_rigidbody.useGravity = true;
        m_rigidbody.isKinematic = false;
        m_rigidbody.velocity = m_velocity;
        m_rigidbody.angularVelocity = m_eulerVelocity;

        m_picked = false;
    }

    public bool picked
    {
        get { return m_picked; }
    }

    public XperItem itemType
    {
        get { return m_itemType; }
    }

    new public Rigidbody rigidbody
    {
        get { return m_rigidbody; }
    }
}
