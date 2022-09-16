using System.Collections.Generic;
using System.Linq;

namespace LostThrone
{
    public static class Services
    {
        private static HashSet<IService> ServicesHashSet = new(5);

        public static void RegisterService<T>(T service) where T : IService
        {
            if (!ServicesHashSet.Any(s => s.GetType() == typeof(T)))
                ServicesHashSet.Add(service);
        }

        public static void UnregisterService<T>(T service) where T : IService
        {
            if (ServicesHashSet.Contains(service))
                ServicesHashSet.Remove(service);
        }

        public static T GetService<T>() where T : IService => ServicesHashSet.OfType<T>().FirstOrDefault();

        public static bool HaveService<T>() where T : IService => GetService<T>() != null;
    }
}
