using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class QuitToMenu : MonoBehaviour
{
    public void changeScene(string scene_name)
    {
        SceneManager.LoadScene(scene_name);
    }
}
