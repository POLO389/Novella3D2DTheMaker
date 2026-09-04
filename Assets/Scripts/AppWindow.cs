using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class AppWindow : MonoBehaviour, IPointerDownHandler
{
    [Header("Настройки окна")]
    public GameObject windowPanel;

    private void Awake()
    {
        if (windowPanel == null)
            windowPanel = gameObject;
    }

    private void Start()
    {
        // Подвязываем все кнопки внутри окна, чтобы клик по ним тоже выводил окно наверх
        Button[] buttons = GetComponentsInChildren<Button>(true);
        foreach (var btn in buttons)
        {
            btn.onClick.AddListener(BringToFront);
        }
    }

    // Вызывается при ЛЮБОМ клике по окну или его фону
    public void OnPointerDown(PointerEventData eventData)
    {
        BringToFront();
    }

    // Вывести окно на самый передний план
    public void BringToFront()
    {
        if (windowPanel != null)
        {
            windowPanel.transform.SetAsLastSibling();
        }
        else
        {
            transform.SetAsLastSibling();
        }
    }

    public void OpenWindow()
    {
        if (windowPanel != null)
        {
            windowPanel.SetActive(true);
            BringToFront();
        }
    }

    public void CloseWindow()
    {
        if (windowPanel != null)
        {
            windowPanel.SetActive(false);
        }
    }

    public void ToggleWindow()
    {
        if (windowPanel != null)
        {
            windowPanel.SetActive(!windowPanel.activeSelf);
            if (windowPanel.activeSelf)
            {
                BringToFront();
            }
        }
    }
}