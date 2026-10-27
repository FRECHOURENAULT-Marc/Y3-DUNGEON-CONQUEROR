using Unity.VisualScripting;
using UnityEngine;

public class MouseController : MonoBehaviour
{
    [SerializeField] PlayerReferences _ref;

    bool _mouseView;


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        _ref.PlayerInput.QuitApp += HandleQuitApp;

        _mouseView = false;

        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
    }

    void OnDestroy()
    {
        _ref.PlayerInput.QuitApp -= HandleQuitApp;
    }


    void HandleQuitApp()
    {
        // Cursor.visible = true;
        // Cursor.lockState = CursorLockMode.None;

        // #if UNITY_EDITOR
        //     UnityEditor.EditorApplication.isPlaying = false;
        // #endif

        // Application.Quit();

        ToggleMouse();
    }

    void ToggleMouse()
    {
        if (_mouseView)
        {
            Cursor.visible = false;
            Cursor.lockState = CursorLockMode.Locked;
        }
        else
        {
            Cursor.visible = true;
            Cursor.lockState = CursorLockMode.None;
        }

        _mouseView = !_mouseView;
    }

    public void ShowMouseCursor()
    {
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
    }

    public void HideMouseCursor()
    {
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
    }

}
