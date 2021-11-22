using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using SQLParserN.SQL.Models;
using System.Data.SqlClient;
using System.Data;
using static SQLParserN.SQL.SQLWorld;
using static StringParserN.CharWorld;
using System.Text.RegularExpressions;

namespace SQLParserN.SQL
{
    public static class TrailMethods
    {
        public static TreeNode TrailToTreeView(this Trail myTrail, List<Trail> trails, TreeNode parentNode = null)
        {
            //parentNode.Text = AliasTree.Trunk;

            parentNode = new TreeNode(myTrail.ID.ToString());

            List<TreeNode> middleNode = new List<TreeNode>();

            if (myTrail.Children.Count != 0)
            {
                foreach (int myBranch in myTrail.Children)
                {
                    TreeNode emptyNode = new TreeNode();
                    middleNode.Add(TrailToTreeView(
                        trails.FindAll(item => item.ID == myBranch)[0],
                       trails,
                        //myBranch, 
                        emptyNode));
                }
            }
            else
            {
            }

            foreach (TreeNode myNode in middleNode)
            {
                parentNode.Nodes.Add(myNode);
            }

            return parentNode;
        }

        public static List<Trail> CreateTrails(List<int> IDs, SqlConnection conn)
        {
            List<Trail> Trails = new List<Trail>();

            for (int i = 0; i < IDs.Count; i++)
            {
                Trail myTrail = new Trail(IDs[i]);
                DataRow row = getDB.getRow2(conn, HitsT.Name, IDs[i]);
                myTrail.SourceSQL = row[HitsT.SQL].ToString();
                Trails.Add(myTrail);
            }
            return Trails;
        }

        public static void AnalizeTrails(List<Trail> breadCrumbs, SqlConnection conn)
        {
            char[] arr = { '[', ']' };
            for (int i = 0; i < breadCrumbs.Count(); i++)
            {
                var dataTable = getDB.getRow3(conn, UpdatedValuesT.Name, UpdatedValuesT.HitsTableID, breadCrumbs[i].ID.ToString());//breadCrumbs[i].Data;//
                breadCrumbs[i].TableName = getTablename(breadCrumbs[i].SourceSQL).
                    Replace(' ', '_').
                    Replace('#', 'X').
                    Trim(arr);
                DataRow PK = findPK(dataTable);

                if (PK != null)
                {
                    breadCrumbs[i].PK = PK[UpdatedValuesT.FieldName].ToString();
                    string search = PK[UpdatedValuesT.FieldValue].ToString();


                    for (int j = i + 1; j < breadCrumbs.Count(); j++)
                    {
                        var dataTable2 = getDB.getRow3(conn, UpdatedValuesT.Name, UpdatedValuesT.HitsTableID, breadCrumbs[j].ID.ToString());//breadCrumbs[j].Data;//

                        foreach (DataRow dataRow2 in dataTable2.Rows)
                        {
                            if (!(bool)dataRow2[UpdatedValuesT.FieldPrimaryKey])
                            {
                                if (search == dataRow2[UpdatedValuesT.FieldValue].ToString())
                                {
                                    string newFK = dataRow2[UpdatedValuesT.FieldName].ToString() + '\t' +
                                        breadCrumbs[i].TableName + '\t' +
                                        breadCrumbs[i].PK + '\t';
                                    bool duplicate = false;

                                    foreach (string myFK in breadCrumbs[j].FKs)
                                    {
                                        if (myFK == newFK)
                                        {
                                            duplicate = true;
                                        }
                                    }
                                    if (!duplicate)
                                    {
                                        breadCrumbs[j].FKs.Add(newFK);
                                    }

                                    breadCrumbs[i].Children.Add(breadCrumbs[j].ID);
                                }
                            }
                        }
                    }
                }
            }
        }

