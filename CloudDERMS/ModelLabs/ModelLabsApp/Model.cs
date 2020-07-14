using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ModelLabsApp
{
    public class Model
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public long Offset { get; set; }
        public byte[] Blob { get; set; }
        public Model()
        {
            Blob = new byte[20000];
        }
    }
}
