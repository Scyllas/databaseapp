using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProductCrud
{
    public class EditEnabledArgs
    {
        public bool Edit { get; set; } = false;
        public bool Delete { get; set; } = false;
        public bool Add { get; set; } = false;
        public bool Confirm { get; set; } = false;
        public bool Cancel { get; set; } = false;
        public bool Products { get; set; } = false;
        public bool Barcode { get; set; } = false;
        public bool Suppliers { get; set; } = false;

    }
}
