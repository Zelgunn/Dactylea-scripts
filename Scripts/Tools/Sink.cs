using UnityEngine;
using System.Collections;

public class Sink : MonoBehaviour
{
    private SmartFluwid m_fluwidManager;
    private AudioSource m_audioSource;

    private void Awake()
    {
        m_fluwidManager = GetComponent<SmartFluwid>();
        m_audioSource = GetComponent<AudioSource>();
    }

    private void Update()
    {
        float ratio = m_fluwidManager.currentVolume / m_fluwidManager.fullVolume;

        if(ratio < 0.1f)
        {
            m_audioSource.enabled = false;
        }
        else
        {
            m_audioSource.enabled = true;
            m_audioSource.volume = (ratio * 0.9f) + 0.1f;
        }
    }
}
