namespace G1.health.Shared.Utilities.Common;

public class AppointmentConstants
{
    public static string appointment_sku_code = "APPOINTMENT";
    public static string appointment_cancelled = "Cancelled";
    public static long? appointment_code = 4;
    public static string sku_code = "DEFAULT_SKU_CODE";

    public static class AppointmentStatusCodeConsts
    {
        public const int Booked = 0;
        public const int CheckedIn = 1;
        public const int Completed = 3;
        public const int Cancelled = 4;
    }
}
