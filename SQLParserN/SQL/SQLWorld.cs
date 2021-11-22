using System;
using System.Collections.Generic;
using System.Linq;
using static StringParserN.CharWorld;
using SQLParserN.SQL.Models.Table;

namespace SQLParserN.SQL
{
    public static class SQLWorld
    {
        //standard Names
        public static string StandarizeLine(char[] array, List<string> possibleNames)
        {
            foreach (int[] word in getSubSectionsIndexes(array, '[', ']', findClosing))
            {
                string nameToReplace = new string(getSubArray(
                            word[0],
                            word[1],
                            array));

                injectSubArray(
                    word[0],
                    word[1],
                    array,
                    standardName(
                        nameToReplace,
                        possibleNames));
                // i'm modifying the original array                
            }
            return new string(array);//array has been modified
        }
        public static char[] standardName(string searchWord, List<string> possibleNames)
        {
            List<string> output = SchemaTaladro(searchWord, possibleNames, bitContains);
            if (output.Count > 0)
            {
                return ("[" + output[0] + "]").ToArray(); //could be more different standards like [ID] [id]
            }
            else
            {
                return searchWord.ToArray(); //ya viene con [ ]
            }
        }

        public static List<string> SchemaTaladro(string search, List<string> possibleNames, DrillBit bit)
        {
            StringComparison comp = StringComparison.OrdinalIgnoreCase;
            List<string> hits = new List<string>();
            foreach (string currentName in possibleNames)
            {
                string line = "[" + currentName + "]";
                if (bit(line, search, comp))
                {
                    hits.Add(currentName);
                }
            }
            return hits;
        }

        public static string SelectorString(string BitName, string line)
        {
            List<object> myParams = new List<object>();
            myParams.Add(line);
            Type type = typeof(SQLWorld);
            return (string)type.GetMethod(BitName).Invoke(null, myParams.ToArray());
        }

        public static string getTablename(string line)
        {
            //Expecting a string name
            string[] Parts2 = line.Split(new string[] { "From" }, StringSplitOptions.None);
            if (Parts2.Length > 1)
            {
                List<string> fields = getSubSections(Parts2[1].ToArray(), '[', ']', true, findClosing);
                if (fields.Count > 0)
                {
                    return fields[0];
                }

            }
            Parts2 = line.Split(new string[] { "FROM" }, StringSplitOptions.None);
            if (Parts2.Length > 1)
            {
                List<string> fields = getSubSections(Parts2[1].ToArray(), '[', ']', true, findClosing);
                if (fields.Count > 0)
                {
                    return fields[0];
                }
            }
            Parts2 = line.Split(new string[] { "from" }, StringSplitOptions.None);
            if (Parts2.Length > 1)
            {
                List<string> fields = getSubSections(Parts2[1].ToArray(), '[', ']', true, findClosing);
                if (fields.Count > 0)
                {
                    return fields[0];
                }
            }
            return "ParseError";
        }

        public static string StandardName(List<string> standardNames, string rawName)
        {
            List<string> finds = SchemaTaladro(rawName, standardNames, bitContains);
            if (finds.Count > 0)
            {
                return finds[0];
            }
            return "NotFound-rawName";
        }

        // There is a Master.json file that contains the names of the Tables and Fields of the WW DB.
        // When parsing that file this object creates that standard names that with can compare agaisnt
        // non-standardrize queries
        public static List<string> TableNames(List<Table> Schema)
        {
            //In theory table names are unique
            List<string> myTablesNames = new List<string>();
            foreach (Table table in Schema)
            {
                myTablesNames.Add(table.Name);
            }
            return myTablesNames;
        }
        public static List<string> FieldNames(List<Table> Schema, string TableName)
        {
            List<string> columnNames = new List<string>();
            foreach (Table table in Schema)
            {
                if (TableName == table.Name)
                {
                    foreach (Column column in table.columns)
                    {
                        columnNames.Add(column.name);
                    }
                    return columnNames;
                }
            }
            return columnNames;
        }

        public enum FieldTypes
        {
            adBigInt = 20,
            adBinary = 128,
            adBoolean = 11,
            adBSTR = 8,
            adChapter = 136,
            adChar = 129,
            adCurrency = 6,
            adDate = 7,
            adDBDate = 133,
            adDBTime = 134,
            adDBTimeStamp = 135,
            adDecimal = 14,
            adDouble = 5,
            adEmpty = 0,
            adError = 10,
            adFileTime = 64,
            adGUID = 72,
            adIDispatch = 9,
            adInteger = 3,
            adIUnknown = 13,
            adLongVarBinary = 205,
            adLongVarChar = 201,
            adLongVarWChar = 203,
            adNumeric = 131,
            adPropVariant = 138,
            adSingle = 4,
            adSmallInt = 2,
            adTinyInt = 16,
            adUnsignedBigInt = 21,
            adUnsignedInt = 19,
            adUnsignedSmallInt = 18,
            adUnsignedTinyInt = 17,
            adUserDefined = 132,
            adVarBinary = 204,
            adVarChar = 200,
            adVariant = 12,
            adVarNumeric = 139,
            adVarWChar = 202,
            adWChar = 130
        }
    }
}
