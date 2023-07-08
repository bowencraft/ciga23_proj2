using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

namespace EFPController
{

    public enum KeyState
    {
        Down,
        Held,
        Released
    }

    [DefaultExecutionOrder(-997)]
    public class InputManager : MonoBehaviour
    {

        public static class InputActionNames
        {
            public const string Movement = "Movement";
            public const string MouseLook = "Mouse Look";
            public const string ControllerLook = "Controller Look";
            public const string Sprint = "Sprint";
            public const string Jump = "Jump";
            public const string Crouch = "Crouch";
            public const string Interact = "Interact";
            public const string Reload = "Reload";
            public const string Menu = "Menu";
            public const string LeanLeft = "Lean Left";
            public const string LeanRight = "Lean Right";
            public const string Slow = "Slow";
            public const string Dash = "Dash";
            public const string Drop = "Drop";
            public const string Fire = "Fire";
            public const string FireAlternative = "Fire Alternative";
            public const string Zoom = "Zoom";
            public const string Holster = "Holster";
            public const string ItemChange = "Item Change";
            public const string Item1Select = "Item 1 Select";
            public const string Item2Select = "Item 2 Select";
            public const string Item3Select = "Item 3 Select";
            public const string Item4Select = "Item 4 Select";
            public const string Item5Select = "Item 5 Select";
            public const string Item6Select = "Item 6 Select";
            public const string Item7Select = "Item 7 Select";
            public const string Item8Select = "Item 8 Select";
            public const string Item9Select = "Item 9 Select";
            public const string Submit = "Submit";
            public const string Cancel = "Cancel";
            public const string InvestigateRotation = "Investigate Rotation";
            public const string InvestigateZoom = "Investigate Zoom";
        }

        public static Dictionary<KeyCode, string> keyNames = new Dictionary<KeyCode, string>()
        {
            { KeyCode.Mouse0, "<sprite index=0>" },
            { KeyCode.Mouse1, "<sprite index=1>" },
            { KeyCode.Mouse2, "<sprite index=2>" },
            { KeyCode.LeftControl, "Left Control" },
            { KeyCode.LeftShift, "Left Shift" },
            { KeyCode.LeftAlt, "Left Alt" },
            { KeyCode.RightControl, "Right Control" },
            { KeyCode.RightShift, "Right Shift" },
            { KeyCode.RightAlt, "Right Alt" },
            { KeyCode.LeftArrow, "Left Arrow" },
            { KeyCode.RightArrow, "Right Arrow" },
            { KeyCode.UpArrow, "Up Arrow" },
            { KeyCode.DownArrow, "Down Arrow" },
        };

        public static InputManager instance;

        public static bool isGamepad = false;

        public InputActionAsset inputActions;

        [Tooltip("Reverse vertical input for look.")]
        public bool invertVerticalLook;

        public InputAction mouseLookInputAction { get; private set; }
        public InputAction controllerLookInputAction { get; private set; }
        public InputAction movementInputAction { get; private set; }
        public InputAction sprintInputAction { get; private set; }
        public InputAction dashInputAction { get; private set; }
        public InputAction jumpInputAction { get; private set; }
        public InputAction crouchInputAction { get; private set; }
        public InputAction interactInputAction { get; private set; }
        public InputAction reloadInputAction { get; private set; }
        public InputAction menuInputAction { get; private set; }
        public InputAction leanLeftInputAction { get; private set; }
        public InputAction leanRightInputAction { get; private set; }
        public InputAction slowInputAction { get; private set; }
        public InputAction dropInputAction { get; private set; }
        public InputAction fireInputAction { get; private set; }
        public InputAction fireAlternativeInputAction { get; private set; }
        public InputAction zoomInputAction { get; private set; }
        public InputAction holsterInputAction { get; private set; }
        public InputAction itemChangeAction { get; private set; }

        public InputAction item1Select { get; private set; }
        public InputAction item2Select { get; private set; }
        public InputAction item3Select { get; private set; }
        public InputAction item4Select { get; private set; }
        public InputAction item5Select { get; private set; }
        public InputAction item6Select { get; private set; }
        public InputAction item7Select { get; private set; }
        public InputAction item8Select { get; private set; }
        public InputAction item9Select { get; private set; }

