using UnityEngine;
using UnityEngine.UI;
using System;
using System.Linq;
using System.Collections.Generic;
using TMPro;

public class HexGrid : MonoBehaviour {

    public const float DistanceOuter = 1.24f;
    public const float DistanceInner = DistanceOuter * 0.866025404f;
    public readonly string[] PossibleConnections = new string[] {"hr", "br", "bl", "hl", "tl", "tr"};
    private int XLimit = 5;
    private int YLimit = 3;

    private List<HexCell> CellList = new List<HexCell> {};
    private int total_draggables = -1;
    [SerializeField] GameObject GridBase;
    [SerializeField] GameObject GridConnection;
    [SerializeField] GameObject DraggableBase;
    [SerializeField] GameObject DraggableConnection;

    [SerializeField] GameObject FinishedText;
    [SerializeField] GameObject SeedText;
    int seed = - 1;
    public bool Interactable = true;



    void Update() {
        if (Input.GetKeyDown("r")) {
            ShuffleDraggables();
        }
    }

    public void MakePuzzle(int backgroundCells, int draggableCells, int specialSeed = -1, bool shuffle = true) {
        if (specialSeed == -1) {
            UnityEngine.Random.InitState(System.Environment.TickCount);
            seed = UnityEngine.Random.Range(0, 999999);
        }
        else {
            seed = specialSeed;
        }
        UnityEngine.Random.InitState(seed);

        GenerateBackground(backgroundCells, XLimit, YLimit);
        CreateBackgroundObjects();
        CreateRandomDraggablePattern(draggableCells);

        if (shuffle) {
            ShuffleDraggables();
        }
        total_draggables = GetDraggableAmount();

        if (SeedText != null) {
            SeedText.transform.GetChild(0).GetComponent<TextMeshProUGUI>().text = backgroundCells.ToString().PadLeft(2, '0') + "." + draggableCells.ToString().PadLeft(2, '0') + "." + seed.ToString().PadLeft(6, '0');
            SeedText.SetActive(true);
        }
    }

    public void RemovePuzzle() {
        foreach (Transform child in transform) {
            GameObject.Destroy(child.gameObject);
        }
        total_draggables = -1;
        if (SeedText != null) {
            SeedText.transform.GetChild(1).GetComponent<Image>().color = new Color(1, 1, 1, 0);
            SeedText.SetActive(false);
            
        }
    }

    public bool StartPuzzleWithSeed(string seed) {
        List<string> Elements = seed.Split('.').ToList();
        if (Elements.Count != 3 && Elements.Count != 2) {
            return false;
        }

        int background = -1;
        int draggables = -1;
        int actualSeed = -1;
        int.TryParse(Elements[0], out background);
        int.TryParse(Elements[1], out draggables);
        if (Elements.Count == 3) {
            int.TryParse(Elements[2], out actualSeed);
            if (actualSeed < 0) {
                actualSeed = -1;
            }
        }
        if (background > 6 && background < 46 && draggables > 1 && background >= draggables) {
            MakePuzzle(background, draggables, actualSeed);
            return true;
        }
        return false;
    }

    ////////////////////////////////////////////////////////////////////////////////
    /// background creation

    // function for random background generation
    private void GenerateBackground(int Amount, int LimitX, int LimitY) {
        // start with basic hexagon
        HexCell starter = new HexCell(0, 0);
        CellList = GetPossibleNeighbourCells(starter);
        CellList.Add(starter);

        // Add new Cells
        for (int i = 7; i < Amount; i++) {
            List<HexCell> OpenNeighbours = new List<HexCell>();

            // Add all open neighbours
            for (int j = 0; j < CellList.Count; j++) {
                OpenNeighbours = OpenNeighbours.Concat(GetEmptyNeighbourCells(CellList[j])).ToList();
            }

            // Pick open neighbours
            while (true) {
                int index = UnityEngine.Random.Range(0, OpenNeighbours.Count);
                if (GetNeighbourCells(CellList, OpenNeighbours[index]).Count > 1 &&
                    Math.Abs(OpenNeighbours[index].x) < LimitX &&
                    Math.Abs(OpenNeighbours[index].y) < LimitY) {
                    CellList.Add(OpenNeighbours[index]);
                    break;
                }
                else {
                    OpenNeighbours.RemoveAt(index);
                }
            }

        }
    }


