using UnityEngine;
using System.Collections;

public class VivePenDistributorHandle : ViveInteractible
{
    private float m_previousWandHeight;

    new protected void Update()
    {
        base.Update();

        if(m_interactionDown)
        {
            m_previousWandHeight = m_interactionWand.transform.position.y;
        }
        else if(m_interactionInProgress)
        {
            Vector3 position = transform.position;
            position.y += m_interactionWand.transform.position.y - m_previousWandHeight;
            transform.position = position;

            position = transform.localPosition;
            if (position.y > 2.45f) position.y = 2.45f;
            if (position.y < 1.65f) position.y = 1.65f;
            transform.localPosition = position;

            m_previousWandHeight = m_interactionWand.transform.position.y;
        }
    }
}
