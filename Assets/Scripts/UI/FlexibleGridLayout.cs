using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class FlexibleGridLayout : LayoutGroup
{
    public enum FitType
    {
        Uniform,
        Width,
        Height,
        FixedRows,
        FixedColumns
    }

    [SerializeField] private FitType fitType;
    [SerializeField] private Vector2 spacing;
    [SerializeField] private Vector2 cellSize;
    [SerializeField] private int rows;
    [SerializeField] private int columns;
    [SerializeField] private bool fitX;
    [SerializeField] private bool fitY;

    public override void CalculateLayoutInputHorizontal()
    {
        base.CalculateLayoutInputHorizontal();

        if (fitType == FitType.Width || fitType == FitType.Height || fitType == FitType.Uniform)
        {
            fitX = true;
            fitY = true;

            float sqrt = Mathf.Sqrt(transform.childCount);
            rows = Mathf.CeilToInt(sqrt);
            columns = Mathf.CeilToInt(sqrt);
        }

        if (fitType == FitType.Width || fitType == FitType.FixedColumns)
            rows = Mathf.CeilToInt(transform.childCount / (float)columns);
        if (fitType == FitType.Height || fitType == FitType.FixedRows)
            columns = Mathf.CeilToInt(transform.childCount / (float)rows);

        float parentWidth = rectTransform.rect.width;
        float parentHeight = rectTransform.rect.height;

        float cellWidth = (parentWidth / (float)columns) - (spacing.x / (float)columns * 2) - (padding.left / (float)columns) - (padding.right / (float)columns);
        float cellHeight = (parentHeight / (float)rows) - (spacing.y / (float)rows * 2) - (padding.top / (float)columns) - (padding.bottom / (float)rows);

        cellSize = new Vector2(fitX ? cellWidth : cellSize.x, fitY ? cellHeight : cellSize.y);

        int columnCount, rowCount;

        for (int i = 0; i < rectChildren.Count; i++)
        {
            rowCount = i / columns;
            columnCount = i % columns;

            float xPos = (cellSize.x * columnCount) + (spacing.x * columnCount) + padding.left;
            float yPos = (cellSize.y * rowCount) + (spacing.y * rowCount) + padding.top;

            SetChildAlongAxis(rectChildren[i], 0, xPos, cellSize.x);
            SetChildAlongAxis(rectChildren[i], 1, yPos, cellSize.y);
        }
    }

    public override void CalculateLayoutInputVertical() { }

    public override void SetLayoutHorizontal() { }

    public override void SetLayoutVertical() { }
}

