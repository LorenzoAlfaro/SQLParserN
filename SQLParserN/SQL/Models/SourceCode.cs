namespace SQLParserN.SQL.Models
{
    class SourceCode
    {
    }

    public struct SourcesT
    {
        public static string Sources = "SourceFile_Table"; //TableName

        public static string Inject = "Inject"; //Field Inject

        public static string ID = "ID"; //Field Inject

    }

    public struct FunctionsT
    {
        public static string Functions = "Function_Table"; //TableName

        public static string FK_Sources = "SourceFile_Table_ID";

        public static string Inject = "Inject"; //Field Inject

        public static string ID = "ID"; //Field Inject

    }

    public struct ProjectFileT
    {
        public static string ProjectFiles = "ProjectFile_Table";

        public static string ID = "ID"; //Field Inject
    }

    //FormMaker Database strings:

    public struct FieldsT
    {
        public static string Name = "Fields_Table";
        public static string ID = "ID";
        public static string Field = "FieldName";
        public static string Hash = "HashName";
        public static string Type = "FieldType";
        public static string JSONType = "JSONType";
        public static string Nullable = "Nullable";
        public static string PK = "PrimaryKey";
        public static string ParentTable = "FormName";
        public static string Pointer = "FK_Field_Table_ID";
        public static string IgnoreFK = "IgnoreFK";
        public static string OneToOne = "OneToOne";
        public static string Include = "IncludeMember";

    }

    public struct TablesT
    {
        public static string Name = "FormsTables_Table";
        public static string ID = "ID";
        public static string Table = "FormName";
        public static string Hash = "HashName";
        public static string Include = "IncludeTable";
    }


    // Injecter Log DB

    public struct HitsT
    {
        public static string Name = "Hits_Table";
        public static string ID = "ID";
        public static string SQL = "SourceSQL";
        public static string IsUpdate = "IsUpdate";
        public static string InjectedIndex_Table_ID = "InjectedIndex_Table_ID";
        public static string BookMark = "BookMark";
    }

    public struct UpdatedValuesT
    {
        public static string Name = "UpdatedValues_Table";
        public static string ID = "ID";
        public static string FieldName = "FieldName";
        public static string FieldType = "FieldType";
        public static string FieldPrimaryKey = "FieldPrimaryKey";
        public static string FieldValue = "FieldValue";
        public static string TableValue = "TableValue";
        public static string HitsTableID = "Hits_Table_ID";
    }

    public class QueryO
    {
        public string SQL;
        public string TableName;
        public int TableID;
        public bool isPK; //it is using the PK to select, if not it might be a FK
        public string PK;
        public string Selector;
        public int PKFieldID;
        public int SelectorFieldID;
        public string Value;

        public QueryO()
        {
            Value = "";
            isPK = false;
            PK = "";
            Selector = "";
            TableID = -1;
            TableName = "";
            SelectorFieldID = -1;
            PKFieldID = -1;
        }
        public override string ToString()
        {
            return SQL;
        }
    }
}
