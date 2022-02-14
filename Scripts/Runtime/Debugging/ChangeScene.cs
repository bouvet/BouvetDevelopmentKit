using UnityEngine;
using UnityEngine.SceneManagement;

namespace Bouvet.DevelopmentKit
{
    public class ChangeScene : MonoBehaviour
    {
        [SerializeField] private int sceneID = 0;
        public void ResetApplication()
        {
            SceneManager.LoadScene(sceneID);
        }
    }
}
