﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Grace.DependencyInjection;
using Grace.DependencyInjection.Lifestyle;
using Grace.Diagnostics;
using Grace.UnitTests.Classes.Simple;
using Xunit;
using Grace.UnitTests.Classes.Attributed;
using Grace.DependencyInjection.Attributes;
using Grace.DependencyInjection.Impl;

namespace Grace.UnitTests.DependencyInjection
{
    public class AdvancedContainerTests
    {
        #region Metdata tests

        [Fact]
        public void LocateAllWithMetadataTest()
        {
            DependencyInjectionContainer container = new DependencyInjectionContainer();

            container.Configure(c =>
            {
                c.Export<SimpleObjectA>().As<ISimpleObject>().WithMetadata("Metadata", "Group1");
                c.Export<SimpleObjectB>().As<ISimpleObject>().WithMetadata("Metadata", "Group1");
                c.Export<SimpleObjectC>().As<ISimpleObject>().WithMetadata("Metadata", "Group1");
                c.Export<SimpleObjectD>().As<ISimpleObject>().WithMetadata("Metadata", "Group2");
                c.Export<SimpleObjectE>().As<ISimpleObject>().WithMetadata("Metadata", "Group2");
            });
            var list = container.LocateAllWithMetadata<ISimpleObject>("Metadata");

            Assert.NotNull(list);

            Assert.Equal(5, list.Count());
        }

        [Fact]
        public void LocateAllWithMetadataFiltered()
        {
            DependencyInjectionContainer container = new DependencyInjectionContainer();

            container.Configure(c =>
            {
                c.Export<SimpleObjectA>().As<ISimpleObject>().WithMetadata("Metadata", "Group1");
                c.Export<SimpleObjectB>().As<ISimpleObject>().WithMetadata("Metadata", "Group1");
                c.Export<SimpleObjectC>().As<ISimpleObject>().WithMetadata("Metadata", "Group1");
                c.Export<SimpleObjectD>().As<ISimpleObject>().WithMetadata("Metadata", "Group2");
                c.Export<SimpleObjectE>().As<ISimpleObject>().WithMetadata("Metadata", "Group2");
            });

            var list = container.LocateAllWithMetadata<ISimpleObject>("Metadata", "Group1");

            Assert.NotNull(list);

            Assert.Equal(3, list.Count());

        }

        #endregion
        
        #region new context

        [Fact]
        public void InNewContextTest()
        {
            DependencyInjectionContainer container = new DependencyInjectionContainer();

            container.Configure(c =>
                                      {
                                          c.Export<ContextSingleton>()
                                            .ByInterfaces()
                                            .UsingLifestyle(new SingletonPerInjectionContextLifestyle());
                                          c.Export<ContextClassA>().ByInterfaces();
                                          c.Export<ContextClassB>().ByInterfaces().InNewContext();
                                          c.Export<ContextClassC>().ByInterfaces();
                                      });

            IContextClassA classA = container.Locate<IContextClassA>();

            Assert.NotNull(classA);
            Assert.NotNull(classA.ContextClassB);
            Assert.NotNull(classA.ContextClassB.ContextClassC);

            Assert.NotNull(classA.ContextSingleton);
            Assert.NotNull(classA.ContextClassB.ContextSingleton);
            Assert.NotNull(classA.ContextClassB.ContextClassC.ContextSingleton);

            Assert.NotSame(classA.ContextSingleton, classA.ContextClassB.ContextSingleton);
            Assert.NotSame(classA.ContextSingleton, classA.ContextClassB.ContextClassC.ContextSingleton);
            Assert.Same(classA.ContextClassB.ContextSingleton, classA.ContextClassB.ContextClassC.ContextSingleton);

        }

        #endregion

        #region BeginLifetimeScope

