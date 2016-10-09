﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Grace.Data.Immutable;
using Grace.DependencyInjection.Exceptions;

namespace Grace.DependencyInjection.Impl.Expressions
{
    public interface IActivationExpressionBuilder
    {
        void SetCompiler(IActivationStrategyCompiler compiler);

        IActivationExpressionResult GetActivationExpression(IInjectionScope scope, Type type);

        IActivationExpressionResult GetActivationExpression(IInjectionScope scope, IActivationExpressionRequest request);

        IActivationExpressionResult GetValueFromRequest(IInjectionScope scope,
                                                        IActivationExpressionRequest request,
                                                        Type activationType,
                                                        object key);

        IActivationExpressionResult DecorateExportStrategy(IInjectionScope scope, IActivationExpressionRequest request, ICompiledExportStrategy strategy);
    }

    public class ActivationExpressionBuilder : IActivationExpressionBuilder
    {
        private readonly IEnumerableExpressionCreator _enumerableExpressionCreator;
        private readonly IArrayExpressionCreator _arrayExpressionCreator;
        private readonly IWrapperExpressionCreator _wrapperExpressionCreator;
        private IActivationStrategyCompiler _compiler;

        public ActivationExpressionBuilder(IArrayExpressionCreator arrayExpressionCreator,
                                           IEnumerableExpressionCreator enumerableExpressionCreator,
                                           IWrapperExpressionCreator wrapperExpressionCreator)
        {
            _enumerableExpressionCreator = enumerableExpressionCreator;
            _arrayExpressionCreator = arrayExpressionCreator;
            _wrapperExpressionCreator = wrapperExpressionCreator;
        }

        public void SetCompiler(IActivationStrategyCompiler compiler)
        {
            _compiler = compiler;
        }

        public IActivationExpressionResult GetActivationExpression(IInjectionScope scope, Type type)
        {
            var request = _compiler.CreateNewRequest(type, 1);

            return GetActivationExpression(scope, request);
        }

        public IActivationExpressionResult GetActivationExpression(IInjectionScope scope, IActivationExpressionRequest request)
        {
            var activationExpressionForStrategy = GetValueFromRequest(scope, request, request.ActivationType, null);

            if (activationExpressionForStrategy != null)
            {
                return activationExpressionForStrategy;
            }

            activationExpressionForStrategy = GetActivationExpressionFromStrategies(scope, request);

            if (activationExpressionForStrategy != null)
            {
                return activationExpressionForStrategy;
            }

            if (request.ActivationType.IsArray)
            {
                return _arrayExpressionCreator.GetArrayExpression(scope, request);
            }

            if (request.ActivationType.IsConstructedGenericType &&
                request.ActivationType.GetGenericTypeDefinition() == typeof(IEnumerable<>))
            {
                return _enumerableExpressionCreator.GetEnumerableExpression(scope, request, _arrayExpressionCreator);
            }

            var wrapperResult = _wrapperExpressionCreator.GetActivationStrategy(scope, request);

            if (wrapperResult != null)
            {
                return wrapperResult;
            }

            lock (scope.GetLockObject(RootInjectionScope.ActivationStrategyAddLockName))
            {
                activationExpressionForStrategy = GetActivationExpressionFromStrategies(scope, request);

                if (activationExpressionForStrategy != null)
                {
                    return activationExpressionForStrategy;
                }

                wrapperResult = _wrapperExpressionCreator.GetActivationStrategy(scope, request);

                if (wrapperResult != null)
                {
                    return wrapperResult;
                }

                request.Services.Compiler.ProcessMissingStrategyProviders(scope, request);

                activationExpressionForStrategy = GetActivationExpressionFromStrategies(scope, request);

                if (activationExpressionForStrategy != null)
                {
                    return activationExpressionForStrategy;
                }

                wrapperResult = _wrapperExpressionCreator.GetActivationStrategy(scope, request);

                if (wrapperResult != null)
                {
                    return wrapperResult;
                }
            }

            return GetValueFromInjectionContext(scope, request);
        }

        public IActivationExpressionResult GetValueFromRequest(IInjectionScope scope,
                                                               IActivationExpressionRequest request,
                                                               Type activationType,
                                                               object key)
        {
            var knownValue =
                request.KnownValueExpressions.FirstOrDefault(
                    v => activationType.GetTypeInfo().IsAssignableFrom(v.ActivationType.GetTypeInfo()));

            if (knownValue != null)
            {
                return knownValue.ValueExpression(request);
            }

