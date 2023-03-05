// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Mongo.Query.Internal;
using System.Reflection;

namespace Microsoft.EntityFrameworkCore.Mongo.Query.Internal;


/// <summary>
///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
///     the same compatibility standards as public APIs. It may be changed or removed without notice in
///     any release. You should only use it directly in your code with extreme caution and knowing that
///     doing so can result in application failures when updating to a new Entity Framework Core release.
/// </summary>
public interface IMethodCallTranslatorPlugin
{
    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    IEnumerable<IMethodCallTranslator> Translators { get; }
}

/// <summary>
///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
///     the same compatibility standards as public APIs. It may be changed or removed without notice in
///     any release. You should only use it directly in your code with extreme caution and knowing that
///     doing so can result in application failures when updating to a new Entity Framework Core release.
/// </summary>
public interface IMethodCallTranslator
{
    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    MqlExpression? Translate(
        MqlExpression? instance,
        MethodInfo method,
        IReadOnlyList<MqlExpression> arguments,
        IDiagnosticsLogger<DbLoggerCategory.Query> logger);
}

/// <summary>
///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
///     the same compatibility standards as public APIs. It may be changed or removed without notice in
///     any release. You should only use it directly in your code with extreme caution and knowing that
///     doing so can result in application failures when updating to a new Entity Framework Core release.
/// </summary>
public class MongoMethodCallTranslatorProvider : IMethodCallTranslatorProvider
{
    private readonly List<IMethodCallTranslator> _plugins = new();
    private readonly List<IMethodCallTranslator> _translators = new();

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public MongoMethodCallTranslatorProvider(
        IMqlExpressionFactory mqlExpressionFactory,
        IEnumerable<IMethodCallTranslatorPlugin> plugins)
    {
        _plugins.AddRange(plugins.SelectMany(p => p.Translators));

        _translators.AddRange(
            new IMethodCallTranslator[]
            {
                new MongoEqualsTranslator(mqlExpressionFactory),
                new MongoStringMethodTranslator(mqlExpressionFactory),
                //new MongoContainsTranslator(mqlExpressionFactory),
                //new MongoRandomTranslator(mqlExpressionFactory),
                //new MongoMathTranslator(mqlExpressionFactory),
                //new MongoRegexTranslator(mqlExpressionFactory)
                //new LikeTranslator(sqlExpressionFactory),
                //new EnumHasFlagTranslator(sqlExpressionFactory),
                //new GetValueOrDefaultTranslator(sqlExpressionFactory),
                //new ComparisonTranslator(sqlExpressionFactory),
            });
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual MqlExpression? Translate(
        IModel model,
        MqlExpression? instance,
        MethodInfo method,
        IReadOnlyList<MqlExpression> arguments,
        IDiagnosticsLogger<DbLoggerCategory.Query> logger)
        => _plugins.Concat(_translators)
            .Select(t => t.Translate(instance, method, arguments, logger))
            .FirstOrDefault(t => t != null);

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    protected virtual void AddTranslators(IEnumerable<IMethodCallTranslator> translators)
        => _translators.InsertRange(0, translators);
}
