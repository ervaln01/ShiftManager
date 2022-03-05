namespace ExportToExcel.Models
{
	using System;
	using System.Globalization;

	/// <summary>
	/// Класс, представляющий собой модель колонки таблицы.
	/// </summary>
	/// <typeparam name="T">Тип колонки.</typeparam>
	public class Column<T>
	{
		/// <summary>
		/// Формат колонки.
		/// </summary>
		public string Format { get; set; }

		/// <summary>
		/// Тип данных.
		/// </summary>
		public Type DataType { get; set; }

		/// <summary>
		/// Название колонки.
		/// </summary>
		public string Title { get; set; }

		/// <summary>
		/// Функция выборки.
		/// </summary>
		public Func<T, object> SelectFunction { get; set; }

		/// <summary>
		/// Региональные параметры.
		/// </summary>
		public IFormatProvider Culture { get; set; }

		/// <summary>
		/// Флаг показывающий закодировано ли значение???
		/// </summary>
		public bool EncodeValue { get; set; }

		/// <summary>
		/// Флаг, показывающий нужно ли конвертировать пустую строку в NULL (аннотация).
		/// </summary>
		public bool ConvertEmptyStringToNull { get; set; }

		/// <summary>
		/// Текст, показываемый по умолчанию (аннотация).
		/// </summary>
		public string NullDisplayText { get; set; }

		/// <summary>
		/// Конструктор класса <see cref="Column{T}"/>.
		/// </summary>
		public Column() => Culture = CultureInfo.CurrentCulture;

		/// <summary>
		/// Получает значение объекта.
		/// </summary>
		/// <param name="obj"></param>
		/// <returns></returns>
		public object GetValue(T obj)
		{
			try
			{
				if (SelectFunction == null) return null;

				var value = SelectFunction(obj);
				if (value == null) return NullDisplayText;

				return ConvertEmptyStringToNull && value is string && string.IsNullOrEmpty(value as string) ? NullDisplayText : value;
			}
			catch
			{
				return null;
			}
		}
	}
}