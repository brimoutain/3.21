using UnityEngine;

public class ScreenShake : MonoBehaviour
{
    public static ScreenShake Instance; // 单例模式，方便调用

    void Awake()
    {
        Instance = this;
    }

    // 简单的震动
    public void Shake(float duration, float magnitude)
    {
        StartCoroutine(ShakeCoroutine(duration, magnitude));
    }

    System.Collections.IEnumerator ShakeCoroutine(float duration, float magnitude)
    {
        Vector3 originalPos = transform.position;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            float x = Random.Range(-1f, 1f) * magnitude;
            float y = Random.Range(-1f, 1f) * magnitude;

            transform.position = originalPos + new Vector3(x, y, 0);

            elapsed += Time.deltaTime;
            yield return null;
        }

        transform.position = originalPos;
    }
}