using System.Linq;
using System.Collections.Generic;

namespace Game
{
    public static class ServiceLocator
    {
        private static HashSet<Service> services = new HashSet<Service>(5);

        public static void RegisterService<T>(T service) where T : Service
        {
            if (!services.Any(s => s.GetType() == typeof(T)))
                services.Add(service);
        }

        public static void UnregisterService<T>(T service) where T : Service
        {
            if (services.Contains(service))
                services.Remove(service);
        }

        public static T GetService<T>() where T : Service => services.FirstOrDefault(s => s.GetType() == typeof(T)) as T;

        public static bool HaveService<T>() where T : Service => GetService<T>() != default;
    }
}
