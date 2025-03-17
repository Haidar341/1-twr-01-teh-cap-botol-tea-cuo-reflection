
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using System.Collections;
using System.Collections.Generic;

public class ObjectDuplicator : MonoBehaviour
{
    public GameObject originalObject;          // Objek asli yang akan diduplikasi
    public Transform[] targetPanels;           // Target panel tempat objek akan diduplikasi
    public PanelButtonManager panelButtonManager; // Referensi ke PanelButtonManager

    [System.Serializable]
    public class Customization
    {
        public Vector3 customPosition = Vector3.zero;
        public Vector3 customScale = Vector3.one;
        public Vector2 customSize = new Vector2(100, 100);
        public bool useFadeIn = false;
        public bool useFadeOut = false;
        public bool applyToChild = false;

        public float fadeInDuration = 1.0f;
        public float fadeOutDuration = 1.0f;
        public bool waitForAnimation = false;
        public AnimationClip animationClip;
    }

    [Header("Customization Settings")]
    public Customization[] customizations;

    [Header("Button Customization")]
    public UnityEvent buttonAction;

    [Header("Child Duplication")]
    public GameObject specificChild;

    private Dictionary<int, GameObject> existingClones = new Dictionary<int, GameObject>();
    private Coroutine currentCoroutine;

    public void DuplicateObject(int index)
    {
        if (originalObject == null || targetPanels.Length == 0)
        {
            Debug.LogWarning("OriginalObject atau TargetPanels belum diatur.");
            return;
        }

        if (index < 0 || index >= targetPanels.Length)
        {
            Debug.LogWarning($"Indeks {index} di luar jangkauan targetPanels.");
            return;
        }

        Transform targetPanel = targetPanels[index];
        if (targetPanel == null) return;

        if (existingClones.ContainsKey(index) && existingClones[index] != null)
        {
            Destroy(existingClones[index]);
            existingClones.Remove(index);
        }

        GameObject targetToDuplicate = specificChild != null ? specificChild : originalObject;

        Customization currentCustomization = (index < customizations.Length) ? customizations[index] : new Customization();

        GameObject clone = Instantiate(targetToDuplicate, targetPanel);
        existingClones[index] = clone;

        ApplyCustomization(clone, currentCustomization);

        // Mengonversi Button[] menjadi List<Button>
        Button[] buttons = clone.GetComponentsInChildren<Button>();
        if (buttons != null && panelButtonManager != null)
        {
            List<Button> buttonList = new List<Button>(buttons);
            panelButtonManager.buttonsToDisable = buttonList; // Menonaktifkan tombol di dalam clone
            panelButtonManager.targetPanel = targetPanel.gameObject; // Menghubungkan panel
        }

        Button button = clone.GetComponentInChildren<Button>();
        if (button != null)
        {
            button.gameObject.SetActive(true);
            button.interactable = true;
            button.onClick.RemoveAllListeners();
            button.onClick.AddListener(() =>
            {
                buttonAction.Invoke();
                FadeOutObject(index);
            });
        }

        // Hentikan Coroutine sebelumnya jika ada
        if (currentCoroutine != null)
        {
            CoroutineManager.Instance.StopManagedCoroutine(currentCoroutine);
        }

        if (currentCustomization.waitForAnimation && currentCustomization.animationClip != null)
        {
            currentCoroutine = CoroutineManager.Instance.StartManagedCoroutine(WaitForAnimationThenFade(clone, currentCustomization));
        }
        else if (currentCustomization.useFadeIn)
        {
            currentCoroutine = CoroutineManager.Instance.StartManagedCoroutine(AnimateFade(clone, true, currentCustomization.fadeInDuration));
        }
    }

    public void FadeOutObject(int index)
    {
        if (!existingClones.ContainsKey(index) || existingClones[index] == null)
        {
            Debug.LogWarning($"Tidak ada clone untuk panel {index} yang dapat di-fade-out.");
            return;
        }

        GameObject clone = existingClones[index];
        Customization currentCustomization = (index < customizations.Length) ? customizations[index] : new Customization();

        if (currentCoroutine != null)
        {
            CoroutineManager.Instance.StopManagedCoroutine(currentCoroutine);
        }

        if (clone.activeSelf && currentCustomization.useFadeOut)
        {
            currentCoroutine = CoroutineManager.Instance.StartManagedCoroutine(AnimateFade(clone, false, currentCustomization.fadeOutDuration));
        }
        else
        {
            Debug.LogWarning($"Clone di panel {index} sudah tidak aktif atau fade out tidak diatur.");
        }
    }

    private void ApplyCustomization(GameObject obj, Customization customization)
    {
        GameObject targetObject = customization.applyToChild && specificChild != null
            ? obj.transform.Find(specificChild.name)?.gameObject
            : obj;

        if (targetObject == null)
        {
            Debug.LogWarning("Child yang dimaksud tidak ditemukan.");
            return;
        }

        targetObject.transform.localPosition = customization.customPosition;
        targetObject.transform.localScale = customization.customScale;

        RectTransform rectTransform = targetObject.GetComponent<RectTransform>();
        if (rectTransform != null)
        {
            rectTransform.sizeDelta = customization.customSize;
        }

        if (customization.animationClip != null)
        {
            Animator animator = obj.GetComponent<Animator>();
            if (animator != null)
            {
                animator.Play(customization.animationClip.name);
            }
        }
    }

    private IEnumerator WaitForAnimationThenFade(GameObject obj, Customization customization)
    {
        Animator animator = obj.GetComponent<Animator>();
        if (animator == null || customization.animationClip == null)
        {
            Debug.LogWarning("Animator atau AnimationClip tidak ditemukan.");
            yield break;
        }

        AnimatorStateInfo stateInfo = animator.GetCurrentAnimatorStateInfo(0);
        while (stateInfo.normalizedTime < 1.0f)
        {
            yield return null;
            stateInfo = animator.GetCurrentAnimatorStateInfo(0);
        }

        if (customization.useFadeIn)
        {
            currentCoroutine = CoroutineManager.Instance.StartManagedCoroutine(AnimateFade(obj, true, customization.fadeInDuration));
        }
    }

    private IEnumerator AnimateFade(GameObject obj, bool fadeIn, float duration)
    {
        if (obj == null || !obj.activeInHierarchy)
        {
            yield break;
        }

        CanvasGroup canvasGroup = obj.GetComponent<CanvasGroup>();
        if (canvasGroup == null)
        {
            canvasGroup = obj.AddComponent<CanvasGroup>();
        }

        float startAlpha = fadeIn ? 0 : 1;
        float endAlpha = fadeIn ? 1 : 0;

        float time = 0;
        while (time < duration)
        {
            time += Time.deltaTime;
            canvasGroup.alpha = Mathf.Lerp(startAlpha, endAlpha, time / duration);
            yield return null;
        }
        canvasGroup.alpha = endAlpha;

        if (!fadeIn)
        {
            obj.SetActive(false);
        }
    }
} 