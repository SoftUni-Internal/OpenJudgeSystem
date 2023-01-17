namespace OJS.Services.Infrastructure.Implementations
{
    using System;

    public class DatesService : IDatesService
    {
        public DateTime GetUtcNow() => DateTime.UtcNow;

        public DateTime GetMaxValue() => DateTime.MaxValue;
    }
}