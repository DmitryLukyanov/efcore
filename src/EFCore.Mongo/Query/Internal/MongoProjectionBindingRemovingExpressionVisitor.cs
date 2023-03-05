// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

#nullable disable

using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Mongo.Query.Internal;
using Microsoft.EntityFrameworkCore.Query;
using System.Linq.Expressions;

namespace Microsoft.EntityFrameworkCore.Mongo.Query.Internal;

/// <summary>
/// ddd.
/// </summary>
public partial class MongoShapedQueryCompilingExpressionVisitor
{
    private sealed class MongoProjectionBindingRemovingExpressionVisitor : MongoProjectionBindingRemovingExpressionVisitorBase
    {
        private readonly SelectExpression _selectExpression;

        public MongoProjectionBindingRemovingExpressionVisitor(
            SelectExpression selectExpression,
            ParameterExpression jObjectParameter,
            bool trackQueryResults)
            : base(jObjectParameter, trackQueryResults)
        {
            _selectExpression = selectExpression;
        }

        protected override ProjectionExpression GetProjection(ProjectionBindingExpression projectionBindingExpression)
            => _selectExpression.Projection[GetProjectionIndex(projectionBindingExpression)];

        private int GetProjectionIndex(ProjectionBindingExpression projectionBindingExpression)
        {
            if (projectionBindingExpression.ProjectionMember != null)
            {
                var projection = _selectExpression.GetMappedProjection(projectionBindingExpression.ProjectionMember);
                return projection.GetConstantValue<int>();
            }
            else
            {
                return (projectionBindingExpression.Index
                            ?? throw new InvalidOperationException(CoreStrings.TranslationFailed(projectionBindingExpression.Print())));
            }
        }
    }
}
