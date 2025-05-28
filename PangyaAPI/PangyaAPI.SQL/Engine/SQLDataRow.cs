using System.Data;

namespace PangyaAPI.SQL.Engine
{
    public class SQLDataRow : System.Data.DataRow
    {
        protected internal SQLDataRow(DataRowBuilder builder) : base(builder)
        {
            //CheckNull();
        }
    }
}
