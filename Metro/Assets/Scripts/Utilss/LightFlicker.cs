using UnityEngine;
using UnityEngine.Rendering.Universal;
using System.Collections;

/// <summary>
/// Скрипт для создания эффекта мерцания ламп в метро. 
/// Поддерживает случайное изменение яркости и полное затухание (мигание).
/// </summary>
[RequireComponent(typeof(Light2D))]
public class LightFlicker : MonoBehaviour
{
    [Header("Настройки яркости")]
    [SerializeField] private float minIntensity = 0.7f;
    [SerializeField] private float maxIntensity = 1.2f;
    
    [Header("Настройки скорости")]
    [Tooltip("Как часто обновляется яркость (в секундах). Чем меньше — тем быстрее дрожание.")]
    [SerializeField] private float flickerInterval = 0.05f;

    [Header("Настройки затухания (Мигание)")]
    [Tooltip("Шанс (0-1), что свет полностью погаснет на короткое время.")]
    [SerializeField] [Range(0f, 1f)] private float offChance = 0.05f;
    [SerializeField] private float offDurationMin = 0.05f;
    [SerializeField] private float offDurationMax = 0.2f;

    [Header("Звук")]
    [SerializeField] private AudioClip flickerSound;
    [SerializeField] [Range(0f, 1f)] private float soundVolume = 0.5f;

    private Light2D light2D;
    private float baseIntensity;
    private bool isOff = false;

    private void Awake()
    {
        light2D = GetComponent<Light2D>();
        baseIntensity = light2D.intensity;
    }

    private void OnEnable()
    {
        StartCoroutine(FlickerRoutine());
    }

    private void OnDisable()
    {
        StopAllCoroutines();
    }

    private IEnumerator FlickerRoutine()
    {
        while (true)
        {
            if (!isOff)
            {
                // Проверяем шанс на полное затухание
                if (Random.value < offChance)
                {
                    yield return StartCoroutine(OffRoutine());
                }
                else
                {
                    // Случайное «дрожание» яркости
                    float targetIntensity = Random.Range(minIntensity, maxIntensity);
                    light2D.intensity = targetIntensity;
                }
            }

            yield return new WaitForSeconds(flickerInterval);
        }
    }

    private IEnumerator OffRoutine()
    {
        isOff = true;
        float originalIntensity = light2D.intensity;
        
        light2D.intensity = 0;

        // Воспроизводим звук треска/искр (если он назначен)
        if (flickerSound != null)
        {
            AudioSource.PlayClipAtPoint(flickerSound, transform.position, soundVolume);
        }
        
        float duration = Random.Range(offDurationMin, offDurationMax);
        yield return new WaitForSeconds(duration);
        
        light2D.intensity = originalIntensity;
        isOff = false;
    }

    // Метод для внешнего управления интенсивностью (если нужно)
    public void ResetBaseIntensity(float newIntensity)
    {
        baseIntensity = newIntensity;
    }
}
