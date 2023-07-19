using G4.QueryFramework.Models;
using System.Linq.Expressions;
using System.Text;

namespace G4.QueryFramework.Controllers {
    public class QueryEntity<Model> : QueryEntityBase<Model> {

        private QueryCommand Command { get; set; }

        internal QueryEntity(QueryCommand queryCommand) {
            Command = queryCommand;
        }

        public Model? GetFirstOrDefault(Expression<Func<Model, bool>>? where = null) => Get(where).FirstOrDefault();

        public List<Model> Get(Expression<Func<Model, bool>>? where = null) {

            var sql = new StringBuilder();

            sql.Append(this.MakeSelect());

            var qWhere = this.GetCondition(where, true);

            if(qWhere == null)
                return Command.Get<Model>(sql.ToString());

            sql.AppendLine("WHERE");
            sql.AppendLine(qWhere.Where);

            return Command.Get<Model>(sql.ToString(), null, qWhere.Values);

        }

        public bool Delete(Expression<Func<Model, bool>> where) {

            var sql = new StringBuilder();
            var qWhere = this.GetCondition(where);

            if (qWhere == null)
                throw new Exception($"Where condition obrigatory for delete '{typeof(Model).Name}'.");

            sql.AppendLine($"DELETE FROM {this.GetTableName()}");
            sql.AppendLine("WHERE");
            sql.AppendLine(qWhere.Where);

            return Command.Execute(sql.ToString(), null, qWhere.Values) > 0;


        }

        public bool Update(Model entity, Expression<Func<Model, bool>> where) {

            var sql = new StringBuilder();
            var qWhere = this.GetCondition(where);

            if (qWhere == null)
                throw new Exception($"Where condition obrigatory for update '{typeof(Model).Name}'.");

            sql.Append(this.MakeUpdateSQL());
            sql.AppendLine("WHERE");
            sql.AppendLine(qWhere.Where);

            return Command.Execute(sql.ToString(), entity, qWhere.Values) > 0;

        }

        public bool Insert(Model entity, EInsertType insertType = EInsertType.Insert) {

            var sql = this.MakeInsertSQL(insertType);

            return Command.Execute(sql.ToString(), entity) > 0;

        }

    }
}
