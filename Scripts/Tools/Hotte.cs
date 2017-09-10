using UnityEngine;
using System.Collections;

public class Hotte : MonoBehaviour
{
    [SerializeField] private VitreHotte m_vitre;

    public VitreHotte vitre
    {
        get { return m_vitre; }
    }
}
