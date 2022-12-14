using UnityEngine;

namespace LostThrone.OpenWorld
{
    public class Initialization : NativeComponent
    {
        public override void Initialize(Player player, PlayerData data)
        {
            base.Initialize(player, data);

            UnitData unitData = Data.PlayerUnitData;
            Attribute[] attributes = unitData.Attributes;

            int points = 0;
            for (int i = 0; i < attributes.Length; i++)
            {
                points += (int)attributes[i].Value;
                attributes[i].Value = 0;
            }

            int random;
            while (points > 0)
            {
                random = Random.Range(0, attributes.Length);

                attributes[random].Value++;
                points--;
            }

            Unit playerUnit = new(unitData);
            Data.Initialize(playerUnit);
        }
    }
}
