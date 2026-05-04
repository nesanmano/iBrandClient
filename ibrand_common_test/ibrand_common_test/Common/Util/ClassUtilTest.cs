using NomadLogic.Common.Util;
using NomadLogic.iBrandDesktop.Ui.ApplicationScreen;
using NUnit.Framework;
// ReSharper disable UnusedTypeParameter

namespace NomadLogic.Test.Common.Util
{

    /// <summary>
    /// https://stackoverflow.com/questions/457676/check-if-a-class-is-derived-from-a-generic-class
    /// </summary>
    public class ClassUtilTest : TestBaseNunit
    {
        #region Base Test classes

        protected interface IFooInterface
        {
        }

        protected interface IGenericFooInterface<T>
        {
        }

        protected class FooBase
        {
        }

        protected class FooImplementor
            : FooBase, IFooInterface
        {
        }

        protected class GenericFooBase
            : FooImplementor, IGenericFooInterface<object>
        {

        }

        protected class GenericFooImplementor<T>
            : FooImplementor, IGenericFooInterface<T>
        {
        }

        #endregion

        [Test]
        public void Should_inherit_or_implement_non_generic_interface()
        {
            Assert.That(typeof(FooImplementor)
                .InheritsOrImplements(typeof(IFooInterface)), Is.True);
        }

        [Test]
        public void Should_inherit_or_implement_generic_interface()
        {
            Assert.That(typeof(GenericFooBase)
                .InheritsOrImplements(typeof(IGenericFooInterface<>)), Is.True);
        }

        [Test]
        public void Should_inherit_or_implement_generic_interface_by_generic_subclass()
        {
            Assert.That(typeof(GenericFooImplementor<>)
                .InheritsOrImplements(typeof(IGenericFooInterface<>)), Is.True);
        }

        [Test]
        public void Should_inherit_or_implement_generic_interface_by_generic_subclass_not_caring_about_generic_type_parameter()
        {
            Assert.That(new GenericFooImplementor<string>().GetType()
                .InheritsOrImplements(typeof(IGenericFooInterface<>)), Is.True);
        }

        [Test]
        public void Should_not_inherit_or_implement_generic_interface_by_generic_subclass_not_caring_about_generic_type_parameter()
        {
            Assert.That(new GenericFooImplementor<string>().GetType()
                .InheritsOrImplements(typeof(IGenericFooInterface<int>)), Is.False);
        }

        [Test]
        public void Should_inherit_or_implement_non_generic_class()
        {
            Assert.That(typeof(FooImplementor)
                .InheritsOrImplements(typeof(FooBase)), Is.True);
        }

        [Test]
        public void Should_inherit_or_implement_any_base_type()
        {
            Assert.That(typeof(GenericFooImplementor<>)
                .InheritsOrImplements(typeof(FooBase)), Is.True);
        }
        
        [Test]
        public void Same_Type()
        {
            Assert.That(typeof(FooBase)
                .InheritsOrImplements(typeof(FooBase)), Is.True);
        }

        /// <summary>
        /// Test avec héritage multiple 
        /// </summary>
        [Test]
        public void LoaderClassTest()
        {
            var className = "LoaderGes201901";
            var @namespace = "NomadLogic.iBrandDesktop.Ui.ApplicationScreen.FullscreenLoaders";
            var fullClassName = $"{@namespace}.{className}";

            var goodTypes = new [] { typeof(IFullscreenLoader) };

            Assert.That(ClassUtil.CheckIfClassExists(className, @namespace), Is.Not.Null);
            Assert.That(ClassUtil.CheckIfClassExists(fullClassName), Is.Not.Null);
            Assert.That(ClassUtil.CheckIfClassExists(fullClassName, "notImportant"), Is.Null); // Si on cherche dans un assembly externe, cela va causer une erreur.
            Assert.That(ClassUtil.CheckIfClassExists(className), Is.Null);

            Assert.That(ClassUtil.CheckIfClassExists(fullClassName, null, goodTypes), Is.Not.Null);
            Assert.That(ClassUtil.CheckIfClassExists(fullClassName, null, typeof(IFullscreenLoader)), Is.Not.Null);
        }
    }
}