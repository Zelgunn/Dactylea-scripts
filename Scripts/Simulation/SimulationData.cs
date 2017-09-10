using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class SimulationData : MonoBehaviour
{
    public enum MaterialType
    {
        AcierNormal,
        Air,
        Argent,
        Béton,
        Bois,
        Caoutchouc,
        Verre,
        VerreBorosilicate,
        Inox,
        Or,
        Plomb
    }

    static private SimulationData s_singleton;

    [SerializeField] [Range(-273.15f, 300)] private float m_roomHeat;
    private Dictionary<MaterialType, float> m_coefficientsOfThermalConductivity;

    [Header("Sounds")]
    [SerializeField] private AudioClip m_explosionSound;
    [SerializeField] private AudioClip m_fireStart;
    [SerializeField] private AudioClip m_fireLoop;
    [SerializeField] private AudioClip m_glassBreakSound;
    [SerializeField] private AudioClip[] m_glassShockSounds;

    private void Awake ()
	{
        s_singleton = this;

        m_coefficientsOfThermalConductivity = new Dictionary<MaterialType, float>();
        m_coefficientsOfThermalConductivity.Add(MaterialType.AcierNormal, 50.2f);
        m_coefficientsOfThermalConductivity.Add(MaterialType.Air, 0.024f);
        m_coefficientsOfThermalConductivity.Add(MaterialType.Argent, 406);
        m_coefficientsOfThermalConductivity.Add(MaterialType.Béton, 0.8f);
        m_coefficientsOfThermalConductivity.Add(MaterialType.Bois, 0.1f);
        m_coefficientsOfThermalConductivity.Add(MaterialType.Caoutchouc, 0.16f);
        m_coefficientsOfThermalConductivity.Add(MaterialType.Inox, 16.3f);
        m_coefficientsOfThermalConductivity.Add(MaterialType.Or, 314);
        m_coefficientsOfThermalConductivity.Add(MaterialType.Plomb, 34.7f);
        m_coefficientsOfThermalConductivity.Add(MaterialType.Verre, 0.8f);
        m_coefficientsOfThermalConductivity.Add(MaterialType.VerreBorosilicate, 1.2f);
	}
	
	private void Update ()
	{
	
	}

    static public float roomHeat
    {
        get { return s_singleton.m_roomHeat; }
    }

    static public float MaterialHeat(MaterialType material)
    {
        if(s_singleton.m_coefficientsOfThermalConductivity.ContainsKey(material))
        {
            return s_singleton.m_coefficientsOfThermalConductivity[material];
        }
        else
        {
            Debug.LogWarning("Attention : Matériel non repertorié (" + material.ToString() + "). Valeur renvoyée : 0");
            return 0;
        }
    }

    static public float airThermalConductivity
    {
        get { return s_singleton.m_coefficientsOfThermalConductivity[MaterialType.Air]; }
    }

    static public AudioClip explosionSound
    {
        get { return s_singleton.m_explosionSound; }
    }

    static public AudioClip fireStart
    {
        get { return s_singleton.m_fireStart; }
    }

    static public AudioClip fireLoop
    {
        get { return s_singleton.m_fireLoop; }
    }

    static public AudioClip glassBreakSound
    {
        get { return s_singleton.m_glassBreakSound; }
    }

    static public AudioClip glassShockSound
    {
        get { return s_singleton.m_glassShockSounds[Random.Range(0, s_singleton.m_glassShockSounds.Length)]; }
    }
}
