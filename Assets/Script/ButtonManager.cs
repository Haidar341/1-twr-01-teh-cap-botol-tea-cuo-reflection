using UnityEngine;
using UnityEngine.UI;

public class ButtonManager : MonoBehaviour
{
    public Button controllingButton; // Button yang akan menonaktifkan button lainnya
    public Button[] targetButtons;  // Button-button yang akan dinonaktifkan

    private void Start()
    {
        if (controllingButton != null)
        {
            controllingButton.onClick.AddListener(DisableOtherButtons);
        }
    }

    private void DisableOtherButtons()
    {
        foreach (Button button in targetButtons)
        {
            if (button != null)
            {
                button.interactable = false; // Menonaktifkan button
            }
        }

        Debug.Log("Semua target button telah dinonaktifkan.");
    }
}
