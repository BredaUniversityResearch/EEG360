using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CancelInteraction : InteractionObject
{
    [SerializeField]
    private string m_type = "base";

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

        m_manager.ForceStopInteraction(m_interactionData.labels[0]);

        StopInteraction(m_manager);
    }


    public override void StopInteraction(InteractionManager manager)
    {
        manager.SendMessageToServer("Continue");

        this.gameObject.SetActive(false);
    }

    public override string GetInteractionType(InteractionManager manager)
    {
        return m_type;
    }
}
