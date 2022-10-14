using UnityEngine;

public class SnapController : MonoBehaviour
{
    [SerializeField] private HexGrid Grid;
    public float SnapRange = 0.5f;


    public void OnDragEnded(Draggable draggable) {
        HexCell ClosestCell = Grid.WorldLocationToPosition(draggable.transform.localPosition);

        if (ClosestCell != null && Vector2.Distance(ClosestCell.Object.transform.localPosition, draggable.transform.localPosition) <= SnapRange) {
            Grid.AttachDraggable(ClosestCell, draggable);
        }
    }

}
