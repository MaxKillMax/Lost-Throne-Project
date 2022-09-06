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

            _path = _data.Map.GetTileDatasOfPath(_player.transform.position, mousePosition);
            _currentPath = 0;

            if (_path != null && _path.Length > 0)
            {
                CanUpdate = true;
                _slowestUnitSpeed = GetMinUnitSpeed();
                if (_data.Zoom.GetZoomState() != ZoomType.Standard)
                    _data.Zoom.SetZoom(ZoomType.Standard);
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
            List<Unit> units = _data.Units;

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
            _data.Transform.position = Vector3.MoveTowards(_data.Transform.position, _path[_currentPath].realPosition, _speed * _slowestUnitSpeed * _path[_currentPath].movementMultiply * 0.00001f);

            if (Vector2.Distance(_data.Transform.position, _path[_currentPath].realPosition) < _minDistance)
                SetNextPath();
        }

        public void SetNextPath()
        {
            _currentPath++;
            if (_currentPath >= _path.Length)
            {
                if (_data.BuildingManager.GetNearestBuildingDistance(_path[_path.Length - 1].realPosition, out Building building) < _buildingDistance)
                    OpenBuilding();
                CanUpdate = false;
            }
        }

        public void OpenBuilding()
        {
            _data.Zoom.SetZoom(ZoomType.InBuilding);
        }
    }
}