        [Fact]
        public void BeginLifetimeScope()
        {
            DependencyInjectionContainer container = new DependencyInjectionContainer();

            container.Configure(c => c.Export<DisposableService>().As<IDisposableService>().Lifestyle.SingletonPerScope());

            IDisposableService service = container.Locate<IDisposableService>();

            Assert.NotNull(service);

            bool called = false;

            using (var scope = container.BeginLifetimeScope())
            {
                var secondService = scope.Locate<IDisposableService>();

                Assert.NotNull(secondService);
                Assert.NotSame(service, secondService);
                Assert.Same(secondService, scope.Locate<IDisposableService>());

                secondService.Disposing += (sender, args) => called = true;
            }

            Assert.True(called);
        }

        [Fact]
        public void BeginLifetimeScopeReturnsCorrectNumberForIEnumerable()
        {
            DependencyInjectionContainer container = new DependencyInjectionContainer();

            container.Configure(c => c.Export<DisposableService>().As<IDisposableService>().Lifestyle.SingletonPerScope());

            bool called = false;

            using (var scope = container.BeginLifetimeScope())
            {
                var allServices = scope.Locate<IEnumerable<IDisposableService>>();

                Assert.Equal(1, allServices.Count());
                allServices.First().Disposing += (sender, args) => called = true;
            }

            Assert.True(called);
        }
        #endregion

        #region WithNamedCtorValue
        [Fact]
        public void WithNamedCtorValue()
        {
            DependencyInjectionContainer container = new DependencyInjectionContainer();
            DateTime currentTime = DateTime.Now;

            container.Configure(c => c.Export(typeof(DateTimeImport)).WithNamedCtorValue(() => currentTime));

            DateTimeImport import = container.Locate<DateTimeImport>();

            Assert.NotNull(import);
            Assert.Equal(currentTime, import.CurrentTime);
        }

        [Fact]
        public void WithNamedCtorValueGeneric()
        {
            DependencyInjectionContainer container = new DependencyInjectionContainer();
            DateTime currentTime = DateTime.Now;

            container.Configure(c => c.Export<DateTimeImport>().WithNamedCtorValue(() => currentTime));

            DateTimeImport import = container.Locate<DateTimeImport>();

            Assert.NotNull(import);
            Assert.Equal(currentTime, import.CurrentTime);
        }


        [Fact]
        public void WithNamedCtorValueGenericNow()
        {
            DependencyInjectionContainer container = new DependencyInjectionContainer();

            container.Configure(c => c.Export<NowDateTimeImport>().WithNamedCtorValue(() => DateTime.Now));

            NowDateTimeImport import = container.Locate<NowDateTimeImport>();

            Assert.NotNull(import);
            Assert.Equal(import.CurrentTime.Date, DateTime.Now.Date);
        }

        [Fact]
        public void ExportNamedValue()
        {
            DependencyInjectionContainer container = new DependencyInjectionContainer();
            DateTime currentTime = DateTime.Now;

            container.Configure(c => c.Export<DateTimeImport>());
            container.Configure(c => c.ExportNamedValue(() => currentTime));

            DateTimeImport import = container.Locate<DateTimeImport>();

            Assert.NotNull(import);
            Assert.Equal(currentTime, import.CurrentTime);
        }
        #endregion

        #region WithInspectorFor

        [Fact]
        public void WithInspectorFor()
        {
            DependencyInjectionContainer container = new DependencyInjectionContainer();

            container.Configure(c => c.Export(Types.FromThisAssembly())
                                      .ByInterfaces()
                                      .WithInspectorFor<ISimpleObject>(e => e.AddMetadata("SimpleObject", e.ActivationType)));

            var list =
                container.LocateAll<ISimpleObject>(consider: ExportsThat.HaveMetadata("SimpleObject"));

            Assert.Equal(5, list.Count);

            list =
                container.LocateAll<ISimpleObject>(consider:
                    ExportsThat.HaveMetadata("SimpleObject", typeof(SimpleObjectA)));

            Assert.Equal(1, list.Count);
        }



        #endregion

        #region BasedOn Interface Test

