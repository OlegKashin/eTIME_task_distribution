using System.Security.Cryptography;

namespace TaskDistribution.BLL.Models
{
    internal record ImportData
    {
        public required int PointId { get; init; }
        public required string Address { get; init; }
        public required bool IsPointRegistrationOld { get; init; }
        public required bool IsCardAndMaterialSend { get; init; }
        public required int CountDaysSend { get; init; }
        public required int CountApprovedApplications { get; init; }
        public required int CountCardSent { get; init; }


        public short Priority => true switch
        {
            _ when (CountDaysSend > 7 && CountApprovedApplications > 0) || CountDaysSend > 14 => 3,
            _ when CountCardSent > 0 && ((float)CountCardSent / CountApprovedApplications) < 0.5f  => 2,
            _ when !IsPointRegistrationOld || !IsCardAndMaterialSend => 1,
            _ => 0
        };

        //Время в формате минут
        public int Time => Priority switch
        {
            3 => 240,
            2 => 120,
            1 => 90,
            _ => 0
        };

        public string Description => Priority switch
        {
            3 => "Выезд на точку для стимулирования выдач",
            2 => "Обучение агента",
            1 => "Доставка карт и материалов",
            _ => string.Empty   
        };
    }
}
