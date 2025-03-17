using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class PanelButtonManager : MonoBehaviour
{
    public GameObject targetPanel;                 // Panel target aktif
    public List<Button> buttonsToDisable = new List<Button>(); // Semua tombol di panel
    public List<Button> buttonsToKeepActive = new List<Button>();


    void Update()
    {
        if (targetPanel != null && targetPanel.activeSelf)
        {
            DisableButtonsExceptOne();
        }
        else
        {
            EnableButtons();
        }
    }

  void DisableButtonsExceptOne()
{
    foreach (Button button in buttonsToDisable)
    {
        if (button != null)
        {
            button.interactable = buttonsToKeepActive.Contains(button); // Hanya aktifkan tombol yang ada di daftar
        }
    }
}


    void EnableButtons()
    {
        foreach (Button button in buttonsToDisable)
        {
            if (button != null)
            {
                button.interactable = true; // Aktifkan kembali semua tombol
            }
        }
    }

    public void SetTargetPanel(GameObject panel)
    {
        targetPanel = panel;
        UpdateButtonsToDisable(); // Perbarui daftar tombol saat target panel diubah
    }

    public void UpdateButtonsToDisable()
    {
        buttonsToDisable.Clear(); // Hapus daftar lama
        if (targetPanel != null)
        {
            Button[] buttons = targetPanel.GetComponentsInChildren<Button>(true); // Ambil tombol termasuk yang tidak aktif
            buttonsToDisable.AddRange(buttons); // Tambahkan tombol-tombol ke daftar
        }
    }
}
