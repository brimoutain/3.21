using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace B
{
    public class UIManager : MonoBehaviour
    {
        public static UIManager instance;
        
        //素材
        //轨道
        //node
        [SerializeField]TextMeshProUGUI combo;

        private void Awake()
        {
            instance = this;
        }

        private void Update()
        {
            combo.text = GameManager.instance.combo.ToString();
        }
    }
}