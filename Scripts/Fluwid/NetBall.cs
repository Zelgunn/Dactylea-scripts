using UnityEngine;
using System.Collections;

public class NetBall : MonoBehaviour
{
    private float m_containedVolume = 0;
    private SmartFluwid m_creator;
    private Color m_color;

    private Compound m_containedCompound;

	private void OnTriggerEnter (Collider other)
	{
        FluwidContainerEntry fluwidContainerEntry = other.GetComponent<FluwidContainerEntry>();

        if(fluwidContainerEntry)
        {
            if(fluwidContainerEntry.hasCork)
            {
                Waste();
            }
            else if(fluwidContainerEntry.fluwidManager != m_creator)
            {
                OnTriggerAtHeight(fluwidContainerEntry);
            }
        }
        else if(other.gameObject.isStatic)
        {
            Waste();
        }

    }

    private void OnTriggerExit(Collider other)
    {
        FluwidContainerEntry fluwidContainerEntry = other.GetComponent<FluwidContainerEntry>();

        if (!fluwidContainerEntry) return;
        if (fluwidContainerEntry.fluwidManager == m_creator) return;
        

        StopCoroutine(OnTriggerAtHeightCoroutine(fluwidContainerEntry));
    }

    private void OnTriggerAtHeight(FluwidContainerEntry fluwidContainerEntry)
    {
        StartCoroutine(OnTriggerAtHeightCoroutine(fluwidContainerEntry));
    }

    private IEnumerator OnTriggerAtHeightCoroutine(FluwidContainerEntry fluwidContainerEntry)
    {
        while (transform.position.y > (fluwidContainerEntry.fluwidManager.fluidHeight + fluwidContainerEntry.fluwidManager.transform.position.y)) yield return null;

        fluwidContainerEntry.fluwidManager.AddCompound(m_containedCompound);
        Destroy(gameObject);
    }

    private void Waste()
    {
        Watcher.AddToWaste(m_containedCompound);
        Destroy(gameObject);
    }

    public Compound containedCompound
    {
        get { return m_containedCompound; }
        set { m_containedCompound = value; }
    }

    public float containedVolume
    {
        get { return m_containedVolume; }
        set { m_containedVolume = value; }
    }

    public SmartFluwid creator
    {
        get { return m_creator; }
        set { m_creator = value; }
    }

    //public Color color
    //{
    //    get { return m_color; }
    //    set { m_color = value; }
    //}
}
