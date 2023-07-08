using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static TMPro.SpriteAssetUtilities.TexturePacker_JsonArray;

public class FacingCamera : MonoBehaviour
{

    // Update is called once per frame
    void Update()
    {
        Quaternion y_rotation = transform.rotation;
        y_rotation.w = Camera.main.transform.rotation.w;

        transform.rotation = y_rotation;
    }
}
