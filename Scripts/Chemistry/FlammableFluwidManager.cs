using UnityEngine;
using System.Collections;

public class FlammableFluwidManager : MonoBehaviour
{
    private SmartFluwid m_fluwidManager;
    private MeshExplosion m_meshExplosion;
    private MeshCollider m_meshCollider;

    private void Awake()
    {
        m_fluwidManager = GetComponent<SmartFluwid>();
        m_meshExplosion = GetComponentInParent<MeshExplosion>();
        m_meshCollider = GetComponent<MeshCollider>();
    }

    private void Start()
    {
        if(!m_meshCollider)
        {
            MeshFilter meshFilter = GetComponent<MeshFilter>();
            if (meshFilter)
            {
                m_meshCollider = gameObject.AddComponent<MeshCollider>();

                m_meshCollider.sharedMesh = meshFilter.sharedMesh;
                m_meshCollider.convex = true;
                m_meshCollider.isTrigger = true;
            }
        }
        else
        {
            m_meshCollider.convex = true;
            m_meshCollider.isTrigger = true;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        Match match = other.GetComponent<Match>();

        if (match && match.isLighted && ContainsEnoughEthanolToReactToFire())
        {
            if (CanExplode())
            {
                FailCondition fail = new FailCondition(FailCondition.FailType.EthanolExplosion);
                fail.failureIncludesExplosion = true;
                fail.failurePosition = transform.position;
                Watcher.ProvokeFail(fail, transform.parent.gameObject);
                m_meshExplosion.Explode(transform.position);
            }
            else if (!m_fluwidManager.burning)
            {
                m_fluwidManager.BurnEthanol();
            }
        }
    }

    private bool ContainsEnoughEthanolToReactToFire()
    {
        if (m_fluwidManager.currentVolume == 0) return false;


        Compound compound = m_fluwidManager.compound;

        float explosiveElementQuantity = 0;

        explosiveElementQuantity += compound.ElementQuantity(Compound.Elements.Ethanol);

        return (explosiveElementQuantity / m_fluwidManager.currentVolume) > 0.5f;
    }

    private bool CanExplode()
    {
        return m_fluwidManager.flowBlocked;
    }

    [ContextMenu("Kaaawaaa :3")]
    private void Kawaaaa()
    {
        FlammableFluwidManager[] kawa = FindObjectsOfType<FlammableFluwidManager>();

        foreach (FlammableFluwidManager k in kawa)
        {
            MeshCollider m = k.gameObject.AddComponent<MeshCollider>();
            m.sharedMesh = GetComponent<MeshFilter>().mesh;
            m.convex = true;
            m.isTrigger = true;
        }
    }
}
