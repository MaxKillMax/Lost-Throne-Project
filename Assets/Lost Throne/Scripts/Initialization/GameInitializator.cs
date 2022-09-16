using UnityEngine.SceneManagement;

namespace LostThrone
{
    public class GameInitializator : Initializator
    {
        protected override void Initialize()
        {
            Services.RegisterService(new BoardBase());
            Services.RegisterService(new Formulas());
            Services.RegisterService(new UI());

            SceneManager.LoadScene(1);
        }
    }
}
