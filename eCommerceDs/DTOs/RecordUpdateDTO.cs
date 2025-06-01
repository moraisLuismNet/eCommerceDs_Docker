﻿using eCommerceDs.Validators;

namespace eCommerceDs.DTOs
{
    public class RecordUpdateDTO
    {
        public int IdRecord { get; set; }
        public string TitleRecord { get; set; } = null!;
        public int YearOfPublication { get; set; }

        [WeightFileValidator(MaximumWeightInMegaBytes: 4)]
        [FileTypeValidator(GroupFileType.Image)]
        public IFormFile? Photo { get; set; }

        public decimal Price { get; set; }
        public int Stock { get; set; }
        public bool Discontinued { get; set; }
        public int GroupId { get; set; }
    }
}
