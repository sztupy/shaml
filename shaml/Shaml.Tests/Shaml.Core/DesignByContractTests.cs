﻿using NUnit.Framework;
using Shaml.Core;
using System;

namespace Tests.Shaml.Core
{
    [TestFixture]
    public class DesignByContractTests
    {
        [Test]
        public void CanGetPastPreconditionAndPostCondition() {
            Assert.AreEqual("20%", ConvertToPercentage(.2m));
        }

        [Test]
        public void CanEnforcePrecondition() {
            Assert.Throws<PreconditionException>(
                () => ConvertToPercentage(-.2m)
            );
        }

        [Test]
        public void CanEnforcePostcondition() {
            Assert.Throws<PostconditionException>(
                () => ConvertToPercentage(2m)
            );
        }

        private string ConvertToPercentage(decimal fractionalPercentage) {
            Check.Require(fractionalPercentage > 0,
                "fractionalPercentage must be > 0");

            decimal convertedValue = fractionalPercentage * 100;

            // Yes, I could have enforced this outcome in the precondition, but then you
            // wouldn't have seen the Check.Ensure in action, now would you?
            Check.Ensure(convertedValue >= 0 && convertedValue <= 100,
                "convertedValue was expected to be within 0-100, but was " + convertedValue);

            return Math.Round(convertedValue) + "%";
        }
    }
}
