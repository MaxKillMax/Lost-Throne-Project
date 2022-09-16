using System;
using UnityEngine;

namespace LostThrone
{
    public abstract class Window : MonoBehaviour
    {
        public event Action OnWindowOpened;
        public event Action OnWindowClosed;

        [SerializeField] private WindowState _initialState; 

        private WindowState _state;
        public WindowState State => _state;

        public virtual void InitializeWindow()
        {
            SetWindowState(_initialState);
        }

        public void SetWindowState(WindowState state)
        {
            if (state == WindowState.Opened)
                OpenWindow();
            else if (state == WindowState.Closed)
                CloseWindow();
        }

        public virtual void OpenWindow()
        {
            _state = WindowState.Opened;
            gameObject.SetActive(true);
            OnWindowOpened?.Invoke();
        }

        public virtual void CloseWindow()
        {
            _state = WindowState.Closed;
            gameObject.SetActive(false);
            OnWindowClosed?.Invoke();
        }
    }
}
