using TMPro;
using UnityEngine;

public class TextCleaner : MonoBehaviour
{
    [SerializeField]
    private TMP_InputField inputField; // Riferimento al componente 

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
