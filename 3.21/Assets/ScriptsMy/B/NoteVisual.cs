using DG.Tweening;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace Node
{
    public class NoteVisual : MonoBehaviour
    {

        // image
        public Sprite imageA;
        public Sprite imageB;
        public Sprite imageC;
        private Image image;
        public int switchImage = 0;

        // time
        public float judgeTime = 0;
        public float duration = 2f;

        // 判定窗口
        private const float JUDGE_WINDOW = 0.5f;
        // 双键两次按下的最大间隔（视为同时）
        private const float DOUBLE_KEY_THRESHOLD = 0.08f;

        // 双键模式下，记录第一个键按下的时刻（unscaledTime）
        private float m_firstKeyTime = float.MinValue;
        private bool m_firstKeyDown = false;

        // 防止重复判定
        private bool m_judged = false;

        private void Update()
        {
            if (m_judged) return;

            float currentTime = GameManager.instance.currentTime;
            float diff = currentTime - judgeTime;

            // Miss：Note 已超出判定窗口后沿
            if (diff > JUDGE_WINDOW)
            {
                Miss();
                return;
            }

            // 判定窗口前沿未到，不响应输入
            if (diff < -JUDGE_WINDOW) return;

            switch (switchImage)
            {
                case 0: HandleSingle(KeyCode.A); break;
                case 1: HandleSingle(KeyCode.L); break;
                case 2: HandleDouble(); break;
            }
        }

        // 单键 Note：只响应指定键
        private void HandleSingle(KeyCode key)
        {
            if (Input.GetKeyDown(key))
                Trigger();
        }

        // 双键 Note：需要 A + L 在窗口内都按下
        private void HandleDouble()
        {
            bool pressedA = Input.GetKeyDown(KeyCode.A);
            bool pressedL = Input.GetKeyDown(KeyCode.L);

            if (!pressedA && !pressedL) return; // ← 无输入直接返回，不执行超时重置

            float now = Time.unscaledTime;

            if (!m_firstKeyDown)
            {
                // 第一个键
                m_firstKeyDown = true;
                m_firstKeyTime = now;
            }
            else
            {
                float gap = now - m_firstKeyTime;
                if (gap <= DOUBLE_KEY_THRESHOLD)
                {
                    // 两键间隔够近，视为双键
                    Trigger();
                }
                else
                {
                    // 间隔太长，以当前键重新开始
                    m_firstKeyDown = true;
                    m_firstKeyTime = now;
                }
            }
        }
        private void Trigger()
        {
            if (m_judged) return;
            m_judged = true;

            switch (switchImage)
            {
                case 0:
                    RhythmController.instance.Right();
                    break;
                case 1:
                    RhythmController.instance.Left();
                    break;
                case 2:
                    RhythmController.instance.Double();
                    break;
            }

            JudgeLane(switchImage);
        }

        private void Miss()
        {
            if (m_judged) return;
            m_judged = true;

            image.DOKill();
            image.DOFade(0, 0.1f).OnComplete(() => Recycle());
        }

        // 找到对应 lane 最近的 Note 并交给 RhythmController 判定
        private void JudgeLane(int lane)
        {
            float currentTime = GameManager.instance.currentTime;
            NoteData best = NoteSpawner.instance.FindClosestNote(lane, currentTime);
            RhythmController.instance.Judge(currentTime, best);
        }

        public void Init(NoteData data)
        {
            judgeTime = data.time;
            switchImage = (int)data.lane;
            m_judged = false;
            m_firstKeyDown = false;
            m_firstKeyTime = float.MinValue;

            gameObject.SetActive(true);
            image = GetComponentInChildren<Image>();

            switch (switchImage)
            {
                case 0: image.sprite = imageA; break;
                case 1: image.sprite = imageB; break;
                case 2: image.sprite = imageC; break;
            }

            ParabolicWithDOTween();
        }

        public void ParabolicWithDOTween()
        {
            Vector3 startPos = new Vector3(-200, -150, 0);
            Vector3 endPos = startPos + new Vector3(400, 20, 0);
            float jumpHeight = 50f;

            DOTween.To(() => 0f, t =>
            {
                float x = Mathf.Lerp(startPos.x, endPos.x, t);
                float y = startPos.y + jumpHeight * Mathf.Sin(Mathf.PI * t);
                image.rectTransform.anchoredPosition = new Vector3(x, y, 0);
                image.rectTransform.localScale = Vector2.one*0.5f;
            }, 1f, duration).SetEase(Ease.Linear);
        }

        public void Recycle()
        {
            gameObject.SetActive(false);
        }
        
    }
}