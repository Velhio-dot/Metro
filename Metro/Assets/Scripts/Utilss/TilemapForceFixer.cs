using UnityEngine;
using UnityEngine.Tilemaps;

[ExecuteInEditMode]
public class TilemapForceFixer : MonoBehaviour
{
    [ContextMenu("FORCE REBUILD COLLIDER")]
    public void ForceRebuild()
    {
        Debug.Log("<color=orange><b>[ForceFixer] Начинаю принудительную пересборку...</b></color>");
        
        Tilemap tilemap = GetComponent<Tilemap>();
        TilemapCollider2D tmCollider = GetComponent<TilemapCollider2D>();
        CompositeCollider2D composite = GetComponent<CompositeCollider2D>();
        Rigidbody2D rb = GetComponent<Rigidbody2D>();

        if (tilemap == null) return;

        // 1. Сброс тайлмепа
        tilemap.RefreshAllTiles();
        
        // 2. Манипуляции с Rigidbody (часто помогает «толкнуть» физику)
        if (rb != null) {
            var oldType = rb.bodyType;
            rb.bodyType = RigidbodyType2D.Dynamic;
            rb.bodyType = oldType;
        }

        // 3. Жесткое переключение колайдеров
        if (tmCollider != null) {
            tmCollider.enabled = false;
            tmCollider.usedByComposite = false;
            
            // Маленький трюк: меняем любой параметр и возвращаем назад
            float oldOffset = tmCollider.offset.x;
            tmCollider.offset = new Vector2(0.001f, 0);
            tmCollider.offset = new Vector2(oldOffset, 0);
            
            tmCollider.enabled = true;
            if (composite != null) tmCollider.usedByComposite = true;
        }

        if (composite != null) {
            composite.GenerateGeometry();
            Debug.Log("[ForceFixer] Геометрия Composite Collider пересобрана.");
        }

        Debug.Log("<color=green><b>[ForceFixer] Готово! Проверьте Shape Count в TilemapCollider2D.</b></color>");
    }
}