    // function to create the objects and sprites for the background
    private void CreateBackgroundObjects() {
        for (int i = 0; i < CellList.Count; i++) {
            // make object
            GameObject CellBase = Instantiate<GameObject>(GridBase);

            CellBase.transform.SetParent(transform, false);
            CellBase.transform.localPosition = PositionToWorldLocation(CellList[i].x, CellList[i].y);
            CellList[i].Object = CellBase;

            // set neighbour connection sprites
            List<string> NeighbourConnections = GetNeighbourConnections(CellList, CellList[i]);

            for (int j = 0; j < PossibleConnections.Length; j++) {
                if (NeighbourConnections.Contains(PossibleConnections[j])) {
                    GameObject Connection = Instantiate<GameObject>(GridConnection);
                    Connection.transform.SetParent(CellBase.transform, false);
                    Connection.transform.Rotate(new Vector3(0, 0, 1), -60f * j);
                }
            }
        }
    }

    ////////////////////////////////////////////////////////////////////////////////
    /// draggable

    // check if all draggable connections are fullfiled
    public bool IsCompleted() {

        if (total_draggables != GetDraggableAmount()) {
            return false;
        }
        foreach(HexCell c in CellList) {
            if (c.Draggable != null && !c.Draggable.IsActivated()) {
                return false;
            }
        }
        return true;
    }

    // Create a random draggable pattern
    private void CreateRandomDraggablePattern(int amount) {
        List<HexCell> draggablePlaces= new List<HexCell>();
        draggablePlaces.Add(CellList[UnityEngine.Random.Range(0, CellList.Count)]);

        for (int i = 1; i < amount; i++) {
            List<HexCell> openspots = GetNeighbourCells(CellList, draggablePlaces[UnityEngine.Random.Range(0, draggablePlaces.Count)]);
            if (openspots.Count > 0) {
                while (openspots.Count > 0) {
                    int index = UnityEngine.Random.Range(0, openspots.Count);
                    if (draggablePlaces.Contains(openspots[index])) {
                        openspots.RemoveAt(index);
                    }
                    else {
                        draggablePlaces.Add(openspots[index]);
                        break;
                    }
                }

                if (openspots.Count == 0) {
                    i--;
                }
            }
            else {
                i--;
            }
        }

        foreach (HexCell cell in draggablePlaces) {
            CreateDraggable(cell, GetNeighbourConnections(draggablePlaces, cell));
        }

        foreach (HexCell cell in draggablePlaces) {
            int connectionsAmount = cell.Draggable.connections.Length;
            if (connectionsAmount > 2) {
                int chance = UnityEngine.Random.Range(0, 8);
                if (chance <= connectionsAmount) {
                    List<HexCell> neighbours = GetNeighbourCells(draggablePlaces, cell);
                    foreach(HexCell neighbour in neighbours) {
                        if (neighbour.Draggable.connections.Length > 2) {
                            RemoveDraggableConnection(cell, GetNeighbourSide(cell, neighbour));
                            RemoveDraggableConnection(neighbour, GetNeighbourSide(neighbour, cell));
                            break;
                        }
                    }
                }
            }
        }
    }

    // remove a connections from a draggable by creating a new one and replacing it
    private void RemoveDraggableConnection(HexCell cell, string side) {
        List<string> newConnections = new List<string>();
        foreach(string s in cell.Draggable.connections) {
            if (s != side) {
                newConnections.Add(s);
            }
        }
        Draggable temp = cell.Draggable;
        DetachDraggable(cell.Draggable);
        Destroy(temp.gameObject);
        CreateDraggable(cell, newConnections);
    }

    // create a draggable with connections
    private void CreateDraggable(HexCell cell, List<string> connections) {
        Draggable Base = Instantiate<GameObject>(DraggableBase).GetComponent<Draggable>();
        Base.transform.SetParent(transform, false);
        SetDraggableConnections(Base, connections);
        AttachDraggable(cell, Base);
    }

    // Set the connection sprites of a draggable
    private void SetDraggableConnections(Draggable draggable, List<string> connections) {
        draggable.connections = connections.ToArray();
        foreach (string c in connections) {
            GameObject Connection = Instantiate<GameObject>(DraggableConnection);
            Connection.transform.SetParent(draggable.gameObject.transform, false);
            Connection.transform.Rotate(new Vector3(0, 0, 1), -60f * Array.IndexOf(PossibleConnections, c));
        }
    }

    // Attach a draggable to a cell
    public void AttachDraggable(HexCell cell, Draggable draggable) {
        if (cell.Draggable != null) {
            DetachDraggable(cell.Draggable, true);
        }
        cell.Draggable = draggable;
        draggable.transform.localPosition = PositionToWorldLocation(cell.x, cell.y);
        CheckDraggableConnections(cell, true);
        draggable.StopDragging();
    }

