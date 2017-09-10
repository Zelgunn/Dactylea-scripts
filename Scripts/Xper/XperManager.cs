using UnityEngine;
using System;
using System.IO;
using System.Xml.Serialization;
using System.Collections.Generic;

public class XperManager : MonoBehaviour
{
    static private XperManager s_singleton;
    private Dictionary<XperItem, XperAction> m_xperFirstIntroduced = new Dictionary<XperItem, XperAction>();

    #region Dico
    #region Actions
    [SerializeField] [HideInInspector] private XperAction m_erlenmeyerAttributions;
    [SerializeField] [HideInInspector] private XperAction m_becherAttributions;
    [SerializeField] [HideInInspector] private XperAction m_waterTapAttributions;
    [SerializeField] [HideInInspector] private XperAction m_pipetteTapAttributions;
    [SerializeField] [HideInInspector] private XperAction m_tubeAttributions;
    [SerializeField] [HideInInspector] private XperAction m_cogWheelAttributions;
    [SerializeField] [HideInInspector] private XperAction m_agitatorButtonAttributions;
    [SerializeField] [HideInInspector] private XperAction m_hodPaneAttributions;
    [SerializeField] [HideInInspector] private XperAction m_corkAttributions;
    [SerializeField] [HideInInspector] private XperAction m_agitatorAttributions;
    [SerializeField] [HideInInspector] private XperAction m_matchAttributions;
    [SerializeField] [HideInInspector] private XperAction m_matchBoxAttributions;
    [SerializeField] [HideInInspector] private XperAction m_spatulaAttributions;
    [SerializeField] [HideInInspector] private XperAction m_instructorButtonAttributions;
    [SerializeField] [HideInInspector] private XperAction m_markerAttributions;
    [SerializeField] [HideInInspector] private XperAction m_markerCorkAttributions;
    #endregion
    #region Portee
    [SerializeField] [HideInInspector] private XperRange m_erlenmeyerRangeMode;
    [SerializeField] [HideInInspector] private XperRange m_becherRangeMode;
    [SerializeField] [HideInInspector] private XperRange m_waterTapRangeMode;
    [SerializeField] [HideInInspector] private XperRange m_pipetteTapRangeMode;
    [SerializeField] [HideInInspector] private XperRange m_tubeRangeMode;
    [SerializeField] [HideInInspector] private XperRange m_cogWheelRangeMode;
    [SerializeField] [HideInInspector] private XperRange m_agitatorButtonRangeMode;
    [SerializeField] [HideInInspector] private XperRange m_hodPaneRangeMode;
    [SerializeField] [HideInInspector] private XperRange m_corkRangeMode;
    [SerializeField] [HideInInspector] private XperRange m_agitatorRangeMode;
    [SerializeField] [HideInInspector] private XperRange m_matchRangeMode;
    [SerializeField] [HideInInspector] private XperRange m_matchBoxRangeMode;
    [SerializeField] [HideInInspector] private XperRange m_spatulaRangeMode;
    [SerializeField] [HideInInspector] private XperRange m_instructorButtonRangeMode;
    [SerializeField] [HideInInspector] private XperRange m_markerRangeMode;
    [SerializeField] [HideInInspector] private XperRange m_markerCorkRangeMode;
    #endregion
    #endregion

    private List<XperEntry> m_entries = new List<XperEntry>();
    private float m_firstEntryTime = 0;

    private void Awake ()
    {
        s_singleton = this;

        XperItem[] items = Enum.GetValues(typeof(XperItem)) as XperItem[];
        XperAction[] types = Enum.GetValues(typeof(XperAction)) as XperAction[];

        List<XperAction> flagedTypes = new List<XperAction>();

        foreach (XperItem item in items)
        {
            flagedTypes.Clear();

            foreach (XperAction type in types)
            {
                XperAction tmp = AttributionsOf(item);
                bool hasFlag = (((int)tmp == -1) || ((tmp & type) > 0));
                if(hasFlag)
                {
                    flagedTypes.Add(type);
                }
            }

            if(flagedTypes.Count > 0)
            {
                m_xperFirstIntroduced.Add(item, flagedTypes[UnityEngine.Random.Range(0, flagedTypes.Count)]);
            }
        }

    }

