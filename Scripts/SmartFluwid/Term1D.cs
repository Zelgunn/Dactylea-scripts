using UnityEngine;
using System.Collections;

public class Term1D
{
    private float m_scalar = 0;
    private float m_power = 0;

    public Term1D()
    {

    }

    public Term1D(float power, float scalar)
    {
        m_power = power;
        m_scalar = scalar;
    }

    public float Calculate(float x)
    {
        return m_scalar * Mathf.Pow(x, m_power);
    }

    #region Operators
    public static Term1D operator *(Term1D a, Term1D b)
    {
        return new Term1D(a.m_power + b.m_power, a.m_scalar * b.m_scalar);
    }

    public static Term1D operator *(float scalar, Term1D term)
    {
        return new Term1D(term.power, term.scalar * scalar);
    }

    public static Term1D operator *(Term1D term, float scalar)
    {
        return new Term1D(term.power, term.scalar * scalar);
    }

    public static Term1D operator +(Term1D term, float scalar)
    {
        return new Term1D(term.power, term.scalar + scalar);
    }

    #endregion

    #region Accesseurs

    public float power
    {
        get { return m_power; }
    }

    public float scalar
    {
        get { return m_scalar; }
    }

    #endregion
}
