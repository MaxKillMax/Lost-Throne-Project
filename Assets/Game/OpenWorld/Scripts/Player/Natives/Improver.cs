namespace LostThrone.OpenWorld
{
    public class Improver : NativeComponent
    {
        public void ImproveAttribute(AttributeType type, float value)
        {
            Data.Unit.SetAttributeValue(type, Data.Unit.GetAttribute(type).Value + value);
        }

        public void DowngradeAttribute(AttributeType type, float value)
        {
            if (Data.Unit.GetAttribute(type).Value - value < 0)
                return;

            Data.Unit.SetAttributeValue(type, Data.Unit.GetAttribute(type).Value - value);
        }
    }
}
