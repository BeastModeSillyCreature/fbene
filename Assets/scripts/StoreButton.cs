using UnityEngine;
using UnityEngine.UI; 
using TMPro;         

public class BuySkinButton : MonoBehaviour
{
    [Header("Настройки скина")]
    public int skinIndex; // Индекс скина (0, 1, 2...)
    public int price;     // Цена скина

    [Header("Ссылки на UI")]
    public TextMeshProUGUI buttonText; 

    void OnEnable()
    {
        UpdateButtonUI();
    }

    void Start()
    {
        UpdateButtonUI();
    }

    public void UpdateButtonUI()
    {
        // ДИАГНОСТИКА 1: Проверяем, на месте ли синглтон данных
        if (GameData.instance == null) 
        {
            
            return;
        }

        // ДИАГНОСТИКА 2: Самая частая причина! Проверяем, привязана ли ссылка на Текст
        if (buttonText == null) 
        {
            
            return;
        }

        // ДИАГНОСТИКА 3: Смотрим, какие значения видит кнопка в момент обновления
        bool isUnlocked = GameData.instance.IsSkinUnlocked(skinIndex);
        int currentEquipped = GameData.instance.currentSkinIndex;
        
        

        // 1. Если этот скин сейчас НАДЕТ
        if (currentEquipped == skinIndex)
        {
            buttonText.text = "equipped";
        }
        // 2. Если скин КУПЛЕН, но сейчас не надет
        else if (isUnlocked)
        {
            buttonText.text = "equip";
        }
        // 3. Если скин еще НЕ КУПЛЕН
        else
        {
            buttonText.text = $"buy ({price})";
        }
    }

    public void ClickBuySkin()
    {
        if (GameData.instance == null) return;

        if (GameData.instance.IsSkinUnlocked(skinIndex))
        {
            GameData.instance.EquipSkin(skinIndex);
        }
        else
        {
            GameData.instance.BuySkin(skinIndex, price);
        }

        RefreshAllButtons();
    }

    private void RefreshAllButtons()
    {
        // Находим вообще все кнопки, включая те, что на родительских объектах
        BuySkinButton[] allButtons = Resources.FindObjectsOfTypeAll<BuySkinButton>();
        foreach (BuySkinButton btn in allButtons)
        {
            // Обновляем только те, которые активны в Иерархии прямо сейчас
            if (btn.gameObject.scene.name != null) 
            {
                btn.UpdateButtonUI();
            }
        }
    }
}