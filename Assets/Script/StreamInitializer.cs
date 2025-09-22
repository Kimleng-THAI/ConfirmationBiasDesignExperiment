using UnityEngine;

public class StreamInitializer : MonoBehaviour
{
    void Awake()
    {
        // Force LSL initialization at game start
        var lslManager = LSLManager.Instance;

        // Send initialization marker
        lslManager.SendMarker("UNITY_GAME_START");
        lslManager.SendMarker($"TIMESTAMP_{System.DateTime.Now:yyyy-MM-dd_HH:mm:ss.fff}");

        Debug.Log("[StreamInitializer] LSL streams initialized at game start");
    }
}
