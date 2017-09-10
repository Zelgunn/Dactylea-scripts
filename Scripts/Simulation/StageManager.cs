using UnityEngine;
using System.Collections;

public class StageManager : MonoBehaviour
{
    static private StageManager s_singleton;

    [SerializeField] private Transform m_cogWheel;
    [SerializeField] private Transform m_stage;
    [SerializeField] [Range(0, 0.5f)] private float m_stageHeight = 0;
    private AudioSource m_audioSource;
    private float m_remainingTimeAudio = 0;

    private bool m_goingUp = false;
    private bool m_goingDown = false;

    private void Awake()
    {
        s_singleton = this;

        m_audioSource = GetComponent<AudioSource>();

        float tmp = m_stageHeight;
        m_stageHeight = 0;
        MoveStage(tmp);
    }

    private void Update()
    {
        if(m_goingUp && !m_goingDown)
        {
            MoveStage(Time.deltaTime / 10);
        }
        else if(m_goingDown && !m_goingUp)
        {
            MoveStage(- Time.deltaTime / 10);
        }

        if(m_remainingTimeAudio > 0)
        {
            m_remainingTimeAudio -= Time.deltaTime;
            m_audioSource.mute = false;
        }
        else
        {
            m_remainingTimeAudio = 0;
            m_audioSource.mute = true;
        }
    }

    private void MoveStage(float delta)
    {
        m_stageHeight += delta;
        if(m_stageHeight < 0)
        {
            m_stageHeight = 0;
        }
        else if(m_stageHeight > 0.5f)
        {
            m_stageHeight = 0.5f;
        }
        else
        {
            m_remainingTimeAudio += Time.deltaTime;
        }

        Vector3 stagePosition = m_stage.localPosition;
        stagePosition.y = m_stageHeight;
        m_stage.localPosition = stagePosition;

        Vector3 cogWheelEuler = m_cogWheel.localEulerAngles;
        cogWheelEuler.z = m_stageHeight * 360 * 5 / 0.5f;
        m_cogWheel.localEulerAngles = cogWheelEuler;
    }

    static public void MoveStageUp(bool activate = true)
    {
        s_singleton.m_goingUp = activate;
        if (activate) s_singleton.m_goingDown = false;
    }

    static public void MoveStageDown(bool activate = true)
    {
        s_singleton.m_goingDown = activate;
        if (activate) s_singleton.m_goingUp = false;
    }

    static public float stageHeight
    {
        get { return s_singleton.m_stageHeight; }
    }
}
