using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
