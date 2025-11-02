using DeviceManagement.Data;
using DeviceManagement.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace DeviceManagement.Services
{
    public class NotificationService : INotificationService
    {
        private readonly ApplicationDbContext _context;

        public NotificationService(ApplicationDbContext context)
        {
            _context = context;
        }

        // Send a new notification to a user
        public async Task SendNotificationAsync(string userId, string message, CancellationToken cancellationToken = default)
        {
            var notification = new Notification
            {
                UserId = userId,
                Message = message,
                CreatedAt = DateTime.UtcNow,
                IsRead = false
            };

            _context.Notifications.Add(notification);
            await _context.SaveChangesAsync(cancellationToken);
        }

        // Get all notifications for a user ordered by newest first
        public async Task<List<Notification>> GetUserNotificationsAsync(string userId, CancellationToken cancellationToken = default)
        {
            return await _context.Notifications
                .Where(n => n.UserId == userId)
                .OrderByDescending(n => n.CreatedAt)
                .ToListAsync(cancellationToken);
        }

        // Mark a notification as read by id
        public async Task<bool> MarkAsReadAsync(int notificationId, CancellationToken cancellationToken = default)
        {
            var notification = await _context.Notifications.FindAsync(new object[] { notificationId }, cancellationToken);
            if (notification == null)
                return false;

            notification.IsRead = true;
            await _context.SaveChangesAsync(cancellationToken);
            return true;
        }

        // Mark all notifications as read for a user
        public async Task MarkAllAsReadAsync(string userId, CancellationToken cancellationToken = default)
        {
            var notifications = await _context.Notifications
                .Where(n => n.UserId == userId && !n.IsRead)
                .ToListAsync(cancellationToken);

            notifications.ForEach(n => n.IsRead = true);
            await _context.SaveChangesAsync(cancellationToken);
        }
    }
}
