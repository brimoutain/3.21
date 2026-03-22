using DG.Tweening;
using System;
using UnityEngine;
using UnityEngine.UI;

namespace Node
{
    public class NoteVisual: MonoBehaviour
    {
        //image
        public Sprite imageA;
        public Sprite imageB;
        private Image image;
        public bool switchImage = false;
        
        //time
        public float judgeTime = 0;
        public float duration = 2f;

        private void Awake()
        {
            ParabolicWithDOTween();
            //image = gameObject.AddComponent<Image>();
            image = GetComponent<Image>();
            image.sprite = switchImage ? imageA : imageB;
        }
        
        public void ParabolicWithDOTween()
        {
            Vector3 startPos = new Vector3(-300, -100, 0);
            Vector3 endPos = startPos + new Vector3(600, 0, 0);
            float jumpHeight = 60f;
            
            float elapsed = 0;
    
            DOTween.To(() => 0f, t => {
                elapsed = t;
        
                // 计算 X 位置（线性移动）
                float x = Mathf.Lerp(startPos.x, endPos.x, t);
        
                // 计算 Y 位置（抛物线公式）
                // y = 起始Y + 高度 * sin(π * t)
                float y = startPos.y + jumpHeight * Mathf.Sin(Mathf.PI * t);
        
                image.rectTransform.anchoredPosition = new Vector3(x, y, 0);
            }, 1f, duration).SetEase(Ease.Linear);
        }

        private void OnCollisionStay2D(Collision2D other)
        {
            if (other.gameObject.CompareTag("Judge"))
            {
                if (Input.GetKeyDown(KeyCode.A))
                {
                    JudgeLane(false);//欧
                    //根据返回结果设置gm
                }
                else if (Input.GetKeyDown(KeyCode.KeypadEnter))
                {
                    JudgeLane(true);//欧
                }
            }
        }

        //欧：对象池对应的初始化和回收
        public void Init(NoteData data)
        {
            judgeTime = data.time;
            switchImage = data.lane;
            image.sprite = switchImage ? imageA : imageB;
            gameObject.SetActive(true);
        }
        public void Recycle()
        {
            gameObject.SetActive(false);
        }
        void JudgeLane(bool lane)
        {
            //找最近的对应方向音符
            float currentTime = RhythmController.instance.CurrentTime;
            NoteData best =NoteSpawner.instance. FindClosestNote(lane, currentTime);
            
            //明公传inputTime
            RhythmController.instance.Judge(GameManager.instance.currentTime, best);
        }
    }
}