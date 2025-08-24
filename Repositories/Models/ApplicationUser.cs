using Microsoft.AspNetCore.Identity;
using System.Collections.Generic;

namespace SpaceFlow.Repositories.Models
{
    public class ApplicationUser : IdentityUser
    {
        public string? FullName { get; set; }

        // Initialize collections to fix non-nullable warnings
        public virtual ICollection<Booking> Bookings { get; set; } = new List<Booking>();
        public virtual ICollection<Subscription> Subscriptions { get; set; } = new List<Subscription>();
        public virtual ICollection<Payment> Payments { get; set; } = new List<Payment>();
        public virtual ICollection<Notification> Notifications { get; set; } = new List<Notification>();
    }
}