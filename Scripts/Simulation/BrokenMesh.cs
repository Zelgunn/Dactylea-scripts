using UnityEngine;
using System.Collections;

public class BrokenMesh : MonoBehaviour
{
    public IEnumerator ExplodeToBrokenCoroutine(Vector3 source)
    {
        float t = 0;
        float time = 3;
        Vector3 grav = new Vector3();

        while (t < time)
        {
            grav.y = - t / 2;

            foreach (Transform child in transform)
            {
                child.transform.position += (child.transform.position - source + grav) * Time.deltaTime * 3;
            }

            t += Time.deltaTime;
            yield return null;
        }

        Destroy(gameObject);
    }
}
