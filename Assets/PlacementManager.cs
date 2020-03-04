using System.Collections.Generic;
using TowerDefense.Towers.Placement;
using UnityEngine;

namespace TowerDefense
{
	public class PlacementManager : MonoBehaviour
	{
		[SerializeField] private float thresholdForVectorDifference = 0.5f;
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
				Vector3 pos = area.transform.position;

				var heading = pos - currentlySelectedArea.transform.position;
				var distance = heading.magnitude;
				var direction = heading / distance;
				direction.y = 0;

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
						differences.Insert(0, diff);
						vectorsToAllAreas.Insert(0, area);
					}
					else
					{
						differences.Add(diff);
						vectorsToAllAreas.Add(area);
					}
				}
			}
			Dictionary<float, SingleTowerPlacementArea> placementByVectorDifference = new Dictionary<float, SingleTowerPlacementArea>();
			int countOfLocationsBelowThreshold = 0;

			for (int i = 0; i < differences.Count; i++)
			{
				if (differences[i] < thresholdForVectorDifference)
				{
					countOfLocationsBelowThreshold++;
				}
				if (placementByVectorDifference.ContainsKey(differences[i]))
				{
					placementByVectorDifference.Add(differences[i] + 0.0001f, vectorsToAllAreas[i]);
				}
				else
				{
					placementByVectorDifference.Add(differences[i], vectorsToAllAreas[i]);
				}
			}
			differences.Sort();
			Dictionary<float, SingleTowerPlacementArea> placementByDistance = new Dictionary<float, SingleTowerPlacementArea>();
			List<float> distances = new List<float>();
			// We take 3 locations that share the similar vector

			Debug.Log(string.Format("<color=blue><b>{0}</b></color>", "count: " + countOfLocationsBelowThreshold));
			for (int i = 0; i < countOfLocationsBelowThreshold + 1; i++)
			{
				// We compare the distances and take the one with shortest
				float distance = (placementByVectorDifference[differences[i]].transform.position - currentlySelectedArea.transform.position).magnitude;

				Debug.Log(string.Format("<color=blue><b>{0}</b></color>", "place: " + placementByVectorDifference[differences[i]].name
					+ ", Distance: " + distance
					+ ", Vector difference: " + differences[i]));
				if (placementByDistance.ContainsKey(distance))
				{
					distance += 0.0001f;
				}
				placementByDistance.Add(distance, placementByVectorDifference[differences[i]]);
				distances.Add(distance);
			}
			distances.Sort();
			// returns clostest
			Debug.Log(string.Format("<color=green><b>{0}</b></color>", "Chose: " + placementByDistance[distances[0]].name));
			return placementByDistance[distances[0]];
		}

		private Vector2 GetScreenSpacePosition(Vector3 worldSpacePos)
		{
			return Camera.main.WorldToScreenPoint(worldSpacePos);
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
