using Model.Public;
using System;
using System.Collections.Generic;
using System.Text;

namespace Model.Entity
{
    public class Goods : BaseEntity<Goods>
    {
        public int id { get; set; }
        public String name { get; set; }
        public decimal price { get; set; }
        public String tag { get; set; }
        public String imgURI { get; set; }
        public DateTime createTime { get; set; }
        public int status { get; set; }
    }
}
