using UnityEngine;
using System.Collections;

public class VolumeDataGatherer : MonoBehaviour
{
    [SerializeField] MeshVolumeFormula.MeshID m_meshID = MeshVolumeFormula.MeshID.Erlenmeyer;
    [SerializeField] private bool m_forceUpdate = false;
    [SerializeField] [Range(3, 25)] private int m_anglesCount = 10;
    [SerializeField] [Range(3, 25)] private int m_volumesCount = 10;
    [SerializeField] [Range(1, 1000)] private int m_heightAccuracy = 10;
    [SerializeField] private float m_clearThreshold = 0.0001f;

    private MeshRenderer m_meshRenderer;
    private MeshFilter m_meshFilter;
    private Mesh m_mesh;
    private Vector3[] m_vertices;
    private int[] m_triangles;

    private float m_fullVolume;

    private float[][] m_datas;
    private float[] m_angles;
    private float[] m_volumes;

	private void Awake ()
    {
        m_meshFilter = GetComponent<MeshFilter>();
        m_meshRenderer = GetComponent<MeshRenderer>();

        m_mesh = m_meshFilter.sharedMesh;

        m_vertices = m_mesh.vertices;
        m_triangles = m_mesh.triangles;

        Vector3[] transformedVertices = TransformedVertices(m_vertices);
        m_fullVolume = CalculateVolume(transformedVertices, m_triangles);

        MainProcess();
	}

    [ContextMenu("Reprocess")]
    private void MainProcess()
    {
        StartCoroutine(MainProcessCoroutine(0.001f));
    }

    private IEnumerator MainProcessCoroutine(float waitTime)
    {
        float anglePace = 180 / (m_anglesCount - 1);
        float volumePace = m_fullVolume / (m_volumesCount - 1);

        float minH = 9999, maxH = -9999;

        m_angles = new float[m_anglesCount];
        m_volumes = new float[m_volumesCount];
        m_datas = new float[m_anglesCount][];

        for (int i = 0; i < m_anglesCount; i++)
        {
            m_datas[i] = new float[m_volumesCount];
            m_angles[i] = anglePace * i;
        }

        for (int i = 0; i < m_volumesCount; i++)
        {
            m_volumes[i] = volumePace * i;
        }

        // 1) Pour chaque orientation
        for (int angleStep = 0; angleStep < m_anglesCount; angleStep++)
        {
            float minHeight, maxHeight;
            // Get MinH et MaxH
            GetMinMaxHighnessOfMesh(out minHeight, out maxHeight);

            //  2) Pour chaque hauteur (entre MinH et MaxH)
            for (int volumeStep = 0; volumeStep < m_volumesCount; volumeStep++)
            {
                float volumeToFind = volumeStep * volumePace;
                // Trouver meilleure hauteur
                float height = HeightFromVolumeDichotomy(volumeToFind, m_heightAccuracy, minHeight, maxHeight);

                minH = Mathf.Min(height, minH);
                maxH = Mathf.Max(height, maxH);

                m_datas[angleStep][volumeStep] = height;

                yield return new WaitForSeconds(waitTime);
                //      Supprimer les parties
            }

            transform.Rotate(Vector3.right, anglePace);
        }

        // Interpolation
        InterpoLagrange2D interpoLagrange2D = new InterpoLagrange2D(m_angles, m_volumes, m_datas, m_clearThreshold);

        float conf = interpoLagrange2D.Confidence();
        Debug.Log(m_meshID.ToString() + " : Confiance à : " + (conf*100).ToString().Substring(0,2) + "%");

        float relativeVolume = m_fullVolume / (transform.lossyScale.x * transform.lossyScale.y * transform.lossyScale.z);

        XmlMeshFormula.SaveFormula(new MeshVolumeFormula(interpoLagrange2D.polynome2D, m_meshID, relativeVolume), m_forceUpdate);
    }

