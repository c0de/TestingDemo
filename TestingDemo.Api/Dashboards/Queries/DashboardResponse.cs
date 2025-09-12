namespace TestingDemo.Api.Dashboards.Queries;

public class DashboardResponse
{
    /// <summary>
    /// Id of the dashboard.
    /// </summary>
    public int Id { get; set; }
    /// <summary>
    /// Name of the dashboard.
    /// </summary>
    public string Name { get; set; }
    /// <summary>
    /// DateTime the dashboard was created.
    /// </summary>
    public DateTime CreatedAt { get; set; }
}
