﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace Grace.DependencyInjection
{
    public static class IFluentExportStrategyConfigurationExtentions
    {
        public static IFluentExportStrategyConfiguration AutoWireProperties(this IFluentExportStrategyConfiguration configuration, Func<PropertyInfo, bool> propertyFilter = null)
        {
            configuration.ImportMembers(MembersThat.AreProperty(propertyFilter));

            return configuration;
        }
    }
}