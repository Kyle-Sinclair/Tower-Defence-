using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameTile : MonoBehaviour {

    GameTileContent content;

    public GameTileContent Content {
        get => content;
        set {
            Debug.Assert(value != null, "Null assigned to content!");
            if (content != null) {
                content.Recycle();
            }
            content = value;
            content.transform.localPosition = transform.localPosition;
        }

    }
    [SerializeField]
    Transform arrow = default;
    public GameTile NextTileOnPath => nextOnPath;
    public Vector3 ExitPoint { get; private set; }
    GameTile north, east, south, west, nextOnPath;
    int distance;

    public bool HasPath => distance != int.MaxValue;
    public GameTile GrowPathNorth() => GrowPathTo(north, Direction.South);
    public GameTile GrowPathEast() => GrowPathTo(east, Direction.West);
    public GameTile GrowPathSouth() => GrowPathTo(south, Direction.North);
    public GameTile GrowPathWest() => GrowPathTo(west, Direction.East);
    public bool IsAlternative { get; set; }

    public Direction PathDirection { get; private set; }
    static Quaternion
        northRotation = Quaternion.Euler(90f, 0f, 0f),
        eastRotation = Quaternion.Euler(90f, 90f, 0f),
        southRotation = Quaternion.Euler(90f, 180f, 0f),
        westRotation = Quaternion.Euler(90f, 270f, 0f);

    GameTile GrowPathTo(GameTile neighbor, Direction direction) {
        if (!HasPath || neighbor == null || neighbor.HasPath) {
            return null;
        }
        neighbor.distance = distance + 1;
        neighbor.nextOnPath = this;
        neighbor.ExitPoint =
            (neighbor.transform.localPosition + transform.localPosition) * 0.5f;
        neighbor.PathDirection = direction;

        return neighbor.Content.BlocksPath ? null : neighbor;
    }
    public static void MakeEastWestNeighbors(GameTile east, GameTile west) {
        Debug.Assert(
            west.east == null && east.west == null, "Redefined neighbors!"
        );
        west.east = east;
        east.west = west;
    }

    public static void MakeNorthSouthNeighbors(GameTile north, GameTile south) {
        Debug.Assert(
            south.north == null && north.south == null, "Redefined neighbors!"
        );
        south.north = north;
        north.south = south;
    }

    public void ClearPath() {
        distance = int.MaxValue;
        nextOnPath = null;
    }
    public void ShowPath() {
        if (distance == 0) {
            arrow.gameObject.SetActive(false);
            return;
        }
        arrow.gameObject.SetActive(true);
        arrow.localRotation =
            nextOnPath == north ? northRotation :
            nextOnPath == east ? eastRotation :
            nextOnPath == south ? southRotation :
            westRotation;
    }
    public void HidePath() {
        arrow.gameObject.SetActive(false);
    }
    public void BecomeDestination() {
        distance = 0;
        ExitPoint = transform.localPosition;
        nextOnPath = null;
    }


}
