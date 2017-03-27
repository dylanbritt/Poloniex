using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Poloniex.Core.Domain.Models
{
    public class TaskLoop
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public Guid TaskLoopId { get; set; }

        [Required, MaxLength(32)]
        public string LoopStatus { get; set; }

        public DateTime? LoopStartedDateTime { get; set; }

        [Required]
        public int Interval { get; set; }

        // Foreign key
        [Key, ForeignKey("Task")]
        public Guid TaskId { get; set; }

        // Navigation properites
        public virtual Task Task { get; set; }
    }
}
