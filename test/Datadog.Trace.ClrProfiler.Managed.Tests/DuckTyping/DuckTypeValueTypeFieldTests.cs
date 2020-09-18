using System.Collections.Generic;
using Datadog.Trace.ClrProfiler.DuckTyping;
using Xunit;

#pragma warning disable SA1201 // Elements must appear in the correct order

namespace Datadog.Trace.ClrProfiler.Managed.Tests.DuckTyping
{
    public class DuckTypeValueTypeFieldTests
    {
        public static IEnumerable<object[]> Data()
        {
            return new[]
            {
                new object[] { ObscureObject.GetFieldPublicObject() },
                new object[] { ObscureObject.GetFieldInternalObject() },
                new object[] { ObscureObject.GetFieldPrivateObject() },
            };
        }

        [Theory]
        [MemberData(nameof(Data))]
        public void StaticReadonlyFieldsSetException(object obscureObject)
        {
            Assert.Throws<DuckTypeFieldIsReadonlyException>(() =>
            {
                obscureObject.As<IObscureStaticReadonlyErrorDuckType>();
            });
        }

        [Theory]
        [MemberData(nameof(Data))]
        public void ReadonlyFieldsSetException(object obscureObject)
        {
            Assert.Throws<DuckTypeFieldIsReadonlyException>(() =>
            {
                obscureObject.As<IObscureReadonlyErrorDuckType>();
            });
        }

        [Theory]
        [MemberData(nameof(Data))]
        public void StaticReadonlyFields(object obscureObject)
        {
            var duckInterface = obscureObject.As<IObscureDuckType>();
            var duckAbstract = obscureObject.As<ObscureDuckTypeAbstractClass>();
            var duckVirtual = obscureObject.As<ObscureDuckType>();

            // *
            Assert.Equal(10, duckInterface.PublicStaticReadonlyValueTypeField);
            Assert.Equal(10, duckAbstract.PublicStaticReadonlyValueTypeField);
            Assert.Equal(10, duckVirtual.PublicStaticReadonlyValueTypeField);

            // *
            Assert.Equal(11, duckInterface.InternalStaticReadonlyValueTypeField);
            Assert.Equal(11, duckAbstract.InternalStaticReadonlyValueTypeField);
            Assert.Equal(11, duckVirtual.InternalStaticReadonlyValueTypeField);

            // *
            Assert.Equal(12, duckInterface.ProtectedStaticReadonlyValueTypeField);
            Assert.Equal(12, duckAbstract.ProtectedStaticReadonlyValueTypeField);
            Assert.Equal(12, duckVirtual.ProtectedStaticReadonlyValueTypeField);

            // *
            Assert.Equal(13, duckInterface.PrivateStaticReadonlyValueTypeField);
            Assert.Equal(13, duckAbstract.PrivateStaticReadonlyValueTypeField);
            Assert.Equal(13, duckVirtual.PrivateStaticReadonlyValueTypeField);
        }

