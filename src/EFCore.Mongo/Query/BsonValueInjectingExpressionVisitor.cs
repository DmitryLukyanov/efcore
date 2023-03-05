// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

#nullable disable

using Microsoft.EntityFrameworkCore.Query;
using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore.Query.Internal;
using MongoDB.Bson;

namespace Microsoft.EntityFrameworkCore.Mongo.Query.Internal;

/// <summary>
/// Summary.
/// </summary>
public partial class MongoShapedQueryCompilingExpressionVisitor
{
    internal sealed class BsonValueInjectingExpressionVisitor : ExpressionVisitor
    {
        private int _currentEntityIndex;

        protected override Expression VisitExtension(Expression extensionExpression)
        {
            switch (extensionExpression)
            {
                case EntityShaperExpression shaperExpression:
                {
                    _currentEntityIndex++;

                    var valueBufferExpression = shaperExpression.ValueBufferExpression;

                    var jObjectVariable = Expression.Variable(
                        typeof(BsonDocument),
                        "BsonDocument" + _currentEntityIndex);
                    var variables = new List<ParameterExpression> { jObjectVariable };

                    var expressions = new List<Expression>
                    {
                        Expression.Assign(
                            jObjectVariable,
                            Expression.TypeAs(
                                valueBufferExpression,
                                typeof(BsonDocument))),
                        Expression.Condition(
                            Expression.Equal(jObjectVariable, Expression.Constant(null, jObjectVariable.Type)),
                            Expression.Constant(null, shaperExpression.Type),
                            shaperExpression)
                    };

                    return Expression.Block(
                        shaperExpression.Type,
                        variables,
                        expressions);
                }

                case CollectionShaperExpression collectionShaperExpression:
                {
                    _currentEntityIndex++;

                    var jArrayVariable = Expression.Variable(
                        typeof(BsonArray),
                        "jArray" + _currentEntityIndex);
                    var variables = new List<ParameterExpression> { jArrayVariable };

                    var expressions = new List<Expression>
                    {
                        Expression.Assign(
                            jArrayVariable,
                            Expression.TypeAs(
                                collectionShaperExpression.Projection,
                                typeof(BsonArray))),
                        Expression.Condition(
                            Expression.Equal(jArrayVariable, Expression.Constant(null, jArrayVariable.Type)),
                            Expression.Constant(null, collectionShaperExpression.Type),
                            collectionShaperExpression)
                    };

                    return Expression.Block(
                        collectionShaperExpression.Type,
                        variables,
                        expressions);
                }
            }

            return base.VisitExtension(extensionExpression);
        }
    }
}
