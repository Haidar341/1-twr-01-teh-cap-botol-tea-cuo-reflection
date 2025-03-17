using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class DelayPanelActivation : MonoBehaviour
{
    public GameObject panel; // Panel yang akan diaktifkan
    public Button button;    // Tombol untuk mengaktifkan panel
    public float delay = 2f; // Waktu delay dalam detik

    private void Start()
    {
        // Pastikan panel dalam keadaan tidak aktif di awal
        if (panel != null)
            panel.SetActive(false);

        // Tambahkan listener ke tombol
        if (button != null)
            button.onClick.AddListener(StartDelay);
    }

    private void StartDelay()
    {
        if (panel != null)
            StartCoroutine(ActivatePanelAfterDelay());
    }

    private IEnumerator ActivatePanelAfterDelay()
    {
        yield return new WaitForSeconds(delay);
        panel.SetActive(true);
    }
}
