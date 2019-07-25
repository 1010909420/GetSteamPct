using Model.Public;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace Model.Entity
{
    public class Activity : BaseEntity<Activity>
    {
        public int id { get; set; }
        public int goodsId { get; set; }
        [ForeignKey("goodsId")]
        public Goods goods { get; set; }
        public int pct { get; set; }
        public decimal finalPrice { get; set; }
        public DateTime createTime { get; set; }
        public int status { get; set; }
    }
}