        public static void AnalizeTrails2(List<Trail> breadCrumbs, SqlConnection conn)
        {
            char[] arr = { '[', ']' };
            for (int i = 0; i < breadCrumbs.Count(); i++)
            {
                var dataTable = getDB.getRow3(conn, UpdatedValuesT.Name, UpdatedValuesT.HitsTableID, breadCrumbs[i].ID.ToString());//breadCrumbs[i].Data;//
                breadCrumbs[i].TableName = getTablename(breadCrumbs[i].SourceSQL).
                    Replace(' ', '_').
                    Replace('#', 'X').
                    Trim(arr);
                DataRow PK = findPK(dataTable);

                if (PK != null)
                {
                    breadCrumbs[i].PK = PK[UpdatedValuesT.FieldName].ToString().
                        Replace('#', 'X').
                        Replace(' ', '_');
                    string search = PK[UpdatedValuesT.FieldValue].ToString();

                    for (int j = 0; j < breadCrumbs.Count(); j++) //might stack overflow
                    {
                        var dataTable2 = getDB.getRow3(conn, UpdatedValuesT.Name, UpdatedValuesT.HitsTableID, breadCrumbs[j].ID.ToString());//breadCrumbs[j].Data;//

                        foreach (DataRow dataRow2 in dataTable2.Rows)
                        {
                            if (!(bool)dataRow2[UpdatedValuesT.FieldPrimaryKey])
                            {
                                if (search == dataRow2[UpdatedValuesT.FieldValue].ToString())
                                {
                                    string newFK = dataRow2[UpdatedValuesT.FieldName].ToString().Replace('#', 'X') + '\t' +
                                        breadCrumbs[i].TableName + '\t' +
                                        breadCrumbs[i].PK + '\t';
                                    bool duplicate = false;

                                    foreach (string myFK in breadCrumbs[j].FKs)
                                    {
                                        if (myFK == newFK)
                                        {
                                            duplicate = true;
                                        }
                                    }
                                    if (!duplicate)
                                    {
                                        breadCrumbs[j].FKs.Add(newFK);
                                    }

                                    breadCrumbs[i].Children.Add(breadCrumbs[j].ID);
                                }
                            }
                        }
                    }
                }
            }
        }

        public static List<Trail> MergeTrails(List<Trail> trails)
        {
            List<Trail> mergedTrails = new List<Trail>();

            for (int i = trails.Count - 1; i >= 0; i--)
            {
                Trail combinedTrail = trails[i];
                for (int j = i - 1; j >= 0; j--)
                {
                    if (combinedTrail.TableName == trails[j].TableName)
                    {
                        if (combinedTrail.PK == "" & trails[j].PK != "")
                        {
                            combinedTrail.PK = trails[j].PK;
                        }
                        foreach (string FK in trails[j].FKs)
                        {
                            bool duplicate = false;
                            foreach (string combinedFK in combinedTrail.FKs)
                            {
                                if (FK == combinedFK)
                                {
                                    duplicate = true;
                                }
                            }

                            if (!duplicate)
                            {
                                combinedTrail.FKs.Add(FK);
                            }
                        }

                        trails.RemoveAt(j);
                        i--;
                    }
                }
                mergedTrails.Add(combinedTrail);
            }

            return mergedTrails;
        }

        public static DataRow findPK(DataTable dataTable)
        {
            foreach (DataRow row in dataTable.Rows)
            {
                if ((bool)row[UpdatedValuesT.FieldPrimaryKey])
                {
                    return row;
                }
            }
            return null;
        }

        public static List<string> FilterSimpleQueries(int lastQuery, SqlConnection connSniffer)
        {
            List<string> Updates = new List<string>();
            for (int i = 1; i <= lastQuery; i++)
            {
                DataRow row = getDB.getRow2(connSniffer, HitsT.Name, i);

                string sql = row[HitsT.SQL].ToString();

                int count = Regex.Matches(sql, "From").Count;
                int coun2 = Regex.Matches(sql, "FROM").Count;
                int coun3 = Regex.Matches(sql, "from").Count;

                if ((count == 1 & coun2 == 0 & coun3 == 0) |
                    (count == 0 & coun2 == 1 & coun3 == 0) |
                    (count == 0 & coun2 == 0 & coun3 == 1))
                {
                    Updates.Add(sql);
                }
            }
            return Updates;
        }

