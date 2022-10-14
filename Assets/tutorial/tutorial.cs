using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Events;

public class tutorial : MonoBehaviour
{

    [SerializeField] public string[] checkpoints;
    [SerializeField] public UnityEvent[] events;

    private int progress = 0;
    [SerializeField] public HexGrid Grid;


    // Start is called before the first frame update
    void Start()
    {
        events[progress].Invoke();
    }

    // Update is called once per frame
    void Update()
    {
        Debug.Log(Grid.IsCompleted());
        if (checkpoints[progress] == "puzzle" && Grid.IsCompleted()) {
            Advance();
        }
        else if (checkpoints[progress] == "talk" && Input.anyKeyDown) {
            Advance();
        }
        else if (checkpoints[progress] == "shuffle" && Input.GetKeyDown("r")) {
            Advance();
        }
    }

    public void Advance() {
        progress++;
        if (progress == checkpoints.Length) {
            SceneManager.LoadScene("SampleScene");
        }
        else {
            events[progress].Invoke();
        }
    }

    public void MakePuzzle1 () {
        Grid.SetInteractable(false);
        Grid.MakePuzzle(7, 2, 836527);
    }

    public void MakePuzzle2 () {
        Grid.RemovePuzzle();
        Grid.SetInteractable(false);
        Grid.MakePuzzle(7, 4, 836527);
    }

    public void MakePuzzle3 () {
        Grid.RemovePuzzle();
        Grid.SetInteractable(false);
        Grid.MakePuzzle(7, 4, 836528, false);
    }
}
