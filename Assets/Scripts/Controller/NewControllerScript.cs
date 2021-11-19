using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.XR;
using UnityEngine.XR.Interaction.Toolkit;

public class NewControllerScript : MonoBehaviour
{
    public XRInteractorLineVisual m_LineVisual = new XRInteractorLineVisual();
    public float m_StartingLength = 5f;

    private void Awake()
    {
        m_LineVisual = GetComponent<XRInteractorLineVisual>();
        m_StartingLength = m_LineVisual.lineLength;
    }
    public void EnableLine()
    {
        Debug.Log("Enable Line");
        m_LineVisual.lineLength = m_StartingLength;
    }
    public void DisableLine()
    {
        Debug.Log("Disable Line");
        m_LineVisual.lineLength = 0;
    }
}
