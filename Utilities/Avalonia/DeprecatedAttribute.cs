// <copyright file="DeprecatedAttribute.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>
//
// Copyright (c) 2023-2023 Peter Kirmeier

#if !WINDOWS
namespace Windows.Foundation.Metadata
{
    using System;

    internal enum DeprecationType
    {
        Deprecate,
        Remove,
    }

    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct | AttributeTargets.Enum | AttributeTargets.Method | AttributeTargets.Property | AttributeTargets.Field | AttributeTargets.Event | AttributeTargets.Interface | AttributeTargets.Delegate, AllowMultiple = true)]
    internal sealed class DeprecatedAttribute : Attribute
    {
        internal DeprecatedAttribute(string message, DeprecationType type, uint version)
        {
        }
    }
}
#endif
