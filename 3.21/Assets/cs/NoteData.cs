using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum NoteType
{
    Left,
    Right,
    Both
}
[System.Serializable]
public class NoteData
{
    public float time;   // ÅÐ¶¨Ê±¼ä
    public NoteType lane;     // ×ó / ÓÒ
    public bool isHit=false;
}
