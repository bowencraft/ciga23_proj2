using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace EFPController.Extras
{

    public class InteractReceiver : MonoBehaviour
    {

        private const float interactDelay = 0.1f;

        public string funcInteractName = "Interact";
        public string funcHoverName = "Hover";
        public bool hold = false;
        public bool self = true;
        public List<GameObject> interactiveObjects = new List<GameObject>();

        private float nextInteractTime;

        void Start()
        {
            gameObject.tag = Game.Tags.Interactable;
        }

        public void InteractRequest()
        {
            if (!enabled || nextInteractTime > Time.time) return;
            nextInteractTime = Time.time + interactDelay;
            if (self)
            {
                gameObject.SendMessage(funcInteractName, SendMessageOptions.DontRequireReceiver);
            } else {
                foreach (GameObject go in interactiveObjects)
                {
                    if (go != null)
                    {
                        go.SendMessage(funcInteractName, SendMessageOptions.DontRequireReceiver);
                    }
                }
            }
        }

        public void HoverRequest()
        {
            if (!enabled) return;
            if (self)
            {
                gameObject.SendMessage(funcHoverName, SendMessageOptions.DontRequireReceiver);
            } else {
                foreach (GameObject go in interactiveObjects)
                {
                    if (go != null)
                    {
                        go.SendMessage(funcHoverName, SendMessageOptions.DontRequireReceiver);
                    }
                }
            }
        }

    }

}