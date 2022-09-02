using UnityEngine;

namespace Game.OpenWorld
{
    public class Improver : NativeComponent
    {
        public void ImproveAttribute(AttributeType type, float value)
        {
            _data.Unit.SetAttributeValue(type, _data.Unit.GetAttribute(type).value + value);
        }

        public void DowngradeAttribute(AttributeType type, float value)
        {
            if (_data.Unit.GetAttribute(type).value - value < 0)
                return;

            _data.Unit.SetAttributeValue(type, _data.Unit.GetAttribute(type).value - value);
        }
    }
}
