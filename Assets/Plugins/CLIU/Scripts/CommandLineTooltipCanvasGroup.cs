using UnityEngine;
using UnityEngine.EventSystems;

/// <summary>
/// Script used to create a tooltip effect using the alpha of a Canvas Group. Unity's IPointerEnterHandler and IPointerExitHandler interfaces are used.
/// </summary>
public class CommandLineTooltipCanvasGroup : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler  {

    public CanvasGroup helperWindow;

    public void OnPointerEnter(PointerEventData eventData)
    {
        helperWindow.alpha = 1;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        helperWindow.alpha = 0;
    }
}
