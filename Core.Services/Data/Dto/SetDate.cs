using System;
using System.Collections.Generic;
using System.Text;

namespace Core.Services.Data.Dto
{
    public class SetDate
    {
        public string SessionDate { get; set; }
        public List<Set> Sets { get; set; }
    }
}
