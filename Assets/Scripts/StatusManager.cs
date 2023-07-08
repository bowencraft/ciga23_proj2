using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class StatusManager : MonoBehaviour
{

    public List<GameObject> InteractiveObjects;

    public Color normalColor;
    public Color highlightColor;

    public int currentStatus = 0;

    public Animator iconSwitch;
    public Sprite[] SpriteList = new Sprite[15];

    [SerializeField]
    private GameObject iconObject;


    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        Image image = iconObject.GetComponent<Image>();
        image.sprite = SpriteList[currentStatus];
        //if (SpriteList[currentStatus] != null)
        //else
        //    textMeshPro.fontMaterial.mainTexture = null;

    }

    void updateStatus(int status)
    {
        currentStatus = status;

        foreach (GameObject interactiveObject in InteractiveObjects)
        {
            InteractiveObject io = interactiveObject.GetComponent<InteractiveObject>();
            if (io.mainFeeling == status)
            {
                io.unlocked = true;
                io.Highlight();
            }
            else 
            if (io.minorFeeling == status)
            {
                io.unlocked = true;
                io.minorFeeling = io.mainFeeling;
                io.mainFeeling = status;
                io.Highlight();
            } else
            {
                
            }
        }
    }
}
