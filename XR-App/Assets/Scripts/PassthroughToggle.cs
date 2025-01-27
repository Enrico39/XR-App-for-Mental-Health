using UnityEngine;
using UnityEngine.UI;

public class PassthroughToggle : MonoBehaviour
{
    public Toggle passthroughToggle; // Riferimento al Toggle

    private OVRPassthroughLayer passthroughLayer;

    void Start()
    {
        // Trova l'OVRPassthroughLayer
        passthroughLayer = FindObjectOfType<OVRPassthroughLayer>();

        if (passthroughLayer == null)
        {
            Debug.LogError("Nessun OVRPassthroughLayer trovato nella scena. Aggiungilo per utilizzare questa funzionalità.");
            return;
        }

        if (passthroughToggle != null)
        {
            passthroughToggle.onValueChanged.AddListener(OnToggleValueChanged); // listener per il suo evento onValueChanged

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
        // Abilita o disabilita il Passthrough 
        if (passthroughLayer != null)
        {
            passthroughLayer.enabled = isOn;
        }
    }

    void OnDestroy()
    {
        // Rimuovi il listener
        if (passthroughToggle != null)
        {
            passthroughToggle.onValueChanged.RemoveListener(OnToggleValueChanged);
        }
    }
}
