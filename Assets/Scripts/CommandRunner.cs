using UnityEngine;

// This script facilitates command input in the Unity Editor Inspector.
public class CommandRunner : MonoBehaviour
{
    [Tooltip("Drag the GameServer component from this GameObject here.")]
    public GameServer server; 
    
    // The command you want to run. Edit this value in the Inspector.
    public string commandInput = "WHITE select 4,7"; 

    // This method creates a clickable option when you right-click the component
    // in the Inspector, or you can select it from the three-dot menu.
    [ContextMenu("Execute Command")] 
    public void ExecuteCommandFromEditor()
    {
        if (server == null)
        {
            Debug.LogError("Server component not assigned. Cannot execute command.");
            return;
        }
        
        // This is where your input string gets passed to the GameServer logic.
        Debug.Log($"[INPUT SIMULATED] Executing command: {commandInput}");
        server.ExecuteCommand(commandInput);
    }
}
