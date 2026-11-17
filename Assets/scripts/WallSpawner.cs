using UnityEngine;
using System.Collections;
using System.Collections.Generic; // Обязательно добавляем для работы со списками List

public class WallSpawner : MonoBehaviour
{
    [Header("Базовые настройки")]
    public GameObject wallPrefab;
    public float spawnInterval = 2f;
    private float timer;

    [Header("Настройки головоломок")]
    public int reactionThreshold = 10; 
    [Range(0f, 1f)] public float colorPuzzleChance = 0.3f; 

    [Header("Настройки головоломки со сдвигом")]
    public int shiftingThreshold = 5; 
    [Range(0f, 1f)] public float shiftingPuzzleChance = 0.3f; 
    public float shiftInterval = 0.4f; // Базовый интервал для низкой скорости

    void Update()
    {
        if (GameManager.instance != null && !GameManager.instance.isGameActive)
            return;

        timer += Time.deltaTime;
        if (timer >= spawnInterval)
        {
            SpawnWall();
            timer = 0;
        }
    }

    void SpawnWall()
    {
        GameObject newWall = Instantiate(wallPrefab, transform.position, Quaternion.identity);
        ColorPuzzle colorPuzzle = newWall.GetComponent<ColorPuzzle>();

        bool doColorPuzzle = (colorPuzzle != null && Random.value <= colorPuzzleChance);
        bool doShiftingPuzzle = (!doColorPuzzle && GameManager.instance != null && GameManager.instance.score >= shiftingThreshold && Random.value <= shiftingPuzzleChance);
        bool doReactionWall = (!doColorPuzzle && !doShiftingPuzzle && GameManager.instance != null && GameManager.instance.score >= reactionThreshold && Random.value > 0.5f);

        if (doColorPuzzle)
        {
            colorPuzzle.frameRenderer.gameObject.SetActive(true);
            colorPuzzle.InitPuzzle();

            for (int i = 0; i < 3; i++)
            {
                int holeInCol = Random.Range(0, colorPuzzle.columns[i].transform.childCount);
                GameObject holeObj = colorPuzzle.columns[i].transform.GetChild(holeInCol).gameObject;

                SetupHole(holeObj);

                if (i != colorPuzzle.correctIndex)
                {
                    holeObj.tag = "Obstacle";
                    BoxCollider col = holeObj.GetComponent<BoxCollider>();
                    if (col != null) col.isTrigger = true; 
                }
            }
        }
        else if (doShiftingPuzzle)
        {
            if (colorPuzzle != null && colorPuzzle.frameRenderer != null) 
                colorPuzzle.frameRenderer.gameObject.SetActive(false); 

            // Запускаем корутину сдвигов
            StartCoroutine(ShiftingHoleRoutine(newWall, colorPuzzle));
        }
        else if (doReactionWall)
        {
            if (colorPuzzle != null && colorPuzzle.frameRenderer != null) 
                colorPuzzle.frameRenderer.gameObject.SetActive(false);

            GameObject holeObj = GetRandomHoleFromColumns(colorPuzzle);
            StartCoroutine(LateHideBlock(newWall, holeObj));
        }
        else
        {
            if (colorPuzzle != null && colorPuzzle.frameRenderer != null) 
                colorPuzzle.frameRenderer.gameObject.SetActive(false);

            GameObject holeObj = GetRandomHoleFromColumns(colorPuzzle);
            SetupHole(holeObj);
        }
    }

    GameObject GetRandomHoleFromColumns(ColorPuzzle puzzle)
    {
        if (puzzle == null || puzzle.columns.Length == 0) return null;

        int randomCol = Random.Range(0, 3);
        int randomBlock = Random.Range(0, puzzle.columns[randomCol].transform.childCount);
        
        return puzzle.columns[randomCol].transform.GetChild(randomBlock).gameObject;
    }

