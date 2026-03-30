using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RhythmController : MonoBehaviour
{
    //把NV图片也放这
    public Sprite imageA;
    public Sprite imageB;
    public Sprite imageC;

    // animation
    private Animator animator;

    //特效
    public ParticleSystem RVFX;
    public ParticleSystem LVFX;
    public ParticleSystem HitVFX;

    //音效
    public AudioSource RightDrum;
    public AudioSource LeftDrum;
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
        if (delta < 0.16f)
        {
            ScreenShake.Instance.Shake(0.3f, 0.1f);
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
        ClipPlay(RightDrum);
        animator.SetTrigger("DrumLeft");
        RestartVFX(LVFX);
    }
    public void Left()
    {
        ClipPlay(LeftDrum);
        animator.SetTrigger("DrumRight");
        RestartVFX(RVFX);
    }
    public void Double()
    {
        ClipPlay(RightDrum);
        ClipPlay(LeftDrum);
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
    public void ClipPlay(AudioSource c)
    {
        if (c == null)
        {
            Debug.LogWarning(" 鼓音效为空，无法播放鼓音效");
            return;
        }
        else
        {
            c.Stop();
            c.Play();
        }
        
    }
}
