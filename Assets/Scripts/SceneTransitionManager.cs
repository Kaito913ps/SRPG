using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneTransitionManager : MonoBehaviour
{
    [SerializeField]
    private string _sceneName;

    public void SceneChange()
    {
        FadeManager._instance.FadeOutToIn(SceneTo);
    }

    private void SceneTo()
    {
        SceneManager.LoadScene(_sceneName);
    }
}
