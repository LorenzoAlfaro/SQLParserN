using System.Collections.Generic;

namespace SQLParserN.SQL.Models
{
    public class Trail
    {
        //public DataTable Data;
        public string TableName;

        public string SourceSQL;
        public int ID; //ID of the Hits_Table PK
        public List<int> Children;

        public string PK;
        public List<string> FKs;

        public Trail(int id)
        {
            ID = id;
            //Data = new DataTable();
            SourceSQL = "";
            Children = new List<int>();
            PK = "";
            TableName = "";
            FKs = new List<string>();
        }

        public override string ToString()
        {
            return this.SourceSQL;
        }


    }
}
