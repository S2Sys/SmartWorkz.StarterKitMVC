using Microsoft.AspNetCore.Mvc;

namespace SmartWorkz.StarterKitMVC.Web.Areas.Admin.Controllers;

[Area("Admin")]
public class CalendarController : Controller
{
    public IActionResult Index() => View();
    
    [HttpGet]
    public IActionResult GetEvents(DateTime start, DateTime end)
    {
        // Sample events - replace with actual data from database
        var events = new List<CalendarEventDto>
        {
            new() { Id = "1", Title = "Team Meeting", Start = DateTime.Today.AddHours(9), End = DateTime.Today.AddHours(10), BackgroundColor = "#6366f1", BorderColor = "#6366f1" },
            new() { Id = "2", Title = "Project Review", Start = DateTime.Today.AddHours(14), End = DateTime.Today.AddHours(15), BackgroundColor = "#10b981", BorderColor = "#10b981" },
            new() { Id = "3", Title = "Client Call", Start = DateTime.Today.AddDays(1).AddHours(11), End = DateTime.Today.AddDays(1).AddHours(12), BackgroundColor = "#f59e0b", BorderColor = "#f59e0b" },
            new() { Id = "4", Title = "Sprint Planning", Start = DateTime.Today.AddDays(2).AddHours(10), End = DateTime.Today.AddDays(2).AddHours(12), BackgroundColor = "#ef4444", BorderColor = "#ef4444" },
            new() { Id = "5", Title = "Code Review", Start = DateTime.Today.AddDays(3).AddHours(15), End = DateTime.Today.AddDays(3).AddHours(16), BackgroundColor = "#8b5cf6", BorderColor = "#8b5cf6" },
            new() { Id = "6", Title = "Training Session", Start = DateTime.Today.AddDays(5).AddHours(9), End = DateTime.Today.AddDays(5).AddHours(17), BackgroundColor = "#06b6d4", BorderColor = "#06b6d4", AllDay = true },
            new() { Id = "7", Title = "Release Day", Start = DateTime.Today.AddDays(7), BackgroundColor = "#ec4899", BorderColor = "#ec4899", AllDay = true },
        };
        
        return Json(events);
    }
    
    [HttpPost]
    public IActionResult CreateEvent([FromBody] EventModel model)
    {
        // TODO: Save to database
        return Json(new { success = true, id = Guid.NewGuid().ToString() });
    }
    
    [HttpPut]
    public IActionResult UpdateEvent([FromBody] EventModel model)
    {
        // TODO: Update in database
        return Json(new { success = true });
    }
    
    [HttpDelete]
    public IActionResult DeleteEvent(string id)
    {
        // TODO: Delete from database
        return Json(new { success = true });
    }
}

public class EventModel
{
    public string? Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public DateTime Start { get; set; }
    public DateTime? End { get; set; }
    public bool AllDay { get; set; }
    public string? BackgroundColor { get; set; }
}

public class CalendarEventDto
{
    public string Id { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public DateTime Start { get; set; }
    public DateTime? End { get; set; }
    public bool AllDay { get; set; }
    public string? BackgroundColor { get; set; }
    public string? BorderColor { get; set; }
}
