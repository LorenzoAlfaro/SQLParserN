using System;
using System.Collections.Generic;
using System.Data.OleDb;
using System.Data.SqlClient;
using System.Data;
using System.IO;

namespace SQLParserN.SQL
{
    public static class getDB
    {
        // These are some general functions to query a DB
        // I don't like embedded SQL in code, so they should be rewritten as SP

        public static void UpdateRow(SqlConnection cnn, string Table, string ID, string field, string value)
        {
            SqlCommand command;
            string sql = "Update " + Table +
                " Set " + field + " = " + value +
                " Where ID  = " + ID;
            command = new SqlCommand(sql, cnn);
            command.ExecuteNonQuery();
        }

        public static void findSimilar(SqlConnection cnn, DataTable dataTable, string TableName, string FieldName, string Query)
        {
            SqlCommand command;
            string sql = "Select * From " + TableName +
                " Where " + FieldName +
                " Like" + "'%" + Query + "%'";
            command = new SqlCommand(sql, cnn);
            SqlDataAdapter da = new SqlDataAdapter(command);
            da.Fill(dataTable);
        }

        public static DataTable findSimilar2(SqlConnection cnn, string TableName, string FieldName, string Query)
        {
            DataTable dataTable = new DataTable();
            SqlCommand command;
            string sql = "Select * From " + TableName +
                " Where " + FieldName +
                " Like" + "'%" + Query + "%'";
            command = new SqlCommand(sql, cnn);
            SqlDataAdapter da = new SqlDataAdapter(command);
            da.Fill(dataTable);
            return dataTable;
        }

        public static DataRow getRow2(SqlConnection cnn, string TableName, int ID)
        {
            DataTable myDataTable = new DataTable();
            SqlCommand command;
            string sql = "Select * From " + TableName + " Where ID = " + ID.ToString();
            command = new SqlCommand(sql, cnn);
            SqlDataAdapter da = new SqlDataAdapter(command);
            da.Fill(myDataTable);
            return myDataTable.Rows[0];
        }

        public static DataTable getRow3(SqlConnection cnn, string TableName, string Filter, string Value)
        {
            DataTable myDataTable = new DataTable();
            SqlCommand command;
            string sql = "Select * From " + TableName + " Where " + Filter + " = " + Value;
            command = new SqlCommand(sql, cnn);
            SqlDataAdapter da = new SqlDataAdapter(command);
            da.Fill(myDataTable);
            return myDataTable;
        }

        public static DataTable getRows(SqlConnection cnn, string TableName, List<int> Keys, string Filter)
        {
            DataTable myDataTable = new DataTable();
            SqlCommand command;


            string concat = "";

            foreach (int key in Keys)
            {
                concat += (Filter + " = " + key.ToString() + " And ");
            }
            string sql = "Select * From " + TableName + " Where " + concat.Remove(concat.Length - 5, 5);

            command = new SqlCommand(sql, cnn);
            SqlDataAdapter da = new SqlDataAdapter(command);
            da.Fill(myDataTable);
            return myDataTable;
        }
        public static SqlDataAdapter getRowsAdapter(SqlConnection cnn, string TableName, List<int> Keys, string Filter)
        {
            DataTable myDataTable = new DataTable();
            SqlCommand command;


            string concat = "";

            foreach (int key in Keys)
            {
                concat += (Filter + " = " + key.ToString() + " Or ");
            }
            string sql = "Select * From " + TableName + " Where " + concat.Remove(concat.Length - 4, 4);

            command = new SqlCommand(sql, cnn);
            SqlDataAdapter da = new SqlDataAdapter(command);
            return da;
        }

        public static void getPrimaryTable(SqlConnection cnn, DataTable dataTable, string TableName)
        {
            SqlCommand command;
            string sql = "Select * From " + TableName;
            command = new SqlCommand(sql, cnn);
            SqlDataAdapter da = new SqlDataAdapter(command);
            da.Fill(dataTable);
        }

        public static DataTable getPrimaryTable3(SqlConnection cnn, string TableName)
        {
            DataTable dataTable = new DataTable();
            SqlCommand command;
            string sql = "Select * From " + TableName;
            command = new SqlCommand(sql, cnn);
            SqlDataAdapter da = new SqlDataAdapter(command);
            da.Fill(dataTable);
            return dataTable;
        }

        public static void getRow(SqlConnection cnn, DataTable dataTable, string TableName, int ID)
        {
            SqlCommand command;
            string sql = "Select * From " + TableName + " Where ID = " + ID.ToString();
            command = new SqlCommand(sql, cnn);
            SqlDataAdapter da = new SqlDataAdapter(command);
            da.Fill(dataTable);
        }

