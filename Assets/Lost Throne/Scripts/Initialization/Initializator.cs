using UnityEngine;

namespace LostThrone
{
    // TODO: Create OpenWorldInitializator
    // TODO: Create OpenWorld Scene, Board Scene, Init Scene
    public abstract class Initializator : MonoBehaviour
    {
        private void Awake()
        {
            Initialize();
            Destroy(gameObject);
        }

        protected abstract void Initialize();
    }
}
