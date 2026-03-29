using DG.Tweening;
using System;
using UnityEngine;
using UnityEngine.UI;

namespace Node
{
    public class NoteVisual : MonoBehaviour
    {
        //animation
        private Animator animator;

        //image
        public Sprite imageA;
        public Sprite imageB;
        public Sprite imageC;
        private Image image;
        public int switchImage = 0;

        //time
        public float judgeTime = 0;
        public float duration = 2f;

        float lastAPressTime = -1f;
        float lastEnterPressTime = -1f;
        float doubleKeyThreshold = 0.05f;

        private void Awake()
        {
            animator = GameObject.Find("player").GetComponent<Animator>();
            if (animator == null)
            {
                Debug.Log("Animator Error");
            }
        }

        private void Update()
        {

            // 单键
            if (Input.GetKeyDown(KeyCode.A))
            {
                animator.SetTrigger("DrumLeft");
                JudgeLane(0);
            }
            else if (Input.GetKeyDown(KeyCode.L))
            {
                animator.SetTrigger("DrumRight");
                JudgeLane(1);
            }
            float currentTime = GameManager.instance.currentTime;

            float diff = currentTime - judgeTime;

            if (Mathf.Abs(diff) <= 0.5f)   // 判定窗口 
            {
                // 记录按键时间
                //if (Input.GetKeyDown(KeyCode.A))
                //    lastAPressTime = Time.time;

                //if (Input.GetKeyDown(KeyCode.L))
                //    lastEnterPressTime = Time.time;

                if (Mathf.Abs(lastAPressTime - lastEnterPressTime) <= doubleKeyThreshold)
                {
                    animator.SetTrigger("DrumDouble");
                    JudgeLane(2);

                    lastAPressTime = -1f;
                    lastEnterPressTime = -1f;
                    return;
                }

            }

            if (diff > 0.5f)
            {
                Miss();
            }
        }

        public void ParabolicWithDOTween()
        {
            Vector3 startPos = new Vector3(-300, -100, 0);
            Vector3 endPos = startPos + new Vector3(540, 20, 0);
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


        void Miss()
        {
            image.DOKill();

            image.DOFade(0, 0.1f)
                .OnComplete(() => Recycle());
        }

        //欧：对象池对应的初始化和回收
        public void Init(NoteData data)
        {
            judgeTime = data.time;
            switchImage = (int)data.lane;
            gameObject.SetActive(true);
            image = GetComponentInChildren<Image>();
            switch (switchImage)
            {
                case 0:
                    image.sprite = imageA;
                    break;
                case 1:
                    image.sprite = imageB;
                    break;
                case 2:
                    image.sprite = imageC;
                    break;
            }
            ParabolicWithDOTween();
        }
        public void Recycle()
        {
            gameObject.SetActive(false);
        }

        void JudgeLane(int lane)
        {
            //找最近的对应方向音符
            float currentTime = GameManager.instance.currentTime;
            NoteData best = NoteSpawner.instance.FindClosestNote(lane, currentTime);

            //明公传inputTime
            RhythmController.instance.Judge(currentTime, best);
        }
    }
}