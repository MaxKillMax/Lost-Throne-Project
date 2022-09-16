using System.Linq;

namespace LostThrone
{
    public class UI : IService
    {
        private Window[] _windows;

        public UI(Window[] windows)
        {
            _windows = windows;
            InitializeWindows(_windows);
        }

        private void InitializeWindows(Window[] windows)
        {
            for (int i = 0; i < windows.Length; i++)
                windows[i].InitializeWindow();
        }

        public T GetWindow<T>() where T : Window => _windows.OfType<T>().ToList()[0];

        public bool AnyWindowIsOpened() => _windows.Any((w) => w.State == WindowState.Opened);
    }

    public enum WindowState
    {
        Opened,
        Closed
    }
}