        [Fact]
        public void BasedOnInterfaceTest()
        {
            DependencyInjectionContainer container = new DependencyInjectionContainer();

            container.Configure(c => c.Export(Types.FromThisAssembly())
                                              .BasedOn<ISimpleObject>()
                                              .ByInterfaces());

            IEnumerable<ISimpleObject> simpleObjects = container.LocateAll<ISimpleObject>();

            Assert.NotNull(simpleObjects);
            Assert.Equal(5, simpleObjects.Count());
        }

        [Fact]
        public void BasedOnInterfaceByType()
        {
            DependencyInjectionContainer container = new DependencyInjectionContainer();

            container.Configure(c => c.Export(Types.FromThisAssembly())
                                              .BasedOn<ISimpleObject>()
                                              .ByType());

            SimpleObjectA simpleObjectA = container.Locate<SimpleObjectA>();

            Assert.NotNull(simpleObjectA);
        }

        #endregion

        #region TypesThat tests

        [Fact]
        public void TypesThatAreBasedOn()
        {
            DependencyInjectionContainer container = new DependencyInjectionContainer();

            container.Configure(c => c.Export(Types.FromThisAssembly())
                                      .ByInterfaces()
                                      .Select(TypesThat.AreBasedOn<ISimpleObject>()));

            List<ISimpleObject> simpleObjects = container.LocateAll<ISimpleObject>();

            Assert.NotNull(simpleObjects);
            Assert.Equal(5, simpleObjects.Count);

        }

        [Fact]
        public void TypesThatAreBasedOnChainedFilter()
        {
            DependencyInjectionContainer container = new DependencyInjectionContainer();

            container.Configure(c => c.Export(Types.FromThisAssembly())
                                      .ByInterfaces()
                                      .Select(TypesThat.AreBasedOn<ISimpleObject>().And.HaveAttribute<SimpleFilterAttribute>()));

            List<ISimpleObject> simpleObjects = container.LocateAll<ISimpleObject>();

            Assert.NotNull(simpleObjects);
            Assert.Equal(3, simpleObjects.Count);

        }

        [Fact]
        public void TypesThatAreBasedOnComplexChainedFilter()
        {
            DependencyInjectionContainer container = new DependencyInjectionContainer();

            container.Configure(c => c.Export(Types.FromThisAssembly())
                                      .ByInterfaces()
                                      .Select(TypesThat.AreBasedOn(TypesThat.StartWith("ISimple"))));

            List<ISimpleObject> simpleObjects = container.LocateAll<ISimpleObject>();

            Assert.NotNull(simpleObjects);
            Assert.Equal(5, simpleObjects.Count);

        }

        [Fact]
        public void TypesThatHaveAttributeChainedFilter()
        {
            DependencyInjectionContainer container = new DependencyInjectionContainer();

            container.Configure(c => c.Export(Types.FromThisAssembly())
                                      .ByInterfaces()
                                      .Select(TypesThat.AreBasedOn(TypesThat.HaveAttribute(TypesThat.StartWith("Simple")))));

            List<ISimpleObject> simpleObjects = container.LocateAll<ISimpleObject>();

            Assert.NotNull(simpleObjects);
            Assert.Equal(5, simpleObjects.Count);

        }

        [Fact]
        public void FromThisAssemblyFilter()
        {
            DependencyInjectionContainer container = new DependencyInjectionContainer();

            container.Configure(c => c.Export(Types.FromThisAssembly(consider: TypesThat.AreBasedOn(TypesThat.HaveAttribute(TypesThat.StartWith("Simple")))))
                                      .ByInterfaces());

            List<ISimpleObject> simpleObjects = container.LocateAll<ISimpleObject>();

            Assert.NotNull(simpleObjects);
            Assert.Equal(5, simpleObjects.Count);

        }

