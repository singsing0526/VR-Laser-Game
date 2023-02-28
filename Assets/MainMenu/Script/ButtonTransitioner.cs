using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;

public class ButtonTransitioner : MonoBehaviour,IPointerClickHandler,IPointerDownHandler,IPointerEnterHandler,IPointerExitHandler
{
    public Color32 m_NormalColor = Color.white;
    public Color32 m_HoverColor = Color.gray;
    public Color32 m_Down = Color.white;

    private Image m_Image = null;

    private void Awake()
    {
        m_Image = GetComponent<Image>();
        PlayerPrefs.SetString("CurrentLevel", "1");
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        m_Image.color = m_HoverColor;
    }
    public void OnPointerExit(PointerEventData eventData)
    {
        m_Image.color = m_NormalColor;
    }
    public void OnPointerDown(PointerEventData eventData)
    {
        m_Image.color = m_Down;
    }
    public void OnPointerClick(PointerEventData eventData)
    {
        SceneManager.LoadScene("Map");
    }
}
