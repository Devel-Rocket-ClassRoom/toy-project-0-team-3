using System.Collections.Generic;
using UnityEngine;

public class HitBox : MonoBehaviour
{
    private List<Collider> colliders = new List<Collider>();

    public List<Collider> Colliders
    {
        get { return colliders; }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!colliders.Contains(other))
        {
            colliders.Add(other);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        colliders.Remove(other);
    }
}