        [Fact]
        public void TypesThatHaveAttributeFilter()
        {
            DependencyInjectionContainer container = new DependencyInjectionContainer();

            container.Configure(c => c.Export(Types.FromThisAssembly())
                                      .ByInterfaces()
                                      .Select(TypesThat.HaveAttribute(TypesThat.StartWith("Simple"))));

            List<ISimpleObject> simpleObjects = container.LocateAll<ISimpleObject>();

            Assert.NotNull(simpleObjects);
            Assert.Equal(3, simpleObjects.Count);

        }
        #endregion

        #region Prioritize

        [Fact]
        public void PrioritizeTypesThat()
        {
            DependencyInjectionContainer container = new DependencyInjectionContainer();

            container.Configure(c => c.Export(Types.FromThisAssembly())
                                      .ByInterfaces()
                                      .Select(TypesThat.AreBasedOn<ISimpleObject>())
                                      .Prioritize(TypesThat.EndWith("C")));

            var simpleObject = container.Locate<ISimpleObject>();

            Assert.NotNull(simpleObject);
            Assert.IsType<SimpleObjectC>(simpleObject);
        }

        #endregion

        #region Lifestyle Tests

        [Fact]
        public void LifestyleSingleton()
        {
            DependencyInjectionContainer container = new DependencyInjectionContainer();

            container.Configure(c => c.Export<BasicService>()
                                      .As<IBasicService>()
                                      .Lifestyle.Singleton());

            IBasicService basicService = container.Locate<IBasicService>();

            Assert.NotNull(basicService);
            Assert.Same(basicService, container.Locate<IBasicService>());
        }

        [Fact]
        public void LifestyleSingletonPerScope()
        {
            DependencyInjectionContainer container = new DependencyInjectionContainer();

            container.Configure(c => c.Export<BasicService>()
                                      .As<IBasicService>()
                                      .Lifestyle.SingletonPerScope());

            IBasicService basicService = container.Locate<IBasicService>();

            Assert.NotNull(basicService);
            Assert.Same(basicService,container.Locate<IBasicService>());

            using (var scope = container.BeginLifetimeScope())
            {
                IBasicService basicService2 = scope.Locate<IBasicService>();

                Assert.NotNull(basicService2);
                Assert.Same(basicService2,scope.Locate<IBasicService>());
                Assert.NotSame(basicService,basicService2);
            }
        }

        [Fact]
        public void BulkLifestyleSingleton()
        {
            DependencyInjectionContainer container = new DependencyInjectionContainer();

            container.Configure(c => c.Export(Types.FromThisAssembly())
                                      .ByInterface<IBasicService>()
                                      .Lifestyle.Singleton());

            IBasicService basicService = container.Locate<IBasicService>();

            Assert.NotNull(basicService);
            Assert.Same(basicService, container.Locate<IBasicService>());
        }

        [Fact]
        public void BulkLifestyleSingletonPerScope()
        {
            DependencyInjectionContainer container = new DependencyInjectionContainer();

            container.Configure(c => c.Export(Types.FromThisAssembly())
                                      .ByInterface<IBasicService>()
                                      .Lifestyle.SingletonPerScope());

            IBasicService basicService = container.Locate<IBasicService>();

            Assert.NotNull(basicService);
            Assert.Same(basicService, container.Locate<IBasicService>());

            using (var scope = container.BeginLifetimeScope())
            {
                IBasicService basicService2 = scope.Locate<IBasicService>();

                Assert.NotNull(basicService2);
                Assert.Same(basicService2, scope.Locate<IBasicService>());
                Assert.NotSame(basicService, basicService2);
            }
        }

        #endregion

        #region PropertyInspector

        [Fact]
        public void PropertyInjectorInspector()
        {
            DependencyInjectionContainer container = new DependencyInjectionContainer();

            container.AddStrategyInspector(new PropertyInjectionInspector<IBasicService>());

            container.Configure(c =>
                                {
                                    c.Export<BasicService>().As<IBasicService>();
                                    c.Export<ImportPropertyService>().As<IImportPropertyService>();
                                });

            IImportPropertyService propertyService = container.Locate<IImportPropertyService>();

            Assert.NotNull(propertyService);
            Assert.NotNull(propertyService.BasicService);
        }

