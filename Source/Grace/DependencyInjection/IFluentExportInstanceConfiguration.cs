﻿using System;
using Grace.DependencyInjection.Conditions;
using Grace.DependencyInjection.Impl;
using Grace.DependencyInjection.Lifestyle;

namespace Grace.DependencyInjection
{
	/// <summary>
	/// This interface allows you to configure an instance for export
	/// </summary>
	/// <typeparam name="T"></typeparam>
	public interface IFluentExportInstanceConfiguration<T>
	{
        /// <summary>
        /// Adds a condition to the export
        /// </summary>
        /// <param name="condition"></param>
        IFluentExportInstanceConfiguration<T> AndCondition(IExportCondition condition);

		/// <summary>
		/// Export as a specific type (usually an interface)
		/// </summary>
		/// <typeparam name="TExportType"></typeparam>
		/// <returns></returns>
		IFluentExportInstanceConfiguration<T> As<TExportType>();

		/// <summary>
		/// Export as a particular interface
		/// </summary>
		/// <param name="exportType"></param>
		/// <returns></returns>
		IFluentExportInstanceConfiguration<T> As(Type exportType);

        /// <summary>
        /// Export this type as particular type under the specified key
        /// </summary>
        /// <typeparam name="TExportType">export type</typeparam>
        /// <typeparam name="TKey">type of key</typeparam>
        /// <param name="key">key to export under</param>
        /// <returns>configuration object</returns>
        IFluentExportInstanceConfiguration<T> AsKeyed<TExportType, TKey>(TKey key);

        /// <summary>
        /// Export this type as particular type under the specified key
        /// </summary>
        /// <param name="exportType">type to export under</param>
        /// <param name="key">export key</param>
        /// <returns>configuration object</returns>
        IFluentExportInstanceConfiguration<T> AsKeyed(Type exportType, object key);

		/// <summary>
		/// Export the type under the specified name
		/// </summary>
		/// <param name="name">name to export under</param>
		/// <returns></returns>
		IFluentExportInstanceConfiguration<T> AsName(string name);

        /// <summary>
        /// You can provide a cleanup method to be called 
        /// </summary>
        /// <param name="disposalCleanupDelegate"></param>
        /// <returns></returns>
        IFluentExportInstanceConfiguration<T> DisposalCleanupDelegate(BeforeDisposalCleanupDelegate disposalCleanupDelegate);
        
        /// <summary>
        /// Configure the export lifestyle
        /// </summary>
        InstanceLifestyleConfiguration<T> Lifestyle { get; }
        
        /// <summary>
        /// Adds a condition to the export
        /// </summary>
        /// <param name="conditionDelegate"></param>
        IFluentExportInstanceConfiguration<T> Unless(ExportConditionDelegate conditionDelegate);

        /// <summary>
        /// Specify a lifestyle to use with the export
        /// </summary>
        /// <param name="lifestyle"></param>
        /// <returns></returns>
        IFluentExportInstanceConfiguration<T> UsingLifestyle(ILifestyle lifestyle);

		/// <summary>
		/// Adds a condition to the export
		/// </summary>
		/// <param name="conditionDelegate"></param>
		IFluentExportInstanceConfiguration<T> When(ExportConditionDelegate conditionDelegate);
        
		/// <summary>
		/// Applies a new WhenInjectedInto condition on the export, using the export only when injecting into the specified class
		/// </summary>
		/// <typeparam name="TInjected"></typeparam>
		/// <returns></returns>
		IFluentExportInstanceConfiguration<T> WhenInjectedInto<TInjected>();

		/// <summary>
		/// Applies a WhenClassHas condition, using the export only if injecting into a class that is attributed with TAttr
		/// </summary>
		/// <typeparam name="TAttr"></typeparam>
		/// <returns></returns>
		IFluentExportInstanceConfiguration<T> WhenClassHas<TAttr>();

		/// <summary>
		/// Applies a WhenMemberHas condition, using the export only if the Property or method or constructor is attribute with TAttr
		/// </summary>
		/// <typeparam name="TAttr"></typeparam>
		/// <returns></returns>
		IFluentExportInstanceConfiguration<T> WhenMemberHas<TAttr>();

		/// <summary>
		/// Applies a WhenTargetHas condition, using the export only if the Property or Parameter is attributed with TAttr
		/// </summary>
		/// <typeparam name="TAttr"></typeparam>
		/// <returns></returns>
		IFluentExportInstanceConfiguration<T> WhenTargetHas<TAttr>();

		/// <summary>
		/// Adds metadata to an export
		/// </summary>
		/// <param name="metadataName"></param>
		/// <param name="metadataValue"></param>
		/// <returns></returns>
		IFluentExportInstanceConfiguration<T> WithMetadata(string metadataName, object metadataValue);

        /// <summary>
        /// Export the type with the specified priority
        /// </summary>
        /// <param name="priority"></param>
        /// <returns></returns>
        IFluentExportInstanceConfiguration<T> WithPriority(int priority);
	}
}