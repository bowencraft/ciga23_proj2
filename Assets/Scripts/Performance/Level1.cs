using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Level1 : MonoBehaviour
{

    public GameObject doorGate;

    [ColorUsage(true, true)]
    public Color doorHighlightColor;

    public Material teleportMaterial;


    // Start is called before the first frame update
    void Start()
    {
        doorHighlightColor = GameManager.Instance.StatusManager.GetComponent<StatusManager>().HDRList[6];
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void interactxianglian()
    {
        Debug.Log("xianglian being interacted");
        doorGate.GetComponent<Renderer>().material.SetColor("_EmissionColor", doorHighlightColor);

        //StartCoroutine(doorOpen());
    }

    public void openDoor()
    {
        doorGate.GetComponent<Renderer>().material = teleportMaterial;

        StartCoroutine(SceneChange());
    }


    private IEnumerator SceneChange()
    {


        // 延迟两秒
        yield return new WaitForSeconds(2f);
        SceneManager.LoadScene(2);


        // 在延迟结束后执行的行为
        //secondLight.SetActive(true);

        //oilLight.GetComponent<InteractiveObject>().Highlight();

    }


}
