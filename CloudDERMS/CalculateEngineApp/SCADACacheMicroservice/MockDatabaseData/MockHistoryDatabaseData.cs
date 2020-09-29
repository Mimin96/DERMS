using CloudCommon.SCADA.AzureStorage;
using CloudCommon.SCADA.AzureStorage.Entities;
using DERMSCommon.SCADACommon;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SCADACacheMicroservice.MockDatabaseData
{
    public class MockHistoryDatabaseData
    {
        private List<CollectItem> _collectItems;
        private List<DayItem> _dayItems;
        private List<MonthItem> _monthItems;
        private List<YearItem> _yearItems;


        public MockHistoryDatabaseData()
        {
            _collectItems = new List<CollectItem>();
            _dayItems = new List<DayItem>();
            _monthItems = new List<MonthItem>();
            _yearItems = new List<YearItem>();

            SetMockDatabaseData();
        }

        public async Task SetDatabaseData()
        {
            List<CollectItem> collectItemsRange;
            List<DayItem> dayItemsRange;
            List<MonthItem> monthItemsRange;
            List<YearItem> yearItemsRange;

            int count = _collectItems.Count, index = 0, range = 5;


            while (count != 0)
            {
                if (count >= range)
                {
                    collectItemsRange = _collectItems.GetRange(index, range);
                    index += range;
                    count -= range;
                }
                else
                {
                    collectItemsRange = _collectItems.GetRange(index, count);
                    count = 0;
                }

                AzureTableStorage.InsertEntitiesInDB(collectItemsRange, "UseDevelopmentStorage=true;", "CollectItems");
            }

            count = _dayItems.Count;
            index = 0;

            while (count != 0)
            {
                if (count >= range)
                {
                    dayItemsRange = _dayItems.GetRange(index, range);
                    index += range;
                    count -= range;
                }
                else
                {
                    dayItemsRange = _dayItems.GetRange(index, count);
                    count = 0;
                }

                AzureTableStorage.InsertEntitiesInDB(dayItemsRange, "UseDevelopmentStorage=true;", "DayItems");
            }

            count = _monthItems.Count;
            index = 0;

            while (count != 0)
            {
                if (count >= range)
                {
                    monthItemsRange = _monthItems.GetRange(index, range);
                    index += range;
                    count -= range;
                }
                else
                {
                    monthItemsRange = _monthItems.GetRange(index, count);
                    count = 0;
                }

                AzureTableStorage.InsertEntitiesInDB(monthItemsRange, "UseDevelopmentStorage=true;", "MonthItems");
            }

            count = _yearItems.Count;
            index = 0;

            while (count != 0)
            {
                if (count >= range)
                {
                    yearItemsRange = _yearItems.GetRange(index, range);
                    index += range;
                    count -= range;
                }
                else
                {
                    yearItemsRange = _yearItems.GetRange(index, count);
                    count = 0;
                }

                AzureTableStorage.InsertEntitiesInDB(yearItemsRange, "UseDevelopmentStorage=true;", "YearItems");
            }
        }

        private void SetMockDatabaseData()
        {
            Random random = new Random();

            DateTime dateTime = new DateTime(2019, 10, 1);
            int counter = 0;

            do
            {
                for (int i = 0; i < 24; i++)
                {
                    for (int j = 0; j < random.Next(2,4); j++)
                    {
                        dateTime = new DateTime(dateTime.Year, dateTime.Month, dateTime.Day, i, j + 1, j + 5);

                        _collectItems.Add(new CollectItem(21474836481, random.Next(random.Next(0, 15), random.Next(190, 250)), dateTime));//solar
                        _collectItems.Add(new CollectItem(21474836483, random.Next(random.Next(0, 15), random.Next(190, 250)), dateTime));//solar
                        _collectItems.Add(new CollectItem(21474836486, random.Next(random.Next(0, 15), random.Next(190, 250)), dateTime));//solar
                        _collectItems.Add(new CollectItem(21474836482, random.Next(random.Next(0, 15), random.Next(90, 100)), dateTime));//wind
                        _collectItems.Add(new CollectItem(21474836484, random.Next(random.Next(0, 15), random.Next(90, 100)), dateTime));//wind
                        _collectItems.Add(new CollectItem(21474836485, random.Next(random.Next(0, 15), random.Next(90, 100)), dateTime));//wind
                    }
                }

                dateTime = new DateTime(2019, 10, 1);
                counter++;
                dateTime = dateTime.AddDays(counter);

            } while (counter < 370);

            dateTime = new DateTime(2019, 10, 1);
            counter = 0;

            do
            {
                _dayItems.Add(new DayItem(21474836481, dateTime,
                                                        random.Next(random.Next(0, 15), random.Next(90, 110)), random.Next(random.Next(110, 140), random.Next(190, 250)),
                                                        random.Next(random.Next(70, 80), random.Next(110, 130)), 0, 0));//solar
                _dayItems.Add(new DayItem(21474836483, dateTime,
                                                       random.Next(random.Next(0, 15), random.Next(90, 110)), random.Next(random.Next(110, 140), random.Next(190, 250)),
                                                       random.Next(random.Next(70, 80), random.Next(110, 130)), 0, 0));//solar
                _dayItems.Add(new DayItem(21474836486, dateTime,
                                                       random.Next(random.Next(0, 15), random.Next(90, 110)), random.Next(random.Next(110, 140), random.Next(190, 250)),
                                                       random.Next(random.Next(70, 80), random.Next(110, 130)), 0, 0));//solar
                _dayItems.Add(new DayItem(21474836482, dateTime,
                                                       random.Next(random.Next(0, 15), random.Next(30, 40)), random.Next(random.Next(10, 40), random.Next(90, 100)),
                                                       random.Next(random.Next(10, 20), random.Next(85, 90)), 0, 0));//wind
                _dayItems.Add(new DayItem(21474836484, dateTime,
                                                      random.Next(random.Next(0, 15), random.Next(30, 40)), random.Next(random.Next(10, 40), random.Next(90, 100)),
                                                      random.Next(random.Next(10, 20), random.Next(85, 90)), 0, 0));//wind
                _dayItems.Add(new DayItem(21474836485, dateTime,
                                                      random.Next(random.Next(0, 15), random.Next(30, 40)), random.Next(random.Next(10, 40), random.Next(90, 100)),
                                                      random.Next(random.Next(10, 20), random.Next(85, 90)), 0, 0));//wind

                dateTime = dateTime.AddDays(1);
                counter++;
            } while (counter < 370);

            for (int j = 0; j < 2; j++)
            {
                for (int i = 0; i < 9; i++)
                {
                    DateTime dateTimeMonth = new DateTime(2020 - j, ++i, 1);

                    _monthItems.Add(new MonthItem(21474836481, dateTimeMonth,
                                                                random.Next(random.Next(0, 15), random.Next(90, 110)), random.Next(random.Next(110, 140), random.Next(190, 250)),
                                                                random.Next(random.Next(70, 80), random.Next(110, 130)), 0, 0));
                    _monthItems.Add(new MonthItem(21474836483, dateTimeMonth,
                                                                random.Next(random.Next(0, 15), random.Next(90, 110)), random.Next(random.Next(110, 140), random.Next(190, 250)),
                                                                random.Next(random.Next(70, 80), random.Next(110, 130)), 0, 0));
                    _monthItems.Add(new MonthItem(21474836486, dateTimeMonth,
                                                                random.Next(random.Next(0, 15), random.Next(90, 110)), random.Next(random.Next(110, 140), random.Next(190, 250)),
                                                                random.Next(random.Next(70, 80), random.Next(110, 130)), 0, 0));

                    _monthItems.Add(new MonthItem(21474836482, dateTimeMonth,
                                                              random.Next(random.Next(0, 15), random.Next(30, 40)), random.Next(random.Next(10, 40), random.Next(90, 100)),
                                                              random.Next(random.Next(10, 20), random.Next(85, 90)), 0, 0));
                    _monthItems.Add(new MonthItem(21474836484, dateTimeMonth,
                                                              random.Next(random.Next(0, 15), random.Next(30, 40)), random.Next(random.Next(10, 40), random.Next(90, 100)),
                                                              random.Next(random.Next(10, 20), random.Next(85, 90)), 0, 0));
                    _monthItems.Add(new MonthItem(21474836485, dateTimeMonth,
                                                              random.Next(random.Next(0, 15), random.Next(30, 40)), random.Next(random.Next(10, 40), random.Next(90, 100)),
                                                              random.Next(random.Next(10, 20), random.Next(85, 90)), 0, 0));
                }
            }

            for (int i = 0; i < 4; i++)
            {
                DateTime dateTime1 = new DateTime(2020 - i, 1, 1);

                _yearItems.Add(new YearItem(21474836481, dateTime1,
                                                                random.Next(random.Next(0, 15), random.Next(90, 110)), random.Next(random.Next(110, 140), random.Next(190, 250)),
                                                                random.Next(random.Next(70, 80), random.Next(110, 130)), 0, 0));
                _yearItems.Add(new YearItem(21474836483, dateTime1,
                                                                random.Next(random.Next(0, 15), random.Next(90, 110)), random.Next(random.Next(110, 140), random.Next(190, 250)),
                                                                random.Next(random.Next(70, 80), random.Next(110, 130)), 0, 0));
                _yearItems.Add(new YearItem(21474836486, dateTime1,
                                                                random.Next(random.Next(0, 15), random.Next(90, 110)), random.Next(random.Next(110, 140), random.Next(190, 250)),
                                                                random.Next(random.Next(70, 80), random.Next(110, 130)), 0, 0));
                _yearItems.Add(new YearItem(21474836482, dateTime1,
                                                              random.Next(random.Next(0, 15), random.Next(30, 40)), random.Next(random.Next(10, 40), random.Next(90, 100)),
                                                              random.Next(random.Next(10, 20), random.Next(85, 90)), 0, 0));
                _yearItems.Add(new YearItem(21474836484, dateTime1,
                                                              random.Next(random.Next(0, 15), random.Next(30, 40)), random.Next(random.Next(10, 40), random.Next(90, 100)),
                                                              random.Next(random.Next(10, 20), random.Next(85, 90)), 0, 0));
                _yearItems.Add(new YearItem(21474836485, dateTime1,
                                                              random.Next(random.Next(0, 15), random.Next(30, 40)), random.Next(random.Next(10, 40), random.Next(90, 100)),
                                                              random.Next(random.Next(10, 20), random.Next(85, 90)), 0, 0));
            }
        }

    }
}
