using UnityEngine;
using System.Collections;

public class ViveHook : MonoBehaviour
{
    private ViveWand m_viveWand;

    [SerializeField] private Transform m_rightHook;
    [SerializeField] private Transform m_leftHook;
    //[SerializeField] [Range(0,1)] private float m_trigger;

    private Quaternion m_baseRightRotation;
    private Quaternion m_baseLeftRotation;
	
    private void Awake()
    {
        m_viveWand = GetComponentInParent<ViveWand>();
        m_baseRightRotation = m_rightHook.localRotation;
        m_baseLeftRotation = m_leftHook.localRotation;
    }

	private void Update ()
	{
        m_rightHook.localRotation = m_baseRightRotation * Quaternion.Euler(0, 0, -m_viveWand.controller.GetAxis(Valve.VR.EVRButtonId.k_EButton_SteamVR_Trigger).x * 70);
        m_leftHook.localRotation = m_baseLeftRotation * Quaternion.Euler(0, 0, m_viveWand.controller.GetAxis(Valve.VR.EVRButtonId.k_EButton_SteamVR_Trigger).x * 70);
	}
}
