using System.Collections;
using System.Collections.Generic;
using System.IO;
using System;
using UnityEditor;
using UnityEngine;
using System.Linq;
using UnityEngine.XR;

public class InteractionManager : MonoBehaviour
{

    private NetworkingScript m_connection = null;

    [SerializeField]
    private InteractionObject[] m_interactionTypes = null;

    [SerializeField]
    private GameObject m_pointerPrefab = null;

    private Dictionary<string, InteractionObject> m_activeInteractions = new Dictionary<string, InteractionObject>();

    private ConfigurationSettings m_settings = new ConfigurationSettings();

    [SerializeField]
    private BackgroundSettings m_background = null;

    [SerializeField]
    private string m_testMessage = "";

    bool m_performedStartInteraction = false;

    void Start()
    {
        m_connection = GetComponent<NetworkingScript>();
        DisableAllInteractions();
        LoadConfigData();
        OutputConfigText();
        m_connection.SetNetworkingSettings(m_settings);
        m_connection.StartNetworking();
    }

    void Update()
    {
        if (!m_performedStartInteraction)
            PerformStartInteraction();

        CheckLatestMessage();
        UpdateActiveInteractions();
        CheckTestMessage();
    }

    void UpdateActiveInteractions()
    {
        string[] currentActive = m_activeInteractions.Keys.ToArray();
        foreach (string action in currentActive)
        {
            foreach (InteractionObject interaction in m_interactionTypes)
            {
                if (interaction.GetInteractionType(this) == action)
                    interaction.UpdateObject(this);
            }
        }
    }

    void PerformStartInteraction()
    {
        SetBackgroundColor();

        if (m_settings.startingMessage!= "")
        {
            ServerMessage sMessage = DecodeMessage(m_settings.startingMessage);
            PerformInteraction(sMessage);
        }
        m_performedStartInteraction = true;
    }

    void CheckTestMessage()
    {

        if (m_testMessage != "")
        {

            ServerMessage sMessage = DecodeMessage(m_testMessage);
            m_testMessage = "";

            PerformInteraction(sMessage);
        }
    }

    void CheckLatestMessage()
    {
        if ((m_connection.GetLatestMessage() != "")&& (m_connection.GetLatestMessage() != null)){

            ServerMessage sMessage = DecodeMessage(m_connection.GetLatestMessage());

            PerformInteraction(sMessage);

            m_connection.ResetLastMessage();
        }
    }

    public void SendMessageToServer(string messsage)
    {
        m_connection.SendMessageToServer(messsage);
    }

    void PerformInteraction(ServerMessage message)
    {
        foreach (InteractionObject interaction in m_interactionTypes)
        {
            if (interaction.GetInteractionType(this) == message.type)
            {
                if (m_activeInteractions.ContainsKey(message.type))
                {
                    m_activeInteractions[message.type].StopInteraction(this);
                    SendMessageToServer("PreviousInteractionStoppedEarly");
                }

                interaction.gameObject.SetActive(true);
                interaction.SetInteraction(this, message);
            }
        }
    }

    public void SetInteractionToActive(InteractionObject interaction)
    {
        m_activeInteractions.Add(interaction.GetInteractionType(this), interaction);
    }

    public void SetInteractionToInactive(InteractionObject interaction)
    {
        m_activeInteractions.Remove(interaction.GetInteractionType(this));    
    }

    public void ForceStopInteraction(string interactionName)
    {
        foreach (InteractionObject interaction in m_interactionTypes)
        {
            if (interaction.gameObject.activeSelf)
                interaction.StopInteraction(this);
        }
    }

    void DisableAllInteractions()
    {
        foreach (KeyValuePair<string, InteractionObject> interaction in m_activeInteractions)
            interaction.Value.StopInteraction(this);
    }
    
    ServerMessage DecodeMessage(string message)
    {
        ServerMessage sMessage = new ServerMessage();

        List<string> labels = new List<string>(1);

        string[] messageSections = message.Split('|');

        for (int i = 0; i < messageSections.Length; i++)
        {
            if (messageSections[i].StartsWith("type="))
                sMessage.type = messageSections[i].Replace("type=", "");

            if (messageSections[i].StartsWith("text="))
                sMessage.text = messageSections[i].Replace("text=", "");

            if (messageSections[i].StartsWith("filename="))
                sMessage.filename = messageSections[i].Replace("filename=", "");

            if (messageSections[i].StartsWith("amount="))
            {
                int amount = 0;
                int.TryParse(messageSections[i].Replace("amount=", ""), out amount);
                sMessage.amount = amount;
            }
            if (messageSections[i].StartsWith("duration="))
            {
                int duration = 0;
                int.TryParse(messageSections[i].Replace("duration=", ""), out duration);
                sMessage.duration = duration;
            }
            if (messageSections[i].StartsWith("label"))
            {
                labels.Add(messageSections[i].Substring(messageSections[i].IndexOf('=') + 1));
                sMessage.labels = labels;
            }
            if (messageSections[i].StartsWith("rotation"))
            {
                Vector3 rotation;
                rotation = GetVector3FromString(messageSections[i].Replace("duration=", ""));
                sMessage.rotation = rotation;
            }
        }
        return sMessage;
    }

