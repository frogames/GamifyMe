using System;
using System.Collections.Generic;

namespace GamifyMe.Shared.Dtos
{
    public class ReorderRequestDto
    {
        public List<Guid> OrderedIds { get; set; } = new List<Guid>();
    }
}
