using Core.Input;
using TowerDefense.Level;
using TowerDefense.Towers;
using TowerDefense.UI.HUD;
using UnityEngine;
using UnityInput = UnityEngine.Input;
using State = TowerDefense.UI.HUD.GameUI.State;
using TowerDefense.UI;
using TowerDefense.Towers.Placement;

namespace TowerDefense.Input
{
	[RequireComponent(typeof(GameUI))]
	public class TowerDefenseXboxInput : XBox360Input
	{
		[SerializeField] private PlacementManager placementManager;
		private InputController controller;

		/// <summary>
		/// Is using Xbox controller to select tower placement area?
		/// </summary>
		bool isSearchingForNextArea;

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
				controller = InputController.instance;

				//controller.tapped += OnTap;
				//controller.mouseMoved += OnMouseMoved;
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

			controller = InputController.instance;

			//controller.tapped -= OnTap;
			//controller.mouseMoved -= OnMouseMoved;
		}

		protected override void Update()
		{
			if (controller.isAnyControllerConnected == false)
			{
				return;
			}
			base.Update();

			UpdateXBoxButtons();

			// place towers with X,Y,B buttons
			if (LevelManager.instanceExists)
			{
				int towerLibraryCount = LevelManager.instance.towerLibrary.Count;

				// find the lowest value between 3 (Xbox X,Y,B buttons - for this level only!)
				// and the amount of towers in the library
				int count = Mathf.Min(3, towerLibraryCount);
				KeyCode highestKey = KeyCode.Alpha1 + count;

				for (var key = KeyCode.Alpha1; key < highestKey; key++)
				{
					// add offset for the KeyCode Alpha 1 index to find correct keycodes
					if (UnityInput.GetKeyDown(key))
					{
						Tower controller = LevelManager.instance.towerLibrary[key - KeyCode.Alpha1];
						if (LevelManager.instance.currency.CanAfford(controller.purchaseCost))
						{
							if (m_GameUI.isBuilding)
							{
								m_GameUI.CancelGhostPlacement();
							}
							GameUI.instance.SetToBuildMode(controller);
							GameUI.instance.TryMoveGhost(InputController.instance.basicMouseInfo);
						}
						break;
					}
				}
			}
		}

		private float horizontalValue;
		private float verticalValue;
		/// <summary>
		/// Update XBox buttons
		/// </summary>
		private void UpdateXBoxButtons()
		{
			// Escape handling
			if (UnityInput.GetButton("Start"))
			{
				switch (m_GameUI.state)
				{
					case State.Normal:
						if (m_GameUI.isTowerSelected)
						{
							m_GameUI.DeselectTower();
						}
						else
						{
							m_GameUI.Pause();
						}
						break;
					case State.BuildingWithDrag:
					case State.Building:
						m_GameUI.CancelGhostPlacement();
						break;
				}
			}
			var currentlySelected = placementManager.GetCurrentlySelectedArea();

			var hor = UnityInput.GetAxis("Horizontal");
			if (hor != 0)
			{
				horizontalValue = hor;
				isSearchingForNextArea = true;
			}
			var vert = UnityInput.GetAxis("Vertical");
			if (vert != 0)
			{
				verticalValue = vert;
				isSearchingForNextArea = true;
			}
			Vector3 targetDirection = new Vector3(horizontalValue, 0, verticalValue);
			Debug.DrawRay(currentlySelected.transform.position, targetDirection, Color.red, 2);

			if (hor == 0 && vert == 0)
			{
				if (isSearchingForNextArea)
				{
					isSearchingForNextArea = false;
					var toSelect = placementManager.GetClosestAreaToDirection(targetDirection.normalized);
					placementManager.SelectArea(toSelect);
				}
			}


			if (UnityInput.GetButton("Fire1"))
			{
				print("button: " + "A");
			}
			if (UnityInput.GetButton("Fire2"))
			{
				print("button: " + "B");
			}
			if (UnityInput.GetButton("Fire3"))
			{
				print("button: " + "X");
			}
			if (UnityInput.GetButton("Jump"))
			{
				print("button: " + "Y");
			}
			if (UnityInput.GetButton("Fire1"))
			{
				print("button: " + "A");
			}
			if (UnityInput.GetButton("Start"))
			{
				print("button: " + "START");
			}
		}

		private void StartSearchingForClosestArea()
		{

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