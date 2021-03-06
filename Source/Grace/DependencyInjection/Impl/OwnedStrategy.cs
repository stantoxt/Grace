﻿using System;
using System.Collections.Generic;
using Grace.Data.Immutable;
using Grace.DependencyInjection.Lifestyle;

namespace Grace.DependencyInjection.Impl
{
	/// <summary>
	/// An export strategy for creating Owned(T) objects
	/// </summary>
	/// <typeparam name="T"></typeparam>
	public class OwnedStrategy<T> : IExportStrategy where T : class
	{
		/// <summary>
		/// Activate the export
		/// </summary>
		/// <param name="exportInjectionScope"></param>
		/// <param name="context"></param>
		/// <param name="consider"></param>
		/// <param name="locateKey"></param>
		/// <returns></returns>
		public object Activate(IInjectionScope exportInjectionScope, IInjectionContext context, ExportStrategyFilter consider, object locateKey)
		{
			Owned<T> owned = new Owned<T>();

			IDisposalScope tempScope = context.DisposalScope;

			context.DisposalScope = owned;

			T outValue = exportInjectionScope.Locate<T>(context, consider, locateKey);

			owned.SetValue(outValue);

			context.DisposalScope = tempScope;

			return owned;
		}

		/// <summary>
		/// Dispose of the strategy
		/// </summary>
		public void Dispose()
		{
		}

		/// <summary>
		/// Initialize the export, caled by the container
		/// </summary>
		public void Initialize()
		{
		}

		/// <summary>
		/// This is type that will be activated, can be used for filtering
		/// </summary>
		public Type ActivationType
		{
			get { return typeof(Owned<T>); }
		}

		/// <summary>
		/// Usually the type.FullName, used for blacklisting purposes
		/// </summary>
		public string ActivationName
		{
			get { return typeof(Owned<T>).FullName; }
		}

		/// <summary>
		/// When considering an export should it be filtered out.
		/// True by default, usually it's only false for special export types like Array ad List
		/// </summary>
		public bool AllowingFiltering
		{
			get { return false; }
		}

		/// <summary>
		/// Attributes associated with the export strategy. 
		/// Note: do not return null. Return an empty enumerable if there are none
		/// </summary>
		public IEnumerable<Attribute> Attributes
		{
            get { return ImmutableArray<Attribute>.Empty; }
		}

		/// <summary>
		/// The scope that owns this export
		/// </summary>
		public IInjectionScope OwningScope { get; set; }

		/// <summary>
		/// Export Key
		/// </summary>
		public object Key
		{
			get { return null; }
		}

		/// <summary>
		/// Names this strategy should be known as.
		/// </summary>
		public IEnumerable<string> ExportNames
		{
			get { yield return typeof(Owned<T>).FullName; }
		}

		/// <summary>
		/// Export types this strategy should
		/// </summary>
		public IEnumerable<Type> ExportTypes
		{
			get { yield return typeof(Owned<T>); }
		}

        /// <summary>
        /// List of keyed interface to export under
        /// </summary>
        public IEnumerable<Tuple<Type, object>> KeyedExportTypes
        {
            get { return ImmutableArray<Tuple<Type, object>>.Empty; }
        }
        
		/// <summary>
		/// What export priority is this being exported as
		/// </summary>
		public int Priority
		{
			get { return -1; }
		}

		/// <summary>
		/// ILifestyle associated with export
		/// </summary>
		public ILifestyle Lifestyle
		{
			get { return null; }
		}

		/// <summary>
		/// Does this export have any conditions, this is important when setting up the strategy
		/// </summary>
		public bool HasConditions
		{
			get { return false; }
		}

		/// <summary>
		/// Are the object produced by this export externally owned
		/// </summary>
		public bool ExternallyOwned
		{
			get { return true; }
		}

		/// <summary>
		/// Does this export meet the conditions to be used
		/// </summary>
		/// <param name="injectionContext"></param>
		/// <returns></returns>
		public bool MeetsCondition(IInjectionContext injectionContext)
		{
			return true;
		}

		/// <summary>
		/// No secondary strategies
		/// </summary>
		/// <returns></returns>
		public IEnumerable<IExportStrategy> SecondaryStrategies()
		{
			yield break;
		}

		/// <summary>
		/// No enrichment supported
		/// </summary>
		/// <param name="enrichWithDelegate">enrichment</param>
		public void EnrichWithDelegate(EnrichWithDelegate enrichWithDelegate)
		{
		}

		/// <summary>
		/// Doesn't depend on anything
		/// </summary>
		public IEnumerable<ExportStrategyDependency> DependsOn
		{
			get { yield break; }
		}

		/// <summary>
		/// Metadata associated with this strategy
		/// </summary>
		public IExportMetadata Metadata
		{
            get { return new ExportMetadata(null); }
		}
	}
}