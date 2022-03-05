namespace ExportToExcel.DataProcessing
{
	using DocumentFormat.OpenXml;
	using DocumentFormat.OpenXml.Spreadsheet;

	using ExportToExcel.Models;

	using System.Collections.Generic;

	/// <summary>
	/// Класс задания стилей таблицы.
	/// </summary>
	public class StylesCreator<T>
	{
		/// <summary>
		/// Тапблица.
		/// </summary>
		private Table<T> _table;

		/// <summary>
		/// Словарь стилей.
		/// </summary>
		private Dictionary<Column<T>, uint> _styleIndex;

		/// <summary>
		/// Конструктор класса <see cref="StylesCreator{T}"/>
		/// </summary>
		/// <param name="table">Таблица.</param>
		/// <param name="styleIndex">Словарь стилей.</param>
		public StylesCreator(Table<T> table, Dictionary<Column<T>, uint> styleIndex)
		{
			_table = table;
			_styleIndex = styleIndex;
		}

		/// <summary>
		/// Создание таблицы стилей.
		/// </summary>
		/// <returns>Таблица стилей.</returns>
		public Stylesheet CreateStylesheet()
		{
			var stylesheet = new Stylesheet { MCAttributes = new MarkupCompatibilityAttributes { Ignorable = "x14ac" } };
			stylesheet.AddNamespaceDeclaration("mc", "http://schemas.openxmlformats.org/markup-compatibility/2006");
			stylesheet.AddNamespaceDeclaration("x14ac", "http://schemas.microsoft.com/office/spreadsheetml/2009/9/ac");

			stylesheet.Borders = CreateBorders();
			stylesheet.CellFormats = CreateCellFormats();
			stylesheet.CellStyleFormats = CreateCellStyleFormats();
			stylesheet.CellStyles = CreateCellStyles();
			stylesheet.DifferentialFormats = CreateDifferentialFormats();
			stylesheet.Fills = CreateFills();
			stylesheet.Fonts = CreateFonts();
			stylesheet.NumberingFormats = CreateNumeringFormats();
			stylesheet.StylesheetExtensionList = CreateStylesheetExtensionList();
			stylesheet.TableStyles = CreateTableStyles();

			return stylesheet;
		}

		/// <summary>
		/// Создание рамок.
		/// </summary>
		/// <returns>Рамки.</returns>
		private static Borders CreateBorders()
		{
			var borders = new Borders { Count = 1 };
			borders.Append(new Border
			{
				LeftBorder = new LeftBorder(),
				RightBorder = new RightBorder(),
				TopBorder = new TopBorder(),
				BottomBorder = new BottomBorder(),
				DiagonalBorder = new DiagonalBorder()
			});

			return borders;
		}

		/// <summary>
		/// Создание форматов ячеек.
		/// </summary>
		/// <returns>Форматы ячеек.</returns>
		private CellFormats CreateCellFormats()
		{
			var cellFormats = new CellFormats { Count = 3 };
			cellFormats.Append(new CellFormat
			{
				NumberFormatId = 0,
				FontId = 0,
				FillId = 0,
				BorderId = 0,
				FormatId = 0,
				Alignment = new Alignment { Horizontal = HorizontalAlignmentValues.Right }
			});

			cellFormats.Append(new CellFormat
			{
				NumberFormatId = 14,
				FontId = 0,
				FillId = 0,
				BorderId = 0,
				FormatId = 0,
				ApplyNumberFormat = true,
				Alignment = new Alignment { Horizontal = HorizontalAlignmentValues.Right }
			});

			cellFormats.Append(new CellFormat
			{
				NumberFormatId = 0,
				FontId = 1,
				FillId = 0,
				BorderId = 0,
				FormatId = 0,
				ApplyFont = true,
				Alignment = new Alignment { Horizontal = HorizontalAlignmentValues.Center }
			});

			for (int index = 0; index < _table.Columns.Count; index++)
			{
				var tableColumn = _table.Columns[index];
				_styleIndex[tableColumn] = cellFormats.Count;
				var cellFormat = new CellFormat { NumberFormatId = 0, FontId = 0, FillId = 0, BorderId = 0, FormatId = 0 };
				if (tableColumn.Format != null)
				{
					cellFormat.NumberFormatId = 164 + (uint)index;
					cellFormat.ApplyNumberFormat = true;
				}

				cellFormats.Append(cellFormat);
				cellFormats.Count++;
			}

			return cellFormats;
		}

		/// <summary>
		/// Создание форматов стилей ячеек.
		/// </summary>
		/// <returns>Форматы стиля ячейки.</returns>
		private static CellStyleFormats CreateCellStyleFormats()
		{
			var cellStyleFormats = new CellStyleFormats { Count = 1 };
			cellStyleFormats.Append(new CellFormat { NumberFormatId = 0, FontId = 0, FillId = 0, BorderId = 0 });
			return cellStyleFormats;
		}

		/// <summary>
		/// Создание стилей ячеек.
		/// </summary>
		/// <returns>Стили ячеек.</returns>
		private static CellStyles CreateCellStyles()
		{
			var cellStyles = new CellStyles { Count = 1 };
			cellStyles.Append(new CellStyle { Name = "Normal", FormatId = 0, BuiltinId = 0 });
			return cellStyles;
		}

		/// <summary>
		/// Создание дифференциальных форматов.
		/// </summary>
		/// <returns>Дифференциальные форматы.</returns>
		private static DifferentialFormats CreateDifferentialFormats() => new() { Count = 0 };

		/// <summary>
		/// Создание заливок.
		/// </summary>
		/// <returns>Заливки.</returns>
		private static Fills CreateFills()
		{
			var fills = new Fills { Count = 1 };
			fills.Append(new Fill(new PatternFill { PatternType = PatternValues.None }));
			return fills;
		}

		/// <summary>
		/// Создание шрифтов.
		/// </summary>
		/// <returns>Шрифты.</returns>
		private static Fonts CreateFonts()
		{
			var fonts = new Fonts { Count = 2, KnownFonts = true };

			// Текст
			fonts.Append(new Font
			{
				FontSize = new FontSize { Val = 11 },
				Color = new Color { Theme = 1 },
				FontName = new FontName { Val = "Calibri" },
				FontFamilyNumbering = new FontFamilyNumbering { Val = 2 },
				FontScheme = new FontScheme { Val = FontSchemeValues.Minor }
			});

			// Заголовки
			fonts.Append(new Font
			{
				Bold = new Bold(),
				FontSize = new FontSize { Val = 11 },
				Color = new Color { Theme = 1 },
				FontName = new FontName { Val = "Calibri" },
				FontFamilyNumbering = new FontFamilyNumbering { Val = 2 },
				FontScheme = new FontScheme { Val = FontSchemeValues.Minor }
			});

			return fonts;
		}

		/// <summary>
		/// Создание форматов нумерации.
		/// </summary>
		/// <returns>Форматы нумерации.</returns>
		private NumberingFormats CreateNumeringFormats()
		{
			var numberingFormats = new NumberingFormats { Count = 0 };
			for (int index = 0; index < _table.Columns.Count; index++)
			{
				var tableColumn = _table.Columns[index];
				if (tableColumn.Format != null)
				{
					numberingFormats.Count++;
					numberingFormats.Append(new NumberingFormat { NumberFormatId = 164 + (uint)index, FormatCode = tableColumn.Format });
				}
			}
			return numberingFormats;
		}

		/// <summary>
		/// Создание списка расширений таблицы стилей.
		/// </summary>
		/// <returns>Список расширений таблицы стилей.</returns>
		private static StylesheetExtensionList CreateStylesheetExtensionList() => new();

		/// <summary>
		/// Создание стилей таблицы.
		/// </summary>
		/// <returns>Стили таблицы.</returns>
		private static TableStyles CreateTableStyles() => new() { Count = 0, DefaultTableStyle = "TableStyleMedium2", DefaultPivotStyle = "PivotStyleLight16" };
	}
}