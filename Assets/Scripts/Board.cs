using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEngine.SceneManagement;
using TMPro;

public class Board : MonoBehaviour
{
    public Tilemap tilemap { get; private set; }
    public TextMeshProUGUI scoreText;
    public TextMeshProUGUI highScoreText;
    Piece activePiece;
    public Vector3Int spawnPosition;
    public TetrominoData[] tetrominoDatas;

    public Vector2Int BoardSize = new Vector2Int(10, 20);

    private int score = 0;
    private int highScore;

    public int HighScore
    {
        set
        {
            highScore = value;
            highScoreText.text = "Highscore: " + value.ToString();
        }
    }

    public RectInt Bounds
    {
        get
        {
            Vector2Int position = new Vector2Int(-this.BoardSize.x / 2, -this.BoardSize.y / 2);
            return new RectInt(position, BoardSize);
        }
    }

    private void Awake()
    {
        this.tilemap = GetComponentInChildren<Tilemap>();
        this.activePiece = GetComponentInChildren<Piece>();
        setLatestHighScore();

        for (int i = 0; i < this.tetrominoDatas.Length; i++)
        {
            tetrominoDatas[i].Initialize();
        }
    }

    private void Start()
    {
        SpawnPiece();
    }

    public void SpawnPiece()
    {
        int random = Random.Range(0, tetrominoDatas.Length);
        TetrominoData data = tetrominoDatas[random];

        this.activePiece.Initialize(this, spawnPosition, data);

        if (isValidTile(this.activePiece, this.spawnPosition))
        {
            Set(this.activePiece);
        }else
        {
            GameOver();
        }

        
    }

    private void harderGame(Piece piece)
    {
        if (piece.stepDelay != 0.04)
        {
            piece.stepDelay -= 0.04f;
        }     
    }

    private void GameOver()
    {
        setHighScoreIfGreater();
        this.tilemap.ClearAllTiles();
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
    }

    public void Set(Piece piece)
    {
        for (int i = 0; i < piece.cells.Length; i++)
        {
            Vector3Int tilePosition = piece.cells[i] + piece.position;
            this.tilemap.SetTile(tilePosition, piece.data.tile);
        }
    }

    public void Clear(Piece piece)
    {
        for (int i = 0; i < piece.cells.Length; i++)
        {
            Vector3Int tilePosition = piece.cells[i] + piece.position;
            this.tilemap.SetTile(tilePosition, null);
        }
    }

    public bool isValidTile(Piece piece, Vector3Int position)
    {
        RectInt bounds = this.Bounds;

        for (int i = 0; i < piece.cells.Length; i++)
        {
            Vector3Int tilePosition = piece.cells[i] + position;

            if (!bounds.Contains((Vector2Int)tilePosition))
            {
                return false;
            }

            if (this.tilemap.HasTile(tilePosition))
            {
                return false;
            }
        }

        return true;
    }

    public void ClearLines()
    {
        RectInt bounds = this.Bounds;
        int row = bounds.yMin;

        while (row < bounds.yMax)
        {
            if (isLineFull(row))
            {
                LineClear(row);
                score++;
                this.scoreText.text = "Score: " + score;
                if (score % 3 != 1 && score != 1 && score != 0)
                {
                    harderGame(this.activePiece);
                }
            }
            else
            {
                row++;
            }
        }
    }

    private bool isLineFull(int row)
    {
        RectInt bounds = this.Bounds;

        for (int col = bounds.xMin; col < bounds.xMax; col++)
        {
            Vector3Int position = new Vector3Int(col, row, 0);

            if (!this.tilemap.HasTile(position)) return false;
        }

        return true;
    }

    public void LineClear(int row)
    {
        RectInt bounds = this.Bounds;

        for (int col = bounds.xMin; col < bounds.xMax; col++)
        {
            Vector3Int position = new Vector3Int(col, row, 0);
            this.tilemap.SetTile(position, null);
        }

        while (row < bounds.yMax)
        {
            for (int col = bounds.xMin; col < bounds.xMax; col++)
            {
                Vector3Int position = new Vector3Int(col, row + 1, 0);
                TileBase above = this.tilemap.GetTile(position);

                position = new Vector3Int(col, row, 0);
                this.tilemap.SetTile(position, above);
            }
            row++;
        }
    }

    public void setHighScoreIfGreater()
    {
        if (score > highScore)
        {
            PlayerPrefs.SetInt("highScore", score);
        }
    }

    private void setLatestHighScore()
    {
        HighScore = PlayerPrefs.GetInt("highScore", 0);
    }

}