        public static void getPrimaryTable2(OleDbConnection cnn, DataTable dataTable, string TableName)
        {
            OleDbCommand command;
            string sql = "Select * From " + TableName;
            command = new OleDbCommand(sql, cnn);
            OleDbDataAdapter da = new OleDbDataAdapter(command);
            da.MissingSchemaAction = MissingSchemaAction.AddWithKey;
            da.Fill(dataTable);
        }

        public static void getChildTable(SqlConnection cnn, DataTable dataTable, string TableName, int ExternalKey, string ExternalKeyName)
        {
            SqlCommand command;
            string sql;
            sql = "Select * From " + TableName + " Where " + ExternalKeyName + " = " + ExternalKey.ToString();
            command = new SqlCommand(sql, cnn);
            SqlDataAdapter da = new SqlDataAdapter(command);
            da.Fill(dataTable);
        }

        public static DataTable getChildTable2(SqlConnection cnn, string TableName, string ExternalKey, string ExternalKeyName)
        {
            DataTable dataTable = new DataTable();
            SqlCommand command;
            string sql;
            sql = "Select * From " + TableName + " Where " + ExternalKeyName + " = " + ExternalKey;
            command = new SqlCommand(sql, cnn);
            SqlDataAdapter da = new SqlDataAdapter(command);
            da.Fill(dataTable);
            return dataTable;
        }

        // *** SnifferDB *** methods
        public static void resetDBSniffer(SqlConnection cnn)
        {

            SqlCommand command;
            string sql;
            sql = "DELETE FROM [Hits_Table];DBCC CHECKIDENT([Hits_Table], RESEED, 0);DELETE FROM [UpdatedValues_Table];DBCC CHECKIDENT([UpdatedValues_Table], RESEED, 0);";
            command = new SqlCommand(sql, cnn);
            int rows = command.ExecuteNonQuery();
        }

        // This function takes the path of an Index File. The Index file is created when searching the VB6 source code
        // for hooking points, that is any point where the code executes SQL code. All those hooks are recorded in a simple
        // txt file that contains SourceFile (AKA forms, module, class), the line Number, and quote of the code that contains the 
        // hook (.Open, .Execute, etc)
        public static void CreateInjectedIndex_Table(SqlConnection cnn, string path, string Table, string sessionID)
        {
            string[] lines = File.ReadAllLines(path);

            foreach (string line in lines)
            {
                string[] parts = line.Split(new string[] { "<%%%>" }, StringSplitOptions.None);

                SqlCommand command;
                string sql2 = "INSERT into " + Table +
                    " (SourceFileName, LineNumber, CodeSelection, ApplyInject, SearchSession_Table_ID)" + "output INSERTED.ID " +
                    " values(" +
                    "'" + parts[0] + "'" + "," +
                    parts[1] + "," +
                    "'" + parts[2].Replace("'", "") + "'" + "," +
                    "1" + "," +
                    sessionID + ")";

                command = new SqlCommand(sql2, cnn);
                command.ExecuteScalar();
            }

        }

        public static int CreateSearchSession_Table(SqlConnection cnn, string SearchString, string ProjectPath, string Table)
        {
            SqlCommand command;
            string sql2 = "INSERT into " + Table +
                "(SearchString, CodeDirectory)" + "output INSERTED.ID " +
                " values(" +
                "'" + SearchString + "'" + "," +
                "'" + ProjectPath + "'" + ")";
            command = new SqlCommand(sql2, cnn);
            return (int)command.ExecuteScalar(); //when adding records (INSERT), the amount of fields have to match    
        }

        public static void CreateIndexDB(SqlConnection cnn, string ProjectPath, string searchString, string path)
        {
            int SessionID = CreateSearchSession_Table(cnn, searchString, ProjectPath, "SearchSession_Table");

            CreateInjectedIndex_Table(cnn, path, "InjectedIndex_Table", SessionID.ToString());
        }

        public static void BackUpDB(SqlConnection cnn, string DBName, string Path)
        {
            DateTime start = DateTime.Now;
            string date = start.ToShortDateString().Replace('/', '-');
            //Backup Database FormMaker To Disk = 'C:\Users\Public\Documents\FormMaker.bak'

            SqlCommand command;
            string sql = "Backup Database " + DBName +
                " To Disk =  " + "'" + Path + "\\" + DBName + "_" + date + ".bak" + "'";
            command = new SqlCommand(sql, cnn);
            command.ExecuteNonQuery();
        }

    }
}
