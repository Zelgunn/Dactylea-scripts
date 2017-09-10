using UnityEngine;
using System;
using System.Collections.Generic;

public class ViveInteractible : MonoBehaviour
{
    [Header("Interactions Vive")]
    [SerializeField] protected XperItem m_itemType;

    protected bool m_interactionInProgress = false;
    protected ViveWand m_interactionWand = null;
    protected XperAction m_interactionActionType = 0;
    protected XperRange m_interactionRange = 0;

    protected bool m_interactionDown = false;
    protected bool m_interactionUp = false;
    protected bool m_interacting = false;

    [SerializeField] protected bool m_keepFocus = false;
    protected bool m_previouslyHadFocus = false;

    protected bool m_isAtRangeOfWand = false;

    protected void Update ()
    {
        ViveWand previousInteractionWand = m_interactionWand;
        m_interactionInProgress = InteractsWithWand(out m_interactionWand, out m_interactionActionType, out m_interactionRange);

        if (m_interactionInProgress)
        {
            m_interactionDown = m_interactionWand.InteractionDown(m_interactionActionType);
            if (m_interactionDown)
            {
                m_interacting = true;
            }

            m_interactionUp = false;

            if(IsSimpleAction(m_interactionActionType))
            {
                OnAction();

                if (m_interactionDown)
                {
                    OnActionDown();
                }

                OnTurnAction(m_interactionWand.AngleDeltaAroundAxis(transform.forward));
            }

            if(m_interactionWand.previouslyTouched)
            {
                OnMoveAction(m_interactionWand.touchDelta);
            }
        }
        else
        {
            m_interactionDown = false;
            if(previousInteractionWand)
            {
                m_interactionUp = previousInteractionWand.InteractionUp(m_interactionActionType);

                if (m_interactionUp)
                {
                    m_interacting = false;
                    OnActionUp();
                }
            }
            else
            {
                m_interactionUp = false;
            }
        }

        m_previouslyHadFocus = m_interactionInProgress;
    }

    #region Fonctions virtuelles
    #region Actions simples

    virtual protected void OnAction()
    {

    }

    virtual protected void OnActionDown()
    {

    }


    virtual protected void OnActionUp()
    {

    }

    #endregion

    virtual protected void OnMoveAction(Vector2 delta)
    {

    }

    virtual protected void OnTurnAction(float angleDelta)
    {

    }
    #endregion

    #region Verifications
    private bool InteractsWithWand(out ViveWand interactionWand, out XperAction mode, out XperRange range)
    {
        int wandCount = ViveWand.wandCount;
        XperAction activationMode = 0;
        XperRange activationRange = 0;
        ViveWand gripingWand = null;

        m_isAtRangeOfWand = false;

        List<ViveWand> wands = new List<ViveWand>();

        for (int i = 0; i < wandCount; i++)
        {
            wands.Add(ViveWand.Wand(i));
        }

        if(m_interactionWand && wands.Contains(m_interactionWand))
        {
            if(wands[0] != m_interactionWand)
            {
                int indexOfInteractionWand = wands.IndexOf(m_interactionWand);
                ViveWand tmp = wands[0];
                wands[0] = m_interactionWand;
                wands[indexOfInteractionWand] = tmp;
            }
        }

        foreach(ViveWand wand in wands)
        {
            if (!wand || !wand.gameObject.activeSelf) continue;

            if (!wand.upToDate) wand.ForceUpdate();

            XperAction[] actions = Enum.GetValues(typeof(XperAction)) as XperAction[];
            XperRange[] ranges = Enum.GetValues(typeof(XperRange)) as XperRange[];

            foreach(XperRange tmpRange in ranges)
            {
                foreach(XperAction tmpAction in actions)
                {
                    bool isAtRangeOfTestedWand = CheckAtRange(wand, tmpRange);
                    m_isAtRangeOfWand |= isAtRangeOfTestedWand;
                    if ((isAtRangeOfTestedWand || hasFocus) && CheckWandAction(wand, tmpAction, tmpRange))
                    {
                        activationMode = tmpAction;
                        activationRange = tmpRange;
                        gripingWand = wand;

                        break;
                    }
                }

                if (gripingWand) break;
            }

            if (gripingWand) break;
        }

        range = activationRange;
        interactionWand = gripingWand;
        mode = activationMode;

        return gripingWand != null;
    }

    private bool CheckWandAction(ViveWand wand, XperAction action, XperRange range)
    {
        return wand.IsInteractionPressed(action) && XperManager.Allows(m_itemType, action, range);
    }

    private bool CheckAtRange(ViveWand wand, XperRange range)
    {
        switch(range)
        {
            case XperRange.Contact:
                return wand.closestObject == gameObject;
            case XperRange.Ranged:
                return (wand.targetedGameObject == gameObject) && (wand.skin != ViveWand.ViveWandSkins.Beam);
            case (XperRange.Contact | XperRange.Ranged):
                return CheckAtRange(wand, XperRange.Contact) || CheckAtRange(wand, XperRange.Ranged);
        }

        return false;
    }
    #endregion

    static public bool IsSimpleAction(XperAction action)
    {
        if ((action & XperAction.Trigger)   == XperAction.Trigger)      return true;
        if ((action & XperAction.TouchPress)== XperAction.TouchPress)   return true;
        if ((action & XperAction.Grip)      == XperAction.Grip)         return true;

        return false;
    }

    #region Xper
    protected void ReportInteraction()
    {
        if (!m_interactionDown) return;
        XperManager.AddEntry(new XperEntry(m_interactionActionType, m_interactionRange, m_itemType));
    }
    #endregion

    #region Accesseurs
    public bool hasFocus
    {
        get { return m_previouslyHadFocus && m_keepFocus; }
    }
    #endregion
}
