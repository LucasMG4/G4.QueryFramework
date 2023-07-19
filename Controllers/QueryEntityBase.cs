using G4.QueryFramework.Attributes;
using G4.QueryFramework.Models;
using System.Linq.Expressions;
using System.Text;

namespace G4.QueryFramework.Controllers {
    public partial class QueryEntityBase<Model> {

        internal string GetTableName() {

            var attribute = typeof(Model).GetCustomAttributes(typeof(TableConfigureAttribute), true).FirstOrDefault() as TableConfigureAttribute;

            if (attribute == null)
                throw new Exception($"Entity '{typeof(Model).Name}' doen't have TableConfigureAttribute");

            return attribute.tableName;

        }

        internal QueryWhere? GetCondition(Expression<Func<Model, bool>>? where, bool useTable = false) {

            if(where == null)
                return null;

            MySqlExpressionVisitor visitor = new MySqlExpressionVisitor();
            visitor.Visit(where);

            string whereClause = visitor.WhereClause;
            Dictionary<string, object> parameters = visitor.Parameters;

            if (!useTable)
                whereClause = whereClause.Replace("n01.", "");

            return new QueryWhere(whereClause, parameters);

        }

        internal string MakeSelect() {

            var sql = new StringBuilder();

            sql.AppendLine("SELECT");
            sql.AppendLine(string.Join(", ", this.GetFields(typeof(Model)).ToArray()));

            sql.AppendLine($"FROM {this.GetTableName()} n01");

            var foreignFields = typeof(Model).GetProperties()
                .Where(x => x.GetCustomAttributes(typeof(ForeignConfigureAttribute), true).FirstOrDefault() != null);

            var foreignValue = 2;

            foreach (var property in foreignFields) {

                var attribute = property.GetCustomAttributes(typeof(ForeignConfigureAttribute), true).FirstOrDefault() as ForeignConfigureAttribute;
                var attributeTableInfo = property.PropertyType.GetCustomAttributes(typeof(TableConfigureAttribute), true).FirstOrDefault() as TableConfigureAttribute;

                if (attribute == null || attributeTableInfo == null)
                    throw new Exception($"Foreign Key error {property.Name}");

                var tableApelido = $"n{foreignValue.ToString().PadLeft(2, '0')}";

                sql.AppendLine($"{attribute.JoinType} JOIN {attributeTableInfo.tableName} {tableApelido}");
                sql.AppendLine($"ON");

                if (attribute.KeysIn.Length != attribute.KeysOut.Length)
                    throw new Exception($"Foreign key's number conflit {property.Name}");

                for(int position = 0; position < attribute.KeysIn.Length; position++) {

                    sql.AppendLine($"   {(position > 0 ? "AND": "")} {tableApelido}.{attribute.KeysOut[position]} = n01.{attribute.KeysIn[position]}");

                }

                foreignValue++;

            }

            return sql.ToString();

        }

        internal string MakeQuery() {

            var sql = new StringBuilder();

            sql.AppendLine($"SELECT");

            var fieldsFounded = 0;

            var properties = typeof(Model).GetProperties();

            foreach (var property in properties) {

                var attribute = property.GetCustomAttributes(typeof(ColumnConfigureAttribute), true).FirstOrDefault() as ColumnConfigureAttribute;

                if (attribute == null)
                    continue;

                sql.AppendLine($"{(fieldsFounded > 0 ? "," : "")} {attribute.columnName} {property.Name}");
                fieldsFounded++;

            }

            if (fieldsFounded == 0)
                throw new Exception($"Doesn't fields for the query in {nameof(Model)}.");

            sql.AppendLine($"FROM {this.GetTableName()}");

            return sql.ToString();

        }

        private List<string> GetFields(Type model, string tableApelido = "n01", bool makeForeign = true) {

            var result = new List<string>();

            var propertiesFields = model.GetProperties()
                .Where(x => x.GetCustomAttributes(typeof(ColumnConfigureAttribute), true).FirstOrDefault() != null);

            if(propertiesFields.Count() == 0)
                throw new Exception($"Doesn't fields for the query in {nameof(Model)}.");

            foreach (var property in propertiesFields) {

                var attribute = property.GetCustomAttributes(typeof(ColumnConfigureAttribute), true).FirstOrDefault() as ColumnConfigureAttribute;

                if (attribute == null)
                    continue;

                result.Add($"{tableApelido}.{attribute.columnName} {tableApelido}_{property.Name}");

            }

            if (!makeForeign)
                return result;

            var foreignFields = typeof(Model).GetProperties()
                .Where(x => x.GetCustomAttributes(typeof(ForeignConfigureAttribute), true).FirstOrDefault() != null);

            var foreignValue = 2;

            foreach(var property in foreignFields) {

                var attribute = property.GetCustomAttributes(typeof(ForeignConfigureAttribute), true).FirstOrDefault() as ForeignConfigureAttribute;

                if (attribute == null) 
                    continue;

                var foreign = this.GetFields(property.PropertyType, $"n{foreignValue.ToString().PadLeft(2, '0')}", false);

                result = result.Concat(foreign).ToList();

                foreignValue++;

            }

            return result;

        }

        internal string MakeUpdateSQL() {

            var sql = new StringBuilder();

            sql.AppendLine($"UPDATE {this.GetTableName()} SET");

            var fieldsFounded = 0;

            var properties = typeof(Model).GetProperties();

            foreach (var property in properties) {

                var attribute = property.GetCustomAttributes(typeof(ColumnConfigureAttribute), true).FirstOrDefault() as ColumnConfigureAttribute;

                if (attribute == null)
                    continue;

                if (attribute.ignoreUpdate || attribute.isMD5)
                    continue;

                sql.AppendLine($"{(fieldsFounded > 0 ? "," : "")} {attribute.columnName} = @{property.Name}");
                fieldsFounded ++;

            }

            if (fieldsFounded == 0)
                throw new Exception($"Doesn't fields for the Update sql in {nameof(Model)}.");

            return sql.ToString();

        }

        internal string MakeInsertSQL(EInsertType insertType = EInsertType.Insert) {

            var sql = new StringBuilder();

            sql.AppendLine($"{(insertType == EInsertType.Insert ? "INSERT":"REPLACE")} INTO {this.GetTableName()} SET");

            var fieldsFounded = 0;

            var properties = typeof(Model).GetProperties();

            foreach (var property in properties) {

                var attribute = property.GetCustomAttributes(typeof(ColumnConfigureAttribute), true).FirstOrDefault() as ColumnConfigureAttribute;

                if (attribute == null)
                    continue;

                if (attribute.ignoreInsert)
                    continue;

                var valueInsert = $"@{property.Name}";

                if (attribute.isMD5)
                    valueInsert = $"MD5(@{property.Name})";

                sql.AppendLine($"{(fieldsFounded > 0 ? "," : "")} {attribute.columnName} = {valueInsert}");
                fieldsFounded++;

            }

            if (fieldsFounded == 0)
                throw new Exception($"Doesn't fields for the Insert sql in {nameof(Model)}.");

            return sql.ToString();

        }


    }


    public class QueryWhere {

        public string Where { get; set; }
        public Dictionary<string, object> Values { get; set; }

        public QueryWhere(string  where, Dictionary<string, object> values) {
            Where = where;
            Values = values;
        }

    }

    
}
