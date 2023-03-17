using System.Collections;
using System.Collections.Generic;
using UnityEngine;
//communication is a must for Piece with Board(like reset this piece etc). Because Board is the main control mechanism.
public class Piece : MonoBehaviour
{
    public Board board { get; private set; }
    public Vector3Int position { get; private set; }
    public Vector3Int[] cells { get; private set; }
    public TetrominoData data { get; private set; }

    public int rotationIndex;

    public float stepDelay = 1f;
    public float lockDelay = 0.5f;

    private float stepTime;
    private float lockTime;

    public void Initialize(Board board, Vector3Int position, TetrominoData data)
    {
        this.board = board;
        this.position = position;
        this.data = data;
        rotationIndex = 0;

        this.stepTime = Time.time + this.stepDelay;
        this.lockTime = 0f;

        if (this.cells == null)
        {
            this.cells = new Vector3Int[data.cells.Length];
        }

        for (int i = 0; i < data.cells.Length; i++)
        {
            cells[i] = (Vector3Int)data.cells[i];
        }
    }

    private void Update()
    {
        this.board.Clear(this);

        this.lockTime += Time.deltaTime;

        if (Input.GetKeyDown(KeyCode.Q))
        {
            Rotate(-1);
        }
        else if (Input.GetKeyDown(KeyCode.E))
        {
            Rotate(1);
        }

        if (Input.GetKeyDown(KeyCode.A))
        {
            Move(Vector2Int.left);
        }else if (Input.GetKeyDown(KeyCode.D))
        {
            Move(Vector2Int.right);
        }else if (Input.GetKeyDown(KeyCode.S))
        {
            Move(Vector2Int.down);
        }

        if (Input.GetKeyDown(KeyCode.Space))
        {
            HardDown();
        }

        if (Time.time > this.stepTime)
        {
            Step();
        }

        this.board.Set(this);
    }

    private void Step()
    {
        this.stepTime = Time.time + this.stepDelay;

        Move(Vector2Int.down);

        if (this.lockTime >= this.lockDelay)
        {
            Lock();
        }
    }

    private void Lock()
    {
        this.board.Set(this);
        this.board.ClearLines();
        this.board.SpawnPiece();
    }

    public void HardDown()
    {
        while (Move(Vector2Int.down))
        {
            continue;
        }
    }

    // its going to be at a new position,
    // so we need to recall Set() function but we can't set it
    // until we've cleared it beforehand because we need to clear
    // where it was previously and then reset it at it's new position
    // So we need Clear() function aswell.
    private bool Move(Vector2Int translation)
    {
        Vector3Int newPosition = this.position;
        newPosition.x += translation.x;
        newPosition.y += translation.y;

        bool valid = this.board.isValidTile(this, newPosition);

        if (valid)
        {
            this.position = newPosition;

            this.lockTime = 0f;
        }

        return valid;
    }

    private void Rotate(int direction)//rotasyon icin matematikten matrix rotasyonunu kullaniyoruz.
    {
        int originalRotation = this.rotationIndex;
        this.rotationIndex += Wrap(this.rotationIndex + direction, 0, 4);

        RotationMatrix(direction);

        if (!TestWallKicks(rotationIndex, direction))
        {
            this.rotationIndex = originalRotation;
            RotationMatrix(-direction);
        }
        
    }

    private void RotationMatrix(int direction)
    {
        for (int i = 0; this.cells.Length > i; i++)
        {
            Vector3 cell = this.cells[i]; //vector3int olmama sebebi super rotation'da i ve o half unit'ten donduruluyor.

            int x, y;

            switch (this.data.tetromino)
            {
                case Tetromino.I:
                case Tetromino.O:
                    cell.x -= 0.5f;//super rotation'da i ve o half unit'ten donduruluyor.
                    cell.y -= 0.5f;
                    //yukariya yuvarlamak icin
                    x = Mathf.CeilToInt((cell.x * Data.RotationMatrix[0] * direction) + (cell.y * Data.RotationMatrix[1] * direction));
                    y = Mathf.CeilToInt((cell.x * Data.RotationMatrix[2] * direction) + (cell.y * Data.RotationMatrix[3] * direction));
                    break;

                default:
                    x = Mathf.RoundToInt((cell.x * Data.RotationMatrix[0] * direction) + (cell.y * Data.RotationMatrix[1] * direction));
                    y = Mathf.RoundToInt((cell.x * Data.RotationMatrix[2] * direction) + (cell.y * Data.RotationMatrix[3] * direction));
                    break;
            }

            this.cells[i] = new Vector3Int(x, y, 0);
        }
    }

    private bool TestWallKicks(int rotationIndex, int rotationDirection)//wallkick parcalarin border disina tasmasini engelliyor.
    {
        int wallKickIndex = GetWallKickIndex(rotationIndex, rotationDirection);

        for (int i = 0; i < this.data.wallKicks.GetLength(1); i++)
        {
            Vector2Int translation = this.data.wallKicks[wallKickIndex, i];

            if (Move(translation))
            {
                return true;
            }
        }

        return false;
    }

    private int GetWallKickIndex(int rotationIndex, int rotationDirection)
    {
        int wallKickIndex = rotationIndex * 2;

        if (rotationDirection < 0)
        {
            wallKickIndex--;
        }

        return Wrap(wallKickIndex, 0, this.data.wallKicks.GetLength(0));
    }


    //bu fonksiyon ornegin 0, 1, 2, 3 sayilari arasinda surekli dolasacagiz diyelim.
    //Ancak 0'a -1 eklersek olmayan bir degere ulasmaya calisiriz. Bu yuzden de sorun ortaya cikar.
    //3'e 1 eklersek yine ayni sekilde problem yasanir.
    //Bunu onlemek icin bu fonksiyon kullanilir.
    private int Wrap(int input, int min, int max)
    {
        if (input < min)
        {
            return max - (min - input) % (max - min);
        }
        else
        {
            return min + (input - min) % (max - min);
        }
    }
    
}
