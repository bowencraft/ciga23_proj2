using UnityEngine;
using System.Collections;
using EFPController.Utils;

namespace EFPController
{

	public static class CameraRootAnimNames
	{
		public const string Idle = "Idle";
		public const string Walk = "Walk";
		public const string Sprint = "Sprint";
		public const string Climbing = "Climbing";
	}

	[DefaultExecutionOrder(-10)]
	public class CameraRootAnimations : MonoBehaviour
	{
	
		// DONT RENAME! USES IN ANIMATIONS.
		public Vector3 camPosAnim;
		public Vector3 camAngleAnim;
		public Vector3 weapPosAnim;
		public Vector3 weapAngleAnim;

		public float revertSpeed = 1f;

		public Animator animator { get; private set; }

		private void Awake()
		{
			animator = GetComponent<Animator>();
		}

		private void Update()
		{
			if (animator.speed.IsZero())
			{
				animator.enabled = false;
				camPosAnim = Vector3.Lerp(camPosAnim, Vector3.zero, revertSpeed * Time.deltaTime);
				camAngleAnim = Vector3.Lerp(camAngleAnim, Vector3.zero, revertSpeed * Time.deltaTime);
				weapPosAnim = Vector3.Lerp(weapPosAnim, Vector3.zero, revertSpeed * Time.deltaTime);
				weapAngleAnim = Vector3.Lerp(weapAngleAnim, Vector3.zero, revertSpeed * Time.deltaTime);
			} else {
				animator.enabled = true;
			}
		}

	}

}