    private void OnApplicationQuit()
    {
        _SaveEntries();
    }

    private void _SaveEntries()
    {
        if (m_entries.Count == 0) return;

        XmlSerializer xmlSerializer = new XmlSerializer(typeof(List<XperEntry>));

        string filename = "Xper\\Xper_" + DateTime.Now.DayOfYear + "_" + DateTime.Now.Hour + "_" + DateTime.Now.Minute + "_" + DateTime.Now.Second + ".xml";

        using (StreamWriter streamWriter = new StreamWriter(filename))
        {
            xmlSerializer.Serialize(streamWriter, m_entries);
        }

        m_entries.Clear();
    }

    static public void SaveEntries()
    {
        s_singleton._SaveEntries();
    }

    static public bool Allows(XperItem item, XperAction type, XperRange range)
    {
        bool validAction = (s_singleton.AttributionsOf(item) & type) == type;
        bool validRange = (s_singleton.RangeModeOf(item) & range) == range;

        return validAction && validRange;
    }

    static public XperAction FirstInteractionIntroduced(XperItem item)
    {
        return s_singleton.m_xperFirstIntroduced[item];
    }

    #region Attribution Switch
    public void SetAttributionsOf(XperItem item, XperAction type)
    {
        switch (item)
        {
            case XperItem.Erlenmeyer:
                m_erlenmeyerAttributions = type;
                break;
            case XperItem.Becher:
                m_becherAttributions = type;
                break;
            case XperItem.WaterTap:
                m_waterTapAttributions = type;
                break;
            case XperItem.PipetteTap:
                m_pipetteTapAttributions = type;
                break;
            case XperItem.Tube:
                m_tubeAttributions = type;
                break;
            case XperItem.CogWheel:
                m_cogWheelAttributions = type;
                break;
            case XperItem.AgitatorButton:
                m_agitatorButtonAttributions = type;
                break;
            case XperItem.HodPane:
                m_hodPaneAttributions = type;
                break;
            case XperItem.Cork:
                m_corkAttributions = type;
                break;
            case XperItem.Agitator:
                m_agitatorAttributions = type;
                break;
            case XperItem.Match:
                m_matchAttributions = type;
                break;
            case XperItem.MatchBox:
                m_matchBoxAttributions = type;
                break;
            case XperItem.Spatula:
                m_spatulaAttributions = type;
                break;
            case XperItem.InstructorButton:
                m_instructorButtonAttributions = type;
                break;
            case XperItem.Pen:
                m_markerAttributions = type;
                break;
            case XperItem.PenCork:
                m_markerCorkAttributions = type;
                break;
        }
    }

