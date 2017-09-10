using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Compound
{
    public enum Elements
    {
        HydrogenNitrate,                // Acide Nitrique           Corrosif        || Comburant || Très toxique
        ElementalMercury,               // Mercure élémentaire      Très toxique    || Dangereux pour l'environnement
        SodiumHypochlorite,             // Eau de Javel             Corrosif        || Dangereux pour l'environnement
        DihidrogenMonoxide,             // De l'eau :3              Chaque humain qui en boit finit un jour par mourir ?
        Ethanol,                        // Ethanol                  
        Ethoxyethane,                   // Ether                    Hautement volatile et inflammable   || Dangereux pour la peau   || Décompose en explosif au contact de l'air et de la lumière
        PotassiumThiocyanate,           // Thiocyanate de potassium
        MercuryNitrate,                 // Nitrate de Mercure
        MercuryThiocyanate,             // Thiocyanate de Mercure
        UnlistedElement
    }

    static private Dictionary<Elements, Color> s_elementsColors = null;

    private Dictionary<Elements, float> m_quantities;
    private Color m_color;
    private float m_quantitySum = 0;

    public static Compound operator +(Compound a, Compound b)
    {
        Compound res = new Compound(a);

        List<Elements> keys = new List<Elements>(b.m_quantities.Keys);
        foreach (Elements key in keys)
        {
            if (res.m_quantities.ContainsKey(key))
            {
                res.m_quantities[key] += b.m_quantities[key];
            }
            else
            {
                res.m_quantities.Add(key, b.m_quantities[key]);
            }
        }

        res.ProcessTotalQuantity();
        res.ProcessColor();

        return res;
    }

    public static Compound operator -(Compound a, Compound b)
    {
        Compound res = new Compound(a);

        List<Elements> keys = new List<Elements>(b.m_quantities.Keys);
        foreach (Elements key in keys)
        {
            if (res.m_quantities.ContainsKey(key))
            {
                res.m_quantities[key] = Mathf.Max(res.m_quantities[key] - b.m_quantities[key], 0);
            }
        }

        res.ProcessTotalQuantity();
        res.ProcessColor();

        return res;
    }

    public static Compound operator /(Compound compound, float f)
    {
        Compound res = new Compound(compound);

        List<Elements> keys = new List<Elements>(res.m_quantities.Keys);
        foreach (Elements key in keys)
        {
            res.m_quantities[key] /= f;
        }

        res.ProcessTotalQuantity();
        res.ProcessColor();

        return res;
    }

    public static Compound operator *(Compound compound, float f)
    {
        Compound res = new Compound(compound);

        List<Elements> keys = new List<Elements>(res.m_quantities.Keys);
        foreach (Elements key in keys)
        {
            res.m_quantities[key] *= f;
        }

        res.ProcessTotalQuantity();
        res.ProcessColor();

        return res;
    }

    public Compound()
    {
        m_quantities = new Dictionary<Elements, float>();

        if(s_elementsColors == null)
        {
            InitCompoundColorArray();
        }

        m_color = Color.clear;
        m_quantitySum = 0;
    }

    public Compound(Elements element, float quantity)
    {
        m_quantities = new Dictionary<Elements, float>();

        m_quantities.Add(element, quantity);

        if (s_elementsColors == null)
        {
            InitCompoundColorArray();
        }

        ProcessTotalQuantity();
        ProcessColor();
    }

    public Compound(Compound other)
    {
        m_quantities = new Dictionary<Elements, float>(other.m_quantities);

        ProcessTotalQuantity();
        ProcessColor();
    }

    public Compound(Compound other, float quantity)
    {
        float quantityRatio = quantity / other.totalQuantity;

        Dictionary<Elements, float> tmpQuantities = new Dictionary<Elements, float>(other.m_quantities);

        m_quantities = new Dictionary<Elements, float>(tmpQuantities.Count);
        foreach(KeyValuePair<Elements, float> tmpQuantity in tmpQuantities)
        {
            m_quantities.Add(tmpQuantity.Key, tmpQuantity.Value * quantityRatio);
        }

        ProcessTotalQuantity();
        ProcessColor();
    }

    public void AddElement(Elements element, float quantity, bool update)
    {
        if (m_quantities.ContainsKey(element))
        {
            m_quantities[element] += quantity;
        }
        else
        {
            m_quantities.Add(element, quantity);
        }

        if(update)
        {
            ProcessTotalQuantity();
            ProcessColor();
        }
    }

    public void UpdateCompound(float heat, float agitation, float deltaTime)
    {
        bool updateCompound = false;

        updateCompound |= UpdateMercuryNitrate(heat, agitation, deltaTime);
        updateCompound |= UpdateMercuryThiocyanate(agitation, deltaTime);

        if(updateCompound)
        {
            ProcessTotalQuantity();
            ProcessColor();
        }
    }

    private bool UpdateMercuryNitrate(float heat, float agitation, float deltaTime)
    {
        if (!m_quantities.ContainsKey(Elements.ElementalMercury) || !m_quantities.ContainsKey(Elements.HydrogenNitrate))
            return false;

        if ((m_quantities[Elements.ElementalMercury] <= 0) || (m_quantities[Elements.HydrogenNitrate] <= 0))
            return false;

        float mercuryQuantity = m_quantities[Elements.ElementalMercury] * 50;
        float hydrogenNitrateQuantity = m_quantities[Elements.HydrogenNitrate];

        float mercuryNitrateQuantity = Mathf.Min(mercuryQuantity, hydrogenNitrateQuantity);
        mercuryNitrateQuantity /= (1 + Mathf.Exp(- (heat - 50) / 10));
        mercuryNitrateQuantity *= (Mathf.Pow(agitation, 2) + 0.1f) / 1.1f;
        mercuryNitrateQuantity *= deltaTime;

        m_quantities[Elements.ElementalMercury] -= mercuryNitrateQuantity / 50;
        m_quantities[Elements.HydrogenNitrate] -= mercuryNitrateQuantity;
        AddElement(Elements.MercuryNitrate, mercuryNitrateQuantity * 1.02f, false);

        return true;
    }

    private bool UpdateMercuryThiocyanate(float agitation, float deltaTime)
    {
        if (!m_quantities.ContainsKey(Elements.PotassiumThiocyanate) || !m_quantities.ContainsKey(Elements.MercuryNitrate))
            return false;

        if ((m_quantities[Elements.PotassiumThiocyanate] <= 0) || (m_quantities[Elements.MercuryNitrate] <= 0))
            return false;

        float potassiumThiocyanateQuantity = m_quantities[Elements.PotassiumThiocyanate];
        float mercuryNitrateQuantity = m_quantities[Elements.MercuryNitrate];

        float mercuryThiocyanateQuantity = Mathf.Min(potassiumThiocyanateQuantity, mercuryNitrateQuantity);
        mercuryThiocyanateQuantity *= Mathf.Pow(agitation, 2);
        mercuryThiocyanateQuantity *= deltaTime;

        m_quantities[Elements.PotassiumThiocyanate] -= mercuryThiocyanateQuantity;
        m_quantities[Elements.MercuryNitrate] -= mercuryThiocyanateQuantity;
        AddElement(Elements.MercuryThiocyanate, mercuryThiocyanateQuantity * 2, false);

        return true;
    }

    private void ProcessTotalQuantity()
    {
        m_quantitySum = 0;

        foreach (KeyValuePair<Elements, float> quantity in m_quantities)
        {
            m_quantitySum += quantity.Value;
        }
    }

    private void ProcessColor()
    {
        m_color = Color.clear;

        foreach (KeyValuePair<Elements, float> quantity in m_quantities)
        {
            m_color += s_elementsColors[quantity.Key] * quantity.Value / m_quantitySum;
        }
    }

    static private void InitCompoundColorArray()
    {
        s_elementsColors = new Dictionary<Elements, Color>();

        s_elementsColors.Add(Elements.HydrogenNitrate,      new Color(0.85f, 0.82f, 0.455f, 0.35f));
        s_elementsColors.Add(Elements.ElementalMercury,     new Color(0.427f, 0.23f, 0.23f, 1));
        s_elementsColors.Add(Elements.SodiumHypochlorite,   new Color(0.36f, 0.94f, 0.87f, 0.35f));
        s_elementsColors.Add(Elements.DihidrogenMonoxide,   new Color(0.8f, 0.8f, 0.9f, 0.35f));
        s_elementsColors.Add(Elements.Ethanol,              new Color(0.635f, 0.513f, 0.874f, 0.35f));
        s_elementsColors.Add(Elements.Ethoxyethane,         new Color(0.866f, 0.384f, 0.474f, 0.35f));
        s_elementsColors.Add(Elements.MercuryNitrate,       new Color(1f, 0.35f, 0f, 0.5f));
        s_elementsColors.Add(Elements.MercuryThiocyanate,   new Color(1f, 1f, 1f, 1f));
        s_elementsColors.Add(Elements.PotassiumThiocyanate, new Color(0.94f, 0.58f, 0.133f, 0.35f));
        s_elementsColors.Add(Elements.UnlistedElement,      new Color(0, 0, 0, 0));
    }

    /// <summary>
    /// Get the color of the compound.
    /// Compound's colors are only calculated when the compound's fields are modified.
    /// </summary>
    public Color color
    {
        get { return m_color; }
    }

    public float totalQuantity
    {
        get { return m_quantitySum; }
    }

    public bool isEmpty
    {
        get { return m_quantitySum == 0; }
    }

    public float ElementQuantity(Elements element)
    {
        if(m_quantities.ContainsKey(element))
        {
            return m_quantities[element];
        }

        return 0;
    }
}