    // Detach a draggable from its cell
    public bool DetachDraggable(Draggable draggable, bool ignore_rules = false) {
        if (!ignore_rules && ((total_draggables != -1 && total_draggables > GetDraggableAmount()) || IsCompleted() || !Interactable)) {
            return false;
        }
        foreach(HexCell cell in CellList) {
            if (cell.Draggable == draggable) {

                CheckDraggableConnections(cell, false);
                cell.Draggable = null;
            }
        }
        draggable.StartDragging();
        return true;
    }

    // Get the amount of draggables
    public int GetDraggableAmount() {
        int output = 0;
        foreach(HexCell cell in CellList) {
            if (cell.Draggable != null) {
                output++;
            }
        }
        return output;
    }

    // Shuffle all the draggables
    private void ShuffleDraggables() {
        List<Draggable> DraggableList = new List<Draggable>();
        foreach (HexCell cell in CellList) {
            if (cell.Draggable != null) {
                DraggableList.Add(cell.Draggable);
                DetachDraggable(cell.Draggable, true);
            }
        }
        List<HexCell> TempList = new List<HexCell>(CellList);
        while (DraggableList.Count > 0) {
            int index = UnityEngine.Random.Range(0, TempList.Count);
            HexCell cell = TempList[index];
            if (cell.Draggable == null) {
                AttachDraggable(cell, DraggableList[0]);
                DraggableList.RemoveAt(0);
            }
            else {
                TempList.RemoveAt(index);
            }
        }
    }


    private void CheckDraggableConnections(HexCell cell, bool ToSet) {
        List<HexCell> neighbours = GetNeighbourCells(CellList, cell);
        foreach(HexCell c in neighbours) {
            if (c.Draggable != null) {
                string side = GetNeighbourSide(cell, c);
                string otherSide = PossibleConnections[(Array.IndexOf(PossibleConnections, side) + 3) % 6];
                if (cell.Draggable.connections.Contains(side) &&
                    c.Draggable.connections.Contains(otherSide)) {
                    cell.Draggable.SetConnection(side, ToSet);
                    c.Draggable.SetConnection(otherSide, ToSet);
                }
            }
        }
    }

    public void SetInteractable(bool input) {
        Interactable = input;
    }

    ////////////////////////////////////////////////////////////////////////////////
    /// position transformation

    // Function to convert grid position to world vector
    public Vector3 PositionToWorldLocation(int x, int y) {
        Vector3 position;
        if (y % 2 == 0) {
            position.x = x * (DistanceInner * 2f);
        }
        else {
            position.x = (x + 0.5f) * (DistanceInner * 2f);
        }
        position.y = y * (DistanceOuter * 1.5f);
        position.z = 0f;

        return position;
    }

    // Function to convert world vector to closest grid position
    public HexCell WorldLocationToPosition(Vector3 position) {
        float x = position.x / (DistanceInner * 2f);
        float y = position.y / (DistanceOuter * 1.5f);

        // float Ydecimals = Math.Abs(y) - Math.Abs((int) y);
        // float Xdecimals = Math.Abs(x) - Math.Abs((int) x);
        // int FinalY = 0;
        // int FinalX = 0;

        // // in next y level
        // if (Ydecimals > 2/3) {
        //     if (y > 0) {
        //         FinalY = (int) y + 1;
        //     }
        //     else {
        //         FinalY = (int) y - 1;
        //     }
        // }
        // // normal y level
        // else if (Ydecimals < 1/3) {
        //     FinalY = (int) y;
        // }
        // // not sure which y level
        // else {
        //     // check if even y level
        //     if (Math.Abs((int) y) == 0) {
        //         // check if next y level
        //         if (Xdecimals < 0.25f || Xdecimals > 0.75f) {
        //             FinalY = (int) y;
        //         }
        //         else {
        //             if (y > 0) {
        //                 FinalY = (int) y + 1;
        //             }
        //             else {
        //                 FinalY = (int) y - 1;
        //             }
        //         }
        //     }
        //     else {
        //         if (Xdecimals < 0.25f || Xdecimals > 0.75f) {
        //             if (y > 0) {
        //                 FinalY = (int) y + 1;
        //             }
        //             else {
        //                 FinalY = (int) y - 1;
        //             }
        //         }
        //         else {
        //             FinalY = (int) y;
        //         }
        //     }
        // }

        // if (Math.Abs(FinalY) % 2 == 0) {
        //     x = x - 0.25f;
        // }
        
        // if (x - (int) x > 0.5f) {
        //     if (x > 0) {
        //         FinalX = (int) x + 1;
        //     }
        //     else {
        //         FinalX = (int) x - 1;
        //     }
        // }
        // else {
        //     FinalX = (int) x;
        // }

        int FinalY = (int) y;
        if (Math.Abs(y) - Math.Abs((int) y) > 0.5f) {
            if (y > 0) {
                FinalY++;
            }
            else {
                FinalY--;
            }
        }
        
        if (Math.Abs(FinalY) % 2 == 1) {
            x = x - 0.5f;
        } 
        int FinalX = (int) x;
        if (Math.Abs(x) - Math.Abs((int) x) > 0.5f) {
            if (x > 0) {
                FinalX++;
            }
            else {
                FinalX--;
            }
        }

        foreach(HexCell cell in CellList) {
            if (FinalX == cell.x && FinalY == cell.y) {
                return cell;
            }
        }
        return null;

    }

