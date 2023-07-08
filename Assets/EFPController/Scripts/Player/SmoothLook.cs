using UnityEngine;
using EFPController.Utils;

namespace EFPController
{

	[DefaultExecutionOrder(-11)]
	public class SmoothLook : MonoBehaviour
	{

		[Tooltip("Mouse look sensitivity/camera move speed.")]
		public float sensitivityMouse = 0.2f;
		[Tooltip("Gamepad sensitivity/camera move speed.")]
		public float sensitivityGamepad = 2f;
		public AnimationCurve sensitivityGamepadCurve = new AnimationCurve() { keys = new Keyframe[] { new Keyframe(0f, 0f), new Keyframe(1f, 1f) } };
		public float verticalSensitivityMultiplier = 1f;
		[Tooltip("Minumum pitch of camera for mouselook.")]
		public float minimumYAngle = -89f;
		[Tooltip("Maximum pitch of camera for mouselook.")]
		public float maximumYAngle = 89f;
		public float maxXAngle { get; set; } = 360f;
		[Tooltip("Smooth speed of camera angles for mouse look.")]
		public float smoothSpeedMouse = 0.75f;
		[Tooltip("Smooth speed of camera angles for controller.")]
		public float smoothSpeedGamepad = 0.6f;

		/////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

		[HideInInspector] public Vector2 lookInput;
		public float sensitivityAmt { get; set; } = 4f; // actual sensitivity modified by IronSights Script
		public float rotationX { get; set; } = 0f;
		public float rotationY { get; set; } = 0f;
		public float rotationZ { get; set; } = 0f;
		public float inputY { get; set; } = 0f;
		public float playerMovedTime { get; set; }
		public Quaternion originalRotation { get; set; }
		public float recoilX { get; set; } // non recovering recoil amount managed by WeaponKick function of Item Behavior
		public float recoilY { get; set; } // non recovering recoil amount managed by WeaponKick function of Item Behavior
		public float currentSensitivity { get; set; }
		public float currentSmoothSpeed { get; set; }
		public bool isGamepad { get; private set; }
		public bool smooth { get; set; } = true;
		public float maxXAngleDef { get; private set; }

		/////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

		private Player player;
		private InputManager inputManager;
		private float maximumX = 360f;
		private Quaternion xQuaternion;
		private Quaternion yQuaternion;
		private Quaternion zQuaternion;
		private float minimumYAngleDef;
		private float maximumYAngleDef;
		private float sensitivityMouseDef;
		private float sensitivityGamepadDef;
		private float smoothSpeedMouseDef;
		private float smoothSpeedGamepadDef;
		private float sensitivityAmtX;
		private float sensitivityAmtY;
		private bool isGamepadPrev;
		private bool inputDeviceChange;

		private void Awake()
		{
			minimumYAngleDef = minimumYAngle;
			maximumYAngleDef = maximumYAngle;
			maxXAngleDef = maxXAngle;
			sensitivityMouseDef = sensitivityMouse;
			sensitivityGamepadDef = sensitivityGamepad;
			smoothSpeedMouseDef = smoothSpeedMouse;
			smoothSpeedGamepadDef = smoothSpeedGamepad;
			inputManager = InputManager.instance;
			player = GetComponentInParent<Player>();
			player.GetComponent<Rigidbody>().freezeRotation = true;
		}

		public void RestoreDefaults()
		{
			minimumYAngle = minimumYAngleDef;
			maximumYAngle = maximumYAngleDef;
			maxXAngle = maxXAngleDef;
			sensitivityGamepad = sensitivityGamepadDef;
			sensitivityMouse = sensitivityMouseDef;
			sensitivityAmt = InputManager.isGamepad ? sensitivityGamepad : sensitivityMouse;
			smoothSpeedMouse = smoothSpeedMouseDef;
			smoothSpeedGamepad = smoothSpeedGamepadDef;
			rotationX = transform.eulerAngles.x;
			rotationY = transform.eulerAngles.y;
			recoilY = 0f;
			recoilX = 0f;
			inputY = 0f;
			playerMovedTime = 0f;
			originalRotation = Quaternion.Euler(transform.eulerAngles.x, transform.eulerAngles.y, 0f);
		}

		void Start()
		{
			transform.SetParent(null);
		
			// sync the initial rotation of the main camera to the y rotation set in editor
			originalRotation = Quaternion.Euler(transform.eulerAngles.x, transform.eulerAngles.y, 0f);
		
			sensitivityAmt = InputManager.isGamepad ? sensitivityGamepad : sensitivityMouse; // initialize sensitivity amount from var set by player
		}

		void LateUpdate()
		{
			if (!Player.canControl || !Player.inited || Time.smoothDeltaTime <= 0f) return;

			lookInput = inputManager.GetLookInput();

			isGamepad = inputManager.LookIsGamepad();
			inputDeviceChange = isGamepad != isGamepadPrev;

			currentSensitivity = isGamepad ? sensitivityGamepad : sensitivityMouse;
			currentSmoothSpeed = isGamepad ? smoothSpeedGamepad : smoothSpeedMouse;

			if (isGamepad)
			{
				lookInput.x *= Mathf.Clamp(sensitivityGamepadCurve.Evaluate(Mathf.Abs(lookInput.x)), 1f, 10f);
				lookInput.y *= Mathf.Clamp(sensitivityGamepadCurve.Evaluate(Mathf.Abs(lookInput.y)), 1f, 10f);
			}
		
			sensitivityAmtX = isGamepad ? sensitivityAmt * Time.deltaTime : sensitivityAmt;
			sensitivityAmtY = isGamepad ? sensitivityAmt * Time.deltaTime : sensitivityAmt;

			lookInput.x *= sensitivityAmtX;
			lookInput.y *= sensitivityAmtY * verticalSensitivityMultiplier;

			rotationX += lookInput.x; // lower sensitivity at slower time settings
			rotationY += lookInput.y;

			// reset vertical recoilY value if it would exceed maximumY amount 
			if (maximumYAngle - lookInput.y < recoilY)
			{
				rotationY += recoilY;
				recoilY = 0f;
			}

			// reset horizontal recoilX value if it would exceed maximumX amount 
			if (maximumX - lookInput.x < recoilX)
			{
				rotationX += recoilX;
				recoilX = 0f;
			}

			rotationX = GameUtils.ClampAngle(rotationX, -maxXAngle, maxXAngle);
			rotationY = GameUtils.ClampAngle(rotationY, minimumYAngle - recoilY, maximumYAngle - recoilY);
			rotationZ = -player.controller.leanPos * player.controller.rotationLeanAmt;

			inputY = rotationY + recoilY; // set public inputY value for use in other scripts
			xQuaternion = Quaternion.AngleAxis(rotationX + recoilX, Vector3.up);
			yQuaternion = Quaternion.AngleAxis(rotationY + recoilY, -Vector3.right);
			zQuaternion = Quaternion.AngleAxis(rotationZ, Vector3.forward);

			Quaternion newRotation = originalRotation * xQuaternion * yQuaternion * zQuaternion;

			if (inputDeviceChange) newRotation = transform.rotation;

			if (smooth && playerMovedTime + 0.1f < Time.time && !inputDeviceChange)
			{
				// smoothing camera rotation
				transform.rotation = Quaternion.Lerp(transform.rotation, newRotation, currentSmoothSpeed * Time.smoothDeltaTime * 60f); 
			} else {
				// snap camera instantly to angles with no smoothing
				transform.rotation = newRotation; 
			}

			isGamepadPrev = isGamepad;
		}

	}

}