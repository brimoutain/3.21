using UnityEngine;
using DG.Tweening;

public class RandomJumpAndRotate : MonoBehaviour
{
    [Header("跳跃参数")]
    public float jumpPower = 2f;
    public float duration = 0.6f;
    public int jumpCount = 1;

    [Header("旋转参数")]
    public float rotateDuration = 0.6f;
    public float rotateRange = 5f; // ±范围

    private Vector3 startPos;

    private void Start()
    {
        startPos = transform.position;
        Play();
    }

    public void Play()
    {
        float offsetY = Random.Range(-5f, 5f);

        transform.DOLocalRotate(
            new Vector3(0, offsetY, 0),
            rotateDuration,
            RotateMode.LocalAxisAdd
        ).SetEase(Ease.InOutSine);
    }
}