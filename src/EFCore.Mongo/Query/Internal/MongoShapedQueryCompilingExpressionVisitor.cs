// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

#nullable disable

using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Query;
using System.Linq.Expressions;
using System.Text;
using MongoDB.Bson;
using System.Collections;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.EntityFrameworkCore.Infrastructure;

namespace Microsoft.EntityFrameworkCore.Mongo.Query.Internal;

/// <summary>
///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
///     the same compatibility standards as public APIs. It may be changed or removed without notice in
///     any release. You should only use it directly in your code with extreme caution and knowing that
///     doing so can result in application failures when updating to a new Entity Framework Core release.
/// </summary>
public class MqlParameter
{
    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public MqlParameter(string name, object value)
    {
        Name = name;
        Value = value;
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual string Name { get; }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual object Value { get; }
}

internal sealed class InExpressionValuesExpandingExpressionVisitor : ExpressionVisitor
{
    private readonly IMqlExpressionFactory _mqlExpressionFactory;
    private readonly IReadOnlyDictionary<string, object> _parametersValues;

    public InExpressionValuesExpandingExpressionVisitor(
        IMqlExpressionFactory mqlExpressionFactory,
        IReadOnlyDictionary<string, object> parametersValues)
    {
        _mqlExpressionFactory = mqlExpressionFactory;
        _parametersValues = parametersValues;
    }

    public override Expression Visit(Expression expression)
    {
        if (expression is InExpression inExpression)
        {
            var inValues = new List<object>();
            var hasNullValue = false;
            CoreTypeMapping typeMapping = null;

            switch (inExpression.Values)
            {
                case MqlConstantExpression mqlConstant:
                {
                    typeMapping = mqlConstant.TypeMapping;
                    var values = (IEnumerable)mqlConstant.Value;
                    foreach (var value in values)
                    {
                        if (value == null)
                        {
                            hasNullValue = true;
                            continue;
                        }

                        inValues.Add(value);
                    }
                }
                break;

                case MqlParameterExpression mqlParameter:
                {
                    typeMapping = mqlParameter.TypeMapping;
                    var values = (IEnumerable)_parametersValues[mqlParameter.Name];
                    foreach (var value in values)
                    {
                        if (value == null)
                        {
                            hasNullValue = true;
                            continue;
                        }

                        inValues.Add(value);
                    }
                }
                break;
            }

            var updatedInExpression = inValues.Count > 0
                ? _mqlExpressionFactory.In(
                    (MqlExpression)Visit(inExpression.Item),
                    _mqlExpressionFactory.Constant(inValues, typeMapping),
                    inExpression.IsNegated)
                : null;

            var nullCheckExpression = hasNullValue
                ? _mqlExpressionFactory.IsNull(inExpression.Item)
                : null;

            if (updatedInExpression != null
                && nullCheckExpression != null)
            {
                return _mqlExpressionFactory.OrElse(updatedInExpression, nullCheckExpression);
            }

            if (updatedInExpression == null
                && nullCheckExpression == null)
            {
                return _mqlExpressionFactory.Equal(_mqlExpressionFactory.Constant(true), _mqlExpressionFactory.Constant(false));
            }

            return (MqlExpression)updatedInExpression ?? nullCheckExpression;
        }

        return base.Visit(expression);
    }
}

/// <summary>
///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
///     the same compatibility standards as public APIs. It may be changed or removed without notice in
///     any release. You should only use it directly in your code with extreme caution and knowing that
///     doing so can result in application failures when updating to a new Entity Framework Core release.
/// </summary>
public class MongoMqlQuery
{
    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public MongoMqlQuery(string query, IReadOnlyList<MqlParameter> parameters)
    {
        Query = query;
        Parameters = parameters;
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual string Query { get; }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual IReadOnlyList<MqlParameter> Parameters { get; }
}

/// <summary>
///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
///     the same compatibility standards as public APIs. It may be changed or removed without notice in
///     any release. You should only use it directly in your code with extreme caution and knowing that
///     doing so can result in application failures when updating to a new Entity Framework Core release.
/// </summary>
public sealed class QueryingEnumerable<T> : IEnumerable<T>, IAsyncEnumerable<T>, IQueryingEnumerable
{
    private sealed class Enumerator : IEnumerator<T>
    {
        private readonly QueryingEnumerable<T> _queryingEnumerable;
        private readonly MongoQueryContext _mongoQueryContext;
        private readonly SelectExpression _selectExpression;
        private readonly Func<MongoQueryContext, BsonDocument, T> _shaper;
        private readonly Type _contextType;
        //private readonly string _partitionKey;
        private readonly IDiagnosticsLogger<DbLoggerCategory.Query> _queryLogger;
        private readonly bool _standAloneStateManager;
        private readonly IConcurrencyDetector _concurrencyDetector;
        private readonly IExceptionDetector _exceptionDetector;

