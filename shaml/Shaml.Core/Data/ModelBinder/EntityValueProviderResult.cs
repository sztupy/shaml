﻿using System.Web.Mvc;
using System.Globalization;
using System;
using Shaml.Core;
using System.Linq;
using System.ComponentModel;
using Shaml.Core.PersistenceSupport;
using System.Reflection;
using Shaml.Core.DomainModel;

namespace Shaml.Web.ModelBinder
{
    internal class EntityValueProviderResult : ValueProviderResult
    {
        public EntityValueProviderResult(ValueProviderResult result, Type propertyType)
            : this(result.RawValue, result.AttemptedValue, result.Culture, propertyType) {
        }

        public EntityValueProviderResult(object rawValue, string attemptedValue, CultureInfo culture, Type propertyType)
            : base(rawValue, attemptedValue, culture) {
            Check.Require(propertyType != null, "propertyType may not be null");

            this.propertyType = propertyType;
        }

        public override object ConvertTo(Type type, CultureInfo culture) {
            Check.Require(type != null, "type may not be null");
            Check.Require(RawValue as string[] != null && (RawValue as string[]).Length == 1,
                "The EntityValueProviderResult can only work with a RawValue of type string[1]; the RawValue was " +
                (RawValue != null ? RawValue.ToString() : "null"));

            Type entityInterfaceType = propertyType.GetInterfaces()
                .First(interfaceType => interfaceType.IsGenericType
                    && interfaceType.GetGenericTypeDefinition() == typeof(IEntityWithTypedId<>));

            Type idType = entityInterfaceType.GetGenericArguments().First();
            string rawId = (RawValue as string[]).First();

            if (string.IsNullOrEmpty(rawId))
                return null;

            object typedId = Convert.ChangeType(rawId, idType);

            object entity = GetEntityFor(typedId, idType);

            return entity;
        }

        private object GetEntityFor(object typedId, Type idType) {
            object entityRepository = GenericRepositoryFactory.CreateEntityRepositoryFor(propertyType, idType);

            return entityRepository.GetType()
                .InvokeMember("Get", BindingFlags.InvokeMethod, null, entityRepository, new[] { typedId });
        }

        private Type propertyType;
    }
}
