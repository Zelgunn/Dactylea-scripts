using UnityEngine;
using System.Collections;

public class ViveModelSelector : MonoBehaviour
{
    [SerializeField] ViveWand.ViveWandSkins m_relatedSkin;

    private void OnTriggerEnter(Collider other)
    {
        ViveWand viveWand = other.GetComponent<ViveWand>();

        if(viveWand)
        {
            viveWand.SetSkin(m_relatedSkin);
        }
    }
}
