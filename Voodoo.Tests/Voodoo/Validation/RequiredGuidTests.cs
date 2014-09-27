﻿using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Voodoo.Validation;

namespace Voodoo.Tests.Voodoo.Validation
{
    [TestClass]
    public class RequiredGuidTests
    {
        [TestMethod]
        public void IsValid_NullableValueIsNull_ReturnsFalse()
        {
            var item = new ClassWithNullableGuid();
            var isvalid = item.Validate();
            Assert.AreEqual(false, isvalid);
        }

        [TestMethod]
        public void IsValid_StringValueIsNull_ReturnsFalse()
        {
            var item = new ClassWithGuidAsString();
            var isvalid = item.Validate();
            Assert.AreEqual(false, isvalid);
        }

        [TestMethod]
        public void IsValid_StringValueIsEmptyGuid_ReturnsFalse()
        {
            var item = new ClassWithGuidAsString();
            item.Guid = Guid.Empty.ToString();
            var isvalid = item.Validate();
            Assert.AreEqual(false, isvalid);
        }

        [TestMethod]
        public void IsValid_NullableValueIsEmptyGuid_ReturnsFalse()
        {
            var item = new ClassWithNullableGuid();
            item.Guid = Guid.Empty;
            var isvalid = item.Validate();
            Assert.AreEqual(false, isvalid);
        }

        [TestMethod]
        public void IsValid_ValueIsGuid_ReturnsTrue()
        {
            var item = new ClassWithGuid();
            item.Guid = Guid.NewGuid();
            var isvalid = item.Validate();
            Assert.AreEqual(true, isvalid);
        }

        [TestMethod]
        public void IsValid_NullableValueIsGuid_ReturnsTrue()
        {
            var item = new ClassWithNullableGuid();
            item.Guid = Guid.NewGuid();
            var isvalid = item.Validate();
            Assert.AreEqual(true, isvalid);
        }

        [TestMethod]
        public void IsValid_StringValueIsGuid_ReturnsTrue()
        {
            var item = new ClassWithGuidAsString();
            item.Guid = Guid.NewGuid().ToString();
            var isvalid = item.Validate();
            Assert.AreEqual(true, isvalid);
        }
    }
}