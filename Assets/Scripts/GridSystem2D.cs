using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace Match3
{
    public class GridSystem2D<T>
    {
        int width;
        int height;
        float cellSize;
        Vector3 origin;
        T[,] gridArray; // This is a 2D array containing type T objects. The first coordinate represents the rows, second the columns

        CoordinateConverter coordinateConverter;

        public event Action<int, int, T> OnValueChangeEvent;
        public GridSystem2D(int width, int height, float cellSize, Vector3 origin, CoordinateConverter coordinateConverter, bool debug)
        {
            this.width = width;
            this.height = height;
            this.cellSize = cellSize;
            this.origin = origin;
            this.coordinateConverter = coordinateConverter ?? new VerticalConverter(); // ?? is the null-coalescing operator

            gridArray = new T[width, height];

            if (debug) DrawDebugLines();
        }
        public static GridSystem2D<T> CreateVerticalGrid(int width, int height, float cellSize, Vector3 origin, bool debug = false)
        {
            return new GridSystem2D<T>(width, height, cellSize, origin, new VerticalConverter(), debug);
        }
        public static GridSystem2D<T> CreateHorizontalGrid(int width, int height, float cellSize, Vector3 origin, bool debug = false)
        {
            return new GridSystem2D<T>(width, height, cellSize, origin, new HorizontalConverter(), debug);
        }
        public void SetValue(Vector3 worldPosition, T value)
        {
            Vector2Int pos = coordinateConverter.WorldToGrid(worldPosition, cellSize, origin);
            SetValue(pos.x, pos.y, value);
        }

        public void SetValue(int x, int y, T value)
        {
            if (IsValid(x, y))
            {
                gridArray[x, y] = value;
                OnValueChangeEvent?.Invoke(x, y, value);
            }
        }

        public T GetValue(Vector3 worldPosition)
        {
            Vector2Int pos = GetXY(worldPosition);
            return GetValue(pos.x, pos.y);
        }

        public T GetValue(int x, int y) => IsValid(x, y) ? gridArray[x, y] : default; // default will return the default of whatever T is, eg for int it's 0

        private Vector3 GetWorldPosition(int x, int y) => coordinateConverter.GridToWorld(x, y, cellSize, origin);
        public Vector3 GetWorldPositionCenter(int x, int y) => coordinateConverter.GridToWorldCenter(x, y, cellSize, origin);
        public Vector2Int GetXY(Vector3 worldPosition) => coordinateConverter.WorldToGrid(worldPosition, cellSize, origin);

        private bool IsValid(int x, int y) => x >= 0 && y >= 0 && x < width && y < height;

        private void DrawDebugLines()
        {
            const float duration = 100f;
            var parent = new GameObject("Debugging");

            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    CreateWorldText(parent, x + "," + y, GetWorldPositionCenter(x,y), coordinateConverter.Forward);
                    Debug.DrawLine(GetWorldPosition(x, y), GetWorldPosition(x, y + 1), Color.white, duration);
                    Debug.DrawLine(GetWorldPosition(x, y), GetWorldPosition(x + 1, y), Color.white, duration);
                }
            }

            Debug.DrawLine(GetWorldPosition(0, height), GetWorldPosition(width, height), Color.white, duration);
            Debug.DrawLine(GetWorldPosition(width, 0), GetWorldPosition(width, height), Color.white, duration);
        }

        private TextMeshPro CreateWorldText(GameObject parent, string text, Vector3 position, Vector3 dir,
                                    int fontSize = 2, Color color = default, TextAlignmentOptions textAnchor = TextAlignmentOptions.Center, int sortingOrder = 0)
        {
            GameObject gameObject = new GameObject("DebugText_" + text, typeof(TextMeshPro));
            gameObject.transform.SetParent(parent.transform);
            gameObject.transform.position = position;
            gameObject.transform.forward = dir;

            TextMeshPro textMeshPro = gameObject.GetComponent<TextMeshPro>();
            textMeshPro.text = text;
            textMeshPro.color = color == default ? Color.white : color;
            textMeshPro.fontSize = fontSize;
            textMeshPro.alignment = textAnchor;
            textMeshPro.GetComponent<MeshRenderer>().sortingOrder = sortingOrder;

            return textMeshPro;
        }


    }
}