using UnityEngine;






public enum State { Clear, Open, Close };
[HideInInspector]
public class HexmapNode
{
    public Vector3          pos;
    public HexmapNode[]     neighbour = new HexmapNode[0];
    public float            cost;
    public Node             parent;
    public bool             walkable = true;
    public State            state = State.Clear;

    public HexmapNode()
    {

    }

    public HexmapNode( Vector3 _pos )
    {
        pos                 = _pos;
    }
}

