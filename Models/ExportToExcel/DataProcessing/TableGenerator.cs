namespace ExportToExcel.DataProcessing
{
	using DocumentFormat.OpenXml;
	using DocumentFormat.OpenXml.Packaging;
	using DocumentFormat.OpenXml.Spreadsheet;

	using ExportToExcel.Models;

	using System.Collections.Generic;
	using System.IO;
	using System.Linq;

	/// <summary>
	/// Класс, осуществляющий генерацию таблицы.
	/// </summary>
	public class TableGenerator<T>
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
		/// Конструктор класса <see cref="TableGenerator{T}"/>
		/// </summary>
		/// <param name="table">Таблица.</param>
		public TableGenerator(Table<T> table)
		{
			_table = table;
			_styleIndex = new Dictionary<Column<T>, uint>();
		}

		/// <summary>
		/// Генерирует таблицу по документу из заданного пути.
		/// </summary>
		/// <param name="path">Путь.</param>
		public void Generate(string path, string tName)
		{
			if (!Directory.Exists(path)) Directory.CreateDirectory(path);

			using var document = SpreadsheetDocument.Create($"{path}\\{tName}", SpreadsheetDocumentType.Workbook);
			Generate(document);
		}

		/// <summary>
		/// Генерирует таблицу из заданного документа.
		/// </summary>
		/// <param name="document">Документ хранящий таблицу.</param>
		private void Generate(SpreadsheetDocument document)
		{
			if (_table.Columns.Count == 0) return;

			var part = document.AddWorkbookPart();
			part.Workbook = new Workbook();

			var stylesCreator = new StylesCreator<T>(_table, _styleIndex);
			var stylePart = part.AddNewPart<WorkbookStylesPart>();
			stylePart.Stylesheet = stylesCreator.CreateStylesheet();

			var worksheetPart = part.AddNewPart<WorksheetPart>();
			worksheetPart.Worksheet = new Worksheet();

			var sheets = document.WorkbookPart.Workbook.AppendChild(new Sheets());
			var id = document.WorkbookPart.GetIdOfPart(worksheetPart);
			var sheet = new Sheet { Id = id, SheetId = 1, Name = _table.Title };
			sheets.Append(sheet);

			var sheetData = new SheetData();
			var appender = new DataAppender<T>(_table, _styleIndex);

			uint rowIndex = 0;
			if (_table.ShowHeader) appender.AppendHeaders(sheetData, rowIndex++);

			if (_table.DataSource != null) _table.DataSource.ToList().ForEach(item => appender.AppendRow(sheetData, rowIndex++, item));

			var columns = new Columns();
			columns.Append(new Column() { Min = 1, Max = UInt32Value.FromUInt32((uint)_table.Columns.Count), Width = 20, CustomWidth = true });
			worksheetPart.Worksheet.Append(columns);
			worksheetPart.Worksheet.AppendChild(sheetData);

			part.Workbook.Save();
		}
	}
}