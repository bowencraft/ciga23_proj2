using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class SubtitleManager : MonoBehaviour
{
    public GameObject subtitle;
    public string content;

    public TextMeshProUGUI textMeshProUI;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        textMeshProUI = subtitle.GetComponent<TextMeshProUGUI>();
        textMeshProUI.text = content;
        
    }
}
