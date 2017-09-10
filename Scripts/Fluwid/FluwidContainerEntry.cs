using UnityEngine;
using System.Collections;

[ExecuteInEditMode]
public class FluwidContainerEntry : MonoBehaviour
{
    [Header("Liquides")]
    [SerializeField] private SmartFluwid m_fluwidManager;
    private ViveCork m_viveCork = null;
    [SerializeField] private Transform m_corkPosition;

    private AudioSource m_audioSource;
    [Header("Sons")]
    [SerializeField] private AudioClip m_openingSound;
    [SerializeField] private AudioClip m_closingSound;
    private float m_lastSoundTime = 0;

    private void Awake()
    {
        m_audioSource = GetComponent<AudioSource>();
    }

    [ContextMenu("Check FluwidManager in Parent")]
    private void Start()
    {
        if (m_fluwidManager != null)
            return;

        SmartFluwid tmp = GetComponentInParent<SmartFluwid>();

        if(tmp != null)
        {
            m_fluwidManager = tmp;
        }
    }

    public SmartFluwid fluwidManager
    {
        get { return m_fluwidManager; }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!m_corkPosition) return;

        ViveCork viveCork = other.GetComponent<ViveCork>();

        if (viveCork && !hasCork)
        {
            viveCork.AttachTo(m_fluwidManager.transform, m_corkPosition.localPosition + transform.localPosition);
            m_fluwidManager.flowBlocked = true;
            m_viveCork = viveCork;
            PlayPopSound(m_openingSound);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (!m_corkPosition) return;

        ViveCork viveCork = other.GetComponent<ViveCork>();

        if (viveCork && hasCork)
        {
            if (!viveCork.picked) viveCork.Release();
            else viveCork.CancelAttach();
            m_fluwidManager.flowBlocked = false;
            m_viveCork = null;
            PlayPopSound(m_closingSound);
        }
    }

    private void PlayPopSound(AudioClip audio)
    {
        if((Time.time - m_lastSoundTime) < 1)
        {
            return;
        }

        m_lastSoundTime = Time.time;
        m_audioSource.pitch = Random.Range(0.9f, 1.1f);
        m_audioSource.PlayOneShot(audio);
    }

    public bool hasCork
    {
        get { return m_viveCork != null; }
    }

    public ViveCork viveCork
    {
        get { return m_viveCork; }
    }
}
