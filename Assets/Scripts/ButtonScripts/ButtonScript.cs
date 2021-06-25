using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEngine.Events;

[System.Serializable]
public class MyButtonEvent : UnityEvent<string>
{
}

public class ButtonScript : MonoBehaviour
{
    private InteractionObject m_manager;
    private string m_text;
    private string m_response;
    private Sprite m_sprite;

    [SerializeField]
    private TextMeshProUGUI m_textObject = null;
    [SerializeField]
    private TextMeshProUGUI m_responseObject = null;
    [SerializeField]
    private Image m_imageObject = null;

    public MyButtonEvent m_buttonEvent;

    private void Start()
    {
        if (m_buttonEvent == null)
            m_buttonEvent = new MyButtonEvent();
    }

    public void SetButton(InteractionObject manager, string response, Sprite image)
    {
        m_manager = manager;
        m_response = response;
        m_sprite = image;

        if (m_imageObject != null)
            m_imageObject.sprite = m_sprite;

        if (m_responseObject != null)
            m_responseObject.text = m_response;
    }

    public void SetButton(InteractionObject manager, string text, string response)
    {
        m_manager = manager;
        m_response = response;
        m_text = text;

        if (m_textObject != null)
            m_textObject.text = m_text;

        if (m_responseObject != null)
            m_responseObject.text = m_response;
    }

    public void SetButton(InteractionObject manager, string text, string response, Sprite image)
    {
        m_manager = manager;
        m_response = response;
        m_text = text;
        m_sprite = image;

        if (m_textObject != null)
            m_textObject.text = m_text;

        if (m_responseObject != null)
            m_responseObject.text = m_response;

        if (m_imageObject != null)
            m_imageObject.sprite = m_sprite;
    }

    public void ButtonPressed()
    {
        m_buttonEvent.Invoke(m_response);
    }
}
