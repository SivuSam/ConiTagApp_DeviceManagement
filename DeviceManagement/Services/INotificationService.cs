using DeviceManagement.Models;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace DeviceManagement.Services
{
    public interface INotificationService
    {
        Task SendNotificationAsync(string userId, string message, CancellationToken cancellationToken = default);
        Task<List<Notification>> GetUserNotificationsAsync(string userId, CancellationToken cancellationToken = default);
        Task<bool> MarkAsReadAsync(int notificationId, CancellationToken cancellationToken = default);
        Task MarkAllAsReadAsync(string userId, CancellationToken cancellationToken = default);
    }
}