    // === ИСПРАВЛЕННАЯ АДАПТИВНАЯ КОРУТИНА СДВИГОВ ===
    IEnumerator ShiftingHoleRoutine(GameObject wall, ColorPuzzle puzzle)
    {
        GameObject currentHole = GetRandomHoleFromColumns(puzzle);
        List<Renderer> disabledRenderers = SetupHole(currentHole);

        // 1. Получаем текущую скорость игры
        float v = 10f;
        if (GameManager.instance != null) v = GameManager.instance.GetCurrentSpeed();

        // 2. Задаем параметры безопасности (сколько времени/расстояния НАДО оставить игроку в конце)
        float reactionTime = 0.55f; // Секунды на реакцию ПОСЛЕ последнего сдвига
        float safetyGap = 1.0f;    // Запасной зазор в метрах
        float safeZ = (v * reactionTime) + safetyGap; // Критическая точка Z, ближе которой двигать отверстие НЕЛЬЗЯ

        // 3. Вычисляем динамический интервал между сдвигами
        float spawnZ = transform.position.z; // Точка, где стена появилась
        float distanceAvailable = spawnZ - safeZ; // Дистанция, которую стена может потратить на «мигание»
        float timeAvailable = distanceAvailable / v; // Время в секундах на всю дистанцию мигания

        float dynamicShiftInterval = shiftInterval;

        if (timeAvailable > 0)
        {
            // Нам нужно успеть сделать ровно 3 сдвига за доступное время
            float maxSafeInterval = timeAvailable / 3f;
            // Выбираем меньшее: либо стандартный комфортный интервал, либо ускоренный безопасный
            dynamicShiftInterval = Mathf.Min(shiftInterval, maxSafeInterval);
        }

        // 4. Цикл блуждания отверстия
        for (int i = 0; i < 3; i++)
        {
            // Ждем вычисленное безопасное время
            yield return new WaitForSeconds(dynamicShiftInterval);

            if (wall == null) yield break;

            // СТРАХОВКА: Если стена из-за лагов или огромной скорости УЖЕ пересекла безопасную черту,
            // немедленно прекращаем сдвиги. Текущее отверстие останется финальным.
            if (wall.transform.position.z <= safeZ)
            {
                break; 
            }

            ResetHole(currentHole, disabledRenderers);

            GameObject nextHole = currentHole;
            int safetyCounter = 0;
            while (nextHole == currentHole && safetyCounter < 10)
            {
                nextHole = GetRandomHoleFromColumns(puzzle);
                safetyCounter++;
            }

            currentHole = nextHole;
            disabledRenderers = SetupHole(currentHole);
        }
    }

    IEnumerator LateHideBlock(GameObject wall, GameObject hole)
    {
        float reactionTime = 0.5f;
        float safetyGap = 1.0f;    
        
        float v = 10f;
        if (GameManager.instance != null) v = GameManager.instance.GetCurrentSpeed();

        float triggerZ = (v * reactionTime) + safetyGap;

        while (wall != null && wall.transform.position.z > triggerZ)
        {
            yield return null;
        }

        if (hole != null) SetupHole(hole);
    }

    List<Renderer> SetupHole(GameObject hole)
    {
        if (hole == null) return null;

        List<Renderer> hiddenRenderers = new List<Renderer>();
        Renderer[] allRenderers = hole.GetComponentsInChildren<Renderer>();
        
        foreach (Renderer ren in allRenderers)
        {
            if (ren.enabled)
            {
                ren.enabled = false;
                hiddenRenderers.Add(ren); 
            }
        }

        BoxCollider col = hole.GetComponent<BoxCollider>();
        if (col != null)
        {
            col.isTrigger = true;
        }

        hole.tag = "ScoreZone";
        
        return hiddenRenderers; 
    }

    void ResetHole(GameObject hole, List<Renderer> renderersToEnable)
    {
        if (hole == null) return;

        if (renderersToEnable != null)
        {
            foreach (Renderer ren in renderersToEnable)
            {
                if (ren != null) ren.enabled = true;
            }
        }

        BoxCollider col = hole.GetComponent<BoxCollider>();
        if (col != null)
        {
            col.isTrigger = false; 
        }

        hole.tag = "Obstacle";
    }
}