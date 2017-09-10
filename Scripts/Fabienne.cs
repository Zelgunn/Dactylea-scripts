using UnityEngine;
using System.Collections;

public class Fabienne : MonoBehaviour
{
    [SerializeField] private ViveWand m_rightWand;
    [SerializeField] private ViveWand m_leftWand;

    public void SetViveWandSkins(ViveWand.ViveWandSkins skin)
    {
        m_rightWand.SetSkin(skin);
        m_leftWand.SetSkin(skin);
    }

    public void SetViveWandOnDefaultSkin()
    {
        SetViveWandSkins(ViveWand.ViveWandSkins.Default);
    }

    public void SetViveWandOnHandsSkin()
    {
        SetViveWandSkins(ViveWand.ViveWandSkins.Hand);
    }

    public void SetViveWandOnClampSkin()
    {
        SetViveWandSkins(ViveWand.ViveWandSkins.Clamp);
    }

    public void SetViveWandOnBeamSkin()
    {
        SetViveWandSkins(ViveWand.ViveWandSkins.Beam);
    }
}
