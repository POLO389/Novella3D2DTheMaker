using UnityEngine;
using UnityEngine.EventSystems;

[RequireComponent(typeof(UnityEngine.UI.Graphic))]
public class WindowDrag : MonoBehaviour, IDragHandler, IPointerDownHandler
{
    [Header("Окно, которое двигаем")]
    public RectTransform targetWindow;

    [Header("Границы монитора (перетащи сюда ComputerScreen)")]
    public RectTransform screenBounds;

    private Canvas canvas;
    private Vector2 pointerOffset;
    private Vector3[] winCorners = new Vector3[4];

    private void Awake()
    {
        if (targetWindow == null)
            targetWindow = transform.parent as RectTransform;

        canvas = GetComponentInParent<Canvas>();

        // Если не назначили границы руками, ищем экран автоматически
        if (screenBounds == null && canvas != null)
        {
            Transform screen = canvas.transform.Find("ComputerScreen");
            if (screen != null) screenBounds = screen as RectTransform;
            else screenBounds = canvas.transform as RectTransform;
        }
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        if (targetWindow == null) return;

        targetWindow.SetAsLastSibling();

        Camera cam = (canvas != null && canvas.renderMode == RenderMode.ScreenSpaceOverlay) ? null : eventData.pressEventCamera;

        RectTransform parentRect = targetWindow.parent as RectTransform;
        if (RectTransformUtility.ScreenPointToLocalPointInRectangle(parentRect, eventData.position, cam, out Vector2 localPoint))
        {
            pointerOffset = targetWindow.anchoredPosition - localPoint;
        }
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (targetWindow == null) return;

        Camera cam = (canvas != null && canvas.renderMode == RenderMode.ScreenSpaceOverlay) ? null : eventData.pressEventCamera;
        RectTransform parentRect = targetWindow.parent as RectTransform;

        if (RectTransformUtility.ScreenPointToLocalPointInRectangle(parentRect, eventData.position, cam, out Vector2 localPoint))
        {
            Vector2 newPos = localPoint + pointerOffset;
            targetWindow.anchoredPosition = newPos;

            // --- ЖЕЛЕЗНЫЙ ОГРАНИЧИТЕЛЬ ПО КРАЯМ МОНИТОРА ---
            if (screenBounds != null)
            {
                targetWindow.GetWorldCorners(winCorners);

                float minX = float.MaxValue, maxX = float.MinValue;
                float minY = float.MaxValue, maxY = float.MinValue;

                for (int i = 0; i < 4; i++)
                {
                    Vector3 localCorner = screenBounds.InverseTransformPoint(winCorners[i]);
                    minX = Mathf.Min(minX, localCorner.x);
                    maxX = Mathf.Max(maxX, localCorner.x);
                    minY = Mathf.Min(minY, localCorner.y);
                    maxY = Mathf.Max(maxY, localCorner.y);
                }

                Vector2 shift = Vector2.zero;
                Rect sRect = screenBounds.rect;

                if (minX < sRect.xMin) shift.x += (sRect.xMin - minX);
                if (maxX > sRect.xMax) shift.x += (sRect.xMax - maxX);
                if (minY < sRect.yMin) shift.y += (sRect.yMin - minY);
                if (maxY > sRect.yMax) shift.y += (sRect.yMax - maxY);

                targetWindow.anchoredPosition = newPos + shift;
            }
        }
    }
}