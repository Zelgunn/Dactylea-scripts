using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class FluwidTap : SmartFluwid
{
    #region Champs
    [SerializeField] private Rigidbody m_tap;
    [SerializeField] private bool m_endless = true;

    private float m_opening = 0;
    private Vector3 m_baseTransformUp;
    [SerializeField] private float m_bottomRadius;
  
    #endregion

    new protected void Awake()
	{
        if (!m_endless)
        {
            base.Awake();
            InitBottomVertices();
        }
        else
        {
            m_scaleFactor = transform.lossyScale.x * transform.lossyScale.y * transform.lossyScale.z;
            InitVertices();
            InitBottomVertices();

            m_compound = new Compound(m_startElement, 1);

            m_audioSource = GetComponent<AudioSource>();
            if (m_audioSource) m_audioSource.enabled = false;
        }

        m_baseTransformUp = m_tap.transform.up;
    }

    #region Updates
    new protected void Update()
	{
        UpdateOpening();

        if(m_endless)
        {
            UpdateEndless();
        }
        else
        {
            UpdateNonEndless();
        }

        UpdateSound();
    }

    private void UpdateOpening()
    {
        float sign = Mathf.Sign(Vector3.Dot(Vector3.Cross(m_baseTransformUp, m_tap.transform.up), Vector3.forward));
        float angle = Vector3.Angle(m_baseTransformUp, m_tap.transform.up);

        if(angle == 0)
        {
            m_opening = 0;
            m_flowing = false;
        }
        else if(sign >= 0)
        {
            m_opening = angle / 180;
            if (m_opening < 0.01f)
            {
                m_opening = 0;
                m_flowing = false;
            }
            else
            {
                m_flowing = true;
            }
        }
        else
        {
            if(angle > 90)
            {
                m_opening = 0;
                m_flowing = false;
            }
            else
            {
                m_opening = 1;
                m_flowing = true;
            }
        }


        m_flowBlocked = !m_flowing;
    }

    override protected void UpdateSound()
    {
        if (m_audioSource)
        {
            m_audioSource.enabled = opened;
            if (opened)
            {
                m_audioSource.volume = m_opening;
            }

        }
    }

    private void UpdateEndless()
    {
        if (!opened)
        {
            m_lastNetBall = null;
            return;
        }

        Compound flowingCompound;
        flowingCompound = new Compound(m_startElement, m_bottomRadius * m_bottomRadius * m_opening);

        UpdateNet(flowingCompound);
    }

    private void UpdateNonEndless()
    {
        if (!opened)
        {
            m_lastNetBall = null;
            return;
        }

        Compound flowingCompound;
        flowingCompound = new Compound(m_startElement, m_bottomRadius * m_bottomRadius * m_opening / 10);
        m_compound -= flowingCompound;

        UpdateNet(flowingCompound);

        m_currentVolume = m_compound.totalQuantity;
        if (m_currentVolume < 0) m_currentVolume = 0;

        UpdateHeight();
        UpdateRenderer();
    }

    private void UpdateNet(Compound flowingCompound)
    {
        #region Net
        NetBall ball = Instantiate<NetBall>(m_netBallPrefab);
        Destroy(ball.gameObject, m_livingTime);

        Vector3 transformedFlowPoint = transform.position + TransformedVertex(m_flowPoint);
        ball.transform.position = transformedFlowPoint;
        ball.containedCompound = flowingCompound;
        ball.creator = this;
        ball.transform.LookAt(transformedFlowPoint - transform.up);

        #region Net - Connexion vers la prochaine balle

        if (!m_lastNetBall)
        {
            m_lastNetBall = ball;
        }
        else
        {
            NetWater waterNet = Instantiate<NetWater>(m_waterNetPrefab);
            waterNet.SetAttachments(m_lastNetBall.transform, ball.transform);
            waterNet.color = m_compound.color;
            waterNet.size = m_bottomRadius * m_opening;

            m_lastNetBall = ball;
        }
        #endregion
        #endregion
    }
    #endregion

    #region Bottom Vertices

    /// <summary>
    /// Calcule le point de sortie du robinet.
    /// </summary>
    protected void InitBottomVertices()
    {
        Vector3[] transformedVertices = TransformedVertices(m_vertices);

        float maxHeight = transformedVertices[0].y;
        float minHeight = maxHeight;

        for (int i = 0; i < transformedVertices.Length; i++)
        {
            float h = transformedVertices[i].y;

            if (h > maxHeight) maxHeight = h;
            if (h < minHeight) minHeight = h;
        }

        float deltaMinMax = maxHeight - minHeight;
        float maxValidDelta = deltaMinMax / 100;

        m_flowPoint = Vector3.zero;
        List<Vector3> tmpBottomVertices = new List<Vector3>(transformedVertices.Length / 4);

        for (int i = 0; i < transformedVertices.Length; i++)
        {
            float h = transformedVertices[i].y;

            float deltaH = h - minHeight;
            if (deltaH <= maxValidDelta)
            {
                m_flowPoint += m_vertices[i];
                tmpBottomVertices.Add(m_vertices[i]);
            }
        }

        Vector3[] bottomVertices = tmpBottomVertices.ToArray();
        m_flowPoint /= bottomVertices.Length;

        // Init radius
        m_bottomRadius = 0;
        for (int i = 0; i < bottomVertices.Length; i++)
        {
            float d = (m_flowPoint - bottomVertices[i]).magnitude;

            if (d > m_bottomRadius) m_bottomRadius = d;
        }
        m_bottomRadius *= m_scaleFactor;

        m_baseFlowFactor = m_flowFactor = m_bottomRadius * m_bottomRadius / m_fullVolume;
    }

    #endregion

    public bool opened
    {
        get { return m_opening > 0.01f; }
    }
}
