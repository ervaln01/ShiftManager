namespace ExportToExcel.DataProcessing
{
	using System;
	using System.ComponentModel.DataAnnotations;
	using System.Linq.Expressions;
	using System.Reflection;
	using System.Text.RegularExpressions;

	/// <summary>
	/// Класс, создающий метаданные.
	/// </summary>
	public class Metadata
	{
		/// <summary>
		/// Флаг скрытия данных.
		/// </summary>
		public bool Hidden { get; set; }

		/// <summary>
		/// Название.
		/// </summary>
		public string Name { get; set; }

		/// <summary>
		/// Демонстрируемое название.
		/// </summary>
		public string DisplayName { get; set; }

		/// <summary>
		/// Короткое название.
		/// </summary>
		public string ShortName { get; set; }

		/// <summary>
		/// Демонстрируемый формат.
		/// </summary>
		public string DisplayFormat { get; set; }

		/// <summary>
		/// Описание.
		/// </summary>
		public string Description { get; set; }

		/// <summary>
		/// Тип данных.
		/// </summary>
		public string DataType { get; set; }

		/// <summary>
		/// Название группы.
		/// </summary>
		public string GroupName { get; set; }

		/// <summary>
		/// Порядок.
		/// </summary>
		public int Order { get; set; }

		/// <summary>
		/// Текст, показываемый по умолчанию (аннотация).
		/// </summary>
		public string NullDisplayText { get; set; }

		/// <summary>
		/// Флаг, показывающий нужно ли конвертировать пустую строку в NULL (аннотация).
		/// </summary>
		public bool ConvertEmptyStringToNull { get; set; }

		/// <summary>
		/// Флаг показывающий закодировано ли значение???
		/// </summary>
		public bool EncodeValue { get; set; }

		/// <summary>
		/// Конструктор класса <see cref="Metadata"/>.
		/// </summary>
		public Metadata() => EncodeValue = true;

		/// <summary>
		/// Создание экземпляра Metadata по заданному выражению.
		/// </summary>
		/// <param name="expression">Выражение.</param>
		/// <returns>Экземпляр Metadata.</returns>
		public Metadata Create<T, TValue>(Expression<Func<T, TValue>> expression) => expression == null ?
			throw new ArgumentNullException(nameof(expression)) :
			expression.Body is MemberExpression memberExpression ? 
				Create(memberExpression.Member) : 
				new Metadata();

		/// <summary>
		/// Создание экземпляра Metadata по заданной информации.
		/// </summary>
		/// <param name="memberInfo">Метаданные.</param>
		/// <returns>Metadata.</returns>
		public Metadata Create(MemberInfo memberInfo)
		{
			if (memberInfo == null) throw new ArgumentNullException(nameof(memberInfo));

			var metadata = new Metadata { Name = memberInfo.Name };

			var displayAttribute = memberInfo.GetCustomAttribute<DisplayAttribute>();
			if (displayAttribute != null)
			{
				metadata.DisplayName = displayAttribute.GetName();
				metadata.ShortName = displayAttribute.GetShortName();
				metadata.GroupName = displayAttribute.GetGroupName();
				metadata.Description = displayAttribute.GetDescription();

				var order = displayAttribute.GetOrder();
				if (order != null) metadata.Order = order.Value;
			}

			if (metadata.DisplayName == null) metadata.DisplayName = SplitColumnName(metadata.Name);

			var dataTypeAttribute = memberInfo.GetCustomAttribute<DataTypeAttribute>();
			if (dataTypeAttribute != null)
			{
				metadata.DataType = dataTypeAttribute.GetDataTypeName();
				Fill(metadata, dataTypeAttribute.DisplayFormat);
			}

			if (metadata.DataType == null) metadata.DataType = SetDataType(memberInfo);

			var displayFormatAttribute = memberInfo.GetCustomAttribute<DisplayFormatAttribute>();
			if (displayFormatAttribute != null) Fill(metadata, displayFormatAttribute);

			var scaffoldColumnAttribute = memberInfo.GetCustomAttribute<ScaffoldColumnAttribute>();
			if (scaffoldColumnAttribute != null) metadata.Hidden = scaffoldColumnAttribute.Scaffold;

			return metadata;
		}

		/// <summary>
		/// Устанавливает тип данных.
		/// </summary>
		/// <param name="memberInfo">Метаданные.</param>
		/// <returns>Тип данных.</returns>
		private static string SetDataType(MemberInfo memberInfo)
		{
			var propertyInfo = memberInfo as PropertyInfo;
			if (propertyInfo != null) return propertyInfo.PropertyType.AssemblyQualifiedName;

			var fieldInfo = memberInfo as FieldInfo;
			return fieldInfo?.FieldType.AssemblyQualifiedName;
		}

		/// <summary>
		/// Разделяет название колонки пробелами.
		/// </summary>
		/// <param name="columnName">Название колонки.</param>
		/// <returns>Строка, разделённая пробелами.</returns>
		private static string SplitColumnName(string columnName) => string.Join(' ', new Regex("[A-Z]+[a-z]*").Matches(columnName));

		/// <summary>
		/// Заполнение Metadata атрибутами.
		/// </summary>
		/// <param name="metadata">Metadata.</param>
		/// <param name="attribute">Аттрибуты.</param>
		private static void Fill(Metadata metadata, DisplayFormatAttribute attribute)
		{
			if (metadata == null) throw new ArgumentNullException(nameof(metadata));
			if (attribute == null) return;

			metadata.DisplayFormat = attribute.DataFormatString;
			metadata.EncodeValue = attribute.HtmlEncode;
			metadata.ConvertEmptyStringToNull = attribute.ConvertEmptyStringToNull;
			metadata.NullDisplayText = attribute.NullDisplayText;
		}
	}
}