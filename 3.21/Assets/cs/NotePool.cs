using Node;
using System.Collections.Generic;
using UnityEngine;

public class NotePool : MonoBehaviour
{
    public GameObject prefab;

    private Queue<GameObject> pool = new Queue<GameObject>();

    public GameObject Get()
    {
        if (pool.Count > 0)
        {
            return pool.Dequeue();
        }
        else
        {
            return Instantiate(prefab, transform);
        }
    }

    public void Return(GameObject note)
    {
        note.GetComponent<NoteVisual>().Recycle();
        pool.Enqueue(note);
    }
}