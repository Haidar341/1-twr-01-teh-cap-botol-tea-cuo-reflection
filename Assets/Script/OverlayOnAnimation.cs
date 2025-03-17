using UnityEngine;
using System.Collections;
public class OverlayOnAnimation : MonoBehaviour
{
    [Header("Pengaturan Overlay")]
    public GameObject overlayObject;  // Referensi ke objek overlay (CanvasGroup atau gambar overlay)

    [Header("Pengaturan Animator")]
    public Animator animator;         // Referensi ke Animator yang memutar animasi
    public string animationName;      // Nama animasi yang akan diputar

    private CanvasGroup overlayCanvasGroup;

    void Start()
    {
        if (overlayObject != null)
        {
            overlayCanvasGroup = overlayObject.GetComponent<CanvasGroup>();
            if (overlayCanvasGroup == null)
            {
                Debug.LogWarning("CanvasGroup tidak ditemukan pada overlayObject. Menggunakan gameObject biasa.");
            }
        }
        else
        {
            Debug.LogWarning("Overlay object belum diatur di Inspector.");
        }
    }

    public void PlayAnimationWithOverlay()
    {
        if (animator != null && !string.IsNullOrEmpty(animationName))
        {
            // Mulai animasi
            animator.Play(animationName);

            // Mengaktifkan overlay dan menunggu animasi selesai
            StartCoroutine(ActivateOverlayAndWaitForAnimation());
        }
        else
        {
            Debug.LogError("Animator atau Nama Animasi tidak diatur.");
        }
    }

    private IEnumerator ActivateOverlayAndWaitForAnimation()
    {
        // Pastikan overlay aktif saat animasi dimulai
        if (overlayCanvasGroup != null)
        {
            overlayCanvasGroup.alpha = 1f; // Mengaktifkan overlay (CanvasGroup alpha)
            overlayCanvasGroup.blocksRaycasts = true;  // Jika ingin overlay menghalangi input
        }
        else if (overlayObject != null)
        {
            overlayObject.SetActive(true); // Jika overlay bukan CanvasGroup
        }

        // Menunggu hingga animasi selesai
        while (animator.GetCurrentAnimatorStateInfo(0).IsName(animationName) && 
               animator.GetCurrentAnimatorStateInfo(0).normalizedTime < 1f)
        {
            yield return null;  // Menunggu satu frame
        }

        // Matikan overlay setelah animasi selesai
        if (overlayCanvasGroup != null)
        {
            overlayCanvasGroup.alpha = 0f; // Menyembunyikan overlay
            overlayCanvasGroup.blocksRaycasts = false; // Jika ingin overlay tidak menghalangi input
        }
        else if (overlayObject != null)
        {
            overlayObject.SetActive(false);  // Menonaktifkan overlay
        }
    }
}
