using UnityEngine;
using UnityEngine.UI;

namespace TezosSDK.MarketplaceSample.MarketplaceExample.UI
{

	public class FlexibleGridLayout : LayoutGroup
	{
		[SerializeField] private FitType _fitType = FitType.Uniform;
		[SerializeField] [Min(1)] private int _rows;
		[SerializeField] [Min(1)] private int _columns;
		[SerializeField] private Vector2 _cellSize;
		[SerializeField] private Vector2 _spacing;

		[SerializeField] private bool _fitX;
		[SerializeField] private bool _fitY;

		public enum FitType
		{
			Uniform,
			Width,
			Height,
			FixedRows,
			FixedColumns
		}

		/// <summary>
		///     Calculates and updates the placement of grid elements based on parameters
		/// </summary>
		public override void CalculateLayoutInputHorizontal()
		{
			base.CalculateLayoutInputHorizontal();

			if (_fitType == FitType.Uniform || _fitType == FitType.Width || _fitType == FitType.Height)
			{
				var sqrt = Mathf.Sqrt(transform.childCount);
				_rows = Mathf.CeilToInt(sqrt);
				_columns = Mathf.CeilToInt(sqrt);
			}

			if (_fitType == FitType.Width || _fitType == FitType.FixedColumns)
			{
				_rows = Mathf.CeilToInt(transform.childCount / (float)_columns);
			}

			if (_fitType == FitType.Height || _fitType == FitType.FixedRows)
			{
				_columns = Mathf.CeilToInt(transform.childCount / (float)_rows);
			}

			var parentWidth = rectTransform.rect.width;
			var parentHeight = rectTransform.rect.height;

			var cellWidth = parentWidth / _columns - _spacing.x / _columns * (_columns - 1) -
			                padding.left / (float)_columns - padding.right / (float)_columns;

			var cellHeight = parentHeight / _rows - _spacing.y / _rows * (_rows - 1) - padding.top / (float)_rows -
			                 padding.bottom / (float)_rows;

			_cellSize.x = _fitX ? cellWidth : _cellSize.x;
			_cellSize.y = _fitY ? cellHeight : _cellSize.y;

			var columnCount = 0;
			var rowCount = 0;

			for (var i = 0; i < rectChildren.Count; i++)
			{
				rowCount = i / _columns;
				columnCount = i % _columns;

				var item = rectChildren[i];

				var xPos = _cellSize.x * columnCount + _spacing.x * columnCount + padding.left;
				var yPos = _cellSize.y * rowCount + _spacing.y * rowCount + padding.top;

				SetChildAlongAxis(item, 0, xPos, _cellSize.x);
				SetChildAlongAxis(item, 1, yPos, _cellSize.y);
			}
		}

		public override void CalculateLayoutInputVertical()
		{
			//Unused for now
		}

		public override void SetLayoutHorizontal()
		{
			//Unused for now
		}

		public override void SetLayoutVertical()
		{
			//Unused for now
		}
	}

}