﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Web.Mvc;
using Grace.DependencyInjection;

namespace Grace.MVC.DependencyInjection
{
    /// <summary>
    /// Activates a new view
    /// </summary>
    public class ViewPageActivator : IViewPageActivator
    {
        private readonly IExportLocator _container;

        /// <summary>
        /// Default constructor
        /// </summary>
        /// <param name="container"></param>
        public ViewPageActivator(IExportLocator container)
        {
            _container = container;
        }

        public object Create(ControllerContext controllerContext, Type type)
        {
            object returnObject = _container.Locate(type);

            if (returnObject == null)
            {
                _container.Configure(c => c.Export(type).ImportAttributedMembers());

                returnObject = _container.Locate(type);
            }

            return returnObject;
        }
    }
}