        public InputAction submitAction { get; private set; }
        public InputAction cancelAction { get; private set; }

        public InputAction investigateRotationAction { get; private set; }
        public InputAction investigateZoomAction { get; private set; }

        public float deadZoneMin { get; private set; }
        public float deadZoneMax { get; private set; }

        private void Awake()
        {
            instance = this;
        }

        void Start()
        {
            InputSystem.onEvent += (eventPtr, device) =>
            {
                if (device != null) isGamepad = device is Gamepad;
            };

            movementInputAction = inputActions.FindAction(InputActionNames.Movement);
            mouseLookInputAction = inputActions.FindAction(InputActionNames.MouseLook);
            controllerLookInputAction = inputActions.FindAction(InputActionNames.ControllerLook);
            movementInputAction = inputActions.FindAction(InputActionNames.Movement);
            sprintInputAction = inputActions.FindAction(InputActionNames.Sprint);
            dashInputAction = inputActions.FindAction(InputActionNames.Dash);
            jumpInputAction = inputActions.FindAction(InputActionNames.Jump);
            crouchInputAction = inputActions.FindAction(InputActionNames.Crouch);
            interactInputAction = inputActions.FindAction(InputActionNames.Interact);
            reloadInputAction = inputActions.FindAction(InputActionNames.Reload);
            menuInputAction = inputActions.FindAction(InputActionNames.Menu);
            leanLeftInputAction = inputActions.FindAction(InputActionNames.LeanLeft);
            leanRightInputAction = inputActions.FindAction(InputActionNames.LeanRight);
            slowInputAction = inputActions.FindAction(InputActionNames.Slow);
            dropInputAction = inputActions.FindAction(InputActionNames.Drop);
            fireInputAction = inputActions.FindAction(InputActionNames.Fire);
            fireAlternativeInputAction = inputActions.FindAction(InputActionNames.FireAlternative);
            zoomInputAction = inputActions.FindAction(InputActionNames.Zoom);
            holsterInputAction = inputActions.FindAction(InputActionNames.Holster);
            itemChangeAction = inputActions.FindAction(InputActionNames.ItemChange);

            item1Select = inputActions.FindAction(InputActionNames.Item1Select);
            item2Select = inputActions.FindAction(InputActionNames.Item2Select);
            item3Select = inputActions.FindAction(InputActionNames.Item3Select);
            item4Select = inputActions.FindAction(InputActionNames.Item4Select);
            item5Select = inputActions.FindAction(InputActionNames.Item5Select);
            item6Select = inputActions.FindAction(InputActionNames.Item6Select);
            item7Select = inputActions.FindAction(InputActionNames.Item7Select);
            item8Select = inputActions.FindAction(InputActionNames.Item8Select);
            item9Select = inputActions.FindAction(InputActionNames.Item9Select);

            submitAction = inputActions.FindAction(InputActionNames.Submit);
            cancelAction = inputActions.FindAction(InputActionNames.Cancel);

            investigateRotationAction = inputActions.FindAction(InputActionNames.InvestigateRotation);
            investigateZoomAction = inputActions.FindAction(InputActionNames.InvestigateZoom);

            SetActiveAllControls(true);
        }

