using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BackgroundSettings : MonoBehaviour
{
    [SerializeField]
    private Texture2D m_backgroundTexture;

    public void SetBackground(Color color)
    {
        Debug.Log(color);

        Color32[] colorArray = m_backgroundTexture.GetPixels32();

        for (int i = 0; i < colorArray.Length; i++)
        {
            colorArray[i] = color;
        }

        m_backgroundTexture.SetPixels32(colorArray);
        m_backgroundTexture.Apply();

       this.gameObject.GetComponent<Renderer>().material.mainTexture = m_backgroundTexture;
    }
}
