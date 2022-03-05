namespace ExportToExcel.DataProcessing
{
	using DocumentFormat.OpenXml;
	using DocumentFormat.OpenXml.Spreadsheet;

	using ExportToExcel.Models;

	using System;
	using System.Collections.Generic;
	using System.Globalization;
	using System.Linq;
	using System.Text;

	/// <summary>
	/// Класс, осуществляющий заполнение таблицы данными.
	/// </summary>
	public class DataAppender<T>
	{
		/// <summary>
		/// Таблица.
		/// </summary>
		private readonly Table<T> _table;

		/// <summary>
		/// Словарь стилей.
		/// </summary>
		private readonly Dictionary<Column<T>, uint> _styleIndex;

		/// <summary>
		/// Конструктор класса <see cref="DataAppender{T}"/>.
		/// </summary>
		/// <param name="table">Таблица.</param>
		/// <param name="styleIndex">Словарь стилей.</param>
		public DataAppender(Table<T> table, Dictionary<Column<T>, uint> styleIndex)
		{
			_table = table;
			_styleIndex = styleIndex;
		}

		/// <summary>
		/// Добавление заголовков в первую строку таблицы.
		/// </summary>
		/// <param name="sheetData">Данные.</param>
		/// <param name="rowIndex">Индекс строки.</param>
		/// <returns>Кортеж.</returns>
		public Row AppendHeaders(SheetData sheetData, uint rowIndex)
		{
			var row = new Row { RowIndex = new UInt32Value(rowIndex + 1) };

			uint columnIndex = 0;
			_table.Columns.ToList().ForEach(column =>
			{
				var cell = new Cell
				{
					CellReference = GetColumnIndex(columnIndex) + row.RowIndex,
					DataType = CellValues.InlineString
				};

				cell.AppendChild(new InlineString(new Text(string.Format(CultureInfo.InvariantCulture, "{0}", column.Title))));
				cell.StyleIndex = 2u;
				row.AppendChild(cell);
				columnIndex++;
			});

			sheetData.AppendChild(row);
			return row;
		}

		/// <summary>
		/// Добавление данных в строку.
		/// </summary>
		/// <param name="sheetData">Данные.</param>
		/// <param name="rowIndex">Индекс строки.</param>
		/// <param name="value">Значение.</param>
		/// <returns>Кортеж.</returns>
		public Row AppendRow(SheetData sheetData, uint rowIndex, T value)
		{
			var row = new Row { RowIndex = new UInt32Value(rowIndex + 1) };

			uint columnIndex = 0;
			_table.Columns.ToList().ForEach(column => AppendCell(row, row.RowIndex, columnIndex++, column, value));

			sheetData.AppendChild(row);
			return row;
		}

		/// <summary>
		/// Заполнение ячейки.
		/// </summary>
		/// <param name="row">Строка</param>
		/// <param name="rowIndex">Индекс строки.</param>
		/// <param name="columnIndex">Индекс колонки.</param>
		/// <param name="column">Колонка.</param>
		/// <param name="value">Значение.</param>
		/// <returns>Ячейка.</returns>
		private Cell AppendCell(Row row, uint rowIndex, uint columnIndex, Column<T> column, T value)
		{
			if (value == null) return null;

			var displayValue = column.GetValue(value);
			if (displayValue == null) return null;

			var cell = new Cell() { CellReference = GetColumnIndex(columnIndex) + rowIndex };
			var datatype = column.DataType == null ? GetDataType(displayValue.GetType()) : GetDataType(column.DataType);

			switch (datatype)
			{
				case CellValues.Boolean:
					var boolean = Convert.ToBoolean(displayValue, column.Culture);
					cell.DataType = CellValues.Boolean;
					cell.CellValue = new CellValue(Convert.ToInt32(boolean));
					break;

				case CellValues.Number:
					var number = Convert.ToString(displayValue, column.Culture);
					cell.DataType = CellValues.Number;
					cell.CellValue = new CellValue(number);
					break;

				case CellValues.String:
					var formula = Convert.ToString(displayValue, column.Culture);
					cell.DataType = CellValues.InlineString;
					cell.CellFormula = new CellFormula(formula);
					break;

				case CellValues.InlineString:
					var text = Convert.ToString(displayValue, column.Culture);
					cell.DataType = CellValues.InlineString;
					cell.AppendChild(new InlineString(new Text(text)));
					break;

				case CellValues.Date:
					var dateTime = Convert.ToDateTime(displayValue, column.Culture);
					cell.CellValue = new CellValue(dateTime.ToOADate().ToString(CultureInfo.InvariantCulture));
					cell.StyleIndex = 1;
					break;

				default:
					throw new ArgumentOutOfRangeException();
			}

			if (column.Format != null)
				cell.StyleIndex = _styleIndex[column];

			row.AppendChild(cell);
			return cell;
		}

		/// <summary>
		/// Получение символьного индекса по числовому значению.
		/// </summary>
		/// <param name="columnIndex">Индекс колонки.</param>
		/// <returns>Символьный индекс.</returns>
		private static string GetColumnIndex(uint columnIndex)
		{
			var sb = new StringBuilder();

			const string columnNames = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
			while (columnIndex != 0)
			{
				sb.Append(columnNames[(int)(columnIndex % 26)]);
				columnIndex /= 26;
			}

			return sb.ToString();
		}

		/// <summary>
		/// Получение типа данных.
		/// </summary>
		/// <param name="type">Тип.</param>
		/// <returns>Значения ячеек.</returns>
		private static CellValues GetDataType(Type type)
		{
			if (type == null) throw new ArgumentNullException("type");

			var typeCode = Type.GetTypeCode(type);

			return typeCode switch
			{
				TypeCode.Empty or TypeCode.DBNull or TypeCode.Object => CellValues.String,
				TypeCode.Boolean => CellValues.Boolean,
				TypeCode.SByte or TypeCode.Byte or TypeCode.Int16 or TypeCode.UInt16 or TypeCode.Int32 or TypeCode.UInt32
				or TypeCode.Int64 or TypeCode.UInt64 or TypeCode.Single or TypeCode.Double or TypeCode.Decimal => CellValues.Number,
				TypeCode.DateTime => CellValues.Date,
				TypeCode.Char or TypeCode.String => CellValues.InlineString,
				_ => throw new ArgumentOutOfRangeException(),
			};
		}
	}
}