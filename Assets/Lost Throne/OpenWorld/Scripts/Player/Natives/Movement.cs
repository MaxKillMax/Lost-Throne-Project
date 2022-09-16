using System.Collections.Generic;
using UnityEngine;

namespace LostThrone.OpenWorld
{
    public class Movement : NativeComponent
    {
        private float _speed = 5;
        private float _slowestUnitSpeed = 0;
        private float _minDistance = 0.05f;
        private float _buildingDistance = 0.25f;

        private TileData[] _path;
        private int _currentPath;

        public void TryMoveToMousePoint()
        {
            Vector3 mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            mousePosition.z = 0;

            _path = Data.Map.GetTileDatasOfPath(Player.transform.position, mousePosition);
            _currentPath = 0;

            if (_path != null && _path.Length > 0)
            {
                CanUpdate = true;
                _slowestUnitSpeed = GetMinUnitSpeed();
                if (Data.Zoom.GetZoomState() != ZoomType.Standard)
                    Data.Zoom.SetZoom(ZoomType.Standard);
            }
            else
            {
                CanUpdate = false;
            }
        }

        public void Proceed()
        {
            CanUpdate = true;
            _slowestUnitSpeed = GetMinUnitSpeed();
        }

        public void Stop()
        {
            CanUpdate = false;
        }

        private float GetMinUnitSpeed()
        {
            List<Unit> units = Data.Units;

            float minSpeed = float.MaxValue;
            float currentSpeed = 0;

            for (int i = 0; i < units.Count; i++)
            {
                currentSpeed = units[i].GetStatistics(StatisticsType.Speed).Value;
                if (currentSpeed < minSpeed)
                    minSpeed = currentSpeed;
            }

            return currentSpeed;
        }

        public override void Update()
        {
            // TODO: Coroutine
            Data.Transform.position = Vector3.MoveTowards(Data.Transform.position, _path[_currentPath].RealPosition, _speed * _slowestUnitSpeed * _path[_currentPath].MovementMultiply * 0.00001f);

            if (Vector2.Distance(Data.Transform.position, _path[_currentPath].RealPosition) < _minDistance)
                SetNextPath();
        }

        public void SetNextPath()
        {
            _currentPath++;
            if (_currentPath >= _path.Length)
            {
                if (Data.BuildingManager.GetNearestBuildingDistance(_path[^1].RealPosition, out _) < _buildingDistance)
                    OpenBuilding();
                CanUpdate = false;
            }
        }

        public void OpenBuilding()
        {
            Data.Zoom.SetZoom(ZoomType.InBuilding);
        }
    }
}
