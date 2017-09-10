using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public class MagneticHeater : MonoBehaviour
{
    [SerializeField] private Text m_temperatureDisplay;
    [SerializeField] private Potentiometer m_heatButton;
    [SerializeField] private float m_maxHeat;
    [SerializeField] private float m_temperature;
    [SerializeField] private SimulationData.MaterialType m_materialType;
    private float m_coefficientOfThermalCondictivity;
    private List<HeatableObject> m_heatedObjects;

    private void Awake()
    {
        m_heatedObjects = new List<HeatableObject>();
    }

	private void Start ()
	{
        m_temperature = SimulationData.roomHeat;
        m_coefficientOfThermalCondictivity = SimulationData.MaterialHeat(m_materialType);
	}
	
	private void Update ()
	{
        float heat = m_heatButton.value * (m_maxHeat - SimulationData.roomHeat) + SimulationData.roomHeat;

        // La température se dissipe dans l'air
        float coef = Time.deltaTime * m_coefficientOfThermalCondictivity * SimulationData.airThermalConductivity;

        if(heat <= 0)
        {
            m_temperature += (SimulationData.roomHeat - m_temperature) * coef;
        }
        else
        {
            m_temperature += (heat - m_temperature) * Time.deltaTime;
        }

        foreach (HeatableObject heatedObject in m_heatedObjects)
        {
            GiveHeatToObject(heatedObject);
        }

        m_temperatureDisplay.text = ((int)m_temperature).ToString() + "'C";
	}

    private void GiveHeatToObject(HeatableObject heatedObject)
    {
        float temperatureDelta = m_temperature - heatedObject.temperature;
        float exchangedHeat = temperatureDelta * heatedObject.coefficientOfThermalCondictivity * m_coefficientOfThermalCondictivity * Time.deltaTime;

        m_temperature -= exchangedHeat;
        heatedObject.temperature += exchangedHeat;
    }

    private void OnTriggerEnter(Collider other)
    {
        HeatableObject target = other.GetComponent<HeatableObject>();

        if (target == null) return;

        m_heatedObjects.Add(target);
    }

    private void OnTriggerExit(Collider other)
    {
        HeatableObject target = other.GetComponent<HeatableObject>();

        if (target == null) return;

        m_heatedObjects.Remove(target);
    }
}
