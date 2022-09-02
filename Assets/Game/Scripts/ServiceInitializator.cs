using UnityEngine;

namespace Game
{
    public class ServiceInitializator : MonoBehaviour
    {
        private void Awake()
        {
            ServiceLocator.RegisterService(new BoardPositionsFinder());
        }
    }
}
