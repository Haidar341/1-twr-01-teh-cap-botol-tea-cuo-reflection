using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.Networking;

public class ElementOutputManager : MonoBehaviour
{
    [Header("UI References")]
    public Text outputText; // UI Text untuk menampilkan output

    [Header("API Configuration")]
    public string apiUrl = "https://example.com/api/receive"; // URL API tujuan

    [Header("Target Elements")]
    public WaterFill waterFillScript; // Referensi ke script WaterFill

    [Header("Element Output Settings")]
    public Dictionary<string, string> elementOutputs = new Dictionary<string, string>();

    private void Start()
    {
        if (waterFillScript == null)
        {
            Debug.LogError("WaterFill script tidak diassign pada ElementOutputManager.");
        }

        GenerateElementOutputs();
    }

    private void GenerateElementOutputs()
    {
        foreach (var info in waterFillScript.targetObjectInfos)
        {
            string output = $"Element: {info.targetObject.name}, Water Amount: {info.waterAmount}, Is Special: {info.isSpecial}";
            elementOutputs[info.targetObject.name] = output;
        }
    }

    public void DisplayOutput(string elementName)
    {
        if (elementOutputs.TryGetValue(elementName, out string output))
        {
            outputText.text = output; // Tampilkan output ke UI Text
            Debug.Log("Output ditampilkan: " + output);
            StartCoroutine(SendToAPI(output));
        }
        else
        {
            Debug.LogError("Element dengan nama " + elementName + " tidak ditemukan.");
        }
    }

    private IEnumerator SendToAPI(string output)
    {
        UnityWebRequest request = new UnityWebRequest(apiUrl, "POST");
        byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(output);
        request.uploadHandler = new UploadHandlerRaw(bodyRaw);
        request.downloadHandler = new DownloadHandlerBuffer();
        request.SetRequestHeader("Content-Type", "application/json");

        Debug.Log("Mengirim data ke API...");

        yield return request.SendWebRequest();

        if (request.result == UnityWebRequest.Result.Success)
        {
            Debug.Log("Data berhasil dikirim: " + request.downloadHandler.text);
        }
        else
        {
            Debug.LogError("Gagal mengirim data ke API: " + request.error);
        }
    }
}