        public static List<QueryO> createQueryOs(List<string> simplequeries, SqlConnection connGeneral)
        {
            List<QueryO> QueryOs = new List<QueryO>();
            foreach (var item in simplequeries)
            {
                QueryO query = new QueryO();
                query.SQL = item;
                query.TableName = getTablename(item).TrimStart('[').TrimEnd(']');
                if (query.TableName != "ParseError")
                {
                    query.TableID = getTableID("'" + query.TableName + "'", connGeneral);
                    query.PKFieldID = getPK2(query.TableID, connGeneral);
                    if (query.PKFieldID != -1)
                    {
                        query.PK = getPK(query.PKFieldID, connGeneral);
                    }
                    string[] parts = item.Split(new string[] { "Where" }, StringSplitOptions.None);
                    if (parts.Length == 1)
                    {
                        parts = item.Split(new string[] { "WHERE" }, StringSplitOptions.None);
                    }
                    if (parts.Length == 1)
                    {
                        parts = item.Split(new string[] { "where" }, StringSplitOptions.None);
                    }
                    if (parts.Length > 1)
                    {
                        List<string> selector = getSubSections(parts[1].ToArray(), '[', ']', false, findClosing);//contains selector
                        if (selector.Count > 0)
                        {
                            query.Selector = selector[0];
                            int id = getFieldID(query.TableID, query.Selector, connGeneral);
                            query.SelectorFieldID = id;
                        }
                        string[] parts2 = parts[1].Split(new string[] { "AND" }, StringSplitOptions.None);
                        if (parts2[0].Contains('='))
                        {
                            string[] parts3 = parts2[0].Split('=');
                            query.Value = parts3[1].Trim(' ').Trim('\'');
                        }
                    }
                }
                QueryOs.Add(query);
                //Console.WriteLine(
                //        '\n' + "SQL: " + query.SQL +
                //        '\n' + "Value: " + query.Value +
                //        '\n' + "TableName: " + query.TableName +
                //        '\n' + "______PK: " + query.PK +
                //        '\n' + "Selector: " + query.Selector +
                //        '\n' + "Table ID: " + query.TableID.ToString() +
                //        '\n' + "PK ID: " + query.PKFieldID.ToString() +
                //        '\n' + "Sel ID: " + query.SelectorFieldID.ToString()
                //        );                             
            }
            foreach (QueryO myQuery in QueryOs)
            {
                if (myQuery.PKFieldID == myQuery.SelectorFieldID)
                {
                    myQuery.isPK = true;
                }
                else
                {
                    myQuery.isPK = false;
                }
            }
            return QueryOs;
            //Got the simple queries
            //Extract  From "TableName"
        }

        public static List<QueryO[]> analizeQueryOs(List<QueryO> queryOs)
        {
            List<QueryO[]> hits = new List<QueryO[]>();
            for (int i = 0; i < queryOs.Count; i++)
            {
                if (queryOs[i].isPK)
                {
                    if (queryOs[i].Value != "" & queryOs[i].Value != "0")
                    {
                        for (int j = 0; j < queryOs.Count; j++)
                        {
                            if (!queryOs[j].isPK)
                            {
                                if (queryOs[j].Value == queryOs[i].Value)
                                {
                                    QueryO[] hit = new QueryO[2];

                                    hit[0] = queryOs[i];//PK
                                    hit[1] = queryOs[j];//FK
                                    hits.Add(hit);
                                    //found foreignKey
                                    Console.WriteLine(">>>>>>>>>>>>>" +
                                        '\n' + queryOs[i].TableName +
                                        '\n' + queryOs[i].PK +
                                        '\n' + queryOs[j].TableName +
                                        '\n' + queryOs[j].Selector +
                                        '\n' + queryOs[i].SQL +
                                        '\n' + queryOs[j].SQL +
                                        '\n' + queryOs[j].Value +
                                        '\n'
                                        );
                                }
                            }
                        }
                    }
                }
            }
            return hits;
        }

