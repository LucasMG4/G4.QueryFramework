namespace G4.QueryFramework.Controllers {
    public class QueryContext {

        private string? ConnectionString { get; set; }
        public QueryCommand Command { 
            get {

                if(_Command != null)
                    return _Command;

                if (ConnectionString == null)
                    throw new ArgumentNullException(nameof(ConnectionString));

                _Command = new QueryCommand(ConnectionString);

                return _Command;

            } 
        }

        private QueryCommand? _Command { get; set; }

        public QueryContext() { }

        public void SetConnectionString(string connectionString) {
            this.ConnectionString = connectionString;
            this._Command = null;
        }

        public QueryEntity<Entity> BuildEntityManipulator<Entity>() => new QueryEntity<Entity>(this.Command);


    }
}
