using UnityEngine;
using TMPro; // Untuk TextMeshPro
using System.Linq;
using System.Collections;
using UnityEngine.UI; 


[System.Serializable]
public class TargetObjectInfo
{
    public GameObject maxWaterReachedObject; // Objek yang diaktifkan saat max water tercapai
    public string elementName;
    public Transform targetObject;
    public GameObject currentPanel;
    public GameObject outOfWaterPanel;
    public GameObject specialPanel;
    public TextMeshProUGUI outputText;
    [HideInInspector]
    public int waterAmount = 0;
    public bool isSpecial = false;
    public string panelIdentifier;
    public string customOutputString = "";

    [Header("Animator Settings")]
    public Animator targetAnimator;
    public string maxWaterAnimationName;
    public string preYIncreaseAnimationName;
    public bool disableAnimatorOnMax = false; 

    [Header("Custom Y Increase Settings")]
    public float customYIncreaseAmount = 0f; // Nilai khusus untuk peningkatan pertama
    [HideInInspector]
    public bool hasCustomYIncreaseUsed = false; // Menandai apakah sudah digunakan
     [Header("Y Increase Animation Settings")]
    public GameObject animationIndicator; // Objek yang diaktifkan saat animasi berjalan
}



public class WaterFill : MonoBehaviour
{
    [Header("Pengaturan Peningkatan")]
    public float yIncreaseAmount = 1.0f;
    public float heightIncreaseDuration = 1.0f;

    [Header("Pengaturan Delay")]
    public float heightIncreaseDelay = 0.8f;
    public float panelActivationDelay = 1.5f;
    public float panelDeactivationDelay = 1.5f;

    [Header("Pengaturan Air")]
    public int pourAmount = 10;
    public int totalAmount = 100;

    [Header("Pengaturan UI")]
    public Button pourButton;
    [Header("Pengaturan Overlay")]
    public GameObject overlayObject; // Referensi ke objek overlay
    private CanvasGroup overlayCanvasGroup; // Declare this variable

    [Header("Target Objects Info")]
    public TargetObjectInfo[] targetObjectInfos;

    private static bool isInitialized = false;
    private int sharedWaterAmount;

    void Start()
    {
        // Existing initialization code
        if (!isInitialized)
        {
            PlayerPrefs.DeleteAll();
            sharedWaterAmount = totalAmount;
            SaveWaterData();
            isInitialized = true;
            Debug.Log("PlayerPrefs direset. Jumlah Air Awal: " + totalAmount);
        }
        else
        {
            sharedWaterAmount = PlayerPrefs.GetInt("SharedWaterAmount", totalAmount);
            Debug.Log("Jumlah Air Awal diambil dari PlayerPrefs.");
        }

        foreach (var info in targetObjectInfos)
        {
            info.waterAmount = totalAmount - sharedWaterAmount;
        }

        ActivateTargetWithMaxWater();
        DebugAllTargets();
        UpdateAllTextOutputs();

        // Overlay initialization code
        if (overlayObject != null)
        {
            overlayCanvasGroup = overlayObject.GetComponent<CanvasGroup>(); // Fetch CanvasGroup component
        }
    }