        [Theory]
        [MemberData(nameof(Data))]
        public void StaticFields(object obscureObject)
        {
            var duckInterface = obscureObject.As<IObscureDuckType>();
            var duckAbstract = obscureObject.As<ObscureDuckTypeAbstractClass>();
            var duckVirtual = obscureObject.As<ObscureDuckType>();

            Assert.Equal(20, duckInterface.PublicStaticValueTypeField);
            Assert.Equal(20, duckAbstract.PublicStaticValueTypeField);
            Assert.Equal(20, duckVirtual.PublicStaticValueTypeField);

            duckInterface.PublicStaticValueTypeField = 42;
            Assert.Equal(42, duckInterface.PublicStaticValueTypeField);
            Assert.Equal(42, duckAbstract.PublicStaticValueTypeField);
            Assert.Equal(42, duckVirtual.PublicStaticValueTypeField);

            duckAbstract.PublicStaticValueTypeField = 50;
            Assert.Equal(50, duckInterface.PublicStaticValueTypeField);
            Assert.Equal(50, duckAbstract.PublicStaticValueTypeField);
            Assert.Equal(50, duckVirtual.PublicStaticValueTypeField);

            duckVirtual.PublicStaticValueTypeField = 60;
            Assert.Equal(60, duckInterface.PublicStaticValueTypeField);
            Assert.Equal(60, duckAbstract.PublicStaticValueTypeField);
            Assert.Equal(60, duckVirtual.PublicStaticValueTypeField);

            // *

            Assert.Equal(21, duckInterface.InternalStaticValueTypeField);
            Assert.Equal(21, duckAbstract.InternalStaticValueTypeField);
            Assert.Equal(21, duckVirtual.InternalStaticValueTypeField);

            duckInterface.InternalStaticValueTypeField = 42;
            Assert.Equal(42, duckInterface.InternalStaticValueTypeField);
            Assert.Equal(42, duckAbstract.InternalStaticValueTypeField);
            Assert.Equal(42, duckVirtual.InternalStaticValueTypeField);

            duckAbstract.InternalStaticValueTypeField = 50;
            Assert.Equal(50, duckInterface.InternalStaticValueTypeField);
            Assert.Equal(50, duckAbstract.InternalStaticValueTypeField);
            Assert.Equal(50, duckVirtual.InternalStaticValueTypeField);

            duckVirtual.InternalStaticValueTypeField = 60;
            Assert.Equal(60, duckInterface.InternalStaticValueTypeField);
            Assert.Equal(60, duckAbstract.InternalStaticValueTypeField);
            Assert.Equal(60, duckVirtual.InternalStaticValueTypeField);

            // *

            Assert.Equal(22, duckInterface.ProtectedStaticValueTypeField);
            Assert.Equal(22, duckAbstract.ProtectedStaticValueTypeField);
            Assert.Equal(22, duckVirtual.ProtectedStaticValueTypeField);

            duckInterface.ProtectedStaticValueTypeField = 42;
            Assert.Equal(42, duckInterface.ProtectedStaticValueTypeField);
            Assert.Equal(42, duckAbstract.ProtectedStaticValueTypeField);
            Assert.Equal(42, duckVirtual.ProtectedStaticValueTypeField);

            duckAbstract.ProtectedStaticValueTypeField = 50;
            Assert.Equal(50, duckInterface.ProtectedStaticValueTypeField);
            Assert.Equal(50, duckAbstract.ProtectedStaticValueTypeField);
            Assert.Equal(50, duckVirtual.ProtectedStaticValueTypeField);

            duckVirtual.ProtectedStaticValueTypeField = 60;
            Assert.Equal(60, duckInterface.ProtectedStaticValueTypeField);
            Assert.Equal(60, duckAbstract.ProtectedStaticValueTypeField);
            Assert.Equal(60, duckVirtual.ProtectedStaticValueTypeField);

            // *

            Assert.Equal(23, duckInterface.PrivateStaticValueTypeField);
            Assert.Equal(23, duckAbstract.PrivateStaticValueTypeField);
            Assert.Equal(23, duckVirtual.PrivateStaticValueTypeField);

            duckInterface.PrivateStaticValueTypeField = 42;
            Assert.Equal(42, duckInterface.PrivateStaticValueTypeField);
            Assert.Equal(42, duckAbstract.PrivateStaticValueTypeField);
            Assert.Equal(42, duckVirtual.PrivateStaticValueTypeField);

            duckAbstract.PrivateStaticValueTypeField = 50;
            Assert.Equal(50, duckInterface.PrivateStaticValueTypeField);
            Assert.Equal(50, duckAbstract.PrivateStaticValueTypeField);
            Assert.Equal(50, duckVirtual.PrivateStaticValueTypeField);

            duckVirtual.PrivateStaticValueTypeField = 60;
            Assert.Equal(60, duckInterface.PrivateStaticValueTypeField);
            Assert.Equal(60, duckAbstract.PrivateStaticValueTypeField);
            Assert.Equal(60, duckVirtual.PrivateStaticValueTypeField);
        }

