using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;

public class ButtonInteraction : InteractionObject
{
    [SerializeField]
    private string m_type = "button";
    [SerializeField]
    private TextMeshProUGUI m_questionText;
    [SerializeField]
    private GameObject m_buttonPrefab;
    [SerializeField]
    private ButtonSprites[] m_buttonSprites;

    [SerializeField]
    private RectTransform m_buttonsParent;

    private ServerMessage m_interactionData;

    private InteractionManager m_manager;
    private Button[] m_buttons;

    private EventSystem m_eventSystem;

    private float m_activeTime = 0f;


    public override void UpdateObject(InteractionManager manager)
    {
        CheckTime(manager);
    }


    public override void CheckTime(InteractionManager manager)
    {
        m_activeTime += 1 * Time.deltaTime;
        if (m_interactionData.duration != 0)
            if (m_activeTime > m_interactionData.duration)
            {
                ButtonPressed("-1");
            }
    }

    public override void SetInteraction(InteractionManager manager, ServerMessage message)
    {
        m_interactionData = message;

        m_manager = manager;

        for (int i = 0; i < m_interactionData.amount; i++)
        {
            GameObject button;

            //Creat object
            button = Instantiate<GameObject>(m_buttonPrefab);


            button.transform.SetParent(m_buttonsParent);
            button.transform.localPosition = new Vector3(0, 0, 0);

            //Set button content
            if (m_interactionData.type == "information")//(m_interactionData.type == "question") || (m_interactionData.type == "information"))
            {
                button.GetComponent<ButtonScript>().SetButton(this, m_interactionData.labels[i], (i + 1).ToString());
            }
            else
                button.GetComponent<ButtonScript>().SetButton(this, m_interactionData.labels[i], (i + 1).ToString(), GetCorrectSprite(m_interactionData.type, m_interactionData.amount, i));

        }

        //Set starting values
        m_buttons = GetComponentsInChildren<Button>();
        SetupButtonClicks();
        if (m_questionText != null)
            m_questionText.text = message.text;
        m_activeTime = 0f;

        manager.EnableControllerLines();

        manager.SetInteractionToActive(this);

        m_manager.LogToFile("Show Question: "+message.text);

        manager.SendMessageToServer("OnScreen");
    }

    public override void StopInteraction(InteractionManager manager)
    {
        ClearButtons();

        m_activeTime = 0f;

        manager.DisableControllerLines();

        manager.SetInteractionToInactive(this);

        this.gameObject.SetActive(false);
    }

    void ClearButtons()
    {
        foreach (Button button in m_buttons)
            GameObject.Destroy(button.gameObject);

        m_buttons = null;
    }

    public override string GetInteractionType(InteractionManager manager)
    {
        return m_type;
    }

    public void SetupButtonClicks()
    {
        foreach (Button button in m_buttons)
        {
            button.GetComponent<ButtonScript>().m_buttonEvent.AddListener(ButtonPressed);
        }
    }


    void ButtonPressed(string response)
    {
        m_manager.LogToFile("Answer Question: " + response);

        m_manager.SendMessageToServer("Answer="+response);
        StopInteraction(m_manager);
    }

    Sprite GetCorrectSprite(string type, int amount, int number)
    {
        foreach (ButtonSprites bSprites in m_buttonSprites)
            if ((bSprites.ButtonType() == type) && (bSprites.ButtonAmount() == amount))
                return bSprites.ButtonSprite(number);
        return null;
    }
}
