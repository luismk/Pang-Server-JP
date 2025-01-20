using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PangyaAPI.SQL
{
    public class SQLResponse
    {
     DataTable _data { get; set; }
        public DataRow data { get; set; }
        int next { get; set; }
        public int cols { get; }
        public int count { get; }
        public bool read { get; set; }
        
        public SQLResponse(DataTable collection)
        {
            if (collection !=null)
            {
                Add(collection);
                 read = true;
               
                if (_data.Rows.Count == 1)
                {
                    count = 1;
                }
                if (_data.Rows.Count > 1)
                {
                    count = _data.Rows.Count - 1;
                }               
                cols = _data.Columns.Count;
            }
            else
            {
                read = false;

                count = 0;
                cols =0;
            }
        }

        public SQLResponse()
        {
            read = false;

            count = 0;
            cols = 0;
            next = 0;
        }
        public void ReaderData()
        {
            if (_data != null)
            {
                if (data == null)
                {
                    data = _data.Rows[next];
                }
                else if (data != null)
                {
                    data = _data.Rows[next];
                }
                if (next != count)
                {
                    next++;
                }
                if (next == count)
                {
                    next = 0;
                }
                CheckNull();
            }
        }

        public void CheckNull()
        {
            for (int i = 0; i < data.ItemArray.Length; i++)
            {
                if (data[i] == DBNull.Value)
                {
                    data[i] = "";
                }
            }
        }

        public bool IsNotNull(int idx)
        {
            if (data[idx] == DBNull.Value)
                return true;

            return false;
        }

        public DataTable getData() { return _data; }

        public void Add(DataTable collection)
        {
            _data =(collection);
        }
    }
}
