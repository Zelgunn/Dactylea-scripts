using UnityEngine;
using System.Collections.Generic;

public class ViveWand : MonoBehaviour
{
    #region Private Static members
    static private List<ViveWand> s_viveWands = new List<ViveWand>();
    #endregion

    #region Private members
    // SteamVR inputs
    private SteamVR_TrackedObject m_wand;
    private SteamVR_Controller.Device m_controller;

    // Objets éligibles
    private List<GameObject> m_closeItems = new List<GameObject>();
    private GameObject m_closestObject = null;

    private GameObject m_targetedGameObject = null;
    private RaycastHit m_lastHit;

    // Interaction Flags
    private XperAction m_interactionsPressed = 0;
    private XperAction m_interactionsDown = 0;
    private XperAction m_interactionsUp = 0;

    // Phys
    private SphereCollider m_collider;

    // Update flag
    private bool m_upToDate = false;

    // Touch
    private bool m_touched = false;
    private bool m_previouslyTouched = false;
     
    private Vector2 m_touchPosition;
    private Vector2 m_previousTouchPosition;
    private Vector2 m_touchDelta; // Evite le calcul plusieurs fois

    // Rotation
    private Quaternion m_previousRotation;
    private Quaternion m_rotationDelta;

    // Skins
    #region Enum Skins
    public enum ViveWandSkins
    {
        Default,
        Hand,
        Beam,
        Clamp
    }
    [SerializeField] private ViveWandSkins m_skin;
    #endregion
    [SerializeField] private SteamVR_RenderModel m_defaultSkin;
    [SerializeField] private ViveHandSkin m_handSkin;
    [SerializeField] private ViveBeam m_beamSkin;
    [SerializeField] private ViveHook m_clampSkin;
    private LineRenderer m_lineRenderer;
    #endregion

    #region Awake / Updates
    private void Awake()
    {
        m_collider = GetComponent<SphereCollider>();
        m_wand = GetComponent<SteamVR_TrackedObject>();
        m_lineRenderer = GetComponent<LineRenderer>();

        s_viveWands.Add(this);

        m_previousRotation = transform.rotation;

        SetSkin(m_skin);
    }