    public XperAction AttributionsOf(XperItem item)
    {
        XperAction res = 0;

        switch (item)
        {
            case XperItem.Erlenmeyer:
                res = m_erlenmeyerAttributions;
                break;
            case XperItem.Becher:
                res = m_becherAttributions;
                break;
            case XperItem.WaterTap:
                res = m_waterTapAttributions;
                break;
            case XperItem.PipetteTap:
                res = m_pipetteTapAttributions;
                break;
            case XperItem.Tube:
                res = m_tubeAttributions;
                break;
            case XperItem.CogWheel:
                res = m_cogWheelAttributions;
                break;
            case XperItem.AgitatorButton:
                res = m_agitatorButtonAttributions;
                break;
            case XperItem.HodPane:
                res = m_hodPaneAttributions;
                break;
            case XperItem.Cork:
                res = m_corkAttributions;
                break;
            case XperItem.Agitator:
                res = m_agitatorAttributions;
                break;
            case XperItem.Match:
                res = m_matchAttributions;
                break;
            case XperItem.MatchBox:
                res = m_matchBoxAttributions;
                break;
            case XperItem.Spatula:
                res = m_spatulaAttributions;
                break;
            case XperItem.InstructorButton:
                res = m_instructorButtonAttributions;
                break;
            case XperItem.Pen:
                res = m_markerAttributions;
                break;
            case XperItem.PenCork:
                res = m_markerCorkAttributions;
                break;
        }

        return res;
    }
    #endregion
    #region Range Switch
    public void SetRangeModeOf(XperItem item, XperRange type)
    {
        switch (item)
        {
            case XperItem.Erlenmeyer:
                m_erlenmeyerRangeMode = type;
                break;
            case XperItem.Becher:
                m_becherRangeMode = type;
                break;
            case XperItem.WaterTap:
                m_waterTapRangeMode = type;
                break;
            case XperItem.PipetteTap:
                m_pipetteTapRangeMode = type;
                break;
            case XperItem.Tube:
                m_tubeRangeMode = type;
                break;
            case XperItem.CogWheel:
                m_cogWheelRangeMode = type;
                break;
            case XperItem.AgitatorButton:
                m_agitatorButtonRangeMode = type;
                break;
            case XperItem.HodPane:
                m_hodPaneRangeMode = type;
                break;
            case XperItem.Cork:
                m_corkRangeMode = type;
                break;
            case XperItem.Agitator:
                m_agitatorRangeMode = type;
                break;
            case XperItem.Match:
                m_matchRangeMode = type;
                break;
            case XperItem.MatchBox:
                m_matchBoxRangeMode = type;
                break;
            case XperItem.Spatula:
                m_spatulaRangeMode = type;
                break;
            case XperItem.InstructorButton:
                m_instructorButtonRangeMode = type;
                break;
            case XperItem.Pen:
                m_markerRangeMode = type;
                break;
            case XperItem.PenCork:
                m_markerCorkRangeMode = type;
                break;
        }
    }

    public XperRange RangeModeOf(XperItem item)
    {
        XperRange res = 0;

        switch (item)
        {
            case XperItem.Erlenmeyer:
                res = m_erlenmeyerRangeMode;
                break;
            case XperItem.Becher:
                res = m_becherRangeMode;
                break;
            case XperItem.WaterTap:
                res = m_waterTapRangeMode;
                break;
            case XperItem.PipetteTap:
                res = m_pipetteTapRangeMode;
                break;
            case XperItem.Tube:
                res = m_tubeRangeMode;
                break;
            case XperItem.CogWheel:
                res = m_cogWheelRangeMode;
                break;
            case XperItem.AgitatorButton:
                res = m_agitatorButtonRangeMode;
                break;
            case XperItem.HodPane:
                res = m_hodPaneRangeMode;
                break;
            case XperItem.Cork:
                res = m_corkRangeMode;
                break;
            case XperItem.Agitator:
                res = m_agitatorRangeMode;
                break;
            case XperItem.Match:
                res = m_matchRangeMode;
                break;
            case XperItem.MatchBox:
                res = m_matchBoxRangeMode;
                break;
            case XperItem.Spatula:
                res = m_spatulaRangeMode;
                break;
            case XperItem.InstructorButton:
                res = m_instructorButtonRangeMode;
                break;
            case XperItem.Pen:
                res = m_markerRangeMode;
                break;
            case XperItem.PenCork:
                res = m_markerCorkRangeMode;
                break;
        }

        return res;
    }
    #endregion

    static public void AddEntry(XperEntry entry)
    {
        if (s_singleton.m_entries.Count == 0) s_singleton.m_firstEntryTime = Time.time;
        entry.m_timeOfEntry -= s_singleton.m_firstEntryTime;
        s_singleton.m_entries.Add(entry);
    }
}
