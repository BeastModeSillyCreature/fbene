using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameData : MonoBehaviour
{
    public static GameData instance;
    
    [Header("Баланс")]
    public int grain; 
    public int highScore;
    public int currentSkinIndex { get; private set; } // Текущий надетый скин

    [Header("UI Ссылки")]
    public TextMeshProUGUI scoreText;
    public TextMeshProUGUI grainText;
    public TextMeshProUGUI highScoreText;
    public TextMeshProUGUI plantButtonText;

    [Header("Эффекты")]
    public ParticleSystem fireParticles;
    public ParticleSystem fireParticles2;

    [Header("Настройки фермы")]
    public GameObject[] wheatStages;
    private int currentStage = 0;

    private int wallsPassed;

    void Awake()
    {
        if (instance == null) instance = this;
        
        // Загружаем сохраненный скин при старте игры (если сохранения нет, будет 0)
        currentSkinIndex = PlayerPrefs.GetInt("SelectedSkin", 0);
    
        // Скин под индексом 0 (стартовый) всегда разблокирован по умолчанию
        PlayerPrefs.SetInt("SkinUnlocked_0", 1); 
    
        // ИСПРАВЛЕНО: Привели к единому ключу сохранения зёрен "Grain"
        grain = PlayerPrefs.GetInt("Grain", 0);
    }

    void Start()
    {
        highScore = PlayerPrefs.GetInt("HighScore", 0);
        grain = PlayerPrefs.GetInt("Grain", 0); 
        
        UpdateUI();
        
        foreach(var stage in wheatStages) 
        {
            if (stage != null) stage.SetActive(false);
        }
    }

    public void ResetProgress()
    {
        // Стираем все сохранения чистенько
        PlayerPrefs.DeleteAll();
        PlayerPrefs.Save();

        Debug.Log("Память очищена. Перезапускаем мир...");

        // Перезапускаем текущий уровень. Все анимации, физика и скрипты создадутся с нуля без багов!
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    // Проверка: куплен ли скин?
    public bool IsSkinUnlocked(int index)
    {
        return PlayerPrefs.GetInt("SkinUnlocked_" + index, 0) == 1;
    }

    // Логика смены (надевания) уже купленного скина
    public void EquipSkin(int index)
    {
        if (IsSkinUnlocked(index))
        {
            currentSkinIndex = index;
        
            // Сохраняем выбор игрока
            PlayerPrefs.SetInt("SelectedSkin", index);
            PlayerPrefs.Save(); 
        
            Debug.Log($"Скин {index} успешно надет!");
        
            // !!! ИСПРАВЛЕНИЕ: Принудительно переодеваем персонажа на экране !!!
            PlayerController player = FindAnyObjectByType<PlayerController>();
            if (player != null)
            {
                player.ApplySelectedSkin();
            }
        }
    }

    // Логика покупки скина
    public void BuySkin(int skinIndex, int price)
    {
        if (IsSkinUnlocked(skinIndex)) return; // Если уже куплен — ничего не делаем

        if (grain >= price)
        {
            grain -= price;
        
            // Сохраняем новый баланс зерен
            PlayerPrefs.SetInt("Grain", grain);
        
            // !!! ИСПРАВЛЕНО: Регистрируем разблокировку скина в системе сохранений !!!
            PlayerPrefs.SetInt("SkinUnlocked_" + skinIndex, 1);
            
            // !!! ИСПРАВЛЕНО: Обновляем текущую переменную скина в коде !!!
            currentSkinIndex = skinIndex;
            PlayerPrefs.SetInt("SelectedSkin", skinIndex);
            
            PlayerPrefs.Save();
        
            UpdateUI();
        
            Debug.Log($"Скин {skinIndex} успешно куплен! Списано зерен: {price}");

            PlayerController player = FindAnyObjectByType<PlayerController>();
            if (player != null)
            {
                player.ApplySelectedSkin();
            }
        }
        else
        {
            Debug.Log($"Не хватает зерен для покупки скина! Нужно: {price}, есть: {grain}");
        }
    }

    public void BuyItem(int price)
    {
        if (grain >= price)
        {
            grain -= price;
            PlayerPrefs.SetInt("Grain", grain);
            PlayerPrefs.Save();
            UpdateUI();
            Debug.Log("Покупка совершена! Списано: " + price + ". Оставлось: " + grain);
        }
    }

    public void BuyGrains(int amount)
    {
        grain += amount; 
        PlayerPrefs.SetInt("Grain", grain);
        PlayerPrefs.Save();
        UpdateUI();
        Debug.Log($"Добавлено зерен: {amount}. Баланс: {grain}");
    }

    public void PlantNext()
    {
        if (currentStage < wheatStages.Length && grain >= 10)
        {
            grain -= 10;
            PlayerPrefs.SetInt("Grain", grain); 
            PlayerPrefs.Save(); 
            wheatStages[currentStage].SetActive(true);
            currentStage++;
            UpdateUI();
        }
    }

    public void AddPoint()
    {
        wallsPassed++;
        if (wallsPassed % 10 == 0)
        {
            grain += 5;
            PlayerPrefs.SetInt("Grain", grain);
            PlayerPrefs.Save();
        }

        if (wallsPassed % 25 == 0) TriggerFire();

        if (wallsPassed > highScore)
        {
            highScore = wallsPassed;
            PlayerPrefs.SetInt("HighScore", highScore);
        }

        UpdateUI();
    }

    private void TriggerFire()
    {
        if (fireParticles != null) { fireParticles.Stop(); fireParticles.Play(); }
        if (fireParticles2 != null) { fireParticles2.Stop(); fireParticles2.Play(); }
    }

    public void UpdateUI()
    {
        if(scoreText) scoreText.text = "Score: " + wallsPassed;
        if(grainText) grainText.text = "Grain: " + grain;
        if(highScoreText) highScoreText.text = "Best: " + highScore;

        // === ЛОГИКА ДЛЯ КНОПКИ ПОСАДКИ ПШЕНИЦЫ ===
        if (plantButtonText != null)
        {
            // Если все стадии грядок уже активированы
            if (currentStage >= wheatStages.Length)
            {
                plantButtonText.text = "no more space";

                // По желанию: делаем саму кнопку «выключенной» (серой), 
                // чтобы игрок не мог кликать по ней впустую
                Button btn = plantButtonText.GetComponentInParent<Button>();
                if (btn != null) btn.interactable = false;
            }
            else
            {
                // Стандартный текст, пока грядки еще есть
                plantButtonText.text = "plant (10)";
            }
        }
    }
}