using UnityEngine;

namespace Game.OpenWorld
{
    public class InputReader : MonoBehaviour
    {
        [SerializeField] private Player _player;

        private Movement _movement;

        private void Start()
        {
            _movement = _player.GetNative<Movement>();
        }

        private void Update()
        {
            if (Input.GetMouseButtonDown(1))
                _movement.TryMoveToMousePoint();
        }
    }
}
