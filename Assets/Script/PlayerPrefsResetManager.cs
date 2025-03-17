using UnityEngine;
using UnityEngine.SceneManagement;

public class PlayerPrefsResetManager : MonoBehaviour
{
    // Fungsi untuk mereset semua data di PlayerPrefs dan me-restart game
    public void ResetPlayerPrefsAndRestart()
    {
        // Hapus semua data di PlayerPrefs
        PlayerPrefs.DeleteAll();
        PlayerPrefs.Save();
        Debug.Log("PlayerPrefs telah direset.");

        // Restart game dengan memuat ulang scene saat ini
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        Debug.Log("Game telah direset.");
    }
}