        private IEnumerator<BsonDocument> _enumerator;

        public Enumerator(QueryingEnumerable<T> queryingEnumerable)
        {
            _queryingEnumerable = queryingEnumerable;
            _mongoQueryContext = queryingEnumerable._mongoQueryContext;
            _shaper = queryingEnumerable._shaper;
            _selectExpression = queryingEnumerable._selectExpression;
            _contextType = queryingEnumerable._contextType;
            //_partitionKey = queryingEnumerable._partitionKey;
            _queryLogger = queryingEnumerable._queryLogger;
            _standAloneStateManager = queryingEnumerable._standAloneStateManager;
            _exceptionDetector = _mongoQueryContext.ExceptionDetector;

            _concurrencyDetector = queryingEnumerable._threadSafetyChecksEnabled
                ? _mongoQueryContext.ConcurrencyDetector
                : null;
        }

        public T Current { get; private set; }

        object IEnumerator.Current
            => Current;

        public bool MoveNext()
        {
            try
            {
                _concurrencyDetector?.EnterCriticalSection();

                try
                {
                    if (_enumerator == null)
                    {
                        var sqlQuery = _queryingEnumerable.GenerateQuery();

                        EntityFrameworkEventSource.Log.QueryExecuting();

                        _enumerator =
                            _mongoQueryContext.MongoClient
                            .ExecuteMqlQuery(
                                _selectExpression.CollectionName,
                                //_partitionKey,
                                sqlQuery)
                            .GetEnumerator();
                        _mongoQueryContext.InitializeStateManager(_standAloneStateManager);
                    }

                    var hasNext = _enumerator.MoveNext();

                    Current
                        = hasNext
                            ? _shaper(_mongoQueryContext, _enumerator.Current)
                            : default;

                    return hasNext;
                }
                finally
                {
                    _concurrencyDetector?.ExitCriticalSection();
                }
            }
            catch (Exception exception)
            {
                if (_exceptionDetector.IsCancellation(exception))
                {
                    _queryLogger.QueryCanceled(_contextType);
                }
                else
                {
                    _queryLogger.QueryIterationFailed(_contextType, exception);
                }

                throw;
            }
        }

        public void Dispose()
        {
            _enumerator?.Dispose();
            _enumerator = null;
        }

        public void Reset()
            => throw new NotSupportedException(CoreStrings.EnumerableResetNotSupported);
    }

    private sealed class AsyncEnumerator : IAsyncEnumerator<T>
    {
        private readonly QueryingEnumerable<T> _queryingEnumerable;
        private readonly MongoQueryContext _mongoQueryContext;
        private readonly SelectExpression _selectExpression;
        private readonly Func<MongoQueryContext, BsonDocument, T> _shaper;
        private readonly Type _contextType;
        //private readonly string _partitionKey;
        private readonly IDiagnosticsLogger<DbLoggerCategory.Query> _queryLogger;
        private readonly bool _standAloneStateManager;
        private readonly CancellationToken _cancellationToken;
        private readonly IConcurrencyDetector _concurrencyDetector;
        private readonly IExceptionDetector _exceptionDetector;

        private IAsyncEnumerator<BsonDocument> _enumerator;

