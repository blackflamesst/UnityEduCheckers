using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;


namespace Checkers
{
    public class SceneController : MonoBehaviour
    {
        public void OpenGameScene()
        {
            SceneManager.LoadScene(0);
        }
    }
}
