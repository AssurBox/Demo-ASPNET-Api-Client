namespace AssurBox.Samples.API.Insurance.DAL
{
    using System;
    using System.Data.Entity;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Linq;

    public partial class ApiDataContext : DbContext
    {
        public ApiDataContext()
            : base("name=ApiDataConnectionString")
        {
        }

        public virtual DbSet<CarGreenCardRequest> CarGreenCardRequests { get; set; }
        public virtual DbSet<Logs> Logs { get; set; }


        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
        }
    }
}
