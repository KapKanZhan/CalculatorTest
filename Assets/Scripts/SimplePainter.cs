using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class SimplePainter : MonoBehaviour,
    IPointerDownHandler, IDragHandler, IPointerUpHandler
{
    [Header("Settings")]
    public Color paintColor = Color.black;

    [Min(1)]
    public int brushSize = 5;          // радиус в пикселях

    [Range(0.1f, 2f)]
    public float spacing = 0.5f;       // чем меньше, тем плотнее "штампы" (0.3–0.7 обычно ок)

    [Header("UI")]
    [SerializeField] public Slider mySlider;

    private RawImage rawImage;
    private Texture2D texture;
    private RectTransform rectTransform;

    private bool hasPrev;
    private Vector2 prevPixelPos;

    void Start()
    {
        rawImage = GetComponent<RawImage>();
        rectTransform = GetComponent<RectTransform>();

        int w = Mathf.Max(1, Mathf.RoundToInt(rectTransform.rect.width));
        int h = Mathf.Max(1, Mathf.RoundToInt(rectTransform.rect.height));

        texture = new Texture2D(w, h, TextureFormat.RGBA32, false);
        texture.filterMode = FilterMode.Point;

        ClearCanvas();
        rawImage.texture = texture;
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        if (DraggableUI.IsInteracting) return;

        if (TryGetPixelPosition(eventData, out var pix))
        {
            hasPrev = true;
            prevPixelPos = pix;

            StampBrush(Mathf.RoundToInt(pix.x), Mathf.RoundToInt(pix.y));
            texture.Apply();
        }
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (DraggableUI.IsInteracting) return;

        if (!TryGetPixelPosition(eventData, out var pix))
            return;

        if (!hasPrev)
        {
            hasPrev = true;
            prevPixelPos = pix;
        }

        DrawLine(prevPixelPos, pix);
        prevPixelPos = pix;

        texture.Apply();
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        if (DraggableUI.IsInteracting) return;

        hasPrev = false;
    }

    private bool TryGetPixelPosition(PointerEventData eventData, out Vector2 pixelPos)
    {
        pixelPos = default;

        if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(
                rectTransform,
                eventData.position,
                eventData.pressEventCamera,
                out var localPoint))
            return false;

        // localPoint: центр RectTransform = (0,0)
        float px = localPoint.x + rectTransform.pivot.x * rectTransform.rect.width;
        float py = localPoint.y + rectTransform.pivot.y * rectTransform.rect.height;

        // в пиксели текстуры
        px = Mathf.Clamp(px, 0, texture.width - 1);
        py = Mathf.Clamp(py, 0, texture.height - 1);

        pixelPos = new Vector2(px, py);
        return true;
    }

    private void DrawLine(Vector2 from, Vector2 to)
    {
        float dist = Vector2.Distance(from, to);

        // шаг штамповки: чем меньше spacing, тем плотнее линия
        float step = Mathf.Max(1f, brushSize * spacing);
        int steps = Mathf.Max(1, Mathf.CeilToInt(dist / step));

        for (int i = 0; i <= steps; i++)
        {
            float t = i / (float)steps;
            Vector2 p = Vector2.Lerp(from, to, t);
            StampBrush(Mathf.RoundToInt(p.x), Mathf.RoundToInt(p.y));
        }
    }

    private void StampBrush(int cx, int cy)
    {
        int r = brushSize;
        int r2 = r * r;

        int xMin = Mathf.Max(0, cx - r);
        int xMax = Mathf.Min(texture.width - 1, cx + r);
        int yMin = Mathf.Max(0, cy - r);
        int yMax = Mathf.Min(texture.height - 1, cy + r);

        for (int x = xMin; x <= xMax; x++)
        {
            int dx = x - cx;
            int dx2 = dx * dx;

            for (int y = yMin; y <= yMax; y++)
            {
                int dy = y - cy;
                if (dx2 + dy * dy <= r2) // круглая кисть
                    texture.SetPixel(x, y, paintColor);
            }
        }
    }

    public void ClearCanvas()
    {
        if (texture == null) return;

        var fill = new Color32(255, 255, 255, 255);
        Color32[] pixels = new Color32[texture.width * texture.height];
        for (int i = 0; i < pixels.Length; i++) pixels[i] = fill;

        texture.SetPixels32(pixels);
        texture.Apply();
    }

    // UI кнопки
    public void SetColorRed() => paintColor = Color.red;
    public void SetColorBlue() => paintColor = Color.blue;
    public void SetColorBlack() => paintColor = Color.black;

    public void SetBrushSize()
    {
        if (mySlider != null)
            brushSize = Mathf.Max(1, Mathf.RoundToInt(mySlider.value));
    }
}



//using UnityEngine;
//using UnityEngine.UI;
//using UnityEngine.EventSystems;

//public class SimplePainter : MonoBehaviour, IDragHandler, IPointerDownHandler
//{
//    [Header("Settings")]
//    public Color paintColor = Color.black;
//    public int brushSize = 5;

//    private RawImage rawImage;
//    private Texture2D texture;
//    private RectTransform rectTransform;

//    [SerializeField] public Slider mySlider;

//    void Start()
//    {
//        rawImage = GetComponent<RawImage>();
//        rectTransform = GetComponent<RectTransform>();

//        // Создаем чистую белую текстуру по размеру панели
//        texture = new Texture2D((int)rectTransform.rect.width, (int)rectTransform.rect.height);
//        for (int x = 0; x < texture.width; x++)
//            for (int y = 0; y < texture.height; y++)
//                texture.SetPixel(x, y, Color.white);

//        texture.Apply();
//        rawImage.texture = texture;
//    }

//    public void OnPointerDown(PointerEventData eventData) => Draw(eventData);
//    public void OnDrag(PointerEventData eventData) => Draw(eventData);

//    void Draw(PointerEventData eventData)
//    {
//        Vector2 localPoint;
//        // Преобразуем координаты клика в координаты текстуры
//        if (RectTransformUtility.ScreenPointToLocalPointInRectangle(rectTransform, eventData.position, eventData.pressEventCamera, out localPoint))
//        {
//            int px = (int)(localPoint.x + rectTransform.pivot.x * rectTransform.rect.width);
//            int py = (int)(localPoint.y + rectTransform.pivot.y * rectTransform.rect.height);

//            for (int x = -brushSize; x < brushSize; x++)
//            {
//                for (int y = -brushSize; y < brushSize; y++)
//                {
//                    if (px + x >= 0 && px + x < texture.width && py + y >= 0 && py + y < texture.height)
//                        texture.SetPixel(px + x, py + y, paintColor);
//                }
//            }
//            texture.Apply();
//        }
//    }

//    public void ClearCanvas()
//    {
//        // Заливаем всю текстуру белым цветом (или другим цветом фона)
//        for (int x = 0; x < texture.width; x++)
//        {
//            for (int y = 0; y < texture.height; y++)
//            {
//                texture.SetPixel(x, y, Color.white);
//            }
//        }
//        texture.Apply(); // изменение текстуры
//    }

//    // Методы для кнопок UI
//    public void SetColorRed()
//    {
//        paintColor = Color.red;
//    }

//    public void SetColorBlue()
//    {
//        paintColor = Color.blue;
//    }

//    public void SetColorBlack()
//    {
//        paintColor = Color.black;
//    }

//    public void SetBrushSize()
//    {
//        brushSize = (int)mySlider.value;
//    }
//}
