using UnityEngine;
using System.Collections;

public class FailCondition
{
    public enum FailType
    {
        WetCup,
        EthoxyethaneExplosion,
        NoMoreReagents,
        EthanolExplosion,
        BrokenGlass
    }

    private bool m_failed = false;
    private FailType m_failType;
    private bool m_failureIncludesExplosion = false;
    private Vector3 m_failurePosition;

    public FailCondition(FailType type)
    {
        m_failed = false;
        m_failureIncludesExplosion = false;
        m_failType = type;
    }

    public bool failed
    {
        get { return m_failed; }
        set { m_failed = value; }
    }

    public bool failureIncludesExplosion
    {
        get { return m_failureIncludesExplosion; }
        set { m_failureIncludesExplosion = value; }
    }

    public Vector3 failurePosition
    {
        get { return m_failurePosition; }
        set { m_failurePosition = value; }
    }

    public FailType failType
    {
        get { return m_failType; }
        set { m_failType = value; }
    }
}
