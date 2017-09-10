using UnityEngine;
using System.Collections;

public class Match : MonoBehaviour
{
    [SerializeField] private ParticleSystem m_fireParticuleSystem;
    private AudioSource m_audioSource;

	private void Awake ()
	{
        m_audioSource = GetComponent<AudioSource>();
        m_fireParticuleSystem.gameObject.SetActive(false);
	}

    private void Update()
    {
        m_fireParticuleSystem.transform.eulerAngles = new Vector3(270, 0, 0);
    }
	
    [ContextMenu("LIGHT IT UP !")]
    public void LightUp()
    {
        if (isLighted) return;
        m_fireParticuleSystem.gameObject.SetActive(true);
        m_audioSource.Play();
    }

    private void LightUp(GameObject other)
    {
        MatchBox matchBox = other.GetComponent<MatchBox>();

        if (matchBox)
        {
            LightUp();
            return;
        }

        if (!isLighted) return;

        Match match = other.GetComponent<Match>();

        if (match)
        {
            match.LightUp();
            return;
        }
    }

    public void LightOff()
    {
        m_fireParticuleSystem.gameObject.SetActive(false);
    }

    private void OnTriggerEnter(Collider other)
    {
        LightUp(other.gameObject);
    }

    private void OnCollisionEnter(Collision other)
    {
        LightUp(other.gameObject);
    }

    public bool isLighted
    {
        get { return m_fireParticuleSystem.gameObject.activeSelf; }
    }
}
