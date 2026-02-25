using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections;
using System.Collections.Generic;

public class SwipeManager : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{

    [Header("Panels (children of this object)")]
    [SerializeField] private RectTransform[] panels; // Main, History, Paint (в любом порядке)

    [Header("Swipe")]
    [Range(0.05f, 0.5f)]
    [SerializeField] private float swipeThreshold = 0.15f; // 15% ширины

    [Header("Snap")]
    [SerializeField] private float snapSpeed = 12f;

    [Header("Start panel index")] 
    [SerializeField] private int startIndex = 1;

    private RectTransform container;   // это RectTransform объекта Swipe
    private RectTransform viewport;    // родитель (обычно область экрана)
    private Canvas canvas;

    private Vector2 targetPos;
    private bool snapping;

    private int currentIndex;
    private Vector2 dragStartContainerPos;

    private float PageWidth => viewport != null ? viewport.rect.width : container.rect.width;

    [SerializeField] private bool clampDrag = true;
    [SerializeField] private float edgeRubber = 0f; // 0 = жёстко, 0.2 = чуть резина

    private void Awake()
    {
        container = GetComponent<RectTransform>();
        viewport = container.parent as RectTransform;
        canvas = GetComponentInParent<Canvas>();

        //if (panels == null || panels.Length == 0)
        //{
        //    panels = new RectTransform[container.childCount];
        //    for (int i = 0; i < container.childCount; i++)
        //        panels[i] = container.GetChild(i) as RectTransform;
        //}


        currentIndex = Mathf.Clamp(startIndex, 0, panels.Length - 1);

        LayoutPanelsSideBySide();
        SnapImmediate(currentIndex);
    }

    private void OnRectTransformDimensionsChange()
    {
        if (container == null || panels == null || panels.Length == 0) return;

        LayoutPanelsSideBySide();
        SnapImmediate(currentIndex);
    }

    private void LayoutPanelsSideBySide()
    {
        float w = PageWidth;

        // Раскладываем панели
        for (int i = 0; i < panels.Length; i++)
        {
            if (panels[i] == null) continue;

            // Чтобы панель занимала весь экран/вьюпорт:
            panels[i].anchorMin = new Vector2(0f, 0f);
            panels[i].anchorMax = new Vector2(0f, 1f);
            panels[i].pivot = new Vector2(0f, 0.5f);

            panels[i].sizeDelta = new Vector2(w, 0f);
            panels[i].anchoredPosition = new Vector2(i * w, 0f);
        }

        // Контейнер шире на количество страниц
        container.anchorMin = new Vector2(0f, 0f);
        container.anchorMax = new Vector2(0f, 1f);
        container.pivot = new Vector2(0f, 0.5f);
        container.sizeDelta = new Vector2(w * panels.Length, 0f);
    }

    private void Update()
    {
        if (!snapping) return;

        container.anchoredPosition = Vector2.Lerp(
            container.anchoredPosition,
            targetPos,
            Time.deltaTime * snapSpeed
        );

        container.anchoredPosition = new Vector2(
            ClampX(container.anchoredPosition.x),
            container.anchoredPosition.y
        );

        if (Vector2.SqrMagnitude(container.anchoredPosition - targetPos) < 1f)
        {
            container.anchoredPosition = targetPos;
            snapping = false;
        }
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        snapping = false;
        dragStartContainerPos = container.anchoredPosition;
    }

    public void OnDrag(PointerEventData eventData)
    {
        float dx = eventData.delta.x / (canvas != null ? canvas.scaleFactor : 1f);
        var pos = container.anchoredPosition + new Vector2(dx, 0f);
        pos.x = ClampX(pos.x);
        container.anchoredPosition = pos;
    }


    public void OnEndDrag(PointerEventData eventData)
    {
        float w = PageWidth;

        float dragDelta = container.anchoredPosition.x - dragStartContainerPos.x;

        if (Mathf.Abs(dragDelta) >= w * swipeThreshold)
        {
            if (dragDelta < 0f) currentIndex++; // свайп влево -> следующая
            else currentIndex--;                // свайп вправо -> предыдущая
        }

        currentIndex = Mathf.Clamp(currentIndex, 0, panels.Length - 1);
        SnapAnimated(currentIndex);
    }

    private void SnapImmediate(int index)
    {
        targetPos = new Vector2(-index * PageWidth, 0f);
        container.anchoredPosition = targetPos;
        snapping = false;
    }

    private void SnapAnimated(int index)
    {
        targetPos = new Vector2(-index * PageWidth, 0f);
        snapping = true;
    }

    // Если вдруг захочешь кнопки:
    public void GoTo(int index)
    {
        currentIndex = Mathf.Clamp(index, 0, panels.Length - 1);
        SnapAnimated(currentIndex);
    }


    private float MinX => -(panels.Length - 1) * PageWidth; // самая правая страница (контент влево)
    private float MaxX => 0f;                               // самая левая страница

    private float ClampX(float x)
    {
        if (!clampDrag) return x;

        // жёсткий стоп
        if (edgeRubber <= 0f)
            return Mathf.Clamp(x, MinX, MaxX);

        // “резина” у края: можно чуть тянуть, но с сопротивлением
        if (x < MinX) return MinX + (x - MinX) * edgeRubber;
        if (x > MaxX) return MaxX + (x - MaxX) * edgeRubber;
        return x;
    }

}


