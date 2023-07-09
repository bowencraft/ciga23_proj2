using System.Collections;
using System.Collections.Generic;
using EFPController.Extras;
using UnityEngine;

public class InteractiveObject : MonoBehaviour
{
    public bool isInHighLight = false;
    public bool unlocked = false;

    public int mainFeeling;
    public int minorFeeling;

    public GameObject particleObject;

    private Color targetNormalColor;
    private Color targetHDRColor;
    private Material renderer;

    public float duration = 2f;
    private float elapsedTime = 0f;

    private void Start()
    {
        // 获取 Renderer 组件
        renderer = GetComponent<Renderer>().material;

        targetNormalColor = GameManager.Instance.StatusManager.GetComponent<StatusManager>().normalColor;
        targetHDRColor = GameManager.Instance.StatusManager.GetComponent<StatusManager>().normalHDRColor;

        renderer.SetColor("_AlbedoColor", GameManager.Instance.StatusManager.GetComponent<StatusManager>().normalColor);
        renderer.SetColor("_EmissionColor", GameManager.Instance.StatusManager.GetComponent<StatusManager>().normalHDRColor);

        if (!isInHighLight)
        {
            particleObject.SetActive(false);
        }
        // 调整材质属性
        //Debug.Log(renderer);
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

        if (unlocked)
        {

            if (isInHighLight)
            {

                this.GetComponent<InteractReceiver>().enabled = true;

                float yRotation = Camera.main.transform.eulerAngles.y;
                transform.eulerAngles = new Vector3(transform.eulerAngles.x, yRotation, transform.eulerAngles.z);
            } else
            {

                this.GetComponent<InteractReceiver>().enabled = false;
            }
        }
        else
        {
            this.GetComponent<InteractReceiver>().enabled = false;


            targetNormalColor = GameManager.Instance.StatusManager.GetComponent<StatusManager>().lockedColor;
            renderer.SetColor("_AlbedoColor", GameManager.Instance.StatusManager.GetComponent<StatusManager>().lockedColor);

        }

        if (elapsedTime < duration)
        {
            // 使用插值函数平滑改变颜色值
            Color currentColor = renderer.GetColor("_AlbedoColor");
            renderer.SetColor("_AlbedoColor", Color.Lerp(currentColor, targetNormalColor, elapsedTime / duration));

            currentColor = renderer.GetColor("_EmissionColor");
            renderer.SetColor("_EmissionColor", Color.Lerp(currentColor, targetHDRColor, elapsedTime / duration));

            // 增加已经过去的时间
            elapsedTime += Time.deltaTime;
        }
    }

    public void Interact()
    {

        if (isInHighLight)
        {
            //Debug.Log("Interacted");
            int tempFeeling = mainFeeling;
            mainFeeling = minorFeeling;
            minorFeeling = tempFeeling;

            GetComponentInParent<Animator>().SetTrigger("touchToMove");

            //GameManager.Instance.StatusManager.GetComponent<StatusManager>().iconSwitch.SetTrigger("isSwitch");
            GameManager.Instance.StatusManager.GetComponent<StatusManager>().currentStatus = mainFeeling;

            particleObject.SetActive(true);


        }

        
    }

    public void Highlight()
    {
        isInHighLight = true;
        unlocked = true;
        //Debug.Log("Highlighted" + this.transform.name);

        //this.GetComponent<InteractReceiver>().enabled = true;

        targetNormalColor = GameManager.Instance.StatusManager.GetComponent<StatusManager>().highlightColor;
        targetHDRColor = GameManager.Instance.StatusManager.GetComponent<StatusManager>().HDRList[mainFeeling];

        elapsedTime = 0f;

        //renderer.SetColor("_AlbedoColor", GameManager.Instance.StatusManager.GetComponent<StatusManager>().highlightColor);
        //renderer.SetColor("_EmissionColor", GameManager.Instance.StatusManager.GetComponent<StatusManager>().HDRList[mainFeeling]);

    }


    public void deHighlight()
    {
        isInHighLight = false;
        //this.GetComponent<InteractReceiver>().enabled = false;

        targetNormalColor = GameManager.Instance.StatusManager.GetComponent<StatusManager>().normalColor;
        targetHDRColor = GameManager.Instance.StatusManager.GetComponent<StatusManager>().normalHDRColor;

        elapsedTime = 0f;

        particleObject.SetActive(false);

        //renderer.SetColor("_AlbedoColor", GameManager.Instance.StatusManager.GetComponent<StatusManager>().normalColor);
        //renderer.SetColor("_EmissionColor", GameManager.Instance.StatusManager.GetComponent<StatusManager>().normalHDRColor);


    }



}
