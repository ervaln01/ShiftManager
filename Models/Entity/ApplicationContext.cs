namespace ShiftManager.Models.Entity
{
	using Microsoft.EntityFrameworkCore;

	/// <summary>
	/// Контекст базы данных.
	/// </summary>
	public class ApplicationContext : DbContext
	{
		/// <summary>
		/// Таблица шаблонов смен.
		/// </summary>
		public DbSet<ShiftTemplate> Templates { get; set; }

		/// <summary>
		/// Таблица смен.
		/// </summary>
		public DbSet<ShiftTimeline> Timelines { get; set; }

		/// <summary>
		/// Подключение к базе данных.
		/// </summary>
		/// <param name="optionsBuilder">API контекста.</param>
		protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder) => optionsBuilder.UseSqlServer(Settings.ConnectionString);

		/// <summary>
		/// Настройка контекста при создании модели.
		/// </summary>
		/// <param name="modelBuilder">Создатель модели.</param>
		protected override void OnModelCreating(ModelBuilder modelBuilder)
		{
			base.OnModelCreating(modelBuilder);
			modelBuilder.Entity<ShiftTimeline>().HasIndex(t => new { t.TargetDate, t.Line, t.IsActive });
		}
	}
}