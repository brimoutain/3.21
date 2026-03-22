using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;
using UnityEngine;

public class NoteSpawner : MonoBehaviour
{
    public float spawnAheadTime = 2f;
    public List<NoteData> data;

    private int currentIndex = 0;

    //void Update()
    //{
    //    float t = rhythm.CurrentTime;

    //    // 持续生成“未来窗口内”的note
    //    while (currentIndex < data.Count &&
    //           data[currentIndex].time <= t + spawnAheadTime)
    //    {
    //        Spawn(data[currentIndex]);
    //        currentIndex++;
    //    }
    //}
    //void Spawn(NoteData note)
    //{
    //    var obj = pool.Get(); // 对象池
    //    obj.Init(note);       // 传入 time / lane
    //}
}
