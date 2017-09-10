using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class InstructorButton : ViveInteractible
{
    public enum InstructorButtonType
    {
        PlayPause,
        Previous,
        Next,
        Loop
    }

    [Header("Bouton instructeur")]
    [SerializeField] private InstructorButtonType m_type;

    private RawImage m_rawImage;

    [SerializeField] [HideInInspector]
    private Texture m_onTexture;
    private Texture m_offTexture;

    [SerializeField] private Color m_pressedColor;
    private Color m_normalColor;
    
    private void Awake()
    {
        m_rawImage = GetComponent<RawImage>();
        m_offTexture = m_rawImage.texture;
        m_normalColor = m_rawImage.color;
    }

    new private void Update()
    {
        base.Update();

        if(m_isAtRangeOfWand)
        {
            m_rawImage.color = m_pressedColor;
        }
        else
        {
            m_rawImage.color = m_normalColor;
        }
    }

    protected override void OnActionDown()
    {
        switch (m_type)
        {
            case InstructorButtonType.Loop:
                Instructor.OnLoopButton();
                if (Instructor.isLooping)
                {
                    TurnOn();
                }
                else
                {
                    TurnOff();
                }
                break;
            case InstructorButtonType.Next:
                Instructor.OnNextButton();
                break;
            case InstructorButtonType.PlayPause:
                Instructor.OnPlayPauseButton();
                break;
            case InstructorButtonType.Previous:
                Instructor.OnPreviousButton();
                break;
        }
        ReportInteraction();
    }

    public void TurnOn()
    {
        m_rawImage.texture = m_onTexture;
    }

    public void TurnOff()
    {
        m_rawImage.texture = m_offTexture;
    }

    public InstructorButtonType type
    {
        get { return m_type; }
    }

    public Texture onTexture
    {
        get { return m_onTexture; }
        set { m_onTexture = value; }
    }

    public Texture offTexture
    {
        get { return m_offTexture; }
        set { m_offTexture = value; }
    }
}
