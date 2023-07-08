using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class StatusManager : MonoBehaviour
{

    public int currentStatus = 0;

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
}
