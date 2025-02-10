using UnityEngine;
using Siccity.GLTFUtility; // Assicurati di avere questa libreria

public class LoadGLTF : MonoBehaviour
{
    public string modelPath = "Assets/YourModel.glb"; // Modifica con il tuo percorso

    void Start()
    {
        GameObject model = Importer.LoadFromFile(modelPath);
        if (model != null)
        {
            model.transform.position = Vector3.zero; // Posiziona l'isola all'origine
            Debug.Log("Modello caricato correttamente!");
        }
        else
        {
            Debug.LogError("Errore nel caricamento del modello.");
        }
    }
}
