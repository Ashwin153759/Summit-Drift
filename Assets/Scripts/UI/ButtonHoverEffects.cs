using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class ButtonHoverEffects : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler {
    [SerializeField] private Color normalColor = new Color32(255, 255, 255, 31);
    [SerializeField] private Color hoverColor = new Color32(176, 176, 176, 31);
    [SerializeField] private float hoverScale = 1.1f;
    private Vector3 originalScale;
    private Image buttonImage;

    private void Awake() {
        buttonImage = GetComponent<Image>();
        originalScale = transform.localScale;
    }

    public void OnPointerEnter(PointerEventData eventData) {
        // Change color and scale on hover
        buttonImage.color = hoverColor;
        transform.localScale = originalScale * hoverScale;
    }

    public void OnPointerExit(PointerEventData eventData) {
        // Revert to original color and scale
        ResetState();
    }

    public void ResetState() {
        buttonImage.color = normalColor;
        transform.localScale = originalScale;
    }
}
