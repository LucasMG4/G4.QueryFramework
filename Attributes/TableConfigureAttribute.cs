namespace G4.QueryFramework.Attributes {
    public class TableConfigureAttribute : Attribute {

        public string tableName { get; set; }
        
        public TableConfigureAttribute(string tableName) {
            this.tableName = tableName;
        }

    }
}
