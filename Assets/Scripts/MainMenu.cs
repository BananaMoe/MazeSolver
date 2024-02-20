#if UNITY_EDITOR
using UnityEditor;
#endif

using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Unity.VisualScripting.Antlr3.Runtime.Tree;

public class MainMenu : MonoBehaviour
{

    public TMP_InputField sizeInputField;
    public Button generateButton;
    public Button playButton;
    public Button highlightPathButton;
    public Button quitButton;
    public MazeGenerator mazeGenerator;
    public TextMeshProUGUI winMessage;
    public Toggle jungleToggle;
    public Toggle candyToggle;
    private Vector2Int mazeSize;

    private void Start()
    {
        generateButton.interactable = false;
        playButton.interactable = false;
        sizeInputField.onValueChanged.AddListener(DelegateMethod);
        generateButton.onClick.AddListener(OnGenerateClicked);
        playButton.onClick.AddListener(OnPlayClicked);
        highlightPathButton.onClick.AddListener(OnHighlightPathClicked);
        quitButton.onClick.AddListener(OnQuitClicked);
        mazeGenerator.OnMazeGenerated += EnableButtons;
        mazeGenerator.OnPathHighlighted += EnableHighlightPathButton;
        highlightPathButton.interactable = false;
    }

    private void Awake()
    {
        jungleToggle.onValueChanged.AddListener(delegate {
            HandleThemeChange(jungleToggle.isOn, "jungle");
        });
        candyToggle.onValueChanged.AddListener(delegate {
            HandleThemeChange(candyToggle.isOn, "candy");
        });

        // Set default theme to Jungle
        jungleToggle.isOn = true;
        mazeGenerator.SetThemeToJungle();
    }

    private void DelegateMethod(string input)
    {
        int size;
        if (int.TryParse(input, out size))
        {
            if (size >= 6 && size <= 20)
            {
                generateButton.interactable = true;
            }
            else
            {
                generateButton.interactable = false;
            }
        }
    }

    private void OnGenerateClicked()
    {
        playButton.interactable = false;
        generateButton.interactable = false;
        sizeInputField.interactable = false;
        jungleToggle.interactable = false;
        candyToggle.interactable = false;
        int size = int.Parse(sizeInputField.text);
        mazeGenerator.ClearMaze();
        mazeGenerator.GenerateNewMaze(size);
    }

    private void OnPlayClicked()
    {
        if (mazeGenerator.HasGeneratedMaze)
        {
            playButton.interactable = false;
            generateButton.interactable = false;
            sizeInputField.interactable = false;
            jungleToggle.interactable = false;
            candyToggle.interactable = false;
            mazeGenerator.PlacePlayer();
            highlightPathButton.interactable = true;
            var playerController = FindObjectOfType<PlayerController>();
            playerController.OnReachedGoal += ShowWinMessage;
        }
    }

    private void OnHighlightPathClicked()
    {
        highlightPathButton.interactable = false;
        quitButton.interactable = false;
        mazeGenerator.HighlightPathFromPlayer();
    }

    private void OnQuitClicked()
    {
        if (mazeGenerator.PlayerExists())
        {
            mazeGenerator.RemovePlayer();
            playButton.interactable = true;
            generateButton.interactable = true;
            sizeInputField.interactable = true;
            highlightPathButton.interactable = false;
            jungleToggle.interactable = true;
            candyToggle.interactable = true;
        }
        else
        {
            #if UNITY_EDITOR
            EditorApplication.isPlaying = false;
            #else
            Application.Quit();
            #endif
        }
    }
    private void EnableButtons()
    {
        playButton.interactable = true;
        generateButton.interactable = true;
        sizeInputField.interactable = true;
        jungleToggle.interactable = true;
        candyToggle.interactable = true;
    }

    private void EnableHighlightPathButton()
    {
        highlightPathButton.interactable = true;
        quitButton.interactable = true;
    }

    private void ShowWinMessage()
    {
        highlightPathButton.interactable = false;
        StartCoroutine(ShowWinMessageCoroutine());
    }

    private IEnumerator ShowWinMessageCoroutine()
    {
        winMessage.gameObject.SetActive(true);
        winMessage.text = "YOU REACHED THE END";
        yield return new WaitForSeconds(2f);
        winMessage.gameObject.SetActive(false);
        quitButton.interactable = true;
        playButton.interactable = true;
        generateButton.interactable = true;
        sizeInputField.interactable = true;
        jungleToggle.interactable = true;
        candyToggle.interactable = true;
    }

    private void HandleThemeChange(bool isOn, string theme)
    {
        if (isOn)
        {
            switch (theme)
            {
                case "jungle":
                    mazeGenerator.SetThemeToJungle();
                    playButton.interactable = false;
                    break;
                case "candy":
                    mazeGenerator.SetThemeToCandy();
                    playButton.interactable = false;
                    break;
                default:
                    break;
            }
        }
    }

    private void OnDestroy()
    {
        mazeGenerator.OnMazeGenerated -= EnableButtons;
        mazeGenerator.OnPathHighlighted -= EnableHighlightPathButton;
        sizeInputField.onValueChanged.RemoveListener(DelegateMethod);
        generateButton.onClick.RemoveListener(OnGenerateClicked);
        playButton.onClick.RemoveListener(OnPlayClicked);
        highlightPathButton.onClick.RemoveListener(OnHighlightPathClicked);
        quitButton.onClick.RemoveListener(OnQuitClicked);
        var playerController = FindObjectOfType<PlayerController>();
        if (playerController != null)
        {
            playerController.OnReachedGoal -= ShowWinMessage;
        }
    }

}
