using TowerDefense.UI;
using UnityEngine;
using UnityInput = UnityEngine.Input;

namespace Core.Input
{
	/// <summary>
	/// Control scheme for XBox360 gamepad, which performs CameraRig motion
	/// </summary>
	public class XBox360Input : CameraInputScheme
	{
		public enum XBoxButton
		{
			A,
			B,
			X,
			Y,
			Start
		}

		[SerializeField] protected float thresholdForStick = 0.05f;
		[SerializeField] protected PauseMenu pauseMenu = null;
		
		/// <summary>
		/// Gets whether the scheme should be activated or not
		/// </summary>
		public override bool shouldActivate
		{
			get
			{
				if (InputController.instanceExists == false || InputController.instance.isAnyControllerConnected == false)
				{
					return false;
				}
				bool anyJoystickAxis = UnityInput.GetAxis("JoystickHorizontal") != 0 ||
					UnityInput.GetAxis("JoystickVertical") != 0 ||
					UnityInput.GetAxis("JoystickHorizontal2") != 0 ||
					UnityInput.GetAxis("JoystickVertical2") != 0;
				return InputController.instance.isAnyJoystickButtonPressed || anyJoystickAxis;
			}
		}

		/// <summary>
		/// This is the default scheme on desktop devices
		/// </summary>
		public override bool isDefault
		{
			get
			{
#if UNITY_STANDALONE || UNITY_EDITOR
				return true;
#else
				return false;
#endif
			}
		}

		/// <summary>
		/// Register input events
		/// </summary>
		protected virtual void OnEnable()
		{
			if (!InputController.instanceExists)
			{
				Debug.LogError("[UI] XBox UI requires InputController");
				return;
			}

			InputController controller = InputController.instance;
		}

		/// <summary>
		/// Deregister input events
		/// </summary>
		protected virtual void OnDisable()
		{
			if (!InputController.instanceExists)
			{
				return;
			}

			InputController controller = InputController.instance;
		}
		
		/// <summary>
		/// Handle camera panning behaviour
		/// </summary>
		protected virtual void Update()
		{
			//if (cameraRig != null)
			//{
			//	DoScreenEdgePan();
			//	DoKeyboardPan();
			//	DecayZoom();
			//}
		}
	}
}