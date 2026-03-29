using System;
using System.Collections;
using System.Collections.Generic;
using Node;
using Unity.VisualScripting;
using UnityEditor.Experimental.GraphView;
using UnityEngine;

public class NoteSpawner : MonoBehaviour
{
    public static NoteSpawner instance;
    public float spawnAheadTime = 2f;//note在要到判定线前2s生成
    public List<NoteData> data;      //管理总音符的逻辑时间
    public NotePool pool;
    public TextAsset csvFile;

    private int currentIndex = 0;

    private void Awake()
    {
        if (instance == null) instance = this;
        else Destroy(instance);
        LoadCSV();
    }

    void Update()
    {
        float t = GameManager.instance.currentTime;

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
        obj.GetComponent<NoteVisual>().Init(note);       // 传入 time / lane
    }

    //找最近音符
    public NoteData FindClosestNote(int lane, float currentTime)
    {
        NoteData closest = null;
        float minDiff = float.MaxValue;

        foreach (var note in data) 
        {
            // 只检查指定轨道且未击打的音符
            if ((int)note.lane == lane && !note.isHit)
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

    //CSV填表
    void LoadCSV()
    {
        
        if (csvFile == null)
        {
            Debug.LogError($"找不到文件：{"notes"}.csv");
            return;
        }
        string[] lines = csvFile.text.Split('\n');

        // 跳过第一二行
        for (int i = 1; i < lines.Length; i++)
        {
            if (string.IsNullOrWhiteSpace(lines[i])) continue;

            string[] values = lines[i].Split(',');

            if (values.Length >= 2)
            {
                float time = float.Parse(values[0]);
                int type = int.Parse (values[1].Trim());

                data.Add(new NoteData
                {
                    time = time,
                    lane = (NoteType)type
                });
            }
        }

        Debug.Log($"从CSV加载 {data.Count} 个音符");
    }

}
