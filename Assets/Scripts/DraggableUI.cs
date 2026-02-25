using UnityEngine;
using UnityEngine.EventSystems;

public class DraggableUI : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    [Header("Scale limits")]
    public float minScale = 0.5f;
    public float maxScale = 2.5f;

    private RectTransform rect;
    private RectTransform parentRect;

    private Vector2 dragOffsetLocal;
    private bool dragging;

    // pinch + rotate
    private bool twoFingerGesture;
    private float startPinchDist;
    private float startScale;

    private float startAngle;
    private float startRotationZ;

    private static DraggableUI active;

    public static bool IsInteracting { get; private set; }

    void Awake()
    {
        rect = GetComponent<RectTransform>();
        parentRect = rect.parent as RectTransform;
    }

    void OnDisable()
    {
        if (active == this) active = null;
        dragging = false;
        twoFingerGesture = false;
        if (active == null) IsInteracting = false;
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        if (active != null && active != this)
            active.CancelGestures();

        active = this;
        rect.SetAsLastSibling();

        // если нажали на стикер Ч считаем, что взаимодействуем (чтобы не рисовать)
        IsInteracting = true;
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        // отпускание одного пальца не всегда означает конец (могут быть 2 пальца),
        // поэтому окончательно сбросим в Update / EndDrag
        if (!dragging && Input.touchCount <= 1)
            IsInteracting = false;
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        if (active != this || parentRect == null) return;

        dragging = true;
        IsInteracting = true;

        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            parentRect, eventData.position, eventData.pressEventCamera, out var localPoint);

        dragOffsetLocal = rect.anchoredPosition - localPoint;
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (active != this || parentRect == null) return;

        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            parentRect, eventData.position, eventData.pressEventCamera, out var localPoint);

        rect.anchoredPosition = localPoint + dragOffsetLocal;
        ClampToParent_AABB();
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        dragging = false;
        if (Input.touchCount < 2) // если не делаем жест двум€ пальцами
            IsInteracting = false;
    }

    void Update()
    {
        if (active != this) return;

        // ∆ест двум€ пальцами: одновременно масштаб + поворот
        if (Input.touchCount == 2)
        {
            IsInteracting = true;

            Touch t0 = Input.GetTouch(0);
            Touch t1 = Input.GetTouch(1);

            Vector2 p0 = t0.position;
            Vector2 p1 = t1.position;

            float dist = Vector2.Distance(p0, p1);
            float angle = Mathf.Atan2(p1.y - p0.y, p1.x - p0.x) * Mathf.Rad2Deg;

            if (!twoFingerGesture)
            {
                twoFingerGesture = true;

                startPinchDist = dist;
                startScale = rect.localScale.x;

                startAngle = angle;
                startRotationZ = rect.localEulerAngles.z;
            }
            else
            {
                // scale
                if (startPinchDist > 0.001f)
                {
                    float k = dist / startPinchDist;
                    float newScale = Mathf.Clamp(startScale * k, minScale, maxScale);
                    rect.localScale = new Vector3(newScale, newScale, 1f);
                }

                // rotation
                float deltaAngle = Mathf.DeltaAngle(startAngle, angle);
                float newZ = startRotationZ + deltaAngle;
                rect.localEulerAngles = new Vector3(0f, 0f, newZ);

                // после поворота/масштаба попробуем не выходить за рамки
                ClampToParent_AABB();
            }
        }
        else
        {
            twoFingerGesture = false;

            // если не т€нем Ч можно снова рисовать
            if (!dragging)
                IsInteracting = false;
        }
    }

    private void CancelGestures()
    {
        dragging = false;
        twoFingerGesture = false;
    }

    private void ClampToParent_AABB()
    {
        if (parentRect == null) return;

        // берЄм мировые углы стикера
        Vector3[] corners = new Vector3[4];
        rect.GetWorldCorners(corners);

        // переводим их в локальные координаты parentRect
        for (int i = 0; i < 4; i++)
            corners[i] = parentRect.InverseTransformPoint(corners[i]);

        float minX = corners[0].x, maxX = corners[0].x;
        float minY = corners[0].y, maxY = corners[0].y;
        for (int i = 1; i < 4; i++)
        {
            minX = Mathf.Min(minX, corners[i].x);
            maxX = Mathf.Max(maxX, corners[i].x);
            minY = Mathf.Min(minY, corners[i].y);
            maxY = Mathf.Max(maxY, corners[i].y);
        }

        Rect pr = parentRect.rect;

        Vector2 shift = Vector2.zero;
        if (minX < pr.xMin) shift.x += (pr.xMin - minX);
        if (maxX > pr.xMax) shift.x -= (maxX - pr.xMax);
        if (minY < pr.yMin) shift.y += (pr.yMin - minY);
        if (maxY > pr.yMax) shift.y -= (maxY - pr.yMax);

        rect.anchoredPosition += shift;
    }
}
