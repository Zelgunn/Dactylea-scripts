using UnityEngine;
using System.Collections;

public class MercuryThiocyanateStack : MonoBehaviour
{
    [SerializeField] private GameObject m_serpentDePharaon;
    [SerializeField] private SmartFluwid m_cupFluwidManager;
    [SerializeField] private Mesh[] m_meshesByStage;
    private MeshCollider m_meshCollider;
    private MeshFilter m_meshFilter;
    private MeshRenderer m_meshrenderer;
    private int m_currentStage = 0;

    private float m_spatuleInTime;

    private float m_wetness = 1.0f;
    private float m_stackSize = 0;
    private Vector3 m_stackBaseScale;

    private float m_startTime;

	private void Awake ()
	{
        m_meshCollider = GetComponent<MeshCollider>();
        m_meshFilter = GetComponent<MeshFilter>();
        m_meshrenderer = GetComponent<MeshRenderer>();

        m_meshFilter.mesh = m_meshesByStage[0];
        m_meshCollider.sharedMesh = m_meshesByStage[0];

        m_spatuleInTime = Time.deltaTime;
        m_stackBaseScale = transform.localScale;
        transform.localScale = Vector3.zero;
	}

    private void Start()
    {
        m_startTime = Time.time;
    }

    private void Update()
    {
        m_meshrenderer.material.color = new Color(1 - m_wetness / 2, 1 - m_wetness / 2, 1 - m_wetness / 2);
        m_wetness = Mathf.Max(m_wetness - Time.deltaTime / 10, 0);
        if(m_cupFluwidManager.currentVolume > 0)
        {
            m_wetness = Mathf.Min(m_wetness + Time.deltaTime / 5, 1);
        }
    }
	
    private IEnumerator ShowPharaonSnakeCoroutine()
    {
        m_serpentDePharaon.SetActive(true);

        yield return new WaitForSeconds(5);

        float timeRating = Time.time - m_startTime;
        if (timeRating <= 180) timeRating = 10;
        else
        {
            timeRating = (900 - timeRating) / 720;
            if (timeRating < 0) timeRating = 0;
        }

        float wasteRating = (10 / (Watcher.WastedCompoundQuantity() / 500 + 1) - 3.33f) * 3 / 2;
        wasteRating = Mathf.Max(0, Mathf.Min(10, wasteRating));

        EndSimulationUI.ShowRatings(timeRating, m_stackSize * 10, wasteRating);
        Instructor.PlaySuccessfulEndSounds();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (m_serpentDePharaon.activeSelf) return;

        if ((other.name == "Spatule") && ((Time.time - m_spatuleInTime) > 1))
        {
            if(m_currentStage < (m_meshesByStage.Length - 1))
            {
                m_currentStage++;
                m_meshFilter.mesh = m_meshesByStage[m_currentStage];
                m_meshCollider.sharedMesh = m_meshesByStage[m_currentStage];
            }

            m_spatuleInTime = Time.time;
        }
        else
        {
            Match match = other.GetComponent<Match>();
            if (match && match.isLighted && ((m_currentStage + 1) == m_meshesByStage.Length) && (m_wetness == 0))
            {
                StartCoroutine(ShowPharaonSnakeCoroutine());
            }
        }
    }

    public void IncreaseStackSize(float delta)
    {
        m_stackSize += delta;

        m_stackSize = Mathf.Min(m_stackSize, 1);

        transform.localScale = m_stackBaseScale * m_stackSize;
    }

    public float stackSize
    {
        get { return m_stackSize; }
    }
}
