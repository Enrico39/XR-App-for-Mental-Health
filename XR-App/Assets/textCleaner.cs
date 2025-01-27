using TMPro; // Necessario per lavorare con TextMeshPro
using UnityEngine;

public class TextCleaner : MonoBehaviour
{
    [SerializeField]
    private TMP_InputField inputField; // Riferimento al componente TMP_InputField

    // Metodo pubblico per impostare il testo dell'input field a vuoto
    public void ClearInputField()
    {
        if (inputField != null)
        {
            inputField.text = string.Empty; // Imposta il testo a vuoto
        }
        else
        {
            Debug.LogWarning("Il componente TMP_InputField non è assegnato!");
        }
    }
}
