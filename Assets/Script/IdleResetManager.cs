using UnityEngine;
using UnityEngine.SceneManagement;

public class IdleResetManager : MonoBehaviour
{
    public float idleTime = 10f; // Waktu dalam detik sebelum game di-reset karena idle
    private float timer = 0f;    // Timer untuk menghitung waktu idle

    void Update()
    {
        // Jika mendeteksi input dari pemain, reset timer
        if (Input.anyKey || Input.GetAxis("Mouse X") != 0 || Input.GetAxis("Mouse Y") != 0)
        {
            timer = 0f;
        }
        else
        {
            timer += Time.deltaTime;
        }

        // Jika waktu idle melebihi batas, reset game
        if (timer >= idleTime)
        {
            ResetGame();
        }
    }

    // Fungsi untuk mereset PlayerPrefs dan me-restart game
    void ResetGame()
    {
        // Hapus semua data di PlayerPrefs
        PlayerPrefs.DeleteAll();
        PlayerPrefs.Save();
        Debug.Log("PlayerPrefs telah direset karena idle.");

        // Restart game dengan memuat ulang scene saat ini
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        Debug.Log("Game telah direset karena idle.");
    }
}
