using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class MercuryThiocyanateCup : MonoBehaviour
{
    static private MercuryThiocyanateCup s_singleton;
    [SerializeField] MercuryThiocyanateStack m_mercuryThiocyanateStack;
    private List<SmartFluwid> m_containedFluwidManagers = new List<SmartFluwid>();

    private void Awake()
    {
        s_singleton = this;
    }

    private void OnTriggerEnter(Collider other)
    {
        FluwidContainerEntry fluwidContainerEntry = other.GetComponent<FluwidContainerEntry>();

        if (fluwidContainerEntry)
        {
            m_containedFluwidManagers.Add(fluwidContainerEntry.fluwidManager);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        FluwidContainerEntry fluwidContainerEntry = other.GetComponent<FluwidContainerEntry>();

        if (fluwidContainerEntry)
        {
            m_containedFluwidManagers.Remove(fluwidContainerEntry.fluwidManager);
        }
    }

    static public void AddToStack(float quantity)
    {
        s_singleton.m_mercuryThiocyanateStack.IncreaseStackSize(quantity);
    }

    static public bool Contains(SmartFluwid fluwidManager)
    {
        return s_singleton.m_containedFluwidManagers.Contains(fluwidManager);
    }
}