    Vector3 GetVector3FromString(string input)
    {
        Vector3 outputVector = new Vector3(0, 0, 0);
        string[] vectorSections = input.Split(',');
        if (vectorSections.Length == 3)
        {
            int[] values = new int[3];
            for (int i=0; i<vectorSections.Length; i++)
            {
                int.TryParse(vectorSections[i], out values[i]);
            }
            outputVector = new Vector3(values[0], values[1], values[2]);
        }
        return outputVector;
    }

    void OutputConfigText()
    {
        string json = JsonUtility.ToJson(m_settings);
        Debug.Log(json);
    }

    void LoadConfigData() {
        if (File.Exists(Application.dataPath+"/config.json"))
        {
            StreamReader reader =new StreamReader(Application.dataPath+ "/config.json");
            string json = reader.ReadToEnd();
            m_settings = JsonUtility.FromJson<ConfigurationSettings>(json);
        }
    }

    public void EnableControllerLines()
    {
        GameObject[] controllers = GameObject.FindGameObjectsWithTag("Controller");

        foreach (GameObject controller in controllers)
        {
            controller.GetComponentInChildren<NewControllerScript>().EnableLine();
        }
    }

    public void DisableControllerLines()
    {
        GameObject[] controllers = GameObject.FindGameObjectsWithTag("Controller");

        foreach (GameObject controller in controllers)
        {
            controller.GetComponentInChildren<NewControllerScript>().DisableLine();
        }
    }

    void SetBackgroundColor()
    {
        if (m_background != null)
            m_background.SetBackground(m_settings.neutralColor);
    }

    public ConfigurationSettings GetSettings()
    {
        return m_settings;
    }

    public void LogToFile(string text, string type = "log")
    {
        string time = new DateTimeOffset(DateTime.Now).ToUnixTimeMilliseconds().ToString();
        
        string path = Application.dataPath + "/vr_log.txt";

        //If the file does not exists, then create the file including headers
        if (!File.Exists(path))
        {
            using (StreamWriter sw = File.CreateText(path))
            {
                sw.WriteLine("time,type,text");
            }
        }

        //If the file does exist, add the current log
        using (StreamWriter sw = File.AppendText(path))
        {
            string writeText = time + "," + type + "," + text;
            sw.WriteLine(writeText);
            Debug.Log(writeText);
        }

    }
}

public class ServerMessage
{
    string m_type = "";
    string m_text="";
    string m_filename = "";
    int m_amount =0;
    List<string> m_labels = new List<string>();
    float m_duration=0;
    Vector3 m_rotation = new Vector3(0, 0, 0);

    [SerializeField]
    public string text {
        get { return m_text; }
        set { m_text = value; }
    }
    [SerializeField]
    public string filename {
        get { return m_filename; }
        set { m_filename = value; }
    }
    [SerializeField]
    public string type {
        get { return m_type; }
        set { m_type = value; }
    }
    [SerializeField]
    public List<string> labels {
        get { return m_labels; }
        set { m_labels = value; }
    }
    [SerializeField]
    public float duration {
        get { return m_duration; }
        set { m_duration = value/1000; }
    }
    [SerializeField]
    public int amount {
        get { return m_amount; }
        set { m_amount = value; }
    }
    [SerializeField]
    public Vector3 rotation
    {
        get { return m_rotation; }
        set { m_rotation = value; }
    }
}

public class ConfigurationSettings
{
    public string m_dataDirectory = null;
    public string m_networkAddress = "127.0.0.1";
    public int m_networkPort = 8052;
    public Color m_neutralColor = Color.black;
    public string m_startingMessage = "type=text|text=Config File Not Found";

    [SerializeField]
    public string directory {
        get { return m_dataDirectory; }
        set { m_dataDirectory = value; }
    }
    [SerializeField]
    public string address {
        get { return m_networkAddress; }
        set { m_networkAddress = value; }
    }
    [SerializeField]
    public int port {
        get { return m_networkPort; }
        set { m_networkPort = value; }
    }
    [SerializeField]
    public Color neutralColor {
        get { return m_neutralColor; }
        set { m_neutralColor = value; }
    }
    [SerializeField]
    public string startingMessage {
        get { return m_startingMessage; }
        set { m_startingMessage = value; }
    }
}
