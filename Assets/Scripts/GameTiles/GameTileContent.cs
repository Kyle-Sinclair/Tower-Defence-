using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[SelectionBase]

public class GameTileContent : MonoBehaviour {

    [SerializeField]
    GameTileContentType type = default;

    public GameTileContentType Type => type;
    public bool BlocksPath =>
        Type == GameTileContentType.Wall || Type == GameTileContentType.Tower;
    public GameTileContentFactory originFactory;

    public GameTileContentFactory OriginFactory {

        get => originFactory;
        set {
            Debug.Assert(originFactory == null, "Redefined origin factory");
            originFactory = value;
        }
    }

    public virtual void GameUpdate() { }

    public void Recycle() {
        originFactory.Reclaim(this);
    }
}
