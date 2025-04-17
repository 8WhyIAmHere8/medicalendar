using System;
using Microsoft.Maui.Graphics;

namespace medi1.Models
{
    /// <summary>
    /// Represents a calendar entry with its occurrence date and associated display color.
    /// </summary>
    public class EntryModel
    {
        /// <summary>
        /// The date of the entry.
        /// </summary>
        public DateTime Date { get; set; }

        /// <summary>
        /// The color used to represent this entry on the calendar.
        /// </summary>
        public Color Color { get; set; }
    }
}
