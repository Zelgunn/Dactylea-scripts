using UnityEngine;
using System.Collections;

public class VitreHotte : ViveInteractible
{
    private enum PositionVitre
    {
        EnBas,
        EnDessous,
        AuDessusAccroche,
        Accroche,
        AuDessusDecroche
    }

    private Rigidbody m_rigidbody;
    private Vector3 m_origin;
    [Header("Parametres vitre")]
    [SerializeField] private Transform m_snapPoint;

    [Header("Sounds")]
    [SerializeField] private AudioSource m_leftSource;
    [SerializeField] private AudioSource m_rightSource;

    [Header("Debug")]
    [SerializeField] private PositionVitre m_positionState = PositionVitre.EnBas;

    private bool m_previouslyGriped = false;
    private float m_previousPointHeight = 0;

	private void Awake ()
	{
        m_rigidbody = GetComponent<Rigidbody>();
        m_origin = transform.position;
	}
	
	new private void Update ()
	{
        base.Update();

        #region Update via Manette du Vive

        bool griped = false;

        if(m_previouslyGriped)
        {
            griped = m_interactionInProgress;
        }
        else
        {
            griped = m_interactionInProgress && (m_interactionWand.targetedGameObject == gameObject) && m_interactionDown;
        }

        griped &= (m_positionState != PositionVitre.Accroche);

        if(griped)
        {
            float pointHeight = (m_interactionWand.transform.position + m_interactionWand.transform.forward).y;

            if (m_previouslyGriped)
            {
                Vector3 position = transform.position;
                position.y += pointHeight - m_previousPointHeight;
                transform.position = position;
            }
            else
            {
                ReportInteraction();
            }

            m_previousPointHeight = pointHeight;
        }

        m_previouslyGriped = griped;
        m_rigidbody.useGravity = !griped;
        m_rigidbody.isKinematic = griped;

        #endregion

        #region Machine à état pour la position
        switch (m_positionState)
        {
            case PositionVitre.EnBas:
                if (m_origin.y < transform.position.y)
                {
                    SetEnDessous();
                }
                else
                {
                    transform.position = m_origin;
                }
                break;
            case PositionVitre.EnDessous:
                if (m_origin.y >= transform.position.y)
                {
                    SetEnBas();
                }
                else if (m_snapPoint.transform.position.y <= transform.position.y)
                {
                    SetAuDessusAccroche();
                }
                break;
            case PositionVitre.AuDessusAccroche:
                if (m_snapPoint.transform.position.y > transform.position.y)
                {
                    SetAccroche();
                }
                break;
            case PositionVitre.Accroche:
                if ((m_snapPoint.transform.position.y + 0.075f) < transform.position.y)
                {
                    SetAuDessusDecroche();
                }
                else
                {
                    transform.position = m_snapPoint.position;
                }
                break;
            case PositionVitre.AuDessusDecroche:
                if (m_snapPoint.transform.position.y > transform.position.y)
                {
                    SetEnDessous();
                }
                break;
        }

        // Si EnBas                 -> ø gravity, ø velocity
        //  -> Si passe audessus bas    => EnDessous
        // Si EnDessous             -> gravity
        //  -> Si passe au dessus       => AuDessusAccroche
        //  -> Si passe sous bas        => EnBas
        // Si AuDessusAccroche      -> gravity
        //  -> Si passe en dessous      => Accroche
        // Si Accroche              -> ø gravity, ø velocity
        //  -> Si passe au dessus       => AuDessusDecroche
        // Si AuDessusDecroche      -> gravity
        //  -> Si passe en dessous      => EnDessous
        #endregion

        
    }

    protected override void OnActionDown()
    {
        
    }

    protected override void OnActionUp()
    {
        
        
    }

    private void SetEnBas()
    {
        m_positionState = PositionVitre.EnBas;

        m_rigidbody.velocity = Vector3.zero;
        transform.position = m_origin;
        m_rigidbody.useGravity = false;
    }

    private void SetEnDessous()
    {
        m_positionState = PositionVitre.EnDessous;

        m_rigidbody.useGravity = true;
    }

    private void SetAuDessusAccroche()
    {
        m_positionState = PositionVitre.AuDessusAccroche;

        m_rigidbody.useGravity = true;

        m_leftSource.pitch = 1f;
        m_rightSource.pitch = 1f;
        m_leftSource.PlayOneShot(m_leftSource.clip);
        m_rightSource.PlayOneShot(m_rightSource.clip);
    }

    private void SetAccroche()
    {
        m_positionState = PositionVitre.Accroche;

        transform.position = m_snapPoint.position;
        m_rigidbody.velocity = Vector3.zero;
        m_rigidbody.useGravity = false;
    }

    private void SetAuDessusDecroche()
    {
        m_positionState = PositionVitre.AuDessusDecroche;

        m_rigidbody.useGravity = true;

        m_leftSource.pitch = 0.9f;
        m_rightSource.pitch = 0.9f;
        m_leftSource.PlayOneShot(m_leftSource.clip);
        m_rightSource.PlayOneShot(m_rightSource.clip);
    }
}
