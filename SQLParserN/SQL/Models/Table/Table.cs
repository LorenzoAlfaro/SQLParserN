using System.Collections.Generic;

namespace SQLParserN.SQL.Models.Table
{
    public class Table
    {
        public string Name;
        public List<Column> columns;
        Table()
        {
            columns = new List<Column>();
        }
    }
}
