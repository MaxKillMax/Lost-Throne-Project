using System;
using System.Collections.Generic;
using UnityEngine;

namespace LostThrone
{
    public class Formulas : IService
    {
        private Camera _camera;

        private readonly float _levelImportance = 0.4f;
        private readonly float _damageImportance = 1;
        private readonly float _armorImportance = 0.1f;
        private readonly float _healthImportance = 0.2f;

        public float LevelImportance => _levelImportance;
        public float DamageImportance => _damageImportance;
        public float ArmorImportance => _armorImportance;
        public float HealthImportance => _healthImportance;

        public void InitializeCamera(Camera camera)
        {
            _camera = camera;
        }

        public void GetCardStats(Unit unit, out float damage, out float armor, out float health)
        {
            damage = unit.GetStatistics(StatisticsType.Damage).Value;
            armor = unit.GetStatistics(StatisticsType.Armor).Value;
            health = unit.GetStatistics(StatisticsType.Health).Value;
        }

        public float GetUnitImportance(Unit unit)
        {
            GetCardStats(unit, out float damage, out float armor, out float health);
            return (unit.Level * _levelImportance) + (damage * _damageImportance) + (armor * _armorImportance) + (health * _healthImportance);
        }

        public float GetUnitAttackImportance(Unit unit)
        {
            GetCardStats(unit, out float damage, out float armor, out float health);
            return 50 + (unit.Level * _levelImportance) + (damage * _damageImportance) - (armor * _armorImportance) - (health * _healthImportance);
        }

        public float DamageReducedByArmor(float damage, float armor) => damage * (100 - armor) / 100;

        public List<Transform> GetTransformsOnMousePosition()
        {
            Ray ray = _camera.ScreenPointToRay(Input.mousePosition);

            List<RaycastHit2D> raycastHits = new(1);
            List<Transform> transforms = new(1);

            ContactFilter2D contactFilter = new();
            contactFilter.NoFilter();

            Physics2D.Raycast(ray.origin, ray.direction, contactFilter, raycastHits);

            for (int i = 0; i < raycastHits.Count; i++)
                transforms.Add(raycastHits[i].transform);

            return transforms;
        }

        public List<T> GetObjectsInMousePosition<T>(Func<T, bool> condition = null)
        {
            List<Transform> transforms = GetTransformsOnMousePosition();
            List<T> ts = new(1);

            for (int i = 0; i < transforms.Count; i++)
            {
                if (transforms[i].TryGetComponent(out T t))
                {
                    bool add = condition == null || condition.Invoke(t);

                    if (add)
                        ts.Add(t);
                }
            }

            return ts;
        }

        public bool TryGetObjectsInMousePosition<T>(out List<T> ts, Func<T, bool> condition = null)
        {
            ts = GetObjectsInMousePosition(condition);
            return ts.Count > 0;
        }

        public bool TryGetObjectInMousePosition<T>(out T t, Func<T, bool> condition = null)
        {
            List<T> ts = GetObjectsInMousePosition(condition);

            t = default;
            if (ts.Count > 0)
                t = ts[0];

            return ts.Count > 0;
        }
    }
}
