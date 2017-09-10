using UnityEngine;
using System.Collections;

public class MeshExplosion : MonoBehaviour
{
    [SerializeField] private GameObject m_partPrefab;
    [SerializeField] private BrokenMesh m_brokenPrefab;
    private MeshRenderer m_meshRenderer;
    private Rigidbody m_rigidbody;
    private VivePickable m_pickable;

	private void Awake ()
	{
        m_meshRenderer = GetComponent<MeshRenderer>();
        m_rigidbody = GetComponent<Rigidbody>();
        m_pickable = GetComponent<VivePickable>();
	}

    private void OnCollisionEnter(Collision other)
    {
        if ((other.collider.isTrigger) || (other.collider.gameObject.name == "VitreHotte")) return;

        ViveCork otherCork = other.gameObject.GetComponent<ViveCork>();
        if (otherCork) return;
        VivePickable otherPickable = other.gameObject.GetComponent<VivePickable>();

        float collisionForce = other.relativeVelocity.magnitude * other.relativeVelocity.magnitude;
        if(other.rigidbody)
        {
            collisionForce *= other.rigidbody.mass;
        }
        else
        {
            collisionForce *= m_rigidbody.mass;
        }
        bool conditionsToExplode = (collisionForce >= 3) && !(m_pickable && m_pickable.picked) && !(otherPickable && otherPickable.picked);

        if (conditionsToExplode)
        {
            MakeGlassBreakSound();

            FailCondition failCondition = new FailCondition(FailCondition.FailType.BrokenGlass);
            failCondition.failed = true;
            failCondition.failurePosition = transform.position;
            Watcher.ProvokeFail(failCondition, gameObject);

            Debug.Log("ColliXion avec " + other.collider.name + "(Force : " + Mathf.Round(collisionForce * 10) / 10 + " )");
            Explode(other.contacts[0].point);
        }
        else
        {
            MakeGlassShockSound(other.contacts[0].point, other.relativeVelocity.magnitude);
        }
    }

    #region Sounds
    private void MakeGlassBreakSound()
    {
        AudioSource brokenGlassSource = (new GameObject()).AddComponent<AudioSource>();

        brokenGlassSource.transform.position = transform.position;
        brokenGlassSource.volume = 0.35f;
        brokenGlassSource.pitch = Random.Range(0.9f, 1.1f);
        brokenGlassSource.spatialBlend = 1;
        brokenGlassSource.clip = SimulationData.glassBreakSound;
        brokenGlassSource.Play();

        brokenGlassSource.name = "Son : bris de verre (" + name + ")";

        Destroy(brokenGlassSource.gameObject, brokenGlassSource.clip.length);
    }

    private void MakeGlassShockSound(Vector3 position, float force)
    {
        AudioSource glassSource = (new GameObject()).AddComponent<AudioSource>();

        glassSource.transform.position = position;
        glassSource.pitch = Random.Range(0.9f, 1.1f);
        glassSource.spatialBlend = 1;
        glassSource.volume = 0.05f * (force + 2);
        glassSource.clip = SimulationData.glassShockSound;
        glassSource.Play();

        glassSource.name = "Son : bruit de verre (" + name + ")";

        Destroy(glassSource.gameObject, glassSource.clip.length);
    }
    #endregion

    public void Explode(Vector3 source)
    {
        if(m_brokenPrefab)
        {
            ExplodeToBroken(source);
        }
        else
        {
            ExplodeMesh();
        }
    }

    private void ExplodeToBroken(Vector3 source)
    {
        BrokenMesh brokenMesh = Instantiate(m_brokenPrefab);
        brokenMesh.StartCoroutine(brokenMesh.ExplodeToBrokenCoroutine(source));
        brokenMesh.transform.position = transform.position;
        brokenMesh.transform.rotation = transform.rotation;

        Destroy(gameObject);
    }

    [ContextMenu("ExplodeMesh")]
    private void ExplodeMesh()
    {
        foreach (Transform t in transform)
        {
            Destroy(t.gameObject);
        }

        Mesh mesh = GetComponent<MeshFilter>().mesh;

        int[] triangles = mesh.triangles;
        int partCount = triangles.Length / 3;

        GameObject[] parts = new GameObject[partCount];
        Vector3[] centersOfTriangles = new Vector3[partCount];

        for (int i = 0; i < partCount; i++)
        {
            GameObject part = Instantiate<GameObject>(m_partPrefab);
            parts[i] = part;
            part.name = "Exploded Part n°" + i;
            Transform t = part.GetComponent<Transform>();
            t.SetParent(transform);
            t.localPosition = Vector3.zero;
            t.localEulerAngles = Vector3.zero;
            t.localScale = Vector3.one;
            MeshFilter filter = part.GetComponent<MeshFilter>();
            MeshRenderer meshRenderer = part.GetComponent<MeshRenderer>();

            Mesh partMesh = new Mesh();
            partMesh.name = "Exploded Mesh n°" + i;

            Vector3[] partMeshVertex = new Vector3[3];
            partMeshVertex[0] = mesh.vertices[triangles[i * 3]];
            partMeshVertex[1] = mesh.vertices[triangles[i * 3 + 1]];
            partMeshVertex[2] = mesh.vertices[triangles[i * 3 + 2]];

            int[] partMeshTriangle = new int[3];
            partMeshTriangle[0] = 0;
            partMeshTriangle[1] = 1;
            partMeshTriangle[2] = 2;

            filter.mesh = partMesh;

            partMesh.vertices = partMeshVertex;
            partMesh.triangles = partMeshTriangle;

            meshRenderer.material = m_meshRenderer.material;

            MeshCollider partCollider = part.GetComponent<MeshCollider>();
            if(partCollider)
                partCollider.sharedMesh = partMesh;

            Vector3 centerOfTriangle = (partMeshVertex[0] + partMeshVertex[1] + partMeshVertex[2]) / 3;
            centersOfTriangles[i] = centerOfTriangle;
        }

        for (int i = 0; i < partCount; i++)
        {
            GameObject part = parts[i];

            Rigidbody partRigidbody = part.GetComponent<Rigidbody>();

            partRigidbody.AddExplosionForce(100 * centersOfTriangles[i].magnitude, transform.position - centersOfTriangles[i], 5);
            partRigidbody.AddTorque(Random.Range(0f, 90), Random.Range(0f, 90), Random.Range(0f, 90));

        }

        Destroy(gameObject, Random.Range(0f, 1f));

        m_rigidbody.isKinematic = true;
        m_rigidbody.useGravity = false;
        m_meshRenderer.enabled = false;
        enabled = false;
    }
}
