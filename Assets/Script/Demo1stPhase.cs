using UnityEngine;
using UnityEngine.Video;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;
using TMPro;

public class Demo1stPhase : MonoBehaviour
{
    private PlayerInputActions inputActions;
    private VideoPlayer videoPlayer;
    private bool videoFinished = false;

    [Header("UI")]
    public TextMeshProUGUI continuePromptText;

    [Header("Next Scene")]
    public string nextSceneName = "InstructionScreen";

    void Awake()
    {
        inputActions = new PlayerInputActions();

        // Add a VideoPlayer dynamically if not added manually
        videoPlayer = gameObject.AddComponent<VideoPlayer>();
        videoPlayer.playOnAwake = true;
        videoPlayer.renderMode = VideoRenderMode.CameraNearPlane;
        videoPlayer.targetCamera = Camera.main;
        videoPlayer.clip = Resources.Load<VideoClip>("DemoVideo/DemoVideoPhase1"); // Load from Resources folder
        videoPlayer.isLooping = false;

        if (videoPlayer.clip == null)
            Debug.LogError("[DemoSceneManager] DemoVideo1stPhase.mp4 not found in Resources/DemoVideo folder.");
    }

    void Start()
    {
        if (continuePromptText != null)
            continuePromptText.gameObject.SetActive(false); // Hide at start

        videoPlayer.loopPointReached += OnVideoFinished;
        videoPlayer.Play();
    }

    void OnEnable()
    {
        inputActions.UI.Enable();
        inputActions.UI.Continue.performed += OnContinuePressed;
    }

    void OnDisable()
    {
        inputActions.UI.Continue.performed -= OnContinuePressed;
        inputActions.UI.Disable();

        if (videoPlayer != null)
            videoPlayer.loopPointReached -= OnVideoFinished;
    }

    private void OnVideoFinished(VideoPlayer vp)
    {
        videoFinished = true;
        Debug.Log("[DemoSceneManager] Video finished playing.");

        // Show prompt to continue
        if (continuePromptText != null)
        {
            continuePromptText.text = "Press SPACE to confirm you understand the video demonstration.";
            continuePromptText.gameObject.SetActive(true);
        }
    }

    private void OnContinuePressed(InputAction.CallbackContext context)
    {
        if (videoFinished)
            LoadNextScene();
    }

    private void LoadNextScene()
    {
        if (!string.IsNullOrEmpty(nextSceneName))
            SceneManager.LoadScene(nextSceneName);
        else
            Debug.LogWarning("[DemoSceneManager] nextSceneName is not set.");
    }
}
