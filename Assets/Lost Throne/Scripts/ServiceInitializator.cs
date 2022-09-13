using UnityEngine;

namespace LostThrone
{
    public class ServiceInitializator : MonoBehaviour
    {
        private void Awake()
        {
            Services.RegisterService(new BoardBase());
            Services.RegisterService(new Formulas());
        }
    }
}
