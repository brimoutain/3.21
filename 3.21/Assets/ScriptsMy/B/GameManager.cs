using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager instance;
    
    //转阶段
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
    
    //摄像机
    [SerializeField] Camera cam;
    [SerializeField] private Transform camPos;
    
    //游戏得分
    public uint currentStateNodeNum = 0;
    private uint currentStateNodeNeed;
    public uint combo = 0;
    public float currentTime = 0;
    
    // Start is called before the first frame update
    private void Awake()
    {
        instance = this;
        currentState = GameState.State0;
        //获取初始node数量
    }

    private void Update()
    {
        currentTime += Time.deltaTime;

    }


    //这里其实就是miss就把combo清零
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

    public void ChangeState()
    {
        switch (currentState)
        {
            case GameState.State1:
                //获取这一阶段需要的node数量
            case GameState.State2:
            case GameState.State3:
                break;
        }
    }
    
    private bool CanChangeState()
    {
        if (currentStateNodeNum >= currentStateNodeNeed)
        {
            return true;
        }
        else return false;
    }
}
