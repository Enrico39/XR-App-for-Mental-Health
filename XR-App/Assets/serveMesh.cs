using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using Siccity.GLTFUtility;
using System;

public class serveMesh : MonoBehaviour
{
    [Header("UI Elements")]
    public Button downloadButton;  

    [Header("Settings")]
    public string modelUrl = "http://192.168.1.59:5000/objects/mesh.glb"; 

    [SerializeField] private GameObject objPrefab; 
    [SerializeField] private AudioClip audioClip; 

    private void Start()
    {
        if (downloadButton != null)
        {
            downloadButton.onClick.AddListener(OnDownloadButtonPressed);
        }
        else
        {
            Debug.LogError("Download button is not assigned in the Inspector!");
        }
    }

    public void OnDownloadButtonPressed()
    {
        Debug.Log("Download button pressed! Downloading from: " + modelUrl);
        StartCoroutine(Download3DObject(modelUrl));
    }

    private IEnumerator Download3DObject(string objectUrl)
    {
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

            string filePath = Path.Combine(directoryPath, "mesh_" + Guid.NewGuid() + ".glb");
            File.WriteAllBytes(filePath, www.downloadHandler.data);
            Debug.Log("File saved to: " + filePath);

            try
            {
                // Carica la mesh
                GameObject importedObject = Importer.LoadFromFile(filePath);

                if (importedObject != null)
                {
                    Debug.Log("Mesh loaded successfully!");

                    // Istanzia il prefab
                    GameObject instance = Instantiate(objPrefab, objPrefab.transform.position, objPrefab.transform.rotation);
                    Transform meshTransform = instance.transform.Find("object/Visuals/Mesh");
            // Riproduce il suono se un AudioClip è assegnato
                    if (audioClip != null)
                    {
                        AudioSource audioSource = instance.AddComponent<AudioSource>();
                        audioSource.clip = audioClip;
                        audioSource.Play();
                    }

                    if (meshTransform != null)
                    {
                        MeshFilter prefabMeshFilter = meshTransform.GetComponent<MeshFilter>();
                        MeshRenderer prefabMeshRenderer = meshTransform.GetComponent<MeshRenderer>();

                        MeshFilter importedMeshFilter = importedObject.GetComponentInChildren<MeshFilter>();
                        MeshRenderer importedMeshRenderer = importedObject.GetComponentInChildren<MeshRenderer>();

                        if (importedMeshFilter != null && prefabMeshFilter != null)
                        {
                            prefabMeshFilter.mesh = importedMeshFilter.mesh;
                            Debug.Log("Mesh successfully replaced in the prefab.");
                        }
                        else
                        {
                            Debug.LogError("MeshFilter missing on either prefab or imported object.");
                        }

                        if (importedMeshRenderer != null && prefabMeshRenderer != null)
                        {
                            prefabMeshRenderer.materials = importedMeshRenderer.materials;
                            Debug.Log("Materials successfully replaced in the prefab.");
                        }
                        else
                        {
                            Debug.LogError("MeshRenderer missing on either prefab or imported object.");
                        }

                        Debug.Log("3D object instantiated successfully!");
                    }
                    else
                    {
                        Debug.LogError("object/Visuals/Mesh not found in the prefab!");
                    }

                    Destroy(importedObject);
                }
                else
                {
                    throw new Exception("Imported object is null.");
                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"Failed to load the 3D object: {ex.Message}");
            }
        }
        else
        {
            Debug.LogError("Download failed: " + www.error);
        }
    }
}
