using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;
using UnityEngine;

public class NoteSpawner : MonoBehaviour
{
    public static NoteSpawner instance;
    public float spawnAheadTime = 2f;//note在要到判定线前2s生成
    public List<NoteData> data;      //管理总音符的逻辑时间
    public NotePool pool;

    private int currentIndex = 0;

    private void Awake()
    {
        if (instance == null) instance = this;
        else Destroy(instance);
    }

    void Update()
    {
        float t =RhythmController.instance.CurrentTime;//当前曲子的播放时间

        // 持续生成note
        while (currentIndex < data.Count &&
               data[currentIndex].time <= t + spawnAheadTime)
        {
            Spawn(data[currentIndex]);
            currentIndex++;
        }
    }
    void Spawn(NoteData note)
    {
        var obj = pool.Get(); // 对象池
        obj.Init(note);       // 传入 time / lane
    }

    //找最近音符
    public NoteData FindClosestNote(bool lane, float currentTime)
    {
        NoteData closest = null;
        float minDiff = float.MaxValue;

        foreach (var note in data) 
        {
            // 只检查指定轨道且未击打的音符
            if (note.lane == lane && !note.isHit)
            {
                float diff = Mathf.Abs(note.time - currentTime);
                if (diff < minDiff)
                {
                    minDiff = diff;
                    closest = note;
                }//取近的
            }
        }

        return closest;
    }
}
