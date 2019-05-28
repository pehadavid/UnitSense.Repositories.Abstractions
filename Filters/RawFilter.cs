

using ZeroFormatter;

namespace UnitSense.Repositories.Abstractions.Filters
{
    [ZeroFormattable]
    public class RawFilter
    {
        [Index(20)] public virtual int Page { get; set; }

        [Index(21)] public virtual int Nb { get; set; }

        public static int DefaultPage = 1;
        public static int DefaultNb = 10;

        public RawFilter()
        {
        }

        public RawFilter(RawFilter rawFilter)
        {
            this.CopyPropertiesFrom(rawFilter);
            if (rawFilter.Nb < 1 || rawFilter.Nb > 200)
            {
                this.Nb = RawFilter.DefaultNb;
            }

            if (rawFilter.Page < 1)
                this.Page = DefaultPage;
        }

        public enum OrderWay
        {
            Asc,
            Desc
        }
    }
}