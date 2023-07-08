using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Level0 : MonoBehaviour
{

    public bool firstInteract = true;
    public GameObject initialLight;
    public GameObject largeLight;
    public GameObject secondLight;
    public GameObject lamp;
    public GameObject oilLight;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void igniteTorch()
    {
        firstInteract = false;
        initialLight.SetActive(false);
        largeLight.SetActive(true);
        StartCoroutine(turnOnSecondLight());
        lamp.SetActive(true);
    }

    private IEnumerator turnOnSecondLight()
    {
        // 延迟两秒
        yield return new WaitForSeconds(1f);

        // 在延迟结束后执行的行为
        secondLight.SetActive(true);

        oilLight.GetComponent<InteractiveObject>().Highlight();

    }
}
