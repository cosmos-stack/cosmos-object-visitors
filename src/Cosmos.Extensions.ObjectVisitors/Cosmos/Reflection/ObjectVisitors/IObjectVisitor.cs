﻿using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using Cosmos.Reflection.ObjectVisitors.Correctness;
using Cosmos.Reflection.ObjectVisitors.Metadata;
using Cosmos.Validation;

namespace Cosmos.Reflection.ObjectVisitors
{
    public interface IObjectVisitor
    {
        Type SourceType { get; }

        bool IsStatic { get; }

        AlgorithmKind AlgorithmKind { get; }

        object Instance { get; }

        IValidationEntry VerifiableEntry { get; }

        VerifyResult Verify();

        VerifyResult Verify(string globalVerifyProviderName);

        void VerifyAndThrow();

        void VerifyAndThrow(string globalVerifyProviderName);

        void SetValue(string memberName, object value, AccessMode mode = AccessMode.Concise);

        void SetValue(string memberName, object value, string globalVerifyProviderName, AccessMode mode = AccessMode.Concise);

        void SetValue<TObj>(Expression<Func<TObj, object>> expression, object value);

        void SetValue<TObj>(Expression<Func<TObj, object>> expression, object value, string globalVerifyProviderName);

        void SetValue<TObj, TValue>(Expression<Func<TObj, TValue>> expression, TValue value);

        void SetValue<TObj, TValue>(Expression<Func<TObj, TValue>> expression, TValue value, string globalVerifyProviderName);

        void SetValue(IDictionary<string, object> keyValueCollection);

        void SetValue(IDictionary<string, object> keyValueCollection, string globalVerifyProviderName);

        object GetValue(string memberName, AccessMode mode = AccessMode.Concise);

        TValue GetValue<TValue>(string memberName, AccessMode mode = AccessMode.Concise);

        object GetValue<TObj>(Expression<Func<TObj, object>> expression);

        TValue GetValue<TObj, TValue>(Expression<Func<TObj, TValue>> expression);

        object this[string memberName] { get; set; }
        
        object this[string memberName, AccessMode mode] { get; set; }

        IEnumerable<string> GetMemberNames();

        ObjectMember GetMember(string memberName);

        bool Contains(string memberName);

        IPropertyValueAccessor ToValueAccessor();
    }

    public interface IObjectVisitor<T> : IObjectVisitor
    {
        new T Instance { get; }

        new IValidationEntry<T> VerifiableEntry { get; }

        void SetValue(Expression<Func<T, object>> expression, object value);

        void SetValue(Expression<Func<T, object>> expression, object value, string validationWithGlobalRules);

        void SetValue<TValue>(Expression<Func<T, TValue>> expression, TValue value);

        void SetValue<TValue>(Expression<Func<T, TValue>> expression, TValue value, string globalVerifyProviderName);

        object GetValue(Expression<Func<T, object>> expression);

        TValue GetValue<TValue>(Expression<Func<T, TValue>> expression);

        ObjectMember GetMember<TValue>(Expression<Func<T, TValue>> expression);
    }
}