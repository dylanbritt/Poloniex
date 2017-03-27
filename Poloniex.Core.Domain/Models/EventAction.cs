using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Poloniex.Core.Domain.Models
{
    public class EventAction
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public Guid EventActionId { get; set; }

        [Required]
        public int Priority { get; set; }

        [Required, MaxLength(32)]
        public string EventActionType { get; set; }

        [Required, MaxLength(32)]
        public string EventActionStatus { get; set; }

        [NotMapped]
        public Action<Guid> Action { get; set; }

        // Foreign key
        [ForeignKey("Task")]
        public Guid TaskId { get; set; }

        // Navigation properites
        public virtual Task Task { get; set; }

        public virtual MovingAverageEventAction MovingAverageEventAction { get; set; }
    }
}
