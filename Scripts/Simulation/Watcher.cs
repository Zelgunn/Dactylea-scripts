using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;
using System.Collections.Generic;
using System;

public class Watcher : MonoBehaviour
{
    static private Watcher s_singleton;

    [Header("Stuff to check")]
    [SerializeField] private SmartFluwid m_cupFluwidManager;
    [SerializeField] private MercuryThiocyanateStack m_mercuryThiocyanateStack;
    [SerializeField] private SmartFluwid m_ethoxyethane;
    private SmartFluwid[] m_allFluwidManagers;

    [Header("Stuff to destroooy")]
    [SerializeField] private MeshExplosion m_ethoxyethaneMeshExplosion;

    [Header("Failbox")]
    [SerializeField] private AudioSource m_explosionAudioSource;
    [SerializeField] private ParticleSystem m_explosionParticuleSystem;

    private bool m_failed = false;
    private Compound m_wastedCompound;

	private void Awake ()
	{
        s_singleton = this;
        m_allFluwidManagers = FindObjectsOfType<SmartFluwid>();
        List<SmartFluwid> tmpAllFluwidManagers = new List<SmartFluwid>(m_allFluwidManagers.Length);
        foreach(SmartFluwid smartFluwid in m_allFluwidManagers)
        {
            if(smartFluwid.meshID != MeshVolumeFormula.MeshID.Undefined)
            {
                tmpAllFluwidManagers.Add(smartFluwid);
            }
        }
        m_allFluwidManagers = tmpAllFluwidManagers.ToArray();

        m_wastedCompound = new Compound();
    }

    //private void Start()
    //{
        
    //}
	
	private void Update ()
	{
        if (m_failed)
            return;

        if (Time.timeSinceLevelLoad < 5) return;

        FailCondition failCondition;
        m_failed = 
            CheckCupFluwidManager(out failCondition) || 
            CheckEthoxethane(out failCondition) || 
            CheckFluwidsQuantities(out failCondition);

        if (m_failed)
        {
            StartCoroutine(FailCoroutine(failCondition));
        }
	}

    private IEnumerator FailCoroutine(FailCondition failCondition)
    {
        Debug.Log("FAIL : " + failCondition.failType);

        if (failCondition.failureIncludesExplosion)
        {
            m_explosionAudioSource.transform.position = failCondition.failurePosition;
            m_explosionAudioSource.Play();

            m_explosionParticuleSystem.transform.position = failCondition.failurePosition;
            m_explosionParticuleSystem.Play();

            if(failCondition.failType == FailCondition.FailType.EthoxyethaneExplosion)
            {
                m_ethoxyethaneMeshExplosion.Explode(failCondition.failurePosition);
            }

            yield return new WaitForSeconds(m_explosionAudioSource.clip.length + 0.25f);
        }

        if(failCondition.failType == FailCondition.FailType.BrokenGlass)
        {
            yield return new WaitForSeconds(2);
        }

        Instructor.PlayFailSound();

        yield return new WaitForSeconds(2);

        XperManager.SaveEntries();
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    private bool CheckCupFluwidManager(out FailCondition failCondition)
    {
        failCondition = new FailCondition(FailCondition.FailType.WetCup);
        failCondition.failed = m_cupFluwidManager.currentVolume >= (m_cupFluwidManager.fullVolume / 10);

        return failCondition.failed;
    }

    private bool CheckEthoxethane(out FailCondition failCondition)
    {
        failCondition = new FailCondition(FailCondition.FailType.EthoxyethaneExplosion);
        failCondition.failed = !m_ethoxyethane.flowBlocked;

        if(failCondition.failed)
        {
            failCondition.failureIncludesExplosion = true;
            failCondition.failurePosition = m_ethoxyethane.GetTopCenterWorldPosition();
        }

        return failCondition.failed;
    }

    private bool CheckFluwidsQuantities(out FailCondition failCondition)
    {
        failCondition = new FailCondition(FailCondition.FailType.NoMoreReagents);

        if (m_mercuryThiocyanateStack.stackSize > 0.1f) return false;

        float reagentsQuantity = 0;

        float hydrogenNitrate = 0;
        float mercury = 0;
        float mercuryNitrate = 0;
        float potassium = 0;

        foreach (SmartFluwid fluwidManager in m_allFluwidManagers)
        {
            if (fluwidManager.stackSize > 0.1f) return false;

            Compound compound = fluwidManager.compound;

            hydrogenNitrate += compound.ElementQuantity(Compound.Elements.HydrogenNitrate);
            mercury += compound.ElementQuantity(Compound.Elements.ElementalMercury) * 50;
            mercuryNitrate += compound.ElementQuantity(Compound.Elements.MercuryNitrate);
            potassium += compound.ElementQuantity(Compound.Elements.PotassiumThiocyanate);

            reagentsQuantity += compound.ElementQuantity(Compound.Elements.MercuryThiocyanate);
        }

        float mercuryNitrateDoable = Mathf.Min(hydrogenNitrate, mercury);
        float mercuryThiocyanateDoable = Mathf.Min(mercuryNitrate + mercuryNitrateDoable, potassium);
        reagentsQuantity += mercuryThiocyanateDoable;


        failCondition.failed = reagentsQuantity <= 0;

        return failCondition.failed;
    }

    static public void ProvokeFail(FailCondition failCondition, GameObject source)
    {
        if (s_singleton.m_failed) return;

        s_singleton.StartCoroutine(s_singleton.FailCoroutine(failCondition));

        if(failCondition.failureIncludesExplosion)
        {
            MeshExplosion meshExplosion = source.GetComponent<MeshExplosion>();
            if(meshExplosion)
            {
                meshExplosion.Explode(source.transform.position);
            }
        }
    }

    static public float WastedCompoundQuantity()
    {
        return s_singleton.m_wastedCompound.totalQuantity;
    }

    private void _AddToWaste(Compound waste)
    {
        m_wastedCompound += waste;
    }

    static public void AddToWaste(Compound waste)
    {
        s_singleton._AddToWaste(waste);
    }
}
