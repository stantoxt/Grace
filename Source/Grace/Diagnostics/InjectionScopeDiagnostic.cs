﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reflection;
using Grace.DependencyInjection;

namespace Grace.Diagnostics
{
	/// <summary>
	/// Provides diagnostic information about an IInjectionScope
	/// Used by visual studio for debugging
	/// </summary>
	public class InjectionScopeDiagnostic
	{
		private bool initialize;
		private IEnumerable<PossibleMissingDependency> possibleMissingDependencies;
		private readonly IInjectionScope injectionScope;

		/// <summary>
		/// Default constructor takes scope as only parameter
		/// </summary>
		/// <param name="injectionScope">injection scope to diagnose</param>
		public InjectionScopeDiagnostic(IInjectionScope injectionScope)
		{
			this.injectionScope = injectionScope;
		}
        
		/// <summary>
		/// Parent scope for injection scope
		/// </summary>
		public IInjectionScope ParentScope
		{
			get { return injectionScope.ParentScope; }
		}

		/// <summary>
		/// Name of scope
		/// </summary>
		public string ScopeName
		{
			get { return injectionScope.ScopeName; }
		}

		/// <summary>
		/// Unique Id for the scope
		/// </summary>
		public Guid ScopeId
		{
			get { return injectionScope.ScopeId; }
		}

		/// <summary>
		/// list of all exports
		/// </summary>
		public IEnumerable<IExportStrategy> Exports
		{
			get { return injectionScope.GetAllStrategies(); }
		}

		/// <summary>
		/// Exported names
		/// </summary>
		public IEnumerable<ExportListDebuggerView> ExportsByName
		{
			get
			{
				Dictionary<string, ExportListDebuggerView> returnValue = new Dictionary<string, ExportListDebuggerView>();

				foreach (IExportStrategy exportStrategy in injectionScope.GetAllStrategies())
				{
					foreach (string exportName in exportStrategy.ExportNames)
					{
						ExportListDebuggerView view;

						if (!returnValue.TryGetValue(exportName, out view))
						{
							view = new ExportListDebuggerView(exportName);

							returnValue[exportName] = view;
						}

						view.Add(exportStrategy);
					}
				}

				List<KeyValuePair<string, ExportListDebuggerView>> sortList =
					new List<KeyValuePair<string, ExportListDebuggerView>>(returnValue);

				sortList.Sort((x, y) => string.CompareOrdinal(x.Key, y.Key));

				return new List<ExportListDebuggerView>(sortList.Select(x => x.Value));
			}
		}

		/// <summary>
		/// Exported types
		/// </summary>
		public IEnumerable<ExportListDebuggerView> ExportsByType
		{
			get
			{
				Dictionary<Type, ExportListDebuggerView> returnValue = new Dictionary<Type, ExportListDebuggerView>();

				foreach (IExportStrategy exportStrategy in injectionScope.GetAllStrategies())
				{
					foreach (Type exportType in exportStrategy.ExportTypes)
					{
						ExportListDebuggerView view;

						if (!returnValue.TryGetValue(exportType, out view))
						{
							view = new ExportListDebuggerView(exportType.FullName);

							returnValue[exportType] = view;
						}

						view.Add(exportStrategy);
					}
				}

				List<KeyValuePair<Type, ExportListDebuggerView>> sortList =
					new List<KeyValuePair<Type, ExportListDebuggerView>>(returnValue);

				sortList.Sort((x, y) => String.CompareOrdinal(x.Key.FullName, y.Key.FullName));

				return new List<ExportListDebuggerView>(sortList.Select(x => x.Value));
			}
		}

		/// <summary>
		/// List of possible missing dependencies
		/// Note: This is just a possible missing dependency
		/// Using static analysis this is a best attempt at resolving.
		/// Because of conditions and other factors it's possible to have no missing dependencies listed
		/// and still fail to resolve and vice versa
		/// </summary>
		public IEnumerable<PossibleMissingDependency> PossibleMissingDependencies
		{
			get
			{
				Initialize();

				return possibleMissingDependencies;
			}
		}

