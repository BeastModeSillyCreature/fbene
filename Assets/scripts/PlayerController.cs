using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [Header("Настройки движения")]
    public float stepSize = 2.0f; // Расстояние между ячейками матрицы
    public float speed = 15.0f;   // Скорость перемещения между позициями

    private Vector3 targetPosition;
    private Vector3 startPosition;

    [Header("Настройки скинов")]
    public GameObject[] birdSkins; // Перетащи сюда все дочерние модели птицы

    void Start()
    {
        // Твой старый код:
        startPosition = transform.position;

        // НАШ НОВЫЙ КОД: Активируем купленный скин
        ApplySelectedSkin();
    }

    public void ApplySelectedSkin()
    {
        // Загружаем индекс выбранного скина (по умолчанию 0 — первая модель)
        int selectedIndex = PlayerPrefs.GetInt("SelectedSkin", 0);

        // Проходим по всем моделям в массиве
        for (int i = 0; i < birdSkins.Length; i++)
        {
            if (birdSkins[i] != null)
            {
                // Если индекс совпадает с сохраненным — включаем модель, иначе выключаем
                birdSkins[i].SetActive(i == selectedIndex);
            }
        }
    }

    void Update()
    {
        if (GameManager.instance != null && !GameManager.instance.isGameActive)
        {
            // Опционально: выключаем физику, чтобы птица не падала в меню
            GetComponent<Rigidbody>().isKinematic = true; 
            return;
        }

        // Включаем физику обратно, когда игра началась
        GetComponent<Rigidbody>().isKinematic = true;
        HandleInput();
        MovePlayer();
    }

    void HandleInput()
    {
        float x = 0;
        float y = 0;

        // Проверяем нажатие стрелок или WASD
        if (Input.GetKey(KeyCode.RightArrow) || Input.GetKey(KeyCode.D)) x = 1;
        if (Input.GetKey(KeyCode.LeftArrow) || Input.GetKey(KeyCode.A)) x = -1;
        if (Input.GetKey(KeyCode.UpArrow) || Input.GetKey(KeyCode.W)) y = 1;
        if (Input.GetKey(KeyCode.DownArrow) || Input.GetKey(KeyCode.S)) y = -1;

        // Рассчитываем целевую точку: центр + смещение * размер шага
        // В Unity ось X — горизонталь, ось Y — вертикаль (для 2D)
        targetPosition = startPosition + new Vector3(x * stepSize, y * stepSize, 0);
    }

    void MovePlayer()
    {
        // Плавное перемещение текущей позиции к целевой
        transform.position = Vector3.Lerp(transform.position, targetPosition, Time.deltaTime * speed);
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Obstacle"))
        {
            Debug.Log("Game Over!");
            GameManager.instance.RestartGame();
        }
        else if (other.CompareTag("ScoreZone"))
        {
            GameManager.instance.AddScore();
            GameData.instance.AddPoint();
        }
    }
}