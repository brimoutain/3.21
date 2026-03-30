using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager instance;

    // 游戏阶段
    public enum GameState
    {
        State0 = 0,
        State1,
        State2,
        State3,
        State4,
        End
    }
    public GameState currentState;
    
    public List<GameObject> stateShow = new List<GameObject>();
    public List<Vector3> scales = new List<Vector3>();
    public List<GameObject> added = new List<GameObject>();
    //public List<AudioSource>  audioSources = new List<AudioSource>();
    public List<GameObject> particles = new List<GameObject>();
    public TextMeshProUGUI finalText;
    

    // 摄像机
    [SerializeField] private Camera cam;
    [SerializeField] private Transform camPos;

    // 游戏得分
    public uint currentStateNodeNum = 0;
    //private uint currentStateNodeNeed;
    public List<uint> stateNeedList;//每个阶段不同的need需求
    public uint combo = 0;
    private uint finalCombo = 0;
    public float currentTime = 0;

    private void Awake()
    {
        instance = this;
        currentState = GameState.State0;

        // 初始化 stateShow
        for (int i = 0; i < stateShow.Count; i++)
        {
            stateShow[i].SetActive(i == 0); // 只有第0个显示
            stateShow[i].transform.localScale = Vector3.one;
        }

        // 初始化 added 全部隐藏
        foreach (var a in added)
        {
            a.GetComponent<SpriteRenderer>().enabled =false;
            a.GetComponent<AudioSource>().volume = 0;
            scales.Add(a.transform.localScale);
            a.transform.localScale = Vector3.zero;
        }
    }

    private void Update()
    {
        currentTime += Time.deltaTime;

        if (RhythmController.instance.rhythm.isPlaying == false && !isTransitioningToMenu)
        {
            StartCoroutine(PlayEndingAndLoadMenu());
        }
    }

    // 添加一个标志位，防止重复触发
    private bool isTransitioningToMenu = false;

    // 结束动画并加载菜单的协程
    private IEnumerator PlayEndingAndLoadMenu()
    {
        isTransitioningToMenu = true;
    
        // 播放 finalText 的放大动画
        if (finalText != null)
        {
            // 确保文本是激活状态
            finalText.gameObject.SetActive(true);
            finalText.text = "Final Grade : " + finalCombo.ToString();
        
            // 设置初始缩放为0
            finalText.transform.localScale = Vector3.zero;
        
            // 播放放大动画（弹性效果）
            finalText.transform.DOScale(Vector3.one * 2, 0.5f)
                .SetEase(Ease.OutBack);
        
            // 可选：添加颜色渐变效果
            finalText.DOColor(Color.yellow, 0.3f)
                .SetEase(Ease.OutQuad);
        
            // 可选：添加文字跳动效果（循环几次）
            for (int i = 0; i < 3; i++)
            {
                yield return new WaitForSeconds(0.2f);
                finalText.transform.DOScale(Vector3.one * 1.1f, 0.1f)
                    .SetEase(Ease.OutQuad);
                yield return new WaitForSeconds(0.1f);
                finalText.transform.DOScale(Vector3.one, 0.1f)
                    .SetEase(Ease.InQuad);
            }
        }
    
        // 等待10秒
        yield return new WaitForSeconds(10f);
    
        // 可选：淡出效果
        if (finalText != null)
        {
            finalText.DOFade(0, 0.5f);
            yield return new WaitForSeconds(0.5f);
        }
    
        // 加载菜单场景
        SceneManager.LoadScene("Menu");
    }

    // 增加 combo 并检测阶段变化
    public void AddComboAndCheck(bool continuous = true)
    {
        if (continuous) 
        { 
            combo++;
            finalCombo++;

            currentStateNodeNum++;
            if (currentState == GameState.End) return; // 防止继续增长

            if (CanChangeState())
            {
                currentStateNodeNum = 0;
                currentState++;
                if ((int)currentState >= stateShow.Count)
                {
                    currentState = GameState.End;
                    return;
                }

                ChangeState();
            }
        }
        else combo = 0;

    }

    // 阶段切换入口
    public void ChangeState()
    {
        int prevIndex = (int)currentState - 1;
        int curIndex = (int)currentState;

        PlayStateTransition(prevIndex, curIndex);
    }

 
    private bool CanChangeState()
    {
        int index = (int)currentState;
        if (index >= stateNeedList.Count) return false;

        return currentStateNodeNum >= stateNeedList[index];
    }
    
    private void PlayStateTransition(int prevIndex, int curIndex)
    {
        // 🎯 启动顺序播放粒子，并在完成后执行状态切换动画
        StartCoroutine(PlayParticlesAndThenTransition(prevIndex, curIndex));
    }

    // 新的协程：先播放粒子，完成后再执行转场动画
    private IEnumerator PlayParticlesAndThenTransition(int prevIndex, int curIndex)
    {
        // 第一步：播放第一个和第二个粒子
        List<GameObject> firstBatch = new List<GameObject>();
        for (int i = 0; i < 2 && i < particles.Count; i++)
        {
            if (particles[i] != null)
            {
                firstBatch.Add(particles[i]);
                PlayParticle(particles[i]);
            }
        }
        
        // 等待第一个和第二个粒子完成（持续1秒）
        yield return new WaitForSeconds(2.0f);
        
        // 第二步：停止并隐藏前两个粒子
        foreach (var particle in firstBatch)
        {
            if (particle != null)
            {
                StopParticle(particle);
                particle.SetActive(false);
            }
        }
        
        // 🎯 第三步：同时执行两个操作
        // 操作1：执行 stateShow 的缩小动画和 added 的动画
        ExecuteStateTransition(prevIndex, curIndex);
        
        // 操作2：播放剩下的粒子（从索引2开始）
        for (int i = 2; i < particles.Count; i++)
        {
            if (particles[i] != null)
            {
                PlayParticle(particles[i]);
                // 每个粒子播放0.1秒后关闭
                StartCoroutine(StopParticleAfterDelay(particles[i], 1f));
            }
        }
    }

    // 提取出来的状态转换逻辑
    private void ExecuteStateTransition(int prevIndex, int curIndex)
    {
        // 原有的 stateShow 过渡代码
        if (prevIndex >= 0 && prevIndex < stateShow.Count)
        {
            GameObject prev = stateShow[prevIndex];
            prev.transform.DOScale(Vector3.zero, 0.1f)
                .OnComplete(() => prev.SetActive(false));
        }

        if (curIndex < stateShow.Count)
        {
            GameObject cur = stateShow[curIndex];
            cur.SetActive(true);
            
            // 如果想要入场动画，可以启用：
            cur.transform.localScale = Vector3.zero;
            cur.transform.DOScale(Vector3.one, 0.3f).SetEase(Ease.OutBack);
        }
        
        // 原有的 added 对象代码
        if (prevIndex >= 0 && prevIndex < added.Count)
        {
            Debug.Log("prevI="+ prevIndex);
            GameObject add = added[prevIndex];
            add.GetComponent<SpriteRenderer>().enabled = true;
            add.GetComponent<AudioSource>().volume = 0.8f;
            
            Transform t = add.transform;
            t.localScale = Vector3.zero;
            t.DOScale(scales[prevIndex], 0.15f).SetEase(Ease.OutBack);
        }
    }

    // 辅助方法：播放粒子
    private void PlayParticle(GameObject particle)
    {
        particle.SetActive(true);
        ParticleSystem ps = particle.GetComponent<ParticleSystem>();
        if (ps != null)
        {
            ps.Play();
        }
    }

    // 辅助方法：停止粒子
    private void StopParticle(GameObject particle)
    {
        ParticleSystem ps = particle.GetComponent<ParticleSystem>();
        if (ps != null)
        {
            ps.Stop();
        }
    }

    // 延迟停止粒子
    private IEnumerator StopParticleAfterDelay(GameObject particle, float delay)
    {
        yield return new WaitForSeconds(delay);
        if (particle != null)
        {
            StopParticle(particle);
            particle.SetActive(false);
        }
    }
}