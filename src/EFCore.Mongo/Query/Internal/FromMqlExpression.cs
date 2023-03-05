// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

#nullable disable

using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Mongo.Query.Internal;
using Microsoft.EntityFrameworkCore.Query;
using System.Linq.Expressions;

namespace Microsoft.EntityFrameworkCore.Mongo.Query.Internal;

/// <summary>
///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
///     the same compatibility standards as public APIs. It may be changed or removed without notice in
///     any release. You should only use it directly in your code with extreme caution and knowing that
///     doing so can result in application failures when updating to a new Entity Framework Core release.
/// </summary>
public class FromMqlExpression : RootReferenceExpression, IPrintableExpression
{
    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public FromMqlExpression(IEntityType entityType, string alias, string mql, Expression arguments)
        : base(entityType, alias)
    {
        Mql = mql;
        Arguments = arguments;
    }

    /// <inheritdoc />
    public override string Alias
        => base.Alias!;

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual string Mql { get; }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual Expression Arguments { get; }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual FromMqlExpression Update(Expression arguments)
        => arguments != Arguments
            ? new FromMqlExpression(EntityType, Alias, Mql, arguments)
            : this;

    /// <inheritdoc />
    protected override Expression VisitChildren(ExpressionVisitor visitor)
        => this;

    /// <inheritdoc />
    public override Type Type
        => typeof(object);

    /// <inheritdoc />
    void IPrintableExpression.Print(ExpressionPrinter expressionPrinter)
        => expressionPrinter.Append(Mql);

    /// <inheritdoc />
    public override bool Equals(object obj)
        => obj != null
            && (ReferenceEquals(this, obj)
                || obj is FromMqlExpression fromMqlExpression
                && Equals(fromMqlExpression));

    private bool Equals(FromMqlExpression fromMqlExpression)
        => base.Equals(fromMqlExpression)
            && Mql == fromMqlExpression.Mql
            && ExpressionEqualityComparer.Instance.Equals(Arguments, fromMqlExpression.Arguments);

    /// <inheritdoc />
    public override int GetHashCode()
        => HashCode.Combine(base.GetHashCode(), Mql);
}
