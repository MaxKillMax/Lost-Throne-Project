using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace LostThrone.OpenWorld
{
    public class Player : MonoBehaviour
    {
        [SerializeField] private PlayerData _data;

        private List<NativeComponent> _nativeComponents;

        private void Awake()
        {
            _nativeComponents = new List<NativeComponent>()
            {
                new Initialization(),
                new Movement(),
                new Improver()
            };

            foreach (NativeComponent native in _nativeComponents)
                native.Initialize(this, _data);
        }

        private void OnDestroy()
        {
            foreach (NativeComponent native in _nativeComponents)
                native.OnDestroy();
        }

        private void Update()
        {
            foreach (NativeComponent native in _nativeComponents)
            {
                if (native.CanUpdate)
                    native.Update();
            }
        }

        public T GetNative<T>() where T : NativeComponent => _nativeComponents.First(n => n.GetType() == typeof(T)) as T;

        public bool RemoveNative<T>() where T : NativeComponent => _nativeComponents.Remove(_nativeComponents.First(n => n.GetType() == typeof(T)));

        public bool AddNative<T>(T t, bool initialize = true) where T : NativeComponent
        {
            if ((_nativeComponents.First(n => n.GetType() == typeof(T)) as T) != null)
                return false;

            if (initialize)
                t.Initialize(this, _data);

            _nativeComponents.Add(t);
            return true;
        }
    }
}
