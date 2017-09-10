using UnityEngine;
using System.Collections;

public class FluwidSink : SmartFluwid
{
    new protected void Update()
    {
        m_currentVolume = m_compound.totalQuantity;
        if (m_currentVolume < 0) m_currentVolume = 0;

        UpdateHeight();
        UpdateRenderer();

        m_volumePercentage = m_currentVolume / m_fullVolume;

        if(m_volumePercentage > 0)
        {
            m_audioSource.enabled = true;

            float ratio = m_currentVolume / m_fullVolume;
            if (ratio < 0.25f) ratio = 0.25f;
            ratio *= m_fullVolume;
            ratio *= Time.deltaTime;
            ratio /= 10;
            ratio = (m_compound.totalQuantity - ratio) / m_compound.totalQuantity;
            if (ratio < 0) ratio = 0;

            m_compound *= ratio;
        }
        else
        {
            m_audioSource.enabled = false;
        }
    }
}
