using UnityEngine;
using UnityEngine.SceneManagement;

public class ChangeScene : MonoBehaviour
{
    [SerializeField] private int sceneID = 0;
    public void ResetApplication()
    {
        SceneManager.LoadScene(sceneID);
    }
}
