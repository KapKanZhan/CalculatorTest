using UnityEngine;

public class StickerSpawner : MonoBehaviour
{
    [Header("Где спавнить (Должен быть внутри RectMask2D)")]
    [SerializeField] private RectTransform spawnParent;

    [Header("Prefab (UI Image)")]
    [SerializeField] private RectTransform stickerPrefab;

    public void SpawnSticker()
    {
        if (spawnParent == null || stickerPrefab == null) return;

        RectTransform sticker = Instantiate(stickerPrefab, spawnParent);
        sticker.anchoredPosition = Vector2.zero;
        sticker.localScale = new Vector3(3f, 3f, 3f);   
        sticker.SetAsLastSibling(); // поверх
    }

    public void ClearAllStickers()
    {
        if (spawnParent == null) return;

        for (int i = spawnParent.childCount - 1; i >= 0; i--)
            Destroy(spawnParent.GetChild(i).gameObject);
    }
}