        [Theory]
        [MemberData(nameof(Data))]
        public void ReadonlyFields(object obscureObject)
        {
            var duckInterface = obscureObject.As<IObscureDuckType>();
            var duckAbstract = obscureObject.As<ObscureDuckTypeAbstractClass>();
            var duckVirtual = obscureObject.As<ObscureDuckType>();

            // *
            Assert.Equal(30, duckInterface.PublicReadonlyValueTypeField);
            Assert.Equal(30, duckAbstract.PublicReadonlyValueTypeField);
            Assert.Equal(30, duckVirtual.PublicReadonlyValueTypeField);

            // *
            Assert.Equal(31, duckInterface.InternalReadonlyValueTypeField);
            Assert.Equal(31, duckAbstract.InternalReadonlyValueTypeField);
            Assert.Equal(31, duckVirtual.InternalReadonlyValueTypeField);

            // *
            Assert.Equal(32, duckInterface.ProtectedReadonlyValueTypeField);
            Assert.Equal(32, duckAbstract.ProtectedReadonlyValueTypeField);
            Assert.Equal(32, duckVirtual.ProtectedReadonlyValueTypeField);

            // *
            Assert.Equal(33, duckInterface.PrivateReadonlyValueTypeField);
            Assert.Equal(33, duckAbstract.PrivateReadonlyValueTypeField);
            Assert.Equal(33, duckVirtual.PrivateReadonlyValueTypeField);
        }

        [Theory]
        [MemberData(nameof(Data))]
        public void Fields(object obscureObject)
        {
            var duckInterface = obscureObject.As<IObscureDuckType>();
            var duckAbstract = obscureObject.As<ObscureDuckTypeAbstractClass>();
            var duckVirtual = obscureObject.As<ObscureDuckType>();

            Assert.Equal(40, duckInterface.PublicValueTypeField);
            Assert.Equal(40, duckAbstract.PublicValueTypeField);
            Assert.Equal(40, duckVirtual.PublicValueTypeField);

            duckInterface.PublicValueTypeField = 42;
            Assert.Equal(42, duckInterface.PublicValueTypeField);
            Assert.Equal(42, duckAbstract.PublicValueTypeField);
            Assert.Equal(42, duckVirtual.PublicValueTypeField);

            duckAbstract.PublicValueTypeField = 50;
            Assert.Equal(50, duckInterface.PublicValueTypeField);
            Assert.Equal(50, duckAbstract.PublicValueTypeField);
            Assert.Equal(50, duckVirtual.PublicValueTypeField);

            duckVirtual.PublicValueTypeField = 60;
            Assert.Equal(60, duckInterface.PublicValueTypeField);
            Assert.Equal(60, duckAbstract.PublicValueTypeField);
            Assert.Equal(60, duckVirtual.PublicValueTypeField);

            // *

            Assert.Equal(41, duckInterface.InternalValueTypeField);
            Assert.Equal(41, duckAbstract.InternalValueTypeField);
            Assert.Equal(41, duckVirtual.InternalValueTypeField);

            duckInterface.InternalValueTypeField = 42;
            Assert.Equal(42, duckInterface.InternalValueTypeField);
            Assert.Equal(42, duckAbstract.InternalValueTypeField);
            Assert.Equal(42, duckVirtual.InternalValueTypeField);

            duckAbstract.InternalValueTypeField = 50;
            Assert.Equal(50, duckInterface.InternalValueTypeField);
            Assert.Equal(50, duckAbstract.InternalValueTypeField);
            Assert.Equal(50, duckVirtual.InternalValueTypeField);

            duckVirtual.InternalValueTypeField = 60;
            Assert.Equal(60, duckInterface.InternalValueTypeField);
            Assert.Equal(60, duckAbstract.InternalValueTypeField);
            Assert.Equal(60, duckVirtual.InternalValueTypeField);

            // *

            Assert.Equal(42, duckInterface.ProtectedValueTypeField);
            Assert.Equal(42, duckAbstract.ProtectedValueTypeField);
            Assert.Equal(42, duckVirtual.ProtectedValueTypeField);

            duckInterface.ProtectedValueTypeField = 45;
            Assert.Equal(45, duckInterface.ProtectedValueTypeField);
            Assert.Equal(45, duckAbstract.ProtectedValueTypeField);
            Assert.Equal(45, duckVirtual.ProtectedValueTypeField);

            duckAbstract.ProtectedValueTypeField = 50;
            Assert.Equal(50, duckInterface.ProtectedValueTypeField);
            Assert.Equal(50, duckAbstract.ProtectedValueTypeField);
            Assert.Equal(50, duckVirtual.ProtectedValueTypeField);

            duckVirtual.ProtectedValueTypeField = 60;
            Assert.Equal(60, duckInterface.ProtectedValueTypeField);
            Assert.Equal(60, duckAbstract.ProtectedValueTypeField);
            Assert.Equal(60, duckVirtual.ProtectedValueTypeField);

            // *

            Assert.Equal(43, duckInterface.PrivateValueTypeField);
            Assert.Equal(43, duckAbstract.PrivateValueTypeField);
            Assert.Equal(43, duckVirtual.PrivateValueTypeField);

            duckInterface.PrivateValueTypeField = 42;
            Assert.Equal(42, duckInterface.PrivateValueTypeField);
            Assert.Equal(42, duckAbstract.PrivateValueTypeField);
            Assert.Equal(42, duckVirtual.PrivateValueTypeField);

            duckAbstract.PrivateValueTypeField = 50;
            Assert.Equal(50, duckInterface.PrivateValueTypeField);
            Assert.Equal(50, duckAbstract.PrivateValueTypeField);
            Assert.Equal(50, duckVirtual.PrivateValueTypeField);

            duckVirtual.PrivateValueTypeField = 60;
            Assert.Equal(60, duckInterface.PrivateValueTypeField);
            Assert.Equal(60, duckAbstract.PrivateValueTypeField);
            Assert.Equal(60, duckVirtual.PrivateValueTypeField);
        }

