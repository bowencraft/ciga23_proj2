using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Level0 : MonoBehaviour
{

    public bool firstInteract = true;

    public GameObject Torch;

    public GameObject initialLight;
    public GameObject largeLight;

    public GameObject igniteCubes;

    public GameObject secondLight;
    public GameObject lamp;
    public GameObject oilLight;

    public GameObject OilLightCubes;
    public GameObject MagicDoor;

    public GameObject DoorGate;
    public GameObject Portal;
    // Start is called before the first frame update
    void Start()
    {
        initialLight.SetActive(true);
        largeLight.SetActive(false);
        igniteCubes.SetActive(false);
        secondLight.SetActive(false);
        lamp.SetActive(false);
        OilLightCubes.SetActive(false);
        MagicDoor.SetActive(false);
        Portal.SetActive(false);

        StartCoroutine(enableTorch());
    }

    // Update is called once per frame
    void Update()
    {

    }

    private IEnumerator enableTorch()
    {
        // 延迟两秒
        yield return new WaitForSeconds(2f);

        // 在延迟结束后执行的行为
        Torch.GetComponent<InteractiveObject>().isInHighLight = true;

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
        largeLight.SetActive(false);

    }

    public void interactOfDoor()
    {
        DoorGate.SetActive(false);
        Portal.SetActive(true);
        StartCoroutine(OpenMagicDoor());
        

    }
    private IEnumerator OpenMagicDoor()
    {
        // 延迟两秒
        yield return new WaitForSeconds(2f);
        SceneManager.LoadScene(1);
        // 在延迟结束后执行的行为

    }

}
