using UnityEngine;
using UnityEngine.Tilemaps;

public class ShadowDiagnostic : MonoBehaviour
{
    void Start()
    {
        RunDiagnostic();
    }

    [ContextMenu("Run Diagnostic")]
    public void RunDiagnostic()
    {
        Debug.Log("<color=cyan><b>[ShadowDiagnostic] Начинаю проверку системы теней...</b></color>");
        
        Tilemap tilemap = GetComponent<Tilemap>();
        if (tilemap == null)
        {
            Debug.LogError("[ShadowDiagnostic] ОШИБКА: Компонент Tilemap не найден на " + gameObject.name);
            return;
        }

        TilemapCollider2D tmCollider = GetComponent<TilemapCollider2D>();
        if (tmCollider != null)
        {
            Debug.Log($"[ShadowDiagnostic] Текущий Shape Count: {tmCollider.shapeCount}");
        }

        BoundsInt bounds = tilemap.cellBounds;
        int tileCount = 0;
        int collidableTiles = 0;

        foreach (var pos in bounds.allPositionsWithin)
        {
            TileBase tile = tilemap.GetTile(pos);
            if (tile != null)
            {
                tileCount++;
                // Используем прямое строковое сравнение или проверку на 0
                // для обхода конфликтов имен в Unity 6
                UnityEngine.Tilemaps.TileData data = new UnityEngine.Tilemaps.TileData();
                tile.GetTileData(pos, tilemap, ref data);
                
                if ((int)data.colliderType != 0)
                {
                    collidableTiles++;
                }
            }
        }

        Debug.Log($"[ShadowDiagnostic] Итог: Найдено тайлов всего: {tileCount}, из них с физикой: {collidableTiles}");

        if (tileCount > 0 && collidableTiles == 0)
        {
            Debug.LogError("[ShadowDiagnostic] ПРИЧИНА: У тайлов выключена физика (Collider Type = None). Проверьте настройки тайла в инспекторе (Physics Shape).");
        }
        else if (tileCount == 0)
        {
            Debug.LogError("[ShadowDiagnostic] ПРИЧИНА: Слой Tilemap пуст! Возможно, вы рисуете на другом слое (Floor или WallsUp).");
        }
        else if (tmCollider != null && tmCollider.shapeCount == 0 && collidableTiles > 0)
        {
             Debug.LogError("[ShadowDiagnostic] ПРИЧИНА: Ошибка синхронизации Unity. Попробуйте выключить и включить компонент TilemapCollider2D.");
        }
        
        Debug.Log("<color=cyan><b>[ShadowDiagnostic] Проверка завершена.</b></color>");
    }
}
