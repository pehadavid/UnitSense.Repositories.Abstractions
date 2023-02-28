using System;
using System.Threading.Tasks;
using MessagePack;

namespace PehaCorp.Repositories.Abstractions.Filters
{
    [MessagePackObject]
    public class RawFilter
    {
        [Key(20)] public virtual int Page { get; set; }

        [Key(21)] public virtual int Nb { get; set; }

        public static int DefaultPage = 1;
        public static int DefaultNb = 10;

        public RawFilter()
        {
        }

        public RawFilter(RawFilter rawFilter)
        {
            this.CopyPropertiesFrom(rawFilter);
            if (rawFilter.Nb < 1)
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