        public static List<QueryO[]> filterDuplicateHits(List<QueryO[]> queryOs)
        {
            List<QueryO[]> filtered = new List<QueryO[]>();

            filtered.AddRange(queryOs);


            for (int i = filtered.Count - 1; i >= 0; i--)
            {
                //string result = hitToString(filtered[i]);

                for (int j = i - 1; j >= 0; j--)
                {
                    //string result2 = hitToString(filtered[j]);
                    if (duplicateHit(filtered[i], filtered[j]))
                    {
                        filtered.RemoveAt(j);
                        i--;
                    }
                }
            }
            return filtered;
        }

        public static string hitToString(QueryO[] hit)
        {
            return
                hit[0].TableName + "-" +
                hit[0].PK + "--" +
                hit[1].TableName + "-" +
                hit[1].Selector;
        }

        public static bool duplicateHit(QueryO[] hit1, QueryO[] hit2)
        {
            if (hit1[0].TableID != hit2[0].TableID)
            {
                return false;
            }
            if (hit1[0].PKFieldID != hit2[0].PKFieldID)
            {
                return false;
            }
            if (hit1[1].TableID != hit2[1].TableID)
            {
                return false;
            }
            if (hit1[1].SelectorFieldID != hit2[1].SelectorFieldID)
            {
                return false;
            }
            return true;
        }

        public static string getPK(int FieldID, SqlConnection conn)
        {
            DataRow dataTable = getDB.getRow2(conn, FieldsT.Name, FieldID);

            return dataTable[FieldsT.Field].ToString();
        }

        public static int getPK2(int TableID, SqlConnection conn)
        {
            DataTable dataTable = getDB.getRow3(conn, FieldsT.Name, FieldsT.ParentTable, TableID.ToString());

            foreach (DataRow dataRow in dataTable.Rows)
            {
                if ((bool)dataRow[FieldsT.PK])
                {
                    return (int)dataRow[FieldsT.ID];
                }
            }

            return -1;
        }

        public static int getTableID(string TableName, SqlConnection conn)
        {
            DataTable dataTable = getDB.getRow3(conn, TablesT.Name, TablesT.Table, TableName);

            if (dataTable.Rows.Count == 1)
            {
                return (int)dataTable.Rows[0][TablesT.ID];
            }
            return -1;
        }

        public static int getFieldID(int TableID, string Field, SqlConnection conn)
        {

            DataTable data = getDB.getRow3(conn, FieldsT.Name, FieldsT.Field, "'" + Field + "'");

            if (data.Rows.Count > 1)
            {
                //Console.WriteLine("Multiple Fields");

                foreach (DataRow dataRow in data.Rows)
                {
                    if ((int)dataRow[FieldsT.ParentTable] == TableID)
                    {
                        return (int)dataRow[FieldsT.ID];
                    }
                }
                //multiple hits
            }
            else if (data.Rows.Count == 1)
            {
                return (int)data.Rows[0][0];
            }
            return 0;
        }

        public static string q(string input)
        {
            return "\"" + input + "\"";
        }

