using UnityEngine;
using Siccity.GLTFUtility; // Libreria GLTFUtility per il parsing
using System.IO;

public class LoadGLTFUtility : MonoBehaviour
{
    public string modelPath;
    
    void Start()
    {
        modelPath = Path.Combine(Application.streamingAssetsPath, "isola/tropical-island/source/Sketchfab/Tropical_Sketchfab.gltf");
        Debug.Log("Caricando modello da: " + modelPath);
        LoadModel();
    }

    void LoadModel()
    {
        if (File.Exists(modelPath))
        {
            GameObject loadedModel = Importer.LoadFromFile(modelPath);
            if (loadedModel != null)
            {
                loadedModel.transform.SetParent(transform, false);
                Debug.Log("Modello GLTF caricato con successo!");
            }
            else
            {
                Debug.LogError("Errore nell'istanziazione del modello.");
            }
        }
        else
        {
            Debug.LogError("Errore: il file GLTF non esiste nel percorso specificato.");
        }
    }
}