        [Theory]
        [MemberData(nameof(Data))]
        public void NullableOfKnown(object obscureObject)
        {
            var duckInterface = obscureObject.As<IObscureDuckType>();
            var duckAbstract = obscureObject.As<ObscureDuckTypeAbstractClass>();
            var duckVirtual = obscureObject.As<ObscureDuckType>();

            Assert.Null(duckInterface.PublicStaticNullableIntField);
            Assert.Null(duckAbstract.PublicStaticNullableIntField);
            Assert.Null(duckVirtual.PublicStaticNullableIntField);

            duckInterface.PublicStaticNullableIntField = 42;
            Assert.Equal(42, duckInterface.PublicStaticNullableIntField);
            Assert.Equal(42, duckAbstract.PublicStaticNullableIntField);
            Assert.Equal(42, duckVirtual.PublicStaticNullableIntField);

            duckAbstract.PublicStaticNullableIntField = 50;
            Assert.Equal(50, duckInterface.PublicStaticNullableIntField);
            Assert.Equal(50, duckAbstract.PublicStaticNullableIntField);
            Assert.Equal(50, duckVirtual.PublicStaticNullableIntField);

            duckVirtual.PublicStaticNullableIntField = null;
            Assert.Null(duckInterface.PublicStaticNullableIntField);
            Assert.Null(duckAbstract.PublicStaticNullableIntField);
            Assert.Null(duckVirtual.PublicStaticNullableIntField);

            // *

            Assert.Null(duckInterface.PrivateStaticNullableIntField);
            Assert.Null(duckAbstract.PrivateStaticNullableIntField);
            Assert.Null(duckVirtual.PrivateStaticNullableIntField);

            duckInterface.PrivateStaticNullableIntField = 42;
            Assert.Equal(42, duckInterface.PrivateStaticNullableIntField);
            Assert.Equal(42, duckAbstract.PrivateStaticNullableIntField);
            Assert.Equal(42, duckVirtual.PrivateStaticNullableIntField);

            duckAbstract.PrivateStaticNullableIntField = 50;
            Assert.Equal(50, duckInterface.PrivateStaticNullableIntField);
            Assert.Equal(50, duckAbstract.PrivateStaticNullableIntField);
            Assert.Equal(50, duckVirtual.PrivateStaticNullableIntField);

            duckVirtual.PrivateStaticNullableIntField = null;
            Assert.Null(duckInterface.PrivateStaticNullableIntField);
            Assert.Null(duckAbstract.PrivateStaticNullableIntField);
            Assert.Null(duckVirtual.PrivateStaticNullableIntField);

            // *

            Assert.Null(duckInterface.PublicNullableIntField);
            Assert.Null(duckAbstract.PublicNullableIntField);
            Assert.Null(duckVirtual.PublicNullableIntField);

            duckInterface.PublicNullableIntField = 42;
            Assert.Equal(42, duckInterface.PublicNullableIntField);
            Assert.Equal(42, duckAbstract.PublicNullableIntField);
            Assert.Equal(42, duckVirtual.PublicNullableIntField);

            duckAbstract.PublicNullableIntField = 50;
            Assert.Equal(50, duckInterface.PublicNullableIntField);
            Assert.Equal(50, duckAbstract.PublicNullableIntField);
            Assert.Equal(50, duckVirtual.PublicNullableIntField);

            duckVirtual.PublicNullableIntField = null;
            Assert.Null(duckInterface.PublicNullableIntField);
            Assert.Null(duckAbstract.PublicNullableIntField);
            Assert.Null(duckVirtual.PublicNullableIntField);

            // *

            Assert.Null(duckInterface.PrivateNullableIntField);
            Assert.Null(duckAbstract.PrivateNullableIntField);
            Assert.Null(duckVirtual.PrivateNullableIntField);

            duckInterface.PrivateNullableIntField = 42;
            Assert.Equal(42, duckInterface.PrivateNullableIntField);
            Assert.Equal(42, duckAbstract.PrivateNullableIntField);
            Assert.Equal(42, duckVirtual.PrivateNullableIntField);

            duckAbstract.PrivateNullableIntField = 50;
            Assert.Equal(50, duckInterface.PrivateNullableIntField);
            Assert.Equal(50, duckAbstract.PrivateNullableIntField);
            Assert.Equal(50, duckVirtual.PrivateNullableIntField);

            duckVirtual.PrivateNullableIntField = null;
            Assert.Null(duckInterface.PrivateNullableIntField);
            Assert.Null(duckAbstract.PrivateNullableIntField);
            Assert.Null(duckVirtual.PrivateNullableIntField);
        }

