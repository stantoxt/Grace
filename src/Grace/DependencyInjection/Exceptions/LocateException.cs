﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace Grace.DependencyInjection.Exceptions
{
    public class LocateException : Exception
    {
        private StaticInjectionContext _context;

        public LocateException(StaticInjectionContext context) : base(CreateMessage(context))
        {
            _context = context;
        }

        private static string CreateMessage(StaticInjectionContext context)
        {
            var infoStack = new List<InjectionTargetInfo>(context.InjectionStack.Reverse());
            var builder = new StringBuilder();

            builder.AppendFormat("Could not locate Type {0}", context.ActivationType);

            for (int i = 0; i < infoStack.Count; i++)
            {
                CreateMessageForTargetInfo(builder, infoStack[i], i + 1);
            }

            return builder.ToString();
        }

        private static void CreateMessageForTargetInfo(StringBuilder builder, InjectionTargetInfo info, int stepIndex)
        {
            builder.AppendFormat("Importing {0} ", info.LocateType);

            var parameter = info.InjectionTarget as ParameterInfo;

            if (parameter != null)
            {
                var method = parameter.Member as MethodInfo;

                if (method != null)
                {
                    builder.AppendFormat(" for method {0} parameter {1}", method.Name, parameter.Name);
                }
                else
                {
                    builder.AppendFormat(" for constructor parameter {0}", parameter.Name);
                }
            }
            else if (info.InjectionTarget is PropertyInfo)
            {
                builder.AppendFormat(" for property {0}", ((PropertyInfo)info.InjectionTarget).Name);
            }
        }

        public LocateException(StaticInjectionContext context, Exception innerException) : base(CreateMessage(context), innerException)
        {
            _context = context;
        }

    }
}