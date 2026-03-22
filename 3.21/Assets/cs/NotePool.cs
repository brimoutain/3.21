using Node;
using System.Collections.Generic;
using UnityEngine;

public class NotePool : MonoBehaviour
{
    public NoteVisual prefab;

    private Queue<NoteVisual> pool = new Queue<NoteVisual>();

    public NoteVisual Get()
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

    public void Return(NoteVisual note)
    {
        note.Recycle();
        pool.Enqueue(note);
    }
}