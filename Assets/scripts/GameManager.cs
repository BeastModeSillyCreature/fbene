using UnityEngine;
using UnityEngine.SceneManagement; // Нужно для перезагрузки сцены
using UnityEngine.UI; // Нужно для работы с текстом
using TMPro;

public class GameManager : MonoBehaviour
{
    public static GameManager instance; // Позволяет обращаться к менеджеру из любого скрипта
    public int score = 0;
    public TextMeshProUGUI scoreText; // Перетащи сюда объект Text из UI
    public float baseSpeed = 10f;
    public float speedMultiplier = 0.5f; // На сколько растет скорость за каждое очко
    public AnimationCurve difficultyCurve; // Появится кривая в инспекторе
    public float timeScale = 100f; // На сколько очков рассчитана кривая
    public bool isGameActive = false;

    void Start()
    {
        // Проверяем, привязан ли объект в инспекторе
        if (scoreText != null)
        {
            scoreText.text = "Score: " + score;
        }
        else
        {
            Debug.LogError("Забыл перетащить объект текста в GameManager!");
        }
    }

    public float GetCurrentSpeed()
    {
        // Оцениваем точку на кривой в зависимости от счета
        float speedBoost = difficultyCurve.Evaluate(score / timeScale);
        return baseSpeed + speedBoost;
    }

    void Awake()
    {
        instance = this;
    }

    public void AddScore()
    {
        score++;
        scoreText.text = "Score: " + score;
    }

    public void StartGame()
    {
        isGameActive = true;
    }

    public void RestartGame()
    {
        // Перезагружаем текущую сцену
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
}