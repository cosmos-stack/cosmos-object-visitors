﻿using CosmosObjectVisitorUT.Helpers;
using CosmosObjectVisitorUT.Model;
using Cosmos.Reflection.ObjectVisitors;
using Cosmos.Validation;
using Shouldly;
using Xunit;

namespace CosmosObjectVisitorUT
{
    [Trait("Validation.FluentApi", "Validation")]
    public class ValidationFluentApiTests : Prepare
    {
        [Fact(DisplayName = "直接类型自定义消息验证测试")]
        public void DirectFutureWithFluentApiTest()
        {
            var type = typeof(NiceAct);
            var v = ObjectVisitor.Create(type);

            v["Name"] = "Balala";

            v.VerifiableEntry.ForMember("Name", c => c.Must(NameMustBeBalala).WithMessage("MustBala!"));

            var r = v.Verify();

            Assert.True(r.IsValid);

            v["Name"] = "Bong";

            r = v.Verify();

            r.IsValid.ShouldBeFalse();

            Assert.Single(r.Errors);
            Assert.Single(r.Errors[0].Details);
            Assert.Equal("MustBala!", r.Errors[0].Details[0].ErrorMessage);
        }

        [Fact(DisplayName = "泛型类型自定义消息验证测试")]
        public void GenericFutureWithFluentApiTest()
        {
            var v = ObjectVisitor.Create<NiceAct>();

            v["Name"] = "Balala";

            v.VerifiableEntry.ForMember("Name", c => c.Must(NameMustBeBalala).WithMessage("MustBala!"));

            var r = v.Verify();

            Assert.True(r.IsValid);

            v["Name"] = "Bong";

            r = v.Verify();

            Assert.False(r.IsValid);
            Assert.Single(r.Errors);
            Assert.Single(r.Errors[0].Details);
            Assert.Equal("MustBala!", r.Errors[0].Details[0].ErrorMessage);
        }

        [Fact(DisplayName = "泛型类型和泛型值自定义消息验证测试")]
        public void GenericFutureWithFluentApiAndValTest()
        {
            var v = ObjectVisitor.Create<NiceAct>();

            v["Name"] = "Balala";

            v.VerifiableEntry.ForMember(x => x.Name, c => c.Must(NameMustBeBalala2).WithMessage("MustBala!"));

            var r = v.Verify();

            Assert.True(r.IsValid);

            v["Name"] = "Bong";

            r = v.Verify();

            Assert.False(r.IsValid);
            Assert.Single(r.Errors);
            Assert.Single(r.Errors[0].Details);
            Assert.Equal("MustBala!", r.Errors[0].Details[0].ErrorMessage);
        }

        public bool NameMustBeBalala(object value)
        {
            if (value is string stringVal)
            {
                return stringVal == "Balala";
            }

            return false;
        }

        public bool NameMustBeBalala2(string value)
        {
            return value == "Balala";
        }
    }
}