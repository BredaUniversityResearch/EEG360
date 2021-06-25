using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using UnityEngine.InputSystem.XR;

public class PhotoInteraction : InteractionObject
{
    [SerializeField]
    private string m_type = "photo";

    private ServerMessage m_interactionData;

    private InteractionManager m_manager;

    private float m_activeTime = 0f;

    private string m_directory = "";

    private MeshRenderer m_renderer;

    private Texture2D m_ImageToDisplay;

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

        ConfigurationSettings settings = manager.GetSettings();

        m_renderer = GetComponent<MeshRenderer>();

        if ((settings.directory.StartsWith("\\"))||(settings.directory.StartsWith("/")))
            m_directory = Application.dataPath + settings.directory;
        else
            m_directory = settings.directory;

        SetRotation(m_interactionData.rotation);

        DisplayPhoto();

        manager.SetInteractionToActive(this);
    }
    
    void DisplayPhoto()
    {
        m_ImageToDisplay = LoadPhoto(m_directory, m_interactionData.filename);
        m_renderer.material.mainTexture = m_ImageToDisplay;
        m_manager.SendMessageToServer("OnScreen");
        m_manager.LogToFile("Show Photo: " + m_interactionData.filename);
    }

    Texture2D LoadPhoto(string directory, string name)
    {
        Texture2D texture = null;
        string filePath = directory + name;
        byte[] fileData;

        if (File.Exists(filePath)) // ERROR: The name 'File' does not exist in the current context?
        {
            fileData = File.ReadAllBytes(filePath); // ERROR: The name 'File' does not exist in the current context?
            texture = new Texture2D(2, 2);
            texture.LoadImage(fileData);
        }
        else
        {
            m_manager.SendMessageToServer("Error: image not found");
            m_manager.LogToFile("Photo not found: " + m_interactionData.filename);
            StopInteraction(m_manager);
        }

        return texture;
    }

    void SetRotation(Vector3 rotation)
    {
        this.transform.rotation = Quaternion.Euler(rotation);
    }


    public override void StopInteraction(InteractionManager manager)
    {
        manager.LogToFile("Hide Photo");

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
