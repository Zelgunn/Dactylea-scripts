using UnityEngine;
using System.Collections;

public class ViveWaterTap : ViveInteractible
{
    private Vector3 m_baseTransformUp;

    private void Awake()
    {
        m_baseTransformUp = transform.up;
    }

    new protected void Update()
    {
        base.Update();

        float sign = Mathf.Sign(Vector3.Dot(Vector3.Cross(m_baseTransformUp, transform.up), Vector3.forward));
        float angle = Vector3.Angle(m_baseTransformUp, transform.up);

        if(sign < 0)
        {
            Vector3 euler = transform.eulerAngles;

            if(angle > 90)
            {
                euler.z = 0;
            }
            else
            {
                euler.z = 180;
            }
            transform.eulerAngles = euler;
        }
    }

    protected override void OnMoveAction(Vector2 delta)
    {
        float angleDelta = transform.localEulerAngles.z - delta.x * 90;

        angleDelta = CheckRangedDeltangle(angleDelta);

        Turn(angleDelta);
    }

    protected override void OnTurnAction(float angleDelta)
    {
        if (!m_interacting) return;

        angleDelta = CheckRangedDeltangle(angleDelta);

        if (m_itemType == XperItem.PipetteTap) angleDelta = -angleDelta;

        Turn(transform.localEulerAngles.z + angleDelta);
    }

    private float CheckRangedDeltangle(float angleDelta)
    {
        //if (m_interactionRange == XperRange.Ranged)
        //{
        //    Plane normalPlane = new Plane(transform.forward, transform.position);
        //    float d;
        //    Ray ray = new Ray(m_interactionWand.transform.position, m_interactionWand.transform.forward);

        //    if (normalPlane.Raycast(ray, out d))
        //    {
        //        Vector3 crossPoint = ray.GetPoint(d);

        //        if (crossPoint.y > (transform.position.y + 0.01f))
        //        {
        //            angleDelta = -angleDelta;
        //        }
        //    }
        //}

        return angleDelta;
    }

    private void Turn(float angle)
    {
        //if ((angle < 0) || (angle > 270)) angle = 0;
        //if (angle > 180) angle = 180;

        Vector3 newLocalEulerAngles = transform.localEulerAngles;
        newLocalEulerAngles.z = angle;
        transform.localEulerAngles = newLocalEulerAngles;

        if (m_interactionDown) ReportInteraction();
    }
}