        public interface IObscureDuckType
        {
            [Duck(Name = "_publicStaticReadonlyValueTypeField", Kind = DuckKind.Field)]
            int PublicStaticReadonlyValueTypeField { get; }

            [Duck(Name = "_internalStaticReadonlyValueTypeField", Kind = DuckKind.Field)]
            int InternalStaticReadonlyValueTypeField { get; }

            [Duck(Name = "_protectedStaticReadonlyValueTypeField", Kind = DuckKind.Field)]
            int ProtectedStaticReadonlyValueTypeField { get; }

            [Duck(Name = "_privateStaticReadonlyValueTypeField", Kind = DuckKind.Field)]
            int PrivateStaticReadonlyValueTypeField { get; }

            // *

            [Duck(Name = "_publicStaticValueTypeField", Kind = DuckKind.Field)]
            int PublicStaticValueTypeField { get; set; }

            [Duck(Name = "_internalStaticValueTypeField", Kind = DuckKind.Field)]
            int InternalStaticValueTypeField { get; set; }

            [Duck(Name = "_protectedStaticValueTypeField", Kind = DuckKind.Field)]
            int ProtectedStaticValueTypeField { get; set; }

            [Duck(Name = "_privateStaticValueTypeField", Kind = DuckKind.Field)]
            int PrivateStaticValueTypeField { get; set; }

            // *

            [Duck(Name = "_publicReadonlyValueTypeField", Kind = DuckKind.Field)]
            int PublicReadonlyValueTypeField { get; }

            [Duck(Name = "_internalReadonlyValueTypeField", Kind = DuckKind.Field)]
            int InternalReadonlyValueTypeField { get; }

            [Duck(Name = "_protectedReadonlyValueTypeField", Kind = DuckKind.Field)]
            int ProtectedReadonlyValueTypeField { get; }

            [Duck(Name = "_privateReadonlyValueTypeField", Kind = DuckKind.Field)]
            int PrivateReadonlyValueTypeField { get; }

            // *

