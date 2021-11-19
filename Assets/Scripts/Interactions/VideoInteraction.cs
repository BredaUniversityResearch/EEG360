using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using UnityEngine.Video;

public class VideoInteraction : InteractionObject
{
    [SerializeField]
    private string m_type = "video";

    private ServerMessage m_interactionData;

    private InteractionManager m_manager;

    private float m_activeTime = 0f;

    private string m_directory = "";

    private VideoPlayer m_videoPlayer;

    private bool m_startedPlaying = false;
    public override void UpdateObject(InteractionManager manager)
    {
        CheckTime(manager);
        CheckVideoState();
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

        m_videoPlayer = GetComponent<VideoPlayer>();

        if ((settings.directory.StartsWith("\\"))||(settings.directory.StartsWith("/")))
            m_directory = Application.dataPath + settings.directory;
        else
            m_directory = settings.directory;

        SetRotation(m_interactionData.rotation);

        SetVideo();

        manager.DisableControllerLines();

        manager.SetInteractionToActive(this);
    }

    void SetVideo()
    {
        string filePath = m_directory + m_interactionData.filename;

        if (File.Exists(filePath)) // ERROR: The name 'File' does not exist in the current context?
        {
            m_videoPlayer.source = VideoSource.Url;
            m_videoPlayer.url = "file://" + m_directory.Replace("/", "\\") + m_interactionData.filename;
            m_videoPlayer.Prepare();
        }
        else
        {
            m_manager.SendMessageToServer("Error: video not found");
            m_manager.LogToFile("Video not found: " + m_interactionData.filename);
            StopInteraction(m_manager);
        }
    }

    void CheckVideoState()
    {
        if ((m_videoPlayer.isPlaying == false) && (m_startedPlaying == true))
            StopInteraction(m_manager);

        if ((m_videoPlayer.isPrepared) && (m_startedPlaying == false))
        {
            m_videoPlayer.Play();
            m_manager.SendMessageToServer("OnScreen");
            m_manager.LogToFile("Show Video: " + m_interactionData.filename);
            m_startedPlaying = true;
            m_videoPlayer.loopPointReached += EndReached;
        }        
    }

    void EndReached(VideoPlayer vp)
    {
        StopInteraction(m_manager);
        m_videoPlayer.loopPointReached -= EndReached;
    }

    void SetRotation(Vector3 rotation)
    {
        this.transform.rotation = Quaternion.Euler(rotation);
    }

    public override void StopInteraction(InteractionManager manager)
    {
        m_manager.LogToFile("Hide Video");

        m_videoPlayer.Stop();
        m_startedPlaying = false;

        m_activeTime = 0f;

        manager.SetInteractionToInactive(this);

        manager.SendMessageToServer("Continue");

        this.gameObject.SetActive(false);
    }

    public override string GetInteractionType(InteractionManager manager)
    {
        return m_type;
    }
}