        public void SetActiveAllControls(bool value)
        {
            if (value)
            {
                mouseLookInputAction.Enable();
                controllerLookInputAction.Enable();
                movementInputAction.Enable();
                sprintInputAction.Enable();
                dashInputAction.Enable();
                jumpInputAction.Enable();
                crouchInputAction.Enable();
                interactInputAction.Enable();
                reloadInputAction.Enable();
                menuInputAction.Enable();
                leanLeftInputAction.Enable();
                leanRightInputAction.Enable();
                slowInputAction.Enable();
                dropInputAction.Enable();
                fireInputAction.Enable();
                fireAlternativeInputAction.Enable();
                zoomInputAction.Enable();
                holsterInputAction.Enable();
                itemChangeAction.Enable();
                item1Select.Enable();
                item2Select.Enable();
                item3Select.Enable();
                item4Select.Enable();
                item5Select.Enable();
                item6Select.Enable();
                item7Select.Enable();
                item8Select.Enable();
                item9Select.Enable();
                submitAction.Enable();
                cancelAction.Enable();
                investigateRotationAction.Enable();
                investigateZoomAction.Enable();
            } else {
                mouseLookInputAction.Disable();
                controllerLookInputAction.Disable();
                movementInputAction.Disable();
                sprintInputAction.Disable();
                dashInputAction.Disable();
                jumpInputAction.Disable();
                crouchInputAction.Disable();
                interactInputAction.Disable();
                reloadInputAction.Disable();
                menuInputAction.Disable();
                leanLeftInputAction.Disable();
                leanRightInputAction.Disable();
                slowInputAction.Disable();
                dropInputAction.Disable();
                fireInputAction.Disable();
                fireAlternativeInputAction.Disable();
                zoomInputAction.Disable();
                holsterInputAction.Disable();
                itemChangeAction.Disable();
                item1Select.Disable();
                item2Select.Disable();
                item3Select.Disable();
                item4Select.Disable();
                item5Select.Disable();
                item6Select.Disable();
                item7Select.Disable();
                item8Select.Disable();
                item9Select.Disable();
                submitAction.Disable();
                cancelAction.Disable();
                investigateRotationAction.Disable();
                investigateZoomAction.Disable();
            }
        }

        protected virtual Vector2 GetMouseLookInput()
        {
            if (mouseLookInputAction != null) return mouseLookInputAction.ReadValue<Vector2>();
            return Vector2.zero;
        }

        protected virtual Vector2 GetControllerLookInput()
        {
            if (controllerLookInputAction != null) return controllerLookInputAction.ReadValue<Vector2>();
            return Vector2.zero;
        }

        public virtual Vector2 GetLookInput(bool useInvert = true)
        {
            Vector2 value = GetMouseLookInput();
            if (value.sqrMagnitude == 0f)
            {
                value = GetControllerLookInput();
            }
            if (invertVerticalLook && useInvert)
		    {
                value.y = -value.y;
            }
            return value;
        }

        public virtual bool LookIsGamepad()
        {
            Vector2 value = GetControllerLookInput();
            if (value.sqrMagnitude > 0f)
            {
                return true;
            }
            return false;
        }

        public Vector2 GetMovementInput()
        {
            if (movementInputAction != null) return movementInputAction.ReadValue<Vector2>();
            return Vector2.zero;
        }

        public float GetItemChangeInput()
        {
            if (itemChangeAction != null) return itemChangeAction.ReadValue<float>();
            return 0f;
        }

        public int GetSelectItemInput()
        {
            if (item1Select.WasPerformedThisFrame())
                return 1;
            else if (item2Select.WasPerformedThisFrame())
                return 2;
            else if (item3Select.WasPerformedThisFrame())
                return 3;
            else if (item4Select.WasPerformedThisFrame())
                return 4;
            else if (item5Select.WasPerformedThisFrame())
                return 5;
            else if (item6Select.WasPerformedThisFrame())
                return 6;
            else if (item7Select.WasPerformedThisFrame())
                return 7;
            else if (item8Select.WasPerformedThisFrame())
                return 8;
            else if (item9Select.WasPerformedThisFrame())
                return 9;
            else
                return -1;
        }

        public virtual Vector2 GetInvestigateRotationInput(bool useInvert = true)
        {
            if (investigateRotationAction != null)
            {
                Vector2 value = investigateRotationAction.ReadValue<Vector2>();
                if (invertVerticalLook && useInvert)
                {
                    value.y = -value.y;
                }
                return value;
            }
            return Vector2.zero;
        }

        public virtual float GetInvestigateZoomInput()
        {
            if (investigateZoomAction != null) return investigateZoomAction.ReadValue<float>();
            return 0f;
        }

    }

}