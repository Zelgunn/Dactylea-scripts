using UnityEngine;
using System.Collections;

public class NetWater : MonoBehaviour
{
    [SerializeField] private MeshFilter m_meshFilter;

    [Header("Positions")]
    [SerializeField] private Transform m_start;
    [SerializeField] private Transform m_startExt;
    [SerializeField] private Transform m_end;
    [SerializeField] private Transform m_endExt;

    [Header("Paramètres")]
    [SerializeField] private float m_size;
    [Range(3, 360)] [SerializeField] private int m_faceCount;

    private Mesh m_mesh;
    private Color m_color;
    private Renderer m_targetRenderer;
    private bool m_isEnd = false;

    private float m_creationTime;

	private void Awake ()
	{
        m_mesh = new Mesh();
        m_mesh.name = "NetWater";
        m_meshFilter.mesh = m_mesh;
        m_targetRenderer = m_meshFilter.GetComponent<Renderer>();
        m_creationTime = Time.time;
        //m_mesh = m_meshFilter.mesh;
	}
	
    [ContextMenu("Update !")]
	private void LateUpdate ()
	{
        if ((Time.time - m_creationTime) > 2)
        {
            Destroy(gameObject);
            return;
        }

        if (m_mesh == null)
        {
            m_mesh = new Mesh();
            m_mesh.name = "NetWater";
            m_meshFilter.mesh = m_mesh;
        }

        if (!m_start || !m_end)
        {
            Destroy(gameObject);
            return;
        }

        if (!m_isEnd)
            m_endExt.transform.localPosition = new Vector3(0, 0, m_size);
        else
            m_endExt.transform.localPosition = new Vector3(0, 0, m_size / 10);
        m_startExt.transform.localPosition = new Vector3(0, 0, m_size);

        Vector3[] vertices = new Vector3[m_faceCount * 2];
        for (int i = 0; i < m_faceCount; i++)
        {
            vertices[i * 2] = m_startExt.transform.position - transform.position;
            vertices[i * 2 + 1] = m_endExt.transform.position - transform.position;

            m_start.parent.Rotate(Vector3.up, 360.0f / m_faceCount);
            m_end.parent.Rotate(Vector3.up, 360.0f / m_faceCount);

            // m_end.parent.forward m_start.parent.forward

            //m_start.Rotate(360.0f / m_faceCount, 0, 0, Space.Self);
            //m_end.Rotate(360.0f / m_faceCount, 0, 0, Space.Self);
        }

        int[] triangles = new int[vertices.Length * 6];
        for (int i = 0; i < m_faceCount; i++)
        {
            if(i == (m_faceCount - 1))
            {
                triangles[i * 12] =         i * 2;
                triangles[i * 12 + 1] =     0;
                triangles[i * 12 + 2] =     i * 2 + 1;

                triangles[i * 12 + 3] =     0;
                triangles[i * 12 + 4] =     1;
                triangles[i * 12 + 5] =     i * 2 + 1;


                triangles[i * 12 + 6] =     i * 2;
                triangles[i * 12 + 7] =     i * 2 + 1;
                triangles[i * 12 + 8] =     0;

                triangles[i * 12 + 9] =     0;
                triangles[i * 12 + 10] =    i * 2 + 1;
                triangles[i * 12 + 11] =    1;
            }
            else
            {
                triangles[i * 12] =         i * 2;
                triangles[i * 12 + 1] =     i * 2 + 2;
                triangles[i * 12 + 2] =     i * 2 + 1;

                triangles[i * 12 + 3] =     i * 2 + 2;
                triangles[i * 12 + 4] =     i * 2 + 3;
                triangles[i * 12 + 5] =     i * 2 + 1;


                triangles[i * 12 + 6] =     i * 2;
                triangles[i * 12 + 7] =     i * 2 + 1;
                triangles[i * 12 + 8] =     i * 2 + 2;

                triangles[i * 12 + 9] =     i * 2 + 2;
                triangles[i * 12 + 10] =    i * 2 + 1;
                triangles[i * 12 + 11] =    i * 2 + 3;
            }
        }

        m_mesh.Clear();
        m_mesh.vertices = vertices;
        m_mesh.triangles = triangles;

        Vector3[] normals = new Vector3[vertices.Length];

        for (int i = 0; i < m_faceCount; i++)
        {
            normals[i] = -Vector3.forward;
        }

        m_mesh.normals = normals;

        Vector2[] uv = new Vector2[m_faceCount * 2];

        for (int i = 0; i < m_faceCount; i++)
        {
            if(i%2 == 0)
            {
                uv[i * 2] = new Vector2(0, 0);
                uv[i * 2 + 1] = new Vector2(1, 0);
            }
            else
            {
                uv[i * 2] = new Vector2(0, 1);
                uv[i * 2 + 1] = new Vector2(1, 1);
            }
        }

        m_mesh.uv = uv;

        //Color[] colors = new Color[vertices.Length];
        //for (int i = 0; i < vertices.Length; i++)
        //{
        //    colors[i] = m_color;
        //}
        //m_mesh.colors = colors;
	}

    public void SetAttachments(Transform start, Transform end)
    {
        m_start.transform.SetParent(start);
        m_start.transform.localPosition = Vector3.zero;
        m_start.transform.localRotation = Quaternion.identity;
        
        m_end.transform.SetParent(end);
        m_end.transform.localPosition = Vector3.zero;
        m_end.transform.localRotation = Quaternion.identity;
    }

    public float size
    {
        get { return m_size; }
        set { m_size = value; }
    }

    public Color color
    {
        get { return m_color; }
        set
        { 
            m_color = value;
            m_targetRenderer.material.color = m_color;
            m_targetRenderer.material.SetColor("_EmissionColor",m_color/5);
        }
    }

    public bool isEnd
    {
        get { return m_isEnd; }
        set { m_isEnd = value; }
    }
}