    ////////////////////////////////////////////////////////////////////////////////
    /// neighbouring cells

    // Get the neighbouring cells in the grid
    public List<HexCell> GetNeighbourCells(List<HexCell> grid, HexCell cell) {
        List<HexCell> output = new List<HexCell>();
        for (int i = 0; i < grid.Count; i++) {
            if ((cell.x + 1 == grid[i].x && cell.y == grid[i].y) || // hard right
                (cell.x - 1 == grid[i].x && cell.y == grid[i].y) || // hard left
                (cell.x == grid[i].x && cell.y + 1 == grid[i].y) || // top (either left or right)
                ((Math.Abs(cell.y % 2) == 0 && cell.x - 1 == grid[i].x && cell.y + 1 == grid[i].y) || (Math.Abs(cell.y % 2) == 1 && cell.x + 1 == grid[i].x && cell.y + 1 == grid[i].y)) || // other top
                (cell.x == grid[i].x && cell.y - 1 == grid[i].y) || // bottom (either left or right)
                ((Math.Abs(cell.y % 2) == 0 && cell.x - 1 == grid[i].x && cell.y - 1 == grid[i].y) || (Math.Abs(cell.y % 2) == 1 && cell.x + 1 == grid[i].x && cell.y - 1 == grid[i].y)) // other bottom
                ) {
                output.Add(grid[i]);
            }
        }
        return output;
    }

    public string GetNeighbourSide(HexCell cell, HexCell neighbour) {
        if (cell.x + 1 == neighbour.x && cell.y == neighbour.y) {
            return "hr";
        }
        else if ((Math.Abs(cell.y % 2) == 0 && cell.x == neighbour.x && cell.y - 1 == neighbour.y) || (Math.Abs(cell.y % 2) == 1 && cell.x + 1 == neighbour.x && cell.y - 1 == neighbour.y)) {
            return "br";
        }
        else if ((Math.Abs(cell.y % 2) == 0 && cell.x - 1 == neighbour.x && cell.y - 1 == neighbour.y) || (Math.Abs(cell.y % 2) == 1 && cell.x == neighbour.x && cell.y - 1 == neighbour.y)) {
            return "bl";
        }
        else if (cell.x - 1 == neighbour.x && cell.y == neighbour.y) {
            return "hl";
        }
        else if ((Math.Abs(cell.y % 2) == 0 && cell.x - 1 == neighbour.x && cell.y + 1 == neighbour.y) || (Math.Abs(cell.y % 2) == 1 && cell.x == neighbour.x && cell.y + 1 == neighbour.y)) {
            return "tl";
        }
        else if ((Math.Abs(cell.y % 2) == 0 && cell.x == neighbour.x && cell.y + 1 == neighbour.y) || (Math.Abs(cell.y % 2) == 1 && cell.x + 1 == neighbour.x && cell.y + 1 == neighbour.y)) {
            return "tr";
        }
        return null;
    }

