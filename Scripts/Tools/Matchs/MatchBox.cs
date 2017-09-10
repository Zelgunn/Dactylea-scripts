using UnityEngine;
using System.Collections;

public class MatchBox : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        Match match = other.GetComponent<Match>();

        if (match)
            match.LightUp();
    }

    private void OnCollisionEnter(Collision other)
    {
        Match match = other.collider.GetComponent<Match>();

        if (match)
            match.LightUp();
    }
}
