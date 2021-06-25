using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ButtonSprites : MonoBehaviour
{
    [SerializeField]
    private string m_type = null;
    [SerializeField]
    private int m_amount = 0;
    [SerializeField]
    private Sprite[] m_sprites = null;

    public string ButtonType() {
        return m_type;
    }
    public int ButtonAmount()
    {
        return m_amount;
    }
    public Sprite ButtonSprite(int num)
    {
        return m_sprites[num];
    }
}
