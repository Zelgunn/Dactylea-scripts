using UnityEngine;
using System.Collections;

public class HeatableObject : MonoBehaviour
{
    [SerializeField] private SimulationData.MaterialType m_materialType;
    private float m_coefficientOfThermalCondictivity;
    [SerializeField] private float m_temperature;
    [SerializeField] private float m_agitation;

	private void Start ()
	{
        m_temperature = SimulationData.roomHeat;
        m_coefficientOfThermalCondictivity = SimulationData.MaterialHeat(m_materialType);
	}
	
	private void Update ()
	{
        m_temperature += (SimulationData.roomHeat - m_temperature) * Time.deltaTime * m_coefficientOfThermalCondictivity * SimulationData.airThermalConductivity; ;
	    
	}

    public float temperature
    {
        get { return m_temperature; }
        set { m_temperature = value; }
    }

    public float agitation
    {
        get { return m_agitation; }
        set { m_agitation = value; }
    }

    public float coefficientOfThermalCondictivity
    {
        get { return m_coefficientOfThermalCondictivity; }
    }
}
