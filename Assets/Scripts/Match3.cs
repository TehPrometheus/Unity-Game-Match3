using UnityEngine;
using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.InputSystem;

namespace Match3
{
    public class Match3 : MonoBehaviour
    {
        [SerializeField] int width = 8;
        [SerializeField] int height = 8;
        [SerializeField] float cellSize = 1.0f;
        [SerializeField] Vector3 originPosition = Vector3.zero;
        [SerializeField] bool debug = true;

        [SerializeField] Gem gemPrefab;
        [SerializeField] GemType[] gemTypes;
        [SerializeField] Ease ease = Ease.InQuad;
        [SerializeField] GameObject ExplosionVFX;
        
        AudioManager audioManager;

        GridSystem2D<GridObject<Gem>> grid;

        InputReader inputReader;
        Vector2Int selectedGem = Vector2Int.one * -1;
        private void Awake()
        {
            inputReader = GetComponent<InputReader>();
            audioManager = GetComponent<AudioManager>();
        }
        void Start()
        {
            InitializeGrid();
            inputReader.Fire += OnSelectGem;
        }



        private void OnDestroy()
        {
            inputReader.Fire -= OnSelectGem;
        }

        private void OnSelectGem()
        {
            var gridPos = grid.GetXY(Camera.main.ScreenToWorldPoint(Input.mousePosition));

            if (!IsValidPosition(gridPos) || IsEmptyPosition(gridPos))
                return;

            if (selectedGem == gridPos)
            {
                DeselectGem();
                audioManager.PlayDeselect();
            }
            else if (selectedGem == Vector2Int.one * -1)
            {
                audioManager.PlayClick();
                SelectGem(gridPos);
            }
            else
            {
                audioManager.PlayClick();
                StartCoroutine(RunGameLoop(selectedGem, gridPos));
            }
        }

        private bool IsEmptyPosition(Vector2Int gridPos) => grid.GetValue(gridPos.x, gridPos.y) == null;

        // The curly brace syntax below is called property pattern matching in C#
        private bool IsValidPosition(Vector2Int gridPos) => gridPos is {x: >= 0 , y: >= 0} && gridPos.x < width && gridPos.y < height;

        private void SelectGem(Vector2Int gridPos) => selectedGem = gridPos;

        private void DeselectGem() => selectedGem = new Vector2Int(-1, -1);

        IEnumerator RunGameLoop(Vector2Int gridPosA, Vector2Int gridPosB)
        {
            // TODO: During RunGameLoop no click input allowed

            // Swap the gems
            yield return StartCoroutine(SwapGems(gridPosA, gridPosB));
            
            // Find the matches
            List<Vector2Int> matches = FindMatches();

            // Explode matches
            yield return StartCoroutine(ExplodeGems(matches));

            // Make gems fall
            yield return StartCoroutine(MakeGemsFall());

            // Fill empty spots
            yield return StartCoroutine(FillEmptySpots());

            DeselectGem();
            yield return null;
        }

        IEnumerator MakeGemsFall()
        {
            // TODO: Make this more efficient
            for (var x = 0; x < width; x++)
            {
                for (var y = 0; y < height; y++)
                {
                    if (grid.GetValue(x, y) == null)
                    {
                        for (var i = y + 1; i < height; i++)
                        {
                            if (grid.GetValue(x, i) != null)
                            {
                                var gem = grid.GetValue(x, i).GetValue();
                                grid.SetValue(x, y, grid.GetValue(x, i));
                                grid.SetValue(x, i, null);
                                gem.transform
                                    .DOLocalMove(grid.GetWorldPositionCenter(x, y), 0.5f)
                                    .SetEase(ease);
                                audioManager.PlayWoosh();
                                yield return new WaitForSeconds(0.1f);
                                break;
                            }
                        }
                    }
                }
            }
        }

