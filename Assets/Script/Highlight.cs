using UnityEngine;
using UnityEngine.UI;

public class Highlight : MonoBehaviour
{
    // Simpan referensi ke Outline Component
    private Outline outline;

    // Simpan referensi ke gambar yang terakhir di-highlight
    private static Highlight lastHighlighted;

    private void Start()
    {
        // Ambil Outline Component di GameObject ini
        outline = GetComponent<Outline>();

        if (outline != null)
        {
            // Nonaktifkan outline saat awal
            outline.enabled = false;
        }
    }

    public void OnClick()
    {
        if (outline != null)
        {
            // Jika ada gambar lain yang sebelumnya di-highlight, nonaktifkan outline-nya
            if (lastHighlighted != null && lastHighlighted != this)
            {
                lastHighlighted.DisableOutline();
            }

            // Aktifkan atau nonaktifkan outline untuk gambar ini
            outline.enabled = !outline.enabled;

            // Perbarui referensi gambar terakhir yang di-highlight
            lastHighlighted = outline.enabled ? this : null;
        }
    }

    private void DisableOutline()
    {
        if (outline != null)
        {
            outline.enabled = false;
        }
    }
}
