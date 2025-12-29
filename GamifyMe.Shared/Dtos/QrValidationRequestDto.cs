using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using GamifyMe.Shared.Models;

namespace GamifyMe.Shared.Dtos
{
    public class QrValidationRequestDto
    {
        public string Content { get; set; } = string.Empty;
        public ValidationMethod? Method { get; set; }
    }
}
