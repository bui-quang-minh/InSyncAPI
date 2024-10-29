namespace InSyncAPI.Helpers
{
    public class VietNamTimeHelper
    {
        public static DateTime GetDateTimeVietNam()
        {
            return DateTime.UtcNow.AddHours(7);
        }

    }
}
