using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using Siccity.GLTFUtility; 
using System;

public class TextTo3DUI : MonoBehaviour
{
    [Header("UI Elements")]
    //public InputField descriptionInput; // Campo di testo per inserire la descrizione
    public Text descriptionInput; // Campo di testo per inserire la descrizione

    public Toggle useLessThan15GBTgl;   // Toggle per l'opzione "Use less than 15GB"
    public Button generateButton;       // Bottone per inviare la richiesta
    public Text statusText;             // Testo per mostrare lo stato del processo

    [Header("Settings")]
    public string serverUrl = "http://192.168.1.89:5000/process"; // URL del server

    private bool isGenerating = false;
    private List<GameObject> generatedObjects = new List<GameObject>();

    private void Start()
    {
        generateButton.onClick.AddListener(OnGenerateButtonPressed);
    }

    public void OnGenerateButtonPressed()
    {
        Debug.Log("Generate button pressed!"); // Debug 
        if (isGenerating)
        {
            statusText.text = "Generation already in progress...";
            return;
        }

        string description = descriptionInput.text;
        bool useLessThan15GB = useLessThan15GBTgl.isOn;

        if (string.IsNullOrWhiteSpace(description))
        {
            statusText.text = "Please enter a valid description.";
            return;
        }

        statusText.text = "Sending request...";
        StartCoroutine(SendRequest(description, useLessThan15GB));
    }

    private IEnumerator SendRequest(string description, bool useLessThan15GB)
    {
        isGenerating = true;
        WWWForm form = new WWWForm();
        form.AddField("description", description);
        form.AddField("use_less_than_15GB", useLessThan15GB.ToString());

        UnityWebRequest www = UnityWebRequest.Post(serverUrl, form);

        yield return www.SendWebRequest();

        if (www.result == UnityWebRequest.Result.Success)
        {
            statusText.text = "Request successful. Processing response...";
            string responseText = www.downloadHandler.text;

            try
            {
                var jsonResponse = JsonUtility.FromJson<ResponseData>(responseText);
                if (!string.IsNullOrEmpty(jsonResponse.object_url))
                {
                    statusText.text = "Downloading 3D object...";
                    StartCoroutine(Download3DObject(jsonResponse.object_url));
                }
                else
                {
                    statusText.text = "Error: No object URL in response.";
                }
            }
            catch (System.Exception e)
            {
                statusText.text = "Error parsing response: " + e.Message;
            }
        }
        else
        {
            statusText.text = $"Error: {www.error}";
        }

        isGenerating = false;
    }

private IEnumerator Download3DObject(string objectUrl)
{
    Debug.Log("Starting download from URL: " + objectUrl);
    UnityWebRequest www = UnityWebRequest.Get(objectUrl);

    yield return www.SendWebRequest();

    if (www.result == UnityWebRequest.Result.Success)
    {
        Debug.Log("Download successful, saving file...");

        string directoryPath = Path.Combine(Application.persistentDataPath, "DownloadedObjects");
        if (!Directory.Exists(directoryPath))
        {
            Directory.CreateDirectory(directoryPath);
        }

        string filePath = Path.Combine(directoryPath, "mesh" + System.Guid.NewGuid() + ".glb");
        File.WriteAllBytes(filePath, www.downloadHandler.data);
        statusText.text = "File saved";
        Debug.Log("File saved to: " + filePath);

        try
        {
            // Carica il modello utilizzando GLTFUtility
            GameObject importedObject = Importer.LoadFromFile(filePath);

            // Istanzia il modello
            Instantiate(importedObject, Vector3.zero, Quaternion.identity);
            statusText.text = "3D object downloaded and instantiated!";
            Debug.Log("3D object instantiated successfully!");
        }
        catch (Exception ex)
        {
            // Gestisce gli errori durante il caricamento del modello
            statusText.text = "Failed to load the 3D object.";
            Debug.LogError($"Failed to load the 3D object: {ex.Message}");
        }

    }
    else
    {
        Debug.LogError("Download failed: " + www.error);
        statusText.text = $"Download error: {www.error}";
    }
}


    [System.Serializable]
    public class ResponseData
    {
        public string object_url;
    }
}
