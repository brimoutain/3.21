using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RhythmController : MonoBehaviour
{
    // animation
    private Animator animator;

    //特效
    public ParticleSystem RVFX;
    public ParticleSystem LVFX;
    public ParticleSystem HitVFX;

    public AudioSource rhythm;
    public static RhythmController instance;

    public float CurrentTime => rhythm.time;

    private void Awake()
    {
        if (instance == null) instance = this;
        else Destroy(instance);
        Invoke("PlayRhythm", 0f);

        animator = GameObject.Find("player").GetComponent<Animator>();
        if (animator == null)
            Debug.LogError("Animator not found on 'player'!");
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
            HitVFX.Play();
            GameManager.instance.AddComboAndCheck(true);
            note.isHit = true;
        }
        else
            GameManager.instance.AddComboAndCheck(false);
    }

    //orz我决定把左右和同步打的效果放这，NV实例太多了
    public void Right()
    {
        animator.SetTrigger("DrumLeft");
        RestartVFX(LVFX);
    }
    public void Left()
    {
        animator.SetTrigger("DrumRight");
        RestartVFX(RVFX);
    }
    public void Double()
    {
        animator.SetTrigger("DrumDouble");
        RestartVFX(LVFX);
        RestartVFX(RVFX);
    }
    public void RestartVFX(ParticleSystem vfx)
    {
        if (vfx == null)
        {
            Debug.LogWarning("VFX 为空，无法播放特效");
            return;
        }
        else
        {
            vfx.Stop();
            vfx.Play();
        }

    }
}
