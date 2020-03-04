using Core.Input;
using TowerDefense.UI;
using TowerDefense.UI.HUD;
using UnityEngine;
using static Core.Input.InputController;
using State = TowerDefense.UI.HUD.GameUI.State;
using UnityInput = UnityEngine.Input;

namespace TowerDefense.Input
{
	[RequireComponent(typeof(GameUI))]
	public class TowerDefenseXboxInput : XBox360Input
	{
		[SerializeField] private PlacementManager placementManager = null;

		/// <summary>
		/// Is using Xbox controller to select tower placement area?
		/// </summary>
		bool isSearchingForNextArea;

		public override bool isDefault
		{
			get
			{
				return true;
			}
		}

		/// <summary>
		/// Cached eference to gameUI
		/// </summary>
		GameUI m_GameUI;

		/// <summary>
		/// Register input events
		/// </summary>
		protected override void OnEnable()
		{
			base.OnEnable();

			m_GameUI = GetComponent<GameUI>();

			if (InputController.instanceExists)
			{
				InputController.instance.xBoxButtonPressed += UpdateXBoxButtons;
			}
		}

		/// <summary>
		/// Deregister input events
		/// </summary>
		protected override void OnDisable()
		{
			if (!InputController.instanceExists)
			{
				return;
			}

			InputController.instance.xBoxButtonPressed -= UpdateXBoxButtons;
		}

		protected override void Update()
		{
			base.Update();
			if (InputController.instance.isAnyControllerConnected == false)
			{
				return;
			}

			UpdateXBoxAxes();

			// place towers with X,Y,B buttons
			//if (LevelManager.instanceExists)
			//{
			//	int towerLibraryCount = LevelManager.instance.towerLibrary.Count;

			//	// find the lowest value between 3 (Xbox X,Y,B buttons - for this level only!)
			//	// and the amount of towers in the library
			//	int count = Mathf.Min(3, towerLibraryCount);
			//	KeyCode highestKey = KeyCode.Alpha1 + count;

			//	for (var key = KeyCode.Alpha1; key < highestKey; key++)
			//	{
			//		// add offset for the KeyCode Alpha 1 index to find correct keycodes
			//		if (UnityInput.GetKeyDown(key))
			//		{
			//			Tower controller = LevelManager.instance.towerLibrary[key - KeyCode.Alpha1];
			//			if (LevelManager.instance.currency.CanAfford(controller.purchaseCost))
			//			{
			//				if (m_GameUI.isBuilding)
			//				{
			//					m_GameUI.CancelGhostPlacement();
			//				}
			//				GameUI.instance.SetToBuildMode(controller);
			//				GameUI.instance.TryMoveGhost(InputController.instance.basicMouseInfo);
			//			}
			//			break;
			//		}
			//	}
			//}
		}

		private float horizontalValue;
		private float verticalValue;
		/// <summary>
		/// Update all of XBox Axis
		/// </summary>
		private void UpdateXBoxAxes()
		{
			var currentlySelected = placementManager.GetCurrentlySelectedArea();

			var hor = UnityInput.GetAxis("JoystickHorizontal");
			if (hor > thresholdForStick || hor < -thresholdForStick)
			{
				horizontalValue = hor;
				isSearchingForNextArea = true;
			}
			var vert = UnityInput.GetAxis("JoystickVertical");
			if (vert > thresholdForStick || vert < -thresholdForStick)
			{
				verticalValue = vert;
				isSearchingForNextArea = true;
			}
			Vector3 targetDirection = new Vector3(horizontalValue, 0, verticalValue);
			//Debug.DrawRay(currentlySelected.transform.position, targetDirection, Color.red, 2);

			if (isSearchingForNextArea && hor == 0 && vert == 0)
			{
				isSearchingForNextArea = false;
				var toSelect = placementManager.GetClosestAreaToDirection(targetDirection.normalized);
				placementManager.SelectArea(toSelect);
			}

			var hor2 = UnityInput.GetAxis("JoystickHorizontal2");
			var ver2 = UnityInput.GetAxis("JoystickVertical2");
		}

		/// <summary>
		/// Update XBox buttons
		/// </summary>
		private void UpdateXBoxButtons(XBoxButton button)
		{
			InputController.instance.isAnyJoystickButtonPressed = true;
			switch (button)
			{
				case XBoxButton.A:
					Debug.Log(string.Format("<color=blue><b>{0}</b></color>", "A"));
					break;
				case XBoxButton.B:
					Debug.Log(string.Format("<color=blue><b>{0}</b></color>", "B"));
					break;
				case XBoxButton.X:
					Debug.Log(string.Format("<color=blue><b>{0}</b></color>", "X"));
					break;
				case XBoxButton.Y:
					Debug.Log(string.Format("<color=blue><b>{0}</b></color>", "Y"));
					break;
				case XBoxButton.Start:
					OnStartButtonPressed();
					break;
				default:
					break;
			}
		}

		private void OnStartBackPressed()
		{

		}

		private void OnStartButtonPressed()
		{
			//if (UnityInput.GetButton("Start"))
			//{
			//	print("button: " + "START");

			//	if (LevelManager.instanceExists)
			//	{
			//		var inst = LevelManager.instance;
			//		if (inst.levelState != LevelState.Building)
			//		{
			//			pauseMenu.Pause();
			//		}
			//		else
			//		{
			//			pauseMenu.Unpause();
			//		}
			//	}
			//}
			switch (m_GameUI.state)
			{
				case State.Normal:
					if (m_GameUI.isTowerSelected)
					{
						m_GameUI.DeselectTower();
					}
					else 
					{
						pauseMenu.Pause();
					}
					break;
				case State.BuildingWithDrag:
				case State.Building:
					m_GameUI.CancelGhostPlacement();
					break;
				case State.Paused:
					pauseMenu.Pause();
					break;
			}
		}

		/// <summary>
		/// Select towers or position ghosts
		/// </summary>
		void OnTap(PointerActionInfo pointer)
		{
			// We only respond to mouse info
			var mouseInfo = pointer as MouseButtonInfo;

			if (mouseInfo != null && !mouseInfo.startedOverUI)
			{
				if (m_GameUI.isBuilding)
				{
					if (mouseInfo.mouseButtonId == 0) // LMB confirms
					{
						m_GameUI.TryPlaceTower(pointer);
					}
					else // RMB cancels
					{
						m_GameUI.CancelGhostPlacement();
					}
				}
				else
				{
					if (mouseInfo.mouseButtonId == 0)
					{
						// select towers
						m_GameUI.TrySelectTower(pointer);
					}
				}
			}
		}

		/// <summary>
		/// The object that holds the confirmation buttons
		/// </summary>
		public MovingCanvas confirmationButtons;

		/// <summary>
		/// The object that holds the invalid selection
		/// </summary>
		public MovingCanvas invalidButtons;

		/// <summary>
		/// Called by the confirm button on the UI
		/// </summary>
		public void OnTowerPlacementConfirmation()
		{
			confirmationButtons.canvasEnabled = false;
			if (!m_GameUI.IsGhostAtValidPosition())
			{
				return;
			}
			m_GameUI.BuyTower();
		}
	}
}