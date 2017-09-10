using UnityEngine;
using System.Collections;

public class MagneticAgitatorBipManager : MonoBehaviour
{
    [SerializeField] private AudioClip m_plusBip;
    [SerializeField] private AudioClip m_lessBip;
    private AudioSource m_audioSource;

	private void Awake ()
	{
        m_audioSource = GetComponent<AudioSource>();
	}
	
	public void PlayPlusBip ()
	{
        m_audioSource.PlayOneShot(m_plusBip);
	}

    public void PlayLessBip()
    {
        m_audioSource.PlayOneShot(m_lessBip);
    }
}
