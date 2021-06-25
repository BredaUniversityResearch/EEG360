﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;

public class FocusInteraction : InteractionObject
{
    [SerializeField]
    private string m_type = "focus";
    [SerializeField]
    private TextMeshProUGUI m_text = null;
    [SerializeField]
    private GameObject m_continueButton;

    private ServerMessage m_interactionData = null;

    private InteractionManager m_manager = null;

    private EventSystem m_eventSystem = null;

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
                manager.SendMessageToServer("Continue");
                StopInteraction(m_manager);
            }        
    }

    public override void SetInteraction(InteractionManager manager, ServerMessage message)
    {
        m_interactionData = message;
        m_eventSystem = GetComponent<EventSystem>();
        m_manager = manager;

        if (m_interactionData.labels.Count != 0)
        {
            Color newCol;

            if (ColorUtility.TryParseHtmlString(m_interactionData.labels[0], out newCol))
            {
                m_text.color = newCol;
            }
        }

        if (m_interactionData.duration == 0)
        {
            m_continueButton.SetActive(true);
            manager.EnableControllerLines();
        }
        else
            m_continueButton.SetActive(false);

        m_activeTime = 0f;

        manager.SetInteractionToActive(this);

        manager.SendMessageToServer("OnScreen");
    }

    public override void StopInteraction(InteractionManager manager)
    {
        m_activeTime = 0f;

        manager.DisableControllerLines();

        manager.SetInteractionToInactive(this);

        this.gameObject.SetActive(false);
    }

    public override string GetInteractionType(InteractionManager manager)
    {
        return m_type;
    }

    public void ContinueButton()
    {
        m_manager.SendMessageToServer("Continue");
        StopInteraction(m_manager);
    }
}