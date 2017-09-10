using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class SmartFluwid : MonoBehaviour
{
    [SerializeField] protected MeshVolumeFormula.MeshID m_meshID = MeshVolumeFormula.MeshID.Erlenmeyer;

    [SerializeField] protected float m_volumePercentage = 0.8f;
    protected float m_fullVolume;
    protected float m_currentVolume;

    protected float m_height;
    protected Polynome2D m_heightFormula;

    protected Vector3[] m_vertices;
    protected Vector3[] m_topVertices;
    protected Vector3 m_topVerticesCenter;
    protected float m_scaleFactor;
    protected float m_topRadius;

    protected float m_baseFlowFactor;
    protected bool m_flowing = false;
    protected bool m_flowBlocked = false;
    protected Vector3 m_flowPoint;
    protected float m_flowFactor;
    protected float m_flowPercentage;

    #region Champs - Rendering

    protected Renderer m_renderer;
    [SerializeField] protected Renderer m_fireRenderer;

    #region Champs - Rendering - Hashcodes
    protected int m_hashLiquidColor;
    protected int m_hashSinDelta;
    protected int m_hashSinFactor;
    protected int m_hashFirstDegree;
    protected int m_hashSecDegree;
    protected int m_hashFluwidHeight;
    #endregion

    [Header("Filet d'eau (apparence)")]
    [SerializeField] protected NetBall m_netBallPrefab;
    [SerializeField] protected NetWater m_waterNetPrefab;
    protected NetBall m_lastNetBall = null;

    protected float m_sinDelta = 0;
    protected float m_livingTime = 1;

    #endregion

    #region Champs - Composants

    protected Compound m_compound;

    [Header("Paramètres liquide")]
    [SerializeField] protected Compound.Elements m_startElement;

    protected Rigidbody m_rigidbody;
    protected float m_baseMass;
    [SerializeField] protected float m_agitation = 0;
    protected float m_heat = 0;

    protected HeatableObject m_heatManager;

    #region Compound - Stack

    [Header("Tas")] [SerializeField]
    private Transform m_stack;
    private float m_stackSize = 0;
    private Vector3 m_baseStackSize;

    #endregion

    #endregion

    #region Champs - Feu

    protected bool m_burning = false;

    #endregion

    protected AudioSource m_audioSource = null;

    protected void Awake()
    {
        LoadMeshFormula();
        InitVertices();
        InitTopVertices();

        m_compound = new Compound(m_startElement, m_currentVolume);
        m_renderer = GetComponent<Renderer>();

        InitHashCodes();
        UpdateHeight();
        UpdateRenderer();

        m_baseFlowFactor = m_topRadius * m_topRadius / m_fullVolume;
        //Debug.Log(name + " : " + m_baseFlowFactor * m_topRadius);
        InitSound();

        m_heatManager = GetComponentInParent<HeatableObject>();

        if (m_stack)
        {
            m_baseStackSize = m_stack.localScale;
            m_stack.gameObject.SetActive(false);
        }
    }

    #region Initialisations

    // Precal
    #region Precalculated Volume-Rotation-Height Datas

    protected void LoadMeshFormula()
    {
        MeshVolumeFormula xmlMeshFormula = XmlMeshFormula.FormulaForMesh(m_meshID);

        if (xmlMeshFormula == null)
        {
            this.enabled = false;
            return;
        }

        m_heightFormula = xmlMeshFormula.formula;
        m_scaleFactor = transform.lossyScale.x * transform.lossyScale.y * transform.lossyScale.z;
        m_fullVolume = xmlMeshFormula.relativeVolume * m_scaleFactor;
        m_currentVolume = m_fullVolume * m_volumePercentage;
    }

    #endregion

    protected void InitHashCodes()
    {
        m_hashLiquidColor = Shader.PropertyToID("_Color");
        m_hashFluwidHeight = Shader.PropertyToID("_FluwidHeight");
        m_hashSinDelta = Shader.PropertyToID("_CurveSinDelta");
        m_hashSinFactor = Shader.PropertyToID("_CurveSinFactor");
        m_hashFirstDegree = Shader.PropertyToID("_CurveFirstDegree");
        m_hashSecDegree = Shader.PropertyToID("_CurveSecDegree");
    }

    virtual protected void InitSound()
    {
        m_audioSource = GetComponent<AudioSource>();
        if (!m_audioSource) return;

        m_audioSource.loop = true;
        m_audioSource.enabled = false;
    }

    #endregion

    protected void Update()
    {
        m_currentVolume = m_compound.totalQuantity;
        m_volumePercentage = m_currentVolume / m_fullVolume;

        UpdateHeight();
        UpdateFlow();
        UpdateCompoundElements();
        UpdateRenderer();
        UpdateSound();
    }

    #region Updates
    virtual protected void UpdateHeight()
    {
        float angle = Vector3.Angle(transform.forward, Vector3.up);
        m_height = m_heightFormula.Calculate(angle, m_currentVolume / m_scaleFactor) * transform.lossyScale.y;
    }

    // Coulure d'eau
    /// <summary>
    /// Cette fonction calcule si l'eau coule du conteneur, et mets à jour les variables m_flowing, m_flowPoint. Nécessite que la variable m_height soit calculée avant.
    /// </summary>
    virtual protected void UpdateFlow()
    {
        if(m_flowBlocked || (m_volumePercentage <= 0))
        {
            m_flowing = false;
            return;
        }

        Vector3 lowestTopPoint, highestTopPoint;
        GetLowestAndHighestTopPoints(out lowestTopPoint, out highestTopPoint);

        if (lowestTopPoint.y > m_height)
        {
            m_flowing = false;
            return;
        }

        m_flowing = true;

        if (highestTopPoint.y == lowestTopPoint.y)
        {
            m_flowPoint = TransformedVertex(m_topVerticesCenter);
        }
        else
        {
            float flowHeightRatio = (m_height - lowestTopPoint.y) / (highestTopPoint.y - lowestTopPoint.y);
            if (flowHeightRatio > 1) flowHeightRatio = 1;

            m_flowPoint = Vector3.Lerp(lowestTopPoint, highestTopPoint, flowHeightRatio);
            m_flowFactor = m_baseFlowFactor * (flowHeightRatio + 0.01f);
        }

        // Sortie physique & compound
        m_flowPercentage = m_flowFactor * Time.deltaTime;

        Compound flowedCompound = new Compound(m_compound, m_fullVolume * m_flowPercentage / 2);
        m_compound -= flowedCompound;

        m_currentVolume = m_compound.totalQuantity;
        m_volumePercentage = m_currentVolume / m_fullVolume;

        Vector3 flowCenter = (lowestTopPoint + m_flowPoint) / 2;
        float flowSize = (flowCenter - lowestTopPoint).magnitude / 2;
        flowCenter += transform.position;

        FlowNet(flowCenter, flowSize, flowedCompound);
    }

    virtual protected void UpdateRenderer()
    {
        if (!m_renderer)
        {
            return;
        }

        if (m_volumePercentage < 0.0001f)
        {
            m_renderer.enabled = false;
        }
        else
        {
            m_renderer.enabled = true;

            float displayHeight = (m_height + transform.position.y);
            m_renderer.material.SetFloat(m_hashFluwidHeight, displayHeight);
            m_renderer.material.SetColor(m_hashLiquidColor, m_compound.color);
            m_renderer.material.color = m_compound.color;
			
			if(m_agitation > 0)
			{
				m_sinDelta += Time.deltaTime * m_agitation * 10;
                m_renderer.material.SetFloat(m_hashSinDelta, m_sinDelta);
                m_renderer.material.SetFloat(m_hashSinFactor, 49 * m_agitation / transform.lossyScale.x + 1);
                m_renderer.material.SetFloat(m_hashFirstDegree, 0.0025f * m_agitation * transform.lossyScale.magnitude);
                m_renderer.material.SetFloat(m_hashSecDegree, 0.0025f * m_agitation * transform.lossyScale.magnitude);
			}
        }
    }

    virtual protected void UpdateSound()
    {
        if (!m_audioSource) return;

        if(m_burning)
        {
            m_audioSource.enabled = true;
            return;
        }

        if(!m_flowing)
        {
            m_audioSource.enabled = false;
        }
        else
        {
            m_audioSource.enabled = true;
            m_audioSource.pitch = 1.25f - 0.3f * (m_currentVolume / m_fullVolume) + m_flowPercentage;
        }
    }
    #endregion

    // Top Vertices : Fonctions qui servent à trouver le point de coulure (potentiel).
    #region Top Vertices 
    protected void InitVertices()
    {
        m_vertices = GetComponent<MeshFilter>().sharedMesh.vertices;
    }


    /// <summary>
    /// Initialise une liste de tous les points faisant partie du haut de l'objet, de son goulot par exemple.
    /// L'objectif est de pouvoir utiliser ces points afin de calculer le point de coulure potentiel.
    /// </summary>
    /// 
    protected void InitTopVertices()
    {
        Vector3[] transformedVertices = TransformedVertices(m_vertices);

        float maxHeight = transformedVertices[0].y;
        float minHeight = maxHeight;

        for(int i = 0; i < transformedVertices.Length; i++)
        {
            float h = transformedVertices[i].y;

            if (h > maxHeight) maxHeight = h;
            if (h < minHeight) minHeight = h;
        }

        float deltaMinMax = maxHeight - minHeight;
        float maxValidDelta = deltaMinMax / 100;

        List<Vector3> tmpTopVertices = new List<Vector3>(transformedVertices.Length / 4);
        m_topVerticesCenter = Vector3.zero;

        for (int i = 0; i < transformedVertices.Length; i++)
        {
            float h = transformedVertices[i].y;

            float deltaH = maxHeight - h;
            if(deltaH <= maxValidDelta)
            {
                tmpTopVertices.Add(m_vertices[i]);
                m_topVerticesCenter += m_vertices[i];
            }
        }

        m_topVertices = tmpTopVertices.ToArray();
        m_topVerticesCenter /= m_topVertices.Length;

        // Init radius
        m_topRadius = 0;
        for (int i = 0; i < m_topVertices.Length; i++)
        {
            float d = (m_topVerticesCenter - m_topVertices[i]).magnitude;

            if (d > m_topRadius) m_topRadius = d;
        }
        m_topRadius *= m_scaleFactor;
    }

    /// <summary>
    /// Cette fonction calcule les points, parmi ceux précedemment trouvés par InitTopVertices(), 
    /// qui sont aux extrêmes en hauteur (le plus haut et le plus) afin de calculer le point de coulure potentiel.
    /// </summary>
    /// <param name="lowestTopPoint"></param>
    /// <param name="highestTopPoint"></param>
    protected void GetLowestAndHighestTopPoints(out Vector3 lowestTopPoint, out Vector3 highestTopPoint)
    {
        Vector3[] transformedTopPoints = TransformedVertices(m_topVertices);

        int minHeightIndex = 0, maxHeightIndex = 0;
        float minHeight = transformedTopPoints[maxHeightIndex].y;
        float maxHeight = minHeight;

        for(int i = 0; i < transformedTopPoints.Length; i++)
        {
            float h = transformedTopPoints[i].y;

            if(h > maxHeight)
            {
                maxHeight = h;
                maxHeightIndex = i;
            }
            
            if(h < minHeight)
            {
                minHeight = h;
                minHeightIndex = i;
            }
        }

        lowestTopPoint = transformedTopPoints[minHeightIndex];
        highestTopPoint = transformedTopPoints[maxHeightIndex];
    }
    #endregion

    #region Vertex Transform
    protected Vector3[] TransformedVertices(Vector3[] vertices)
    {
        Vector3[] result = new Vector3[vertices.Length];

        for (int i = 0; i < vertices.Length; i++)
        {
            result[i] = TransformedVertex(vertices[i]);
        }

        return result;
    }

    protected Vector3 TransformedVertex(Vector3 vertex)
    {
        vertex.Scale(transform.lossyScale);
        vertex = transform.rotation * vertex;

        return vertex;
    }
    #endregion

    #region Compound

    private void UpdateMercuryThiocyanateStack(float mercuryThiocyanateQuantityDelta)
    {
        if (mercuryThiocyanateQuantityDelta < 0) mercuryThiocyanateQuantityDelta = 0;

        m_stackSize += mercuryThiocyanateQuantityDelta / m_fullVolume * 2;
        m_stackSize = Mathf.Min(1, m_stackSize);

        if (m_stackSize > 0)
        {
            m_stack.gameObject.SetActive(true);
            m_stack.localScale = m_baseStackSize * m_stackSize;

            if (m_currentVolume == 0)
            {
                float directionFactor = Vector3.Project(transform.forward, Vector3.down).magnitude;
                directionFactor *= Vector3.Dot(transform.forward, Vector3.down);
                directionFactor = Mathf.Max(0, directionFactor);

                float delta = Time.deltaTime * directionFactor;
                m_stackSize -= delta;

                m_stackSize = Mathf.Max(m_stackSize, 0);

                if (MercuryThiocyanateCup.Contains(this))
                {
                    MercuryThiocyanateCup.AddToStack(delta);
                }
            }
        }
    }

    public void AddCompound(Compound compound)
    {
        m_compound += compound;

        m_currentVolume = m_compound.totalQuantity;
        m_volumePercentage = m_currentVolume / m_fullVolume;

        float ratio = m_currentVolume / m_fullVolume;
        if (ratio > 1)
        {
            m_compound /= ratio;
            m_volumePercentage = 1;
            m_currentVolume = m_fullVolume;
        }
    }

    private void UpdateCompoundElements()
    {
        float mercuryThiocyanateQuantityBefore = m_compound.ElementQuantity(Compound.Elements.MercuryThiocyanate);

        if (m_heatManager)
        {
            m_heat = m_heatManager.temperature;
            m_agitation = m_heatManager.agitation;
            m_compound.UpdateCompound(m_heat, m_agitation, Time.deltaTime);
        }
        else
        {
            m_compound.UpdateCompound(0, 0, Time.deltaTime);
        }

        if (m_stack)
        {
            float mercuryThiocyanateQuantityAfter = m_compound.ElementQuantity(Compound.Elements.MercuryThiocyanate);
            UpdateMercuryThiocyanateStack(mercuryThiocyanateQuantityAfter - mercuryThiocyanateQuantityBefore);
        }
    }

    #endregion

    #region Flow

    private void FlowNet(Vector3 flowCenter, float flowSize, Compound flowedCompound)
    {
        NetBall ball = Instantiate<NetBall>(m_netBallPrefab);
        Destroy(ball.gameObject, m_livingTime);
        ball.transform.position = flowCenter;
        ball.containedCompound = flowedCompound;
        ball.creator = this;

        float pressure = 0.5f - transform.forward.y / 2;
        pressure = Mathf.Log(10 * pressure * pressure + 1);

        ball.GetComponent<Rigidbody>().AddForce(transform.forward * 10 * pressure);
        pressure = Mathf.Max(Mathf.Min(pressure / Mathf.Log(11), 1), 0.1f);

        ball.transform.LookAt(transform.position - transform.forward);

        #region Net - Connexion vers la prochaine balle

        if (!m_lastNetBall)
        {
            m_lastNetBall = ball;
        }
        // Si on a deux gouttes à la suite, on dessine un filet d'eau.
        else
        {
            NetWater waterNet = Instantiate<NetWater>(m_waterNetPrefab);
            waterNet.SetAttachments(m_lastNetBall.transform, ball.transform);
            waterNet.color = m_compound.color;

            m_lastNetBall = ball;
            waterNet.size = flowSize;
        }

        #endregion
    }

    #endregion

    #region Feu

    public void BurnEthanol()
    {
        StartCoroutine(BurnEthanolCoroutine());
    }

    private IEnumerator BurnEthanolCoroutine()
    {
        m_fireRenderer.gameObject.SetActive(true);
        m_burning = true;

        float timeToBurn = m_compound.ElementQuantity(Compound.Elements.Ethanol) / m_fullVolume * 10;
        float currentTime = 0;

        AudioClip baseClip = m_audioSource.clip;
        m_audioSource.clip = SimulationData.fireLoop;
        m_audioSource.pitch = 1;
        m_audioSource.volume /= 2;

        while (currentTime < timeToBurn)
        {
            m_fireRenderer.material.SetFloat(m_hashFluwidHeight, m_renderer.material.GetFloat(m_hashFluwidHeight));

            m_compound -= new Compound(Compound.Elements.Ethanol, m_compound.ElementQuantity(Compound.Elements.Ethanol) / timeToBurn * Time.deltaTime);
            yield return new WaitForEndOfFrame();
            currentTime += Time.deltaTime;
        }

        m_compound -= new Compound(Compound.Elements.Ethanol, m_compound.ElementQuantity(Compound.Elements.Ethanol));
        m_fireRenderer.gameObject.SetActive(false);

        m_audioSource.volume *= 2;
        m_audioSource.clip = baseClip;

        m_burning = false;
    }

    #endregion

    #region Accesseurs

    public bool burning
    {
        get { return m_burning; }
    }

    public float fluidHeight
    {
        get { return m_height; }
    }

    public float currentVolume
    {
        get { return m_currentVolume; }
    }

    public Compound compound
    {
        get { return m_compound; }
    }

    public bool flowBlocked
    {
        get { return m_flowBlocked; }
        set { m_flowBlocked = value; }
    }

    public float fullVolume
    {
        get { return m_fullVolume; }
    }

    public float stackSize
    {
        get { return m_stackSize; }
    }

    public Vector3 GetTopCenterWorldPosition()
    {
        return TransformedVertex(m_topVerticesCenter) + transform.position;
    }

    public MeshVolumeFormula.MeshID meshID
    {
        get { return m_meshID; }
    }

    public bool empty
    {
        get { return m_currentVolume == 0; }
    }

    #endregion
}