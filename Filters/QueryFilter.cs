using System;
using System.Threading.Tasks;
using MessagePack;

namespace UnitSense.Repositories.Abstractions.Filters
{
    [MessagePackObject]
    public class QueryFilter
    {
        [Key(20)] public virtual int Page { get; set; }

        [Key(21)] public virtual int Nb { get; set; }

        public static int DefaultPage = 1;
        public static int DefaultNb = 10;

        public QueryFilter()
        {
        }

        public QueryFilter(QueryFilter queryFilter)
        {
            this.CopyPropertiesFrom(queryFilter);
            if (queryFilter.Nb < 1 || queryFilter.Nb > 200)
            {
                this.Nb = QueryFilter.DefaultNb;
            }

            if (queryFilter.Page < 1)
                this.Page = DefaultPage;
        }

        public virtual string GetUniqueKey()
        {
            var data = MessagePackSerializer.Serialize(this, MessagePackSerializerOptions.Standard);
            return $"filtered:{BitConverter.ToString(data)}";
        }

        public virtual Task<FilteredDataSetResult<TData>> CreateGenTaskAsync<TDbContext, TData>(TDbContext dbContext)
        {
            throw new NotImplementedException();
        }

        public enum OrderWay
        {
            Asc,
            Desc
        }
    }
}