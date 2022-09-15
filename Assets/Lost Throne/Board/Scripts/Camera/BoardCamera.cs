using UnityEngine;
using Cinemachine;

namespace LostThrone.Board
{
    public class BoardCamera : MonoBehaviour
    {
        [SerializeField] private Camera _camera;
        [SerializeField] private CinemachineVirtualCamera _virtualCamera;

        [SerializeField] private GameObject _hand;

        [SerializeField] private float _cameraStandardSize = 2;
        [SerializeField] private float _cameraOverviewSize = 6.5f;

        private Cell _currentCameraCell;
        public Cell CurrentCameraCell => _currentCameraCell;

        private bool _overviewIsActivated = false;
        public bool OverviewIsActivated => _overviewIsActivated;

        private Cell[,] _grid;

        public void InitializeCamera(Cell[,] grid)
        {
            _grid = grid;
            // TODO: Camera initialized in initializer
            Services.GetService<Formulas>().InitializeCamera(_camera);
        }

        public void MoveCamera(Direction direction)
        {
            for (int x = 0; x < _grid.GetLength(0); x++)
            {
                for (int y = 0; y < _grid.GetLength(1); y++)
                {
                    if (_grid[x, y] == _currentCameraCell)
                    {
                        switch (direction)
                        {
                            case Direction.Top:
                                if (x + 1 < _grid.GetLength(0))
                                    SetCameraTarget(_grid[x + 1, y]);
                                break;
                            case Direction.Right:
                                if (y + 1 < _grid.GetLength(1))
                                    SetCameraTarget(_grid[x, y + 1]);
                                break;
                            case Direction.Bottom:
                                if (x - 1 >= 0)
                                    SetCameraTarget(_grid[x - 1, y]);
                                break;
                            case Direction.Left:
                                if (y - 1 >= 0)
                                    SetCameraTarget(_grid[x, y - 1]);
                                break;
                        }

                        return;
                    }
                }
            }
        }

        public void SetCameraTarget(Cell cell)
        {
            if (_overviewIsActivated)
                return;

            _currentCameraCell = cell;
            _virtualCamera.Follow = _currentCameraCell.transform;
        }

        public void SwitchOverviewMode()
        {
            if (_overviewIsActivated)
                DeactivateOverviewMode();
            else
                ActivateOverviewMode();
        }

        public void ActivateOverviewMode()
        {
            SetCameraTarget(_grid[1, 1]);
            SetOverviewProperties(true, _cameraOverviewSize);
        }

        public void DeactivateOverviewMode()
        {
            SetOverviewProperties(false, _cameraStandardSize);
        }

        private void SetOverviewProperties(bool state, float size)
        {
            _overviewIsActivated = state;
            _hand.SetActive(!state);
            _virtualCamera.m_Lens.OrthographicSize = size;
        }
    }
}
