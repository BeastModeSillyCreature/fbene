using UnityEngine;

public class WallMover : MonoBehaviour
{
    private float currentSpeed; 
    public float destroyDistance = -10f;

    [Header("Настройки прозрачности")]
    public float fadeDistance = 5f; 
    public float minVisibleDistance = 2f; 
    private Renderer[] childRenderers;

    void Start()
    {
        // Подхватываем все рендереры для эффекта прозрачности
        childRenderers = GetComponentsInChildren<Renderer>();
    }

    void Update()
    {
        // Шлагбаум: не двигаемся, если игра не началась
        if (GameManager.instance != null && !GameManager.instance.isGameActive)
            return;

        // Весь твой остальной код движения ниже...
        currentSpeed = GameManager.instance.GetCurrentSpeed();
        transform.Translate(Vector3.back * currentSpeed * Time.deltaTime);
    
        ApplyFade();

        // 4. Удаляем объект, когда он улетел далеко за спину
        if (transform.position.z < destroyDistance)
        {
            Destroy(gameObject);
        }
    }

    void ApplyFade()
    {
        float distance = transform.position.z;

        if (distance <= minVisibleDistance)
        {
            SetAlpha(0);
            return;
        }

        if (distance >= fadeDistance)
        {
            SetAlpha(1);
            return;
        }

        // Плавное исчезновение по формуле: (D - min) / (max - min)
        float alpha = (distance - minVisibleDistance) / (fadeDistance - minVisibleDistance);
        SetAlpha(Mathf.Clamp01(alpha));
    }

    void SetAlpha(float alpha)
    {
        foreach (Renderer ren in childRenderers)
        {
            if (ren != null)
            {
                // Убедись, что на материале труб стоит Surface Type: Transparent!
                Color color = ren.material.color;
                color.a = alpha;
                ren.material.color = color;
            }
        }
    }
}