        public static string buildJSON(DataRow PK, bool First, SqlConnection connGeneral, List<int> includedTables, string FatherPK = "", bool stop = false, bool includeAllFields = false)
        {
            //string ID = PK["ID"].ToString();
            string PKName = PK[FieldsT.Field].ToString();
            //Get my Parent Table
            DataTable tableNameData = getDB.getRow3(connGeneral, TablesT.Name, TablesT.ID, PK[FieldsT.ParentTable].ToString());

            if (includedTables.Contains((int)PK[FieldsT.ParentTable]))
            {
                //get my siblings
                DataTable membersTable = getDB.getRow3(connGeneral, FieldsT.Name, FieldsT.ParentTable, PK[FieldsT.ParentTable].ToString());
                //what to include from me?
                //string members = addMembers(membersTable) ? createMember(membersTable) : "[]";
                string members = includeAllFields ? createMember(membersTable) : "[]";

                string myTable = tableNameData.Rows[0][FieldsT.ParentTable].ToString();
                // the first time, we start with the PK, but subsequent times it comes from the ForeignKeys, myPK could be -1, not found
                int myPK = getPK2((int)tableNameData.Rows[0][TablesT.ID], connGeneral);
                //get who points to me
                DataTable children = getDB.getRow3(connGeneral, FieldsT.Name, FieldsT.Pointer, myPK.ToString());
                string output = "";

                if (!stop)// 1:1 Join, not a parent->child relationship
                {
                    if (First)
                    {
                        output = "{" +
                                        q("table") + ":" + q(myTable) + "," +
                                        q("rules") + ":" + q("CRUD") + "," +
                                        q("as") + ":" + q(standardName(myTable)) + "," +
                                        q("type") + ":" + q("many") + "," +
                                        q("members") + ":" + members + "," +
                                        q("joins") + ":" + "[";
                    }
                    else
                    {
                        output = "{" +
                                        q("table") + ":" + q(myTable) + "," +
                                        q("rules") + ":" + q("CRUD") + "," +
                                        q("on") + ":" + q(PKName) + "," +      //FK in this table
                                        q("with") + ":" + q(FatherPK) + "," + //PK of father table
                                        q("as") + ":" + q(standardName(myTable)) + "," +
                                        q("type") + ":" + q("many") + "," +
                                        q("members") + ":" + members + "," +
                                        q("joins") + ":" + "[";
                    }
                }
                else
                {
                    output = "{" +
                                        q("table") + ":" + q(myTable) + "," +
                                        q("rules") + ":" + q("XRXX") + "," +
                                        q("on") + ":" + q(PKName) + "," +      //FK in this table
                                        q("with") + ":" + q(FatherPK) + "," + //PK of father table
                                        q("as") + ":" + q(standardName(myTable)) + "," +
                                        q("members") + ":" + members + "," +
                                        //q("type") + ":" + q("many") + "," +
                                        q("joins") + ":" + "[";
                }




                string middle = ""; //iterate all FK fields
                if (!stop)
                {

                    for (int i = 0; i < children.Rows.Count; i++)
                    {
                        //if (!(bool)children.Rows[i]["OneToONe"])
                        //{
                        //TODO: Move this outside loop
                        if (myPK != -1)
                        {
                            DataRow futureParent = getDB.getRow3(connGeneral, FieldsT.Name, FieldsT.ID, myPK.ToString()).Rows[0];
                            string myParentName = futureParent[FieldsT.Field].ToString();

                            if (!(bool)children.Rows[i][FieldsT.IgnoreFK])
                            {
                                if (i != children.Rows.Count - 1)
                                {
                                    //get my PK 
                                    middle += buildJSON(children.Rows[i], false, connGeneral, includedTables, myParentName, false, includeAllFields) + ",";
                                }
                                else
                                {
                                    middle += buildJSON(children.Rows[i], false, connGeneral, includedTables, myParentName, false, includeAllFields);
                                }
                            }
                        }
                        //}                    
                    }

                    DataTable myOneToONe = getOneToOne(FieldsT.Name, PK[FieldsT.ParentTable].ToString(), FieldsT.ParentTable, FieldsT.OneToOne, connGeneral);
                    //add the ONE TO ONE like Order Detail -> INV ITEM

                    if (myOneToONe.Rows.Count > 0)
                    {
                        middle += ",";
                    }
                    for (int i = 0; i < myOneToONe.Rows.Count; i++)
                    {
                        if (myPK != -1)
                        {
                            //TODO: Move this outside loop
                            //DataRow myParent = getDB.getRow3(connGeneral, "Fields_Table", myPK.ToString(), "ID").Rows[0];
                            //string myParentName = myParent["FieldName"].ToString();

                            if (!(bool)myOneToONe.Rows[i][FieldsT.IgnoreFK])
                            {
                                DataTable newPKRow = getDB.getRow3(connGeneral, FieldsT.Name, FieldsT.ID, myOneToONe.Rows[i][FieldsT.Pointer].ToString());

                                if (FatherPK != newPKRow.Rows[0][1].ToString())// AR-ORDERD_Extended -> Inventory -> AR-ORDERD_Extended
                                {
                                    if (i != myOneToONe.Rows.Count - 1) // cannot be recursive for this case
                                    {
                                        //get my PK 
                                        middle += buildJSON(newPKRow.Rows[0], false, connGeneral, includedTables, myOneToONe.Rows[i][1].ToString(), true, includeAllFields) + ",";
                                    }
                                    else
                                    {
                                        middle += buildJSON(newPKRow.Rows[0], false, connGeneral, includedTables, myOneToONe.Rows[i][1].ToString(), true, includeAllFields);
                                    }
                                }
                            }
                        }
                    }
                }
                string end = "]}";
                return output + middle + end;

            }
            else
            {
                return "";
            }

        }

