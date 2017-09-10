using UnityEngine;
using System.Collections.Generic;

public class InterpoLagrange2D
{
    private float[] m_xValues;
    private float[] m_yValues;
    private float[][] m_zValues;

    private Polynome2D m_polynome2D;

    public InterpoLagrange2D(float[] xValues, float[] yValues, float[][] zValues)
    {
        m_xValues = xValues;
        m_yValues = yValues;
        m_zValues = zValues;

        CalculateFunction();
    }

    public InterpoLagrange2D(float[] xValues, float[] yValues, float[][] zValues, float clearThreshold)
        : this(xValues, yValues, zValues)
    {
        m_polynome2D.Clear(clearThreshold);
    }

    public float Confidence()
    {
        int xCount = m_xValues.Length;
        int yCount = m_yValues.Length;
        //float[] deltas = new float[xCount * yCount];
        float averageError = 0;

        for (int i = 0; i < xCount; i++)
        {
            for (int j = 1; j < yCount; j++)
            {
                float originalValue = m_zValues[i][j];
                float interpolatedValue = m_polynome2D.Calculate(m_xValues[i], m_yValues[j]);

                float delta = originalValue - interpolatedValue;
                if(interpolatedValue == 0)
                {
                    if (originalValue != 0) averageError += Mathf.Abs(delta / originalValue);
                }
                else
                {
                    averageError += Mathf.Abs(delta / interpolatedValue);
                }


                //deltas[i * yCount + j] = delta;
            }
        }

        averageError /= xCount * yCount;
        return (1 - averageError);
    }

    #region Private Functions
    private void CalculateFunction()
    {
        m_polynome2D = new Polynome2D();

        int xCount = m_xValues.Length;
        int yCount = m_yValues.Length;

        // Somme de i = 0 à n de...
        for (int i = 0; i < xCount; i++)
        {
            // Somme de j = 0 à m de...
            for (int j = 0; j < yCount; j++)
            {
                // f(xi,yj)
                float z = m_zValues[i][j];
                // * Li(x) * Lj(y)
                Polynome1D lix = LagrangePolynome(m_xValues, i);
                Polynome1D ljy = LagrangePolynome(m_yValues, j);

                m_polynome2D += z * Polynome2D.MultiplyXYPolygones(lix, ljy);
            }
        }
    }
    #endregion

    #region LagrangePolynome
    static public Polynome1D LagrangePolynome(float[] points, int i)
    {
        return LagrangePolynome(new List<float>(points), i);
    }

    static public Polynome1D LagrangePolynome(List<float> points, int i)
    {
        Polynome1D result = new Polynome1D(1);
        int n = points.Count;

        for (int j = 0; j < n; j++)
        {
            if (j == i) continue;

            float denom = points[i] - points[j];

            Term1D term = new Term1D (1 , 1 / denom);
            List<Term1D> terms = new List<Term1D>();
            terms.Add(term);
            Polynome1D polynome = new Polynome1D (terms, -points[j] / denom);

            result *= polynome;
        }

        return result;
    }
    #endregion

    #region Accesseurs
    public Polynome2D polynome2D
    {
        get { return m_polynome2D; }
    }
    #endregion
}
