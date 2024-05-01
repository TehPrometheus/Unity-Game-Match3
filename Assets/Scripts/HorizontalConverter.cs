using UnityEngine;

namespace Match3
{
    public class HorizontalConverter : CoordinateConverter
    {
        public override Vector3 GridToWorld(int x, int y, float cellSize, Vector3 origin)
        {
            return origin + new Vector3(x * cellSize,
                                        0,
                                        y * cellSize);
        }

        public override Vector3 GridToWorldCenter(int x, int y, float cellSize, Vector3 origin)
        {
            return origin + new Vector3(x * cellSize + cellSize * 0.5f,
                                        0,
                                        y * cellSize + cellSize * 0.5f);
        }

        public override Vector2Int WorldToGrid(Vector3 worldPosition, float cellSize, Vector3 origin)
        {
            Vector3 gridPosition = (worldPosition - origin) / cellSize;
            var x = Mathf.FloorToInt(gridPosition.x);
            var y = Mathf.FloorToInt(gridPosition.z);
            return new Vector2Int(x, y);
        }
        public override Vector3 Forward => -Vector3.up;
    }
}