            [Duck(Name = "_publicValueTypeField", Kind = DuckKind.Field)]
            int PublicValueTypeField { get; set; }

            [Duck(Name = "_internalValueTypeField", Kind = DuckKind.Field)]
            int InternalValueTypeField { get; set; }

            [Duck(Name = "_protectedValueTypeField", Kind = DuckKind.Field)]
            int ProtectedValueTypeField { get; set; }

            [Duck(Name = "_privateValueTypeField", Kind = DuckKind.Field)]
            int PrivateValueTypeField { get; set; }

            // *

            [Duck(Name = "_publicStaticNullableIntField", Kind = DuckKind.Field)]
            int? PublicStaticNullableIntField { get; set; }

            [Duck(Name = "_privateStaticNullableIntField", Kind = DuckKind.Field)]
            int? PrivateStaticNullableIntField { get; set; }

            [Duck(Name = "_publicNullableIntField", Kind = DuckKind.Field)]
            int? PublicNullableIntField { get; set; }

            [Duck(Name = "_privateNullableIntField", Kind = DuckKind.Field)]
            int? PrivateNullableIntField { get; set; }
        }

        public interface IObscureStaticReadonlyErrorDuckType
        {
            [Duck(Name = "_publicStaticReadonlyValueTypeField", Kind = DuckKind.Field)]
            int PublicStaticReadonlyValueTypeField { get; set; }
        }

        public interface IObscureReadonlyErrorDuckType
        {
            [Duck(Name = "_publicReadonlyValueTypeField", Kind = DuckKind.Field)]
            int PublicReadonlyValueTypeField { get; set; }
        }

        public abstract class ObscureDuckTypeAbstractClass
        {
            [Duck(Name = "_publicStaticReadonlyValueTypeField", Kind = DuckKind.Field)]
            public abstract int PublicStaticReadonlyValueTypeField { get; }

            [Duck(Name = "_internalStaticReadonlyValueTypeField", Kind = DuckKind.Field)]
            public abstract int InternalStaticReadonlyValueTypeField { get; }

            [Duck(Name = "_protectedStaticReadonlyValueTypeField", Kind = DuckKind.Field)]
            public abstract int ProtectedStaticReadonlyValueTypeField { get; }

            [Duck(Name = "_privateStaticReadonlyValueTypeField", Kind = DuckKind.Field)]
            public abstract int PrivateStaticReadonlyValueTypeField { get; }

            // *

            [Duck(Name = "_publicStaticValueTypeField", Kind = DuckKind.Field)]
            public abstract int PublicStaticValueTypeField { get; set; }

            [Duck(Name = "_internalStaticValueTypeField", Kind = DuckKind.Field)]
            public abstract int InternalStaticValueTypeField { get; set; }

            [Duck(Name = "_protectedStaticValueTypeField", Kind = DuckKind.Field)]
            public abstract int ProtectedStaticValueTypeField { get; set; }

            [Duck(Name = "_privateStaticValueTypeField", Kind = DuckKind.Field)]
            public abstract int PrivateStaticValueTypeField { get; set; }

            // *

            [Duck(Name = "_publicReadonlyValueTypeField", Kind = DuckKind.Field)]
            public abstract int PublicReadonlyValueTypeField { get; }

            [Duck(Name = "_internalReadonlyValueTypeField", Kind = DuckKind.Field)]
            public abstract int InternalReadonlyValueTypeField { get; }

            [Duck(Name = "_protectedReadonlyValueTypeField", Kind = DuckKind.Field)]
            public abstract int ProtectedReadonlyValueTypeField { get; }

            [Duck(Name = "_privateReadonlyValueTypeField", Kind = DuckKind.Field)]
            public abstract int PrivateReadonlyValueTypeField { get; }

            // *

            [Duck(Name = "_publicValueTypeField", Kind = DuckKind.Field)]
            public abstract int PublicValueTypeField { get; set; }

            [Duck(Name = "_internalValueTypeField", Kind = DuckKind.Field)]
            public abstract int InternalValueTypeField { get; set; }

            [Duck(Name = "_protectedValueTypeField", Kind = DuckKind.Field)]
            public abstract int ProtectedValueTypeField { get; set; }

