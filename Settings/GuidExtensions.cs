namespace EdgePMO.API.Settings
{
    public static class GuidExtensions
    {
        public static Guid? ToGuid(this string? value)
        {
            if (string.IsNullOrWhiteSpace(value))
                return null;

            return Guid.TryParse(value, out var guid) ? guid : null;
        }

        public static Guid ToGuidOrDefault(this string? value, Guid defaultValue = default)
        {
            if (string.IsNullOrWhiteSpace(value))
                return defaultValue;

            return Guid.TryParse(value, out var guid) ? guid : defaultValue;
        }
    }
}
