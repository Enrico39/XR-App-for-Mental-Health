using UnityEngine;
using UnityEngine.UI; // Per utilizzare il Toggle

public class PassthroughToggle : MonoBehaviour
{
    public Toggle passthroughToggle; // Riferimento al componente Toggle

    private OVRPassthroughLayer passthroughLayer;

    void Start()
    {
        // Trova l'OVRPassthroughLayer nella scena (assicurati che sia presente un oggetto con questo componente)
        passthroughLayer = FindObjectOfType<OVRPassthroughLayer>();

        if (passthroughLayer == null)
        {
            Debug.LogError("Nessun OVRPassthroughLayer trovato nella scena. Aggiungilo per utilizzare questa funzionalità.");
            return;
        }

        // Assicurati che il Toggle sia assegnato e aggiungi un listener per il suo evento onValueChanged
        if (passthroughToggle != null)
        {
            passthroughToggle.onValueChanged.AddListener(OnToggleValueChanged);

            // Imposta lo stato iniziale del Toggle in base allo stato del Passthrough
            passthroughToggle.isOn = passthroughLayer.enabled;
        }
        else
        {
            Debug.LogError("Il Toggle non è assegnato nello script. Assegnalo dall'Inspector.");
        }
    }

    private void OnToggleValueChanged(bool isOn)
    {
        // Abilita o disabilita il Passthrough in base allo stato del Toggle
        if (passthroughLayer != null)
        {
            passthroughLayer.enabled = isOn;
        }
    }

    void OnDestroy()
    {
        // Rimuovi il listener per evitare errori se l'oggetto viene distrutto
        if (passthroughToggle != null)
        {
            passthroughToggle.onValueChanged.RemoveListener(OnToggleValueChanged);
        }
    }
}
