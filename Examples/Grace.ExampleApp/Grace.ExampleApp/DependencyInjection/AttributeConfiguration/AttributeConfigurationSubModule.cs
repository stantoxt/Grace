﻿using System.Collections.Generic;

namespace Grace.ExampleApp.DependencyInjection.AttributeConfiguration
{
	public class AttributeSubModule : IExampleSubModule<DependencyInjectionExampleModule>
	{
		private readonly IEnumerable<IExample<AttributeSubModule>> examples;

		public AttributeSubModule(IEnumerable<IExample<AttributeSubModule>> examples)
		{
			this.examples = examples;
		}

		public void Execute()
		{
			foreach (IExample<AttributeSubModule> example in examples)
			{
				example.ExecuteExample();
			}
		}
	}
}
