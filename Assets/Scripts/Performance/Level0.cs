using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Level0 : MonoBehaviour
{

    public bool firstInteract = true;
    public GameObject initialLight;
    public GameObject largeLight;

    public GameObject igniteCubes;

    public GameObject secondLight;
    public GameObject lamp;
    public GameObject oilLight;

    public GameObject OilLightCubes;
    public GameObject MagicDoor;
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
        igniteCubes.SetActive(true);
        StartCoroutine(turnOnSecondLight());

        lamp.SetActive(true);
    }

    private IEnumerator turnOnSecondLight()
    {
        // 延迟两秒
        yield return new WaitForSeconds(2f);

        // 在延迟结束后执行的行为
        secondLight.SetActive(true);

        oilLight.GetComponent<InteractiveObject>().Highlight();

    }

    public void igniteOilLight()
    {
        igniteCubes.SetActive(false);
        OilLightCubes.SetActive(true);
        StartCoroutine(shownMagicDoor());

    }
    private IEnumerator shownMagicDoor()
    {
        // 延迟两秒
        yield return new WaitForSeconds(2f);

        // 在延迟结束后执行的行为
        MagicDoor.SetActive(true);


    }
}