    private void Update()
    {
        if (m_upToDate) return;
        m_upToDate = true;

        m_rotationDelta = transform.rotation * Quaternion.Inverse(m_previousRotation); ;
        m_previousRotation = transform.rotation;


        if (m_controller == null)
        {
            m_controller = SteamVR_Controller.Input((int)m_wand.index);
            return;
        }

        #region Objets "ciblés"
        #region Targeted Object Update
        RaycastHit hit;
        if(Physics.Raycast(transform.position, transform.forward, out hit))
        {
            m_targetedGameObject = hit.collider.gameObject;

            Hotte hotte = m_targetedGameObject.GetComponent<Hotte>();
            if(hotte)
            {
                m_targetedGameObject = hotte.vitre.gameObject;
            }

            m_lastHit = hit;
        }
        else
        {
            m_targetedGameObject = null;
        }
        #endregion
        #region Closest Object Update

        GameObject closestItem = null;
        float closestItemDistance = 99999;

        List<GameObject> lostItems = new List<GameObject>();

        foreach (GameObject item in m_closeItems)
        {
            if (!item)
            {
                lostItems.Add(item);
                continue;
            }

            float tmp = (m_collider.bounds.center - item.transform.position).magnitude;
            if (tmp < closestItemDistance)
            {
                closestItemDistance = tmp;
                closestItem = item;
            }
        }

        foreach (GameObject item in lostItems)
        {
            m_closeItems.Remove(item);
        }

        m_closestObject = closestItem;

        //if (closestItem)
        //{
        //    if (closestItem.itemType == XperItem.Cork)
        //    {
        //        Cork cork = closestItem.GetComponent<Cork>();
        //        if (!cork.released)
        //        {
        //            VivePickable corkParent = cork.transform.parent.parent.GetComponent<VivePickable>();
        //            if (!corkParent.picked)
        //                closestItem = corkParent;
        //        }
        //    }

        //    closestItem.Grip(transform);
        //    XperManager.AddEntry(new XperEntry(XperAction.Grip, closestItem.itemType));
        //    m_pickedItem = closestItem;
        //}

        #endregion
        #endregion

        #region Buttons
        #region Trigger button update
        if (m_controller.GetPress(Valve.VR.EVRButtonId.k_EButton_SteamVR_Trigger))
        {
            OnTriggerButton();
        }

        if (m_controller.GetPressDown(Valve.VR.EVRButtonId.k_EButton_SteamVR_Trigger))
        {
            OnTriggerButtonDown();
        }

        if (m_controller.GetPressUp(Valve.VR.EVRButtonId.k_EButton_SteamVR_Trigger))
        {
            OnTriggerButtonUp();
        }
        #endregion
        #region Grip button update

        if (m_controller.GetPress(Valve.VR.EVRButtonId.k_EButton_Grip))
        {
            OnGripButton();
        }

        if (m_controller.GetPressDown(Valve.VR.EVRButtonId.k_EButton_Grip))
        {
            OnGripButtonDown();
        }

        if (m_controller.GetPressUp(Valve.VR.EVRButtonId.k_EButton_Grip))
        {
            OnGripButtonUp();
        }

        #endregion
        #region Touch update

        #region Touch only
        if (m_controller.GetTouch(Valve.VR.EVRButtonId.k_EButton_Axis0))
        {
            OnTouch();
        }

        if (m_controller.GetTouchDown(Valve.VR.EVRButtonId.k_EButton_Axis0))
        {
            OnTouchDown();
        }

        if (m_controller.GetTouchUp(Valve.VR.EVRButtonId.k_EButton_Axis0))
        {
            OnTouchUp();
        }
        #endregion
        #region Touch + Press
        if (m_controller.GetPress(Valve.VR.EVRButtonId.k_EButton_Axis0))
        {
            OnTouchPress();
        }

        if (m_controller.GetPressDown(Valve.VR.EVRButtonId.k_EButton_Axis0))
        {
            OnTouchPressDown();
        }

        if (m_controller.GetPressUp(Valve.VR.EVRButtonId.k_EButton_Axis0))
        {
            OnTouchPressUp();
        }
        #endregion

        #endregion
        #endregion
    }

    private void LateUpdate()
    {
        m_upToDate = false;
        m_previouslyTouched = m_touched;
        m_previousTouchPosition = m_touchPosition;
    }

    public void ForceUpdate()
    {
        if(!gameObject.activeSelf)
        {
            return;
        }

        Update();
    }

    #endregion

    #region OnTrigger Enter/Exit

