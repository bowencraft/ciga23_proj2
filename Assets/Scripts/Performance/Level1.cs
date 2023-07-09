using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Level1 : MonoBehaviour
{

    public GameObject initialLight;
    public GameObject largeLight;

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
        //StartCoroutine(shownMagicDoor());
    }

    private IEnumerator turnOnSecondLight()
    {
        // 延迟两秒
        yield return new WaitForSeconds(2f);

        // 在延迟结束后执行的行为
        //secondLight.SetActive(true);

        //oilLight.GetComponent<InteractiveObject>().Highlight();

    }


}
