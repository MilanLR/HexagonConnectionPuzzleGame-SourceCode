using UnityEngine;
using System.Linq;
using UnityEngine.SceneManagement;

public class MenuManager : MonoBehaviour
{
    [SerializeField] GameObject MainMenu;
    [SerializeField] HexGrid Grid;
    [SerializeField] GameObject FinishedMenu;
    [SerializeField] GameObject BackButton;
    [SerializeField] GameObject InfoText;
    [SerializeField] Fades InvalidFade;

    private int LastBackground;
    private int LastDraggable;


    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown("escape")) {
            MainMenu.SetActive(true);
            FinishedMenu.SetActive(false);
            InfoText.SetActive(false);
            Grid.RemovePuzzle();
        }
        if (Grid.IsCompleted()) {
            FinishedMenu.SetActive(true);
            BackButton.SetActive(false);
        }
    }


    public void StartSeed(string seed) {
        bool output = Grid.StartPuzzleWithSeed(seed);
        if (!output) {
            InvalidFade.FadeInAndOut();
        }
        else {
            int.TryParse(seed.Split('.').ToList()[0], out LastBackground);
            int.TryParse(seed.Split('.').ToList()[1], out LastDraggable);
            MainMenu.SetActive(false);
            BackButton.SetActive(true);
        }
    }
    
    public void RestartPuzzle() {
        Grid.RemovePuzzle();
        Grid.MakePuzzle(LastBackground, LastDraggable);
    }

    public void StartTutorial() {
        SceneManager.LoadScene("tutorial");
    }

    public void StartEasy() {
        Grid.MakePuzzle(9, 7);
        LastBackground = 9;
        LastDraggable = 7;
    }

    public void StartMedium() {
        Grid.MakePuzzle(12, 9);
        LastBackground = 12;
        LastDraggable = 9;
    }

    public void StartHard() {
        Grid.MakePuzzle(20, 17);
        LastBackground = 20;
        LastDraggable = 17;
    }

    public void StartTimeWaster() {
        Grid.MakePuzzle(30, 25);
        LastBackground = 30;
        LastDraggable = 25;
    }

    public void Quit() {
        Application.Quit();
    }
}
