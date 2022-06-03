using System;
using System.Linq;

namespace IM.Service.Shared.Background;

public  abstract class BackgroundTaskSettings
{
    public string Name { get; set; } = null!;
    public bool IsEnable { get; set; } = true;
    public bool IsOnce { get; set; }

    public DateTime DateStart { get; set; } = DateTime.Now;
    public DateTime? DateStop { get; set; }
    public DayOfWeek[]? WorkDays { get; set; }
    public string? WorkTime { get; set; }

    public bool IsReady(out string info)
    {
        info = string.Empty;
        var now = DateTime.Now;

        if (!IsEnable)
        {
            info = $"задача выключена настройкой '{nameof(IsEnable)}'";
            return false;
        }

        if (DateStart.Date > now.Date)
        {
            info = $"дата старта сервиса '{nameof(DateStart)}' еще не настала";
            return false;
        }

        if (DateStop.HasValue && DateStop.Value < now)
        {
            info = $"дата и время остановки сервиса '{nameof(DateStop)}' уже наступили";
            return false;
        }

        WorkDays ??= new[]
        {
            DayOfWeek.Monday,
            DayOfWeek.Tuesday,
            DayOfWeek.Wednesday,
            DayOfWeek.Thursday,
            DayOfWeek.Friday,
            DayOfWeek.Saturday,
            DayOfWeek.Sunday
        };

        if (!WorkDays.Contains(now.DayOfWeek))
        {
            info = $"текущего дня недели нет в массиве '{nameof(WorkDays)}'";
            return false;
        }

        WorkTime ??= "01:00:00";

        if (!IsOnce)
            return true;

        var time = TimeOnly.Parse(WorkTime);
        var currentTime = TimeOnly.FromDateTime(now);

        if (time.Hour == currentTime.Hour
            && (time.Minute <= 0 || time.Minute == currentTime.Minute)
            && (time.Second <= 0 || time.Second == currentTime.Second))
        {
            WorkTime = time.AddHours(1).ToShortTimeString();
            return true;
        }

        info = $"установленное время выполнения задачи c включенной настройкой '{nameof(IsOnce)}' не совпало";
        return false;
    }
}