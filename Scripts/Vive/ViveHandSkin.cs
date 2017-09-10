using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class ViveHandSkin : MonoBehaviour
{
    private ViveWand m_viveWand;
    //[SerializeField][Range(0, 1)] private float m_trigger;
    [SerializeField] private Transform m_openedHandBonesRoot;
    [SerializeField] private Transform m_closedHandBonesRoot;

    private Dictionary<string, Vector3> m_closedPositions = new Dictionary<string,Vector3>();
    private Dictionary<string, Quaternion> m_closedRotations = new Dictionary<string,Quaternion>();

    private Dictionary<string, Vector3> m_openedPositions = new Dictionary<string, Vector3>();
    private Dictionary<string, Quaternion> m_openedRotations = new Dictionary<string, Quaternion>();

    private void Awake()
    {
        m_viveWand = GetComponentInParent<ViveWand>();
    }

	private void Start ()
	{
	    foreach(Transform t in m_openedHandBonesRoot)
        {
            RecursiveAddChild(t, true);
        }

        foreach (Transform t in m_closedHandBonesRoot)
        {
            RecursiveAddChild(t, false);
        }
	}

    private void RecursiveAddChild(Transform t, bool opened)
    {
        if(opened)
        {
            m_openedPositions.Add(t.name, t.localPosition);
            m_openedRotations.Add(t.name, t.localRotation);
        }
        else
        {
            m_closedPositions.Add(t.name, t.localPosition);
            m_closedRotations.Add(t.name, t.localRotation);
        }

        if(t.childCount > 0)
        {
            RecursiveAddChild(t.GetChild(0), opened);
        }
    }

	private void Update ()
	{
	    foreach(Transform t in m_closedHandBonesRoot)
        {
            RecursiveUpdateChild(t);
        }
	}

    private void RecursiveUpdateChild(Transform t)
    {
        t.localPosition = Vector3.Lerp(m_openedPositions[t.name], m_closedPositions[t.name], m_viveWand.controller.GetAxis(Valve.VR.EVRButtonId.k_EButton_SteamVR_Trigger).x);
        t.localRotation = Quaternion.Slerp(m_openedRotations[t.name], m_closedRotations[t.name], m_viveWand.controller.GetAxis(Valve.VR.EVRButtonId.k_EButton_SteamVR_Trigger).x);

        if (t.childCount > 0)
        {
            RecursiveUpdateChild(t.GetChild(0));
        }
    }
}
