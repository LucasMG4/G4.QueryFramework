using G4.QueryFramework.Attributes;

namespace G4.QueryFramework.Controllers {
    public class QueryCommand : QueryConnection {
        
        internal QueryCommand(string connectionString) : base(connectionString) {   }

        public int Execute(string commandText, object? parameters = null, Dictionary<string, object>? where = null) {

            var command = this.CreateCommand(commandText, parameters, where);
            var result = command.ExecuteNonQuery();

            this.DisposeConnection();

            return result;

        }

        public Entity? GetFirstOrDefault<Entity>(string queryText, object? parameters = null, Dictionary<string, object>? where = null) {
            return Get<Entity>(queryText, parameters, where).FirstOrDefault();
        }

        public List<Entity> Get<Entity>(string queryText, object? parameters = null, Dictionary<string, object>? where = null) {

            var command = this.CreateCommand(queryText, parameters, where);
            var reader = command.ExecuteReader();

            var result = new List<Entity>();

            while(reader.Read()) {

                var obj = Activator.CreateInstance<Entity>();

                if(obj == null)
                    throw new ArgumentNullException(nameof(Entity));

                var foreignFields = obj.GetType().GetProperties()
                    .Where(x => x.GetCustomAttributes(typeof(ForeignConfigureAttribute), true).FirstOrDefault() != null);

                var entities = new Dictionary<string, object>();
                entities.Add("n01", obj);

                if (foreignFields.Count() > 0) {

                    var foreignTableName = 2;

                    foreach(var property in foreignFields) {

                        var tableName = $"n{foreignTableName.ToString().PadLeft(2, '0')}";

                        var foreignObj = Activator.CreateInstance(property.PropertyType);

                        if (foreignObj == null)
                            throw new Exception($"Foreign error, property {property.Name} have no public constructor.");

                        property.SetValue(obj, foreignObj);

                        entities.Add(tableName, foreignObj);

                    }

                }

                var fieldsCount = reader.FieldCount;

                for (int fieldPosition = 0; fieldPosition < fieldsCount; fieldPosition++) {

                    var fieldName = reader.GetName(fieldPosition).Substring(4);
                    var tableName = reader.GetName(fieldPosition).Substring(0, 3);
                    var fieldValue = reader.GetValue(fieldPosition);

                    if (fieldValue == null || fieldName == null)
                        continue;

                    var entity = entities[tableName];

                    var property = entity.GetType().GetProperty(fieldName);

                    if (property == null)
                        continue;

                    property.SetValue(entity, fieldValue);

                }

                result.Add(obj);

            }

            reader.Close();
            this.DisposeConnection();

            return result;

        }


        



    }
}
