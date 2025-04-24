using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace ORMasst.Test
{
    internal enum CarType
    {
        NASCAR,
        StationWagon,
        Berlina,
        Van,
    }

    internal class Car
    {
        public string? ID { get; set; }
        public CarType? CarType { get; set; }
        public long? HorsePower { get; set; }
        public double? WheelBase { get; set; }
        public int? AxisNum { get; set; }
    }

    internal class CarMapper : DataMapper<Car>
    {
        public CarMapper()
        {
            base.MapProperty(o => o.ID).MapField("ID")
                .MapProperty(o => o.CarType).DefaultIfNull(null).MapField("CAR_TYPE")
                .MapProperty(o => o.HorsePower).DefaultIfNull(null).MapField("HORSE_POWER")
                .MapProperty(o => o.WheelBase).DefaultIfNull(null).MapField("WHEEL_BASE")
                .MapProperty(o => o.AxisNum).DefaultIfNull(null).MapField("AXIS_NUM");
        }
    }
}
