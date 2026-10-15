using UnityEngine;

public class MenuLoad : MonoBehaviour
{
    [SerializeField] private GameObject MainButtonsScreen;
    [SerializeField] private GameObject LearningLevelsScreen;
    [SerializeField] private GameObject TestLevelsScreen;

    private void Awake()
    {

        MainButtonsScreen.SetActive(true);
        LearningLevelsScreen.SetActive(false);
        TestLevelsScreen.SetActive(false);
    }
}
   
