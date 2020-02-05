using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace PortFreight.Data.Entities
{
    [Bind(include: "Token")]
    public class ApiKey
    {
        [Key]
        public string Id { get; set; }

        public string Token { get; set; }
        public string Source { get; set; }
    }
}
