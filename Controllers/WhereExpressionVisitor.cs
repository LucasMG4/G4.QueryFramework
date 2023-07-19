using G4.QueryFramework.Attributes;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;

namespace G4.QueryFramework.Controllers {
    public class MySqlExpressionVisitor : ExpressionVisitor {
        private int parameterIndex;
        private Dictionary<string, object> parameters;
        private StringBuilder whereClause;

        public string WhereClause => whereClause.ToString().TrimStart();

        public Dictionary<string, object> Parameters => parameters;

        public MySqlExpressionVisitor() {
            parameterIndex = 1;
            parameters = new Dictionary<string, object>();
            whereClause = new StringBuilder();
        }

        protected override Expression VisitBinary(BinaryExpression node) {
            whereClause.Append("(");
            Visit(node.Left);
            whereClause.Append(GetOperator(node.NodeType));
            Visit(node.Right);
            whereClause.Append(")");
            return node;
        }

        private string memberNameOld = "";
        private string memberName = "";

        protected override Expression VisitMember(MemberExpression node) {

            var memberName = node.Member.Name;

            if (node.Expression is ParameterExpression) {
                
                whereClause.Append($"n01.{GetMemberName(node)}");
                return node;
            }

            if (node.Expression is ConstantExpression constantExpression && node.Expression.Type.Name.StartsWith("<>c__DisplayClass")) {
                
                object? memberValue = GetMemberValue(constantExpression.Value, node);

                if(memberValue != null) {
                    if(!memberNameOld.Equals("")) {
                        var property = memberValue.GetType().GetProperty(memberNameOld);
                        if(property != null) {
                            memberValue = property.GetValue(memberValue, null);
                        }
                    }
                }

                var parameterName = $"@p{parameterIndex++}";
                parameters.Add(parameterName, memberValue);
                whereClause.Append(parameterName);
                return node;
            }

            if (node.NodeType == ExpressionType.MemberAccess) {

                memberNameOld = memberName;
                memberName = node.Member.Name;

            }

            return base.VisitMember(node);
        }

        protected override Expression VisitConstant(ConstantExpression node) {
            string parameterName = $"@p{parameterIndex++}";
            parameters.Add(parameterName, node.Value);
            whereClause.Append(parameterName);
            return node;
        }

        private string GetMemberName(MemberExpression node) {

            if (node.Member is PropertyInfo propertyInfo) {
                
                var attribute = propertyInfo.GetCustomAttributes(typeof(ColumnConfigureAttribute), true).FirstOrDefault() as ColumnConfigureAttribute;
                if (attribute == null)
                    throw new Exception($"Where clause '{propertyInfo.Name}' not have ColumnConfigure attribute.");

                return attribute.columnName;

            }

            throw new NotSupportedException($"Unsupported member type: {node.Member.GetType()}");
        }

        private object GetMemberValue(object container, MemberExpression memberExpression) {

            if (memberExpression.Expression is MemberExpression nestedMemberExpression) {
                var nestedContainer = GetMemberValue(container, nestedMemberExpression);
                return GetMemberValue(nestedContainer, memberExpression);
            }

            if(memberExpression.Expression is ConstantExpression constantExpression) {
                object value = ((FieldInfo) memberExpression.Member).GetValue(container);
                return value;
            }

            throw new NotSupportedException($"Unsupported member expression: {memberExpression}");
        }

        private string GetOperator(ExpressionType nodeType) {
            switch (nodeType) {
                case ExpressionType.Equal:
                return " = ";
                case ExpressionType.NotEqual:
                return " <> ";
                case ExpressionType.GreaterThan:
                return " > ";
                case ExpressionType.GreaterThanOrEqual:
                return " >= ";
                case ExpressionType.LessThan:
                return " < ";
                case ExpressionType.LessThanOrEqual:
                return " <= ";
                case ExpressionType.And:
                case ExpressionType.AndAlso:
                return " AND ";
                case ExpressionType.Or:
                case ExpressionType.OrElse:
                return " OR ";
                default:
                throw new NotSupportedException($"Unsupported binary operator: {nodeType}");
            }
        }
    }

}