        public AsyncEnumerator(QueryingEnumerable<T> queryingEnumerable, CancellationToken cancellationToken)
        {
            _queryingEnumerable = queryingEnumerable;
            _mongoQueryContext = queryingEnumerable._mongoQueryContext;
            _shaper = queryingEnumerable._shaper;
            _selectExpression = queryingEnumerable._selectExpression;
            _contextType = queryingEnumerable._contextType;
            //_partitionKey = queryingEnumerable._partitionKey;
            _queryLogger = queryingEnumerable._queryLogger;
            _standAloneStateManager = queryingEnumerable._standAloneStateManager;
            _exceptionDetector = _mongoQueryContext.ExceptionDetector;
            _cancellationToken = cancellationToken;

            _concurrencyDetector = queryingEnumerable._threadSafetyChecksEnabled
                ? _mongoQueryContext.ConcurrencyDetector
                : null;
        }

        public T Current { get; private set; }

        public async ValueTask<bool> MoveNextAsync()
        {
            try
            {
                _concurrencyDetector?.EnterCriticalSection();

                try
                {
                    if (_enumerator == null)
                    {
                        var sqlQuery = _queryingEnumerable.GenerateQuery();

                        EntityFrameworkEventSource.Log.QueryExecuting();

                        _enumerator = _mongoQueryContext.MongoClient
                        .ExecuteMqlQueryAsync(
                            _selectExpression.CollectionName,
                            sqlQuery)
                        .GetAsyncEnumerator(_cancellationToken);
                        _mongoQueryContext.InitializeStateManager(_standAloneStateManager);
                    }

                    var hasNext = await _enumerator.MoveNextAsync().ConfigureAwait(false);

                    Current
                        = hasNext
                            ? _shaper(_mongoQueryContext, _enumerator.Current)
                            : default;

                    return hasNext;
                }
                finally
                {
                    _concurrencyDetector?.ExitCriticalSection();
                }
            }
            catch (Exception exception)
            {
                if (_exceptionDetector.IsCancellation(exception, _cancellationToken))
                {
                    _queryLogger.QueryCanceled(_contextType);
                }
                else
                {
                    _queryLogger.QueryIterationFailed(_contextType, exception);
                }

                throw;
            }
        }

        public ValueTask DisposeAsync()
        {
            var enumerator = _enumerator;
            if (enumerator != null)
            {
                _enumerator = null;
                return enumerator.DisposeAsync();
            }

            return default;
        }
    }

    private readonly MongoQueryContext _mongoQueryContext;
    private readonly IMqlExpressionFactory _mqlExpressionFactory;
    private readonly SelectExpression _selectExpression;
    private readonly Func<MongoQueryContext, BsonDocument, T> _shaper;
    private readonly IQueryMqlGeneratorFactory _queryMqlGeneratorFactory;
    private readonly Type _contextType;
    //private readonly string _partitionKey;
    private readonly IDiagnosticsLogger<DbLoggerCategory.Query> _queryLogger;
    private readonly bool _standAloneStateManager;
    private readonly bool _threadSafetyChecksEnabled;

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public QueryingEnumerable(
        MongoQueryContext mongoQueryContext,
        IMqlExpressionFactory mqlExpressionFactory,
        IQueryMqlGeneratorFactory queryMqlGeneratorFactory,
        SelectExpression selectExpression,
        Func<MongoQueryContext, BsonDocument, T> shaper,
        Type contextType,
        //string partitionKeyFromExtension,
        bool standAloneStateManager,
        bool threadSafetyChecksEnabled)
    {
        _mongoQueryContext = mongoQueryContext;
        _mqlExpressionFactory = mqlExpressionFactory;
        _queryMqlGeneratorFactory = queryMqlGeneratorFactory;
        _selectExpression = selectExpression;
        _shaper = shaper;
        _contextType = contextType;
        _queryLogger = mongoQueryContext.QueryLogger;
        _standAloneStateManager = standAloneStateManager;
        _threadSafetyChecksEnabled = threadSafetyChecksEnabled;

        //var partitionKey = selectExpression.GetPartitionKey(cosmosQueryContext.ParameterValues);
        //if (partitionKey != null && partitionKeyFromExtension != null && partitionKeyFromExtension != partitionKey)
        //{
        //    throw new InvalidOperationException(CosmosStrings.PartitionKeyMismatch(partitionKeyFromExtension, partitionKey));
        //}

        //_partitionKey = partitionKey ?? partitionKeyFromExtension;
    }