        [Fact]
        public void PropertyInjectorInspectorChildScope()
        {
            DependencyInjectionContainer container = new DependencyInjectionContainer();

            container.AddStrategyInspector(new PropertyInjectionInspector<IBasicService>());

            var child = container.CreateChildScope();

            child.Configure(c =>
            {
                c.Export<BasicService>().As<IBasicService>();
                c.Export<ImportPropertyService>().As<IImportPropertyService>();
            });

            IImportPropertyService propertyService = child.Locate<IImportPropertyService>();

            Assert.NotNull(propertyService);
            Assert.NotNull(propertyService.BasicService);
        }

        #endregion

        #region Import Attribute

        [Fact]
        public void ImportAttributedMemberForExports()
        {
            DependencyInjectionContainer container = new DependencyInjectionContainer();

            container.Configure(
                c => c.Export(Types.FromThisAssembly(TypesThat.AreInTheSameNamespace("Grace.UnitTests.Classes.Attributed")))
                      .ByInterfaces()
                      .ImportAttributedMembers());

            var importService = container.Locate<IAttributedImportPropertyService>();

            Assert.NotNull(importService);
            Assert.NotNull(importService.BasicService);
        }

        #endregion

        #region Import Members

        [Fact]
        public void ImportMembersForExports()
        {
            DependencyInjectionContainer container = new DependencyInjectionContainer();

            container.Configure(
                c => c.Export(Types.FromThisAssembly(TypesThat.AreInTheSameNamespace("Grace.UnitTests.Classes.Attributed")))
                      .ByInterfaces()
                      .ImportMembers(MembersThat.HaveAttribute<ImportAttribute>().And.AreProperty()));

            var importService = container.Locate<IAttributedImportPropertyService>();

            Assert.NotNull(importService);
            Assert.NotNull(importService.BasicService);

        }

        #endregion

        #region Attributed Export Property test
        [Fact]
        public void ExportAttributedPropertyTest()
        {
            DependencyInjectionContainer container = new DependencyInjectionContainer();

            container.Configure(c => c.Export(Types.From(typeof(AttributedExportService), typeof(AttributeImportConstructorService))).ExportAttributedTypes());

            var constructor = container.Locate<IAttributeImportConstructorService>();

            Assert.NotNull(constructor);
        }
        #endregion

        #region ProcessAttributes tests
        [Fact]
        public void ExportByInterfacesProcessAttributes()
        {
            var container = new DependencyInjectionContainer();

            container.Configure(c => c.Export(Types.FromThisAssembly(TypesThat.AreInTheSameNamespaceAs<IAttributeBasicService>()))
                                        .ByInterfaces().ProcessAttributes());

            var instance = container.Locate<IAttributedImportMethodService>();


            Assert.NotNull(instance);
            Assert.IsType<AttributeBasicService>(instance.BasicService);
        }

        [Fact]
        public void ExportByInterfacesProcessAttributesPriority()
        {
            var container = new DependencyInjectionContainer();

            container.Configure(c => c.Export(Types.FromThisAssembly(TypesThat.AreInTheSameNamespaceAs<IAttributeBasicService>()))
                                        .ByInterfaces().ProcessAttributes());

            var instances = container.Locate<IPriorityAttributeService[]>();

            Assert.Equal(5, instances.Length);
            Assert.IsType<PriorityAttributeServiceE>(instances[0]);
            Assert.IsType<PriorityAttributeServiceD>(instances[1]);
            Assert.IsType<PriorityAttributeServiceC>(instances[2]);
            Assert.IsType<PriorityAttributeServiceB>(instances[3]);
            Assert.IsType<PriorityAttributeServiceA>(instances[4]);
        }
        #endregion

