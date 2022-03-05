namespace ExportToExcel.Models
{
	using System.Collections.Generic;

	/// <summary>
	/// Класс, представляющий собой модель таблицы.
	/// </summary>
	/// <typeparam name="T">Тип таблицы.</typeparam>
	public class Table<T>
	{
		/// <summary>
		/// Список колонок таблицы.
		/// </summary>
		public IList<Column<T>> Columns { get; set; }

		/// <summary>
		/// Флаг, показывающий, нужно ли показывать название таблицы.
		/// </summary>
		public bool ShowHeader { get; set; }

		/// <summary>
		/// Название таблицы.
		/// </summary>
		public string Title { get; set; }

		/// <summary>
		/// Источник данных.
		/// </summary>
		public IEnumerable<T> DataSource { get; set; }

		/// <summary>
		/// Конструктор класса <see cref="Table{T}"/>.
		/// </summary>
		public Table() => Columns = new List<Column<T>>();
	}
}