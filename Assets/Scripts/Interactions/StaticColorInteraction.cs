using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

public class StaticColorInteraction : InteractionObject
{
    [SerializeField]
    private string m_type = "neutral";

    private ServerMessage m_interactionData;

    private InteractionManager m_manager;

    private float m_activeTime = 0f;

    private MeshRenderer m_renderer;

    private Color m_color = Color.black;


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

        m_renderer = GetComponent<MeshRenderer>();

        ConfigurationSettings settings = manager.GetSettings();

        SetColor();

        manager.SetInteractionToActive(this);
        manager.SendMessageToServer("OnScreen");
    }
    
    void SetColor()
    {
        ColorUtility.TryParseHtmlString(m_interactionData.labels[0], out m_color);
        m_renderer.material.color = m_color;
    }

    public override void StopInteraction(InteractionManager manager)
    {
        m_activeTime = 0f;

        manager.SendMessageToServer("Continue");

        manager.SetInteractionToInactive(this);

        this.gameObject.SetActive(false);
    }

    public override string GetInteractionType(InteractionManager manager)
    {
        return m_type;
    }
}
