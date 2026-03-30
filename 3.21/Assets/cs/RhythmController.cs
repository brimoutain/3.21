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
        Invoke("PlayRhythm", 0f);
    }
    private void PlayRhythm()
    {
        rhythm.Play();
    }

    public void Judge(float inputTime, NoteData note)
    {
        float delta = Mathf.Abs(inputTime - note.time);

        Debug.Log("Judge调用一下");
        //Debug.Log(delta);
        if (delta < 0.05f)
        {
            GameManager.instance.AddComboAndCheck(true);
            note.isHit = true;
        }
        else
            GameManager.instance.AddComboAndCheck(false);
    }

    
}
