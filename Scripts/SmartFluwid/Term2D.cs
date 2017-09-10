using UnityEngine;
using System.Collections;

public class Term2D
{
    private float m_scalar = 0;
    private float m_xPower = 0, m_yPower = 0;

    public Term2D()
    {

    }

    public Term2D(float xPower, float yPower, float scalar)
    {
        m_xPower = xPower;
        m_yPower = yPower;
        m_scalar = scalar;
    }

    public float Calculate(float x, float y)
    {
        return m_scalar * Mathf.Pow(x, m_xPower) * Mathf.Pow(y, m_yPower);
    }

    static public Term2D MultiplyXYTerms(Term1D xPolygon, Term1D yPolygon)
    {
        Term2D result = new Term2D();

        result.m_xPower = xPolygon.power;
        result.m_yPower = yPolygon.power;
        result.m_scalar = xPolygon.scalar * yPolygon.scalar;

        return result;
    }

    #region Operators
    public static Term2D operator *(Term2D a, Term2D b)
    {
        Term2D result = new Term2D();

        result.m_scalar = a.m_scalar * b.m_scalar;
        result.m_xPower = a.m_xPower + b.m_xPower;
        result.m_yPower = a.m_yPower + b.m_yPower;

        return result;
    }

    public static Term2D operator *(float scalar, Term2D term)
    {
        term.m_scalar *= scalar;
        return term;
    }

    public static Term2D operator *(Term2D term, float scalar)
    {
        term.m_scalar *= scalar;
        return term;
    }

    public static Term2D operator +(Term2D term, float scalar)
    {
        term.m_scalar += scalar;
        return term;
    }
    #endregion

    #region Accesseurs

    public float xPower
    {
        get { return m_xPower; }
    }

    public float yPower
    {
        get { return m_yPower; }
    }

    public float scalar
    {
        get { return m_scalar; }
    }

    #endregion
}