    /// <summary>
    /// Summary.
    /// </summary>
    /// <param name="cancellationToken">te.</param>
    /// <returns>result.</returns>
    public IAsyncEnumerator<T> GetAsyncEnumerator(CancellationToken cancellationToken = default)
        => new AsyncEnumerator(this, cancellationToken);

    /// <summary>
    /// Summary.
    /// </summary>
    /// <returns>result.</returns>
    public IEnumerator<T> GetEnumerator()
        => new Enumerator(this);

    IEnumerator IEnumerable.GetEnumerator()
        => GetEnumerator();

    private MongoMqlQuery GenerateQuery()
        => _queryMqlGeneratorFactory.Create().GetMqlQuery(
            (SelectExpression)new InExpressionValuesExpandingExpressionVisitor(
                    _mqlExpressionFactory,
                    _mongoQueryContext.ParameterValues)
                .Visit(_selectExpression),
            _mongoQueryContext.ParameterValues);

    /// <summary>
    /// TODO.
    /// </summary>
    /// <returns>TODO.</returns>
    public string ToQueryString()
    {
        var sqlQuery = GenerateQuery();
        if (sqlQuery.Parameters.Count == 0)
        {
            return sqlQuery.Query;
        }

        var builder = new StringBuilder();
        foreach (var parameter in sqlQuery.Parameters)
        {
            builder
                .Append("-- ")
                .Append(parameter.Name)
                .Append("='")
                .Append(parameter.Value)
                .AppendLine("'");
        }

        return builder.Append(sqlQuery.Query).ToString();
    }
}

/// <summary>
///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
///     the same compatibility standards as public APIs. It may be changed or removed without notice in
///     any release. You should only use it directly in your code with extreme caution and knowing that
///     doing so can result in application failures when updating to a new Entity Framework Core release.
/// </summary>
public partial class MongoShapedQueryCompilingExpressionVisitor : ShapedQueryCompilingExpressionVisitor
{
    private readonly IMqlExpressionFactory _mqlExpressionFactory;
    private readonly IQueryMqlGeneratorFactory _queryMqlGeneratorFactory;
    private readonly Type _contextType;
    private readonly bool _threadSafetyChecksEnabled;
    //private readonly string _partitionKeyFromExtension;

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public MongoShapedQueryCompilingExpressionVisitor(
        ShapedQueryCompilingExpressionVisitorDependencies dependencies,
        MongoQueryCompilationContext mongoQueryCompilationContext,
        IMqlExpressionFactory mqlExpressionFactory,
        IQueryMqlGeneratorFactory queryMqlGeneratorFactory
        )
        : base(dependencies, mongoQueryCompilationContext)
    {
        _mqlExpressionFactory = mqlExpressionFactory;
        _queryMqlGeneratorFactory = queryMqlGeneratorFactory;
        _contextType = mongoQueryCompilationContext.ContextType;
        _threadSafetyChecksEnabled = dependencies.CoreSingletonOptions.AreThreadSafetyChecksEnabled;
        //_partitionKeyFromExtension = mongoQueryCompilationContext.PartitionKeyFromExtension;
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    protected override Expression VisitShapedQuery(ShapedQueryExpression shapedQueryExpression)
    {
        var jObjectParameter = Expression.Parameter(typeof(BsonDocument), "BsonDocument");

        var shaperBody = shapedQueryExpression.ShaperExpression;
        shaperBody = new BsonValueInjectingExpressionVisitor().Visit(shaperBody);
        shaperBody = InjectEntityMaterializers(shaperBody);

        switch (shapedQueryExpression.QueryExpression)
        {
            case SelectExpression selectExpression:
                shaperBody = new MongoProjectionBindingRemovingExpressionVisitor(
                        selectExpression, jObjectParameter,
                        QueryCompilationContext.QueryTrackingBehavior == QueryTrackingBehavior.TrackAll)
                    .Visit(shaperBody);  // here

                var shaperLambda = Expression.Lambda(
                    shaperBody,
                    QueryCompilationContext.QueryContextParameter,
                    jObjectParameter);

                return Expression.New(
                    typeof(QueryingEnumerable<>).MakeGenericType(shaperLambda.ReturnType).GetConstructors()[0],
                    Expression.Convert(
                        QueryCompilationContext.QueryContextParameter,
                        typeof(MongoQueryContext)),
                    Expression.Constant(_mqlExpressionFactory),
                    Expression.Constant(_queryMqlGeneratorFactory),
                    Expression.Constant(selectExpression),
                    Expression.Constant(shaperLambda.Compile()),
                    Expression.Constant(_contextType),
                    //Expression.Constant(_partitionKeyFromExtension, typeof(string)),
                    Expression.Constant(
                        QueryCompilationContext.QueryTrackingBehavior == QueryTrackingBehavior.NoTrackingWithIdentityResolution),
                    Expression.Constant(_threadSafetyChecksEnabled));

            //case SelectExpression selectExpression:
            //shaperBody = new MongoProjectionBindingRemovingExpressionVisitor(
            //        selectExpression, jObjectParameter,
            //        QueryCompilationContext.QueryTrackingBehavior == QueryTrackingBehavior.TrackAll)
            //    .Visit(shaperBody);

            //var shaperLambda = Expression.Lambda(
            //    shaperBody,
            //    QueryCompilationContext.QueryContextParameter,
            //    jObjectParameter);

            //return Expression.New(
            //    typeof(QueryingEnumerable<>).MakeGenericType(shaperLambda.ReturnType).GetConstructors()[0],
            //    Expression.Convert(
            //        QueryCompilationContext.QueryContextParameter,
            //        typeof(CosmosQueryContext)),
            //    Expression.Constant(_sqlExpressionFactory),
            //    Expression.Constant(_querySqlGeneratorFactory),
            //    Expression.Constant(selectExpression),
            //    Expression.Constant(shaperLambda.Compile()),
            //    Expression.Constant(_contextType),
            //    Expression.Constant(_partitionKeyFromExtension, typeof(string)),
            //    Expression.Constant(
            //        QueryCompilationContext.QueryTrackingBehavior == QueryTrackingBehavior.NoTrackingWithIdentityResolution),
            //    Expression.Constant(_threadSafetyChecksEnabled));

            //case ReadItemExpression readItemExpression:
            //    shaperBody = new CosmosProjectionBindingRemovingReadItemExpressionVisitor(
            //            readItemExpression, jObjectParameter,
            //            QueryCompilationContext.QueryTrackingBehavior == QueryTrackingBehavior.TrackAll)
            //        .Visit(shaperBody);

            //    var shaperReadItemLambda = Expression.Lambda(
            //        shaperBody,
            //        QueryCompilationContext.QueryContextParameter,
            //        jObjectParameter);

            //    return Expression.New(
            //        typeof(ReadItemQueryingEnumerable<>).MakeGenericType(shaperReadItemLambda.ReturnType).GetConstructors()[0],
            //        Expression.Convert(
            //            QueryCompilationContext.QueryContextParameter,
            //            typeof(CosmosQueryContext)),
            //        Expression.Constant(readItemExpression),
            //        Expression.Constant(shaperReadItemLambda.Compile()),
            //        Expression.Constant(_contextType),
            //        Expression.Constant(
            //            QueryCompilationContext.QueryTrackingBehavior == QueryTrackingBehavior.NoTrackingWithIdentityResolution),
            //        Expression.Constant(_threadSafetyChecksEnabled));

            default:
                throw new NotSupportedException(CoreStrings.UnhandledExpressionNode(shapedQueryExpression.QueryExpression));
        }
    }

}
