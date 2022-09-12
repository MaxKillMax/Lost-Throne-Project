using System.Collections.Generic;
using UnityEngine;

public class BuildingManager : MonoBehaviour
{
    private List<Building> _buildings;

    public List<Building> Buildings => _buildings;

    public void SetBuildingsList(List<Building> buildings) => _buildings = buildings;

    public float GetNearestBuildingDistance(Vector3 position, out Building nearestBuilding)
    {
        if (_buildings == null || _buildings.Count == 0)
        {
            nearestBuilding = default;
            return float.MaxValue;
        }

        int index = 0;
        float distance = float.MaxValue;

        for (int i = 0; i < _buildings.Count; i++)
        {
            float currentDistance = Vector3.Distance(position, _buildings[i].transform.position);
            if (currentDistance < distance)
            {
                distance = currentDistance;
                index = i;
            }
        }

        nearestBuilding = _buildings[index];
        return distance;
    }
}
