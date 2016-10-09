﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Threading.Tasks;
using Grace.DependencyInjection.Lifestyle;

namespace Grace.DependencyInjection.Impl.EnumerableStrategies
{
    public class ReadOnlyCollectionStrategy : BaseGenericEnumerableStrategy
    {
        public ReadOnlyCollectionStrategy(IInjectionScope injectionScope) : base(typeof(ReadOnlyCollection<>), injectionScope)
        {
            AddExportAs(typeof(ReadOnlyCollection<>));
            AddExportAs(typeof(IReadOnlyList<>));
            AddExportAs(typeof(IReadOnlyCollection<>));
        }

        public override IActivationExpressionResult GetDecoratorActivationExpression(IInjectionScope scope, IActivationExpressionRequest request,
            ICompiledLifestyle lifestyle)
        {
            throw new NotSupportedException("Decorators on collection is not supported at this time");
        }

        public override IActivationExpressionResult GetActivationExpression(IInjectionScope scope, IActivationExpressionRequest request)
        {
            var elementType = request.ActivationType.GenericTypeArguments[0];

            var newRequest = request.NewRequest(typeof(IList<>).MakeGenericType(elementType), request.InjectedType, RequestType.Other, null, true);

            var listResult = request.Services.ExpressionBuilder.GetActivationExpression(scope, newRequest);

            var closedType = typeof(ReadOnlyCollection<>).MakeGenericType(elementType);

            var constructor = closedType.GetTypeInfo().DeclaredConstructors.First(c =>
            {
                var parameters = c.GetParameters();

                if (parameters.Length == 1)
                {
                    return parameters[0].ParameterType.IsConstructedGenericType &&
                           parameters[0].ParameterType.GetGenericTypeDefinition() == typeof(IList<>);
                }

                return false;
            });

            var expression = Expression.New(constructor, listResult.Expression);

            var result = request.Services.Compiler.CreateNewResult(request, expression);

            result.AddExpressionResult(listResult);

            return result;
        }
    }
}