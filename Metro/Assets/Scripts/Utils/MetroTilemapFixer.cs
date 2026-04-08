using UnityEngine;
using UnityEngine.Tilemaps;

[ExecuteInEditMode]
public class MetroTilemapFixer : MonoBehaviour
{
    [ContextMenu("FORCE FIX CORNER SHADOWS")]
    public void FixCornerShadows()
    {
        Debug.Log("<color=orange><b>[MetroFixer] Исправляю тени от углов...</b></color>");
        
        CompositeCollider2D composite = GetComponent<CompositeCollider2D>();
        TilemapCollider2D tmCollider = GetComponent<TilemapCollider2D>();

        if (composite == null || tmCollider == null) return;

        // 1. Форсируем режим полигонов (важно для теней от углов)
        composite.geometryType = CompositeCollider2D.GeometryType.Polygons;
        
        // 2. Сброс геометрии
        tmCollider.enabled = false;
        tmCollider.enabled = true;
        
        composite.GenerateGeometry();

        // 3. Пытаемся "пнуть" Shadow Caster
        var caster = GetComponent<UnityEngine.Rendering.Universal.ShadowCaster2D>();
        if (caster != null) {
            caster.enabled = false;
            caster.enabled = true;
        }

        Debug.Log("<color=green><b>[MetroFixer] Геометрия обновлена до Polygons. Проверьте углы теней!</b></color>");
    }
}
