using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SpaceFlow.Repositories.Models
{
    public class Payment
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string ApplicationUserId { get; set; }
        [ForeignKey("ApplicationUserId")]
        public virtual ApplicationUser ApplicationUser { get; set; }

        // A payment can be for a booking OR a subscription, so these are nullable
        public int? BookingId { get; set; }
        [ForeignKey("BookingId")]
        public virtual Booking? Booking { get; set; }

        public int? SubscriptionId { get; set; }
        [ForeignKey("SubscriptionId")]
        public virtual Subscription? Subscription { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal Amount { get; set; }

        public DateTime PaymentDate { get; set; }
        public string? StripePaymentIntentId { get; set; }
    }
}