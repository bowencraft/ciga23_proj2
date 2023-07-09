using System.Collections;
using System.Collections.Generic;
using EFPController.Extras;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Level2 : MonoBehaviour
{


    [ColorUsage(true, true)]
    public Color doorHighlightColor;

    public Material teleportMaterial;

    public Material doorXianglianMaterial;

    // Start is called before the first frame update
    void Start()
    {
        doorHighlightColor = GameManager.Instance.StatusManager.GetComponent<StatusManager>().HDRList[6];


    }

    // Update is called once per frame
    void Update()
    {
        
    }


    public void openDoor()
    {
        //doorGate.SetActive(false);
        //portal.SetActive(true);
        //StartCoroutine(SceneChange());
    }


    private IEnumerator DoorChange()
    {


        // 延迟两秒
        yield return new WaitForSeconds(2f);

        // 在延迟结束后执行的行为
        //secondLight.SetActive(true);

        //oilLight.GetComponent<InteractiveObject>().Highlight();

    }

    private IEnumerator SceneChange()
    {


        // 延迟两秒
        yield return new WaitForSeconds(2f);
        SceneManager.LoadScene(1);


        // 在延迟结束后执行的行为
        //secondLight.SetActive(true);

        //oilLight.GetComponent<InteractiveObject>().Highlight();

    }

}