    IEnumerator TriggerMaxWaterAnimation(TargetObjectInfo info)
    {
        Debug.Log($"Memulai animasi batas maksimum untuk target: {info.targetObject.name}");

        Vector3 startPosition = info.targetObject.position;
        Vector3 endPosition = startPosition + new Vector3(0, 2.0f, 0); // Contoh animasi naik 2 unit pada sumbu Y

        float duration = 1.5f; // Durasi animasi
        float elapsedTime = 0;

        while (elapsedTime < duration)
        {
            info.targetObject.position = Vector3.Lerp(startPosition, endPosition, elapsedTime / duration);
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        info.targetObject.position = endPosition;

        Debug.Log($"Animasi selesai untuk target: {info.targetObject.name}");
    }

    IEnumerator DelayedPanelActivation(GameObject panelToActivate)
    {
        if (panelActivationDelay > 0f)
        {
            yield return new WaitForSeconds(panelActivationDelay);
        }

        if (panelToActivate != null)
        {
            panelToActivate.SetActive(true);
            Debug.Log($"Panel {panelToActivate.name} diaktifkan setelah delay {panelActivationDelay} detik.");
        }
    }

    private bool IsWaterDistributedEvenly(out int evenAmount)
    {
        var nonSpecialTargets = targetObjectInfos.Where(t => !t.isSpecial).ToList();
        var waterAmounts = nonSpecialTargets.Select(t => t.waterAmount).Distinct().ToList();
        
        evenAmount = nonSpecialTargets.FirstOrDefault()?.waterAmount ?? 0;
        return waterAmounts.Count == 1 && nonSpecialTargets.Count > 0;
    }

    IEnumerator DelayedPanelDeactivation(GameObject panelToDeactivate)
    {
        if (panelDeactivationDelay > 0f)
        {
            yield return new WaitForSeconds(panelDeactivationDelay);
        }

        if (panelToDeactivate != null)
        {
            panelToDeactivate.SetActive(false);
            Debug.Log($"Panel {panelToDeactivate.name} dinonaktifkan setelah delay {panelDeactivationDelay} detik.");
        }
    }

private int lastTargetIndex = -1; // Indeks target yang terakhir diisi

public void PourWater()
{
    int remainingPourAmount = pourAmount;
    int maxAllowedWater = 45; // Batas maksimum air
    int lastTargetIndex = -1;

    while (remainingPourAmount > 0)
    {
        var activeTarget = GetActiveTarget();

        if (activeTarget != null && sharedWaterAmount > 0)
        {
            int currentWaterAmount = activeTarget.waterAmount;
            int allowedPourAmount = Mathf.Max(0, maxAllowedWater - currentWaterAmount);

            if (currentWaterAmount >= maxAllowedWater)
            {
                Debug.Log($"Batas maksimum air {maxAllowedWater} tercapai untuk {activeTarget.targetObject.name}.");
                
                // Aktifkan objek jika max water tercapai
                if (activeTarget.maxWaterReachedObject != null)
                {
                    activeTarget.maxWaterReachedObject.SetActive(true);
                    Debug.Log($"Objek {activeTarget.maxWaterReachedObject.name} diaktifkan untuk target: {activeTarget.targetObject.name}.");
                }

                if (activeTarget.disableAnimatorOnMax && activeTarget.targetAnimator != null)
                {
                    activeTarget.targetAnimator.enabled = false;
                    Debug.Log($"Animator dimatikan untuk target: {activeTarget.targetObject.name}.");
                }

                break;
            }

            int waterToPour = Mathf.Min(remainingPourAmount, sharedWaterAmount, allowedPourAmount);

            sharedWaterAmount -= waterToPour;
            activeTarget.waterAmount += waterToPour;
            remainingPourAmount -= waterToPour;

            lastTargetIndex = System.Array.IndexOf(targetObjectInfos, activeTarget);

            SaveWaterData();

            CoroutineManager.Instance.StartManagedCoroutine(AnimateHeightIncrease(activeTarget));

            Debug.Log($"Air dikeluarkan {waterToPour} untuk {activeTarget.targetObject.name}. Sisa air bersama: {sharedWaterAmount}");

            if (sharedWaterAmount <= 0)
            {
                Debug.Log("Semua air telah habis. Memicu kondisi out of water.");
                CoroutineManager.Instance.StartManagedCoroutine(DelayedTriggerOutOfWater());
                break;
            }
        }
        else
        {
            Debug.Log("Tidak ada air yang cukup untuk dikeluarkan atau tidak ada target aktif!");
            break;
        }
    }

    if (lastTargetIndex != -1)
    {
        ActivatePanelForLastTarget(lastTargetIndex); // Aktifkan panel target terakhir yang diisi
    }

    UpdateAllTextOutputs();

    // Cek apakah jumlah air sudah mencapai totalAmount, jika ya nonaktifkan tombol
    if (sharedWaterAmount >= totalAmount && pourButton != null)
    {
        pourButton.interactable = false;  // Nonaktifkan tombol pour
        Debug.Log("Jumlah air sudah mencapai totalAmount. Tombol 'Pour' dinonaktifkan.");
    }
}



   IEnumerator WaitForAnimationToFinish(Animator animator, string animationName, TargetObjectInfo info)
{
    // Cek apakah animator atau nama animasi valid
    if (animator == null || string.IsNullOrEmpty(animationName))
    {
        Debug.LogWarning("Animator atau nama animasi tidak diatur. Langsung ke Y Increase.");
        yield return AnimateHeightIncrease(info);
        yield break;
    }

    // Memutar animasi PreYIncreaseAnimation jika ada
    if (!string.IsNullOrEmpty(info.preYIncreaseAnimationName))
    {
        animator.Play(info.preYIncreaseAnimationName);
        Debug.Log($"Animasi '{info.preYIncreaseAnimationName}' diputar untuk target {info.targetObject.name}.");

        // Tunggu hingga animasi PreYIncrease selesai
        while (animator.GetCurrentAnimatorStateInfo(0).IsName(info.preYIncreaseAnimationName) &&
               animator.GetCurrentAnimatorStateInfo(0).normalizedTime < 1f)
        {
            yield return null; // Tunggu frame berikutnya
        }

        Debug.Log($"Animasi '{info.preYIncreaseAnimationName}' selesai untuk target {info.targetObject.name}.");
    }

    // Memutar animasi utama jika nama animasi diberikan
    if (!string.IsNullOrEmpty(animationName))
    {
        animator.Play(animationName);
        Debug.Log($"Animasi '{animationName}' diputar untuk target {info.targetObject.name}.");

        // Tunggu hingga animasi utama selesai
        while (animator.GetCurrentAnimatorStateInfo(0).IsName(animationName) &&
               animator.GetCurrentAnimatorStateInfo(0).normalizedTime < 1f)
        {
            yield return null; // Tunggu frame berikutnya
        }

        Debug.Log($"Animasi '{animationName}' selesai untuk target {info.targetObject.name}.");
    }

    // Lanjutkan ke peningkatan posisi Y
    Debug.Log($"Memulai peningkatan posisi Y untuk target {info.targetObject.name}.");
    yield return AnimateHeightIncrease(info);
}



    void SaveWaterData()
    {
        PlayerPrefs.SetInt("SharedWaterAmount", sharedWaterAmount);
        PlayerPrefs.Save();
    }

 IEnumerator AnimateHeightIncrease(TargetObjectInfo info)
{
    if (info.targetObject != null)
    {
        float yIncrease = info.hasCustomYIncreaseUsed ? yIncreaseAmount : info.customYIncreaseAmount;

        // Jika customYIncreaseAmount tidak diatur, gunakan yIncreaseAmount default
        if (!info.hasCustomYIncreaseUsed && yIncrease == 0f)
        {
            yIncrease = yIncreaseAmount;
        }

        // Tandai bahwa customYIncreaseAmount sudah digunakan
        info.hasCustomYIncreaseUsed = true;

        // Tampilkan game object indikator animasi jika diatur
        if (info.animationIndicator != null)
        {
            info.animationIndicator.SetActive(true);
            Debug.Log($"Game object {info.animationIndicator.name} diaktifkan untuk {info.targetObject.name} saat animasi dimulai.");
        }

        // Menunggu selama delay sebelum memulai peningkatan
        if (heightIncreaseDelay > 0f)
        {
            Debug.Log($"Menunggu delay selama {heightIncreaseDelay} detik sebelum meningkatkan posisi Y untuk {info.targetObject.name}.");
            yield return new WaitForSeconds(heightIncreaseDelay);
        }

        Vector3 startPosition = info.targetObject.position;
        Vector3 endPosition = startPosition + new Vector3(0, yIncrease, 0);

        float elapsedTime = 0;

        Debug.Log($"Memulai peningkatan posisi Y sebesar {yIncrease} untuk {info.targetObject.name} selama {heightIncreaseDuration} detik.");
        while (elapsedTime < heightIncreaseDuration)
        {
            info.targetObject.position = Vector3.Lerp(startPosition, endPosition, elapsedTime / heightIncreaseDuration);
            elapsedTime += Time.deltaTime;
            yield return null; // Tunggu frame berikutnya
        }

        info.targetObject.position = endPosition;
        Debug.Log($"Peningkatan posisi Y selesai untuk {info.targetObject.name}.");

        // Nonaktifkan game object indikator animasi jika diatur
        if (info.animationIndicator != null)
        {
            info.animationIndicator.SetActive(false);
            Debug.Log($"Game object {info.animationIndicator.name} dinonaktifkan untuk {info.targetObject.name} setelah animasi selesai.");
        }
    }
}

     IEnumerator DelayedPanelActivation(GameObject panelToActivate, float delay)
    {
        if (delay > 0f)
        {
            yield return new WaitForSeconds(delay);
        }

        if (panelToActivate != null)
        {
            panelToActivate.SetActive(true);
            Debug.Log($"Panel {panelToActivate.name} diaktifkan setelah delay {delay} detik.");
        }
    }
    IEnumerator DelayedPanelDeactivation(GameObject panelToDeactivate, float delay)
    {
        if (delay > 0f)
        {
            yield return new WaitForSeconds(delay);
        }

        if (panelToDeactivate != null)
        {
            panelToDeactivate.SetActive(false);
            Debug.Log($"Panel {panelToDeactivate.name} dinonaktifkan setelah delay {delay} detik.");
        }
    }

  IEnumerator DelayedTriggerOutOfWater()
{
      if (overlayObject != null)
    {
        overlayObject.SetActive(true);
        Debug.Log("Overlay diaktifkan untuk mencegah interaksi.");
    }

    // Tunggu beberapa detik untuk memberi efek transisi
    yield return new WaitForSeconds(2f);

    TriggerOutOfWaterState();
    
    // Setelah perpindahan selesai, matikan overlay
    if (overlayObject != null)
    {
        overlayObject.SetActive(false);
        Debug.Log("Overlay dinonaktifkan, interaksi diaktifkan kembali.");
    }
    
    yield return new WaitForSeconds(2f); // Memberikan jeda tambahan jika diperlukan
    TriggerOutOfWaterState();
}

void TriggerOutOfWaterState()
{
    if (sharedWaterAmount > 0)
    {
        Debug.LogWarning("TriggerOutOfWaterState dipanggil meskipun air belum habis.");
        return;
    }

    // Nonaktifkan semua panel
    foreach (var info in targetObjectInfos)
    {
        if (info.outOfWaterPanel != null) info.outOfWaterPanel.SetActive(false);
        if (info.specialPanel != null) info.specialPanel.SetActive(false);
    }

    // Cek special target
    var specialTarget = targetObjectInfos.FirstOrDefault(info => info.isSpecial && info.waterAmount > 0);
    if (specialTarget != null)
    {
        int maxWaterAmount = targetObjectInfos.Max(info => info.waterAmount);
        if (specialTarget.waterAmount >= maxWaterAmount)
        {
            ActivateSpecialPanel(specialTarget);
            return;
        }
    }

    // Cek target dengan air tertinggi
    int highestWater = targetObjectInfos.Where(info => !info.isSpecial).Max(info => info.waterAmount);
    var equalTargets = targetObjectInfos
        .Where(info => !info.isSpecial && 
                      info.waterAmount == highestWater && 
                      info.waterAmount > 0)
        .ToList();

    // Jika ada target setara dan lastTargetIndex valid
    if (equalTargets.Count > 1 && lastTargetIndex >= 0 && lastTargetIndex < targetObjectInfos.Length)
    {
        var lastFilled = targetObjectInfos[lastTargetIndex];
        // Pastikan target terakhir memiliki air yang sama
        if (equalTargets.Contains(lastFilled))
        {
            if (lastFilled.outOfWaterPanel != null)
            {
                Debug.Log($"Mengaktifkan panel out of water untuk target terakhir: {lastFilled.elementName}");
                CoroutineManager.Instance.StartManagedCoroutine(
                    DelayedPanelActivation(lastFilled.outOfWaterPanel, panelActivationDelay)
                );
                return;
            }
        }
    }

    // Default: aktivasi panel untuk air tertinggi
    ActivatePanelForHighestWater();
    UpdateAllTextOutputs();
}
void ActivatePanelForHighestWater()
{
    var targetWithMaxWater = targetObjectInfos
        .Where(info => info.waterAmount > 0)
        .OrderByDescending(info => info.waterAmount)
        .FirstOrDefault();

    if (targetWithMaxWater != null && targetWithMaxWater.outOfWaterPanel != null)
    {
        CoroutineManager.Instance.StartManagedCoroutine(DelayedPanelActivation(targetWithMaxWater.outOfWaterPanel, panelActivationDelay));
        Debug.Log($"Panel outOfWater diaktifkan untuk target: {targetWithMaxWater.targetObject.name}");
    }
    else
    {
        Debug.Log("Semua target telah kehabisan air.");
    }
}

void ActivateSpecialPanel(TargetObjectInfo specialTarget)
{
    foreach (var info in targetObjectInfos)
    {
        if (info.currentPanel != null)
        {
            CoroutineManager.Instance.StartManagedCoroutine(DelayedPanelDeactivation(info.currentPanel, 0f));
        }

        if (info.outOfWaterPanel != null)
        {
            CoroutineManager.Instance.StartManagedCoroutine(DelayedPanelDeactivation(info.outOfWaterPanel, 0f));
        }

        if (info.specialPanel != null)
        {
            CoroutineManager.Instance.StartManagedCoroutine(DelayedPanelDeactivation(info.specialPanel, 0f));
        }
    }

    if (specialTarget.specialPanel != null)
    {
        CoroutineManager.Instance.StartManagedCoroutine(DelayedPanelActivation(specialTarget.specialPanel, 0f));
        Debug.Log($"Panel khusus diaktifkan untuk: {specialTarget.targetObject.name}");
    }
}


  void ActivateTargetWithMaxWater()
{
    int maxWaterAmount = targetObjectInfos.Max(info => info.waterAmount);
    var candidates = targetObjectInfos
        .Where(info => info.waterAmount == maxWaterAmount && info.waterAmount > 0)
        .ToList();

    TargetObjectInfo targetWithMaxWater = null;

    if (candidates.Count > 1)
    {
        // Prioritaskan target yang terakhir diisi
        targetWithMaxWater = candidates.FirstOrDefault(info => System.Array.IndexOf(targetObjectInfos, info) == lastTargetIndex);

        if (targetWithMaxWater == null)
        {
            // Jika lastTargetIndex tidak valid, pilih kandidat terakhir di daftar
            targetWithMaxWater = candidates.Last();
        }
    }
    else if (candidates.Count == 1)
    {
        targetWithMaxWater = candidates[0];
    }

    // Nonaktifkan semua panel kecuali currentPanel untuk target yang aktif
    foreach (var info in targetObjectInfos)
    {
        if (info.currentPanel != null && info != targetWithMaxWater)
        {
            info.currentPanel.SetActive(false);
        }

        if (info.outOfWaterPanel != null)
        {
            info.outOfWaterPanel.SetActive(false);
        }

        if (info.specialPanel != null)
        {
            info.specialPanel.SetActive(false);
        }
    }

    if (targetWithMaxWater != null)
    {
        // Aktifkan currentPanel untuk target dengan air tertinggi
        if (targetWithMaxWater.currentPanel != null)
        {
            targetWithMaxWater.currentPanel.SetActive(true);
            Debug.Log($"Panel current aktif untuk target: {targetWithMaxWater.targetObject.name}");
        }

        // Aktifkan outOfWaterPanel jika memenuhi syarat
        if (targetWithMaxWater.waterAmount >= 80 && targetWithMaxWater.outOfWaterPanel != null)
        {
            targetWithMaxWater.outOfWaterPanel.SetActive(true);
            Debug.Log($"Panel outOfWater aktif untuk target: {targetWithMaxWater.targetObject.name}");
        }

        // Aktifkan specialPanel jika target adalah spesial
        if (targetWithMaxWater.isSpecial && targetWithMaxWater.specialPanel != null)
        {
            targetWithMaxWater.specialPanel.SetActive(true);
            Debug.Log($"Panel special aktif untuk target: {targetWithMaxWater.targetObject.name}");
        }
    }
    else
    {
        Debug.Log("Tidak ada target dengan air yang cukup untuk diaktifkan.");
    }

    DisplayTopWaterTarget();
    UpdateAllTextOutputs();
}


     TargetObjectInfo GetActiveTarget()
    {
        var target = targetObjectInfos.FirstOrDefault(info => info.currentPanel != null && info.currentPanel.activeSelf);

        if (target != null)
        {
            Debug.Log($"Target aktif ditemukan: {target.targetObject.name}");
        }
        else
        {
            Debug.Log("Tidak ada target aktif yang ditemukan.");
        }

        return target;
    }

    public void ActivatePanelByIdentifier(string identifier)
    {
        foreach (var info in targetObjectInfos)
        {
            if (info.panelIdentifier == identifier)
            {
                foreach (var otherInfo in targetObjectInfos)
                {
                    if (otherInfo.currentPanel != null)
                    {
                        otherInfo.currentPanel.SetActive(false);
                    }
                }

                if (info.currentPanel != null)
                {
                    info.currentPanel.SetActive(true);
                    Debug.Log($"Panel diaktifkan untuk identifier: {identifier}, Target: {info.targetObject.name}");
                }
                else
                {
                    Debug.LogWarning($"Panel tidak ditemukan untuk identifier: {identifier}");
                }

                return;
            }
        }

        Debug.LogWarning($"Tidak ada target yang cocok dengan identifier: {identifier}");
    }

    void EnsureSingleCurrentPanelActive()
{
    bool isAnyCurrentPanelActive = false;

    foreach (var info in targetObjectInfos)
    {
        if (info.currentPanel != null && info.currentPanel.activeSelf)
        {
            if (isAnyCurrentPanelActive)
            {
                info.currentPanel.SetActive(false);
                Debug.Log($"Menonaktifkan panel ganda: {info.targetObject.name}");
            }
            else
            {
                isAnyCurrentPanelActive = true;
            }
        }
    }
}


   void ActivatePanelForLastTarget(int targetIndex)
{
    for (int i = 0; i < targetObjectInfos.Length; i++)
    {
        if (targetObjectInfos[i].currentPanel != null)
        {
            targetObjectInfos[i].currentPanel.SetActive(i == targetIndex); // Aktifkan hanya panel target terakhir
        }
    }

    Debug.Log($"Panel diaktifkan untuk target terakhir yang diisi: {targetObjectInfos[targetIndex].targetObject.name}");

    // Pastikan hanya satu panel aktif
    EnsureSingleCurrentPanelActive();
}


    void DebugAllTargets()
    {
        foreach (var info in targetObjectInfos)
        {
            Debug.Log($"Target: {info.targetObject.name}, WaterAmount: {info.waterAmount}, IsSpecial: {info.isSpecial}");
        }
    }

    [Header("Pesan Debug")]
public string specialTargetMessage = "Target khusus '{0}' memiliki {1} air.";
public string highestTargetMessage = "Target dengan air tertinggi adalah '{0}' dengan {1} air.";

    void DisplayTopWaterTarget()
{
    var specialTarget = targetObjectInfos.FirstOrDefault(info => info.isSpecial && info.waterAmount > 0);

    if (specialTarget != null)
    {
        Debug.Log(string.Format(specialTargetMessage, specialTarget.elementName, specialTarget.waterAmount));
        return;
    }

    var highestWaterTarget = targetObjectInfos
        .Where(info => info.waterAmount > 0)
        .OrderByDescending(info => info.waterAmount)
        .FirstOrDefault();

    if (highestWaterTarget != null)
    {
        Debug.Log(string.Format(highestTargetMessage, highestWaterTarget.elementName, highestWaterTarget.waterAmount));
    }
}


   void UpdateAllTextOutputs()
    {
        foreach (var info in targetObjectInfos)
        {
            if (info.outputText != null)
            {
                info.outputText.text = info.waterAmount.ToString();
            }
        }
    }
}