using UnityEngine;
using System.Collections;
using System;
using System.Xml.Serialization;

#region Enums
[Flags]
public enum XperAction
{
    [XmlEnum("Serrage")]    Grip            = 1,
    [XmlEnum("Gachette")]   Trigger         = 2,
    [XmlEnum("Appuyer")]    TouchPress      = 4,
    [XmlEnum("Toucher")]    Touch           = 8,
    [XmlEnum("Glissement")] Slide           = 16,
    [XmlEnum("Balayage")]   TouchSwipe      = 32,
    [XmlEnum("Cercle")]     Circle          = 64
}

[Flags]
public enum XperRange
{
    Contact = 1,
    Ranged = 2
}

public enum XperItem
{
                                        Erlenmeyer,
                                        Becher,
    [XmlEnum("Robinet")]                WaterTap,
    [XmlEnum("RouePipette")]            PipetteTap,
    [XmlEnum("TubeAEssai")]             Tube,
    [XmlEnum("RouageEstrade")]          CogWheel,
    [XmlEnum("BouttonAgitateur")]       AgitatorButton,
    [XmlEnum("VitreHotte")]             HodPane,
    [XmlEnum("Bouchon")]                Cork,
    [XmlEnum("Agitateur")]              Agitator,
    [XmlEnum("Alumette")]               Match,
    [XmlEnum("BoiteAlumette")]          MatchBox,
    [XmlEnum("Spatule")]                Spatula,
    [XmlEnum("BouttonInstructeur")]     InstructorButton,
    [XmlEnum("Marqueur")]               Pen,
    [XmlEnum("BouchonMarqueur")]        PenCork
}
#endregion

public class XperEntry
{
    public XperAction m_type;
    public XperItem m_item;
    public XperRange m_range;
    public XperAction m_introducedFirst;
    public float m_timeOfEntry;
    public float m_height;

    public XperEntry()
    {

    }

    public XperEntry(XperAction type, XperRange range, XperItem item)
    {
        m_type = type;
        m_item = item;
        m_range = range;
        m_timeOfEntry = Time.time;
        m_introducedFirst = XperManager.FirstInteractionIntroduced(item);
        m_height = StageManager.stageHeight;
    }

     
}
