using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DERMSCommon.SCADACommon
{
    public class ScadaDB
    {
        public ScadaDB()
        {
            SqlConnection con = new SqlConnection("Data Source=(localdb)\\MSSQLLocalDB;Initial Catalog=SCADA;Integrated Security=True;Connect Timeout=30;Encrypt=False;TrustServerCertificate=False;ApplicationIntent=ReadWrite;MultiSubnetFailover=False");
            con.Open();

            string sql = @"DELETE FROM dbo.Day;";
            SqlCommand cmd = new SqlCommand(sql, con);
            cmd.ExecuteNonQuery();
            string sql1 = @"DELETE FROM dbo.Month;";
            SqlCommand cmd1 = new SqlCommand(sql1, con);
            cmd1.ExecuteNonQuery();
            string sql2 = @"DELETE FROM dbo.Year;";
            SqlCommand cmd2 = new SqlCommand(sql2, con);
            cmd2.ExecuteNonQuery();
            con.Close();
        }
        public double MinProductionPerHour(int hour, int day, Dictionary<Tuple<long, DateTime>, DERMSCommon.SCADACommon.CollectItem> dayItems, long key)//u kom satu u toku dana je minimalna vrednost ovog dera
        {
            double minPerHour = double.MaxValue;
            foreach (var d in dayItems)
            {
                if (d.Key.Item2.DayOfYear.Equals(day) && d.Key.Item1.Equals(key) && d.Value.P < minPerHour)
                    minPerHour = d.Value.P;
            }

            return minPerHour;
        }

        public double MaxProductionPerHour(int hour, int day, Dictionary<Tuple<long, DateTime>, DERMSCommon.SCADACommon.CollectItem> collectItems, long key)
        {
            double maxPerHour = double.MinValue;
            foreach (var d in collectItems)
            {
                if (d.Key.Item2.DayOfYear.Equals(day) && d.Key.Item1.Equals(key) && d.Value.P > maxPerHour)
                    maxPerHour = d.Value.P;
            }

            return maxPerHour;
        }

        public double AvgProductionPerHour(int hour, int day, Dictionary<Tuple<long, DateTime>, DERMSCommon.SCADACommon.CollectItem> collectItems, long key)
        {
            int counter = 0;
            double sumPerHour = 0;
            foreach (var d in collectItems)
            {
                if (d.Key.Item2.DayOfYear.Equals(day) && d.Key.Item1.Equals(key))
                {
                    counter++;
                    sumPerHour += d.Value.P;
                }
            }

            return sumPerHour / counter;
        }

        //private double MinProductionPerHour(int hour, int day, Dictionary<Tuple<long, DateTime>, DERMSCommon.SCADACommon.CollectItem> dayItems)
        //{
        //    double minPerHour = double.MaxValue;
        //    foreach (var d in dayItems)
        //    {
        //        if (d.Key.Item2.DayOfYear.Equals(day) && d.Key.Item2.Hour.Equals(hour) && d.Value.P < minPerHour)
        //            minPerHour = d.Value.P;
        //    }

        //    return minPerHour;
        //}

        //private double MaxProductionPerHour(int hour, int day, Dictionary<Tuple<long, DateTime>, DERMSCommon.SCADACommon.CollectItem> dayItems)
        //{
        //    double maxPerHour = double.MinValue;
        //    foreach (var d in dayItems)
        //    {
        //        if (d.Key.Item2.DayOfYear.Equals(day) && d.Key.Item2.Hour.Equals(hour) && d.Value.P > maxPerHour)
        //            maxPerHour = d.Value.P;
        //    }

        //    return maxPerHour;
        //}

        //private double AvgProductionPerHour(int hour, int day, Dictionary<Tuple<long, DateTime>, DERMSCommon.SCADACommon.CollectItem> dayItems)
        //{
        //    int counter = 0;
        //    double sumPerHour = 0;
        //    foreach (var d in dayItems)
        //    {
        //        if (d.Key.Item2.DayOfYear.Equals(day) && d.Key.Item2.Hour.Equals(hour))
        //        {
        //            counter++;
        //            sumPerHour += d.Value.P;
        //        }
        //    }

        //    return sumPerHour / counter;
        //}

        public Dictionary<Tuple<long, DateTime>, DERMSCommon.SCADACommon.CollectItem> ConvertDataPoints(List<DataPoint> pointTypeToConfiguration)
        {
            Dictionary<Tuple<long, DateTime>, DERMSCommon.SCADACommon.CollectItem> collectItems = new Dictionary<Tuple<long, DateTime>, DERMSCommon.SCADACommon.CollectItem>();
            Tuple<long, DateTime> key = null;
            DERMSCommon.SCADACommon.CollectItem item = null;
            foreach (var dataPoint in pointTypeToConfiguration)
            {
                if (dataPoint.Name == "Aqusition")
                {
                    item = new DERMSCommon.SCADACommon.CollectItem(dataPoint.GidGeneratora, dataPoint.RawValue, dataPoint.Timestamp/*, dataPoint.Value.GidGeneratora*/);
                    key = new Tuple<long, DateTime>(item.Gid, item.Timestamp);
                    collectItems.Add(key, item);
                }
            }

            return collectItems;
        }

        public Dictionary<Tuple<long, DateTime>, DERMSCommon.SCADACommon.DayItem> ReadFromCollectTable(string connectionString)
        {
            DERMSCommon.SCADACommon.DayItem itemDay = null;
            Dictionary<Tuple<long, DateTime>, DERMSCommon.SCADACommon.DayItem> dayItems = new Dictionary<Tuple<long, DateTime>, DERMSCommon.SCADACommon.DayItem>();
            Tuple<long, DateTime> key = null;
            Dictionary<Tuple<long, DateTime>, DERMSCommon.SCADACommon.CollectItem> collectItemsData = new Dictionary<Tuple<long, DateTime>, DERMSCommon.SCADACommon.CollectItem>();
            using (System.Data.SqlClient.SqlConnection _con = new System.Data.SqlClient.SqlConnection(connectionString))
            {

                using (System.Data.SqlClient.SqlCommand cmd = new System.Data.SqlClient.SqlCommand("SELECT Timestamp, Gid, Production FROM dbo.Collect", _con))
                {
                    _con.Open();
                    using (System.Data.SqlClient.SqlDataReader reader = cmd.ExecuteReader())
                    {
                        // Check is the reader has any rows at all before starting to read.
                        if (reader.HasRows)
                        {
                            // Read advances to the next row.
                            while (reader.Read())
                            {
                                DERMSCommon.SCADACommon.CollectItem c = new DERMSCommon.SCADACommon.CollectItem();
                                // To avoid unexpected bugs access columns by name.
                                try
                                {
                                    c.Timestamp = reader.GetDateTime(reader.GetOrdinal("Timestamp"));
                                    c.Gid = reader.GetInt64(reader.GetOrdinal("Gid"));
                                    c.P = reader.GetDouble(reader.GetOrdinal("Production"));
                                    key = new Tuple<long, DateTime>(c.Gid, c.Timestamp);

                                    collectItemsData.Add(key, c);
                                }
                                catch (Exception e)
                                { }
                            }
                        }
                    }

                    _con.Close();
                }
            }
                        
            foreach (var d in collectItemsData)
            {
                //itemDay = new DERMSCommon.SCADACommon.DayItem(d.Key.Item1, d.Key.Item2.Date.AddHours(d.Key.Item2.Hour), MinProductionPerHour(d.Key.Item2.Hour, d.Key.Item2.DayOfYear, collectItemsData), MaxProductionPerHour(d.Key.Item2.Hour, d.Key.Item2.DayOfYear, collectItemsData), AvgProductionPerHour(d.Key.Item2.Hour, d.Key.Item2.DayOfYear, collectItemsData), 0, 0);
                itemDay = new DERMSCommon.SCADACommon.DayItem(d.Key.Item1, d.Key.Item2.Date.AddHours(d.Key.Item2.Hour), MinProductionPerHour(d.Key.Item2.Hour, d.Key.Item2.DayOfYear, collectItemsData, d.Key.Item1), MaxProductionPerHour(d.Key.Item2.Hour, d.Key.Item2.DayOfYear, collectItemsData, d.Key.Item1), AvgProductionPerHour(d.Key.Item2.Hour, d.Key.Item2.DayOfYear, collectItemsData, d.Key.Item1), 0, d.Value.P);
                key = new Tuple<long, DateTime>(itemDay.Gid, itemDay.Timestamp);
                if (!dayItems.ContainsKey(key))
                    dayItems.Add(key, itemDay);
            }

            return dayItems;
        }


        public Dictionary<Tuple<long, DateTime>, DERMSCommon.SCADACommon.MonthItem> ReadFromDayTable(string connectionString)
        {
            DERMSCommon.SCADACommon.MonthItem itemMonth = null;
            Dictionary<Tuple<long, DateTime>, DERMSCommon.SCADACommon.MonthItem> monthItems = new Dictionary<Tuple<long, DateTime>, DERMSCommon.SCADACommon.MonthItem>();
            Tuple<long, DateTime> key = null;
            Tuple<long, DateTime> keyM = null;
            Dictionary<Tuple<long, DateTime>, DERMSCommon.SCADACommon.DayItem> dayItemsData = new Dictionary<Tuple<long, DateTime>, DERMSCommon.SCADACommon.DayItem>();
            using (System.Data.SqlClient.SqlConnection _con = new System.Data.SqlClient.SqlConnection(connectionString))
            {

                using (System.Data.SqlClient.SqlCommand cmd = new System.Data.SqlClient.SqlCommand("SELECT Timestamp, Gid, E, PMax, PMin, PAvg FROM dbo.Day", _con))
                {
                    _con.Open();
                    using (System.Data.SqlClient.SqlDataReader reader = cmd.ExecuteReader())
                    {
                        // Check is the reader has any rows at all before starting to read.
                        if (reader.HasRows)
                        {
                            // Read advances to the next row.
                            while (reader.Read())
                            {
                                DERMSCommon.SCADACommon.DayItem c = new DERMSCommon.SCADACommon.DayItem();
                                // To avoid unexpected bugs access columns by name.
                                try
                                {
                                    c.E = reader.GetDouble(reader.GetOrdinal("E"));
                                    c.PAvg = reader.GetDouble(reader.GetOrdinal("PAvg"));
                                    c.PMax = reader.GetDouble(reader.GetOrdinal("PMax"));
                                    c.PMin = reader.GetDouble(reader.GetOrdinal("PMin"));
                                    c.Timestamp = reader.GetDateTime(reader.GetOrdinal("Timestamp"));
                                    c.Gid = reader.GetInt64(reader.GetOrdinal("Gid"));
                                    //c.P = reader.GetDouble(reader.GetOrdinal("P"));

                                    key = new Tuple<long, DateTime>(c.Gid, c.Timestamp);
                                    dayItemsData.Add(key, c);
                                }
                                catch (Exception e)
                                { }
                            }
                        }
                    }

                    _con.Close();
                }
            }

            foreach (var d in dayItemsData)
            {
                //itemDay = new DERMSCommon.SCADACommon.DayItem(d.Key.Item1, d.Key.Item2.Date.AddHours(d.Key.Item2.Hour), MinProductionPerHour(d.Key.Item2.Hour, d.Key.Item2.DayOfYear, collectItemsData), MaxProductionPerHour(d.Key.Item2.Hour, d.Key.Item2.DayOfYear, collectItemsData), AvgProductionPerHour(d.Key.Item2.Hour, d.Key.Item2.DayOfYear, collectItemsData), 0, 0);
                itemMonth = new DERMSCommon.SCADACommon.MonthItem(d.Key.Item1, d.Key.Item2.Date, MinProductionPerDay(d.Key.Item2.DayOfYear, d.Key.Item2.Month, dayItemsData, d.Key.Item1), MaxProductionPerDay(d.Key.Item2.DayOfYear, d.Key.Item2.Month, dayItemsData, d.Key.Item1), AvgProductionPerDay(d.Key.Item2.DayOfYear, d.Key.Item2.Month, dayItemsData, d.Key.Item1), 0, d.Value.P);
                keyM = new Tuple<long, DateTime>(itemMonth.Gid, itemMonth.Timestamp);
                if (!monthItems.ContainsKey(keyM))
                    monthItems.Add(keyM, itemMonth);
            }

            return monthItems;
        }

        public double MinProductionPerDay(int day, int month, Dictionary<Tuple<long, DateTime>, DERMSCommon.SCADACommon.DayItem> dayItems, long key)//u kom danu u toku meseca je minimalna vrednost ovog dera
        {
            double minPerDay = double.MaxValue;
            foreach (var d in dayItems)
            {
                if (d.Key.Item2.DayOfYear.Equals(day) && d.Key.Item2.Month.Equals(month) && d.Key.Item1.Equals(key) && d.Value.PMin < minPerDay)
                    minPerDay = d.Value.PMin;
            }

            return minPerDay;
        }

        public double MaxProductionPerDay(int day, int month, Dictionary<Tuple<long, DateTime>, DERMSCommon.SCADACommon.DayItem> dayItems, long key)
        {
            double maxPerDay = double.MinValue;
            foreach (var d in dayItems)
            {
                if (d.Key.Item2.DayOfYear.Equals(day) && d.Key.Item2.Month.Equals(month) && d.Key.Item1.Equals(key) && d.Value.PMax > maxPerDay)
                    maxPerDay = d.Value.PMax;
            }

            return maxPerDay;
        }

        public double AvgProductionPerDay(int day, int month, Dictionary<Tuple<long, DateTime>, DERMSCommon.SCADACommon.DayItem> dayItems, long key)
        {
            int counter = 0;
            double sumPerDay = 0;
            foreach (var d in dayItems)
            {
                if (d.Key.Item2.DayOfYear.Equals(day) && d.Key.Item2.Month.Equals(month) && d.Key.Item1.Equals(key))
                {
                    counter++;
                    sumPerDay += d.Value.PAvg;
                }
            }

            return sumPerDay / counter;
        }

        public Dictionary<Tuple<long, DateTime>, DERMSCommon.SCADACommon.YearItem> ReadFromMonthTable(string connectionString)
        {
            DERMSCommon.SCADACommon.YearItem itemYear = null;
            Dictionary<Tuple<long, DateTime>, DERMSCommon.SCADACommon.YearItem> yearItems = new Dictionary<Tuple<long, DateTime>, DERMSCommon.SCADACommon.YearItem>();
            Tuple<long, DateTime> key = null;
            Tuple<long, DateTime> keyY = null;
            Dictionary<Tuple<long, DateTime>, DERMSCommon.SCADACommon.MonthItem> monthItemsData = new Dictionary<Tuple<long, DateTime>, DERMSCommon.SCADACommon.MonthItem>();
            using (System.Data.SqlClient.SqlConnection _con = new System.Data.SqlClient.SqlConnection(connectionString))
            {

                using (System.Data.SqlClient.SqlCommand cmd = new System.Data.SqlClient.SqlCommand("SELECT Timestamp, Gid, E, PMax, PMin, PAvg FROM dbo.Month", _con))
                {
                    _con.Open();
                    using (System.Data.SqlClient.SqlDataReader reader = cmd.ExecuteReader())
                    {
                        // Check is the reader has any rows at all before starting to read.
                        if (reader.HasRows)
                        {
                            // Read advances to the next row.
                            while (reader.Read())
                            {
                                DERMSCommon.SCADACommon.MonthItem c = new DERMSCommon.SCADACommon.MonthItem();
                                // To avoid unexpected bugs access columns by name.
                                try
                                {
                                    c.E = reader.GetDouble(reader.GetOrdinal("E"));
                                    c.PAvg = reader.GetDouble(reader.GetOrdinal("PAvg"));
                                    c.PMax = reader.GetDouble(reader.GetOrdinal("PMax"));
                                    c.PMin = reader.GetDouble(reader.GetOrdinal("PMin"));
                                    c.Timestamp = reader.GetDateTime(reader.GetOrdinal("Timestamp"));
                                    c.Gid = reader.GetInt64(reader.GetOrdinal("Gid"));
                                    //c.P = reader.GetDouble(reader.GetOrdinal("P"));

                                    key = new Tuple<long, DateTime>(c.Gid, c.Timestamp);
                                    monthItemsData.Add(key, c);
                                }
                                catch (Exception e)
                                { }
                            }
                        }
                    }

                    _con.Close();
                }
            }
            bool ok = false;
            foreach (var d in monthItemsData)
            {
                ok = false;
                //itemDay = new DERMSCommon.SCADACommon.DayItem(d.Key.Item1, d.Key.Item2.Date.AddHours(d.Key.Item2.Hour), MinProductionPerHour(d.Key.Item2.Hour, d.Key.Item2.DayOfYear, collectItemsData), MaxProductionPerHour(d.Key.Item2.Hour, d.Key.Item2.DayOfYear, collectItemsData), AvgProductionPerHour(d.Key.Item2.Hour, d.Key.Item2.DayOfYear, collectItemsData), 0, 0);
                itemYear = new DERMSCommon.SCADACommon.YearItem(d.Key.Item1, d.Key.Item2.Date, MinProductionPerMonth(d.Key.Item2.Month, d.Key.Item2.Year, monthItemsData, d.Key.Item1), MaxProductionPerMonth(d.Key.Item2.Month, d.Key.Item2.Year, monthItemsData, d.Key.Item1), AvgProductionPerMonth(d.Key.Item2.Month, d.Key.Item2.Year, monthItemsData, d.Key.Item1), 0, d.Value.P);
                keyY = new Tuple<long, DateTime>(itemYear.Gid, itemYear.Timestamp);
                if (yearItems.Count > 0)
                {
                    foreach (var y in yearItems)
                    {
                        if (!(y.Key.Item2.Month == keyY.Item2.Month && y.Key.Item1 == keyY.Item1))
                            ok = true;
                        else
                        {
                            ok = false;
                            break;
                        }
                    }
                    if (ok)
                        yearItems.Add(keyY, itemYear);
                }
                else
                    yearItems.Add(keyY, itemYear);
                //if (!yearItems.ContainsKey(keyY))
                //    yearItems.Add(keyY, itemYear);
            }

            return yearItems;
        }

        public double MinProductionPerMonth(int month, int year, Dictionary<Tuple<long, DateTime>, DERMSCommon.SCADACommon.MonthItem> monthItems, long key)//u kom danu u toku meseca je minimalna vrednost ovog dera
        {
            double minPerMonth = double.MaxValue;
            foreach (var d in monthItems)
            {
                if (d.Key.Item2.Month.Equals(month) && d.Key.Item2.Year.Equals(year) && d.Key.Item1.Equals(key) && d.Value.PMin < minPerMonth)
                    minPerMonth = d.Value.PMin;
            }

            return minPerMonth;
        }

        public double MaxProductionPerMonth(int month, int year, Dictionary<Tuple<long, DateTime>, DERMSCommon.SCADACommon.MonthItem> monthItems, long key)
        {
            double maxPerMonth = double.MinValue;
            foreach (var d in monthItems)
            {
                if (d.Key.Item2.Month.Equals(month) && d.Key.Item2.Year.Equals(year) && d.Key.Item1.Equals(key) && d.Value.PMax > maxPerMonth)
                    maxPerMonth = d.Value.PMax;
            }

            return maxPerMonth;
        }

        public double AvgProductionPerMonth(int month, int year, Dictionary<Tuple<long, DateTime>, DERMSCommon.SCADACommon.MonthItem> monthItems, long key)
        {
            int counter = 0;
            double sumPerMonth = 0;
            foreach (var d in monthItems)
            {
                if (d.Key.Item2.Month.Equals(month) && d.Key.Item2.Year.Equals(year) && d.Key.Item1.Equals(key))
                {
                    counter++;
                    sumPerMonth += d.Value.PAvg;
                }
            }

            return sumPerMonth / counter;
        }

        public void InsertInDayTable(Dictionary<Tuple<long, DateTime>, DERMSCommon.SCADACommon.DayItem> dayItems, string query, string connectionString)
        {
            using (System.Data.SqlClient.SqlConnection _con = new System.Data.SqlClient.SqlConnection(connectionString))
            {
                foreach (var day in dayItems)
                {
                    using (System.Data.SqlClient.SqlCommand _cmd = new System.Data.SqlClient.SqlCommand(query, _con))
                    {
                        System.Data.SqlClient.SqlParameter param1 = _cmd.Parameters.Add("@Gid", System.Data.SqlDbType.BigInt);
                        System.Data.SqlClient.SqlParameter param2 = _cmd.Parameters.Add("@Pmin", System.Data.SqlDbType.Float);
                        System.Data.SqlClient.SqlParameter param3 = _cmd.Parameters.Add("@Pmax", System.Data.SqlDbType.Float);
                        System.Data.SqlClient.SqlParameter param4 = _cmd.Parameters.Add("@Pavg", System.Data.SqlDbType.Float);
                        System.Data.SqlClient.SqlParameter param5 = _cmd.Parameters.Add("@E", System.Data.SqlDbType.Float);
                        System.Data.SqlClient.SqlParameter param6 = _cmd.Parameters.Add("@Timestamp", System.Data.SqlDbType.DateTime);
                        //System.Data.SqlClient.SqlParameter param7 = _cmd.Parameters.Add("@P", System.Data.SqlDbType.Float);

                        param1.Value = day.Key.Item1;
                        param2.Value = day.Value.PMin;
                        param3.Value = day.Value.PMax;
                        param4.Value = day.Value.PAvg;
                        param5.Value = day.Value.E;
                        param6.Value = day.Value.Timestamp;
                        //param7.Value = day.Value.P;
                        _con.Open();
                        try
                        {
                            _cmd.ExecuteNonQuery();
                        }
                        catch (Exception e)
                        { }
                        _con.Close();
                    }
                }
            }
        }

        public void InsertInMonthTable(Dictionary<Tuple<long, DateTime>, DERMSCommon.SCADACommon.MonthItem> monthItems, string query, string connectionString)
        {
            using (System.Data.SqlClient.SqlConnection _con = new System.Data.SqlClient.SqlConnection(connectionString))
            {
                foreach (var day in monthItems)
                {
                    using (System.Data.SqlClient.SqlCommand _cmd = new System.Data.SqlClient.SqlCommand(query, _con))
                    {
                        System.Data.SqlClient.SqlParameter param1 = _cmd.Parameters.Add("@Gid", System.Data.SqlDbType.BigInt);
                        System.Data.SqlClient.SqlParameter param2 = _cmd.Parameters.Add("@Pmin", System.Data.SqlDbType.Float);
                        System.Data.SqlClient.SqlParameter param3 = _cmd.Parameters.Add("@Pmax", System.Data.SqlDbType.Float);
                        System.Data.SqlClient.SqlParameter param4 = _cmd.Parameters.Add("@Pavg", System.Data.SqlDbType.Float);
                        System.Data.SqlClient.SqlParameter param5 = _cmd.Parameters.Add("@E", System.Data.SqlDbType.Float);
                        System.Data.SqlClient.SqlParameter param6 = _cmd.Parameters.Add("@Timestamp", System.Data.SqlDbType.DateTime);
                        //System.Data.SqlClient.SqlParameter param7 = _cmd.Parameters.Add("@P", System.Data.SqlDbType.Float);

                        param1.Value = day.Key.Item1;
                        param2.Value = day.Value.PMin;
                        param3.Value = day.Value.PMax;
                        param4.Value = day.Value.PAvg;
                        param5.Value = day.Value.E;
                        param6.Value = day.Value.Timestamp;
                        //param7.Value = day.Value.P;
                        _con.Open();
                        try
                        {
                            _cmd.ExecuteNonQuery();
                        }
                        catch (Exception e)
                        { }
                        _con.Close();
                    }
                }
            }
        }

        public void InsertInYearTable(Dictionary<Tuple<long, DateTime>, DERMSCommon.SCADACommon.YearItem> yearItems, string query, string connectionString)
        {
            using (System.Data.SqlClient.SqlConnection _con = new System.Data.SqlClient.SqlConnection(connectionString))
            {
                foreach (var month in yearItems)
                {
                    using (System.Data.SqlClient.SqlCommand _cmd = new System.Data.SqlClient.SqlCommand(query, _con))
                    {
                        System.Data.SqlClient.SqlParameter param1 = _cmd.Parameters.Add("@Gid", System.Data.SqlDbType.BigInt);
                        System.Data.SqlClient.SqlParameter param2 = _cmd.Parameters.Add("@Pmin", System.Data.SqlDbType.Float);
                        System.Data.SqlClient.SqlParameter param3 = _cmd.Parameters.Add("@Pmax", System.Data.SqlDbType.Float);
                        System.Data.SqlClient.SqlParameter param4 = _cmd.Parameters.Add("@Pavg", System.Data.SqlDbType.Float);
                        System.Data.SqlClient.SqlParameter param5 = _cmd.Parameters.Add("@E", System.Data.SqlDbType.Float);
                        System.Data.SqlClient.SqlParameter param6 = _cmd.Parameters.Add("@Timestamp", System.Data.SqlDbType.DateTime);
                        //System.Data.SqlClient.SqlParameter param7 = _cmd.Parameters.Add("@P", System.Data.SqlDbType.Float);

                        param1.Value = month.Key.Item1;
                        param2.Value = month.Value.PMin;
                        param3.Value = month.Value.PMax;
                        param4.Value = month.Value.PAvg;
                        param5.Value = month.Value.E;
                        param6.Value = month.Value.Timestamp;
                        //param7.Value = month.Value.P;
                        _con.Open();
                        try
                        {
                            _cmd.ExecuteNonQuery();
                        }
                        catch (Exception e)
                        { }
                        _con.Close();
                    }
                }
            }
        }

        public void InsertInCollectTable(Dictionary<Tuple<long, DateTime>, DERMSCommon.SCADACommon.CollectItem> collectItems, string query, string connectionString)
        {
            using (System.Data.SqlClient.SqlConnection _con = new System.Data.SqlClient.SqlConnection("Data Source=(localdb)\\MSSQLLocalDB;Initial Catalog=SCADA;Integrated Security=True;Connect Timeout=30;Encrypt=False;TrustServerCertificate=False;ApplicationIntent=ReadWrite;MultiSubnetFailover=False"))
            {

                foreach (var c in collectItems)
                {
                    using (System.Data.SqlClient.SqlCommand _cmd = new System.Data.SqlClient.SqlCommand(query, _con))
                    {

                        System.Data.SqlClient.SqlParameter param = _cmd.Parameters.Add("@Timestamp", System.Data.SqlDbType.DateTime);
                        System.Data.SqlClient.SqlParameter param1 = _cmd.Parameters.Add("@Gid", System.Data.SqlDbType.BigInt);
                        System.Data.SqlClient.SqlParameter param2 = _cmd.Parameters.Add("@Production", System.Data.SqlDbType.Float);

                        param.Value = c.Value.Timestamp;
                        param1.Value = c.Value.Gid;
                        param2.Value = c.Value.P;
                        _con.Open();
                        try
                        {
                            _cmd.ExecuteNonQuery();
                        }
                        catch (Exception e)
                        { }
                        _con.Close();
                    }
                }
            }
        }
    }
}
