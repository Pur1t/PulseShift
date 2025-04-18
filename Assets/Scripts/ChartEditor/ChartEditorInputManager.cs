using UnityEngine;
using UnityEngine.SceneManagement;

public class ChartEditorInputManager : MonoBehaviour
{
    void Update()
    {
        bool isCmdOrCtrl = Input.GetKey(KeyCode.LeftCommand) || Input.GetKey(KeyCode.RightCommand) ||
                           Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl);

        if (Input.GetKeyDown(KeyCode.X))
        {
            ChartEditorManager.Instance.DeleteSelectedNotes();
        }
        if (isCmdOrCtrl && Input.GetKeyDown(KeyCode.S))
        {
            ChartEditorManager.Instance.SaveChart();
        }
    }
}