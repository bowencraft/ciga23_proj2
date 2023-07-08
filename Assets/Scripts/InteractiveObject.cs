using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InteractiveObject : MonoBehaviour
{
    public bool isInHighLight;

    public int mainFeeling;
    public int minorFeeling;

    public bool unlocked;

    public Color newColor;
    private Material renderer;

    private void Start()
    {
        // 获取 Renderer 组件
        renderer = GetComponent<Renderer>().material;

        // 调整材质属性
        Debug.Log(renderer);
        if (renderer != null)
        {
            // 获取当前的 Material

            // 修改颜色属性
            renderer.SetColor("_Albedo", newColor);
        }
    }

    // Update is called once per frame
    void Update()
    {
    }

    public void Interact()
    {

        if (isInHighLight)
        {
            Debug.Log("Interacted");
            int tempFeeling = mainFeeling;
            mainFeeling = minorFeeling;
            minorFeeling = tempFeeling;

            GameManager.Instance.StatusManager.GetComponent<StatusManager>().currentStatus = mainFeeling;

        }

        
    }



}
