using System;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

namespace Node
{
    public class NodeViusal: MonoBehaviour
    {
        public Sprite imageA;
        public Sprite imageB;
        private Image image;
        public bool switchImage = false;
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
                    //判定，左
                    //根据返回结果设置gm
                }
                else if (Input.GetKeyDown(KeyCode.KeypadEnter))
                {
                    //判定，右
                }
            }
        }
    }
}