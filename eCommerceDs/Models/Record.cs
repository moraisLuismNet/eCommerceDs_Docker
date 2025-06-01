﻿namespace eCommerceDs.Models;

public class Record
{
    public int IdRecord { get; set; }
    public string TitleRecord { get; set; } = null!;
    public int YearOfPublication { get; set; }
    public string? ImageRecord { get; set; }
    public decimal Price { get; set; }
    public int Stock { get; set; }
    public bool Discontinued { get; set; }
    public int GroupId { get; set; }
    public virtual Group Group { get; set; } = null!;
}