            if (request.WrapperPathNode != null)
            {
                if (activationType.GetTypeInfo().IsAssignableFrom(request.WrapperPathNode.ActivationType.GetTypeInfo()))
                {
                    var wrapper = request.PopWrapperPathNode();

                    return ProcessPathNode(scope, request, activationType, wrapper);
                }
            }
            else if (request.DecoratorPathNode != null)
            {
                if (activationType.GetTypeInfo().IsAssignableFrom(request.DecoratorPathNode.Strategy.ActivationType.GetTypeInfo()))
                {
                    var decorator = request.PopDecoratorPathNode();

                    return ProcessPathNode(scope, request, activationType, decorator);
                }
            }

            if (request.ActivationType == typeof(IExportLocatorScope) ||
                request.ActivationType == typeof(ILocatorService))
            {
                return request.Services.Compiler.CreateNewResult(request, request.Constants.ScopeParameter);
            }

            if (request.ActivationType == typeof(IInjectionContext))
            {
                request.RequireInjectionContext();

                return request.Services.Compiler.CreateNewResult(request, request.Constants.InjectionContextParameter);
            }

            if (request.ActivationType == typeof(StaticInjectionContext))
            {
                var staticContext = request.GetStaticInjectionContext();

                return request.Services.Compiler.CreateNewResult(request, Expression.Constant(staticContext));
            }

            return null;
        }

        private IActivationExpressionResult ProcessPathNode(IInjectionScope scope, IActivationExpressionRequest request, Type activationType, IActivationPathNode decorator)
        {
            return decorator.GetActivationExpression(scope, request);
        }

        private IActivationExpressionResult GetActivationExpressionFromStrategies(IInjectionScope scope, IActivationExpressionRequest request)
        {
            var expressionResult = GetExpressionFromStrategyCollection(scope, request);

            if (expressionResult != null)
            {
                return expressionResult;
            }

            return GetExpressionFromGenericStrategies(scope, request);
        }

        private IActivationExpressionResult GetExpressionFromGenericStrategies(IInjectionScope scope, IActivationExpressionRequest request)
        {
            if (request.ActivationType.IsConstructedGenericType)
            {
                var genericType = request.ActivationType.GetGenericTypeDefinition();

                var collection = scope.StrategyCollectionContainer.GetActivationStrategyCollection(genericType);

                if (collection != null)
                {
                    if (request.LocateKey != null)
                    {
                        var keyedStrategy = collection.GetKeyedStrategy(request.LocateKey);

                        if (keyedStrategy != null)
                        {
                            return ActivationExpressionForStrategy(scope, request, keyedStrategy);
                        }
                    }
                    else
                    {
                        var primary = collection.GetPrimary();

                        if (primary != null && request.Filter == null)
                        {
                            return ActivationExpressionForStrategy(scope, request, primary);
                        }
                        else
                        {
                            throw new NotImplementedException();
                        }
                    }
                }
            }

            return null;
        }

        private IActivationExpressionResult GetExpressionFromStrategyCollection(IInjectionScope scope, IActivationExpressionRequest request)
        {
            var collection = scope.StrategyCollectionContainer.GetActivationStrategyCollection(request.ActivationType);

            if (collection != null)
            {
                if (request.LocateKey != null)
                {
                    var keyedStrategy = collection.GetKeyedStrategy(request.LocateKey);

                    if (keyedStrategy != null)
                    {
                        return ActivationExpressionForStrategy(scope, request, keyedStrategy);
                    }
                }
                else
                {
                    var primary = collection.GetPrimary();

                    if (primary != null && request.Filter == null)
                    {
                        return ActivationExpressionForStrategy(scope, request, primary);
                    }
                    else
                    {
                        throw new NotImplementedException();
                    }
                }
            }
            return null;
        }

        private IActivationExpressionResult ActivationExpressionForStrategy(IInjectionScope scope, IActivationExpressionRequest request, ICompiledExportStrategy strategy)
        {
            return strategy.GetActivationExpression(scope, request);
        }

        public IActivationExpressionResult GetValueFromInjectionContext(IInjectionScope scope, IActivationExpressionRequest request)
        {
            var valueMethod = typeof(ActivationExpressionBuilder).GetRuntimeMethod("GetValueFromInjectionContext", new Type[]
            {
                typeof(IExportLocatorScope),
                typeof(StaticInjectionContext),
                typeof(object),
                typeof(IInjectionContext),
                typeof(object),
                typeof(bool),
                typeof(bool)
            });

            var closedMethod = valueMethod.MakeGenericMethod(request.ActivationType);

            var expresion = Expression.Call(closedMethod,
                                            request.Constants.ScopeParameter,
                                            Expression.Constant(request.GetStaticInjectionContext()),
                                            Expression.Constant(request.LocateKey, typeof(object)),
                                            request.Constants.InjectionContextParameter,
                                            Expression.Constant(request.DefaultValue?.DefaultValue, typeof(object)),
                                            Expression.Constant(request.DefaultValue != null),
                                            Expression.Constant(request.IsRequired));

            return request.Services.Compiler.CreateNewResult(request, expresion);
        }