        IEnumerator FillEmptySpots()
        {
            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    if(grid.GetValue(x,y) == null)
                    {
                        CreateGem(x, y);
                        audioManager.PlayPop();
                        yield return new WaitForSeconds(0.1f);
                    }
                }
            }
        }

        IEnumerator ExplodeGems(List<Vector2Int> matches)
        {
            audioManager.PlayPop();

            foreach (var match in matches)
            {
                var gem = grid.GetValue(match.x, match.y).GetValue();
                grid.SetValue(match.x,match.y, null);

                ExplodeVFX(match);

                gem.transform.DOPunchScale(Vector3.one * 0.1f, 0.1f, 1, 0.5f);

                yield return new WaitForSeconds(0.1f);

                gem.Destroy();
            }


        }

        private void ExplodeVFX(Vector2Int match)
        {
            // TODO: Pool. These are memory intensive.
            var fx = Instantiate(ExplosionVFX, transform);
            fx.transform.position = grid.GetWorldPositionCenter(match.x, match.y);
            Destroy(fx, 0.5f);
        }

        private List<Vector2Int> FindMatches()
        {
            HashSet<Vector2Int> matches = new();

            // horizontal matches
            for (int row = 0; row < height; row++)
            {
                for (int col = 0; col < width-2; col++)
                {
                    var gemA = grid.GetValue(col  , row);
                    var gemB = grid.GetValue(col+1, row);
                    var gemC = grid.GetValue(col+2, row);

                    if (gemA == null || gemB == null || gemC == null) continue;

                    if (IsMatch(gemA, gemB, gemC))
                    {
                        matches.Add(new Vector2Int(col  , row));
                        matches.Add(new Vector2Int(col+1, row));
                        matches.Add(new Vector2Int(col+2, row));
                    }

                }
            }
            
            // vertical matches
            for (int col = 0; col < width; col++)
            {
                for (int row = 0; row < height-2; row++)
                {
                    var gemA = grid.GetValue(col, row    );
                    var gemB = grid.GetValue(col, row + 1);
                    var gemC = grid.GetValue(col, row + 2);

                    if (gemA == null || gemB == null || gemC == null) continue;

                    if (IsMatch(gemA, gemB, gemC))
                    {
                        matches.Add(new Vector2Int(col, row    ));
                        matches.Add(new Vector2Int(col, row + 1));
                        matches.Add(new Vector2Int(col, row + 2));
                    }

                }
            }

            if(matches.Count == 0)
                audioManager.PlayNoMatch();
            else
                audioManager.PlayMatch();

            return new List<Vector2Int>(matches);
        }

        private bool IsMatch(GridObject<Gem> gemA, GridObject<Gem> gemB, GridObject<Gem> gemC)
        {
            return gemA.GetValue().GetGemType() == gemB.GetValue().GetGemType() && gemB.GetValue().GetGemType() == gemC.GetValue().GetGemType();
        }

        IEnumerator SwapGems(Vector2Int gridPosA, Vector2Int gridPosB)
        {
            var gridObjectA = grid.GetValue(gridPosA.x, gridPosA.y);
            var gridObjectB = grid.GetValue(gridPosB.x, gridPosB.y);

            gridObjectA.GetValue().transform
                .DOLocalMove(grid.GetWorldPositionCenter(gridPosB.x, gridPosB.y), 0.5f)
                .SetEase(ease);
            gridObjectB.GetValue().transform
                .DOLocalMove(grid.GetWorldPositionCenter(gridPosA.x, gridPosA.y), 0.5f)
                .SetEase(ease);

            grid.SetValue(gridPosA.x, gridPosA.y, gridObjectB);
            grid.SetValue(gridPosB.x, gridPosB.y, gridObjectA);

            yield return new WaitForSeconds(0.5f);
        }

        void InitializeGrid()
        {
            grid = GridSystem2D<GridObject<Gem>>.CreateVerticalGrid(width, height, cellSize, originPosition, debug);
            for(int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    CreateGem(x,y);

                }
            }
        }

        private void CreateGem(int x, int y)
        {
            var gem = Instantiate(gemPrefab, grid.GetWorldPositionCenter(x, y), Quaternion.identity, transform);
            gem.SetType(gemTypes[Random.Range(0, gemTypes.Length)]);
            var gridObject = new GridObject<Gem>(grid, x, y);
            gridObject.SetValue(gem);
            grid.SetValue(x, y, gridObject);
        }
    }
}