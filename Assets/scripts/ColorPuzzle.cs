using UnityEngine;

public class ColorPuzzle : MonoBehaviour
{
    [Header("Цвета для выбора")]
    public Color color1 = Color.red;
    public Color color2 = Color.green;
    public Color color3 = Color.blue;

    [Header("Ссылки")]
    public Renderer frameRenderer;
    public GameObject[] columns; // Положи сюда Col_Left, Col_Center, Col_Right

    [HideInInspector] public int correctIndex;

    public void InitPuzzle()
    {
        // 1. Красим каждую колонну в свой цвет
        SetColumnColor(0, color1);
        SetColumnColor(1, color2);
        SetColumnColor(2, color3);

        // 2. Выбираем случайную "правильную" колонну
        correctIndex = Random.Range(0, 3);

        // 3. Красим рамку в цвет этой колонны
        if (correctIndex == 0) frameRenderer.material.color = color1;
        else if (correctIndex == 1) frameRenderer.material.color = color2;
        else frameRenderer.material.color = color3;
    }

    void SetColumnColor(int index, Color targetColor)
    {
        // Находим все рендереры в колонне (включая трубы) и красим их
        Renderer[] rens = columns[index].GetComponentsInChildren<Renderer>();
        foreach (var r in rens)
        {
            r.material.color = targetColor;
        }
    }
}