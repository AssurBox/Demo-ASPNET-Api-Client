namespace AssurBox.Samples.API.Insurance.DAL
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("AssurBox.CarGreenCardRequest")]
    public partial class CarGreenCardRequest
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        public DateTime RequestDate { get; set; }

        [Required]
        public string RequestId { get; set; }

        [Required]
        public string RawRequest { get; set; }

        public DateTime? RequestRespondDate { get; set; }

        public string ResponseInfo { get; set; }
    }
}
