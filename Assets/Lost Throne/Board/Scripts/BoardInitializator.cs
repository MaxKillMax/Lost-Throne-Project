using UnityEngine;

namespace LostThrone
{
    public class BoardInitializator : Initializator
    {
        [SerializeField] private Window[] _boardWindows;

        protected override void Initialize()
        {
            if (Services.HaveService<UI>())
                Services.UnregisterService(Services.GetService<UI>());
            Services.RegisterService(new UI(_boardWindows));
        }
    }
}
