using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace AssurBox.Samples.Client.Garage.Web.Models
{
    public class CredentialsModel
    {
        [Required]
        public string ClientID { get; set; }
        [Required]
        public string ClientSecret { get; set; }
    }
}