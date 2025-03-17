using UnityEngine;
using System.Collections;

public class FadeOutHandler : MonoBehaviour
{
    // Fungsi untuk memulai fade out pada objek tertentu
    public void TriggerFadeOut(GameObject targetObject, float duration)
    {
        if (targetObject != null)
        {
            CoroutineManager.Instance.StartManagedCoroutine(AnimateFade(targetObject, duration));
        }
        else
        {
            Debug.LogWarning("Target object untuk fade out tidak ditemukan.");
        }
    }

    // Coroutine untuk menjalankan animasi fade out
    private IEnumerator AnimateFade(GameObject obj, float duration)
    {
        CanvasGroup canvasGroup = obj.GetComponent<CanvasGroup>();
        if (canvasGroup == null)
        {
            canvasGroup = obj.AddComponent<CanvasGroup>();
        }

        float startAlpha = canvasGroup.alpha;
        float endAlpha = 0;

        float time = 0;
        while (time < duration)
        {
            time += Time.deltaTime;
            canvasGroup.alpha = Mathf.Lerp(startAlpha, endAlpha, time / duration);
            yield return null;
        }
        canvasGroup.alpha = endAlpha;

        // Nonaktifkan objek setelah fade out selesai
        obj.SetActive(false);
    }
}
