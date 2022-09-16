using UnityEngine;

namespace LostThrone
{
    public class BoardInitializator : Initializator
    {
        [SerializeField] private Camera _camera;
        [SerializeField] private Window[] _windows;

        protected override void Initialize()
        {
            if (Services.HaveService<Formulas>())
                Services.GetService<Formulas>().InitializeCamera(_camera);

            if (Services.HaveService<UI>())
                Services.GetService<UI>().InitializeWindows(_windows);
        }
    }
}
