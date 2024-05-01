using UnityEngine;
using UnityEngine.UIElements;

// A coordinate converter for vertical grids, where the grid lies on the X-Y plane.
namespace Match3
{
    public class VerticalConverter : CoordinateConverter
    {
        public override Vector3 GridToWorld(int x, int y, float cellSize, Vector3 origin)
        {
            return origin + new Vector3(x * cellSize,
                                        y * cellSize,
                                        0);
        }

        public override Vector3 GridToWorldCenter(int x, int y, float cellSize, Vector3 origin)
        {
            return origin + new Vector3(x * cellSize + cellSize * 0.5f,
                                y * cellSize + cellSize * 0.5f,
                                0);
        }

        public override Vector2Int WorldToGrid(Vector3 worldPosition, float cellSize, Vector3 origin)
        {
            Vector3 gridPosition = (worldPosition - origin) / cellSize;
            var x = Mathf.FloorToInt(gridPosition.x);
            var y = Mathf.FloorToInt(gridPosition.y);
            return new Vector2Int(x, y);
        }
        public override Vector3 Forward => Vector3.forward;
    }
}