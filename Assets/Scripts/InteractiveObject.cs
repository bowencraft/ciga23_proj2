using System.Collections;
using System.Collections.Generic;
using EFPController.Extras;
using UnityEngine;

public class InteractiveObject : MonoBehaviour
{
    public bool isInHighLight = false;

    public int mainFeeling;
    public int minorFeeling;

    public bool unlocked = false;

    public Color newColor;
    private Material renderer;

    private void Start()
    {
        // 获取 Renderer 组件
        renderer = GetComponent<Renderer>().material;

        // 调整材质属性
        Debug.Log(renderer);
    }

    // Update is called once per frame
    void Update()
    {
        if (renderer != null)
        {
            //Debug.Log(renderer.GetColor("_AlbedoColor"));
            // 获取当前的 Material

            // 修改颜色属性   
            //renderer.SetColor("_AlbedoColor", newColor);
        }

        if (isInHighLight)
        {

            float yRotation = Camera.main.transform.eulerAngles.y;
            transform.eulerAngles = new Vector3(transform.eulerAngles.x, yRotation, transform.eulerAngles.z);
        } else
        {
            this.GetComponent<InteractReceiver>().enabled = false;

        }
    }

    public void Interact()
    {

        if (isInHighLight)
        {
            Debug.Log("Interacted");
            int tempFeeling = mainFeeling;
            mainFeeling = minorFeeling;
            minorFeeling = tempFeeling;

            GameManager.Instance.StatusManager.GetComponent<StatusManager>().iconSwitch.SetTrigger("isSwitch");
            GameManager.Instance.StatusManager.GetComponent<StatusManager>().currentStatus = mainFeeling;


        }

        
    }

    public void Highlight()
    {
        isInHighLight = true;
        this.GetComponent<InteractReceiver>().enabled = true;
        renderer.SetColor("_AlbedoColor", GameManager.Instance.StatusManager.GetComponent<StatusManager>().highlightColor);

    }


    public void deHighlight()
    {
        isInHighLight = false;
        this.GetComponent<InteractReceiver>().enabled = false;
        renderer.SetColor("_AlbedoColor", GameManager.Instance.StatusManager.GetComponent<StatusManager>().normalColor);
        

    }



}
