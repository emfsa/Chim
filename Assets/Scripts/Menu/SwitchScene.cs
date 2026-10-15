using Unity.VectorGraphics;
using UnityEngine;

public class SwitchScene : MonoBehaviour
{
   public void Switch(string sceneName)
   {
      UnityEngine.SceneManagement.SceneManager.LoadScene(sceneName);
   }
}
