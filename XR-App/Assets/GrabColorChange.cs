using UnityEngine;
using Oculus.Interaction;

public class GrabColorChange : MonoBehaviour
{
    private Renderer _renderer;
    private Color _originalColor;
    public Color grabColor = Color.red; // Colore quando afferrato
    private Grabbable _grabbable;

    private void Start()
    {
        // Cerca il Renderer nei figli
        _renderer = GetComponentInChildren<Renderer>();
        _grabbable = GetComponent<Grabbable>();

        if (_renderer != null)
        {
            _originalColor = _renderer.material.color;
            Debug.Log("[GrabColorChange] Renderer trovato su " + _renderer.gameObject.name + "! Colore originale: " + _originalColor);
        }
        else
        {
            Debug.LogError("[GrabColorChange] Renderer non trovato! Assicurati che l'oggetto abbia un MeshRenderer nei figli.");
        }

        if (_grabbable == null)
        {
            Debug.LogError("[GrabColorChange] Grabbable non trovato!");
        }
    }

    private void OnEnable()
    {
        if (_grabbable != null)
        {
            _grabbable.WhenPointerEventRaised += OnPointerEvent;
            Debug.Log("[GrabColorChange] Evento WhenPointerEventRaised registrato.");
        }
    }

    private void OnDisable()
    {
        if (_grabbable != null)
        {
            _grabbable.WhenPointerEventRaised -= OnPointerEvent;
            Debug.Log("[GrabColorChange] Evento WhenPointerEventRaised rimosso.");
        }
    }

    private void OnPointerEvent(PointerEvent evt)
    {
        if (_renderer == null)
        {
            Debug.LogError("[GrabColorChange] Renderer assente! Il colore non può essere cambiato.");
            return;
        }

        Debug.Log("[GrabColorChange] Evento ricevuto: " + evt.Type);

        if (evt.Type == PointerEventType.Select) // Quando viene afferrato
        {
            _renderer.material.color = grabColor;
            Debug.Log("[GrabColorChange] Oggetto afferrato! Colore cambiato in " + grabColor);
        }
        else if (evt.Type == PointerEventType.Unselect) // Quando viene rilasciato
        {
            _renderer.material.color = _originalColor;
            Debug.Log("[GrabColorChange] Oggetto rilasciato! Colore ripristinato a " + _originalColor);
        }
    }
}
