using System.Collections.Generic;
using TowerDefense.Towers.Placement;
using UnityEngine;

namespace TowerDefense
{
	public class PlacementManager : MonoBehaviour
	{
		[SerializeField] private SingleTowerPlacementArea[] placementAreas;
		[SerializeField] private SingleTowerPlacementArea firstSelectedArea;
		private SingleTowerPlacementArea currentlySelectedArea;

		private void Start()
		{
			SelectArea(firstSelectedArea);
		}

		public SingleTowerPlacementArea GetCurrentlySelectedArea()
		{
			return currentlySelectedArea;
		}

		public SingleTowerPlacementArea GetClosestAreaToDirection(Vector3 targetDirection)
		{
			List<SingleTowerPlacementArea> vectorsToAllAreas = new List<SingleTowerPlacementArea>();
			List<float> differences = new List<float>();
			foreach (var area in placementAreas)
			{
				if (area == currentlySelectedArea)
				{
					continue;
				}

				var heading = area.transform.position - currentlySelectedArea.transform.position;
				var distance = heading.magnitude;
				var direction = heading / distance;
				direction.y = 0;
				Debug.DrawRay(area.transform.position, direction, Color.green, 5);
				float diff = Vector3.Distance(targetDirection, direction);
				bool foundLowerDifference = false;
				if (differences.Count == 0)
				{
					differences.Add(diff);
					vectorsToAllAreas.Add(area);
				}
				else
				{
					foreach (var alreadyAdded in differences)
					{
						if (diff < alreadyAdded)
						{
							foundLowerDifference = true;
							break;
						}
					}
					if (foundLowerDifference)
					{
						float previouslyAtZero = differences[0];
						differences.Insert(0, diff);
						differences.Add(previouslyAtZero);
						SingleTowerPlacementArea previous = vectorsToAllAreas[0];
						vectorsToAllAreas.Insert(0, area);
						vectorsToAllAreas.Add(previous);

						Debug.Log(string.Format("<color=blue><b>{0}</b></color>", "added at 0: " + diff));
					}
					else
					{
						differences.Add(diff);
						vectorsToAllAreas.Add(area);
						Debug.Log(string.Format("<color=blue><b>{0}</b></color>", "added: " + diff));
					}
				}
			}
			Dictionary<float, SingleTowerPlacementArea> placementByDistance = new Dictionary<float, SingleTowerPlacementArea>();
			for (int i = 0; i < differences.Count; i++)
			{
				if (placementByDistance.ContainsKey(differences[i]))
				{
					placementByDistance.Add(differences[i] + 0.0001f, vectorsToAllAreas[i]);
				}
				else
				{
					placementByDistance.Add(differences[i], vectorsToAllAreas[i]);
				}
			}
			differences.Sort();
			// returns clostest
			return placementByDistance[differences[0]];
		}

		public void SelectArea(SingleTowerPlacementArea area)
		{
			if (currentlySelectedArea != null)
			{
				currentlySelectedArea.Select(false);
			}
			currentlySelectedArea = area;
			currentlySelectedArea.Select(true);
		}
	}
}
