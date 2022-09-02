namespace Game.OpenWorld
{
    public abstract class NativeComponent
    {
        protected Player _player;
        protected PlayerData _data;

        public bool CanUpdate { get; protected set; }

        public virtual void Initialize(Player player, PlayerData data)
        {
            _player = player;
            _data = data;
        }

        public virtual void Update() { }

        public virtual void OnDestroy() { }
    }
}
