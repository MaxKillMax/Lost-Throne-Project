
namespace LostThrone
{
    public class ServiceInitializator : Initializator
    {
        protected override void Initialize()
        {
            Services.RegisterService(new BoardBase());
            Services.RegisterService(new Formulas());
        }
    }
}
