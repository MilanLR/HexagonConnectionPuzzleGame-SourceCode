using UnityEngine;

public class HexCell {

    public HexCell(int Inputx, int Inputy) {
        x = Inputx;
        y = Inputy;
    }

    public HexCell(int Inputx, int Inputy, GameObject InputObject) {
        x = Inputx;
        y = Inputy;
        Object = InputObject;
    }

    public int x;
    public int y;

    public GameObject Object;

    public Draggable Draggable = null;

}