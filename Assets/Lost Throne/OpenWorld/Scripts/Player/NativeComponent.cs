namespace LostThrone.OpenWorld
{
    public abstract class NativeComponent
    {
        protected Player Player;
        protected PlayerData Data;

        public bool CanUpdate { get; protected set; }

        public virtual void Initialize(Player player, PlayerData data)
        {
            Player = player;
            Data = data;
        }

        public virtual void Update() { }

        public virtual void OnDestroy() { }
    }
}
