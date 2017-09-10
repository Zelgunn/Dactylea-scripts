using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class Instructor : MonoBehaviour
{
    static private Instructor s_singleton;

    [SerializeField] private InstructorButton m_playPauseButton;
    [SerializeField] private Text m_currentInstructionDisplay;
    [Header("Instructions")]
    [SerializeField] private AudioClip[] m_instructions;
    [Header("Sons fin")]
    [SerializeField] private AudioClip m_reactionFinished;
    [SerializeField] private AudioClip m_congratulationSound;
    [Header("Sons d'échec")]
    [SerializeField] private AudioClip[] m_failSounds;
    private AudioSource m_audioSource;
    private int m_currentInstruction = 0;
    private float m_timeAtPause = 0;

	private void Awake ()
	{
        s_singleton = this;
        m_audioSource = GetComponent<AudioSource>();

        m_currentInstructionDisplay.text = "1 / " + m_instructions.Length;
        m_audioSource.clip = m_instructions[0];

        m_audioSource.Play();
	}

    private void Update()
    {
        if(isPlaying)
        {
            m_playPauseButton.TurnOn();
        }
        else
        {
            m_playPauseButton.TurnOff();
        }
    }

    private void _OnPlayPauseButton()
    {
        if (m_audioSource.isPlaying)
        {
            m_audioSource.Stop();
        }
        else
        {
            m_audioSource.Play();
        }
    }

    static public void OnPlayPauseButton()
    {
        s_singleton._OnPlayPauseButton();
    }

    public void PlayInstruction()
    {
        m_audioSource.Play();
    }

    public void PauseInstruction()
    {
        m_timeAtPause = m_audioSource.time;
        m_audioSource.Stop();
    }

    public void ContinueInstruction()
    {
        m_audioSource.Play();
        m_audioSource.time = m_timeAtPause;
        m_timeAtPause = 0;
    }

    private void _OnPreviousButton()
    {
        m_currentInstruction = Mathf.Max(m_currentInstruction - 1, 0);

        m_audioSource.clip = m_instructions[m_currentInstruction];
        m_audioSource.Play();

        m_currentInstructionDisplay.text = (m_currentInstruction + 1) + " / " + m_instructions.Length;
    }

    public void PlayPreviousInstruction()
    {
        _OnPreviousButton();
    }

    static public void OnPreviousButton()
    {
        s_singleton._OnPreviousButton();
    }

    private void _OnNextButton()
    {
        m_currentInstruction = Mathf.Min(m_currentInstruction + 1, m_instructions.Length - 1);

        m_audioSource.clip = m_instructions[m_currentInstruction];
        m_audioSource.Play();

        m_currentInstructionDisplay.text = (m_currentInstruction + 1) + " / " + m_instructions.Length;
    }

    public void PlayNextInstruction()
    {
        _OnNextButton();
    }

    static public void OnNextButton()
    {
        s_singleton._OnNextButton();
    }

    public void RepeatInstruction()
    {
        m_audioSource.time = 0;
        m_audioSource.Play();
    }

    private void _OnLoopButton()
    {
        m_audioSource.loop = !m_audioSource.loop;
        if (m_audioSource.loop && !m_audioSource.isPlaying) m_audioSource.Play();
    }

    static public void OnLoopButton()
    {
        s_singleton._OnLoopButton();
    }

    static public bool isLooping
    {
        get { return s_singleton.m_audioSource.loop; }
    }

    static public bool isPlaying
    {
        get { return s_singleton.m_audioSource.isPlaying; }
    }

    static public void PlayFailSound()
    {
        s_singleton.m_audioSource.Stop();
        s_singleton.m_audioSource.PlayOneShot(s_singleton.m_failSounds[Random.Range(0, s_singleton.m_failSounds.Length)]);
    }

    private IEnumerator PlaySuccessfulEndSoundsCoroutine()
    {
        m_audioSource.PlayOneShot(m_reactionFinished);

        yield return new WaitForSeconds(m_reactionFinished.length);

        m_audioSource.PlayOneShot(m_congratulationSound);
    }

    static public void PlaySuccessfulEndSounds()
    {
        s_singleton.StartCoroutine(s_singleton.PlaySuccessfulEndSoundsCoroutine());
    }
}
