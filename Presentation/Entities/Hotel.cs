﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Presentation.Entities
{
    public partial class Hotel
    {
        [Key]
        [Column("HotelID")]
        public int HotelId { get; set; }
        [StringLength(100)]
        public string? HotelName { get; set; }
        [StringLength(100)]
        public string? Location { get; set; }
        public int? Stars { get; set; }
        [StringLength(255)]
        public string? Description { get; set; }
        public string? Photo { get; set; }
    }
}