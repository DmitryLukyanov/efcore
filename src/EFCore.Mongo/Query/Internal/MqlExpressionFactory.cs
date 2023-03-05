// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Diagnostics.CodeAnalysis;
using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.EntityFrameworkCore.Storage;

namespace Microsoft.EntityFrameworkCore.Mongo.Query.Internal;

/// <summary>
///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
///     the same compatibility standards as public APIs. It may be changed or removed without notice in
///     any release. You should only use it directly in your code with extreme caution and knowing that
///     doing so can result in application failures when updating to a new Entity Framework Core release.
/// </summary>
public class MqlExpressionFactory : IMqlExpressionFactory
{
    private readonly ITypeMappingSource _typeMappingSource;
    private readonly IModel _model;
    private readonly CoreTypeMapping _boolTypeMapping;

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public MqlExpressionFactory(ITypeMappingSource typeMappingSource, IModel model)
    {
        _typeMappingSource = typeMappingSource;
        _model = model;
        _boolTypeMapping = typeMappingSource.FindMapping(typeof(bool), model)!;
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    [return: NotNullIfNotNull("mqlExpression")]
    public virtual MqlExpression? ApplyDefaultTypeMapping(MqlExpression? mqlExpression)
        => mqlExpression == null
            || mqlExpression.TypeMapping != null
                ? mqlExpression
                : ApplyTypeMapping(mqlExpression, _typeMappingSource.FindMapping(mqlExpression.Type, _model));

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    [return: NotNullIfNotNull("mqlExpression")]
    public virtual MqlExpression? ApplyTypeMapping(MqlExpression? mqlExpression, CoreTypeMapping? typeMapping)
    {
        if (mqlExpression == null
            || mqlExpression.TypeMapping != null)
        {
            return mqlExpression;
        }

#pragma warning disable IDE0066 // Convert switch statement to expression
        switch (mqlExpression)
#pragma warning restore IDE0066 // Convert switch statement to expression
        {
            case MqlConditionalExpression mqlConditionalExpression:
                return ApplyTypeMappingOnSqlConditional(mqlConditionalExpression, typeMapping);

            case MqlBinaryExpression mqlBinaryExpression:
                return ApplyTypeMappingOnSqlBinary(mqlBinaryExpression, typeMapping);

            case MqlUnaryExpression mqlUnaryExpression:
                return ApplyTypeMappingOnSqlUnary(mqlUnaryExpression, typeMapping);

            case MqlConstantExpression mqlConstantExpression:
                return mqlConstantExpression.ApplyTypeMapping(typeMapping);

            case MqlParameterExpression mqlParameterExpression:
                return mqlParameterExpression.ApplyTypeMapping(typeMapping);

            case MqlFunctionExpression mqlFunctionExpression:
                return mqlFunctionExpression.ApplyTypeMapping(typeMapping);

            default:
                return mqlExpression;
        }
    }

    private MqlExpression ApplyTypeMappingOnSqlConditional(
        MqlConditionalExpression sqlConditionalExpression,
        CoreTypeMapping? typeMapping)
        => sqlConditionalExpression.Update(
            sqlConditionalExpression.Test,
            ApplyTypeMapping(sqlConditionalExpression.IfTrue, typeMapping),
            ApplyTypeMapping(sqlConditionalExpression.IfFalse, typeMapping));

    private MqlExpression ApplyTypeMappingOnSqlUnary(
        MqlUnaryExpression mqlUnaryExpression,
        CoreTypeMapping? typeMapping)
    {
        MqlExpression operand;
        Type resultType;
        CoreTypeMapping? resultTypeMapping;
        switch (mqlUnaryExpression.OperatorType)
        {
            case ExpressionType.Equal:
            case ExpressionType.NotEqual:
            case ExpressionType.Not
                when mqlUnaryExpression.IsLogicalNot():
            {
                resultTypeMapping = _boolTypeMapping;
                resultType = typeof(bool);
                operand = ApplyDefaultTypeMapping(mqlUnaryExpression.Operand);
                break;
            }

            case ExpressionType.Convert:
                resultTypeMapping = typeMapping;
                // Since we are applying convert, resultTypeMapping decides the clrType
                resultType = resultTypeMapping?.ClrType ?? mqlUnaryExpression.Type;
                operand = ApplyDefaultTypeMapping(mqlUnaryExpression.Operand);
                break;

            case ExpressionType.Not:
            case ExpressionType.Negate:
                resultTypeMapping = typeMapping;
                // While Not is logical, negate is numeric hence we use clrType from TypeMapping
                resultType = resultTypeMapping?.ClrType ?? mqlUnaryExpression.Type;
                operand = ApplyTypeMapping(mqlUnaryExpression.Operand, typeMapping);
                break;

            default:
                throw new InvalidOperationException(
                    "CosmosStrings.UnsupportedOperatorForSqlExpression(sqlUnaryExpression.OperatorType, typeof(SqlUnaryExpression).ShortDisplayName())");
        }

        return new MqlUnaryExpression(mqlUnaryExpression.OperatorType, operand, resultType, resultTypeMapping);
    }

    private MqlExpression ApplyTypeMappingOnSqlBinary(
        MqlBinaryExpression mqlBinaryExpression,
        CoreTypeMapping? typeMapping)
    {
        var left = mqlBinaryExpression.Left;
        var right = mqlBinaryExpression.Right;

        Type resultType;
        CoreTypeMapping? resultTypeMapping;
        CoreTypeMapping? inferredTypeMapping;
        switch (mqlBinaryExpression.OperatorType)
        {
            case ExpressionType.Equal:
            case ExpressionType.GreaterThan:
            case ExpressionType.GreaterThanOrEqual:
            case ExpressionType.LessThan:
            case ExpressionType.LessThanOrEqual:
            case ExpressionType.NotEqual:
            {
                inferredTypeMapping = ExpressionExtensions.InferTypeMapping(left, right)
                    // We avoid object here since the result does not get typeMapping from outside.
                    ?? (left.Type != typeof(object)
                        ? _typeMappingSource.FindMapping(left.Type, _model)
                        : _typeMappingSource.FindMapping(right.Type, _model));
                resultType = typeof(bool);
                resultTypeMapping = _boolTypeMapping;
            }
            break;

            case ExpressionType.AndAlso:
            case ExpressionType.OrElse:
            {
                inferredTypeMapping = _boolTypeMapping;
                resultType = typeof(bool);
                resultTypeMapping = _boolTypeMapping;
            }
            break;

            case ExpressionType.Add:
            case ExpressionType.Subtract:
            case ExpressionType.Multiply:
            case ExpressionType.Divide:
            case ExpressionType.Modulo:
            case ExpressionType.LeftShift:
            case ExpressionType.RightShift:
            case ExpressionType.And:
            case ExpressionType.Or:
            {
                inferredTypeMapping = typeMapping ?? ExpressionExtensions.InferTypeMapping(left, right);
                resultType = inferredTypeMapping?.ClrType ?? left.Type;
                resultTypeMapping = inferredTypeMapping;
            }
            break;

            default:
                throw new InvalidOperationException("CosmosStrings.UnsupportedOperatorForSqlExpression(sqlBinaryExpression.OperatorType, typeof(SqlBinaryExpression).ShortDisplayName())");
        }

        return new MqlBinaryExpression(
            mqlBinaryExpression.OperatorType,
            ApplyTypeMapping(left, inferredTypeMapping),
            ApplyTypeMapping(right, inferredTypeMapping),
            resultType,
            resultTypeMapping);
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual MqlBinaryExpression MakeBinary(
        ExpressionType operatorType,
        MqlExpression left,
        MqlExpression right,
        CoreTypeMapping? typeMapping)
    {
        var returnType = left.Type;
        switch (operatorType)
        {
            case ExpressionType.Equal:
            case ExpressionType.GreaterThan:
            case ExpressionType.GreaterThanOrEqual:
            case ExpressionType.LessThan:
            case ExpressionType.LessThanOrEqual:
            case ExpressionType.NotEqual:
            case ExpressionType.AndAlso:
            case ExpressionType.OrElse:
                returnType = typeof(bool);
                break;
        }

        return (MqlBinaryExpression)ApplyTypeMapping(
            new MqlBinaryExpression(operatorType, left, right, returnType, null), typeMapping);
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual MqlBinaryExpression Equal(MqlExpression left, MqlExpression right)
        => MakeBinary(ExpressionType.Equal, left, right, null);

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual MqlBinaryExpression NotEqual(MqlExpression left, MqlExpression right)
        => MakeBinary(ExpressionType.NotEqual, left, right, null);

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual MqlBinaryExpression GreaterThan(MqlExpression left, MqlExpression right)
        => MakeBinary(ExpressionType.GreaterThan, left, right, null);

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual MqlBinaryExpression GreaterThanOrEqual(MqlExpression left, MqlExpression right)
        => MakeBinary(ExpressionType.GreaterThanOrEqual, left, right, null);

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual MqlBinaryExpression LessThan(MqlExpression left, MqlExpression right)
        => MakeBinary(ExpressionType.LessThan, left, right, null);

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual MqlBinaryExpression LessThanOrEqual(MqlExpression left, MqlExpression right)
        => MakeBinary(ExpressionType.LessThanOrEqual, left, right, null);

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual MqlBinaryExpression AndAlso(MqlExpression left, MqlExpression right)
        => MakeBinary(ExpressionType.AndAlso, left, right, null);

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual MqlBinaryExpression OrElse(MqlExpression left, MqlExpression right)
        => MakeBinary(ExpressionType.OrElse, left, right, null);

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual MqlBinaryExpression Add(MqlExpression left, MqlExpression right, CoreTypeMapping? typeMapping = null)
        => MakeBinary(ExpressionType.Add, left, right, typeMapping);

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual MqlBinaryExpression Subtract(MqlExpression left, MqlExpression right, CoreTypeMapping? typeMapping = null)
        => MakeBinary(ExpressionType.Subtract, left, right, typeMapping);

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual MqlBinaryExpression Multiply(MqlExpression left, MqlExpression right, CoreTypeMapping? typeMapping = null)
        => MakeBinary(ExpressionType.Multiply, left, right, typeMapping);

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual MqlBinaryExpression Divide(MqlExpression left, MqlExpression right, CoreTypeMapping? typeMapping = null)
        => MakeBinary(ExpressionType.Divide, left, right, typeMapping);

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual MqlBinaryExpression Modulo(MqlExpression left, MqlExpression right, CoreTypeMapping? typeMapping = null)
        => MakeBinary(ExpressionType.Modulo, left, right, typeMapping);

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual MqlBinaryExpression And(MqlExpression left, MqlExpression right, CoreTypeMapping? typeMapping = null)
        => MakeBinary(ExpressionType.And, left, right, typeMapping);

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual MqlBinaryExpression Or(MqlExpression left, MqlExpression right, CoreTypeMapping? typeMapping = null)
        => MakeBinary(ExpressionType.Or, left, right, typeMapping);

    private MqlUnaryExpression MakeUnary(
        ExpressionType operatorType,
        MqlExpression operand,
        Type type,
        CoreTypeMapping? typeMapping = null)
        => (MqlUnaryExpression)ApplyTypeMapping(new MqlUnaryExpression(operatorType, operand, type, null), typeMapping);

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual MqlBinaryExpression IsNull(MqlExpression operand)
        => Equal(operand, Constant(null));

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual MqlBinaryExpression IsNotNull(MqlExpression operand)
        => NotEqual(operand, Constant(null));

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual MqlUnaryExpression Convert(MqlExpression operand, Type type, CoreTypeMapping? typeMapping = null)
        => MakeUnary(ExpressionType.Convert, operand, type, typeMapping);

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual MqlUnaryExpression Not(MqlExpression operand)
        => MakeUnary(ExpressionType.Not, operand, operand.Type, operand.TypeMapping);

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual MqlUnaryExpression Negate(MqlExpression operand)
        => MakeUnary(ExpressionType.Negate, operand, operand.Type, operand.TypeMapping);

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual MqlFunctionExpression Function(
        string functionName,
        IEnumerable<MqlExpression> arguments,
        Type returnType,
        CoreTypeMapping? typeMapping = null)
    {
        var typeMappedArguments = new List<MqlExpression>();

        foreach (var argument in arguments)
        {
            typeMappedArguments.Add(ApplyDefaultTypeMapping(argument));
        }

        return new MqlFunctionExpression(
            functionName,
            typeMappedArguments,
            returnType,
            typeMapping);
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual MqlConditionalExpression Condition(MqlExpression test, MqlExpression ifTrue, MqlExpression ifFalse)
    {
        var typeMapping = ExpressionExtensions.InferTypeMapping(ifTrue, ifFalse);

        return new MqlConditionalExpression(
            ApplyTypeMapping(test, _boolTypeMapping),
            ApplyTypeMapping(ifTrue, typeMapping),
            ApplyTypeMapping(ifFalse, typeMapping));
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual InExpression In(MqlExpression item, MqlExpression values, bool negated)
    {
        var typeMapping = item.TypeMapping ?? _typeMappingSource.FindMapping(item.Type, _model);

        item = ApplyTypeMapping(item, typeMapping);
        values = ApplyTypeMapping(values, typeMapping);

        return new InExpression(item, negated, values, _boolTypeMapping);
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual MqlConstantExpression Constant(object? value, CoreTypeMapping? typeMapping = null)
        => new(Expression.Constant(value), typeMapping);

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual SelectExpression Select(IEntityType entityType)
    {
        var selectExpression = new SelectExpression(entityType);
        AddDiscriminator(selectExpression, entityType);

        return selectExpression;
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual SelectExpression Select(IEntityType entityType, string sql, Expression argument)
        => new(entityType, sql, argument);

    private void AddDiscriminator(SelectExpression selectExpression, IEntityType entityType)
    {
        var concreteEntityTypes = entityType.GetConcreteDerivedTypesInclusive().ToList();

        if (concreteEntityTypes.Count == 1)
        {
            var concreteEntityType = concreteEntityTypes[0];
            var discriminatorProperty = concreteEntityType.FindDiscriminatorProperty();
            if (discriminatorProperty != null)
            {
                var discriminatorColumn = ((EntityProjectionExpression)selectExpression.GetMappedProjection(new ProjectionMember()))
                    .BindProperty(discriminatorProperty, clientEval: false);

                selectExpression.ApplyPredicate(
                    Equal((MqlExpression)discriminatorColumn, Constant(concreteEntityType.GetDiscriminatorValue())));
            }
        }
        else
        {
            var discriminatorColumn = ((EntityProjectionExpression)selectExpression.GetMappedProjection(new ProjectionMember()))
                .BindProperty(concreteEntityTypes[0].FindDiscriminatorProperty(), clientEval: false);

            selectExpression.ApplyPredicate(
                In(
                    (MqlExpression)discriminatorColumn, Constant(concreteEntityTypes.Select(et => et.GetDiscriminatorValue()).ToList()),
                    negated: false));
        }
    }
}
