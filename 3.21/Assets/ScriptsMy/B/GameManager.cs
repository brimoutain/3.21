using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

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
    public List<AudioSource>  audioSources = new List<AudioSource>();
    

    // 摄像机
    [SerializeField] private Camera cam;
    [SerializeField] private Transform camPos;

    // 游戏得分
    public uint currentStateNodeNum = 0;
    private uint currentStateNodeNeed;
    public uint combo = 0;
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
            a.SetActive(false);
            scales.Add(a.transform.localScale);
            a.transform.localScale = Vector3.zero;
        }
    }

    private void Update()
    {
        currentTime += Time.deltaTime;
    }

    // 增加 combo 并检测阶段变化
    public void AddComboAndCheck(bool continuous = true)
    {
        if (continuous) combo++;
        else combo = 0;

        currentStateNodeNum++;

        if (CanChangeState())
        {
            currentState++;
            ChangeState();
        }
    }

    // 阶段切换入口
    public void ChangeState()
    {
        int prevIndex = (int)currentState - 1;
        int curIndex = (int)currentState;

        PlayStateTransition(prevIndex, curIndex);
    }
    
    private void PlayStateTransition(int prevIndex, int curIndex)
    {
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

            // 如果想让当前 stateShow 也做入场动画，可启用：
            // cur.transform.localScale = Vector3.zero;
            // cur.transform.DOScale(Vector3.one, 0.2f).SetEase(Ease.OutBack);
        }
        
        if (prevIndex >= 0 && prevIndex < added.Count)
        {
            GameObject add = added[prevIndex];
            add.SetActive(true);

            Transform t = add.transform;
            t.localScale = Vector3.zero;

            t.DOScale(scales[prevIndex], 0.15f)
                .SetEase(Ease.OutBack);

            audioSources[curIndex].volume = 1;
        }
    }

 
    private bool CanChangeState()
    {
        return currentStateNodeNum >= currentStateNodeNeed;
    }
}