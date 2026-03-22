using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RhythmController : MonoBehaviour
{

    public AudioSource rhythm;
    public static RhythmController instance;

    public float CurrentTime => rhythm.time;

    private void Awake()
    {
        if (instance == null) instance = this;
        else Destroy(instance);

        rhythm = GetComponent<AudioSource>();
        rhythm.Play();
    }

    //¼ì²â½á¹û
    public void Judge(float inputTime, NoteData note)
    {
        float delta = Mathf.Abs(inputTime - note.time);

        if (delta < 0.05f)
        {
            GameManager.instance.AddComboAndCheck(true);
            note.isHit = true;
        }
        else
            GameManager.instance.AddComboAndCheck(false);
    }

    
}
