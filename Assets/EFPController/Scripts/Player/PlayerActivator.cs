using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace EFPController
{

    [DefaultExecutionOrder(-100)]
    public class PlayerActivator : MonoBehaviour
    {

        public bool activeOnAwake = true;

        private Player player;

        private void Awake()
        {
            player = GetComponent<Player>();
            if (!activeOnAwake) Deactivate();
        }

        public void Activate() => SetActivate(true);
        public void Deactivate() => SetActivate(false);

        public void SetActivate(bool value)
        {
            player.weaponRoot.SetActive(value);
            player.cameraRoot.SetActive(value);
            player.controller.enabled = value;
            player.cameraBobAnims.enabled = value;
            player.footsteps.enabled = value;
            player.enabled = value;
        }

    }

}