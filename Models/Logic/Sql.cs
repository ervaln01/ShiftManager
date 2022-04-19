namespace ShiftManager.Models.Logic
{
	using ShiftManager.Models.Data;
	using ShiftManager.Models.Entity;
	using System;
	using System.Collections.Generic;
	using System.Linq;

	public static class Sql
	{
		/// <summary>
		/// Получение деталей определенного шаблона.
		/// </summary>
		/// <param name="option">Выбранная опция.</param>
		/// <returns>Детали шаблона.</returns>
		public static ShiftTemplate GetTemplate(int option)
		{
			using var context = new ApplicationContext();
			return context.Templates.FirstOrDefault(x => x.Id == option);
		}

		public static List<ShiftTimeline> GetTimelines(DateRange range)
		{
			using var context = new ApplicationContext();
			return context.Timelines.Where(x => x.IsActive && x.TargetDate >= range.Before && x.TargetDate <= range.After).ToList();
		}

		/// <summary>
		/// Получение информации о шаблонах.
		/// </summary>
		/// <param name="line">Линия.</param>
		/// <param name="shiftNumber">Номер смены.</param>
		/// <returns>Набор шаблонов по заданным параметрам.</returns>
		public static List<ShiftTemplate> GetTemplates(int line, int shiftNumber)
		{
			using var context = new ApplicationContext();
			return context.Templates.Where(x => x.Line == line && x.ShiftNumber == shiftNumber).ToList();
		}

		/// <summary>
		/// Получение шаблонов смен.
		/// </summary>
		/// <returns>Набор всех шаблонов.</returns>
		public static List<ShiftTemplate> GetTemplates()
		{
			using var context = new ApplicationContext();
			return context.Templates.OrderBy(x => x.Line).ThenBy(x => x.ShiftNumber).ToList();
		}


		/// <summary>
		/// Получение кода корректности заданных смен.
		/// </summary>
		/// <param name="shift1">Шаблон первой смены.</param>
		/// <param name="shift2">Шаблон второй смены.</param>
		/// <param name="shift3">Шаблон третьей смены.</param>
		/// <returns>Код корректности заданных смен.</returns>
		public static int Verify(int shift1, int shift2, int shift3)
		{
			using var context = new ApplicationContext();
			var count = new int[3] { shift1, shift2, shift3 }.Count(x => x > 0);
			if (count == 0 || count == 1) return 0;

			if (count == 2)
			{
				if (shift3 == 0) return CorrectTemplates(shift1, shift2) ? 0 : 1;
				if (shift2 == 0) return CorrectTemplates(shift1, shift3) ? 0 : 1;
				if (shift1 == 0) return CorrectTemplates(shift2, shift3) ? 0 : 1;
			}

			if (count == 3)
			{
				var template1 = context.Templates.FirstOrDefault(x => x.Id == shift1);
				var template2 = context.Templates.FirstOrDefault(x => x.Id == shift2);
				var template3 = context.Templates.FirstOrDefault(x => x.Id == shift3);
				return
					template1.ShiftEnd == template2.ShiftBegin &&
					template2.ShiftEnd == template3.ShiftBegin &&
					template3.ShiftEnd == template1.ShiftBegin.AddDays(1) ? 0 : 1;
			}
			return 2;

			bool CorrectTemplates(int id1, int id2)
			{
				var template1 = context.Templates.FirstOrDefault(x => x.Id == id1);
				var template2 = context.Templates.FirstOrDefault(x => x.Id == id2);
				return template1.ShiftEnd <= template2.ShiftBegin;
			}
		}
	}
}
