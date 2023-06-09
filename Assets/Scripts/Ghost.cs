using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class Ghost : MonoBehaviour
{
    public Tile tile;
    public Board board;
    public Piece trackingPiece;

    public Tilemap tilemap { get; private set; }
    public Vector3Int[] cells { get; private set; }
    public Vector3Int position { get; private set; }

    private void Awake()
    {
        this.tilemap = GetComponentInChildren<Tilemap>();
        this.cells = new Vector3Int[4];
    }

    private void LateUpdate()
    {
        Clear();
        Copy();
        Drop();
        Set();
    }

    public void Clear()
    {
        for (int i = 0; i < this.cells.Length; i++)
        {
            Vector3Int tilePosition = this.cells[i] + this.position;
            this.tilemap.SetTile(tilePosition, null);
        }
    }

    public void Copy()
    {
        for (int i = 0; i < this.cells.Length; i++)
        {
            this.cells[i] = trackingPiece.cells[i];
        }
    }

    //eger bu fonksiyonda (Drop()) tracking piece'i clear'lamazsak
    //isvalidposition hep false dondurecektir
    //bu da yanlis calismasina neden olacaktir.
    public void Drop()
    {
        Vector3Int position = this.trackingPiece.position;

        int current = position.y;
        int bottom = -this.board.BoardSize.y / 2 - 1;

        this.board.Clear(this.trackingPiece);

        for (int row = current; row >= bottom; row--)
        {
            position.y = row;

            if (this.board.isValidTile(this.trackingPiece, position))
            {
                this.position = position;
            }else
            {
                break;
            }
        }

        this.board.Set(this.trackingPiece);
    }

    public void Set()
    {
        for (int i = 0; i < this.cells.Length; i++)
        {
            Vector3Int tilePosition = this.cells[i] + this.position;
            this.tilemap.SetTile(tilePosition, this.tile);
        }
    }

}
