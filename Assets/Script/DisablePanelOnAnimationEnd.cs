using UnityEngine;

public class DisablePanelOnSpecificAnimationEnd : MonoBehaviour
{
    public GameObject targetPanel;       // Panel yang akan dinonaktifkan
    public Animator animator;            // Animator yang memainkan animasi
    public string animationName;         // Nama animasi yang ingin diawasi

    void Start()
    {
        // Pastikan Animator sudah di-assign
        if (animator == null)
        {
            animator = GetComponent<Animator>();
        }
    }

    void Update()
    {
        // Periksa apakah animasi dengan nama tertentu selesai
        if (animator != null && IsSpecificAnimationFinished(animationName))
        {
            targetPanel.SetActive(false);
        }
    }

    bool IsSpecificAnimationFinished(string animName)
    {
        AnimatorStateInfo stateInfo = animator.GetCurrentAnimatorStateInfo(0);
        
        // Periksa apakah animasi yang sedang diputar sesuai dengan nama yang diinginkan
        return stateInfo.IsName(animName) && stateInfo.normalizedTime >= 1 && !animator.IsInTransition(0);
    }
}
