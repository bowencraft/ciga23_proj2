using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class StatusManager : MonoBehaviour
{

    public List<GameObject> InteractiveObjects;

    public Color normalColor;
    public Color highlightColor;
    public Color lockedColor;

    [ColorUsage(true, true)]
    public Color normalHDRColor;

    [ColorUsage(true, true)]
    public Color[] HDRList = new Color[15];


    public int currentStatus = 0;
    public int lastframeStatus = 0;

    public Animator iconSwitch;
    public Sprite[] SpriteList = new Sprite[15];

    [SerializeField]
    private GameObject iconObject;


    // Start is called before the first frame update
    void Start()
    {

        StartCoroutine(InitializeStatus());
    }

    private IEnumerator InitializeStatus()
    {

        yield return new WaitForSeconds(1f);
        updateStatus(currentStatus);
    }
    // Update is called once per frame
    void Update()
    {
        Image image = iconObject.GetComponent<Image>();
        image.sprite = SpriteList[currentStatus];
        //if (SpriteList[currentStatus] != null)
        //else
        //    textMeshPro.fontMaterial.mainTexture = null;

        if (lastframeStatus != currentStatus)
        {
            updateStatus(currentStatus);
        }

        lastframeStatus = currentStatus;

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            SceneManager.LoadScene(0);
        }

    }

    void updateStatus(int status)
    {
        currentStatus = status;
        iconSwitch.SetTrigger("isSwitch");

        foreach (GameObject interactiveObject in InteractiveObjects)
        {
            InteractiveObject io = interactiveObject.GetComponent<InteractiveObject>();
            if (io.mainFeeling == status)
            {
                io.Highlight();
            }
            else 
            if (io.minorFeeling == status)
            {
                io.minorFeeling = io.mainFeeling;
                io.mainFeeling = status;
                io.Highlight();
            } else
            {
                io.deHighlight();

                io.particleObject.SetActive(false);
            }
        }
    }
}