        public static string createMember(DataTable dataTable)
        {
            string result = "[";
            foreach (DataRow row in dataTable.Rows)
            {
                if ((bool)row[FieldsT.Include])
                {
                    result += q(row[FieldsT.Field].ToString()) + ",";
                }
            }
            return result += "]";
        }

        public static bool addMembers(DataTable dataTable)
        {
            foreach (DataRow row in dataTable.Rows)
            {
                if (!(bool)row[FieldsT.Include])
                {
                    return true; //There is at least one that should not be include, create the whole list
                }
            }
            return false;
        }

        public static Task<string> MainObjectAsync(DataRow PK, string ObjectName, SqlConnection connGeneral, List<int> avoidTables, bool includeAllFields)
        {
            return Task.FromResult(MainObject(PK, ObjectName, connGeneral, avoidTables, includeAllFields));
        }

        public static string MainObject(DataRow PK, string ObjectName, SqlConnection connGeneral, List<int> avoidTables, bool includeAllFields)
        {

            string outPut =
                    "{" +
                    q(ObjectName) + ":" + buildJSON(PK, true, connGeneral, avoidTables, "", false, includeAllFields)
                    .Replace("[,", "[")
                    .Replace(",]", "]")
                    .Replace(",,", ",") +

                "}";

            return outPut.Replace(",,", ",");
        }

        public static string standardName(string Name)
        {
            //string[] parts = Name.Split(' ');

            //string result = parts
            //.ToList()
            //.Aggregate("_", (i, j) => i + j);
            return "_" + Name.Replace('#', 'X').Replace(' ', '_');
        }

        public static List<int> getFamilyRows(DataRow PK, SqlConnection connGeneral, bool stop = false, string parent = "")
        {
            List<int> myChildren = new List<int>();//get all the ID for all tables related

            //if (!(bool)PK["IgnoreFK"])

            string ID = PK[FieldsT.ID].ToString();//first time PK, second FK
            DataTable tableNameData = getDB.getRow3(connGeneral, TablesT.Name, TablesT.ID, PK[FieldsT.ParentTable].ToString());

            int tableID = (int)tableNameData.Rows[0][TablesT.ID];

            myChildren.Add(tableID);

            if (!stop)
            {
                int myPK = getPK2(tableID, connGeneral);

                DataTable Children = getDB.getRow3(connGeneral, FieldsT.Name, FieldsT.Pointer, myPK.ToString());

                for (int i = 0; i < Children.Rows.Count; i++)
                {
                    if (!(bool)Children.Rows[i][FieldsT.IgnoreFK])
                    {
                        DataRow futureParent = getDB.getRow3(connGeneral, FieldsT.Name, FieldsT.ID, myPK.ToString()).Rows[0];
                        string myParentName = futureParent[FieldsT.Field].ToString();
                        myChildren.AddRange(getFamilyRows(Children.Rows[i], connGeneral, false, myParentName));
                    }

                }

                DataTable myOneToONe = getOneToOne(FieldsT.Name, PK[FieldsT.ParentTable].ToString(), FieldsT.ParentTable, FieldsT.OneToOne, connGeneral);

                for (int i = 0; i < myOneToONe.Rows.Count; i++)
                {
                    //myChildren.AddRange(getFamilyRows(myOneToONe.Rows[i], connGeneral, true));

                    if (myPK != -1)
                    {
                        //TODO: Move this outside loop
                        //DataRow myParent = getDB.getRow3(connGeneral, "Fields_Table", myPK.ToString(), "ID").Rows[0];
                        //string myParentName = myParent["FieldName"].ToString();

                        if (!(bool)myOneToONe.Rows[i][FieldsT.IgnoreFK])
                        {

                            DataTable newPKRow = getDB.getRow3(connGeneral, FieldsT.Name, FieldsT.ID, myOneToONe.Rows[i][FieldsT.Pointer].ToString());

                            if (parent != newPKRow.Rows[0][1].ToString())// AR-ORDERD_Extended -> Inventory -> AR-ORDERD_Extended
                            {
                                myChildren.AddRange(getFamilyRows(newPKRow.Rows[0], connGeneral, true));
                            }

                            //get my PK                                                                                                                        
                        }
                    }
                }
            }



            return myChildren.Distinct().ToList();
        }