    private float CalculateVolume(Vector3[] vertices, int[] triangles)
    {
        if ((vertices == null) || (triangles == null)) return 0;

        Vector3 v1, v2, v3;

        float volume = 0.0f;

        for (int j = 0; j < triangles.Length; j += 3)
        {
            v1 = vertices[triangles[j]];
            v2 = vertices[triangles[j + 1]];
            v3 = vertices[triangles[j + 2]];

            volume += ((v2.y - v1.y) * (v3.z - v1.z) - (v2.z - v1.z) * (v3.y - v1.y)) * (v1.x + v2.x + v3.x);
        }

        return volume / 6.0f;
    }

    private float HeightFromVolumeDichotomy(float volume, int accuracy, float min, float max)
    {
        float previousHeight = 0, height = 0;
        float volumeAtPreviousHeight = 0, volumeAtHeight = 0, tmp;

        for (int i = 0; i < accuracy; i++)
        {
            height = (max + min) / 2;

            tmp = MeshVolumeAtHeight(height);

            volumeAtHeight = tmp;

            if ((previousHeight < height) && (volumeAtPreviousHeight > volumeAtHeight))
            {
                volumeAtHeight = volumeAtPreviousHeight;
            }

            if (volumeAtHeight == volume) return height;

            if (volumeAtHeight > volume)
            {
                max = height;
            }
            else
            {
                min = height;
            }

            if (i < (accuracy - 1))
            {
                previousHeight = height;
                volumeAtPreviousHeight = volumeAtHeight;
            }
        }

        if (volumeAtPreviousHeight == volumeAtHeight)
        {
            return height;
        }

        float ratio = (volumeAtPreviousHeight - volume) / (volumeAtPreviousHeight - volumeAtHeight);

        height = ratio * height + (1 - ratio) * previousHeight;

        return height;
    }

    private float MeshVolumeAtHeight(float height)
    {   
        //      Duppliquer Mesh
        GameObject copy = new GameObject();
        MeshFilter meshFilter = copy.AddComponent<MeshFilter>();
        meshFilter.mesh = m_mesh;
        MeshRenderer meshRenderer = copy.AddComponent<MeshRenderer>();
        meshRenderer.sharedMaterials = m_meshRenderer.sharedMaterials;

        copy.transform.localScale = transform.localScale;
        copy.transform.rotation = transform.rotation;

        //      Couper duplicatat
        GameObject[] slices = BLINDED_AM_ME.MeshCut.Cut(copy, Vector3.up * height, Vector3.up, m_meshRenderer.material);
        
        if(!slices[0])
        {
            Debug.Log("Erreur : Impossible de calculer le mesh à cette hauteur : " + height);
            return -1;
        }

        //      Calculer volume partie "gauche"
        Mesh resultingMesh = slices[0].GetComponent<MeshFilter>().mesh;

        Vector3[] vertices = TransformedVertices(resultingMesh.vertices);
        int[]triangles = resultingMesh.triangles;

        float volume = CalculateVolume(vertices, triangles);

        Destroy(slices[0]);
        if(slices[1]) Destroy(slices[1]);

        return volume;
    }

    private void GetMinMaxHighnessOfMesh(out float min, out float max)
    {
        Vector3[] transformedVertices = TransformedVertices(m_vertices);
        min = max = 0;

        for (int i = 0; i < transformedVertices.Length; i++)
        {
            Vector3 point = transformedVertices[i];
            if (i == 0)
            {
                min = point.y;
                max = point.y;
            }
            else if (point.y < min)
            {
                min = point.y;
            }
            else if (point.y > max)
            {
                max = point.y;
            }
        }
    }

    private Vector3[] TransformedVertices(Vector3[] vertices)
    {
        Vector3[] result = new Vector3[vertices.Length];

        for (int i = 0; i < vertices.Length; i++)
        {
            result[i] = TransformedVertex(vertices[i]);
        }

        return result;
    }

    private Vector3 TransformedVertex(Vector3 vertex)
    {
        vertex.Scale(transform.lossyScale);
        vertex = transform.rotation * vertex;

        return vertex;
    }
}
