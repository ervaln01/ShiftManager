namespace ExportToExcel
{
	using ExportToExcel.DataProcessing;
	using ExportToExcel.Models;

	using System;
	using System.Collections.Generic;
	using System.Globalization;
	using System.Linq;
	using System.Linq.Expressions;

	/// <summary>
	/// Класс, осуществляющий создание таблицы.
	/// </summary>
	public static class TableCreator
	{
		/// <summary>
		/// Формирование таблицы из списка данных.
		/// </summary>
		/// <param name="values"></param>
		/// <param name="showHeader">Флаг показа заголовка.</param>
		/// <param name="sheetName">Название таблицы.</param>
		/// <returns>Экземпляр таблицы.</returns>
		public static Table<T> ToTable<T>(this IEnumerable<T> values, bool showHeader = true, string sheetName = null) => new()
		{
			DataSource = values,
			ShowHeader = showHeader,
			Title = sheetName ?? values.ToList().First().GetType().Name
		};

		/// <summary>
		/// Добавление колонки.
		/// </summary>
		/// <param name="table">Таблица.</param>
		/// <param name="expression">Выражение.</param>
		/// <param name="title">Заголовок.</param>
		/// <param name="dataType">Тип данных.</param>
		/// <param name="format">Формат.</param>
		/// <param name="convertEmptyStringToNull">Конвертировать пустую строку в NULL.</param>
		/// <param name="encodeValue">Флаг показывающий закодировано ли значение???</param>
		/// <param name="nullDisplayText">Дефолтный текст.</param>
		/// <param name="culture">Региональный параметр.</param>
		/// <param name="select">Функция выборки.</param>
		/// <returns>Экземпляр таблицы.</returns>
		public static Table<T> AddColumn<T, TValue>(
			this Table<T> table,
			Expression<Func<T, TValue>> expression = null,
			string title = null,
			Type dataType = null,
			string format = null,
			bool? convertEmptyStringToNull = null,
			bool? encodeValue = null,
			string nullDisplayText = null,
			CultureInfo culture = null,
			Func<T, object> select = null)
		{
			var tableColumn = new Column<T>();
			if (expression != null)
			{
				var emptyMetadata = new Metadata();
				var metadata = emptyMetadata.Create(expression);
				tableColumn.DataType = dataType ?? (Type.GetType(metadata.DataType) ?? typeof(string)) ?? typeof(string);
				tableColumn.Format = format ?? metadata.DisplayFormat;
				tableColumn.Title = title ?? metadata.DisplayName;
				tableColumn.ConvertEmptyStringToNull = convertEmptyStringToNull ?? metadata.ConvertEmptyStringToNull;
				tableColumn.NullDisplayText = nullDisplayText ?? metadata.NullDisplayText;
				tableColumn.EncodeValue = encodeValue ?? metadata.EncodeValue;
			}
			else
			{
				tableColumn.DataType = dataType ?? typeof(string);
				tableColumn.Format = format;
				tableColumn.Title = title;

				if (convertEmptyStringToNull.HasValue) tableColumn.ConvertEmptyStringToNull = convertEmptyStringToNull.Value;

				tableColumn.NullDisplayText = nullDisplayText;
				if (encodeValue.HasValue) tableColumn.EncodeValue = encodeValue.Value;
			}

			if (culture != null) tableColumn.Culture = culture;

			var unCorrectSelectFunction = select == null && expression != null;
			tableColumn.SelectFunction = !unCorrectSelectFunction ? select : obj => expression.Compile()(obj);

			table.Columns.Add(tableColumn);
			return table;
		}

		/// <summary>
		/// Создание таблицы по заданному пути.
		/// </summary>
		/// <param name="table">Таблица.</param>
		/// <param name="path">Путь.</param>
		public static void GenerateTable<T>(this Table<T> table, string path, string name)
		{
			if (table == null) throw new ArgumentNullException(nameof(table));

			if (path == null) throw new ArgumentNullException(nameof(path));

			var generator = new TableGenerator<T>(table);
			generator.Generate(path, name);
		}
	}
}