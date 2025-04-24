// See https://aka.ms/new-console-template for more information
using Microsoft.Data.SqlClient;
using ORMasst;
using ORMasst.Test;

var connString = "Data Source=127.0.0.1;User Id=SomeUser;Password=SomePsw";

using (var conn = new SqlConnection(connString))
using (var cmd = conn.CreateCommand())
{
    await conn.OpenAsync();

    cmd.CommandText = "SELECT * FROM car_types WHERE rownum<=10";

    //  with static map
    var statCars = await cmd.LoadEntitiesAsync(new CarMapper()).ToArrayAsync();

    //  with dynamic map
    var mapper = GenericMapper.MakeMapper<Car>(opt => opt
    .MapProperty(o => o.ID).MapField("ID")
    .MapProperty(o => o.CarType).DefaultIfNull(null).MapField("CAR_TYPE")
    .MapProperty(o => o.HorsePower).DefaultIfNull(null).MapField("HORSE_POWER")
    .MapProperty(o => o.WheelBase).DefaultIfNull(null).MapField("WHEEL_BASE")
    .MapProperty(o => o.AxisNum).DefaultIfNull(null).MapField("AXIS_NUM"));

    var dynCars = await cmd.LoadEntitiesAsync(mapper).ToArrayAsync();
}