        public static Task<List<int>> getFamilyRowsAsync(DataRow PK, SqlConnection connGeneral, bool stop = false, string parent = "")
        {
            return Task.FromResult(getFamilyRows(PK, connGeneral, stop, parent));
        }

        public static Task<List<int>> getIncludedTablesAsync(DataRow PK, SqlConnection connGeneral, bool stop = false, string parent = "")
        {
            return Task.FromResult(getIncludedTables(PK, connGeneral, stop, parent));
        }

        public static List<int> getIncludedTables(DataRow PK, SqlConnection connGeneral, bool stop = false, string parent = "")
        {
            List<int> myChildren = new List<int>();//get all the ID for all tables related

            string ID = PK[FieldsT.ID].ToString();//first time PK, second FK
            DataTable tableNameData = getDB.getRow3(connGeneral, TablesT.Name, TablesT.ID, PK[FieldsT.ParentTable].ToString());

            if ((bool)tableNameData.Rows[0][TablesT.Include])
            {
                int tableID = (int)tableNameData.Rows[0][TablesT.ID];

                myChildren.Add(tableID);

                if (!stop)
                {
                    int myPK = getPK2(tableID, connGeneral);

                    DataTable Children = getDB.getRow3(connGeneral, FieldsT.Name, FieldsT.Pointer, myPK.ToString());

                    for (int i = 0; i < Children.Rows.Count; i++)
                    {
                        if (!(bool)Children.Rows[i][FieldsT.IgnoreFK])
                        {
                            DataRow futureParent = getDB.getRow3(connGeneral, FieldsT.Name, FieldsT.ID, myPK.ToString()).Rows[0];
                            string myParentName = futureParent[FieldsT.Field].ToString();
                            myChildren.AddRange(getIncludedTables(Children.Rows[i], connGeneral, false, myParentName));

                        }

                    }

                    DataTable myOneToONe = getOneToOne(FieldsT.Name, PK[FieldsT.ParentTable].ToString(), FieldsT.ParentTable, FieldsT.OneToOne, connGeneral);

                    for (int i = 0; i < myOneToONe.Rows.Count; i++)
                    {
                        //myChildren.AddRange(getFamilyRows(myOneToONe.Rows[i], connGeneral, true));

                        if (myPK != -1)
                        {
                            //TODO: Move this outside loop
                            //DataRow myParent = getDB.getRow3(connGeneral, "Fields_Table", myPK.ToString(), "ID").Rows[0];
                            //string myParentName = myParent["FieldName"].ToString();

                            if (!(bool)myOneToONe.Rows[i][FieldsT.IgnoreFK])
                            {

                                DataTable newPKRow = getDB.getRow3(connGeneral, FieldsT.Name, FieldsT.ID, myOneToONe.Rows[i][FieldsT.Pointer].ToString());

                                if (parent != newPKRow.Rows[0][1].ToString())// AR-ORDERD_Extended -> Inventory -> AR-ORDERD_Extended
                                {
                                    myChildren.AddRange(getIncludedTables(newPKRow.Rows[0], connGeneral, true));
                                }

                                //get my PK                                                                                                                        
                            }
                        }
                    }
                }

            }
            return myChildren.Distinct().ToList();
        }

