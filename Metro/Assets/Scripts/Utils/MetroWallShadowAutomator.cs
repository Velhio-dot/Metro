using UnityEngine;
using UnityEngine.Rendering.Universal;
using System.Collections.Generic;
using System.Reflection;

#if UNITY_EDITOR
using UnityEditor;
#endif

public class MetroWallShadowAutomator : MonoBehaviour
{
    [Header("Ссылки")]
    public Transform shadowContainer;
    public GameObject searchRoot;

    [Header("Фильтры")]
    [Tooltip("Выберите слой, на котором находятся ваши стены.")]
    public LayerMask wallLayerMask;
    public bool searchAllScene = false;

    [Header("Настройки тени")]
    [Range(0.01f, 0.5f)] public float stripHeight = 0.05f;
    public float overhang = 0.02f;

    [ContextMenu("GENERATE IDEAL SHADOWS")]
    public void GenerateIdealShadows()
    {
        if (shadowContainer == null) { Debug.LogError("Назначьте Shadow Container!"); return; }

        int count = 0;
        int skipped = 0;

        // Определяем, где искать
        List<Renderer> targets = new List<Renderer>();
        if (searchAllScene)
        {
            targets.AddRange(Object.FindObjectsByType<SpriteRenderer>(FindObjectsInactive.Include, FindObjectsSortMode.None));
            targets.AddRange(Object.FindObjectsByType<UnityEngine.Tilemaps.TilemapRenderer>(FindObjectsInactive.Include, FindObjectsSortMode.None));
        }
        else
        {
            GameObject root = searchRoot != null ? searchRoot : gameObject;
            targets.AddRange(root.GetComponentsInChildren<SpriteRenderer>(true));
            targets.AddRange(root.GetComponentsInChildren<UnityEngine.Tilemaps.TilemapRenderer>(true));
        }

        Debug.Log($"[MetroShadows] Найдено потенциальных объектов: {targets.Count}. Начинаю фильтрацию по слою...");

        foreach (var r in targets)
        {
            if (((1 << r.gameObject.layer) & wallLayerMask) == 0)
            {
                skipped++;
                continue;
            }

            // Проверяем, есть ли CompositeCollider2D для точной нарезки
            var composite = r.GetComponent<CompositeCollider2D>();
            if (composite != null)
            {
                GenerateShadowsFromComposite(composite);
                count++;
            }
            else
            {
                // Если нет композитного коллайдера, используем старый метод (для одиночных спрайтов)
                CreateStrip(r.gameObject, r.bounds);
                count++;
            }
            
            SetupBaseObject(r.gameObject);
        }

        Debug.Log($"<color=green><b>[MetroShadows] Готово!</b></color> Обработано объектов: {count}. Пропущено: {skipped}");
    }

    private void GenerateShadowsFromComposite(CompositeCollider2D composite)
    {
        for (int i = 0; i < composite.pathCount; i++)
        {
            Vector2[] pathPoints = new Vector2[composite.GetPathPointCount(i)];
            composite.GetPath(i, pathPoints);

            if (pathPoints.Length < 2) continue;

            // Вычисляем границы конкретного пути в локальных координатах
            float minX = float.MaxValue, maxX = float.MinValue;
            float minY = float.MaxValue, maxY = float.MinValue;

            foreach (var p in pathPoints)
            {
                if (p.x < minX) minX = p.x; if (p.x > maxX) maxX = p.x;
                if (p.y < minY) minY = p.y; if (p.y > maxY) maxY = p.y;
            }

            // Переводим в мировые координаты
            Vector3 worldMin = composite.transform.TransformPoint(new Vector3(minX, minY, 0));
            Vector3 worldMax = composite.transform.TransformPoint(new Vector3(maxX, maxY, 0));
            
            // Создаем Bounds для этого сегмента
            Bounds segmentBounds = new Bounds();
            segmentBounds.SetMinMax(worldMin, worldMax);

            CreateStrip(composite.gameObject, segmentBounds, i);
        }
    }

    private void CreateStrip(GameObject source, Bounds b, int pathIndex = -1)
    {
        string suffix = pathIndex >= 0 ? $"_Path_{pathIndex}" : "";
        string id = $"Strip_{source.name}_{source.GetInstanceID()}{suffix}";
        Transform t = shadowContainer.Find(id);
        GameObject strip = t != null ? t.gameObject : new GameObject(id);

        strip.transform.SetParent(shadowContainer);
        strip.layer = source.layer;
        strip.transform.position = new Vector3(b.center.x, b.max.y - (stripHeight / 2f), b.center.z);

        var caster = strip.GetComponent<ShadowCaster2D>();
        if (caster == null) caster = strip.AddComponent<ShadowCaster2D>();

        ConfigureShadowCaster(caster, b.size.x + overhang, stripHeight);
    }

    private void ConfigureShadowCaster(ShadowCaster2D caster, float w, float h)
    {
        FieldInfo useSilhouetteField = typeof(ShadowCaster2D).GetField("m_UseRendererSilhouette", BindingFlags.NonPublic | BindingFlags.Instance);
        if (useSilhouetteField != null) useSilhouetteField.SetValue(caster, false);

        Vector3[] points = new Vector3[4];
        float hw = w / 2f; float hh = h / 2f;
        points[0] = new Vector3(-hw, -hh, 0); points[1] = new Vector3(hw, -hh, 0);
        points[2] = new Vector3(hw, hh, 0); points[3] = new Vector3(-hw, hh, 0);

        FieldInfo shapePathField = typeof(ShadowCaster2D).GetField("m_ShapePath", BindingFlags.NonPublic | BindingFlags.Instance);
        if (shapePathField != null) shapePathField.SetValue(caster, points);

        caster.enabled = false;
        caster.enabled = true;
    }

    private void SetupBaseObject(GameObject obj)
    {
        var caster = obj.GetComponent<ShadowCaster2D>();
        if (caster == null) caster = obj.AddComponent<ShadowCaster2D>();
        caster.selfShadows = false;
    }

    [ContextMenu("CLEAR ALL")]
    public void Clear()
    {
        if (shadowContainer == null) return;
        List<GameObject> toDestroy = new List<GameObject>();
        foreach (Transform child in shadowContainer) toDestroy.Add(child.gameObject);
        foreach (var g in toDestroy) DestroyImmediate(g);
        Debug.Log("[MetroShadows] Очищено.");
    }
}
