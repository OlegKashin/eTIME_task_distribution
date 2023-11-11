using ExcelDataReader;
using System.Data;
using TaskDistribution.BLL.Models;

namespace TaskDistribution.BLL.Helpers
{
    internal static class ExcelFileImporter
    {
        public static IReadOnlyCollection<ImportData> Parse(Stream excelStream)
        {
            var result = new List<ImportData>();
            using (var reader = ExcelReaderFactory.CreateReader(excelStream))
            {
                var data = reader.AsDataSet(new ExcelDataSetConfiguration()
                {
                    ConfigureDataTable = (_) => new ExcelDataTableConfiguration()
                    {
                        UseHeaderRow = true
                    },
                    UseColumnDataType = true,
                    FilterSheet = (tableReader, sheetIndex) => tableReader.VisibleState == "visible",
                });
                foreach (DataRow row in data.Tables[0].Rows)
                {
                    bool? isPointRegistrationOld = PointRegistrationParse(TryChangeType<string>(row.ItemArray[2])!);
                    if (!isPointRegistrationOld.HasValue)
                        continue;

                    bool? isCardAndMaterialSend = CardAndMaterialSendItem(TryChangeType<string>(row.ItemArray[3])!);
                    if (!isCardAndMaterialSend.HasValue)
                        continue;

                    result.Add(new ImportData
                    {
                        PointId = TryChangeType<int>(row.ItemArray[0]),
                        Address = $"гр. Краснодар {TryChangeType<string>(row.ItemArray[1])}",
                        IsPointRegistrationOld = isPointRegistrationOld.Value,
                        IsCardAndMaterialSend = isCardAndMaterialSend.Value,
                        CountDaysSend = TryChangeType<int>(row.ItemArray[4]),
                        CountApprovedApplications = TryChangeType<int>(row.ItemArray[5]),
                        CountCardSent = TryChangeType<int>(row.ItemArray[6]),
                    });
                }
            }
            return result;

            bool? PointRegistrationParse(string data) => data.Trim() switch
            {
                "вчера" => false,
                "давно" => true,
                _ => null
            };
            bool? CardAndMaterialSendItem(string data) => data.Trim() switch
            {
                "нет" => false,
                "да" => true,
                _ => null
            };
        }

        private static T? TryChangeType<T>(object? data)
        {
            try
            {
                return (T)Convert.ChangeType(data, typeof(T))!;
            }
            catch (Exception)
            {
                return default(T);
            }
        }
    }
}
