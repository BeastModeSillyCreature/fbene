using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class MenuManager : MonoBehaviour
{
    [Header("Панели UI")]
    public GameObject mainPanel;
    public GameObject farmPanel;
    public GameObject storePanel;
    public GameObject donstorePanel;

    [Header("Настройки камеры")]
    public Camera MainCamera;
    [Header("Настройки позиции")]
    public Vector3 mainPos;
    public Vector3 farmPos;
    public Vector3 storePos;
    public Vector3 donstorePos;

    [Header("Настройки поворота (углы)")]
    public Vector3 mainRot;
    public Vector3 farmRot;
    public Vector3 storeRot;
    public Vector3 donstoreRot;

    private Vector3 targetPos;
    private Vector3 targetRot; // Целевой угол поворота
    public float moveSpeed = 5f;

    void Start()
    {
        targetPos = mainPos;
        targetRot = mainRot;
        ShowPanel(mainPanel);
    }

    void Update()
    {
        // Плавно двигаем позицию
        MainCamera.transform.position = Vector3.Lerp(MainCamera.transform.position, targetPos, Time.deltaTime * moveSpeed);

        // Плавно вращаем камеру (используем Quaternion.Euler для перевода градусов в формат Unity)
        Quaternion targetQuaternion = Quaternion.Euler(targetRot);
        MainCamera.transform.rotation = Quaternion.Lerp(MainCamera.transform.rotation, targetQuaternion, Time.deltaTime * moveSpeed);
    }

    // --- КНОПКИ ГЛАВНОГО МЕНЮ ---
    public void PlayGame()
    {
        // 1. Даем команду менеджеру начать игру
        if (GameManager.instance != null)
        {
            GameManager.instance.StartGame();
        }
    
        // 2. Прячем главное меню
        if (mainPanel != null)
        {
            mainPanel.SetActive(false);
        }
    }

    public void ExitGame()
    {
        #if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
        #else
            Application.Quit();
        #endif
    }

    public void GoToFarm()
    {
        targetPos = farmPos;
        targetRot = farmRot;
        ShowPanel(farmPanel);
    }

    public void GoToStore()
    {
        targetPos = storePos;
        targetRot = storeRot;
        ShowPanel(storePanel);
    }

    public void GoTodonStore()
    {
        targetPos = donstorePos;
        targetRot = donstoreRot;
        ShowPanel(donstorePanel);
    }

    public void GoBack()
    {
        targetPos = mainPos;
        targetRot = mainRot;
        ShowPanel(mainPanel);
    }

    private void ShowPanel(GameObject panelToShow)
    {
        mainPanel.SetActive(panelToShow == mainPanel);
        farmPanel.SetActive(panelToShow == farmPanel);
        storePanel.SetActive(panelToShow == storePanel);
        donstorePanel.SetActive(panelToShow == donstorePanel);
    }
}