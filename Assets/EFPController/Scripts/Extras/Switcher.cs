using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace EFPController.Extras
{

    public class Switcher : MonoBehaviour
    {

        public enum SwitcherType
        {
            OnOn,
            OnOff,
        }

        public SwitcherType type = SwitcherType.OnOn;
        public float cooldown = 1f;
        public float eventDelay = 0.25f;
        public bool disableOnSwitch = false;
        public bool onByDefault = false;

        public event UnityAction<bool> OnPress;

        [Header("Events")]
        public bool initialEvents = false;
        public UnityEvent EventOn;
        public UnityEvent EventOff;

        private bool _active = true;
        public bool active {
            get {
                return _active;
            }    
            set {
                _active = value;
                interactReceiver.enabled = _active;
            }    
        }

        private InteractReceiver interactReceiver;
        private bool currentState = false;
        private float nextAllowPressTime;

        private void Awake()
        {
            interactReceiver = GetComponent<InteractReceiver>();
        }

        private void Start()
        {
            currentState = true;
            if (type == SwitcherType.OnOff)
            {
                currentState = onByDefault;
                if (initialEvents) InvokeEvents();
            }
        }

        public void Interact()
        {
            if (!active || nextAllowPressTime > Time.time) return;
            nextAllowPressTime = Time.time + cooldown;
            if (disableOnSwitch) active = false;
            if (type == SwitcherType.OnOn)
            {
                SetState(true);
            } else if (type == SwitcherType.OnOff) {
                SetState(!currentState);
            }
        }

        public void SetState(bool value)
        {
            currentState = value;
            Invoke(nameof(InvokeEvents), eventDelay);
        }

        public void InvokeEvents()
        {
            OnPress?.Invoke(currentState);
            if (currentState)
            {
                EventOn?.Invoke();
            } else if (type == SwitcherType.OnOff) {
                EventOff?.Invoke();
            }
        }

    }

}