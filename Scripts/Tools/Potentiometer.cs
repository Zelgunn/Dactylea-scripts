using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public class Potentiometer : ViveInteractible
{
    [SerializeField] private int m_maxValue = 20;

    [SerializeField] private MagneticAgitatorBipManager m_audioManager;
    [SerializeField] private Text m_display;

    [Header("Debug")]
    [SerializeField] private int m_value = 0;

    #region Old Update
    //private void Update ()
    //{
    //       int previousValue = m_value;

    //       int wandCount = ViveWand.wandCount;

    //       bool touched = false;

    //       for (int i = 0; i < wandCount; i++)
    //       {
    //           ViveWand wand = ViveWand.Wand(i);

    //           if (!wand) continue;
    //           if (wand.targetedGameObject == gameObject)
    //           {
    //               if(wand.controller.GetTouch(Valve.VR.EVRButtonId.k_EButton_Axis0))
    //               {
    //                   touched = true;
    //                   float delta = wand.controller.GetAxis(Valve.VR.EVRButtonId.k_EButton_Axis0).x;

    //                   if(!m_previouslyTouched)
    //                   {
    //                       m_previousDelta = delta;
    //                   }
    //                   else
    //                   {
    //                       if ((delta - m_previousDelta) > 0.1f)
    //                       {
    //                           m_previousDelta = delta;
    //                           m_value++;
    //                       }
    //                       else if ((delta - m_previousDelta) < - 0.1f)
    //                       {
    //                           m_previousDelta = delta;
    //                           m_value--;
    //                       }

    //                       if (m_value < 0)
    //                       {
    //                           m_value = 0;
    //                       }
    //                       else if (m_value > m_maxValue)
    //                       {
    //                           m_value = m_maxValue;
    //                       }
    //                   }
    //               }
    //           }
    //       }

    //       m_previouslyTouched = touched;

    //       if(previousValue > m_value)
    //       {
    //           m_audioManager.PlayLessBip();
    //       }
    //       else if(previousValue < m_value)
    //       {
    //           m_audioManager.PlayPlusBip();
    //       }

    //       m_display.text = m_value.ToString();
    //       Vector3 angle = transform.localEulerAngles;
    //       angle.z = value * 360;
    //       transform.localEulerAngles = angle;
    //   }
    #endregion

    protected override void OnMoveAction(Vector2 delta)
    {
        PacedTurn(delta.x);
    }

    protected override void OnTurnAction(float angleDelta)
    {
        if (!m_interacting) return;

        float paceAngle = 360 / m_maxValue;
        PacedTurn(angleDelta / paceAngle);
    }

    private void PacedTurn(float delta)
    {
        float previousValue = value;
        if ((m_value > 0) && (delta <= -0.1f))
        {
            m_value--;
            m_audioManager.PlayLessBip();
        }
        else if ((m_value < m_maxValue) && (delta >= 0.1f))
        {
            m_value++;
            m_audioManager.PlayPlusBip();
        }
        else return;

        m_display.text = m_value.ToString();
        transform.Rotate(transform.forward, (value - previousValue) * 360, Space.World);
    }

    public float value
    {
        get { return (float)m_value / m_maxValue; }
    }
}
