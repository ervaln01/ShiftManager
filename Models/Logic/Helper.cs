namespace ShiftManager.Models.Logic
{
	using ShiftManager.Models.Data;
	using ShiftManager.Models.Entity;

	using System;
	using System.Collections.Generic;
	using System.Linq;

	/// <summary>
	/// Класс расширений обработки данных для облегчения контроллеров.
	/// </summary>
	public static class Helper
	{
		/// <summary>
		/// Получение форматированного времени.
		/// </summary>
		/// <param name="dateTime">Дата.</param>
		/// <returns>Форматированная строка времени.</returns>
		public static string GetTime(this DateTime? dateTime) => dateTime == null ? string.Empty : $"{dateTime.Value.Hour:D2}:{dateTime.Value.Minute:D2}";

		/// <summary>
		/// Получение форматированного времени.
		/// </summary>
		/// <param name="dateTime">Дата.</param>
		/// <returns>Форматированная строка времени.</returns>
		public static string GetTime(this DateTime dateTime) => $"{dateTime.Hour:D2}:{dateTime.Minute:D2}";

		/// <summary>
		/// Получение корректной даты.
		/// </summary>
		/// <param name="datetime">Дата.</param>
		/// <param name="templateDate">Дата из шаблона.</param>
		/// <returns>Корректная дата.</returns>
		public static DateTime GetCorrectDate(this DateTime datetime, DateTime templateDate) =>
			new DateTime(datetime.Year, datetime.Month, datetime.Day, templateDate.Hour, templateDate.Minute, 0).AddDays(templateDate.Day == 2 ? 1 : 0);

		/// <summary>
		/// Получение корректной даты.
		/// </summary>
		/// <param name="datetime">Дата.</param>
		/// <param name="templateDate">Дата из шаблона.</param>
		/// <returns>Корректная дата.</returns>
		public static DateTime? GetCorrectDate(this DateTime datetime, DateTime? templateDate) => !templateDate.HasValue ?
			null :
			new DateTime(datetime.Year, datetime.Month, datetime.Day, templateDate.Value.Hour, templateDate.Value.Minute, 0).AddDays(templateDate.Value.Day == 2 ? 1 : 0);

		/// <summary>
		/// Получение дней со сменами на любой линии.
		/// </summary>
		/// <param name="active">Дни с активными сменами.</param>
		/// <param name="date">Первый день месяца.</param>
		/// <returns>Дни со сменами</returns>
		public static int[] GetShifts(this IEnumerable<ShiftTimeline> active, DateTime date) =>
			active.Where(x => x.TargetDate >= date && x.TargetDate < date.AddMonths(1)).Select(x => x.TargetDate.Day).Distinct().ToArray();

		/// <summary>
		/// Получение описаний смен на дни, где они заданы.
		/// </summary>
		/// <param name="active">Дни с активными сменами.</param>
		/// <param name="date">Первый день месяца.</param>
		/// <returns>Набор описаний заданных смен.</returns>
		public static IEnumerable<string> GetShiftDescriptions(this IEnumerable<ShiftTimeline> active, DateTime date)
		{
			var activeDays = active.Where(x => x.TargetDate >= date && x.TargetDate < date.AddMonths(1)).OrderBy(x => x.TargetDate).ToList();
			return activeDays.Select(x => x.TargetDate).Distinct().Select(day =>
			{
				var rf = $"{string.Join(" ", activeDays.Where(x => x.TargetDate == day && x.Line == 1).Select(x => $"{x.ShiftBegin.GetTime()}-{x.ShiftEnd.GetTime()}"))}";
				var wm = $"{string.Join(" ", activeDays.Where(x => x.TargetDate == day && x.Line == 2).Select(x => $"{x.ShiftBegin.GetTime()}-{x.ShiftEnd.GetTime()}"))}";
				return (!string.IsNullOrEmpty(rf) && !string.IsNullOrEmpty(wm)) ? $"RF {rf}\nWM {wm}" : !string.IsNullOrEmpty(rf) ? $"RF {rf}" : $"WM {wm}";
			});
		}

		/// <summary>
		/// Формирование таблицы смен на заданный период.
		/// </summary>
		/// <param name="timelines">Последовательность смен.</param>
		/// <param name="range">Период дат.</param>
		/// <returns>Последовательность динамически задаваемых строк таблицы.</returns>
		public static IEnumerable<TableModel> GetTable(this IEnumerable<ShiftTimeline> timelines, DateRange range)
		{
			for (var date = range.before; date <= range.after; date = date.AddDays(1))
			{
				var response = new TableRow(date);
				var dayShifts = timelines.Where(x => x.TargetDate == date);

				foreach (var shift in dayShifts.Where(x => x.Line == 1).OrderBy(shift => shift.ShiftNumber))
				{
					response.RF[shift.ShiftNumber - 1] = $"{shift.ShiftBegin.GetTime()}-{shift.ShiftEnd.GetTime()}";
				}

				foreach (var shift in dayShifts.Where(x => x.Line == 2).OrderBy(shift => shift.ShiftNumber))
				{
					response.WM[shift.ShiftNumber - 1] = $"{shift.ShiftBegin.GetTime()}-{shift.ShiftEnd.GetTime()}";
				}

				yield return new ()
				{
					date = response.Date.ToString("yyyy-MM-dd"),
					rf1 = response.RF[0],
					rf2 = response.RF[1],
					rf3 = response.RF[2],
					wm1 = response.WM[0],
					wm2 = response.WM[1],
					wm3 = response.WM[2]
				}; ;
			}
		}

		/// <summary>
		/// Получение строки таблицы отображения смен.
		/// </summary>
		/// <param name="timelines">Последовательность смен на текущую дату.</param>
		/// <param name="currentDate">Текущая дата.</param>
		/// <returns>Строка таблицы отображения смен.</returns>
		public static TableRow GetTargetDay(this IEnumerable<ShiftTimeline> timelines, DateTime currentDate)
		{
			var response = new TableRow(currentDate);

			for (var shiftNumber = 1; shiftNumber <= 3; shiftNumber++)
			{
				var rf = timelines.FirstOrDefault(x => x.ShiftNumber == shiftNumber && x.Line == 1);
				if (rf != null) response.RF[shiftNumber - 1] = $"{rf.ShiftId}";

				var wm = timelines.FirstOrDefault(x => x.ShiftNumber == shiftNumber && x.Line == 2);
				if (wm != null) response.WM[shiftNumber - 1] = $"{wm.ShiftId}";
			}
			return response;
		}

		/// <summary>
		/// Получение деталей шаблона.
		/// </summary>
		/// <param name="template">Шаблон.</param>
		/// <returns>Развернутая информация текущего шаблона.</returns>
		public static string GetDetails(this ShiftTemplate template) =>
			$"Shift {template.ShiftBegin.GetTime()}-{template.ShiftEnd.GetTime()}; " +
			$"Lunch {template.LunchBegin.GetTime()}-{template.LunchEnd.GetTime()}; " +
			$"Breaks {template.Break1Begin.GetTime()}-{template.Break1End.GetTime()}; " +
			$"{template.Break2Begin.GetTime()}-{template.Break2End.GetTime()}; " +
			$"{template.Break3Begin.GetTime()}-{template.Break3End.GetTime()}";

		/// <summary>
		/// Получения информации о шаблонах смен.
		/// </summary>
		/// <param name="templates">Последовательность шаблонов смен.</param>
		/// <returns>Информация о шаблонах.</returns>
		public static List<Descriptions> GetTemplates(this IEnumerable<ShiftTemplate> templates) =>
			templates.Select(x => new Descriptions() { id = x.Id, description = $"{x.ShiftBegin.GetTime()}-{x.ShiftEnd.GetTime()}" }).ToList();

		/// <summary>
		/// Получение информации о шаблонах.
		/// </summary>
		/// <param name="templates">Последовательность шаблонов смен.</param>
		/// <returns>Последовательность информации о всех шаблонах.</returns>
		public static IEnumerable<dynamic> GetAllTemplates(this IEnumerable<ShiftTemplate> templates) => templates.Select(template => new
		{
			line = template.Line == 1 ? "RF" : "WM",
			number = template.ShiftNumber,
			shift = $"{template.ShiftBegin.GetTime()}-{template.ShiftEnd.GetTime()}",
			lunch = $"{template.LunchBegin.GetTime()}-{template.LunchEnd.GetTime()}",
			break1 = $"{template.Break1Begin.GetTime()}-{template.Break1End.GetTime()}",
			break2 = $"{template.Break2Begin.GetTime()}-{template.Break2End.GetTime()}",
			break3 = $"{template.Break3Begin.GetTime()}-{template.Break3End.GetTime()}",
		} as dynamic);

		/// <summary>
		/// Запись данных в БД.
		/// </summary>
		/// <param name="timelines">Последовательность смен.</param>
		/// <param name="user">Пользователь, добавивший данные.</param>
		/// <param name="currentDate">Текущая дата.</param>
		/// <param name="range">Диапазон дней?</param>
		/// <param name="lastDate">Последняя дата диапазона.</param>
		/// <param name="saturday">Задавать смены по субботам?</param>
		/// <param name="sunday">Задавать смены по воскресеньям?</param>
		public static void Process(this IEnumerable<ShiftTimeline> timelines, string user, DateTime currentDate,
			DailyShifts shifts, bool range,
			DateTime? lastDate = null, bool saturday = false, bool sunday = false)
		{
			using var context = new ApplicationContext();
			var RFShift = shifts.RFShift;
			var WMShift = shifts.WMShift;
			for (var index = 0; index < 3; index++)
			{
				if (shifts.RFActive[index] == null) RFShift[index] = null;
				if (shifts.WMActive[index] == null) WMShift[index] = null;
			}

			foreach (var timeline in timelines)
			{
				timeline.IsActive = false;
				context.Timelines.Update(timeline);
			}

			var dbinteraction = new TimelinesFormation(context);
			var newTimelines = !range ?
				dbinteraction.CreateTimelines(currentDate, RFShift, WMShift, DateTime.Now, user) :
				dbinteraction.CreateTimelines(new DateTime[2] { currentDate, lastDate.Value }, saturday, sunday, RFShift, WMShift, DateTime.Now, user);

			newTimelines.ForEach(item => context.Timelines.Add(item));
			context.SaveChanges();
		}

		/// <summary>
		/// Получение кода корректности заданных смен.
		/// </summary>
		/// <param name="context">Контекст базы данных.</param>
		/// <param name="shift1">Шаблон первой смены.</param>
		/// <param name="shift2">Шаблон второй смены.</param>
		/// <param name="shift3">Шаблон третьей смены.</param>
		/// <returns>Код корректности заданных смен.</returns>
		public static int Verify(this ApplicationContext context, int shift1, int shift2, int shift3)
		{
			var count = new int[3] { shift1, shift2, shift3 }.Count(x => x > 0);
			if (count == 0 || count == 1) return 0;

			if (count == 2)
			{
				if (shift3 == 0) return CorrectTemplates(shift1, shift2) ? 0 : 1;
				if (shift2 == 0) return CorrectTemplates(shift3, shift1) ? 0 : 1;
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

			bool CorrectTemplates(int shift1, int shift2)
			{
				var template1 = context.Templates.FirstOrDefault(x => x.Id == shift1);
				var template2 = context.Templates.FirstOrDefault(x => x.Id == shift2);
				return template1.ShiftEnd <= template2.ShiftBegin;
			}
		}
	}
}