        #region Registration Order Test
        [Fact]
        public void ContainerKeepRegistrationOrder()
        {
            var container = new DependencyInjectionContainer();

            container.Configure(
                c => 
                {
                    c.ExportAs<SimpleObjectA, ISimpleObject>();
                    c.ExportAs<SimpleObjectB, ISimpleObject>();
                    c.ExportAs<SimpleObjectC, ISimpleObject>();
                    c.ExportAs<SimpleObjectD, ISimpleObject>();
                    c.ExportAs<SimpleObjectE, ISimpleObject>();
                });

            var exports = container.Locate<ISimpleObject[]>();

            Assert.Equal(5, exports.Length);
            Assert.IsType<SimpleObjectA>(exports[0]);
            Assert.IsType<SimpleObjectB>(exports[1]);
            Assert.IsType<SimpleObjectC>(exports[2]);
            Assert.IsType<SimpleObjectD>(exports[3]);
            Assert.IsType<SimpleObjectE>(exports[4]);

            var container2 = new DependencyInjectionContainer();

            container2.Configure(
                c =>
                {
                    c.ExportAs<SimpleObjectE, ISimpleObject>();
                    c.ExportAs<SimpleObjectD, ISimpleObject>();
                    c.ExportAs<SimpleObjectC, ISimpleObject>();
                    c.ExportAs<SimpleObjectB, ISimpleObject>();
                    c.ExportAs<SimpleObjectA, ISimpleObject>();
                });

            exports = container2.Locate<ISimpleObject[]>();

            Assert.Equal(5, exports.Length);
            Assert.IsType<SimpleObjectE>(exports[0]);
            Assert.IsType<SimpleObjectD>(exports[1]);
            Assert.IsType<SimpleObjectC>(exports[2]);
            Assert.IsType<SimpleObjectB>(exports[3]);
            Assert.IsType<SimpleObjectA>(exports[4]);
        }
        #endregion
        
        
        #region Lifetime resolve IEnumerable Test

        [Fact]
        public void BeginLifeTimeScopeRequestIEnumerable()
        {
            var container = new DependencyInjectionContainer();

            using (var scope = container.BeginLifetimeScope())
            {
                IEnumerable<ISimpleObject> objects;

                Assert.True(scope.TryLocate(out objects));
            }
        }

        #endregion

        #region injection context information correct

        [Fact]
        public void InjectionContextInformationIsCorrect()
        {
            var container = new DependencyInjectionContainer();

            IInjectionTargetInfo targetInfo = null;

            container.Configure(c => 
            {
                c.ExportInstance<IBasicService>((scope, context) =>
                {
                    targetInfo = context.TargetInfo;

                    var stack = context.GetInjectionStack();

                    Assert.Same(targetInfo, stack.Last().TargetInfo);

                    return new BasicService();
                });

                c.Export<ImportConstructorService>().As<IImportConstructorService>();
            });

            var service = container.Locate<IImportConstructorService>();

            Assert.NotNull(targetInfo);
            Assert.Equal(targetInfo.InjectionTargetType, typeof(IBasicService));
            Assert.Equal(targetInfo.InjectionType, typeof(ImportConstructorService));
        }

        #endregion

        #region Injection Value Provider
        [Fact]
        public void InjectionValueProviderInspectorTest()
        {
            var container = new DependencyInjectionContainer
            {
                c => c.Export<ImportConstructorService>().ByInterfaces()
            };

            container.AddInjectionValueProviderInspector(new BasicServiceInjectionInspector());

            var service = container.Locate<IImportConstructorService>();

            Assert.NotNull(service);
            Assert.Equal(10, service.BasicService.Count);
        }

        public class BasicServiceInjectionInspector : IInjectionValueProviderInspector
        {
            public IExportValueProvider GetValueProvider(IInjectionScope scope, IInjectionTargetInfo targetInfo, IExportValueProvider valueProvider, ExportStrategyFilter exportStrategyFilter, ILocateKeyValueProvider locateKey)
            {
                if(targetInfo.InjectionTargetType == typeof(IBasicService))
                {
                    return new FuncValueProvider<IBasicService>(() => new BasicService { Count = 10});
                }

                return null;
            }
        }
        #endregion
    }
}
