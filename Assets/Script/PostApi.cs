using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.Networking;
using System.Collections;
using System.Linq;

public class PostApi : MonoBehaviour
{
    [Header("Input Fields")]
    public TMP_InputField namaInputField;
    public TMP_InputField emailInputField;

    [Header("Popup Configuration")]
    public GameObject warningPopup; // Popup untuk menampilkan peringatan
    public TMP_Text warningPopupText; // Text di dalam popup
    public float warningDuration = 2.0f; // Durasi popup dalam detik
    public GameObject nameEmptyIndicator; // GameObject yang ditampilkan saat nama kosong

    [Header("API Configuration")]
    public string apiUrl = "https://example.com/api";
    private readonly string username = "animatronics";
    private readonly string password = "uH7fL9zR5mPqYwK8vW2tJxD3oA0qZg9h";

    [Header("Dependencies")]
    public WaterFill[] waterFills;
    public Image qrCodeImage; // Referensi ke Image UI untuk QR code
    public GameObject qrCodeGameObject; // GameObject QR Code (harus diassign)
    public Button actionButton; // Button yang akan diaktifkan setelah QR sukses

   public void SendDataToApi()
{
    string nama = namaInputField.text;
    string email = emailInputField.text;
    
    // Get the highest water element name from the WaterFill script
    string result = GetHighestWaterElementNameFromAll();

    // Validasi input nama
    if (string.IsNullOrEmpty(nama))
    {
        DisplayWarning("Nama tidak boleh kosong.");
        return;
    }

    if (string.IsNullOrEmpty(result))
    {
        DisplayWarning("Result tidak ditemukan.");
        return;
    }

    // Jika email kosong, tetap kirim dengan nilai string kosong
    email = string.IsNullOrEmpty(email) ? "" : email;

    ApiData data = new ApiData
    {
        name = nama,
        email = email,
        result = result  // Use the result we fetched
    };

    string jsonData = JsonUtility.ToJson(data);
    StartCoroutine(PostToApi(jsonData));
}


    private void DisplayWarning(string message)
    {
        if (string.IsNullOrEmpty(namaInputField.text))
        {
            // Tampilkan indikator nama kosong
            if (nameEmptyIndicator != null)
            {
                nameEmptyIndicator.SetActive(true);
            }
        }
        else if (warningPopup != null)
        {
            // Tampilkan popup peringatan jika diatur
            warningPopupText.text = message;
            warningPopup.SetActive(true);
            StartCoroutine(HideWarningAfterDelay());
        }
    }

    private IEnumerator HideWarningAfterDelay()
    {
        yield return new WaitForSeconds(warningDuration);
        if (warningPopup != null)
        {
            warningPopup.SetActive(false);
            warningPopupText.text = ""; // Hapus teks popup setelah sembunyi
        }
        if (nameEmptyIndicator != null)
        {
            nameEmptyIndicator.SetActive(false); // Sembunyikan indikator nama kosong
        }
    }

    private IEnumerator PostToApi(string jsonData)
    {
        using (UnityWebRequest www = new UnityWebRequest(apiUrl, "POST"))
        {
            byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(jsonData);
            www.uploadHandler = new UploadHandlerRaw(bodyRaw);
            www.downloadHandler = new DownloadHandlerBuffer();
            www.SetRequestHeader("Content-Type", "application/json");

            string credentials = CredentialsAPI(username, password);
            www.SetRequestHeader("Authorization", "Basic " + credentials);

            yield return www.SendWebRequest();

            if (www.result == UnityWebRequest.Result.Success)
            {
                Debug.Log("Berhasil mengirim data.");
                yield return HandleQrCodeResponse(www.downloadHandler.data);
            }
            else
            {
                Debug.LogError("Gagal mengirim data: " + www.error);
            }
        }
    }

    private IEnumerator HandleQrCodeResponse(byte[] responseData)
    {
        if (responseData == null || responseData.Length == 0)
        {
            Debug.LogError("Data respons kosong.");
            yield break;
        }

        Texture2D qrTexture = new Texture2D(2, 2);
        if (qrTexture.LoadImage(responseData))
        {
            // Aktifkan QR Code Image dan tampilkan sprite
            qrCodeGameObject.SetActive(true); // Aktifkan GameObject QR
            qrCodeImage.sprite = Sprite.Create(qrTexture, new Rect(0, 0, qrTexture.width, qrTexture.height), new Vector2(0.5f, 0.5f));
            Debug.Log("QR code berhasil ditampilkan.");

            // Aktifkan Button setelah QR Code sukses
            if (actionButton != null)
            {
                actionButton.gameObject.SetActive(true);
                Debug.Log("Button berhasil diaktifkan.");
            }

            DisableInputFields();
        }
        else
        {
            Debug.LogError("Gagal memuat gambar QR dari respons.");
        }
    }

    private void DisableInputFields()
    {
        namaInputField.interactable = false;
        emailInputField.interactable = false;

        if (ColorUtility.TryParseHtmlString("#B5B5B5", out Color grayColor))
        {
            namaInputField.textComponent.color = grayColor;
            emailInputField.textComponent.color = grayColor;
        }
        else
        {
            Debug.LogError("Gagal mengatur warna teks ke #B5B5B5.");
        }
    }

    public string CredentialsAPI(string username, string password)
    {
        return System.Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(username + ":" + password));
    }

   private string GetHighestWaterElementNameFromAll()
{
    if (waterFills == null || waterFills.Length == 0)
    {
        Debug.LogError("Tidak ada WaterFill yang terdaftar.");
        return null;
    }

    string resultElementName = null;
    int highestAmount = -1;

    foreach (var waterFill in waterFills)
    {
        // Cek target spesial terlebih dahulu
        var specialTarget = waterFill.targetObjectInfos
            .FirstOrDefault(info => info.isSpecial && info.waterAmount > 0);

        if (specialTarget != null)
        {
            int maxWaterAmount = waterFill.targetObjectInfos.Max(info => info.waterAmount);
            
            // Jika target spesial memiliki air yang setara atau lebih tinggi
            if (specialTarget.waterAmount >= maxWaterAmount)
            {
                resultElementName = specialTarget.elementName;
                highestAmount = specialTarget.waterAmount;
                continue;
            }
        }

        // Jika tidak ada target spesial atau airnya lebih rendah
        var highestTarget = waterFill.targetObjectInfos
            .Where(info => info.waterAmount > 0)
            .OrderByDescending(info => info.waterAmount)
            .FirstOrDefault();

        if (highestTarget != null && highestTarget.waterAmount > highestAmount)
        {
            resultElementName = highestTarget.elementName;
            highestAmount = highestTarget.waterAmount;
        }
    }

    return resultElementName;
}

    [System.Serializable]
    public class ApiData
    {
        public string name;
        public string email;
        public string result;
    }
}
