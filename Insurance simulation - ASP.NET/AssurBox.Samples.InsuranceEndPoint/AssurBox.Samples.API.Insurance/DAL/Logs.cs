namespace AssurBox.Samples.API.Insurance.DAL
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("Technical.Logs")]
    public partial class Logs
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        public DateTime LogDate { get; set; }

      
        public string Title { get; set; }


        public string Content { get; set; }

      
    }
}