    private void OnTriggerEnter(Collider other)
    {
        if (!m_closeItems.Contains(other.gameObject) && EligibleToGripping(other.gameObject))
        {
            m_closeItems.Add(other.gameObject);

            if ((m_skin == ViveWandSkins.Beam) && (other.GetComponent<ViveWaterTap>()))
            {
                m_beamSkin.SelectItem(other.transform);
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (m_closeItems.Contains(other.gameObject))
        {
            m_closeItems.Remove(other.gameObject);
            if(m_closestObject == other.gameObject)
            {
                m_closestObject = null;
            }
        }
    }

    private bool EligibleToGripping(GameObject item)
    {
        if (item.GetComponent<ViveInteractible>()) return true;

        return false;
    }

    #endregion

    #region Buttons functions
    #region OnTriggerButton functions
    private void OnTriggerButtonDown()
    {
        m_interactionsPressed |= XperAction.Trigger;
        m_interactionsDown |= XperAction.Trigger;
    }

    private void OnTriggerButtonUp()
    {
        m_interactionsPressed &= ~XperAction.Trigger;
        m_interactionsUp |= XperAction.Trigger;
    }

    private void OnTriggerButton()
    {
        m_interactionsDown &= ~XperAction.Trigger;
        m_interactionsUp &= ~XperAction.Trigger;
    }
    #endregion
    #region OnGripButton functions
    private void OnGripButtonDown()
    {
        m_interactionsPressed |= XperAction.Grip;
        m_interactionsDown |= XperAction.Grip;
    }

    private void OnGripButtonUp()
    {
        m_interactionsPressed &= ~XperAction.Grip;
        m_interactionsUp |= XperAction.Grip;
    }

    private void OnGripButton()
    {
        m_interactionsDown &= ~XperAction.Grip;
        m_interactionsUp &= ~XperAction.Grip;
    }
    #endregion
    #region OnTouchPad functions
    private void OnTouchDown()
    {
        m_interactionsPressed |= XperAction.Touch;
        m_interactionsDown |= XperAction.Touch;
        m_touched = true;
    }

    private void OnTouchUp()
    {
        m_interactionsPressed &= ~XperAction.Touch;
        m_interactionsUp |= XperAction.Touch;
        m_touched = false;
    }

    private void OnTouch()
    {
        m_interactionsDown &= ~XperAction.Touch;
        m_interactionsUp &= ~XperAction.Touch;

        m_touchPosition = m_controller.GetAxis(Valve.VR.EVRButtonId.k_EButton_Axis0);

        if (m_previouslyTouched)
        {
            m_touchDelta = m_touchPosition - m_previousTouchPosition;
        }
    }
    #endregion
    #region OnTouchPress functions
    private void OnTouchPressDown()
    {
        m_interactionsPressed |= XperAction.TouchPress;
        m_interactionsDown |= XperAction.TouchPress;
    }

    private void OnTouchPressUp()
    {
        m_interactionsPressed &= ~XperAction.TouchPress;
        m_interactionsUp |= XperAction.TouchPress;
    }

    private void OnTouchPress()
    {
        m_interactionsDown &= ~XperAction.TouchPress;
        m_interactionsUp &= ~XperAction.TouchPress;
    }
    #endregion
    #endregion

    #region Accesseurs

    static public ViveWand Wand(int index)
    {
        if (index >= s_viveWands.Count)
            return null;

        return s_viveWands[index];
    }

    static public int wandCount
    {
        get { return s_viveWands.Count; }
    }

    public GameObject closestObject
    {
        get { return m_closestObject; }
    }

    public bool IsCloseTo(GameObject item)
    {
        return m_closeItems.Contains(item);
    }

    public GameObject targetedGameObject
    {
        get { return m_targetedGameObject; }
    }

    public RaycastHit lastHit
    {
        get { return m_lastHit; }
    }

    public bool IsInteractionPressed(XperAction action)
    {
        return (m_interactionsPressed & action) == action;
    }

    public bool InteractionDown(XperAction action)
    {
        return (m_interactionsDown & action) == action;
    }

    public bool InteractionUp(XperAction action)
    {
        return (m_interactionsUp & action) == action;
    }

    public SteamVR_Controller.Device controller
    {
        get { return m_controller; }
    }

    public bool upToDate
    {
        get { return m_upToDate; }
    }

    public bool touched
    {
        get { return m_touched; }
    }

    public bool previouslyTouched
    {
        get { return m_previouslyTouched; }
    }

    public Vector2 touchPosition
    {
        get { return m_touchPosition; }
    }

    public Vector2 touchDelta
    {
        get { return m_touchDelta; }
    }

    public float AngleDeltaAroundAxis(Vector3 axis)
    {
        float angle = Vector3.Project(m_rotationDelta.eulerAngles, axis).magnitude;

        while (angle >= 180) angle -= 360;
        while (angle <= -180) angle += 360;
        angle = Mathf.Max(Mathf.Min(angle, 5), -5);

        return angle;
    }

    #endregion

    #region Skins

    public void SetSkin(ViveWandSkins skin)
    {
        m_defaultSkin.gameObject.SetActive( skin == ViveWandSkins.Default  );
        m_handSkin.gameObject.SetActive(    skin == ViveWandSkins.Hand     );
        m_beamSkin.gameObject.SetActive(    skin == ViveWandSkins.Beam     );
        m_clampSkin.gameObject.SetActive(   skin == ViveWandSkins.Clamp    );

        m_lineRenderer.enabled =    (skin != ViveWandSkins.Beam     );
    }

    public ViveWandSkins skin
    {
        get { return m_skin; }
    }

    #endregion
}
