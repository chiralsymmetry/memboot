using Dapper;
using System.Data;

namespace MemBoot.DataAccess.Sqlite
{
    internal class GuidTypeHandler : SqlMapper.TypeHandler<Guid>
    {
        public override Guid Parse(object value)
        {
            if (value is byte[] bytes)
            {
                return new Guid(bytes);
            }
            else
            {
                throw new ArgumentException("Invalid GUID value.");
            }
        }

        public override void SetValue(IDbDataParameter parameter, Guid value)
        {
            parameter.Value = value.ToByteArray();
        }
    }
}
