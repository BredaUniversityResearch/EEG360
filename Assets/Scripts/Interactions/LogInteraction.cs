using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LogInteraction : InteractionObject
{
    [SerializeField]
    private string m_type = "log";

    private ServerMessage m_interactionData;

    private InteractionManager m_manager;

    public override void UpdateObject(InteractionManager manager)
    {
    }

    public override void CheckTime(InteractionManager manager)
    {     
    }

    public override void SetInteraction(InteractionManager manager, ServerMessage message)
    {
        m_interactionData = message;
        m_manager = manager;

        if (message.labels.Count > 0)
            m_manager.LogToFile(message.text, message.labels[0]);
        else
            m_manager.LogToFile(message.text);

        StopInteraction(m_manager);
    }


    public override void StopInteraction(InteractionManager manager)
    {
        this.gameObject.SetActive(false);
    }

    public override string GetInteractionType(InteractionManager manager)
    {
        return m_type;
    }
}
