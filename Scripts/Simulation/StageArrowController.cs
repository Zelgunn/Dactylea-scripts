using UnityEngine;
using System.Collections;

[RequireComponent(typeof(BoxCollider))]
public class StageArrowController : MonoBehaviour
{
    [Header("Arrow")]
    [SerializeField] private bool m_isUpperArrow;

    [Header("Animation")]
    [SerializeField] private float m_maxScale = 1.5f;
    [SerializeField] [Range(0, 1)] private float m_scaleSpeed = 0.25f;

    private Vector3 m_baseScale;
    private float m_scaleFactor = 1;
    private float m_scaleFactorDirection = 1;

    private void Awake ()
    {
        GetComponent<BoxCollider>().isTrigger = true;
        m_baseScale = transform.localScale;
    }

    private void Start()
    {

    }

    private void Update ()
    {
        int wandCount = ViveWand.wandCount;
        bool targeted = false;
        bool targetedAndActivated = false;

        for (int i = 0; i < wandCount; i++)
        {
            ViveWand wand = ViveWand.Wand(i);

            if (!wand) continue;
            if (wand.skin == ViveWand.ViveWandSkins.Beam) continue;

            if (wand.targetedGameObject == gameObject)
            {
                targeted = true;

                if (wand.IsInteractionPressed(XperAction.Grip) || wand.IsInteractionPressed(XperAction.Trigger))
                {
                    targetedAndActivated = true;
                }
            }
        }

        if(targeted)
        {
            if((m_scaleFactor <= 1) || (m_scaleFactor >= m_maxScale))
            {
                m_scaleFactorDirection = -m_scaleFactorDirection;
            }
        }
        else
        {
            m_scaleFactorDirection = -1;
        }

        m_scaleFactor += m_scaleFactorDirection * Time.deltaTime * m_scaleSpeed;
        m_scaleFactor = Mathf.Max(Mathf.Min(m_scaleFactor, m_maxScale), 1);

        transform.localScale = m_scaleFactor * m_baseScale;

        if(m_isUpperArrow)
        {
            StageManager.MoveStageUp(targetedAndActivated);
        }
        else
        {
            StageManager.MoveStageDown(targetedAndActivated);
        }
    }


}
