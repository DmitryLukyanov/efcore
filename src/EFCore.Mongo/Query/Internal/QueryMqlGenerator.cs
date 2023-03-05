// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

#nullable disable

using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Mongo.Storage.Internal;
using Microsoft.EntityFrameworkCore.Storage;
using MongoDB.Bson;

namespace Microsoft.EntityFrameworkCore.Mongo.Query.Internal;

/// <summary>
///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
///     the same compatibility standards as public APIs. It may be changed or removed without notice in
///     any release. You should only use it directly in your code with extreme caution and knowing that
///     doing so can result in application failures when updating to a new Entity Framework Core release.
/// </summary>
public class QueryMqlGenerator : MqlExpressionVisitor
{
    private readonly ITypeMappingSource _typeMappingSource;
    private readonly IndentedStringBuilder _mqlBuilder = new();
    private IReadOnlyDictionary<string, object> _parameterValues;
    private List<MqlParameter> _sqlParameters;
#pragma warning disable IDE0044 // Add readonly modifier
    private bool _useValueProjection = true;
#pragma warning restore IDE0044 // Add readonly modifier
    private ParameterNameGenerator _parameterNameGenerator;

    private readonly IDictionary<ExpressionType, string> _operatorMap = new Dictionary<ExpressionType, string>
    {
        // Arithmetic
        { ExpressionType.Add, " + " },
        { ExpressionType.Subtract, " - " },
        { ExpressionType.Multiply, " * " },
        { ExpressionType.Divide, " / " },
        { ExpressionType.Modulo, " % " },

        // Bitwise >>> (zero-fill right shift) not available in C#
        { ExpressionType.Or, " | " },
        { ExpressionType.And, " & " },
        { ExpressionType.ExclusiveOr, " ^ " },
        { ExpressionType.LeftShift, " << " },
        { ExpressionType.RightShift, " >> " },

        // Logical
        { ExpressionType.AndAlso, " AND " },
        { ExpressionType.OrElse, " OR " },

        // Comparison
        { ExpressionType.Equal, " = " },
        { ExpressionType.NotEqual, " != " },
        { ExpressionType.GreaterThan, " > " },
        { ExpressionType.GreaterThanOrEqual, " >= " },
        { ExpressionType.LessThan, " < " },
        { ExpressionType.LessThanOrEqual, " <= " },

        // Unary
        { ExpressionType.UnaryPlus, "+" },
        { ExpressionType.Negate, "-" },
        { ExpressionType.Not, "~" }
    };

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public QueryMqlGenerator(ITypeMappingSource typeMappingSource)
    {
        _typeMappingSource = typeMappingSource;
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual MongoMqlQuery GetMqlQuery(
        SelectExpression selectExpression,
        IReadOnlyDictionary<string, object> parameterValues)
    {
        _mqlBuilder.Clear();
        _parameterValues = parameterValues;
        _sqlParameters = new List<MqlParameter>();
        _parameterNameGenerator = new ParameterNameGenerator();

        Visit(selectExpression);

        return new MongoMqlQuery(_mqlBuilder.ToString(), _sqlParameters);
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    protected override Expression VisitEntityProjection(EntityProjectionExpression entityProjectionExpression)
    {
        Visit(entityProjectionExpression.AccessExpression);

        return entityProjectionExpression;
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    protected override Expression VisitObjectArrayProjection(ObjectArrayProjectionExpression objectArrayProjectionExpression)
    {
        _mqlBuilder.Append(objectArrayProjectionExpression.ToString());

        return objectArrayProjectionExpression;
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    protected override Expression VisitKeyAccess(KeyAccessExpression keyAccessExpression)
    {
        _mqlBuilder.Append(keyAccessExpression.ToString());

        return keyAccessExpression;
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    protected override Expression VisitObjectAccess(ObjectAccessExpression objectAccessExpression)
    {
        _mqlBuilder.Append(objectAccessExpression.ToString());

        return objectAccessExpression;
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    protected override Expression VisitProjection(ProjectionExpression projectionExpression)
    {
        if (_useValueProjection)
        {
            _mqlBuilder.Append('"').Append(projectionExpression.Alias).Append("\" : ");
        }

        Visit(projectionExpression.Expression);

        if (!_useValueProjection
            && !string.IsNullOrEmpty(projectionExpression.Alias)
            && projectionExpression.Alias != projectionExpression.Name)
        {
            _mqlBuilder.Append(" AS " + projectionExpression.Alias);
        }

        return projectionExpression;
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    protected override Expression VisitRootReference(RootReferenceExpression rootReferenceExpression)
    {
        _mqlBuilder.Append(rootReferenceExpression.ToString());

        return rootReferenceExpression;
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    protected override Expression VisitSelect(SelectExpression selectExpression)
    {
        // TODO:

        _mqlBuilder.Append($"{{ $match : {{}} }}");

        //if (selectExpression.IsDistinct)
        //{
        //    throw new NotSupportedException();
        //}

        //if (selectExpression.Predicate != null)
        //{
        //    _mqlBuilder.AppendLine().Append("WHERE ");
        //    Visit(selectExpression.Predicate);
        //}



        //_mqlBuilder.Append("SELECT ");

        //if (selectExpression.IsDistinct)
        //{
        //    _mqlBuilder.Append("DISTINCT ");
        //}

        //if (selectExpression.Projection.Count > 0)
        //{
        //    if (selectExpression.Projection.Any(p => !string.IsNullOrEmpty(p.Alias) && p.Alias != p.Name)
        //        && !selectExpression.Projection.Any(p => p.Expression is MqlFunctionExpression)) // Aggregates are not allowed
        //    {
        //        _useValueProjection = true;
        //        _mqlBuilder.Append("VALUE {");
        //        GenerateList(selectExpression.Projection, e => Visit(e));
        //        _mqlBuilder.Append('}');
        //        _useValueProjection = false;
        //    }
        //    else
        //    {
        //        GenerateList(selectExpression.Projection, e => Visit(e));
        //    }
        //}
        //else
        //{
        //    _mqlBuilder.Append('1');
        //}

        //_mqlBuilder.AppendLine();

        //_mqlBuilder.Append(selectExpression.FromExpression is FromMqlExpression ? "FROM " : "FROM root ");

        //Visit(selectExpression.FromExpression);

        //if (selectExpression.Predicate != null)
        //{
        //    _mqlBuilder.AppendLine().Append("WHERE ");
        //    Visit(selectExpression.Predicate);
        //}

        //if (selectExpression.Orderings.Any())
        //{
        //    _mqlBuilder.AppendLine().Append("ORDER BY ");

        //    GenerateList(selectExpression.Orderings, e => Visit(e));
        //}

        //if (selectExpression.Offset != null
        //    || selectExpression.Limit != null)
        //{
        //    _mqlBuilder.AppendLine().Append("OFFSET ");

        //    if (selectExpression.Offset != null)
        //    {
        //        Visit(selectExpression.Offset);
        //    }
        //    else
        //    {
        //        _mqlBuilder.Append('0');
        //    }

        //    _mqlBuilder.Append(" LIMIT ");

        //    if (selectExpression.Limit != null)
        //    {
        //        Visit(selectExpression.Limit);
        //    }
        //    else
        //    {
        //        // TODO: See Issue#18923
        //        throw new InvalidOperationException("CosmosStrings.OffsetRequiresLimit");
        //    }
        //}

        return selectExpression;
    }

    /// <inheritdoc />
    protected override Expression VisitFromSql(FromMqlExpression fromMqlExpression)
    {
        var sql = fromMqlExpression.Mql;

        string[] substitutions;

        switch (fromMqlExpression.Arguments)
        {
            case ParameterExpression { Name: not null } parameterExpression
                when _parameterValues.TryGetValue(parameterExpression.Name, out var parameterValue)
                && parameterValue is object[] parameterValues:
            {
                substitutions = new string[parameterValues.Length];
                for (var i = 0; i < parameterValues.Length; i++)
                {
                    var parameterName = _parameterNameGenerator.GenerateNext();
                    _sqlParameters.Add(new MqlParameter(parameterName, parameterValues[i]));
                    substitutions[i] = parameterName;
                }

                break;
            }

            case ConstantExpression { Value: object[] constantValues }:
            {
                substitutions = new string[constantValues.Length];
                for (var i = 0; i < constantValues.Length; i++)
                {
                    var value = constantValues[i];
                    substitutions[i] = GenerateConstant(value, _typeMappingSource.FindMapping(value.GetType()));
                }

                break;
            }

            default:
                throw new ArgumentOutOfRangeException(
                    nameof(fromMqlExpression),
                    fromMqlExpression.Arguments,
                    @"CosmosStrings.InvalidFromSqlArguments(
                        fromSqlExpression.Arguments.GetType(),
                        fromSqlExpression.Arguments is ConstantExpression constantExpression
                            ? constantExpression.Value?.GetType()
                            : null)");
        }

        // ReSharper disable once CoVariantArrayConversion
        // InvariantCulture not needed since substitutions are all strings
        sql = string.Format(sql, substitutions);

        _mqlBuilder.AppendLine("(");

        using (_mqlBuilder.Indent())
        {
            _mqlBuilder.AppendLines(sql);
        }

        _mqlBuilder
            .Append(") ")
            .Append(fromMqlExpression.Alias);

        return fromMqlExpression;
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    protected override Expression VisitOrdering(OrderingExpression orderingExpression)
    {
        Visit(orderingExpression.Expression);

        if (!orderingExpression.IsAscending)
        {
            _mqlBuilder.Append(" DESC");
        }

        return orderingExpression;
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    protected override Expression VisitSqlBinary(MqlBinaryExpression mqlBinaryExpression)
    {
        var op = _operatorMap[mqlBinaryExpression.OperatorType];
        _mqlBuilder.Append('(');
        Visit(mqlBinaryExpression.Left);

        if (mqlBinaryExpression.OperatorType == ExpressionType.Add
            && mqlBinaryExpression.Left.Type == typeof(string))
        {
            op = " || ";
        }

        _mqlBuilder.Append(op);

        Visit(mqlBinaryExpression.Right);
        _mqlBuilder.Append(')');

        return mqlBinaryExpression;
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    protected override Expression VisitSqlUnary(MqlUnaryExpression mqlUnaryExpression)
    {
        var op = _operatorMap[mqlUnaryExpression.OperatorType];

        if (mqlUnaryExpression.OperatorType == ExpressionType.Not
            && mqlUnaryExpression.Operand.Type == typeof(bool))
        {
            op = "NOT";
        }

        _mqlBuilder.Append(op);

        _mqlBuilder.Append('(');
        Visit(mqlUnaryExpression.Operand);
        _mqlBuilder.Append(')');

        return mqlUnaryExpression;
    }

    private void GenerateList<T>(
        IReadOnlyList<T> items,
        Action<T> generationAction,
        Action<IndentedStringBuilder> joinAction = null)
    {
        joinAction ??= (isb => isb.Append(", "));

        for (var i = 0; i < items.Count; i++)
        {
            if (i > 0)
            {
                joinAction(_mqlBuilder);
            }

            generationAction(items[i]);
        }
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    protected override Expression VisitSqlConstant(MqlConstantExpression mqlConstantExpression)
    {
        _mqlBuilder.Append(GenerateConstant(mqlConstantExpression.Value, mqlConstantExpression.TypeMapping));

        return mqlConstantExpression;
    }

    private static string GenerateConstant(object value, CoreTypeMapping typeMapping)
    {
        var jToken = GenerateToken(value, typeMapping);

        return jToken is null ? "null" : jToken.ToString();
    }

    private static BsonValue GenerateToken(object value, CoreTypeMapping typeMapping)
    {
        if (value?.GetType().IsInteger() == true)
        {
            var unwrappedType = typeMapping.ClrType.UnwrapNullableType();
            value = unwrappedType.IsEnum
                ? Enum.ToObject(unwrappedType, value)
                : unwrappedType == typeof(char)
                    ? Convert.ChangeType(value, unwrappedType)
                    : value;
        }

        var converter = typeMapping.Converter;
        if (converter != null)
        {
            value = converter.ConvertToProvider(value);
        }

        return value == null
            ? null
            : (value as BsonValue) ?? BsonValue.Create(value/*, MongoClientWrapper.Serializer*/);
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    protected override Expression VisitSqlConditional(MqlConditionalExpression mqlConditionalExpression)
    {
        _mqlBuilder.Append('(');
        Visit(mqlConditionalExpression.Test);
        _mqlBuilder.Append(" ? ");
        Visit(mqlConditionalExpression.IfTrue);
        _mqlBuilder.Append(" : ");
        Visit(mqlConditionalExpression.IfFalse);
        _mqlBuilder.Append(')');

        return mqlConditionalExpression;
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    protected override Expression VisitSqlParameter(MqlParameterExpression mqlParameterExpression)
    {
        var parameterName = $"@{mqlParameterExpression.Name}";

        if (_sqlParameters.All(sp => sp.Name != parameterName))
        {
            var jToken = GenerateToken(_parameterValues[mqlParameterExpression.Name], mqlParameterExpression.TypeMapping);
            _sqlParameters.Add(new MqlParameter(parameterName, jToken));
        }

        _mqlBuilder.Append(parameterName);

        return mqlParameterExpression;
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    protected override Expression VisitIn(InExpression inExpression)
    {
        Visit(inExpression.Item);
        _mqlBuilder.Append(inExpression.IsNegated ? " NOT IN " : " IN ");
        _mqlBuilder.Append('(');
        var valuesConstant = (MqlConstantExpression)inExpression.Values;
        var valuesList = ((IEnumerable<object>)valuesConstant.Value)
            .Select(v => new MqlConstantExpression(Expression.Constant(v), valuesConstant.TypeMapping)).ToList();
        GenerateList(valuesList, e => Visit(e));
        _mqlBuilder.Append(')');

        return inExpression;
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    protected override Expression VisitSqlFunction(MqlFunctionExpression mqlFunctionExpression)
    {
        _mqlBuilder.Append(mqlFunctionExpression.Name);
        _mqlBuilder.Append('(');
        GenerateList(mqlFunctionExpression.Arguments, e => Visit(e));
        _mqlBuilder.Append(')');

        return mqlFunctionExpression;
    }

    private sealed class ParameterNameGenerator
    {
        private int _count;

        public string GenerateNext()
            => "@p" + _count++;
    }
}
