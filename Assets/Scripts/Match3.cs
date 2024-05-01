using UnityEngine;
using Unity.VisualScripting;
using DG.Tweening;
using System.Collections;

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

        GridSystem2D<GridObject<Gem>> grid;

        InputReader inputReader;
        Vector2Int selectedGem = Vector2Int.one * -1;
        private void Awake()
        {
            inputReader = GetComponent<InputReader>();
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
            var gridPos = grid.GetXY(Camera.main.ScreenToWorldPoint(inputReader.Selected));

            if (!IsValidPosition(gridPos) || IsEmptyPosition(gridPos))
                return;

            if (selectedGem == gridPos)
                DeselectGem();
            else if (selectedGem == Vector2Int.one * -1)
                SelectGem(gridPos);
            else
                StartCoroutine(RunGameLoop(selectedGem, gridPos));
        }

        private bool IsEmptyPosition(Vector2Int gridPos) => grid.GetValue(gridPos.x, gridPos.y) == null;

        // The curly brace syntax below is called property pattern matching in C#
        private bool IsValidPosition(Vector2Int gridPos) => gridPos is {x: >= 0 , y: >= 0} && gridPos.x < width && gridPos.y < height;

        private void SelectGem(Vector2Int gridPos) => selectedGem = gridPos;

        private void DeselectGem() => selectedGem = new Vector2Int(-1, -1);

        IEnumerator RunGameLoop(Vector2Int gridPosA, Vector2Int gridPosB)
        {
            yield return StartCoroutine(SwapGems(gridPosA, gridPosB));
            DeselectGem();
            yield return null;
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