        public static T GetValueFromInjectionContext<T>(
            IExportLocatorScope locator,
            StaticInjectionContext staticContext,
            object key,
            IInjectionContext dataProvider,
            object defaultValue,
            bool useDefault,
            bool isRequired)
        {
            object value = null;

            if (dataProvider != null && key != null)
            {
                value = dataProvider.GetExtraData(key);
            }

            if (value == null && useDefault)
            {
                var defaultFunc = defaultValue as Func<IExportLocatorScope, StaticInjectionContext, IInjectionContext, T>;

                value = defaultFunc != null ? defaultFunc(locator, staticContext, dataProvider) : defaultValue;
            }

            if (value != null)
            {
                if (!value.GetType().GetTypeInfo().IsAssignableFrom(typeof(T).GetTypeInfo()))
                {
                    try
                    {
                        value = Convert.ChangeType(value, typeof(T));
                    }
                    catch (Exception exp)
                    {
                        // to do fix up exception
                        throw new LocateException(staticContext);
                    }
                }
            }
            else if (isRequired && !useDefault)
            {
                throw new LocateException(staticContext);
            }

            return (T)value;
        }

        public IActivationExpressionResult DecorateExportStrategy(IInjectionScope scope, IActivationExpressionRequest request,
            ICompiledExportStrategy strategy)
        {
            var decorators = FindDecoratorsForStrategy(scope, request);

            if (decorators.Count == 0)
            {
                return null;
            }

            return CreateDecoratedActiationStrategy(scope, request, strategy, decorators);
        }

        protected virtual List<ICompiledDecoratorStrategy> FindDecoratorsForStrategy(IInjectionScope scope, IActivationExpressionRequest request)
        {
            var decorators = new List<ICompiledDecoratorStrategy>();

            var collection =
                scope.DecoratorCollectionContainer.GetActivationStrategyCollection(request.ActivationType);

            if (collection != null)
            {
                decorators.AddRange(collection.GetStrategies());
            }

            if (request.ActivationType.IsConstructedGenericType)
            {
                var generic = request.ActivationType.GetGenericTypeDefinition();

                collection = scope.DecoratorCollectionContainer.GetActivationStrategyCollection(generic);

                if (collection != null)
                {
                    decorators.AddRange(collection.GetStrategies());
                }
            }

            return decorators;
        }

        protected virtual IActivationExpressionResult CreateDecoratedActiationStrategy(IInjectionScope scope, IActivationExpressionRequest request, ICompiledExportStrategy strategy, List<ICompiledDecoratorStrategy> decorators)
        {
            decorators.Sort((x, y) => Comparer<int>.Default.Compare(x.Priority, y.Priority));

            ImmutableLinkedList<IActivationPathNode> pathNodes = ImmutableLinkedList<IActivationPathNode>.Empty;

            if (decorators.All(d => d.ApplyAfterLifestyle))
            {
                pathNodes = pathNodes.Add(new DecoratorActivationPathNode(strategy, request.ActivationType, strategy.Lifestyle));

                foreach (var decorator in decorators)
                {
                    pathNodes = pathNodes.Add(new DecoratorActivationPathNode(decorator, request.ActivationType, null));
                }
            }
            else
            {
                pathNodes = pathNodes.Add(new DecoratorActivationPathNode(strategy, request.ActivationType, null));

                DecoratorActivationPathNode currentNode = null;

                foreach (var decorator in decorators.Where(d => !d.ApplyAfterLifestyle))
                {
                    currentNode = new DecoratorActivationPathNode(decorator, request.ActivationType, null);

                    pathNodes = pathNodes.Add(currentNode);
                }

                currentNode.Lifestyle = strategy.Lifestyle;

                foreach (var decorator in decorators.Where(d => d.ApplyAfterLifestyle))
                {
                    pathNodes = pathNodes.Add(new DecoratorActivationPathNode(decorator, request.ActivationType, null));
                }
            }

            request.SetDecoratorPath(pathNodes);

            var pathNode = request.PopDecoratorPathNode();

            return pathNode.GetActivationExpression(scope, request);
        }
    }
}
