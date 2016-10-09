﻿using Grace.DependencyInjection.Lifestyle;
using System;
using System.Reflection;

namespace Grace.DependencyInjection
{
    public interface IFluentExportStrategyConfiguration
    {
        IFluentExportStrategyConfiguration As(Type type);

        IFluentExportStrategyConfiguration UsingLifestyle(ICompiledLifestyle lifestyle);

        ILifestylePicker<IFluentExportStrategyConfiguration> Lifestyle { get; }

        IFluentExportStrategyConfiguration WithMetadata(object key, object value);

        IFluentExportStrategyConfiguration ImportMembers(Func<MemberInfo, bool> selector = null);

        //IWhenConditionConfiguration<IFluentExportStrategyConfiguration> When { get; }
    }

    public interface IFluentExportStrategyConfiguration<T>
    {
        /// <summary>
        /// Export as a specific type
        /// </summary>
        /// <param name="type">type to export as</param>
        /// <returns></returns>
        IFluentExportStrategyConfiguration<T> As(Type type);

        /// <summary>
        /// Export as a particular type
        /// </summary>
        /// <typeparam name="TInterface">type to export as</typeparam>
        /// <returns>configuration object</returns>
        IFluentExportStrategyConfiguration<T> As<TInterface>();

        /// <summary>
        /// Export as a keyed type
        /// </summary>
        /// <typeparam name="TInterface">export type</typeparam>
        /// <param name="key">key to export under</param>
        /// <returns>configuration object</returns>
        IFluentExportStrategyConfiguration<T> AsKeyed<TInterface>(object key);

        /// <summary>
        /// Export using a specific lifestyle
        /// </summary>
        /// <param name="lifestyle">lifestlye to use</param>
        /// <returns>configuration object</returns>
        IFluentExportStrategyConfiguration<T> UsingLifestyle(ICompiledLifestyle lifestyle);

        /// <summary>
        /// Assign a lifestyle to this export
        /// </summary>
        ILifestylePicker<IFluentExportStrategyConfiguration<T>> Lifestyle { get; }

        /// <summary>
        /// Apply an action to the export just after construction
        /// </summary>
        /// <param name="applyAction">action to apply to export upon construction</param>
        /// <returns>configuration object</returns>
        IFluentExportStrategyConfiguration<T> Apply(Action<T> applyAction);
        
            /// <summary>
        /// Mark specific members to be injected
        /// </summary>
        /// <param name="selector">select specific members, if null all public members will be injected</param>
        /// <returns>configuration object</returns>
        IFluentExportStrategyConfiguration<T> ImportMembers(Func<MemberInfo, bool> selector = null);

        /// <summary>
        /// Add a condition to when this export can be used
        /// </summary>
        //IWhenConditionConfiguration<IFluentExportStrategyConfiguration<T>> When { get; }

        /// <summary>
        /// Add a specific value for a particuar parameter in the constructor
        /// </summary>
        /// <typeparam name="TParam">type of parameter</typeparam>
        /// <param name="paramValue">Func(T) value for the parameter</param>
        /// <returns>configuration object</returns>
        IFluentWithCtorConfiguration<T, TParam> WithCtorParam<TParam>(Func<TParam> paramValue = null);
        
        /// <summary>
        /// Add a specific value for a particuar parameter in the constructor
        /// </summary>
        /// <typeparam name="TParam">type of parameter</typeparam>
        /// <param name="paramValue">Func(IInjectionScope, IInjectionContext, T) value for the parameter</param>
        /// <returns>configuration object</returns>
        IFluentWithCtorConfiguration<T, TParam> WithCtorParam<TParam>(Func<IExportLocatorScope, StaticInjectionContext, IInjectionContext, TParam> paramValue);
        
        /// <summary>
        /// Adds metadata to an export
        /// </summary>
        /// <param name="key">metadata key</param>
        /// <param name="value">metadata value</param>
        /// <returns>configuration object</returns>
        IFluentExportStrategyConfiguration<T> WithMetadata(object key, object value);

    }
}