		/// <summary>
		/// Calculates a list of possible missing dependencies
		/// </summary>
		/// <param name="locator"></param>
		/// <returns></returns>
		public static IEnumerable<PossibleMissingDependency> CalculatePossibleMissingDependencies(IExportLocator locator)
		{
			List<PossibleMissingDependency> possibleMissingDependencies = new List<PossibleMissingDependency>();

			foreach (IExportStrategy exportStrategy in locator.GetAllStrategies())
			{
				foreach (ExportStrategyDependency exportStrategyDependency in exportStrategy.DependsOn)
				{
					if (exportStrategyDependency.HasValueProvider)
					{
						continue;
					}

					if (exportStrategyDependency.ImportName != null)
					{
					}
					else if (exportStrategyDependency.ImportType != null &&
								LocateExportByType(locator, exportStrategyDependency))
					{
						continue;
					}

					possibleMissingDependencies.Add(new PossibleMissingDependency
															  {
																  Dependency = exportStrategyDependency,
																  Strategy = exportStrategy
															  });
				}
			}

			possibleMissingDependencies.Sort((x, y) => string.CompareOrdinal(x.Dependency.DebuggerDisplayString, y.Dependency.DebuggerDisplayString));

			return possibleMissingDependencies;
		}

		private static bool LocateExportByType(IExportLocator locator, ExportStrategyDependency exportStrategyDependency)
		{
			if (locator.GetStrategy(exportStrategyDependency.ImportType) != null)
			{
				return true;
			}

			if (TestForSpecialType(locator, exportStrategyDependency.ImportType))
			{
				return true;
			}

			if (exportStrategyDependency.ImportType.GetTypeInfo().IsClass &&
				 !exportStrategyDependency.ImportType.GetTypeInfo().IsAbstract &&
				 !exportStrategyDependency.ImportType.GetTypeInfo().IsInterface)
			{
				return true;
			}

			IInjectionScope injectionScope = locator as IInjectionScope;

			if (injectionScope != null)
			{
				InjectionContext context = new InjectionContext(injectionScope);

				foreach (ISecondaryExportLocator secondaryExportLocator in injectionScope.SecondaryExportLocators)
				{
					if (secondaryExportLocator.CanLocate(context, null, exportStrategyDependency.ImportType, null, null))
					{
						return true;
					}
				}

				if (injectionScope.ParentScope != null)
				{
					return LocateExportByType(injectionScope.ParentScope, exportStrategyDependency);
				}
			}

			return false;
		}

		private static bool TestForSpecialType(IExportLocator locator, Type importType)
		{
			if (importType == typeof(IDisposalScope) || importType == typeof(IExportLocator) ||
				 importType == typeof(IInjectionScope) || importType == typeof(IDependencyInjectionContainer))
			{
				return true;
			}

			if (importType.IsConstructedGenericType)
			{
				Type openType = importType.GetGenericTypeDefinition();

				if (TestForListSpecialType(openType))
				{
					return true;
				}

				if (importType.FullName.StartsWith("System.Func`"))
				{
					return true;
				}

				if (TestForLazyType(openType))
				{
					return true;
				}

				if (TestForOwnedType(openType))
				{
					return true;
				}
			}
			if (importType.IsArray)
			{
				return true;
			}

			return false;
		}

		private static bool TestForOwnedType(Type openType)
		{
			return openType == typeof(Owned<>);
		}

		private static bool TestForLazyType(Type openType)
		{
			return openType == typeof(Lazy<>);
		}

		private static bool TestForListSpecialType(Type openType)
		{
			if (openType == typeof(IEnumerable<>) || openType == typeof(ICollection<>) ||
				 openType == typeof(IList<>) || openType == typeof(List<>))
			{
				return true;
			}

			if (openType == typeof(IReadOnlyCollection<>) || openType == typeof(IReadOnlyList<>) ||
				 openType == typeof(ReadOnlyCollection<>) || openType == typeof(ReadOnlyObservableCollection<>))
			{
				return true;
			}

			foreach (Type implementedInterface in openType.GetTypeInfo().ImplementedInterfaces)
			{
				if (implementedInterface.IsConstructedGenericType &&
					 implementedInterface.GetGenericTypeDefinition() == typeof(IList<>))
				{
					return true;
				}
			}

			return false;
		}

		private void Initialize()
		{
			if (initialize)
			{
				return;
			}

			initialize = true;

			possibleMissingDependencies = CalculatePossibleMissingDependencies(injectionScope);
		}
	}
}