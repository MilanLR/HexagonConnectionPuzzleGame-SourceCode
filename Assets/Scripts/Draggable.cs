using UnityEngine;
using System;

public class Draggable : MonoBehaviour
{
    private delegate void DragEndedDelegate(Draggable DraggableObject);
    private DragEndedDelegate DragEndedCallback;

    private bool IsDragged = false;
    private Vector3 MouseDragStartLocation;
    private Vector3 SpriteDragStartPosition;

    public string[] connections;
    private int ActiveConnections = 0;
    private HexGrid Grid;

    [SerializeField] Sprite ConnectionActive;
    [SerializeField] Sprite ConnectionInactive;
    [SerializeField] Sprite BaseActive;
    [SerializeField] Sprite BaseInactive;


    void Start() {
        DragEndedCallback = GameObject.FindWithTag("SnapController").GetComponent<SnapController>().OnDragEnded;
        Grid = GameObject.FindWithTag("Grid").GetComponent<HexGrid>();
    }

    void Update() {
        if (IsDragged) {
            transform.localPosition = SpriteDragStartPosition + (Camera.main.ScreenToWorldPoint(Input.mousePosition) - MouseDragStartLocation);
        }
    }

    public void StartDragging() {
        IsDragged = true;
        transform.localPosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        transform.localPosition = new Vector3(transform.localPosition.x, transform.localPosition.y, 0f);
        MouseDragStartLocation = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        SpriteDragStartPosition = transform.localPosition;

        // set to front
        foreach(SpriteRenderer spriteRenderer in this.gameObject.GetComponentsInChildren<SpriteRenderer>()) {
            spriteRenderer.sortingOrder = 5;
        }
        this.gameObject.GetComponent<SpriteRenderer>().sortingOrder = 6;
        this.transform.localScale = new Vector3(0.7f, 0.7f, 0.7f);
    }

    public void StopDragging() {
        IsDragged = false;

        // set to front
        foreach(SpriteRenderer spriteRenderer in this.gameObject.GetComponentsInChildren<SpriteRenderer>()) {
            spriteRenderer.sortingOrder = 3;
        }
        this.gameObject.GetComponent<SpriteRenderer>().sortingOrder = 4;
        this.transform.localScale = new Vector3(0.6f, 0.6f, 0.6f);

    }

    private void OnMouseDown() {
        if (!IsDragged) {
            IsDragged = Grid.DetachDraggable(this);
        }
        else {
            DragEndedCallback(this);
        }
    }

    public void SetConnection(string side, bool active) {
        if (active) {
            this.gameObject.transform.GetChild(Array.IndexOf(connections, side)).GetComponent<SpriteRenderer>().sprite = ConnectionActive;
            ActiveConnections++;
            if (ActiveConnections == connections.Length) {
                SetBase(true);
            }
        }
        else {
            this.gameObject.transform.GetChild(Array.IndexOf(connections, side)).GetComponent<SpriteRenderer>().sprite = ConnectionInactive;
            ActiveConnections--;
            if (IsActivated() && ActiveConnections != connections.Length) {
                SetBase(false);
            }
        }
    }

    private void SetBase(bool active) {
        if (active) {
            this.gameObject.GetComponent<SpriteRenderer>().sprite = BaseActive;
        }
        else {
            this.gameObject.GetComponent<SpriteRenderer>().sprite = BaseInactive;
        }
    }

    public bool IsActivated() {
        return this.gameObject.GetComponent<SpriteRenderer>().sprite == BaseActive;
    }


}
