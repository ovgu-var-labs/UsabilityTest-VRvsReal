using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneController : MonoBehaviour
{
    public delegate void SceneManagerKeyPress();
    public event SceneManagerKeyPress OnSceneReloadKeyPressed;
    public EventManager eventManager;

    public void ReloadScene()
    {
        OnSceneReloadKeyPressed?.Invoke();
    }
}
