namespace G4.QueryFramework.Attributes {
    public class ColumnConfigureAttribute : Attribute {

        public string columnName { get; set; }
        public bool isPrimarykey { get; set; } = false;
        public bool isMD5 { get; set; } = false;
        public bool ignoreUpdate { get; set; } = false;
        public bool ignoreInsert { get; set; } = false;

        public ColumnConfigureAttribute(string columnName, bool isPrimaryKey = false, bool ignoreUpdate = false, bool ignoreInsert = false, bool isMD5 = false) { 
            this.columnName = columnName;
            this.isPrimarykey = isPrimaryKey;
            this.ignoreUpdate = ignoreUpdate;
            this.ignoreInsert = ignoreInsert;
            this.isMD5 = isMD5;
        }


    }
}