        public static string printTables(List<int> TableIDs, SqlConnection connGeneral)
        {
            string result = "";

            foreach (int tableID in TableIDs)
            {
                string interResult = "\n";
                string TableHeader = "Table ";

                // get all the information for the table that has ID tableID
                DataTable table = getDB.getRow3(connGeneral, TablesT.Name, TablesT.ID, tableID.ToString());

                // Rows[0] but there really is only ONE row anyways
                string TableName = table.Rows[0][TablesT.Table].ToString();

                // i.e. Table _AR_Customer {
                //
                interResult = TableHeader + standardName(TableName) + " {" + '\n';


                // get the name of the PK for this table
                string primaryKey = "";
                int myPK = getPK2(tableID, connGeneral);
                if (myPK != -1)
                {
                    primaryKey = getDB.getRow3(connGeneral, FieldsT.Name, FieldsT.ID, myPK.ToString())
                    .Rows[0][FieldsT.Field]
                    .ToString();


                    // Table _AR_Customer {
                    // _AR_CUST_Customer_ID int [pk]
                    interResult += standardName(primaryKey) + " int [pk]" + '\n';
                }
                else
                {
                    Console.WriteLine("NO PK");
                }


                // string PK;

                List<int> myFKS = getFks(tableID, connGeneral);
                foreach (int FK in myFKS)
                {
                    DataTable foreignKeyInfo = getDB.getRow3(connGeneral, FieldsT.Name, FieldsT.ID, FK.ToString());

                    if (!(bool)foreignKeyInfo.Rows[0][FieldsT.IgnoreFK])
                    {
                        string foreignKeyName = foreignKeyInfo
                        .Rows[0][FieldsT.Field]
                        .ToString();

                        int myForeignKey = (int)foreignKeyInfo.Rows[0][FieldsT.Pointer];

                        DataTable pointer = getDB.getRow3(connGeneral, FieldsT.Name, FieldsT.ID, myForeignKey.ToString());

                        int myTableID = (int)pointer.Rows[0][FieldsT.ParentTable];

                        string primaryKeyName = pointer.Rows[0][FieldsT.Field].ToString();

                        DataTable tablePointer = getDB.getRow3(connGeneral, TablesT.Name, TablesT.ID, myTableID.ToString());

                        string exTableName = tablePointer.Rows[0][TablesT.Table].ToString();

                        string exPrimaryKey = pointer.Rows[0][FieldsT.Field].ToString();


                        int myExternalPK = getPK2(myForeignKey, connGeneral);
                        string exprimaryKey = "";
                        if (myExternalPK != -1)
                        {
                            exprimaryKey = getDB.getRow3(connGeneral, FieldsT.Name, FieldsT.ID, myExternalPK.ToString())
                            .Rows[0][FieldsT.Field]
                            .ToString();
                        }
                        DataTable extable = getDB.getRow3(connGeneral, TablesT.Name, TablesT.ID, myForeignKey.ToString());

                        interResult += standardName(foreignKeyName) + " int [ref: > " +
                            standardName(exTableName) + "." +
                            standardName(primaryKeyName) + "]" + '\n';


                        Console.WriteLine(standardName(foreignKeyName)
                            + "-" + standardName(exTableName)
                            + "." + standardName(primaryKeyName));

                    }
                }
                result += interResult + '\n' + '}' + '\n';
            }
            return result;
        }


        public static Task<string> printTablesAsync(List<int> TableIDs, SqlConnection connGeneral)
        {
            return Task.FromResult(printTables(TableIDs, connGeneral));
        }
        public static List<int> getFks(int tableId, SqlConnection connGeneral)
        {
            List<int> FKs = new List<int>();
            DataTable Children = getDB.getRow3(connGeneral, FieldsT.Name, FieldsT.ParentTable, tableId.ToString());

            foreach (DataRow row in Children.Rows)
            {
                if (!row.IsNull(FieldsT.Pointer))
                {

                    FKs.Add((int)row[FieldsT.ID]);
                }
            }
            return FKs;
        }

        public static DataTable getOneToOne(string TableName, string ForeignValue, string ForeignKey, string FieldName, SqlConnection cnn)
        {
            DataTable dataTable = new DataTable();
            SqlCommand command;
            string sql = "Select * From " + TableName +
                " Where " + FieldName + //OneToOne Flag is set to true
                " <> 0" +
                " AND " +
                ForeignKey + " = " + ForeignValue;

            command = new SqlCommand(sql, cnn);
            SqlDataAdapter da = new SqlDataAdapter(command);
            da.Fill(dataTable);
            return dataTable;
        }

    }
}