            [Duck(Name = "_privateValueTypeField", Kind = DuckKind.Field)]
            public abstract int PrivateValueTypeField { get; set; }

            // *

            [Duck(Name = "_publicStaticNullableIntField", Kind = DuckKind.Field)]
            public abstract int? PublicStaticNullableIntField { get; set; }

            [Duck(Name = "_privateStaticNullableIntField", Kind = DuckKind.Field)]
            public abstract int? PrivateStaticNullableIntField { get; set; }

            [Duck(Name = "_publicNullableIntField", Kind = DuckKind.Field)]
            public abstract int? PublicNullableIntField { get; set; }

            [Duck(Name = "_privateNullableIntField", Kind = DuckKind.Field)]
            public abstract int? PrivateNullableIntField { get; set; }
        }

        public class ObscureDuckType
        {
            [Duck(Name = "_publicStaticReadonlyValueTypeField", Kind = DuckKind.Field)]
            public virtual int PublicStaticReadonlyValueTypeField { get; }

            [Duck(Name = "_internalStaticReadonlyValueTypeField", Kind = DuckKind.Field)]
            public virtual int InternalStaticReadonlyValueTypeField { get; }

            [Duck(Name = "_protectedStaticReadonlyValueTypeField", Kind = DuckKind.Field)]
            public virtual int ProtectedStaticReadonlyValueTypeField { get; }

            [Duck(Name = "_privateStaticReadonlyValueTypeField", Kind = DuckKind.Field)]
            public virtual int PrivateStaticReadonlyValueTypeField { get; }

            // *

            [Duck(Name = "_publicStaticValueTypeField", Kind = DuckKind.Field)]
            public virtual int PublicStaticValueTypeField { get; set; }

            [Duck(Name = "_internalStaticValueTypeField", Kind = DuckKind.Field)]
            public virtual int InternalStaticValueTypeField { get; set; }

            [Duck(Name = "_protectedStaticValueTypeField", Kind = DuckKind.Field)]
            public virtual int ProtectedStaticValueTypeField { get; set; }

            [Duck(Name = "_privateStaticValueTypeField", Kind = DuckKind.Field)]
            public virtual int PrivateStaticValueTypeField { get; set; }

            // *

            [Duck(Name = "_publicReadonlyValueTypeField", Kind = DuckKind.Field)]
            public virtual int PublicReadonlyValueTypeField { get; }

            [Duck(Name = "_internalReadonlyValueTypeField", Kind = DuckKind.Field)]
            public virtual int InternalReadonlyValueTypeField { get; }

            [Duck(Name = "_protectedReadonlyValueTypeField", Kind = DuckKind.Field)]
            public virtual int ProtectedReadonlyValueTypeField { get; }

            [Duck(Name = "_privateReadonlyValueTypeField", Kind = DuckKind.Field)]
            public virtual int PrivateReadonlyValueTypeField { get; }

            // *

            [Duck(Name = "_publicValueTypeField", Kind = DuckKind.Field)]
            public virtual int PublicValueTypeField { get; set; }

            [Duck(Name = "_internalValueTypeField", Kind = DuckKind.Field)]
            public virtual int InternalValueTypeField { get; set; }

            [Duck(Name = "_protectedValueTypeField", Kind = DuckKind.Field)]
            public virtual int ProtectedValueTypeField { get; set; }

            [Duck(Name = "_privateValueTypeField", Kind = DuckKind.Field)]
            public virtual int PrivateValueTypeField { get; set; }

            // *

            [Duck(Name = "_publicStaticNullableIntField", Kind = DuckKind.Field)]
            public virtual int? PublicStaticNullableIntField { get; set; }

            [Duck(Name = "_privateStaticNullableIntField", Kind = DuckKind.Field)]
            public virtual int? PrivateStaticNullableIntField { get; set; }

            [Duck(Name = "_publicNullableIntField", Kind = DuckKind.Field)]
            public virtual int? PublicNullableIntField { get; set; }

            [Duck(Name = "_privateNullableIntField", Kind = DuckKind.Field)]
            public virtual int? PrivateNullableIntField { get; set; }
        }
    }
}