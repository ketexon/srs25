using System.Collections.Generic;
using System.Collections;
using UnityEngine;

public class BulletHole : MonoBehaviour
{
    [SerializeField] float lifetime = 5;

    const int MAX_HOLES = 50;
    static LinkedList<BulletHole> allHoles = new();

    [System.NonSerialized] public bool Cleaned = false;

    void Awake()
    {
        allHoles.AddLast(this);
        StartCoroutine(DestroyCoroutine());
        Clean();
    }

    IEnumerator DestroyCoroutine()
    {
        yield return new WaitForSeconds(lifetime);
        Destroy(gameObject);
    }

    void OnDestroy()
    {
        if (!Cleaned)
        {
            allHoles.Remove(this);
        }
    }

    static void Clean()
    {
        while(allHoles.Count > MAX_HOLES)
        {
            var first = allHoles.First.Value;
            first.Cleaned = true;
            if (first)
            {
                Destroy(first.gameObject);
            }
            allHoles.RemoveFirst();
        }
    }
}