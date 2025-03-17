using UnityEngine;
using UnityEngine.UI;

public class PanelSwitcher : MonoBehaviour
{
    public GameObject currentPanel; // Panel yang sedang aktif
    public GameObject nextPanel; // Panel yang akan ditampilkan
    public Button switchButton; // Tombol untuk mengganti panel

    public float animationDuration = 0.5f; // Durasi animasi
    public Vector2 slideOffset = new Vector2(-1920, 0); // Offset geser (horizontal atau vertikal tergantung kebutuhan)

    void Start()
    {
        // Pastikan tombol memiliki fungsi klik yang terdaftar
        if (switchButton != null)
        {
            switchButton.onClick.AddListener(SwitchPanel);
        }
    }

    void SwitchPanel()
    {
        if (currentPanel != null && nextPanel != null)
        {
            // Animasi untuk menggeser panel keluar
            LeanTween.move(currentPanel.GetComponent<RectTransform>(),
                currentPanel.GetComponent<RectTransform>().anchoredPosition + slideOffset,
                animationDuration).setOnComplete(() => 
            {
                currentPanel.SetActive(false); // Nonaktifkan panel setelah animasi selesai
            });

            // Posisikan nextPanel di luar layar (kebalikan dari slideOffset)
            RectTransform nextRect = nextPanel.GetComponent<RectTransform>();
            nextRect.anchoredPosition = currentPanel.GetComponent<RectTransform>().anchoredPosition - slideOffset;
            nextPanel.SetActive(true);

            // Animasi untuk menggeser nextPanel masuk
            LeanTween.move(nextRect,
                currentPanel.GetComponent<RectTransform>().anchoredPosition,
                animationDuration);
        }
    }
}
