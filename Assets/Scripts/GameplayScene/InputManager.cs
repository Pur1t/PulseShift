using UnityEngine;
using System.Collections.Generic;

public class InputManager : MonoBehaviour
{
    public static InputManager Instance;
    public delegate void KeyPressed(int lane);
    public static event KeyPressed OnKeyPress;

    private Dictionary<int, KeyCode> laneBindings = new Dictionary<int, KeyCode>();

    void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);

        //DontDestroyOnLoad(gameObject);
        LoadDefaultBindings();
    }


    void Update()
    {
        for (int lane = 0; lane < laneBindings.Count; lane++)
        {
            if (Input.GetKeyDown(laneBindings[lane]))
            {
                if (OnKeyPress != null)
                {
                    OnKeyPress.Invoke(lane);
                }
                else
                {
                    Debug.LogWarning("OnKeyPress event is null. No listeners are registered.");
                }
            }
        }  
    }

    public KeyCode GetKeyBinding(int lane)
    {
        if (laneBindings.ContainsKey(lane))
        {
            return laneBindings[lane];
        }
        else
        {
            Debug.LogWarning("Lane index out of range: " + lane);
            return KeyCode.None;
        }
    }

    public void SetKeyBinding(int lane, KeyCode newKey)
    {
        if (laneBindings.ContainsKey(lane))
        {
            laneBindings[lane] = newKey;
        }
        else
        {
            Debug.LogWarning("Lane index " + lane + " is not bound.");
        }
    }

    private void LoadDefaultBindings()
    {
        laneBindings[0] = KeyCode.A;  // Leftmost lane
        laneBindings[1] = KeyCode.S;  // Second lane
        laneBindings[2] = KeyCode.Semicolon;  // Third lane
        laneBindings[3] = KeyCode.Quote;  // Rightmost lane
    }

}
