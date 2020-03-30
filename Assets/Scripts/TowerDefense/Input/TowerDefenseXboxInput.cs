using Core.Input;
using TowerDefense.Level;
using TowerDefense.Towers;
using TowerDefense.UI.HUD;
using UnityEngine;
using State = TowerDefense.UI.HUD.GameUI.State;
using UnityInput = UnityEngine.Input;

namespace TowerDefense.Input
{
	[RequireComponent(typeof(GameUI))]
	public class TowerDefenseXboxInput : XBox360Input
	{
		[SerializeField] private PlacementManager placementManager = null;
		[SerializeField] private BuildSidebar buildSidebar = null;
		[SerializeField] private GameObject firstSelectedTowerMenuButton = null;
		private int selectedTowerTypeButtonId = -1;
		private Tower selectedTowerType;

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

		//private void Start()
		//{
		//	if (LevelManager.instanceExists)
		//	{
		//		selectedTowerType = LevelManager.instance.towerLibrary[0];
		//		SelectTowerType(selectedTowerType);
		//	}
		//}

		protected override void Update()
		{
			base.Update();
			if (InputController.instance.isAnyControllerConnected == false)
			{
				return;
			}

			UpdateXBoxAxes();
		}

		private float horizontalValue;
		private float verticalValue;
		private bool justSwitchedButtons;
		/// <summary>
		/// Update all of XBox Axis
		/// </summary>
		private void UpdateXBoxAxes()
		{
			if (m_GameUI.state == State.BuildingMenu)
			{
				return;
			}
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

			if (justSwitchedButtons == false)
			{
				if (hor2 > thresholdForStick * 2)
				{
					justSwitchedButtons = true;
					if (selectedTowerTypeButtonId < 2)
					{
						selectedTowerTypeButtonId++;
					}
					SelectTowerType(selectedTowerTypeButtonId);
				}
				else if (hor2 < -thresholdForStick * 2)
				{
					justSwitchedButtons = true;
					if (selectedTowerTypeButtonId > 0)
					{
						selectedTowerTypeButtonId--;
					}

					SelectTowerType(Mathf.Clamp(selectedTowerTypeButtonId, 0, 2));
				}
			}
			if (hor2 == 0)
			{
				justSwitchedButtons = false;
			}
		}

		/// <summary>
		/// Update XBox buttons
		/// </summary>
		private void UpdateXBoxButtons(XBoxButton button)
		{
			if (LevelManager.instanceExists == false)
			{
				return;
			}
			int towerLibraryCount = LevelManager.instance.towerLibrary.Count;

			InputController.instance.isAnyJoystickButtonPressed = true;
			selectedTowerTypeButtonId = -1;
			switch (button)
			{
				case XBoxButton.A:
					if (m_GameUI.state == State.BuildingMenu)
					{
						return;
					}
					if (placementManager.isCurrentAreaOccupied() == false)
					{
						if (selectedTowerType == null)
						{
							return;
						}
						int cost = selectedTowerType.purchaseCost;
						bool successfulPurchase = LevelManager.instance.currency.TryPurchase(cost);
						if (successfulPurchase)
						{
							Tower createdTower = Instantiate(selectedTowerType);
							createdTower.Initialize(placementManager.GetCurrentlySelectedArea());
						}
					}
					else
					{
						var tower = placementManager.GetCurrentlySelectedArea().towerHere;
						if (tower != null)
						{
							m_GameUI.SelectTower(tower);
							m_GameUI.SetBuildingMenuState();
							UnityEngine.EventSystems.EventSystem.current.SetSelectedGameObject(firstSelectedTowerMenuButton, null);
							m_GameUI.SetUpGhostTower(tower);
						}
					}
					break;
				case XBoxButton.Start:
					OnStartPressed();
					break;
				case XBoxButton.Back:
					OnBackPressed();
					break;
				case XBoxButton.X:
					selectedTowerTypeButtonId = 0;
					break;
				case XBoxButton.Y:
					selectedTowerTypeButtonId = 1;
					break;
				case XBoxButton.B:
					selectedTowerTypeButtonId = 2;
					break;

			}
			if (selectedTowerTypeButtonId != -1)
			{
				SelectTowerType(selectedTowerTypeButtonId);
			}
		}

		private void SelectTowerType(int id)
		{
			if (LevelManager.instanceExists == false)
			{
				return;
			}
			if (LevelManager.instance.towerLibrary[id] != null)
			{
				selectedTowerType = LevelManager.instance.towerLibrary[id];
			}
			buildSidebar.SelectButton(id);
			SelectTowerType(selectedTowerType);
		}

		private void SelectTowerType(Tower t)
		{
			if (LevelManager.instanceExists == false)
			{
				return;
			}
			m_GameUI.SetToBuildMode(t);
		}

		private void OnBackPressed()
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
						pauseMenu.Pause();
					}
					break;
				case State.BuildingWithDrag:
				case State.Building:
					m_GameUI.CancelGhostPlacement();
					break;
				case State.BuildingMenu:
					m_GameUI.DeselectTower();
					m_GameUI.Unpause();
					break;
			}
		}

		private void OnStartPressed()
		{
			if (LevelManager.instanceExists == false || LevelManager.instance.levelState != LevelState.Building)
			{
				return;
			}
			buildSidebar.StartWaveButtonPressed();
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
	}
}