using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BaseInteraction : InteractionObject
{
    [SerializeField]
    private string m_type = "base";

    private ServerMessage m_interactionData;

    private InteractionManager m_manager;

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
                StopInteraction(m_manager);
    }


    public override void SetInteraction(InteractionManager manager, ServerMessage message)
    {
        m_interactionData = message;
        m_manager = manager;

        m_activeTime = 0f;

        manager.SetInteractionToActive(this);
    }


    public override void StopInteraction(InteractionManager manager)
    {
        manager.SendMessageToServer("Base");

        manager.SetInteractionToInactive(this);

        this.gameObject.SetActive(false);
    }

    public override string GetInteractionType(InteractionManager manager)
    {
        return m_type;
    }
}