    // Get a specific side neighbour in the grid
    public HexCell GetSideCell(HexCell cell, string side) {
        if (side == "hr") {
            for (int i = 0; i < CellList.Count; i++) {
                if (cell.x + 1 == CellList[i].x && cell.y == CellList[i].y) {
                    return cell;
                }
            }
        }
        else if (side == "br") {
            for (int i = 0; i < CellList.Count; i++) {
                if ((Math.Abs(cell.y % 2) == 0 && cell.x == CellList[i].x && cell.y - 1 == CellList[i].y) || (Math.Abs(cell.y % 2) == 1 && cell.x + 1 == CellList[i].x && cell.y - 1 == CellList[i].y)) {
                    return cell;
                }
            }
        }
        else if (side == "bl") {
            for (int i = 0; i < CellList.Count; i++) {
                if ((Math.Abs(cell.y % 2) == 0 && cell.x - 1 == CellList[i].x && cell.y - 1 == CellList[i].y) || (Math.Abs(cell.y % 2) == 1 && cell.x == CellList[i].x && cell.y - 1 == CellList[i].y)) {
                    return cell;
                }
            }
        }
        else if (side == "hl") {
            for (int i = 0; i < CellList.Count; i++) {
                if (cell.x - 1 == CellList[i].x && cell.y == CellList[i].y) {
                    return cell;
                }
            }
        }
        else if (side == "tl") {
            for (int i = 0; i < CellList.Count; i++) {
                if ((Math.Abs(cell.y % 2) == 0 && cell.x - 1 == CellList[i].x && cell.y + 1 == CellList[i].y) || (Math.Abs(cell.y % 2) == 1 && cell.x == CellList[i].x && cell.y + 1 == CellList[i].y)) {
                    return cell;
                }
            }
        }
        else if (side == "tr") {
            for (int i = 0; i < CellList.Count; i++) {
                if ((Math.Abs(cell.y % 2) == 0 && cell.x == CellList[i].x && cell.y + 1 == CellList[i].y) || (Math.Abs(cell.y % 2) == 1 && cell.x + 1 == CellList[i].x && cell.y + 1 == CellList[i].y)) {
                    return cell;
                }
            }
        }
        return null;
    }


    // Get the positions around a cell
    public List<HexCell> GetPossibleNeighbourCells(HexCell cell) {  
        List<HexCell> output = new List<HexCell>() {
            new HexCell(cell.x + 1, cell.y), // hard right
            new HexCell(cell.x - 1, cell.y), // hard left
            new HexCell(cell.x, cell.y + 1), // top
            new HexCell(cell.x, cell.y - 1), // bottom
            };

        if (Math.Abs(cell.y % 2) == 0) {
            output.Add(new HexCell(cell.x - 1, cell.y + 1)); // other top
            output.Add(new HexCell(cell.x - 1, cell.y - 1)); // other bottom
        }
        else {
            output.Add(new HexCell(cell.x + 1, cell.y + 1)); // other top
            output.Add(new HexCell(cell.x + 1, cell.y - 1)); // other bottom
        }
        return output;
    }

    // Get the neighbour cells that are empty
    public List<HexCell> GetEmptyNeighbourCells(HexCell cell) {
        List<HexCell> PossibleNeighbours = GetPossibleNeighbourCells(cell);

        for (int i = 0; i < CellList.Count; i++) {
            for (int j = 0; j < PossibleNeighbours.Count; j++) {
                if (PossibleNeighbours[j].x == CellList[i].x && PossibleNeighbours[j].y == CellList[i].y) {
                    PossibleNeighbours.RemoveAt(j);
                    break;
                }
            }
        }
        return PossibleNeighbours;
    }

    // Get the connections to other cells
    public List<string> GetNeighbourConnections(List<HexCell> grid, HexCell cell) {
        List<string> output = new List<string>();
        for (int i = 0; i < grid.Count; i++) {
            if (cell.x + 1 == grid[i].x && cell.y == grid[i].y) {
                output.Add("hr"); // hard right
            }
            else if (cell.x - 1 == grid[i].x && cell.y == grid[i].y) {
                output.Add("hl"); // hard left
            }
            else if ((Math.Abs(cell.y % 2) == 0 && cell.x - 1 == grid[i].x && cell.y + 1 == grid[i].y) || (Math.Abs(cell.y % 2) == 1 && cell.x == grid[i].x && cell.y + 1 == grid[i].y)) {
                output.Add("tl"); // top left
            }
            else if ((Math.Abs(cell.y % 2) == 0 && cell.x == grid[i].x && cell.y + 1 == grid[i].y) || (Math.Abs(cell.y % 2) == 1 && cell.x + 1 == grid[i].x && cell.y + 1 == grid[i].y)) {
                output.Add("tr"); // top right
            }
            else if ((Math.Abs(cell.y % 2) == 0 && cell.x - 1 == grid[i].x && cell.y - 1 == grid[i].y) || (Math.Abs(cell.y % 2) == 1 && cell.x == grid[i].x && cell.y - 1 == grid[i].y)) {
                output.Add("bl"); // bottom left
            }
            else if ((Math.Abs(cell.y % 2) == 0 && cell.x == grid[i].x && cell.y - 1 == grid[i].y) || (Math.Abs(cell.y % 2) == 1 && cell.x + 1 == grid[i].x && cell.y - 1 == grid[i].y)) {
                output.Add("br"); // bottom right
            }
        }
        return output;
    }

}