/*
The MIT License (MIT)

Copyright (c) 2016-2019 Maksim Volkau

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included AddOrUpdateServiceFactory
all copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
THE SOFTWARE.
*/

// ReSharper disable CoVariantArrayConversion

#if LIGHT_EXPRESSION
namespace FastExpressionCompiler.LightExpression
#else
namespace FastExpressionCompiler
#endif
{
    using System;
    using System.Collections.Generic;
    using System.Linq.Expressions;
    using System.Reflection;
    using System.Reflection.Emit;

    /// <summary>Compiles expression to delegate ~20 times faster than Expression.Compile.
    /// Partial to extend with your things when used as source file.</summary>
    // ReSharper disable once PartialTypeWithSinglePart
    public static partial class ExpressionCompiler
    {
        #region Expression.CompileFast overloads for Delegate, Func, and Action

        /// <summary>Compiles lambda expression to TDelegate type. Use ifFastFailedReturnNull parameter to Not fallback to Expression.Compile, useful for testing.</summary>
        public static TDelegate CompileFast<TDelegate>(this LambdaExpression lambdaExpr,
            bool ifFastFailedReturnNull = false) where TDelegate : class
        {
            var closureInfo = new ClosureInfo(false);
            return (TDelegate)TryCompile(ref closureInfo,
                       typeof(TDelegate), Tools.GetParamTypes(lambdaExpr.Parameters),
                       lambdaExpr.ReturnType, lambdaExpr.Body, lambdaExpr.Parameters)
                ?? (ifFastFailedReturnNull ? null : (TDelegate)(object)lambdaExpr.CompileSys());
        }

        /// <summary>Compiles lambda expression to delegate. Use ifFastFailedReturnNull parameter to Not fallback to Expression.Compile, useful for testing.</summary>
        public static Delegate CompileFast(this LambdaExpression lambdaExpr, bool ifFastFailedReturnNull = false)
        {
            var closureInfo = new ClosureInfo(false);
            return (Delegate)TryCompile(ref closureInfo,
                lambdaExpr.Type, Tools.GetParamTypes(lambdaExpr.Parameters),
                       lambdaExpr.ReturnType, lambdaExpr.Body, lambdaExpr.Parameters)
                ?? (ifFastFailedReturnNull ? null : lambdaExpr.CompileSys());
        }

        /// <summary>Unifies Compile for System.Linq.Expressions and FEC.LightExpression</summary>
        public static TDelegate CompileSys<TDelegate>(this Expression<TDelegate> lambdaExpr) where TDelegate : class =>
            lambdaExpr
#if LIGHT_EXPRESSION
            .ToLambdaExpression()
#endif
            .Compile();

        /// <summary>Unifies Compile for System.Linq.Expressions and FEC.LightExpression</summary>
        public static Delegate CompileSys(this LambdaExpression lambdaExpr) =>
            lambdaExpr
#if LIGHT_EXPRESSION
            .ToLambdaExpression()
#endif
            .Compile();

        /// <summary>Compiles lambda expression to TDelegate type. Use ifFastFailedReturnNull parameter to Not fallback to Expression.Compile, useful for testing.</summary>
        public static TDelegate CompileFast<TDelegate>(this Expression<TDelegate> lambdaExpr,
            bool ifFastFailedReturnNull = false)
            where TDelegate : class => ((LambdaExpression)lambdaExpr).CompileFast<TDelegate>(ifFastFailedReturnNull);

        /// <summary>Compiles lambda expression to delegate. Use ifFastFailedReturnNull parameter to Not fallback to Expression.Compile, useful for testing.</summary>
        public static Func<R> CompileFast<R>(this Expression<Func<R>> lambdaExpr,
            bool ifFastFailedReturnNull = false) =>
            TryCompile<Func<R>>(lambdaExpr.Body, lambdaExpr.Parameters, Tools.Empty<Type>(), typeof(R))
            ?? (ifFastFailedReturnNull ? null : lambdaExpr.CompileSys());

        /// <summary>Compiles lambda expression to delegate. Use ifFastFailedReturnNull parameter to Not fallback to Expression.Compile, useful for testing.</summary>
        public static Func<T1, R> CompileFast<T1, R>(this Expression<Func<T1, R>> lambdaExpr,
            bool ifFastFailedReturnNull = false) =>
            TryCompile<Func<T1, R>>(lambdaExpr.Body, lambdaExpr.Parameters, new[] { typeof(T1) }, typeof(R))
            ?? (ifFastFailedReturnNull ? null : lambdaExpr.CompileSys());

        /// <summary>Compiles lambda expression to TDelegate type. Use ifFastFailedReturnNull parameter to Not fallback to Expression.Compile, useful for testing.</summary>
        public static Func<T1, T2, R> CompileFast<T1, T2, R>(this Expression<Func<T1, T2, R>> lambdaExpr,
            bool ifFastFailedReturnNull = false) =>
            TryCompile<Func<T1, T2, R>>(lambdaExpr.Body, lambdaExpr.Parameters, new[] { typeof(T1), typeof(T2) },
                typeof(R))
            ?? (ifFastFailedReturnNull ? null : lambdaExpr.CompileSys());

        /// <summary>Compiles lambda expression to delegate. Use ifFastFailedReturnNull parameter to Not fallback to Expression.Compile, useful for testing.</summary>
        public static Func<T1, T2, T3, R> CompileFast<T1, T2, T3, R>(
            this Expression<Func<T1, T2, T3, R>> lambdaExpr, bool ifFastFailedReturnNull = false) =>
            TryCompile<Func<T1, T2, T3, R>>(lambdaExpr.Body, lambdaExpr.Parameters,
                new[] { typeof(T1), typeof(T2), typeof(T3) }, typeof(R))
            ?? (ifFastFailedReturnNull ? null : lambdaExpr.CompileSys());

        /// <summary>Compiles lambda expression to TDelegate type. Use ifFastFailedReturnNull parameter to Not fallback to Expression.Compile, useful for testing.</summary>
        public static Func<T1, T2, T3, T4, R> CompileFast<T1, T2, T3, T4, R>(
            this Expression<Func<T1, T2, T3, T4, R>> lambdaExpr, bool ifFastFailedReturnNull = false) =>
            TryCompile<Func<T1, T2, T3, T4, R>>(lambdaExpr.Body, lambdaExpr.Parameters,
                new[] { typeof(T1), typeof(T2), typeof(T3), typeof(T4) }, typeof(R))
            ?? (ifFastFailedReturnNull ? null : lambdaExpr.CompileSys());

        /// <summary>Compiles lambda expression to delegate. Use ifFastFailedReturnNull parameter to Not fallback to Expression.Compile, useful for testing.</summary>
        public static Func<T1, T2, T3, T4, T5, R> CompileFast<T1, T2, T3, T4, T5, R>(
            this Expression<Func<T1, T2, T3, T4, T5, R>> lambdaExpr, bool ifFastFailedReturnNull = false) =>
            TryCompile<Func<T1, T2, T3, T4, T5, R>>(lambdaExpr.Body, lambdaExpr.Parameters,
                new[] { typeof(T1), typeof(T2), typeof(T3), typeof(T4), typeof(T5) }, typeof(R))
            ?? (ifFastFailedReturnNull ? null : lambdaExpr.CompileSys());

        /// <summary>Compiles lambda expression to delegate. Use ifFastFailedReturnNull parameter to Not fallback to Expression.Compile, useful for testing.</summary>
        public static Func<T1, T2, T3, T4, T5, T6, R> CompileFast<T1, T2, T3, T4, T5, T6, R>(
            this Expression<Func<T1, T2, T3, T4, T5, T6, R>> lambdaExpr, bool ifFastFailedReturnNull = false) =>
            TryCompile<Func<T1, T2, T3, T4, T5, T6, R>>(lambdaExpr.Body, lambdaExpr.Parameters,
                new[] { typeof(T1), typeof(T2), typeof(T3), typeof(T4), typeof(T5), typeof(T6) }, typeof(R))
            ?? (ifFastFailedReturnNull ? null : lambdaExpr.CompileSys());

        /// <summary>Compiles lambda expression to delegate. Use ifFastFailedReturnNull parameter to Not fallback to Expression.Compile, useful for testing.</summary>
        public static Action CompileFast(this Expression<Action> lambdaExpr, bool ifFastFailedReturnNull = false) =>
            TryCompile<Action>(lambdaExpr.Body, lambdaExpr.Parameters, Tools.Empty<Type>(), typeof(void))
            ?? (ifFastFailedReturnNull ? null : lambdaExpr.CompileSys());

        /// <summary>Compiles lambda expression to delegate. Use ifFastFailedReturnNull parameter to Not fallback to Expression.Compile, useful for testing.</summary>
        public static Action<T1> CompileFast<T1>(this Expression<Action<T1>> lambdaExpr,
            bool ifFastFailedReturnNull = false) =>
            TryCompile<Action<T1>>(lambdaExpr.Body, lambdaExpr.Parameters, new[] { typeof(T1) }, typeof(void))
            ?? (ifFastFailedReturnNull ? null : lambdaExpr.CompileSys());

        /// <summary>Compiles lambda expression to delegate. Use ifFastFailedReturnNull parameter to Not fallback to Expression.Compile, useful for testing.</summary>
        public static Action<T1, T2> CompileFast<T1, T2>(this Expression<Action<T1, T2>> lambdaExpr,
            bool ifFastFailedReturnNull = false) =>
            TryCompile<Action<T1, T2>>(lambdaExpr.Body, lambdaExpr.Parameters, new[] { typeof(T1), typeof(T2) },
                typeof(void))
            ?? (ifFastFailedReturnNull ? null : lambdaExpr.CompileSys());

        /// <summary>Compiles lambda expression to delegate. Use ifFastFailedReturnNull parameter to Not fallback to Expression.Compile, useful for testing.</summary>
        public static Action<T1, T2, T3> CompileFast<T1, T2, T3>(this Expression<Action<T1, T2, T3>> lambdaExpr,
            bool ifFastFailedReturnNull = false) =>
            TryCompile<Action<T1, T2, T3>>(lambdaExpr.Body, lambdaExpr.Parameters,
                new[] { typeof(T1), typeof(T2), typeof(T3) }, typeof(void))
            ?? (ifFastFailedReturnNull ? null : lambdaExpr.CompileSys());

        /// <summary>Compiles lambda expression to delegate. Use ifFastFailedReturnNull parameter to Not fallback to Expression.Compile, useful for testing.</summary>
        public static Action<T1, T2, T3, T4> CompileFast<T1, T2, T3, T4>(
            this Expression<Action<T1, T2, T3, T4>> lambdaExpr, bool ifFastFailedReturnNull = false) =>
            TryCompile<Action<T1, T2, T3, T4>>(lambdaExpr.Body, lambdaExpr.Parameters,
                new[] { typeof(T1), typeof(T2), typeof(T3), typeof(T4) }, typeof(void))
            ?? (ifFastFailedReturnNull ? null : lambdaExpr.CompileSys());

        /// <summary>Compiles lambda expression to delegate. Use ifFastFailedReturnNull parameter to Not fallback to Expression.Compile, useful for testing.</summary>
        public static Action<T1, T2, T3, T4, T5> CompileFast<T1, T2, T3, T4, T5>(
            this Expression<Action<T1, T2, T3, T4, T5>> lambdaExpr, bool ifFastFailedReturnNull = false) =>
            TryCompile<Action<T1, T2, T3, T4, T5>>(lambdaExpr.Body, lambdaExpr.Parameters,
                new[] { typeof(T1), typeof(T2), typeof(T3), typeof(T4), typeof(T5) }, typeof(void))
            ?? (ifFastFailedReturnNull ? null : lambdaExpr.CompileSys());

        /// <summary>Compiles lambda expression to delegate. Use ifFastFailedReturnNull parameter to Not fallback to Expression.Compile, useful for testing.</summary>
        public static Action<T1, T2, T3, T4, T5, T6> CompileFast<T1, T2, T3, T4, T5, T6>(
            this Expression<Action<T1, T2, T3, T4, T5, T6>> lambdaExpr, bool ifFastFailedReturnNull = false) =>
            TryCompile<Action<T1, T2, T3, T4, T5, T6>>(lambdaExpr.Body, lambdaExpr.Parameters,
                new[] { typeof(T1), typeof(T2), typeof(T3), typeof(T4), typeof(T5), typeof(T6) }, typeof(void))
            ?? (ifFastFailedReturnNull ? null : lambdaExpr.CompileSys());

        #endregion

        /// <summary>Tries to compile lambda expression to <typeparamref name="TDelegate"/></summary>
        public static TDelegate TryCompile<TDelegate>(this LambdaExpression lambdaExpr) where TDelegate : class =>
            TryCompile<TDelegate>(lambdaExpr.Body, lambdaExpr.Parameters, Tools.GetParamTypes(lambdaExpr.Parameters),
                lambdaExpr.ReturnType);

        /// <summary>Tries to compile lambda expression to <typeparamref name="TDelegate"/> 
        /// with the provided closure object and constant expressions (or lack there of) -
        /// Constant expression should be the in order of Fields in closure object!
        /// Note 1: Use it on your own risk - FEC won't verify the expression is compile-able with passed closure, it is up to you!
        /// Note 2: The expression with NESTED LAMBDA IS NOT SUPPORTED!
        /// Note 3: `Label` and `GoTo` are not supported in this case, because they need first round to collect out-of-order labels</summary>
        public static TDelegate TryCompileWithPreCreatedClosure<TDelegate>(this LambdaExpression lambdaExpr,
            object closure, params ConstantExpression[] closureConstantsExprs)
            where TDelegate : class
        {
            var closureInfo = new ClosureInfo(true, closure, closureConstantsExprs);
            return (TDelegate)TryCompile(ref closureInfo, typeof(TDelegate), Tools.GetParamTypes(lambdaExpr.Parameters),
                lambdaExpr.ReturnType, lambdaExpr.Body, lambdaExpr.Parameters);
        }

        /// <summary>Tries to compile expression to "static" delegate, skipping the step of collecting the closure object.</summary>
        public static TDelegate TryCompileWithoutClosure<TDelegate>(this LambdaExpression lambdaExpr)
            where TDelegate : class => lambdaExpr.TryCompileWithPreCreatedClosure<TDelegate>(null, null);

        /// <summary>Compiles expression to delegate by emitting the IL. 
        /// If sub-expressions are not supported by emitter, then the method returns null.
        /// The usage should be calling the method, if result is null then calling the Expression.Compile.</summary>
        public static TDelegate TryCompile<TDelegate>(
            Expression bodyExpr, IReadOnlyList<ParameterExpression> paramExprs, Type[] paramTypes, Type returnType)
            where TDelegate : class
        {
            var closureInfo = new ClosureInfo(false);
            return (TDelegate)TryCompile(ref closureInfo, typeof(TDelegate), paramTypes, returnType, bodyExpr, paramExprs);
        }

        private static object TryCompile(ref ClosureInfo closureInfo,
            Type delegateType, Type[] paramTypes, Type returnType, Expression expr,
            IReadOnlyList<ParameterExpression> paramExprs, bool isNestedLambda = false)
        {
            object closureObject;
            if (!closureInfo.IsClosureConstructed)
            {
                if (!TryCollectBoundConstants(ref closureInfo, expr, paramExprs))
                    return null;

                var nestedLambdaExprs = closureInfo.NestedLambdaExprs;
                if (nestedLambdaExprs.Length != 0)
                {
                    closureInfo.NestedLambdas = new NestedLambdaInfo[nestedLambdaExprs.Length];
                    for (var i = 0; i < nestedLambdaExprs.Length; ++i)
                        if (!TryCompileNestedLambda(ref closureInfo, i, nestedLambdaExprs[i]))
                            return null;
                }

                closureObject = closureInfo.ConstructClosureTypeAndObject(constructTypeOnly: isNestedLambda);
            }
            else
            {
                closureObject = closureInfo.Closure;
            }

            var closureType = closureInfo.ClosureType;
            var methodParamTypes = closureType == null ? paramTypes : GetClosureAndParamTypes(paramTypes, closureType);

            var method = new DynamicMethod(string.Empty, returnType, methodParamTypes,
                typeof(ExpressionCompiler), skipVisibility: true);

            var il = method.GetILGenerator();
            var parentFlags = returnType == typeof(void) ? ParentFlags.IgnoreResult : ParentFlags.Empty;
            if (!EmittingVisitor.TryEmit(expr, paramExprs, il, ref closureInfo, parentFlags))
                return null;

            il.Emit(OpCodes.Ret);

            // Include the closure as the first parameter, BUT don't bound to it. It will be bound later in EmitNestedLambda.
            if (isNestedLambda)
                delegateType = Tools.GetFuncOrActionType(methodParamTypes, returnType);
            // Create a specific delegate if user requested delegate is untyped, otherwise CreateMethod will fail
            else if (delegateType == typeof(Delegate))
                delegateType = Tools.GetFuncOrActionType(paramTypes, returnType);

            return method.CreateDelegate(delegateType, closureObject);
        }

        private static void CopyNestedClosureInfo(IReadOnlyList<ParameterExpression> lambdaParamExprs,
            ref ClosureInfo info, ref ClosureInfo nestedInfo)
        {
            // if nested non passed parameter is no matched with any outer passed parameter, 
            // then ensure it goes to outer non passed parameter.
            // But check that having a non-passed parameter in root expression is invalid.
            var nestedNonPassedParams = nestedInfo.NonPassedParameters;
            if (nestedNonPassedParams.Length != 0)
                for (var i = 0; i < nestedNonPassedParams.Length; i++)
                {
                    var nestedNonPassedParam = nestedNonPassedParams[i];
                    if (lambdaParamExprs.GetFirstIndex(nestedNonPassedParam) == -1)
                        info.AddNonPassedParam(nestedNonPassedParam);
                }

            // Promote found constants and nested lambdas into outer closure
            var nestedConstants = nestedInfo.Constants;
            if (nestedConstants.Length != 0)
                for (var i = 0; i < nestedConstants.Length; i++)
                    info.AddConstant(nestedConstants[i]);

            // Add nested constants to outer lambda closure.
            // At this moment we know that the NestedLambdaExprs are non-empty, cause we are doing this from the nested lambda already.
            var nestedNestedLambdaExprs = nestedInfo.NestedLambdaExprs;
            if (nestedNestedLambdaExprs.Length != 0)
            {
                var fixedNestedLambdaCount = info.NestedLambdaExprs.Length;
                for (var i = 0; i < nestedNestedLambdaExprs.Length; i++)
                {
                    var nestedNestedLambdaExpr = nestedNestedLambdaExprs[i];

                    var j = info.NestedLambdaExprs.Length - 1;
                    for (; j >= fixedNestedLambdaCount; --j)
                        if (ReferenceEquals(info.NestedLambdaExprs[j], nestedNestedLambdaExpr))
                            break;

                    if (j < fixedNestedLambdaCount)
                    {
                        info.NestedLambdaExprs = info.NestedLambdaExprs.WithLast(nestedNestedLambdaExpr);
                        info.NestedLambdas = info.NestedLambdas.WithLast(nestedInfo.NestedLambdas[i]);
                    }
                }
            }
        }

        private static Type[] GetClosureAndParamTypes(Type[] paramTypes, Type closureType)
        {
            var paramCount = paramTypes.Length;
            if (paramCount == 0)
                return new[] { closureType };

            if (paramCount == 1)
                return new[] { closureType, paramTypes[0] };

            if (paramCount == 2)
                return new[] { closureType, paramTypes[0], paramTypes[1] };

            var closureAndParamTypes = new Type[paramCount + 1];
            closureAndParamTypes[0] = closureType;
            Array.Copy(paramTypes, 0, closureAndParamTypes, 1, paramCount);
            return closureAndParamTypes;
        }

        private sealed class BlockInfo
        {
            public static readonly BlockInfo Empty = new BlockInfo();
            private BlockInfo() { }

            public bool IsEmpty => Parent == null;
            public readonly BlockInfo Parent;
            public readonly IReadOnlyList<ParameterExpression> VarExprs;
            public readonly LocalBuilder[] LocalVars;

            internal BlockInfo(BlockInfo parent, IReadOnlyList<ParameterExpression> varExprs, LocalBuilder[] localVars)
            {
                Parent = parent;
                VarExprs = varExprs;
                LocalVars = localVars;
            }
        }

        // Track the info required to build a closure object + some context information not directly related to closure.
        private struct ClosureInfo
        {
            public bool IsClosureConstructed;

            // Constructed closure object.
            public readonly object Closure;

            // Type of constructed closure, may be available even without closure object (in case of nested lambda)
            public Type ClosureType;
            public bool HasClosure => ClosureType != null;

            public bool LastEmitIsAddress;

            // Constant expressions to find an index (by reference) of constant expression from compiled expression.
            public ConstantExpression[] Constants;

            // Parameters not passed through lambda parameter list But used inside lambda body.
            // The top expression should not! contain non passed parameters. 
            public ParameterExpression[] NonPassedParameters;

            // All nested lambdas recursively nested in expression
            public NestedLambdaInfo[] NestedLambdas;
            public LambdaExpression[] NestedLambdaExprs;

            public int ClosedItemCount => Constants.Length + NonPassedParameters.Length + NestedLambdas.Length;

            // FieldInfos are needed to load field of closure object on stack in emitter.
            // It is also an indicator that we use typed Closure object and not an array.
            public FieldInfo[] ClosureFields;

            // Helper to know if a Return GotoExpression's Label should be emitted
            private int _tryCatchFinallyReturnLabelIndex;

            // Helper to decide whether we are inside the block or not
            private BlockInfo _currentBlock;

            // Dictionary for the used Labels in IL
            private KeyValuePair<LabelTarget, Label?>[] _labels;

            // Populates info directly with provided closure object and constants.
            public ClosureInfo(bool isConstructed, object closure = null,
                ConstantExpression[] closureConstantExpressions = null)
            {
                IsClosureConstructed = isConstructed;

                NonPassedParameters = Tools.Empty<ParameterExpression>();
                NestedLambdas = Tools.Empty<NestedLambdaInfo>();
                NestedLambdaExprs = Tools.Empty<LambdaExpression>();
                _tryCatchFinallyReturnLabelIndex = int.MinValue;
                _currentBlock = BlockInfo.Empty;
                _labels = null;
                LastEmitIsAddress = false;

                if (closure == null)
                {
                    Closure = null;
                    Constants = Tools.Empty<ConstantExpression>();
                    ClosureType = null;
                    ClosureFields = null;
                }
                else
                {
                    Closure = closure;
                    Constants = closureConstantExpressions ?? Tools.Empty<ConstantExpression>();
                    ClosureType = closure.GetType();
                    // todo: verify that Fields types are correspond to `closureConstantExpressions`
                    ClosureFields = ClosureType.GetTypeInfo().DeclaredFields.AsArray();
                }
            }

            public void AddConstant(ConstantExpression expr)
            {
                if (Constants.Length == 0 ||
                    Constants.GetFirstIndex(expr) == -1)
                    Constants = Constants.WithLast(expr);
            }

            public void AddNonPassedParam(ParameterExpression expr)
            {
                if (NonPassedParameters.Length == 0 ||
                    NonPassedParameters.GetFirstIndex(expr) == -1)
                    NonPassedParameters = NonPassedParameters.WithLast(expr);
            }

            public void AddNestedLambda(LambdaExpression lambdaExpr)
            {
                if (NestedLambdaExprs.Length == 0 ||
                    NestedLambdaExprs.GetFirstIndex(lambdaExpr) == -1)
                    NestedLambdaExprs = NestedLambdaExprs.WithLast(lambdaExpr);
            }

            public void AddLabel(LabelTarget labelTarget)
            {
                if (labelTarget != null &&
                    GetLabelIndex(labelTarget) == -1)
                    _labels = _labels.WithLast(new KeyValuePair<LabelTarget, Label?>(labelTarget, null));
            }

            public Label GetOrCreateLabel(LabelTarget labelTarget, ILGenerator il) =>
                GetOrCreateLabel(GetLabelIndex(labelTarget), il);

            public Label GetOrCreateLabel(int index, ILGenerator il)
            {
                var labelPair = _labels[index];
                var label = labelPair.Value;
                if (!label.HasValue)
                    _labels[index] = new KeyValuePair<LabelTarget, Label?>(labelPair.Key, label = il.DefineLabel());
                return label.Value;
            }

            public int GetLabelIndex(LabelTarget labelTarget)
            {
                if (_labels != null)
                    for (var i = 0; i < _labels.Length; ++i)
                        if (_labels[i].Key == labelTarget)
                            return i;
                return -1;
            }

            public void MarkLabelAsTryReturn(int index) => _tryCatchFinallyReturnLabelIndex = index;

            public object ConstructClosureTypeAndObject(bool constructTypeOnly)
            {
                IsClosureConstructed = true;

                var constants = Constants;
                var nonPassedParams = NonPassedParameters;
                var nestedLambdas = NestedLambdas;
                if (constants.Length == 0 && nonPassedParams.Length == 0 && nestedLambdas.Length == 0)
                    return null;

                var constPlusParamCount = constants.Length + nonPassedParams.Length;
                var totalItemCount = constPlusParamCount + nestedLambdas.Length;

                // Construct the array based closure when number of values is bigger than
                // number of fields in biggest supported Closure class.
                var createMethods = ExpressionCompiler.Closure.CreateMethods;
                if (totalItemCount > createMethods.Length)
                {
                    ClosureType = typeof(ArrayClosure);
                    if (constructTypeOnly)
                        return null;

                    var items = new object[totalItemCount];
                    if (constants.Length != 0)
                        for (var i = 0; i < constants.Length; i++)
                            items[i] = constants[i].Value;

                    // skip non passed parameters as it is only for nested lambdas

                    if (nestedLambdas.Length != 0)
                        for (var i = 0; i < nestedLambdas.Length; i++)
                            items[constPlusParamCount + i] = nestedLambdas[i].Lambda;

                    return new ArrayClosure(items);
                }

                // Construct the Closure Type and optionally Closure object with closed values stored as fields:
                object[] fieldValues = null;
                var fieldTypes = new Type[totalItemCount];
                if (constructTypeOnly)
                {
                    if (constants.Length != 0)
                        for (var i = 0; i < constants.Length; i++)
                            fieldTypes[i] = constants[i].Type;

                    if (nonPassedParams.Length != 0)
                        for (var i = 0; i < nonPassedParams.Length; i++)
                            fieldTypes[constants.Length + i] = nonPassedParams[i].Type;

                    if (nestedLambdas.Length != 0)
                        for (var i = 0; i < nestedLambdas.Length; i++)
                            fieldTypes[constPlusParamCount + i] =
                                nestedLambdas[i].Lambda.GetType(); // compiled lambda type
                }
                else
                {
                    fieldValues = new object[totalItemCount];

                    if (constants.Length != 0)
                        for (var i = 0; i < constants.Length; i++)
                        {
                            var constantExpr = constants[i];
                            if (constantExpr != null)
                            {
                                fieldTypes[i] = constantExpr.Type;
                                fieldValues[i] = constantExpr.Value;
                            }
                        }

                    if (nonPassedParams.Length != 0)
                        for (var i = 0; i < nonPassedParams.Length; i++)
                            fieldTypes[constants.Length + i] = nonPassedParams[i].Type;

                    if (nestedLambdas.Length != 0)
                        for (var i = 0; i < nestedLambdas.Length; i++)
                        {
                            var lambda = nestedLambdas[i].Lambda;
                            fieldValues[constPlusParamCount + i] = lambda;
                            fieldTypes[constPlusParamCount + i] = lambda.GetType();
                        }
                }

                var createClosure = createMethods[totalItemCount - 1].MakeGenericMethod(fieldTypes);
                ClosureType = createClosure.ReturnType;
                ClosureFields = ClosureType.GetTypeInfo().DeclaredFields.AsArray();

                return constructTypeOnly ? null : createClosure.Invoke(null, fieldValues);
            }

            public void PushBlock(IReadOnlyList<ParameterExpression> blockVarExprs, LocalBuilder[] localVars) =>
                _currentBlock = new BlockInfo(_currentBlock, blockVarExprs, localVars);

            public void PushBlockAndConstructLocalVars(IReadOnlyList<ParameterExpression> blockVarExprs, ILGenerator il)
            {
                var localVars = Tools.Empty<LocalBuilder>();
                if (blockVarExprs.Count != 0)
                {
                    localVars = new LocalBuilder[blockVarExprs.Count];
                    for (var i = 0; i < localVars.Length; i++)
                        localVars[i] = il.DeclareLocal(blockVarExprs[i].Type);
                }

                PushBlock(blockVarExprs, localVars);
            }

            public void PopBlock() =>
                _currentBlock = _currentBlock.Parent;

            public bool IsLocalVar(object varParamExpr)
            {
                var i = -1;
                for (var block = _currentBlock; i == -1 && !block.IsEmpty; block = block.Parent)
                    i = block.VarExprs.GetFirstIndex(varParamExpr);
                return i != -1;
            }

            public bool IsTryReturnLabel(int index) => index == _tryCatchFinallyReturnLabelIndex;

            public LocalBuilder GetDefinedLocalVarOrDefault(ParameterExpression varParamExpr)
            {
                for (var block = _currentBlock; !block.IsEmpty; block = block.Parent)
                {
                    if (block.LocalVars.Length == 0)
                        continue;
                    var varIndex = block.VarExprs.GetFirstIndex(varParamExpr);
                    if (varIndex != -1)
                        return block.LocalVars[varIndex];
                }

                return null;
            }
        }

        #region Closures

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member

        public static class Closure
        {
            private static readonly IEnumerable<MethodInfo> _methods = typeof(Closure).GetTypeInfo().DeclaredMethods;
            internal static readonly MethodInfo[] CreateMethods = _methods.AsArray();

            public static Closure<T1> Create<T1>(T1 v1) => new Closure<T1>(v1);

            public static Closure<T1, T2> Create<T1, T2>(T1 v1, T2 v2) => new Closure<T1, T2>(v1, v2);

            public static Closure<T1, T2, T3> Create<T1, T2, T3>(T1 v1, T2 v2, T3 v3) =>
                new Closure<T1, T2, T3>(v1, v2, v3);

            public static Closure<T1, T2, T3, T4> Create<T1, T2, T3, T4>(T1 v1, T2 v2, T3 v3, T4 v4) =>
                new Closure<T1, T2, T3, T4>(v1, v2, v3, v4);

            public static Closure<T1, T2, T3, T4, T5> Create<T1, T2, T3, T4, T5>(T1 v1, T2 v2, T3 v3, T4 v4,
                T5 v5) => new Closure<T1, T2, T3, T4, T5>(v1, v2, v3, v4, v5);

            public static Closure<T1, T2, T3, T4, T5, T6> Create<T1, T2, T3, T4, T5, T6>(T1 v1, T2 v2, T3 v3,
                T4 v4, T5 v5, T6 v6) => new Closure<T1, T2, T3, T4, T5, T6>(v1, v2, v3, v4, v5, v6);

            public static Closure<T1, T2, T3, T4, T5, T6, T7> Create<T1, T2, T3, T4, T5, T6, T7>(T1 v1, T2 v2,
                T3 v3, T4 v4, T5 v5, T6 v6, T7 v7) =>
                new Closure<T1, T2, T3, T4, T5, T6, T7>(v1, v2, v3, v4, v5, v6, v7);

            public static Closure<T1, T2, T3, T4, T5, T6, T7, T8> Create<T1, T2, T3, T4, T5, T6, T7, T8>(
                T1 v1, T2 v2, T3 v3, T4 v4, T5 v5, T6 v6, T7 v7, T8 v8) =>
                new Closure<T1, T2, T3, T4, T5, T6, T7, T8>(v1, v2, v3, v4, v5, v6, v7, v8);

            public static Closure<T1, T2, T3, T4, T5, T6, T7, T8, T9> Create<T1, T2, T3, T4, T5, T6, T7, T8, T9>(
                T1 v1, T2 v2, T3 v3, T4 v4, T5 v5, T6 v6, T7 v7, T8 v8, T9 v9) =>
                new Closure<T1, T2, T3, T4, T5, T6, T7, T8, T9>(v1, v2, v3, v4, v5, v6, v7, v8, v9);

            public static Closure<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10> Create<T1, T2, T3, T4, T5, T6, T7, T8, T9,
                T10>(
                T1 v1, T2 v2, T3 v3, T4 v4, T5 v5, T6 v6, T7 v7, T8 v8, T9 v9, T10 v10) =>
                new Closure<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>(v1, v2, v3, v4, v5, v6, v7, v8, v9, v10);
        }

        public sealed class Closure<T1>
        {
            public T1 V1;

            public Closure(T1 v1)
            {
                V1 = v1;
            }
        }

        public sealed class Closure<T1, T2>
        {
            public T1 V1;
            public T2 V2;

            public Closure(T1 v1, T2 v2)
            {
                V1 = v1;
                V2 = v2;
            }
        }

        public sealed class Closure<T1, T2, T3>
        {
            public T1 V1;
            public T2 V2;
            public T3 V3;

            public Closure(T1 v1, T2 v2, T3 v3)
            {
                V1 = v1;
                V2 = v2;
                V3 = v3;
            }
        }

        public sealed class Closure<T1, T2, T3, T4>
        {
            public T1 V1;
            public T2 V2;
            public T3 V3;
            public T4 V4;

            public Closure(T1 v1, T2 v2, T3 v3, T4 v4)
            {
                V1 = v1;
                V2 = v2;
                V3 = v3;
                V4 = v4;
            }
        }

        public sealed class Closure<T1, T2, T3, T4, T5>
        {
            public T1 V1;
            public T2 V2;
            public T3 V3;
            public T4 V4;
            public T5 V5;

            public Closure(T1 v1, T2 v2, T3 v3, T4 v4, T5 v5)
            {
                V1 = v1;
                V2 = v2;
                V3 = v3;
                V4 = v4;
                V5 = v5;
            }
        }

        public sealed class Closure<T1, T2, T3, T4, T5, T6>
        {
            public T1 V1;
            public T2 V2;
            public T3 V3;
            public T4 V4;
            public T5 V5;
            public T6 V6;

            public Closure(T1 v1, T2 v2, T3 v3, T4 v4, T5 v5, T6 v6)
            {
                V1 = v1;
                V2 = v2;
                V3 = v3;
                V4 = v4;
                V5 = v5;
                V6 = v6;
            }
        }

        public sealed class Closure<T1, T2, T3, T4, T5, T6, T7>
        {
            public T1 V1;
            public T2 V2;
            public T3 V3;
            public T4 V4;
            public T5 V5;
            public T6 V6;
            public T7 V7;

            public Closure(T1 v1, T2 v2, T3 v3, T4 v4, T5 v5, T6 v6, T7 v7)
            {
                V1 = v1;
                V2 = v2;
                V3 = v3;
                V4 = v4;
                V5 = v5;
                V6 = v6;
                V7 = v7;
            }
        }

        public sealed class Closure<T1, T2, T3, T4, T5, T6, T7, T8>
        {
            public T1 V1;
            public T2 V2;
            public T3 V3;
            public T4 V4;
            public T5 V5;
            public T6 V6;
            public T7 V7;
            public T8 V8;

            public Closure(T1 v1, T2 v2, T3 v3, T4 v4, T5 v5, T6 v6, T7 v7, T8 v8)
            {
                V1 = v1;
                V2 = v2;
                V3 = v3;
                V4 = v4;
                V5 = v5;
                V6 = v6;
                V7 = v7;
                V8 = v8;
            }
        }

        public sealed class Closure<T1, T2, T3, T4, T5, T6, T7, T8, T9>
        {
            public T1 V1;
            public T2 V2;
            public T3 V3;
            public T4 V4;
            public T5 V5;
            public T6 V6;
            public T7 V7;
            public T8 V8;
            public T9 V9;

            public Closure(T1 v1, T2 v2, T3 v3, T4 v4, T5 v5, T6 v6, T7 v7, T8 v8, T9 v9)
            {
                V1 = v1;
                V2 = v2;
                V3 = v3;
                V4 = v4;
                V5 = v5;
                V6 = v6;
                V7 = v7;
                V8 = v8;
                V9 = v9;
            }
        }

        public sealed class Closure<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>
        {
            public T1 V1;
            public T2 V2;
            public T3 V3;
            public T4 V4;
            public T5 V5;
            public T6 V6;
            public T7 V7;
            public T8 V8;
            public T9 V9;
            public T10 V10;

            public Closure(T1 v1, T2 v2, T3 v3, T4 v4, T5 v5, T6 v6, T7 v7, T8 v8, T9 v9, T10 v10)
            {
                V1 = v1;
                V2 = v2;
                V3 = v3;
                V4 = v4;
                V5 = v5;
                V6 = v6;
                V7 = v7;
                V8 = v8;
                V9 = v9;
                V10 = v10;
            }
        }

        public sealed class ArrayClosure
        {
            public readonly object[] Constants;

            public static FieldInfo ArrayField = typeof(ArrayClosure).GetTypeInfo().GetDeclaredField(nameof(Constants));

            public static ConstructorInfo Constructor =
                typeof(ArrayClosure).GetTypeInfo().DeclaredConstructors.GetFirst();

            public ArrayClosure(object[] constants)
            {
                Constants = constants;
            }
        }

#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member

        #endregion

        #region Nested Lambdas

        private struct NestedLambdaInfo
        {
            public readonly ClosureInfo ClosureInfo;
            public readonly object Lambda;
            public readonly bool IsAction;

            public NestedLambdaInfo(ClosureInfo closureInfo, object lambda, bool isAction)
            {
                ClosureInfo = closureInfo;
                Lambda = lambda;
                IsAction = isAction;
            }
        }

        internal static class CurryClosureFuncs
        {
            private static readonly IEnumerable<MethodInfo> _methods =
                typeof(CurryClosureFuncs).GetTypeInfo().DeclaredMethods;

            public static readonly MethodInfo[] Methods = _methods.AsArray();

            public static Func<R> Curry<C, R>(Func<C, R> f, C c) => () => f(c);
            public static Func<T1, R> Curry<C, T1, R>(Func<C, T1, R> f, C c) => t1 => f(c, t1);
            public static Func<T1, T2, R> Curry<C, T1, T2, R>(Func<C, T1, T2, R> f, C c) => (t1, t2) => f(c, t1, t2);

            public static Func<T1, T2, T3, R> Curry<C, T1, T2, T3, R>(Func<C, T1, T2, T3, R> f, C c) =>
                (t1, t2, t3) => f(c, t1, t2, t3);

            public static Func<T1, T2, T3, T4, R> Curry<C, T1, T2, T3, T4, R>(Func<C, T1, T2, T3, T4, R> f, C c) =>
                (t1, t2, t3, t4) => f(c, t1, t2, t3, t4);

            public static Func<T1, T2, T3, T4, T5, R> Curry<C, T1, T2, T3, T4, T5, R>(Func<C, T1, T2, T3, T4, T5, R> f,
                C c) => (t1, t2, t3, t4, t5) => f(c, t1, t2, t3, t4, t5);

            public static Func<T1, T2, T3, T4, T5, T6, R>
                Curry<C, T1, T2, T3, T4, T5, T6, R>(Func<C, T1, T2, T3, T4, T5, T6, R> f, C c) =>
                (t1, t2, t3, t4, t5, t6) => f(c, t1, t2, t3, t4, t5, t6);
        }

        internal static class CurryClosureActions
        {
            private static readonly IEnumerable<MethodInfo> _methods =
                typeof(CurryClosureActions).GetTypeInfo().DeclaredMethods;

            public static readonly MethodInfo[] Methods = _methods.AsArray();

            public static Action Curry<C>(Action<C> a, C c) => () => a(c);
            public static Action<T1> Curry<C, T1>(Action<C, T1> f, C c) => t1 => f(c, t1);
            public static Action<T1, T2> Curry<C, T1, T2>(Action<C, T1, T2> f, C c) => (t1, t2) => f(c, t1, t2);

            public static Action<T1, T2, T3> Curry<C, T1, T2, T3>(Action<C, T1, T2, T3> f, C c) =>
                (t1, t2, t3) => f(c, t1, t2, t3);

            public static Action<T1, T2, T3, T4> Curry<C, T1, T2, T3, T4>(Action<C, T1, T2, T3, T4> f, C c) =>
                (t1, t2, t3, t4) => f(c, t1, t2, t3, t4);

            public static Action<T1, T2, T3, T4, T5> Curry<C, T1, T2, T3, T4, T5>(Action<C, T1, T2, T3, T4, T5> f,
                C c) => (t1, t2, t3, t4, t5) => f(c, t1, t2, t3, t4, t5);

            public static Action<T1, T2, T3, T4, T5, T6>
                Curry<C, T1, T2, T3, T4, T5, T6>(Action<C, T1, T2, T3, T4, T5, T6> f, C c) =>
                (t1, t2, t3, t4, t5, t6) => f(c, t1, t2, t3, t4, t5, t6);
        }

        #endregion

        #region Collect Bound Constants

        private static bool IsClosureBoundConstant(object value, TypeInfo type) =>
            value is Delegate ||
            !type.IsPrimitive && !type.IsEnum && !(value is string) && !(value is Type) && !(value is decimal);

        // @paramExprs is required for nested lambda compilation
        private static bool TryCollectBoundConstants(ref ClosureInfo closure, Expression expr, IReadOnlyList<ParameterExpression> paramExprs)
        {
            while (true)
            {
                if (expr == null)
                    return false;

                switch (expr.NodeType)
                {
                    case ExpressionType.Constant:
                        var constantExpr = (ConstantExpression)expr;
                        var value = constantExpr.Value;
                        if (value != null && IsClosureBoundConstant(value, value.GetType().GetTypeInfo()))
                            closure.AddConstant(constantExpr);
                        return true;

                    case ExpressionType.Parameter:
                        // if parameter is used BUT is not in passed parameters and not in local variables,
                        // it means parameter is provided by outer lambda and should be put in closure for current lambda
                        if (paramExprs.GetFirstIndex(expr) == -1 && !closure.IsLocalVar(expr))
                            closure.AddNonPassedParam((ParameterExpression)expr);
                        return true;

                    case ExpressionType.Call:
                        var methodCallExpr = (MethodCallExpression)expr;
                        if (methodCallExpr.Arguments.Count != 0 &&
                            !TryCollectBoundConstants(ref closure, methodCallExpr.Arguments, paramExprs))
                            return false;
                        if (methodCallExpr.Object == null)
                            return true;
                        expr = methodCallExpr.Object;
                        continue;

                    case ExpressionType.MemberAccess:
                        var memberExpr = ((MemberExpression)expr).Expression;
                        if (memberExpr == null)
                            return true;
                        expr = memberExpr;
                        continue;

                    case ExpressionType.New:
                        return TryCollectBoundConstants(ref closure, ((NewExpression)expr).Arguments, paramExprs);

                    case ExpressionType.NewArrayBounds:
                    case ExpressionType.NewArrayInit:
                        return TryCollectBoundConstants(ref closure, ((NewArrayExpression)expr).Expressions, paramExprs);

                    case ExpressionType.MemberInit:
                        return TryCollectMemberInitExprConstants(ref closure, (MemberInitExpression)expr, paramExprs);

                    case ExpressionType.Lambda:
                        closure.AddNestedLambda((LambdaExpression)expr);
                        return true;

                    case ExpressionType.Invoke:
                        // optimization #138: we inline the invoked lambda body (only for lambdas without arguments)
                        // therefore we skipping collecting the lambda and invocation arguments and got directly to lambda body.
                        // This approach is repeated in `TryEmitInvoke`
                        var invokeExpr = (InvocationExpression)expr;
                        if (invokeExpr.Expression is LambdaExpression lambdaExpr && lambdaExpr.Parameters.Count == 0)
                        {
                            expr = lambdaExpr.Body;
                            continue;
                        }

                        if (invokeExpr.Arguments.Count != 0 &&
                            !TryCollectBoundConstants(ref closure, invokeExpr.Arguments, paramExprs))
                            return false;

                        expr = invokeExpr.Expression;
                        continue;

                    case ExpressionType.Conditional:
                        var condExpr = (ConditionalExpression)expr;
                        if (!TryCollectBoundConstants(ref closure, condExpr.Test, paramExprs) ||
                            !TryCollectBoundConstants(ref closure, condExpr.IfFalse, paramExprs))
                            return false;
                        expr = condExpr.IfTrue;
                        continue;

                    case ExpressionType.Block:
                        var blockExpr = (BlockExpression)expr;
                        closure.PushBlock(blockExpr.Variables, Tools.Empty<LocalBuilder>());
                        if (!TryCollectBoundConstants(ref closure, blockExpr.Expressions, paramExprs))
                            return false;
                        closure.PopBlock();
                        return true;

                    case ExpressionType.Loop:
                        var loopExpr = (LoopExpression)expr;
                        closure.AddLabel(loopExpr.BreakLabel);
                        closure.AddLabel(loopExpr.ContinueLabel);
                        expr = loopExpr.Body;
                        continue;

                    case ExpressionType.Index:
                        var indexExpr = (IndexExpression)expr;
                        if (!TryCollectBoundConstants(ref closure, indexExpr.Arguments, paramExprs))
                            return false;
                        if (indexExpr.Object == null)
                            return true;
                        expr = indexExpr.Object;
                        continue;

                    case ExpressionType.Try:
                        return TryCollectTryExprConstants(ref closure, (TryExpression)expr, paramExprs);

                    case ExpressionType.Label:
                        var labelExpr = (LabelExpression)expr;
                        var defaultValueExpr = labelExpr.DefaultValue;
                        closure.AddLabel(labelExpr.Target);
                        if (defaultValueExpr == null)
                            return true;
                        expr = defaultValueExpr;
                        continue;

                    case ExpressionType.Goto:
                        var gotoValueExpr = ((GotoExpression)expr).Value;
                        if (gotoValueExpr == null)
                            return true;
                        expr = gotoValueExpr;
                        continue;

                    case ExpressionType.Switch:
                        var switchExpr = ((SwitchExpression)expr);
                        if (!TryCollectBoundConstants(ref closure, switchExpr.SwitchValue, paramExprs) ||
                            switchExpr.DefaultBody != null && !TryCollectBoundConstants(ref closure, switchExpr.DefaultBody, paramExprs))
                            return false;
                        for (var i = 0; i < switchExpr.Cases.Count; i++)
                            if (!TryCollectBoundConstants(ref closure, switchExpr.Cases[i].Body, paramExprs))
                                return false;
                        return true;

                    case ExpressionType.Extension:
                        expr = expr.Reduce();
                        continue;

                    case ExpressionType.Default:
                        return true;

                    default:
                        if (expr is UnaryExpression unaryExpr)
                        {
                            expr = unaryExpr.Operand;
                            continue;
                        }

                        if (expr is BinaryExpression binaryExpr)
                        {
                            if (!TryCollectBoundConstants(ref closure, binaryExpr.Left, paramExprs))
                                return false;
                            expr = binaryExpr.Right;
                            continue;
                        }

                        if (expr is TypeBinaryExpression typeBinaryExpr)
                        {
                            expr = typeBinaryExpr.Expression;
                            continue;
                        }

                        return false;
                }
            }
        }

        private static bool TryCompileNestedLambda(ref ClosureInfo closure, int lambdaIndex,
            LambdaExpression lambdaExpr)
        {
            // 1. Try to compile nested lambda in place
            // 2. Check that parameters used in compiled lambda are passed or closed by outer lambda
            // 3. Add the compiled lambda to closure of outer lambda for later invocation

            var lambdaParamExprs = lambdaExpr.Parameters;

            var nestedClosure = new ClosureInfo(false);
            var compiledLambda = TryCompile(ref nestedClosure,
                lambdaExpr.Type, Tools.GetParamTypes(lambdaParamExprs), lambdaExpr.ReturnType, lambdaExpr.Body,
                lambdaParamExprs, isNestedLambda: true);

            if (compiledLambda == null)
                return false;

            var isAction = lambdaExpr.ReturnType == typeof(void);
            closure.NestedLambdas[lambdaIndex] = new NestedLambdaInfo(nestedClosure, compiledLambda, isAction);

            if (nestedClosure.HasClosure)
                CopyNestedClosureInfo(lambdaParamExprs, ref closure, ref nestedClosure);

            return true;
        }

        private static bool TryCollectMemberInitExprConstants(ref ClosureInfo closure, MemberInitExpression expr,
            IReadOnlyList<ParameterExpression> paramExprs)
        {
            var newExpr = expr.NewExpression
#if LIGHT_EXPRESSION
                          ?? expr.Expression
#endif
                ;
            if (!TryCollectBoundConstants(ref closure, newExpr, paramExprs))
                return false;

            var memberBindings = expr.Bindings;
            for (var i = 0; i < memberBindings.Count; ++i)
            {
                var memberBinding = memberBindings[i];
                if (memberBinding.BindingType == MemberBindingType.Assignment &&
                    !TryCollectBoundConstants(ref closure, ((MemberAssignment)memberBinding).Expression, paramExprs))
                    return false;
            }

            return true;
        }

        private static bool TryCollectTryExprConstants(ref ClosureInfo closure, TryExpression tryExpr,
            IReadOnlyList<ParameterExpression> paramExprs)
        {
            if (!TryCollectBoundConstants(ref closure, tryExpr.Body, paramExprs))
                return false;

            var catchBlocks = tryExpr.Handlers;
            for (var i = 0; i < catchBlocks.Count; i++)
            {
                var catchBlock = catchBlocks[i];
                var catchBody = catchBlock.Body;
                var catchExVar = catchBlock.Variable;
                if (catchExVar != null)
                {
                    closure.PushBlock(new[] { catchExVar }, Tools.Empty<LocalBuilder>());
                    if (!TryCollectBoundConstants(ref closure, catchExVar, paramExprs))
                        return false;
                }

                var filterExpr = catchBlock.Filter;
                if (filterExpr != null &&
                    !TryCollectBoundConstants(ref closure, filterExpr, paramExprs) ||
                    !TryCollectBoundConstants(ref closure, catchBody, paramExprs))
                    return false;

                if (catchExVar != null)
                    closure.PopBlock();
            }

            var finallyExpr = tryExpr.Finally;

            return finallyExpr == null || TryCollectBoundConstants(ref closure, finallyExpr, paramExprs);
        }

        private static bool TryCollectBoundConstants(ref ClosureInfo closure, IReadOnlyList<Expression> exprs,
            IReadOnlyList<ParameterExpression> paramExprs)
        {
            for (var i = 0; i < exprs.Count; i++)
                if (!TryCollectBoundConstants(ref closure, exprs[i], paramExprs))
                    return false;
            return true;
        }

        #endregion

        // The minimal context-aware flags set by parent
        [Flags]
        internal enum ParentFlags
        {
            Empty = 0,
            IgnoreResult = 1 << 1,
            Call = 1 << 2,
            MemberAccess = 1 << 3, // Any Parent Expression is a MemberExpression
            Arithmetic = 1 << 4,
            Coalesce = 1 << 5,
            InstanceAccess = 1 << 6,
            DupMemberOwner = 1 << 7,
            TryCatch = 1 << 8,
            InstanceCall = Call | InstanceAccess
        }

        internal static bool IgnoresResult(this ParentFlags parent) => (parent & ParentFlags.IgnoreResult) != 0;

        /// <summary>Supports emitting of selected expressions, e.g. lambdaExpr are not supported yet.
        /// When emitter find not supported expression it will return false from <see cref="TryEmit"/>, so I could fallback
        /// to normal and slow Expression.Compile.</summary>
        private static class EmittingVisitor
        {
#if NETSTANDARD1_3 || NETSTANDARD1_4 || NETSTANDARD1_5 || NETSTANDARD1_6
            private static readonly MethodInfo _getTypeFromHandleMethod =
                typeof(Type).GetTypeInfo().GetDeclaredMethod("GetTypeFromHandle");

            private static readonly MethodInfo _objectEqualsMethod = GetObjectEquals();
            private static MethodInfo GetObjectEquals()
            {
                var ms = typeof(object).GetTypeInfo().GetDeclaredMethods("Equals");
                foreach (var m in ms)
                    if (m.GetParameters().Length == 2)
                        return m;
                throw new InvalidOperationException("object.Equals is not found");
            }
#else
            private static readonly MethodInfo _getTypeFromHandleMethod =
                ((Func<RuntimeTypeHandle, Type>)Type.GetTypeFromHandle).Method;

            private static readonly MethodInfo _objectEqualsMethod =
                ((Func<object, object, bool>)object.Equals).Method;
#endif

            public static bool TryEmit(Expression expr, IReadOnlyList<ParameterExpression> paramExprs,
                ILGenerator il, ref ClosureInfo closure, ParentFlags parent, int byRefIndex = -1)
            {
                while (true)
                {
                    closure.LastEmitIsAddress = false;

                    switch (expr.NodeType)
                    {
                        case ExpressionType.Parameter:
                            return parent.IgnoresResult() ||
                                   TryEmitParameter((ParameterExpression)expr, paramExprs, il, ref closure, parent, byRefIndex);

                        case ExpressionType.TypeAs:
                            return TryEmitTypeAs((UnaryExpression)expr, paramExprs, il, ref closure, parent);

                        case ExpressionType.TypeIs:
                            return TryEmitTypeIs((TypeBinaryExpression)expr, paramExprs, il, ref closure, parent);

                        case ExpressionType.Not:
                            return TryEmitNot((UnaryExpression)expr, paramExprs, il, ref closure, parent);

                        case ExpressionType.Convert:
                        case ExpressionType.ConvertChecked:
                            return TryEmitConvert((UnaryExpression)expr, paramExprs, il, ref closure, parent);

                        case ExpressionType.ArrayIndex:
                            var arrIndexExpr = (BinaryExpression)expr;
                            return TryEmit(arrIndexExpr.Left, paramExprs, il, ref closure, parent) &&
                                   TryEmit(arrIndexExpr.Right, paramExprs, il, ref closure, parent) &&
                                   TryEmitArrayIndex(expr.Type, il);

                        case ExpressionType.Constant:
                            var constantExpression = (ConstantExpression)expr;
                            return IgnoresResult(parent) ||
                                   TryEmitConstant(constantExpression, constantExpression.Type, constantExpression.Value, il, ref closure);

                        case ExpressionType.Call:
                            return TryEmitMethodCall((MethodCallExpression)expr, paramExprs, il, ref closure, parent);

                        case ExpressionType.MemberAccess:
                            return TryEmitMemberAccess((MemberExpression)expr, paramExprs, il, ref closure, parent);

                        case ExpressionType.New:
                            var newExpr = (NewExpression)expr;
                            var argExprs = newExpr.Arguments;
                            for (var i = 0; i < argExprs.Count; i++)
                                if (!TryEmit(argExprs[i], paramExprs, il, ref closure, parent,
                                    argExprs[i].Type.IsByRef ? i : -1))
                                    return false;

                            return TryEmitNew(newExpr.Constructor, newExpr.Type, il);

                        case ExpressionType.NewArrayBounds:
                        case ExpressionType.NewArrayInit:
                            return EmitNewArray((NewArrayExpression)expr, paramExprs, il, ref closure, parent);

                        case ExpressionType.MemberInit:
                            return EmitMemberInit((MemberInitExpression)expr, paramExprs, il, ref closure, parent);

                        case ExpressionType.Lambda:
                            return TryEmitNestedLambda((LambdaExpression)expr, paramExprs, il, ref closure);

                        case ExpressionType.Invoke:
                            return TryEmitInvoke((InvocationExpression)expr, paramExprs, il, ref closure, parent);

                        case ExpressionType.GreaterThan:
                        case ExpressionType.GreaterThanOrEqual:
                        case ExpressionType.LessThan:
                        case ExpressionType.LessThanOrEqual:
                        case ExpressionType.Equal:
                        case ExpressionType.NotEqual:
                            var binaryExpr = (BinaryExpression)expr;
                            return TryEmitComparison(binaryExpr.Left, binaryExpr.Right, binaryExpr.NodeType,
                                paramExprs, il, ref closure, parent);

                        case ExpressionType.Add:
                        case ExpressionType.AddChecked:
                        case ExpressionType.Subtract:
                        case ExpressionType.SubtractChecked:
                        case ExpressionType.Multiply:
                        case ExpressionType.MultiplyChecked:
                        case ExpressionType.Divide:
                        case ExpressionType.Modulo:
                        case ExpressionType.Power:
                        case ExpressionType.And:
                        case ExpressionType.Or:
                        case ExpressionType.ExclusiveOr:
                        case ExpressionType.LeftShift:
                        case ExpressionType.RightShift:
                            var arithmeticExpr = (BinaryExpression)expr;
                            return TryEmitArithmetic(arithmeticExpr, expr.NodeType, paramExprs, il, ref closure, parent);

                        case ExpressionType.AndAlso:
                        case ExpressionType.OrElse:
                            return TryEmitLogicalOperator((BinaryExpression)expr, paramExprs, il, ref closure, parent);

                        case ExpressionType.Coalesce:
                            return TryEmitCoalesceOperator((BinaryExpression)expr, paramExprs, il, ref closure, parent);

                        case ExpressionType.Conditional:
                            return TryEmitConditional((ConditionalExpression)expr, paramExprs, il, ref closure, parent);

                        case ExpressionType.PostIncrementAssign:
                        case ExpressionType.PreIncrementAssign:
                        case ExpressionType.PostDecrementAssign:
                        case ExpressionType.PreDecrementAssign:
                            return TryEmitIncDecAssign((UnaryExpression)expr, paramExprs, il, ref closure, parent);

                        case ExpressionType arithmeticAssign
                            when Tools.GetArithmeticFromArithmeticAssignOrSelf(arithmeticAssign) != arithmeticAssign:
                        case ExpressionType.Assign:
                            return TryEmitAssign((BinaryExpression)expr, paramExprs, il, ref closure, parent);

                        case ExpressionType.Block:
                            var blockExpr = (BlockExpression)expr;
                            var blockHasVars = blockExpr.Variables.Count != 0;
                            if (blockHasVars)
                                closure.PushBlockAndConstructLocalVars(blockExpr.Variables, il);

                            // ignore result for all not the last statements in block
                            var exprs = blockExpr.Expressions;
                            for (var i = 0; i < exprs.Count - 1; i++)
                                if (!TryEmit(exprs[i], paramExprs, il, ref closure, parent | ParentFlags.IgnoreResult))
                                    return false;

                            // last (result) statement in block will provide the result
                            expr = blockExpr.Result;
                            if (!blockHasVars)
                                continue; // OMG, no recursion!

                            if (!TryEmit(blockExpr.Result, paramExprs, il, ref closure, parent))
                                return false;
                            closure.PopBlock();
                            return true;

                        case ExpressionType.Loop:
                            var loopExpr = (LoopExpression)expr;

                            // Mark the start of the loop body:
                            var loopBodyLabel = il.DefineLabel();
                            il.MarkLabel(loopBodyLabel);

                            if (loopExpr.ContinueLabel != null)
                                il.MarkLabel(closure.GetOrCreateLabel(loopExpr.ContinueLabel, il));

                            if (!TryEmit(loopExpr.Body, paramExprs, il, ref closure, parent))
                                return false;

                            // If loop hasn't exited, jump back to start of its body:
                            il.Emit(OpCodes.Br, loopBodyLabel);

                            if (loopExpr.BreakLabel != null)
                                il.MarkLabel(closure.GetOrCreateLabel(loopExpr.BreakLabel, il));

                            return true;

                        case ExpressionType.Try:
                            return TryEmitTryCatchFinallyBlock((TryExpression)expr, paramExprs, il, ref closure,
                                parent | ParentFlags.TryCatch);

                        case ExpressionType.Throw:
                            {
                                var opExpr = ((UnaryExpression)expr).Operand;
                                if (!TryEmit(opExpr, paramExprs, il, ref closure, parent & ~ParentFlags.IgnoreResult))
                                    return false;

                                il.Emit(OpCodes.Throw);
                                return true;
                            }

                        case ExpressionType.Default:
                            return expr.Type == typeof(void) || IgnoresResult(parent) ||
                                   EmitDefault(expr.Type, il);

                        case ExpressionType.Index:
                            var indexExpr = (IndexExpression)expr;
                            if (indexExpr.Object != null &&
                                !TryEmit(indexExpr.Object, paramExprs, il, ref closure, parent))
                                return false;

                            var indexArgExprs = indexExpr.Arguments;
                            for (var i = 0; i < indexArgExprs.Count; i++)
                                if (!TryEmit(indexArgExprs[i], paramExprs, il, ref closure, parent,
                                    indexArgExprs[i].Type.IsByRef ? i : -1))
                                    return false;

                            return TryEmitIndex((IndexExpression)expr, il);

                        case ExpressionType.Goto:
                            return TryEmitGoto((GotoExpression)expr, paramExprs, il, ref closure, parent);

                        case ExpressionType.Label:
                            return TryEmitLabel((LabelExpression)expr, paramExprs, il, ref closure, parent);

                        case ExpressionType.Switch:
                            return TryEmitSwitch((SwitchExpression)expr, paramExprs, il, ref closure, parent);

                        case ExpressionType.Extension:
                            expr = expr.Reduce();
                            continue;

                        default:
                            return false;
                    }
                }
            }

            private static bool TryEmitLabel(LabelExpression expr,
                IReadOnlyList<ParameterExpression> paramExprs, ILGenerator il, ref ClosureInfo closure, ParentFlags parent)
            {
                var index = closure.GetLabelIndex(expr.Target);
                if (index == -1)
                    return false; // should be found in first collecting constants round

                if (closure.IsTryReturnLabel(index))
                    return true; // label will be emitted by TryEmitTryCatchFinallyBlock

                // define a new label or use the label provided by the preceding GoTo expression
                var label = closure.GetOrCreateLabel(index, il);

                il.MarkLabel(label);

                return expr.DefaultValue == null || TryEmit(expr.DefaultValue, paramExprs, il, ref closure, parent);
            }

            private static bool TryEmitGoto(GotoExpression expr,
                IReadOnlyList<ParameterExpression> paramExprs, ILGenerator il, ref ClosureInfo closure, ParentFlags parent)
            {
                var index = closure.GetLabelIndex(expr.Target);
                if (index == -1)
                    throw new InvalidOperationException("Cannot jump, no labels found");

                if ((expr.Value != null) && !TryEmit(expr.Value, paramExprs, il, ref closure, parent & ~ParentFlags.IgnoreResult))
                    return false;

                switch (expr.Kind)
                {
                    case GotoExpressionKind.Break:
                    case GotoExpressionKind.Continue:
                        return EmitGotoLabel(OpCodes.Br, index, il, ref closure);

                    case GotoExpressionKind.Goto:
                        if (expr.Value != null)
                            goto case GotoExpressionKind.Return;

                        return EmitGotoLabel(OpCodes.Br, index, il, ref closure);

                    case GotoExpressionKind.Return:
                        if ((parent & ParentFlags.TryCatch) == 0)
                            return EmitGotoLabel(OpCodes.Ret, index, il, ref closure);

                        // Can't emit a Return inside a Try/Catch, so leave it to TryEmitTryCatchFinallyBlock
                        // to emit the Leave instruction, return label and return result
                        closure.MarkLabelAsTryReturn(index);
                        return true;

                    default:
                        return false;
                }
            }

            private static bool EmitGotoLabel(OpCode gotoOpCode, int labelIndex, ILGenerator il, ref ClosureInfo closure)
            {
                // use label defined by Label expression or define its own to use by subsequent Label
                il.Emit(gotoOpCode, closure.GetOrCreateLabel(labelIndex, il));
                return true;
            }

            private static bool TryEmitIndex(IndexExpression expr, ILGenerator il)
            {
                var elemType = expr.Type;
                if (expr.Indexer != null)
                    return EmitMethodCall(il, expr.Indexer.FindPropertyGetMethod());

                if (expr.Arguments.Count == 1) // one dimensional array
                {
                    if (elemType.IsValueType())
                        il.Emit(OpCodes.Ldelem, elemType);
                    else
                        il.Emit(OpCodes.Ldelem_Ref);
                    return true;
                }

                // multi dimensional array
                return EmitMethodCall(il, expr.Object?.Type.FindMethod("Get"));
            }

            private static bool TryEmitCoalesceOperator(BinaryExpression exprObj,
                IReadOnlyList<ParameterExpression> paramExprs, ILGenerator il, ref ClosureInfo closure, ParentFlags parent)
            {
                var labelFalse = il.DefineLabel();
                var labelDone = il.DefineLabel();

                var left = exprObj.Left;
                var right = exprObj.Right;

                if (!TryEmit(left, paramExprs, il, ref closure, parent | ParentFlags.Coalesce))
                    return false;

                var leftType = left.Type;
                if (leftType.IsValueType()) // Nullable -> It's the only ValueType comparable to null
                {
                    var loc = il.DeclareLocal(leftType);
                    il.Emit(OpCodes.Stloc_S, loc);
                    il.Emit(OpCodes.Ldloca_S, loc);

                    if (!EmitMethodCall(il, leftType.FindNullableHasValueGetterMethod()))
                        return false;

                    il.Emit(OpCodes.Brfalse, labelFalse);
                    il.Emit(OpCodes.Ldloca_S, loc);
                    if (!EmitMethodCall(il, leftType.FindNullableGetValueOrDefaultMethod()))
                        return false;

                    il.Emit(OpCodes.Br, labelDone);
                    il.MarkLabel(labelFalse);
                    if (!TryEmit(right, paramExprs, il, ref closure, parent | ParentFlags.Coalesce))
                        return false;

                    il.MarkLabel(labelDone);
                    return true;
                }

                il.Emit(OpCodes.Dup); // duplicate left, if it's not null, after the branch this value will be on the top of the stack
                il.Emit(OpCodes.Ldnull);
                il.Emit(OpCodes.Ceq);
                il.Emit(OpCodes.Brfalse, labelFalse);

                il.Emit(OpCodes.Pop); // left is null, pop its value from the stack

                if (!TryEmit(right, paramExprs, il, ref closure, parent | ParentFlags.Coalesce))
                    return false;

                if (right.Type != exprObj.Type)
                {
                    if (right.Type.IsValueType())
                        il.Emit(OpCodes.Box, right.Type);
                    else
                        il.Emit(OpCodes.Castclass, exprObj.Type);
                }

                if (left.Type == exprObj.Type)
                    il.MarkLabel(labelFalse);
                else
                {
                    il.Emit(OpCodes.Br, labelDone);
                    il.MarkLabel(labelFalse);
                    il.Emit(OpCodes.Castclass, exprObj.Type);
                    il.MarkLabel(labelDone);
                }

                return true;
            }

            private static bool EmitDefault(Type type, ILGenerator il)
            {
                if (type == typeof(string))
                {
                    il.Emit(OpCodes.Ldnull);
                }
                else if (
                    type == typeof(bool) ||
                    type == typeof(byte) ||
                    type == typeof(char) ||
                    type == typeof(sbyte) ||
                    type == typeof(int) ||
                    type == typeof(uint) ||
                    type == typeof(short) ||
                    type == typeof(ushort))
                {
                    il.Emit(OpCodes.Ldc_I4_0);
                }
                else if (
                    type == typeof(long) ||
                    type == typeof(ulong))
                {
                    il.Emit(OpCodes.Ldc_I4_0);
                    il.Emit(OpCodes.Conv_I8);
                }
                else if (type == typeof(float))
                    il.Emit(OpCodes.Ldc_R4, default(float));
                else if (type == typeof(double))
                    il.Emit(OpCodes.Ldc_R8, default(double));
                else if (type.IsValueType())
                    il.Emit(OpCodes.Ldloc, InitValueTypeVariable(il, type));
                else
                    il.Emit(OpCodes.Ldnull);

                return true;
            }

            private static bool TryEmitTryCatchFinallyBlock(TryExpression tryExpr,
                IReadOnlyList<ParameterExpression> paramExprs, ILGenerator il, ref ClosureInfo closure, ParentFlags parent)
            {
                il.BeginExceptionBlock();

                if (!TryEmit(tryExpr.Body, paramExprs, il, ref closure, parent))
                    return false;

                var exprType = tryExpr.Type;
                var isNonVoid = exprType != typeof(void); // todo: check how it is correlated with `parent.IgnoreResult`
                var returnResult = default(LocalBuilder);
                if (isNonVoid)
                {
                    il.Emit(OpCodes.Stloc_S, returnResult = il.DeclareLocal(exprType));
                }

                var catchBlocks = tryExpr.Handlers;
                for (var i = 0; i < catchBlocks.Count; i++)
                {
                    var catchBlock = catchBlocks[i];
                    if (catchBlock.Filter != null)
                        return false; // todo: Add support for filters on catch expression

                    il.BeginCatchBlock(catchBlock.Test);

                    // at the beginning of catch the Exception value is on the stack,
                    // we will store into local variable.
                    var exVarExpr = catchBlock.Variable;
                    if (exVarExpr != null)
                    {
                        var exVar = il.DeclareLocal(exVarExpr.Type);
                        closure.PushBlock(new[] { exVarExpr }, new[] { exVar });
                        il.Emit(OpCodes.Stloc_S, exVar);
                    }

                    if (!TryEmit(catchBlock.Body, paramExprs, il, ref closure, parent))
                        return false;

                    if (exVarExpr != null)
                        closure.PopBlock();

                    if (isNonVoid)
                    {
                        il.Emit(OpCodes.Stloc_S, returnResult);
                    }
                }

                var finallyExpr = tryExpr.Finally;
                if (finallyExpr != null)
                {
                    il.BeginFinallyBlock();
                    if (!TryEmit(finallyExpr, paramExprs, il, ref closure, parent))
                        return false;
                }

                il.EndExceptionBlock();
                if (isNonVoid)
                {
                    il.Emit(OpCodes.Ldloc, returnResult);
                }

                return true;
            }

            private static bool TryEmitParameter(ParameterExpression paramExpr,
                IReadOnlyList<ParameterExpression> paramExprs, ILGenerator il, ref ClosureInfo closure,
                ParentFlags parent, int byRefIndex = -1)
            {
                // if parameter is passed through, then just load it on stack
                var paramType = paramExpr.Type;
                var paramIndex = paramExprs.GetFirstIndex(paramExpr);
                if (paramIndex != -1)
                {
                    if (closure.HasClosure)
                        paramIndex += 1; // shift parameter index by one, because the first one will be closure

                    closure.LastEmitIsAddress = !paramExpr.IsByRef && paramType.IsValueType() &&
                        ((parent & ParentFlags.InstanceCall) == ParentFlags.InstanceCall ||
                        (parent & ParentFlags.MemberAccess) != 0);

                    EmitLoadParamArg(il, paramIndex, closure.LastEmitIsAddress);

                    if (paramExpr.IsByRef)
                    {
                        if ((parent & ParentFlags.MemberAccess) != 0 && paramType.IsClass() ||
                            (parent & ParentFlags.Coalesce) != 0)
                            il.Emit(OpCodes.Ldind_Ref);
                        else if ((parent & ParentFlags.Arithmetic) != 0)
                            EmitDereference(il, paramType);
                    }

                    return true;
                }

                // if parameter isn't passed, then it is passed into some outer lambda or it is a local variable,
                // so it should be loaded from closure or from the locals. Then the closure is null will be an invalid state.
                if (!closure.IsClosureConstructed)
                    return false;

                // parameter may represent a variable, so first look if this is the case
                var variable = closure.GetDefinedLocalVarOrDefault(paramExpr);
                if (variable != null)
                {
                    if (byRefIndex != -1 || paramType.IsValueType() && (parent & ParentFlags.MemberAccess) != 0)
                        il.Emit(OpCodes.Ldloca_S, variable);
                    else
                        il.Emit(OpCodes.Ldloc, variable);
                    return true;
                }

                if (paramExpr.IsByRef)
                {
                    il.Emit(OpCodes.Ldloca_S, byRefIndex);
                    return true;
                }

                // the only possibility that we are here is because we are in nested lambda,
                // and it uses some parameter or variable from the outer lambda
                var nonPassedParamIndex = closure.NonPassedParameters.GetFirstIndex(paramExpr);
                if (nonPassedParamIndex == -1)
                    return false; // what??? no chance

                var closureItemIndex = closure.Constants.Length + nonPassedParamIndex;
                return LoadClosureFieldOrItem(ref closure, il, closureItemIndex, paramType);
            }

            private static void EmitDereference(ILGenerator il, Type type)
            {
                if (type == typeof(Int32))
                    il.Emit(OpCodes.Ldind_I4);
                else if (type == typeof(Int64))
                    il.Emit(OpCodes.Ldind_I8);
                else if (type == typeof(Int16))
                    il.Emit(OpCodes.Ldind_I2);
                else if (type == typeof(SByte))
                    il.Emit(OpCodes.Ldind_I1);
                else if (type == typeof(Single))
                    il.Emit(OpCodes.Ldind_R4);
                else if (type == typeof(Double))
                    il.Emit(OpCodes.Ldind_R8);
                else if (type == typeof(IntPtr))
                    il.Emit(OpCodes.Ldind_I);
                else if (type == typeof(UIntPtr))
                    il.Emit(OpCodes.Ldind_I);
                else if (type == typeof(Byte))
                    il.Emit(OpCodes.Ldind_U1);
                else if (type == typeof(UInt16))
                    il.Emit(OpCodes.Ldind_U2);
                else if (type == typeof(UInt32))
                    il.Emit(OpCodes.Ldind_U4);
                else
                    il.Emit(OpCodes.Ldobj, type);
                //todo: UInt64 as there is no OpCodes? Ldind_Ref?
            }

            // loads argument at paramIndex onto evaluation stack
            private static void EmitLoadParamArg(ILGenerator il, int paramIndex, bool asAddress)
            {
                if (asAddress)
                {
                    if (paramIndex <= byte.MaxValue)
                        il.Emit(OpCodes.Ldarga_S, (byte)paramIndex);
                    else
                        il.Emit(OpCodes.Ldarga, paramIndex);
                    return;
                }

                switch (paramIndex)
                {
                    case 0:
                        il.Emit(OpCodes.Ldarg_0);
                        break;
                    case 1:
                        il.Emit(OpCodes.Ldarg_1);
                        break;
                    case 2:
                        il.Emit(OpCodes.Ldarg_2);
                        break;
                    case 3:
                        il.Emit(OpCodes.Ldarg_3);
                        break;
                    default:
                        if (paramIndex <= byte.MaxValue)
                            il.Emit(OpCodes.Ldarg_S, (byte)paramIndex);
                        else
                            il.Emit(OpCodes.Ldarg, paramIndex);
                        break;
                }
            }

            private static bool TryEmitTypeAs(UnaryExpression expr,
                IReadOnlyList<ParameterExpression> paramExprs, ILGenerator il, ref ClosureInfo closure,
                ParentFlags parent)
            {
                if (!TryEmit(expr.Operand, paramExprs, il, ref closure, parent))
                    return false;
                if ((parent & ParentFlags.IgnoreResult) != 0)
                    il.Emit(OpCodes.Pop);
                else
                    il.Emit(OpCodes.Isinst, expr.Type);
                return true;
            }

            private static bool TryEmitTypeIs(TypeBinaryExpression expr,
                IReadOnlyList<ParameterExpression> paramExprs, ILGenerator il, ref ClosureInfo closure,
                ParentFlags parent)
            {
                if (!TryEmit(expr.Expression, paramExprs, il, ref closure, parent))
                    return false;

                if ((parent & ParentFlags.IgnoreResult) != 0)
                    il.Emit(OpCodes.Pop);
                else
                {
                    il.Emit(OpCodes.Isinst, expr.TypeOperand);
                    il.Emit(OpCodes.Ldnull);
                    il.Emit(OpCodes.Cgt_Un);
                }

                return true;
            }

            private static bool TryEmitNot(UnaryExpression expr,
                IReadOnlyList<ParameterExpression> paramExprs, ILGenerator il, ref ClosureInfo closure,
                ParentFlags parent)
            {
                if (!TryEmit(expr.Operand, paramExprs, il, ref closure, parent))
                    return false;
                if ((parent & ParentFlags.IgnoreResult) > 0)
                    il.Emit(OpCodes.Pop);
                else
                {
                    if (expr.Type == typeof(bool))
                    {
                        il.Emit(OpCodes.Ldc_I4_0);
                        il.Emit(OpCodes.Ceq);
                    }
                    else
                    {
                        il.Emit(OpCodes.Not);
                    }
                }
                return true;
            }

            private static bool TryEmitConvert(UnaryExpression expr,
                IReadOnlyList<ParameterExpression> paramExprs, ILGenerator il, ref ClosureInfo closure, ParentFlags parent)
            {
                var opExpr = expr.Operand;
                var method = expr.Method;
                if (method != null && method.Name != "op_Implicit" && method.Name != "op_Explicit")
                    return TryEmit(opExpr, paramExprs, il, ref closure, parent & ~ParentFlags.IgnoreResult | ParentFlags.InstanceCall, 0)
                        && EmitMethodCall(il, method, parent);

                var targetType = expr.Type;

                var sourceType = opExpr.Type;
                var sourceTypeIsNullable = sourceType.IsNullable();
                var underlyingNullableSourceType = Nullable.GetUnderlyingType(sourceType);
                if (sourceTypeIsNullable && targetType == underlyingNullableSourceType)
                {
                    if (!TryEmit(opExpr, paramExprs, il, ref closure, parent & ~ParentFlags.IgnoreResult | ParentFlags.InstanceAccess))
                        return false;

                    if (!closure.LastEmitIsAddress)
                        DeclareAndLoadLocalVariable(il, sourceType);

                    return EmitMethodCall(il, sourceType.FindValueGetterMethod(), parent);
                }

                if (!TryEmit(opExpr, paramExprs, il, ref closure, parent & ~ParentFlags.IgnoreResult & ~ParentFlags.InstanceAccess))
                    return false;

                var targetTypeIsNullable = targetType.IsNullable();
                var underlyingNullableTargetType = Nullable.GetUnderlyingType(targetType);
                if (targetTypeIsNullable && sourceType == underlyingNullableTargetType)
                {
                    il.Emit(OpCodes.Newobj, targetType.GetTypeInfo().DeclaredConstructors.GetFirst());
                    return true;
                }

                if (sourceType == targetType || targetType == typeof(object))
                {
                    if (targetType == typeof(object) && sourceType.IsValueType())
                        il.Emit(OpCodes.Box, sourceType);
                    if (IgnoresResult(parent))
                        il.Emit(OpCodes.Pop);
                    return true;
                }

                // check implicit / explicit conversion operators on source and target types
                // for non-primitives and for non-primitive nullable - #73
                if (!sourceTypeIsNullable && !sourceType.IsPrimitive())
                {
                    var actualTargetType = targetTypeIsNullable ? underlyingNullableTargetType : targetType;

                    var convertOpMethod = sourceType.FindConvertOperator(sourceType, actualTargetType);
                    if (convertOpMethod != null)
                    {
                        EmitMethodCall(il, convertOpMethod, parent);
                        if (targetTypeIsNullable)
                            il.Emit(OpCodes.Newobj, targetType.GetTypeInfo().DeclaredConstructors.GetFirst());
                        return true;
                    }
                }
                else if (!targetTypeIsNullable)
                {
                    var actualSourceType = sourceTypeIsNullable ? underlyingNullableSourceType : sourceType;

                    var convertOpMethod = actualSourceType.FindConvertOperator(actualSourceType, targetType);
                    if (convertOpMethod != null)
                    {
                        if (sourceTypeIsNullable)
                        {
                            DeclareAndLoadLocalVariable(il, sourceType);
                            EmitMethodCall(il, sourceType.FindValueGetterMethod(), parent);
                        }

                        EmitMethodCall(il, convertOpMethod, parent);
                        return true;
                    }
                }

                if (!targetTypeIsNullable && !targetType.IsPrimitive())
                {
                    var actualSourceType = sourceTypeIsNullable ? underlyingNullableSourceType : sourceType;

                    var convertOpMethod = targetType.FindConvertOperator(actualSourceType, targetType);
                    if (convertOpMethod != null)
                    {
                        if (sourceTypeIsNullable)
                        {
                            DeclareAndLoadLocalVariable(il, sourceType);
                            EmitMethodCall(il, sourceType.FindValueGetterMethod(), parent);
                        }

                        EmitMethodCall(il, convertOpMethod, parent);
                        return true;
                    }
                }
                else if (!sourceTypeIsNullable)
                {
                    var actualTargetType = targetTypeIsNullable ? underlyingNullableTargetType : targetType;

                    var convertOpMethod = actualTargetType.FindConvertOperator(sourceType, actualTargetType);
                    if (convertOpMethod != null)
                    {
                        EmitMethodCall(il, convertOpMethod, parent);
                        if (targetTypeIsNullable)
                            il.Emit(OpCodes.Newobj, targetType.GetTypeInfo().DeclaredConstructors.GetFirst());
                        return true;
                    }
                }

                if (sourceType == typeof(object) && targetType.IsValueType())
                {
                    il.Emit(OpCodes.Unbox_Any, targetType);
                }
                else if (targetTypeIsNullable)
                {
                    // Conversion to Nullable: `new Nullable<T>(T val);`
                    if (!sourceTypeIsNullable)
                    {
                        if (!TryEmitValueConvert(underlyingNullableTargetType, il, isChecked: false))
                            return false;

                        il.Emit(OpCodes.Newobj, targetType.GetTypeInfo().DeclaredConstructors.GetFirst());
                    }
                    else
                    {
                        var sourceVar = DeclareAndLoadLocalVariable(il, sourceType);

                        if (!EmitMethodCall(il, sourceType.FindNullableHasValueGetterMethod()))
                            return false;

                        var labelSourceHasValue = il.DefineLabel();
                        il.Emit(OpCodes.Brtrue_S, labelSourceHasValue); // jump where source has a value

                        // otherwise, emit and load a `new Nullable<TTarget>()` struct (that's why a Init instead of New)
                        il.Emit(OpCodes.Ldloc, InitValueTypeVariable(il, targetType));

                        // jump to completion
                        var labelDone = il.DefineLabel();
                        il.Emit(OpCodes.Br_S, labelDone);

                        // if source nullable has a value:
                        il.MarkLabel(labelSourceHasValue);
                        il.Emit(OpCodes.Ldloca_S, sourceVar);
                        if (!EmitMethodCall(il, sourceType.FindNullableGetValueOrDefaultMethod()))
                            return false;

                        if (!TryEmitValueConvert(underlyingNullableTargetType, il,
                            expr.NodeType == ExpressionType.ConvertChecked))
                            return false;

                        il.Emit(OpCodes.Newobj, targetType.GetTypeInfo().DeclaredConstructors.GetFirst());
                        il.MarkLabel(labelDone);
                    }
                }
                else
                {
                    if (targetType.GetTypeInfo().IsEnum)
                        targetType = Enum.GetUnderlyingType(targetType);

                    // fixes #159
                    if (sourceTypeIsNullable)
                    {
                        DeclareAndLoadLocalVariable(il, sourceType);
                        EmitMethodCall(il, sourceType.FindValueGetterMethod(), parent);
                    }

                    // cast as the last resort and let's it fail if unlucky
                    if (!TryEmitValueConvert(targetType, il, expr.NodeType == ExpressionType.ConvertChecked))
                        il.Emit(OpCodes.Castclass, targetType);
                }

                if (IgnoresResult(parent))
                    il.Emit(OpCodes.Pop);

                return true;
            }

            private static bool TryEmitValueConvert(Type targetType, ILGenerator il, bool isChecked)
            {
                if (targetType == typeof(int))
                    il.Emit(isChecked ? OpCodes.Conv_Ovf_I4 : OpCodes.Conv_I4);
                else if (targetType == typeof(float))
                    il.Emit(OpCodes.Conv_R4);
                else if (targetType == typeof(uint))
                    il.Emit(isChecked ? OpCodes.Conv_Ovf_U4 : OpCodes.Conv_U4);
                else if (targetType == typeof(sbyte))
                    il.Emit(isChecked ? OpCodes.Conv_Ovf_I1 : OpCodes.Conv_I1);
                else if (targetType == typeof(byte))
                    il.Emit(isChecked ? OpCodes.Conv_Ovf_U1 : OpCodes.Conv_U1);
                else if (targetType == typeof(short))
                    il.Emit(isChecked ? OpCodes.Conv_Ovf_I2 : OpCodes.Conv_I2);
                else if (targetType == typeof(ushort) || targetType == typeof(char))
                    il.Emit(isChecked ? OpCodes.Conv_Ovf_U2 : OpCodes.Conv_U2);
                else if (targetType == typeof(long))
                    il.Emit(isChecked ? OpCodes.Conv_Ovf_I8 : OpCodes.Conv_I8);
                else if (targetType == typeof(ulong))
                    il.Emit(isChecked ? OpCodes.Conv_Ovf_U8 : OpCodes.Conv_U8);
                else if (targetType == typeof(double))
                    il.Emit(OpCodes.Conv_R8);
                else
                    return false;
                return true;
            }

            private static bool TryEmitConstant(ConstantExpression expr, Type exprType, object constantValue, ILGenerator il, ref ClosureInfo closure)
            {
                if (constantValue == null)
                {
                    if (exprType.IsValueType()) // handles the conversion of null to Nullable<T>
                        il.Emit(OpCodes.Ldloc, InitValueTypeVariable(il, exprType));
                    else
                        il.Emit(OpCodes.Ldnull);
                    return true;
                }

                var constantType = constantValue.GetType();
                if (expr != null && IsClosureBoundConstant(constantValue, constantType.GetTypeInfo()))
                {
                    var constIndex = closure.Constants.GetFirstIndex(expr);
                    if (constIndex == -1 || !LoadClosureFieldOrItem(ref closure, il, constIndex, exprType))
                        return false;
                }
                else
                {
                    // get raw enum type to light
                    if (constantType.GetTypeInfo().IsEnum)
                        constantType = Enum.GetUnderlyingType(constantType);

                    if (constantType == typeof(int))
                    {
                        EmitLoadConstantInt(il, (int)constantValue);
                    }
                    else if (constantType == typeof(char))
                    {
                        EmitLoadConstantInt(il, (char)constantValue);
                    }
                    else if (constantType == typeof(short))
                    {
                        EmitLoadConstantInt(il, (short)constantValue);
                    }
                    else if (constantType == typeof(byte))
                    {
                        EmitLoadConstantInt(il, (byte)constantValue);
                    }
                    else if (constantType == typeof(ushort))
                    {
                        EmitLoadConstantInt(il, (ushort)constantValue);
                    }
                    else if (constantType == typeof(sbyte))
                    {
                        EmitLoadConstantInt(il, (sbyte)constantValue);
                    }
                    else if (constantType == typeof(uint))
                    {
                        unchecked
                        {
                            EmitLoadConstantInt(il, (int)(uint)constantValue);
                        }
                    }
                    else if (constantType == typeof(long))
                    {
                        il.Emit(OpCodes.Ldc_I8, (long)constantValue);
                    }
                    else if (constantType == typeof(ulong))
                    {
                        unchecked
                        {
                            il.Emit(OpCodes.Ldc_I8, (long)(ulong)constantValue);
                        }
                    }
                    else if (constantType == typeof(float))
                    {
                        il.Emit(OpCodes.Ldc_R4, (float)constantValue);
                    }
                    else if (constantType == typeof(double))
                    {
                        il.Emit(OpCodes.Ldc_R8, (double)constantValue);
                    }
                    else if (constantType == typeof(bool))
                    {
                        il.Emit((bool)constantValue ? OpCodes.Ldc_I4_1 : OpCodes.Ldc_I4_0);
                    }
                    else if (constantValue is string stringValue)
                    {
                        il.Emit(OpCodes.Ldstr, stringValue);
                        return true;
                    }
                    else if (constantValue is Type typeValue)
                    {
                        il.Emit(OpCodes.Ldtoken, typeValue);
                        il.Emit(OpCodes.Call, _getTypeFromHandleMethod);
                        return true;
                    }
                    else if (constantType == typeof(IntPtr))
                    {
                        il.Emit(OpCodes.Ldc_I8, ((IntPtr)constantValue).ToInt64());
                    }
                    else if (constantType == typeof(UIntPtr))
                    {
                        unchecked
                        {
                            il.Emit(OpCodes.Ldc_I8, (long)((UIntPtr)constantValue).ToUInt64());
                        }
                    }
                    else if (constantType == typeof(decimal))
                    {
                        EmitDecimalConstant((decimal)constantValue, il);
                    }
                    else return false;
                }

                var underlyingNullableType = Nullable.GetUnderlyingType(exprType);
                if (underlyingNullableType != null)
                    il.Emit(OpCodes.Newobj, exprType.GetTypeInfo().DeclaredConstructors.GetFirst());

                // todo: consider how to remove boxing where it is not required
                // boxing the value type, otherwise we can get a strange result when 0 is treated as Null.
                else if (exprType == typeof(object) && constantType.IsValueType())
                    il.Emit(OpCodes.Box, constantValue.GetType()); // using normal type for Enum instead of underlying type

                return true;
            }

            private static void EmitDecimalConstant(decimal value, ILGenerator il)
            {
                //check if decimal has decimal places, if not use shorter IL code (constructor from int or long)
                if (value % 1 == 0)
                {
                    if (value >= int.MinValue && value <= int.MaxValue)
                    {
                        EmitLoadConstantInt(il, decimal.ToInt32(value));
                        il.Emit(OpCodes.Newobj, typeof(decimal).FindSingleParamConstructor(typeof(int)));
                        return;
                    }

                    if (value >= long.MinValue && value <= long.MaxValue)
                    {
                        il.Emit(OpCodes.Ldc_I8, decimal.ToInt64(value));
                        il.Emit(OpCodes.Newobj, typeof(decimal).FindSingleParamConstructor(typeof(long)));
                        return;
                    }
                }

                if (value == decimal.MinValue)
                {
                    il.Emit(OpCodes.Ldsfld, typeof(decimal).GetTypeInfo().GetDeclaredField(nameof(decimal.MinValue)));
                    return;
                }

                if (value == decimal.MaxValue)
                {
                    il.Emit(OpCodes.Ldsfld, typeof(decimal).GetTypeInfo().GetDeclaredField(nameof(decimal.MaxValue)));
                    return;
                }

                var parts = decimal.GetBits(value);
                var sign = (parts[3] & 0x80000000) != 0;
                var scale = (byte)((parts[3] >> 16) & 0x7F);

                EmitLoadConstantInt(il, parts[0]);
                EmitLoadConstantInt(il, parts[1]);
                EmitLoadConstantInt(il, parts[2]);

                il.Emit(sign ? OpCodes.Ldc_I4_1 : OpCodes.Ldc_I4_0);
                EmitLoadConstantInt(il, scale);

                il.Emit(OpCodes.Conv_U1);

                il.Emit(OpCodes.Newobj, _decimalCtor.Value);
            }

            private static readonly Lazy<ConstructorInfo> _decimalCtor = new Lazy<ConstructorInfo>(() =>
            {
                foreach (var ctor in typeof(decimal).GetTypeInfo().DeclaredConstructors)
                    if (ctor.GetParameters().Length == 5)
                        return ctor;
                return null;
            });

            private static LocalBuilder DeclareAndLoadLocalVariable(ILGenerator il, Type type)
            {
                var loc = il.DeclareLocal(type);
                il.Emit(OpCodes.Stloc, loc);
                il.Emit(OpCodes.Ldloca_S, loc);
                return loc;
            }

            private static LocalBuilder InitValueTypeVariable(ILGenerator il, Type exprType,
                LocalBuilder existingVar = null)
            {
                var valVar = existingVar ?? il.DeclareLocal(exprType);
                il.Emit(OpCodes.Ldloca_S, valVar);
                il.Emit(OpCodes.Initobj, exprType);
                return valVar;
            }

            private static bool LoadClosureFieldOrItem(ref ClosureInfo closure, ILGenerator il, int itemIndex,
                Type itemType, Expression itemExprObj = null)
            {
                il.Emit(OpCodes.Ldarg_0); // closure is always a first argument

                var closureFields = closure.ClosureFields;
                if (closureFields != null)
                    il.Emit(OpCodes.Ldfld, closureFields[itemIndex]);
                else
                {
                    // for ArrayClosure load an array field
                    il.Emit(OpCodes.Ldfld, ArrayClosure.ArrayField);

                    // load array item index
                    EmitLoadConstantInt(il, itemIndex);

                    // load item from index
                    il.Emit(OpCodes.Ldelem_Ref);
                    itemType = itemType ?? itemExprObj?.Type;
                    if (itemType == null)
                        return false;

                    il.Emit(itemType.IsValueType() ? OpCodes.Unbox_Any : OpCodes.Castclass, itemType);
                }

                return true;
            }

            // todo: Replace resultValueVar with a closureInfo block
            private static bool TryEmitNew(ConstructorInfo ctor, Type exprType, ILGenerator il, LocalBuilder resultValueVar = null)
            {
                if (ctor != null)
                    il.Emit(OpCodes.Newobj, ctor);
                else
                {
                    if (!exprType.IsValueType())
                        return false; // null constructor and not a value type, better fallback

                    var valueVar = InitValueTypeVariable(il, exprType, resultValueVar);
                    if (resultValueVar == null)
                        il.Emit(OpCodes.Ldloc, valueVar);
                }

                return true;
            }

            private static bool EmitNewArray(NewArrayExpression expr,
                IReadOnlyList<ParameterExpression> paramExprs, ILGenerator il, ref ClosureInfo closure, ParentFlags parent)
            {
                var arrayType = expr.Type;
                var elems = expr.Expressions;
                var elemType = arrayType.GetElementType();
                if (elemType == null)
                    return false;

                var arrVar = il.DeclareLocal(arrayType);

                var rank = arrayType.GetArrayRank();
                if (rank == 1) // one dimensional
                {
                    EmitLoadConstantInt(il, elems.Count);
                }
                else // multi dimensional
                {
                    for (var i = 0; i < elems.Count; i++)
                        if (!TryEmit(elems[i], paramExprs, il, ref closure, parent, i))
                            return false;

                    il.Emit(OpCodes.Newobj, arrayType.GetTypeInfo().DeclaredConstructors.GetFirst());
                    return true;
                }

                il.Emit(OpCodes.Newarr, elemType);
                il.Emit(OpCodes.Stloc, arrVar);

                var isElemOfValueType = elemType.IsValueType();

                for (int i = 0, n = elems.Count; i < n; i++)
                {
                    il.Emit(OpCodes.Ldloc, arrVar);
                    EmitLoadConstantInt(il, i);

                    // loading element address for later copying of value into it.
                    if (isElemOfValueType)
                        il.Emit(OpCodes.Ldelema, elemType);

                    if (!TryEmit(elems[i], paramExprs, il, ref closure, parent))
                        return false;

                    if (isElemOfValueType)
                        il.Emit(OpCodes.Stobj, elemType); // store element of value type by array element address
                    else
                        il.Emit(OpCodes.Stelem_Ref);
                }

                il.Emit(OpCodes.Ldloc, arrVar);
                return true;
            }

            private static bool TryEmitArrayIndex(Type exprType, ILGenerator il)
            {
                if (exprType.IsValueType())
                    il.Emit(OpCodes.Ldelem, exprType);
                else
                    il.Emit(OpCodes.Ldelem_Ref);
                return true;
            }

            private static bool EmitMemberInit(MemberInitExpression expr,
                IReadOnlyList<ParameterExpression> paramExprs, ILGenerator il, ref ClosureInfo closure, ParentFlags parent)
            {
                // todo: Use closureInfo Block to track the variable instead
                LocalBuilder valueVar = null;
                if (expr.Type.IsValueType())
                    valueVar = il.DeclareLocal(expr.Type);

                var newExpr = expr.NewExpression;
#if LIGHT_EXPRESSION
                if (newExpr == null)
                {
                    if (!TryEmit(expr.Expression, paramExprs, il, ref closure, parent/*, valueVar*/)) // todo: fix me
                        return false;
                }
                else
#endif
                {
                    var argExprs = newExpr.Arguments;
                    for (var i = 0; i < argExprs.Count; i++)
                        if (!TryEmit(argExprs[i], paramExprs, il, ref closure, parent, i))
                            return false;
                    if (!TryEmitNew(newExpr.Constructor, newExpr.Type, il, valueVar))
                        return false;
                }

                var bindings = expr.Bindings;
                for (var i = 0; i < bindings.Count; i++)
                {
                    var binding = bindings[i];
                    if (binding.BindingType != MemberBindingType.Assignment)
                        return false;

                    if (valueVar != null) // load local value address, to set its members
                        il.Emit(OpCodes.Ldloca, valueVar);
                    else
                        il.Emit(OpCodes.Dup); // duplicate member owner on stack

                    if (!TryEmit(((MemberAssignment)binding).Expression, paramExprs, il, ref closure, parent) ||
                        !EmitMemberAssign(il, binding.Member))
                        return false;
                }

                if (valueVar != null)
                    il.Emit(OpCodes.Ldloc, valueVar);
                return true;
            }

            private static bool EmitMemberAssign(ILGenerator il, MemberInfo member)
            {
                if (member is PropertyInfo prop)
                    return EmitMethodCall(il, prop.FindPropertySetMethod());

                if (!(member is FieldInfo field))
                    return false;

                il.Emit(field.IsStatic ? OpCodes.Stsfld : OpCodes.Stfld, field);
                return true;
            }

            private static bool TryEmitIncDecAssign(UnaryExpression expr,
                IReadOnlyList<ParameterExpression> paramExprs, ILGenerator il, ref ClosureInfo closure, ParentFlags parent)
            {
                LocalBuilder localVar;
                MemberExpression memberAccess;
                bool useLocalVar;

                var isVar = expr.Operand.NodeType == ExpressionType.Parameter;
                var usesResult = !parent.IgnoresResult();

                if (isVar)
                {
                    localVar = closure.GetDefinedLocalVarOrDefault((ParameterExpression)expr.Operand);

                    if (localVar == null)
                        return false;

                    memberAccess = null;
                    useLocalVar = true;

                    il.Emit(OpCodes.Ldloc, localVar);
                }
                else if (expr.Operand.NodeType == ExpressionType.MemberAccess)
                {
                    memberAccess = (MemberExpression)expr.Operand;

                    if (!TryEmitMemberAccess(memberAccess, paramExprs, il, ref closure, parent | ParentFlags.DupMemberOwner))
                        return false;

                    useLocalVar = (memberAccess.Expression != null) && (usesResult || memberAccess.Member is PropertyInfo);
                    localVar = useLocalVar ? il.DeclareLocal(expr.Operand.Type) : null;
                }
                else
                {
                    return false;
                }

                switch (expr.NodeType)
                {
                    case ExpressionType.PreIncrementAssign:
                        il.Emit(OpCodes.Ldc_I4_1);
                        il.Emit(OpCodes.Add);
                        StoreIncDecValue(il, usesResult, isVar, localVar);
                        break;

                    case ExpressionType.PostIncrementAssign:
                        StoreIncDecValue(il, usesResult, isVar, localVar);
                        il.Emit(OpCodes.Ldc_I4_1);
                        il.Emit(OpCodes.Add);
                        break;

                    case ExpressionType.PreDecrementAssign:
                        il.Emit(OpCodes.Ldc_I4_M1);
                        il.Emit(OpCodes.Add);
                        StoreIncDecValue(il, usesResult, isVar, localVar);
                        break;

                    case ExpressionType.PostDecrementAssign:
                        StoreIncDecValue(il, usesResult, isVar, localVar);
                        il.Emit(OpCodes.Ldc_I4_M1);
                        il.Emit(OpCodes.Add);
                        break;
                }

                if (isVar || (useLocalVar && !usesResult))
                    il.Emit(OpCodes.Stloc, localVar);

                if (isVar)
                    return true;

                if (useLocalVar && !usesResult)
                    il.Emit(OpCodes.Ldloc, localVar);

                if (!EmitMemberAssign(il, memberAccess.Member))
                    return false;

                if (useLocalVar && usesResult)
                    il.Emit(OpCodes.Ldloc, localVar);

                return true;
            }

            private static void StoreIncDecValue(ILGenerator il, bool usesResult, bool isVar, LocalBuilder localVar)
            {
                if (!usesResult)
                    return;

                if (isVar || (localVar == null))
                    il.Emit(OpCodes.Dup);
                else
                {
                    il.Emit(OpCodes.Stloc, localVar);
                    il.Emit(OpCodes.Ldloc, localVar);
                }
            }

            private static bool TryEmitAssign(BinaryExpression expr,
                IReadOnlyList<ParameterExpression> paramExprs, ILGenerator il, ref ClosureInfo closure, ParentFlags parent)
            {
                var exprType = expr.Type;
                var left = expr.Left;
                var right = expr.Right;
                var leftNodeType = expr.Left.NodeType;
                var nodeType = expr.NodeType;

                // if this assignment is part of a single body-less expression or the result of a block
                // we should put its result to the evaluation stack before the return, otherwise we are
                // somewhere inside the block, so we shouldn't return with the result
                var flags = parent & ~ParentFlags.IgnoreResult;
                switch (leftNodeType)
                {
                    case ExpressionType.Parameter:
                        var leftParamExpr = (ParameterExpression)left;
                        var paramIndex = paramExprs.GetFirstIndex(leftParamExpr);
                        var arithmeticNodeType = Tools.GetArithmeticFromArithmeticAssignOrSelf(nodeType);

                        if (paramIndex != -1)
                        {
                            // shift parameter index by one, because the first one will be closure
                            if (closure.HasClosure)
                                paramIndex += 1;

                            if (paramIndex >= byte.MaxValue)
                                return false;

                            if (leftParamExpr.IsByRef)
                                EmitLoadParamArg(il, paramIndex, false);

                            if (arithmeticNodeType == nodeType)
                            {
                                if (!TryEmit(right, paramExprs, il, ref closure, flags))
                                    return false;
                            }
                            else if (!TryEmitArithmetic(expr, arithmeticNodeType, paramExprs, il, ref closure, parent))
                                return false;

                            if ((parent & ParentFlags.IgnoreResult) == 0)
                                il.Emit(OpCodes.Dup); // duplicate value to assign and return

                            if (leftParamExpr.IsByRef)
                                EmitByRefStore(il, leftParamExpr.Type);
                            else
                                il.Emit(OpCodes.Starg_S, paramIndex);

                            return true;
                        }
                        else if (arithmeticNodeType != nodeType)
                        {
                            var localVar = closure.GetDefinedLocalVarOrDefault(leftParamExpr);
                            if (localVar != null)
                            {
                                if (!TryEmitArithmetic(expr, arithmeticNodeType, paramExprs, il, ref closure, parent))
                                    return false;

                                il.Emit(OpCodes.Stloc, localVar);
                                return true;
                            }
                        }

                        // if parameter isn't passed, then it is passed into some outer lambda or it is a local variable,
                        // so it should be loaded from closure or from the locals. Then the closure is null will be an invalid state.
                        if (!closure.IsClosureConstructed)
                            return false;

                        // if it's a local variable, then store the right value in it
                        var localVariable = closure.GetDefinedLocalVarOrDefault(leftParamExpr);
                        if (localVariable != null)
                        {
                            if (!TryEmit(right, paramExprs, il, ref closure, flags))
                                return false;

                            if ((right as ParameterExpression)?.IsByRef == true)
                                il.Emit(OpCodes.Ldind_I4);

                            if ((parent & ParentFlags.IgnoreResult) == 0) // if we have to push the result back, duplicate the right value
                                il.Emit(OpCodes.Dup);

                            il.Emit(OpCodes.Stloc, localVariable);
                            return true;
                        }

                        // check that it's a captured parameter by closure
                        var nonPassedParamIndex = closure.NonPassedParameters.GetFirstIndex(leftParamExpr);
                        if (nonPassedParamIndex == -1)
                            return false; // what??? no chance

                        var paramInClosureIndex = closure.Constants.Length + nonPassedParamIndex;

                        il.Emit(OpCodes.Ldarg_0); // closure is always a first argument

                        if ((parent & ParentFlags.IgnoreResult) == 0)
                        {
                            if (!TryEmit(right, paramExprs, il, ref closure, flags))
                                return false;

                            var valueVar = il.DeclareLocal(exprType); // store left value in variable
                            if (closure.ClosureFields != null)
                            {
                                il.Emit(OpCodes.Dup);
                                il.Emit(OpCodes.Stloc, valueVar);
                                il.Emit(OpCodes.Stfld, closure.ClosureFields[paramInClosureIndex]);
                                il.Emit(OpCodes.Ldloc, valueVar);
                            }
                            else
                            {
                                il.Emit(OpCodes.Stloc, valueVar);
                                il.Emit(OpCodes.Ldfld, ArrayClosure.ArrayField); // load array field
                                EmitLoadConstantInt(il, paramInClosureIndex); // load array item index
                                il.Emit(OpCodes.Ldloc, valueVar);
                                if (exprType.IsValueType())
                                    il.Emit(OpCodes.Box, exprType);
                                il.Emit(OpCodes.Stelem_Ref); // put the variable into array
                                il.Emit(OpCodes.Ldloc, valueVar);
                            }
                        }
                        else
                        {
                            var isArrayClosure = closure.ClosureFields == null;
                            if (isArrayClosure)
                            {
                                il.Emit(OpCodes.Ldfld, ArrayClosure.ArrayField); // load array field
                                EmitLoadConstantInt(il, paramInClosureIndex); // load array item index
                            }

                            if (!TryEmit(right, paramExprs, il, ref closure, flags))
                                return false;

                            if (isArrayClosure)
                            {
                                if (exprType.IsValueType())
                                    il.Emit(OpCodes.Box, exprType);
                                il.Emit(OpCodes.Stelem_Ref); // put the variable into array
                            }
                            else
                                il.Emit(OpCodes.Stfld, closure.ClosureFields[paramInClosureIndex]);
                        }

                        return true;

                    case ExpressionType.MemberAccess:
                        var assignFromLocalVar = right.NodeType == ExpressionType.Try;
                        var resultLocalVar = assignFromLocalVar ? il.DeclareLocal(right.Type) : null;

                        if (assignFromLocalVar)
                        {
                            if (!TryEmit(right, paramExprs, il, ref closure, ParentFlags.Empty))
                                return false;

                            il.Emit(OpCodes.Stloc, resultLocalVar);
                        }

                        var memberExpr = (MemberExpression)left;
                        var objExpr = memberExpr.Expression;
                        if (objExpr != null && !TryEmit(objExpr, paramExprs, il, ref closure, flags | ParentFlags.MemberAccess | ParentFlags.InstanceAccess))
                            return false;

                        if (assignFromLocalVar)
                            il.Emit(OpCodes.Ldloc, resultLocalVar);
                        else if (!TryEmit(right, paramExprs, il, ref closure, ParentFlags.Empty))
                            return false;

                        var member = memberExpr.Member;
                        if ((parent & ParentFlags.IgnoreResult) != 0)
                            return EmitMemberAssign(il, member);

                        il.Emit(OpCodes.Dup);

                        var rightVar = il.DeclareLocal(exprType); // store right value in variable
                        il.Emit(OpCodes.Stloc, rightVar);

                        if (!EmitMemberAssign(il, member))
                            return false;

                        il.Emit(OpCodes.Ldloc, rightVar);
                        return true;

                    case ExpressionType.Index:
                        var indexExpr = (IndexExpression)left;

                        var obj = indexExpr.Object;
                        if (obj != null && !TryEmit(obj, paramExprs, il, ref closure, flags))
                            return false;

                        var indexArgExprs = indexExpr.Arguments;
                        for (var i = 0; i < indexArgExprs.Count; i++)
                            if (!TryEmit(indexArgExprs[i], paramExprs, il, ref closure, flags, i))
                                return false;

                        if (!TryEmit(right, paramExprs, il, ref closure, flags))
                            return false;

                        if ((parent & ParentFlags.IgnoreResult) != 0)
                            return TryEmitIndexAssign(indexExpr, obj?.Type, exprType, il);

                        var variable = il.DeclareLocal(exprType); // store value in variable to return
                        il.Emit(OpCodes.Dup);
                        il.Emit(OpCodes.Stloc, variable);

                        if (!TryEmitIndexAssign(indexExpr, obj?.Type, exprType, il))
                            return false;

                        il.Emit(OpCodes.Ldloc, variable);
                        return true;

                    default: // todo: not yet support assignment targets
                        return false;
                }
            }

            private static void EmitByRefStore(ILGenerator il, Type type)
            {
                if (type == typeof(int) || type == typeof(uint))
                    il.Emit(OpCodes.Stind_I4);
                else if (type == typeof(byte))
                    il.Emit(OpCodes.Stind_I1);
                else if (type == typeof(short) || type == typeof(ushort))
                    il.Emit(OpCodes.Stind_I2);
                else if (type == typeof(long) || type == typeof(ulong))
                    il.Emit(OpCodes.Stind_I8);
                else if (type == typeof(float))
                    il.Emit(OpCodes.Stind_R4);
                else if (type == typeof(double))
                    il.Emit(OpCodes.Stind_R8);
                else if (type == typeof(object))
                    il.Emit(OpCodes.Stind_Ref);
                else if (type == typeof(IntPtr) || type == typeof(UIntPtr))
                    il.Emit(OpCodes.Stind_I);
                else
                    il.Emit(OpCodes.Stobj, type);
            }

            private static bool TryEmitIndexAssign(IndexExpression indexExpr, Type instType, Type elementType, ILGenerator il)
            {
                if (indexExpr.Indexer != null)
                    return EmitMemberAssign(il, indexExpr.Indexer);

                if (indexExpr.Arguments.Count == 1) // one dimensional array
                {
                    if (elementType.IsValueType())
                        il.Emit(OpCodes.Stelem, elementType);
                    else
                        il.Emit(OpCodes.Stelem_Ref);
                    return true;
                }

                // multi dimensional array
                return EmitMethodCall(il, instType?.FindMethod("Set"));
            }

            private static bool TryEmitMethodCall(MethodCallExpression expr,
                IReadOnlyList<ParameterExpression> paramExprs, ILGenerator il, ref ClosureInfo closure, ParentFlags parent)
            {
                var flags = parent & ~ParentFlags.IgnoreResult | ParentFlags.Call;

                var objExpr = expr.Object;
                if (objExpr != null)
                {
                    if (!TryEmit(objExpr, paramExprs, il, ref closure, flags | ParentFlags.InstanceAccess))
                        return false;

                    var objType = objExpr.Type;
                    if (objType.IsValueType() && objExpr.NodeType != ExpressionType.Parameter && !closure.LastEmitIsAddress)
                        DeclareAndLoadLocalVariable(il, objType);
                }

                IReadOnlyList<Expression> argExprs = expr.Arguments;
                if (argExprs.Count != 0)
                {
                    var args = expr.Method.GetParameters();
                    for (var i = 0; i < argExprs.Count; i++)
                    {
                        var byRefIndex = args[i].ParameterType.IsByRef ? i : -1;
                        if (!TryEmit(argExprs[i], paramExprs, il, ref closure, flags, byRefIndex))
                            return false;
                    }
                }

                if (expr.Method.IsVirtual && objExpr?.Type.IsValueType() == true)
                    il.Emit(OpCodes.Constrained, objExpr.Type);

                closure.LastEmitIsAddress = false;
                return EmitMethodCall(il, expr.Method, parent);
            }

            private static bool TryEmitMemberAccess(MemberExpression expr,
                IReadOnlyList<ParameterExpression> paramExprs, ILGenerator il, ref ClosureInfo closure, ParentFlags parent)
            {
                var prop = expr.Member as PropertyInfo;

                var instanceExpr = expr.Expression;
                if (instanceExpr != null)
                {
                    if (!TryEmit(instanceExpr, paramExprs, il, ref closure,
                        ~ParentFlags.IgnoreResult & ~ParentFlags.DupMemberOwner &
                        (parent | (prop != null ? ParentFlags.Call : parent) | ParentFlags.MemberAccess | ParentFlags.InstanceAccess)))
                        return false;

                    if ((parent & ParentFlags.DupMemberOwner) != 0)
                        il.Emit(OpCodes.Dup);
                }

                if (prop != null)
                {
                    // Value type special treatment to load address of value instance in order to access a field or call a method.
                    // Parameter should be excluded because it already loads an address via `LDARGA`, and you don't need to.
                    // And for field access no need to load address, cause the field stored on stack nearby
                    if (!closure.LastEmitIsAddress && instanceExpr != null &&
                        instanceExpr.NodeType != ExpressionType.Parameter &&
                        instanceExpr.Type.IsValueType())
                    {
                        DeclareAndLoadLocalVariable(il, instanceExpr.Type);
                    }

                    closure.LastEmitIsAddress = false;
                    return EmitMethodCall(il, prop.FindPropertyGetMethod());
                }

                if (!(expr.Member is FieldInfo field))
                    return false;

                if (field.IsStatic)
                {
                    if (field.IsLiteral)
                        TryEmitConstant(null, field.FieldType, field.GetValue(null), il, ref closure);
                    else
                        il.Emit(OpCodes.Ldsfld, field);
                }
                else
                {
                    closure.LastEmitIsAddress = field.FieldType.IsValueType() && (parent & ParentFlags.InstanceAccess) != 0;
                    il.Emit(closure.LastEmitIsAddress ? OpCodes.Ldflda : OpCodes.Ldfld, field);
                }
                return true;
            }

            // ReSharper disable once FunctionComplexityOverflow
            private static bool TryEmitNestedLambda(LambdaExpression lambdaExpr,
                IReadOnlyList<ParameterExpression> paramExprs, ILGenerator il, ref ClosureInfo closure)
            {
                // First, find in closed compiled lambdas the one corresponding to the current lambda expression.
                // Situation with not found lambda is not possible/exceptional,
                // it means that we somehow skipped the lambda expression while collecting closure info.
                var outerNestedLambdaExprs = closure.NestedLambdaExprs;
                var outerNestedLambdaIndex = outerNestedLambdaExprs.GetFirstIndex(lambdaExpr);
                if (outerNestedLambdaIndex == -1)
                    return false;

                var nestedLambdaInfo = closure.NestedLambdas[outerNestedLambdaIndex];
                var nestedLambda = nestedLambdaInfo.Lambda;

                var outerConstants = closure.Constants;
                var outerNonPassedParams = closure.NonPassedParameters;

                // Load compiled lambda on stack counting the offset
                outerNestedLambdaIndex += outerConstants.Length + outerNonPassedParams.Length;

                if (!LoadClosureFieldOrItem(ref closure, il, outerNestedLambdaIndex, nestedLambda.GetType()))
                    return false;

                // If lambda does not use any outer parameters to be set in closure, then we're done
                var nestedClosureInfo = nestedLambdaInfo.ClosureInfo;
                if (!nestedClosureInfo.HasClosure)
                    return true;

                // If closure is array-based, the create a new array to represent closure for the nested lambda
                var isNestedArrayClosure = nestedClosureInfo.ClosureFields == null;
                if (isNestedArrayClosure)
                {
                    EmitLoadConstantInt(il, nestedClosureInfo.ClosedItemCount); // size of array
                    il.Emit(OpCodes.Newarr, typeof(object));
                }

                // Load constants on stack
                var nestedConstants = nestedClosureInfo.Constants;
                if (nestedConstants.Length != 0)
                {
                    for (var nestedConstIndex = 0; nestedConstIndex < nestedConstants.Length; nestedConstIndex++)
                    {
                        var nestedConstant = nestedConstants[nestedConstIndex];

                        // Find constant index in the outer closure
                        var outerConstIndex = outerConstants.GetFirstIndex(nestedConstant);
                        if (outerConstIndex == -1)
                            return false; // some error is here

                        if (isNestedArrayClosure)
                        {
                            // Duplicate nested array on stack to store the item, and load index to where to store
                            il.Emit(OpCodes.Dup);
                            EmitLoadConstantInt(il, nestedConstIndex);
                        }

                        if (!LoadClosureFieldOrItem(ref closure, il, outerConstIndex, nestedConstant.Type))
                            return false;

                        if (isNestedArrayClosure)
                        {
                            if (nestedConstant.Type.IsValueType())
                                il.Emit(OpCodes.Box, nestedConstant.Type);
                            il.Emit(OpCodes.Stelem_Ref); // store the item in array
                        }
                    }
                }

                // Load used and closed parameter values on stack
                var nestedNonPassedParams = nestedClosureInfo.NonPassedParameters;
                for (var nestedParamIndex = 0; nestedParamIndex < nestedNonPassedParams.Length; nestedParamIndex++)
                {
                    var nestedUsedParam = nestedNonPassedParams[nestedParamIndex];

                    Type nestedUsedParamType = null;
                    if (isNestedArrayClosure)
                    {
                        // get a parameter type for the later
                        nestedUsedParamType = nestedUsedParam.Type;

                        // Duplicate nested array on stack to store the item, and load index to where to store
                        il.Emit(OpCodes.Dup);
                        EmitLoadConstantInt(il, nestedConstants.Length + nestedParamIndex);
                    }

                    var paramIndex = paramExprs.GetFirstIndex(nestedUsedParam);
                    if (paramIndex != -1) // load parameter from input params
                    {
                        // +1 is set cause of added first closure argument
                        EmitLoadParamArg(il, 1 + paramIndex, false);
                    }
                    else // load parameter from outer closure or from the locals
                    {
                        if (outerNonPassedParams.Length == 0)
                            return false; // impossible, better to throw?

                        var variable = closure.GetDefinedLocalVarOrDefault(nestedUsedParam);
                        if (variable != null) // it's a local variable
                        {
                            il.Emit(OpCodes.Ldloc, variable);
                        }
                        else // it's a parameter from outer closure
                        {
                            var outerParamIndex = outerNonPassedParams.GetFirstIndex(nestedUsedParam);
                            if (outerParamIndex == -1 ||
                                !LoadClosureFieldOrItem(ref closure, il, outerConstants.Length + outerParamIndex,
                                    nestedUsedParamType, nestedUsedParam))
                                return false;
                        }
                    }

                    if (isNestedArrayClosure)
                    {
                        if (nestedUsedParamType.IsValueType())
                            il.Emit(OpCodes.Box, nestedUsedParamType);

                        il.Emit(OpCodes.Stelem_Ref); // store the item in array
                    }
                }

                // Load nested lambdas on stack
                var nestedLambdaExprs = closure.NestedLambdaExprs;
                var nestedNestedLambdaExprs = nestedClosureInfo.NestedLambdaExprs;
                var nestedNestedLambdas = nestedClosureInfo.NestedLambdas;
                if (nestedNestedLambdas.Length != 0)
                {
                    for (var nestedLambdaIndex = 0; nestedLambdaIndex < nestedNestedLambdas.Length; nestedLambdaIndex++)
                    {
                        var nestedNestedLambda = nestedNestedLambdas[nestedLambdaIndex];

                        // Find constant index in the outer closure
                        var outerLambdaIndex =
                            nestedLambdaExprs.GetFirstIndex(nestedNestedLambdaExprs[nestedLambdaIndex]);
                        if (outerLambdaIndex == -1)
                            return false; // some error is here

                        // Duplicate nested array on stack to store the item, and load index to where to store
                        if (isNestedArrayClosure)
                        {
                            il.Emit(OpCodes.Dup);
                            EmitLoadConstantInt(il, nestedConstants.Length + nestedNonPassedParams.Length + nestedLambdaIndex);
                        }

                        outerLambdaIndex += outerConstants.Length + outerNonPassedParams.Length;

                        var nestedNestedLambdaType = nestedNestedLambda.Lambda.GetType();
                        if (!LoadClosureFieldOrItem(ref closure, il, outerLambdaIndex, nestedNestedLambdaType))
                            return false;

                        if (isNestedArrayClosure)
                            il.Emit(OpCodes.Stelem_Ref); // store the item in array
                    }
                }

                // Create nested closure object composed of all constants, params, lambdas loaded on stack
                il.Emit(OpCodes.Newobj, isNestedArrayClosure
                    ? ArrayClosure.Constructor
                    : nestedClosureInfo.ClosureType.GetTypeInfo().DeclaredConstructors.GetFirst());

                return EmitMethodCall(il, GetCurryClosureMethod(nestedLambda, nestedLambdaInfo.IsAction));
            }

            private static MethodInfo GetCurryClosureMethod(object lambda, bool isAction)
            {
                var lambdaTypeArgs = lambda.GetType().GetTypeInfo().GenericTypeArguments;
                return isAction
                    ? CurryClosureActions.Methods[lambdaTypeArgs.Length - 1].MakeGenericMethod(lambdaTypeArgs)
                    : CurryClosureFuncs.Methods[lambdaTypeArgs.Length - 2].MakeGenericMethod(lambdaTypeArgs);
            }

            private static bool TryEmitInvoke(InvocationExpression expr,
                IReadOnlyList<ParameterExpression> paramExprs, ILGenerator il, ref ClosureInfo closure, ParentFlags parent)
            {
                // optimization #138: we inline the invoked lambda body (only for lambdas without arguments) 
                var lambda = expr.Expression;
                if (lambda is LambdaExpression lambdaExpr && lambdaExpr.Parameters.Count == 0)
                    return TryEmit(lambdaExpr.Body, paramExprs, il, ref closure, parent);

                if (!TryEmit(lambda, paramExprs, il, ref closure, parent & ~ParentFlags.IgnoreResult))
                    return false;

                var argExprs = expr.Arguments;
                for (var i = 0; i < argExprs.Count; i++)
                {
                    var byRefIndex = argExprs[i].Type.IsByRef ? i : -1;
                    if (!TryEmit(argExprs[i], paramExprs, il, ref closure, parent & ~ParentFlags.IgnoreResult & ~ParentFlags.InstanceAccess, byRefIndex))
                        return false;
                }

                return EmitMethodCall(il, lambda.Type.FindDelegateInvokeMethod(), parent);
            }

            private static bool TryEmitSwitch(SwitchExpression expr,
                IReadOnlyList<ParameterExpression> paramExprs, ILGenerator il, ref ClosureInfo closure, ParentFlags parent)
            {
                // todo:
                //- use switch statement for int comparison (if int difference is less or equal 3 -> use IL switch)
                //- TryEmitComparison should not emit "CEQ" so we could use Beq_S instead of Brtrue_S (not always possible (nullable))
                //- if switch SwitchValue is a nullable parameter, we should call getValue only once and store the result.
                //- use comparison methods (when defined)

                var endLabel = il.DefineLabel();
                var labels = new Label[expr.Cases.Count];
                for (var index = 0; index < expr.Cases.Count; index++)
                {
                    var switchCase = expr.Cases[index];
                    labels[index] = il.DefineLabel();

                    foreach (var switchCaseTestValue in switchCase.TestValues)
                    {
                        if (!TryEmitComparison(expr.SwitchValue, switchCaseTestValue, ExpressionType.Equal, paramExprs, il,
                            ref closure, parent))
                            return false;
                        il.Emit(OpCodes.Brtrue, labels[index]);
                    }
                }

                if (expr.DefaultBody != null)
                {
                    if (!TryEmit(expr.DefaultBody, paramExprs, il, ref closure, parent))
                        return false;
                    il.Emit(OpCodes.Br, endLabel);
                }

                for (var index = 0; index < expr.Cases.Count; index++)
                {
                    var switchCase = expr.Cases[index];
                    il.MarkLabel(labels[index]);
                    if (!TryEmit(switchCase.Body, paramExprs, il, ref closure, parent))
                        return false;

                    if (index != expr.Cases.Count - 1)
                        il.Emit(OpCodes.Br, endLabel);
                }

                il.MarkLabel(endLabel);

                return true;
            }

            private static bool TryEmitComparison(Expression exprLeft, Expression exprRight, ExpressionType expressionType,
                IReadOnlyList<ParameterExpression> paramExprs, ILGenerator il, ref ClosureInfo closure, ParentFlags parent)
            {
                var leftOpType = exprLeft.Type;
                var leftIsNullable = leftOpType.IsNullable();
                var rightOpType = exprRight.Type;
                if (exprRight is ConstantExpression c && c.Value == null && exprRight.Type == typeof(object))
                    rightOpType = leftOpType;

                LocalBuilder lVar = null, rVar = null;
                if (!TryEmit(exprLeft, paramExprs, il, ref closure, parent & ~ParentFlags.IgnoreResult & ~ParentFlags.InstanceAccess))
                    return false;

                if (leftIsNullable)
                {
                    lVar = DeclareAndLoadLocalVariable(il, leftOpType);
                    if (!EmitMethodCall(il, leftOpType.FindNullableGetValueOrDefaultMethod()))
                        return false;
                    leftOpType = Nullable.GetUnderlyingType(leftOpType);
                }

                if (!TryEmit(exprRight, paramExprs, il, ref closure, parent & ~ParentFlags.IgnoreResult & ~ParentFlags.InstanceAccess))
                    return false;

                if (leftOpType != rightOpType)
                {
                    if (leftOpType.IsClass() && rightOpType.IsClass() &&
                        (leftOpType == typeof(object) || rightOpType == typeof(object)))
                    {
                        if (expressionType == ExpressionType.Equal)
                        {
                            il.Emit(OpCodes.Ceq);
                            if ((parent & ParentFlags.IgnoreResult) > 0)
                                il.Emit(OpCodes.Pop);
                        }
                        else if (expressionType == ExpressionType.NotEqual)
                        {
                            il.Emit(OpCodes.Ceq);
                            il.Emit(OpCodes.Ldc_I4_0);
                            il.Emit(OpCodes.Ceq);
                        }
                        else
                            return false;

                        if ((parent & ParentFlags.IgnoreResult) > 0)
                            il.Emit(OpCodes.Pop);

                        return true;
                    }
                }

                if (rightOpType.IsNullable())
                {
                    rVar = DeclareAndLoadLocalVariable(il, rightOpType);
                    if (!EmitMethodCall(il, rightOpType.FindNullableGetValueOrDefaultMethod()))
                        return false;
                    // ReSharper disable once AssignNullToNotNullAttribute
                    rightOpType = Nullable.GetUnderlyingType(rightOpType);
                }

                var leftOpTypeInfo = leftOpType.GetTypeInfo();
                if (!leftOpTypeInfo.IsPrimitive && !leftOpTypeInfo.IsEnum)
                {
                    var methodName
                        = expressionType == ExpressionType.Equal ? "op_Equality"
                        : expressionType == ExpressionType.NotEqual ? "op_Inequality"
                        : expressionType == ExpressionType.GreaterThan ? "op_GreaterThan"
                        : expressionType == ExpressionType.GreaterThanOrEqual ? "op_GreaterThanOrEqual"
                        : expressionType == ExpressionType.LessThan ? "op_LessThan"
                        : expressionType == ExpressionType.LessThanOrEqual ? "op_LessThanOrEqual"
                        : null;

                    if (methodName == null)
                        return false;

                    // todo: for now handling only parameters of the same type
                    foreach (var m in leftOpTypeInfo.DeclaredMethods)
                        if (m.IsStatic && m.Name == methodName && IsComparisonOperatorSignature(m.GetParameters(), leftOpType))
                            return EmitMethodCall(il, m);

                    if (expressionType != ExpressionType.Equal && expressionType != ExpressionType.NotEqual)
                        return false;

                    EmitMethodCall(il, _objectEqualsMethod);
                    if (expressionType == ExpressionType.NotEqual) // invert result for not equal
                    {
                        il.Emit(OpCodes.Ldc_I4_0);
                        il.Emit(OpCodes.Ceq);
                    }

                    if (leftIsNullable)
                        goto nullCheck;

                    if ((parent & ParentFlags.IgnoreResult) > 0)
                        il.Emit(OpCodes.Pop);

                    return true;
                }

                // handle primitives comparison
                switch (expressionType)
                {
                    case ExpressionType.Equal:
                        il.Emit(OpCodes.Ceq);
                        break;

                    case ExpressionType.NotEqual:
                        il.Emit(OpCodes.Ceq);
                        il.Emit(OpCodes.Ldc_I4_0);
                        il.Emit(OpCodes.Ceq);
                        break;

                    case ExpressionType.LessThan:
                        il.Emit(OpCodes.Clt);
                        break;

                    case ExpressionType.GreaterThan:
                        il.Emit(OpCodes.Cgt);
                        break;

                    case ExpressionType.GreaterThanOrEqual:
                    case ExpressionType.LessThanOrEqual:
                        var ifTrueLabel = il.DefineLabel();
                        if (rightOpType == typeof(uint) || rightOpType == typeof(ulong) ||
                            rightOpType == typeof(ushort) || rightOpType == typeof(byte))
                            il.Emit(expressionType == ExpressionType.GreaterThanOrEqual ? OpCodes.Bge_Un_S : OpCodes.Ble_Un_S, ifTrueLabel);
                        else
                            il.Emit(expressionType == ExpressionType.GreaterThanOrEqual ? OpCodes.Bge_S : OpCodes.Ble_S, ifTrueLabel);

                        il.Emit(OpCodes.Ldc_I4_0);
                        var doneLabel = il.DefineLabel();
                        il.Emit(OpCodes.Br_S, doneLabel);

                        il.MarkLabel(ifTrueLabel);
                        il.Emit(OpCodes.Ldc_I4_1);

                        il.MarkLabel(doneLabel);
                        break;

                    default:
                        return false;
                }

            nullCheck:
                if (leftIsNullable)
                {
                    il.Emit(OpCodes.Ldloca_S, lVar);
                    if (!EmitMethodCall(il, exprLeft.Type.FindNullableHasValueGetterMethod()))
                        return false;
                    // ReSharper disable once AssignNullToNotNullAttribute
                    il.Emit(OpCodes.Ldloca_S, rVar);
                    if (!EmitMethodCall(il, exprLeft.Type.FindNullableHasValueGetterMethod()))
                        return false;

                    switch (expressionType)
                    {
                        case ExpressionType.Equal:
                            il.Emit(OpCodes.Ceq); // compare both HasValue calls
                            il.Emit(OpCodes.And); // both results need to be true
                            break;

                        case ExpressionType.NotEqual:
                            il.Emit(OpCodes.Ceq);
                            il.Emit(OpCodes.Ldc_I4_0);
                            il.Emit(OpCodes.Ceq);
                            il.Emit(OpCodes.Or);
                            break;

                        case ExpressionType.LessThan:
                        case ExpressionType.GreaterThan:
                        case ExpressionType.LessThanOrEqual:
                        case ExpressionType.GreaterThanOrEqual:
                            il.Emit(OpCodes.Ceq);
                            il.Emit(OpCodes.Ldc_I4_1);
                            il.Emit(OpCodes.Ceq);
                            il.Emit(OpCodes.And);
                            break;

                        default:
                            return false;
                    }
                }

                if ((parent & ParentFlags.IgnoreResult) > 0)
                    il.Emit(OpCodes.Pop);

                return true;
            }

            private static bool IsComparisonOperatorSignature(ParameterInfo[] pars, Type t) =>
                pars.Length == 2 && pars[0].ParameterType == t && pars[1].ParameterType == t;

            private static bool TryEmitArithmetic(BinaryExpression expr, ExpressionType exprNodeType,
                IReadOnlyList<ParameterExpression> paramExprs, ILGenerator il, ref ClosureInfo closure,
                ParentFlags parent)
            {
                var flags = parent & ~ParentFlags.IgnoreResult & ~ParentFlags.InstanceCall | ParentFlags.Arithmetic;

                var leftNoValueLabel = default(Label);
                var leftExpr = expr.Left;
                var lefType = leftExpr.Type;
                var leftIsNullable = lefType.IsNullable();
                if (leftIsNullable)
                {
                    leftNoValueLabel = il.DefineLabel();
                    if (!TryEmit(leftExpr, paramExprs, il, ref closure, flags | ParentFlags.InstanceCall))
                        return false;

                    if (!closure.LastEmitIsAddress)
                        DeclareAndLoadLocalVariable(il, lefType);

                    il.Emit(OpCodes.Dup);
                    EmitMethodCall(il, lefType.FindNullableHasValueGetterMethod());

                    il.Emit(OpCodes.Brfalse, leftNoValueLabel);
                    EmitMethodCall(il, lefType.FindNullableGetValueOrDefaultMethod());
                }
                else if (!TryEmit(leftExpr, paramExprs, il, ref closure, flags))
                    return false;

                var rightNoValueLabel = default(Label);
                var rightExpr = expr.Right;
                var rightType = rightExpr.Type;
                var rightIsNullable = rightType.IsNullable();
                if (rightIsNullable)
                {
                    rightNoValueLabel = il.DefineLabel();
                    if (!TryEmit(rightExpr, paramExprs, il, ref closure, flags | ParentFlags.InstanceCall))
                        return false;

                    if (!closure.LastEmitIsAddress)
                        DeclareAndLoadLocalVariable(il, rightType);

                    il.Emit(OpCodes.Dup);
                    EmitMethodCall(il, rightType.FindNullableHasValueGetterMethod());

                    il.Emit(OpCodes.Brfalse, rightNoValueLabel);
                    EmitMethodCall(il, rightType.FindNullableGetValueOrDefaultMethod());
                }
                else if (!TryEmit(rightExpr, paramExprs, il, ref closure, flags))
                    return false;

                var exprType = expr.Type;
                if (!TryEmitArithmeticOperation(expr, exprNodeType, exprType, il))
                    return false;

                if (leftIsNullable || rightIsNullable)
                {
                    var valueLabel = il.DefineLabel();
                    il.Emit(OpCodes.Br, valueLabel);

                    if (rightIsNullable)
                        il.MarkLabel(rightNoValueLabel);
                    il.Emit(OpCodes.Pop);

                    if (leftIsNullable)
                        il.MarkLabel(leftNoValueLabel);
                    il.Emit(OpCodes.Pop);

                    if (exprType.IsNullable())
                    {
                        var endL = il.DefineLabel();
                        var loc = InitValueTypeVariable(il, exprType);
                        il.Emit(OpCodes.Ldloc_S, loc);
                        il.Emit(OpCodes.Br_S, endL);
                        il.MarkLabel(valueLabel);
                        il.Emit(OpCodes.Newobj, exprType.GetTypeInfo().DeclaredConstructors.GetFirst());
                        il.MarkLabel(endL);
                    }
                    else
                    {
                        il.Emit(OpCodes.Ldc_I4_0);
                        il.MarkLabel(valueLabel);
                    }
                }

                return true;
            }

            private static bool TryEmitArithmeticOperation(BinaryExpression expr,
                ExpressionType exprNodeType, Type exprType, ILGenerator il)
            {
                if (!exprType.IsPrimitive())
                {
                    if (exprType.IsNullable())
                        exprType = Nullable.GetUnderlyingType(exprType);

                    if (!exprType.IsPrimitive())
                    {
                        MethodInfo method = null;
                        if (exprType == typeof(string))
                        {
                            var paraType = typeof(string);
                            if (expr.Left.Type != expr.Right.Type || expr.Left.Type != typeof(string))
                                paraType = typeof(object);

                            foreach (var m in typeof(string).GetTypeInfo().DeclaredMethods)
                                if (m.Name == "Concat" && m.GetParameters().Length == 2 &&
                                    m.GetParameters()[0].ParameterType == paraType)
                                {
                                    method = m;
                                    break;
                                }
                        }
                        else
                        {
                            var methodName
                                = exprNodeType == ExpressionType.Add ? "op_Addition"
                                : exprNodeType == ExpressionType.AddChecked ? "op_Addition"
                                : exprNodeType == ExpressionType.Subtract ? "op_Subtraction"
                                : exprNodeType == ExpressionType.SubtractChecked ? "op_Subtraction"
                                : exprNodeType == ExpressionType.Multiply ? "op_Multiply"
                                : exprNodeType == ExpressionType.MultiplyChecked ? "op_Multiply"
                                : exprNodeType == ExpressionType.Divide ? "op_Division"
                                : exprNodeType == ExpressionType.Modulo ? "op_Modulus"
                                : null;

                            if (methodName != null)
                                method = exprType.FindMethod(methodName);
                        }
                        return method != null && EmitMethodCall(il, method);
                    }
                }

                switch (exprNodeType)
                {
                    case ExpressionType.Add:
                    case ExpressionType.AddAssign:
                        il.Emit(OpCodes.Add);
                        return true;

                    case ExpressionType.AddChecked:
                    case ExpressionType.AddAssignChecked:
                        il.Emit(exprType.IsUnsigned() ? OpCodes.Add_Ovf_Un : OpCodes.Add_Ovf);
                        return true;

                    case ExpressionType.Subtract:
                    case ExpressionType.SubtractAssign:
                        il.Emit(OpCodes.Sub);
                        return true;

                    case ExpressionType.SubtractChecked:
                    case ExpressionType.SubtractAssignChecked:
                        il.Emit(exprType.IsUnsigned() ? OpCodes.Sub_Ovf_Un : OpCodes.Sub_Ovf);
                        return true;

                    case ExpressionType.Multiply:
                    case ExpressionType.MultiplyAssign:
                        il.Emit(OpCodes.Mul);
                        return true;

                    case ExpressionType.MultiplyChecked:
                    case ExpressionType.MultiplyAssignChecked:
                        il.Emit(exprType.IsUnsigned() ? OpCodes.Mul_Ovf_Un : OpCodes.Mul_Ovf);
                        return true;

                    case ExpressionType.Divide:
                    case ExpressionType.DivideAssign:
                        il.Emit(OpCodes.Div);
                        return true;

                    case ExpressionType.Modulo:
                    case ExpressionType.ModuloAssign:
                        il.Emit(OpCodes.Rem);
                        return true;

                    case ExpressionType.And:
                    case ExpressionType.AndAssign:
                        il.Emit(OpCodes.And);
                        return true;

                    case ExpressionType.Or:
                    case ExpressionType.OrAssign:
                        il.Emit(OpCodes.Or);
                        return true;

                    case ExpressionType.ExclusiveOr:
                    case ExpressionType.ExclusiveOrAssign:
                        il.Emit(OpCodes.Xor);
                        return true;

                    case ExpressionType.LeftShift:
                    case ExpressionType.LeftShiftAssign:
                        il.Emit(OpCodes.Shl);
                        return true;

                    case ExpressionType.RightShift:
                    case ExpressionType.RightShiftAssign:
                        il.Emit(OpCodes.Shr);
                        return true;

                    case ExpressionType.Power:
                        return EmitMethodCall(il, typeof(Math).FindMethod("Pow"));
                }

                return false;
            }

            private static bool TryEmitLogicalOperator(BinaryExpression expr,
                IReadOnlyList<ParameterExpression> paramExprs, ILGenerator il, ref ClosureInfo closure, ParentFlags parent)
            {
                if (!TryEmit(expr.Left, paramExprs, il, ref closure, parent))
                    return false;

                var labelSkipRight = il.DefineLabel();
                il.Emit(expr.NodeType == ExpressionType.AndAlso ? OpCodes.Brfalse : OpCodes.Brtrue, labelSkipRight);

                if (!TryEmit(expr.Right, paramExprs, il, ref closure, parent))
                    return false;

                var labelDone = il.DefineLabel();
                il.Emit(OpCodes.Br, labelDone);

                il.MarkLabel(labelSkipRight); // label the second branch
                il.Emit(expr.NodeType == ExpressionType.AndAlso ? OpCodes.Ldc_I4_0 : OpCodes.Ldc_I4_1);
                il.MarkLabel(labelDone);

                return true;
            }

            private static bool TryEmitConditional(ConditionalExpression expr,
                IReadOnlyList<ParameterExpression> paramExprs, ILGenerator il, ref ClosureInfo closure, ParentFlags parent)
            {
                var testExpr = expr.Test;
                var usedInverted = false;

                // optimization: special handling of comparing with null
                if (testExpr is BinaryExpression b &&
                    ((testExpr.NodeType == ExpressionType.Equal || testExpr.NodeType == ExpressionType.NotEqual) &&
                     !(b.Left.Type.IsNullable() || b.Right.Type.IsNullable()) &&
                      b.Right is ConstantExpression r && r.Value == null
                    ? TryEmit(b.Left, paramExprs, il, ref closure, parent & ~ParentFlags.IgnoreResult)
                    : b.Left is ConstantExpression l && l.Value == null &&
                      TryEmit(b.Right, paramExprs, il, ref closure, parent & ~ParentFlags.IgnoreResult)))
                {
                    usedInverted = true;
                }
                else if (!TryEmit(testExpr, paramExprs, il, ref closure, parent & ~ParentFlags.IgnoreResult))
                    return false;

                var labelIfFalse = il.DefineLabel();
                il.Emit(usedInverted && testExpr.NodeType == ExpressionType.Equal ? OpCodes.Brtrue : OpCodes.Brfalse, labelIfFalse);

                var ifTrueExpr = expr.IfTrue;
                if (!TryEmit(ifTrueExpr, paramExprs, il, ref closure, parent & ParentFlags.IgnoreResult))
                    return false;

                var ifFalseExpr = expr.IfFalse;
                if ((ifFalseExpr.NodeType == ExpressionType.Default) && (ifFalseExpr.Type == typeof(void)))
                {
                    il.MarkLabel(labelIfFalse);
                    return true;
                }

                var labelDone = il.DefineLabel();

                il.Emit(OpCodes.Br, labelDone);

                il.MarkLabel(labelIfFalse);
                if (!TryEmit(ifFalseExpr, paramExprs, il, ref closure, parent & ParentFlags.IgnoreResult))
                    return false;

                il.MarkLabel(labelDone);
                return true;
            }

            private static bool EmitMethodCall(ILGenerator il, MethodInfo method, ParentFlags parent = ParentFlags.Empty)
            {
                if (method == null)
                    return false;

                il.Emit(method.IsVirtual ? OpCodes.Callvirt : OpCodes.Call, method);

                if (parent.IgnoresResult() && method.ReturnType != typeof(void))
                    il.Emit(OpCodes.Pop);
                return true;
            }

            private static void EmitLoadConstantInt(ILGenerator il, int i)
            {
                switch (i)
                {
                    case -1:
                        il.Emit(OpCodes.Ldc_I4_M1);
                        break;
                    case 0:
                        il.Emit(OpCodes.Ldc_I4_0);
                        break;
                    case 1:
                        il.Emit(OpCodes.Ldc_I4_1);
                        break;
                    case 2:
                        il.Emit(OpCodes.Ldc_I4_2);
                        break;
                    case 3:
                        il.Emit(OpCodes.Ldc_I4_3);
                        break;
                    case 4:
                        il.Emit(OpCodes.Ldc_I4_4);
                        break;
                    case 5:
                        il.Emit(OpCodes.Ldc_I4_5);
                        break;
                    case 6:
                        il.Emit(OpCodes.Ldc_I4_6);
                        break;
                    case 7:
                        il.Emit(OpCodes.Ldc_I4_7);
                        break;
                    case 8:
                        il.Emit(OpCodes.Ldc_I4_8);
                        break;
                    case int n when (n > -129 && n < 128):
                        il.Emit(OpCodes.Ldc_I4_S, (sbyte)i);
                        break;
                    default:
                        il.Emit(OpCodes.Ldc_I4, i);
                        break;
                }
            }
        }
    }

    // Helpers targeting the performance. Extensions method names may be a bit funny (non standard), 
    // in order to prevent conflicts with YOUR helpers with standard names
    internal static class Tools
    {
        internal static bool IsValueType(this Type type) => type.GetTypeInfo().IsValueType;
        internal static bool IsPrimitive(this Type type) => type.GetTypeInfo().IsPrimitive;
        internal static bool IsClass(this Type type) => type.GetTypeInfo().IsClass;

        internal static bool IsUnsigned(this Type type) =>
            type == typeof(byte) || type == typeof(ushort) || type == typeof(uint) || type == typeof(ulong);

        internal static bool IsNullable(this Type type) =>
            type.GetTypeInfo().IsGenericType && type.GetTypeInfo().GetGenericTypeDefinition() == typeof(Nullable<>);

#if LIGHT_EXPRESSION
        internal static PropertyInfo FindProperty(this Type type, string propertyName) =>
            type.GetTypeInfo().GetDeclaredProperty(propertyName);

        internal static FieldInfo FindField(this Type type, string fieldName) =>
            type.GetTypeInfo().GetDeclaredField(fieldName);
#endif
        internal static MethodInfo FindMethod(this Type type, string methodName) =>
            type.GetTypeInfo().GetDeclaredMethod(methodName);

        internal static MethodInfo FindDelegateInvokeMethod(this Type type) =>
            type.FindMethod("Invoke");

        internal static MethodInfo FindNullableGetValueOrDefaultMethod(this Type type)
        {
            foreach (var m in type.GetTypeInfo().GetDeclaredMethods("GetValueOrDefault"))
                if (m.GetParameters().Length == 0)
                    return m;
            return null;
        }

        internal static MethodInfo FindValueGetterMethod(this Type type) =>
            type.FindMethod("get_Value");

        internal static MethodInfo FindNullableHasValueGetterMethod(this Type type) =>
            type.FindMethod("get_HasValue");

        internal static MethodInfo FindPropertyGetMethod(this PropertyInfo prop) =>
            prop.DeclaringType.FindMethod("get_" + prop.Name);

        internal static MethodInfo FindPropertySetMethod(this PropertyInfo prop) =>
            prop.DeclaringType.FindMethod("set_" + prop.Name);

        internal static MethodInfo FindConvertOperator(this Type type, Type sourceType, Type targetType)
        {
            foreach (var m in type.GetTypeInfo().DeclaredMethods)
                if (m.IsStatic && m.ReturnType == targetType &&
                    (m.Name == "op_Implicit" || m.Name == "op_Explicit") &&
                    m.GetParameters()[0].ParameterType == sourceType)
                    return m;
            return null;
        }

        internal static ConstructorInfo FindSingleParamConstructor(this Type type, Type paramType)
        {
            foreach (var ctor in type.GetTypeInfo().DeclaredConstructors)
            {
                var parameters = ctor.GetParameters();
                if (parameters.Length == 1 && parameters[0].ParameterType == paramType)
                    return ctor;
            }
            return null;
        }

        // todo: test what is faster? Copy and inline switch? Switch in method? Ors in method?
        internal static ExpressionType GetArithmeticFromArithmeticAssignOrSelf(ExpressionType arithmetic)
        {
            switch (arithmetic)
            {
                case ExpressionType.AddAssign: return ExpressionType.Add;
                case ExpressionType.AddAssignChecked: return ExpressionType.AddChecked;
                case ExpressionType.SubtractAssign: return ExpressionType.Subtract;
                case ExpressionType.SubtractAssignChecked: return ExpressionType.SubtractChecked;
                case ExpressionType.MultiplyAssign: return ExpressionType.Multiply;
                case ExpressionType.MultiplyAssignChecked: return ExpressionType.MultiplyChecked;
                case ExpressionType.DivideAssign: return ExpressionType.Divide;
                case ExpressionType.ModuloAssign: return ExpressionType.Modulo;
                case ExpressionType.PowerAssign: return ExpressionType.Power;
                case ExpressionType.AndAssign: return ExpressionType.And;
                case ExpressionType.OrAssign: return ExpressionType.Or;
                case ExpressionType.ExclusiveOrAssign: return ExpressionType.ExclusiveOr;
                case ExpressionType.LeftShiftAssign: return ExpressionType.LeftShift;
                case ExpressionType.RightShiftAssign: return ExpressionType.RightShift;
                default: return arithmetic;
            }
        }

        public static T[] AsArray<T>(this IEnumerable<T> xs) =>
            xs is T[] array ? array : xs == null ? null : new List<T>(xs).ToArray();

        private static class EmptyArray<T>
        {
            public static readonly T[] Value = new T[0];
        }

        public static T[] Empty<T>() => EmptyArray<T>.Value;

        public static T[] WithLast<T>(this T[] source, T value)
        {
            if (source == null || source.Length == 0)
                return new[] { value };
            if (source.Length == 1)
                return new[] { source[0], value };
            if (source.Length == 2)
                return new[] { source[0], source[1], value };
            var sourceLength = source.Length;
            var result = new T[sourceLength + 1];
            Array.Copy(source, result, sourceLength);
            result[sourceLength] = value;
            return result;
        }

        public static Type[] GetParamTypes(IReadOnlyList<ParameterExpression> paramExprs)
        {
            if (paramExprs == null || paramExprs.Count == 0)
                return Empty<Type>();

            if (paramExprs.Count == 1)
                return new[] { paramExprs[0].IsByRef ? paramExprs[0].Type.MakeByRefType() : paramExprs[0].Type };

            var paramTypes = new Type[paramExprs.Count];
            for (var i = 0; i < paramTypes.Length; i++)
            {
                var parameterExpr = paramExprs[i];
                paramTypes[i] = parameterExpr.IsByRef ? parameterExpr.Type.MakeByRefType() : parameterExpr.Type;
            }

            return paramTypes;
        }

        public static Type GetFuncOrActionType(Type[] paramTypes, Type returnType)
        {
            if (returnType == typeof(void))
            {
                switch (paramTypes.Length)
                {
                    case 0: return typeof(Action);
                    case 1: return typeof(Action<>).MakeGenericType(paramTypes);
                    case 2: return typeof(Action<,>).MakeGenericType(paramTypes);
                    case 3: return typeof(Action<,,>).MakeGenericType(paramTypes);
                    case 4: return typeof(Action<,,,>).MakeGenericType(paramTypes);
                    case 5: return typeof(Action<,,,,>).MakeGenericType(paramTypes);
                    case 6: return typeof(Action<,,,,,>).MakeGenericType(paramTypes);
                    case 7: return typeof(Action<,,,,,,>).MakeGenericType(paramTypes);
                    default:
                        throw new NotSupportedException(
                            $"Action with so many ({paramTypes.Length}) parameters is not supported!");
                }
            }

            paramTypes = paramTypes.WithLast(returnType);
            switch (paramTypes.Length)
            {
                case 1: return typeof(Func<>).MakeGenericType(paramTypes);
                case 2: return typeof(Func<,>).MakeGenericType(paramTypes);
                case 3: return typeof(Func<,,>).MakeGenericType(paramTypes);
                case 4: return typeof(Func<,,,>).MakeGenericType(paramTypes);
                case 5: return typeof(Func<,,,,>).MakeGenericType(paramTypes);
                case 6: return typeof(Func<,,,,,>).MakeGenericType(paramTypes);
                case 7: return typeof(Func<,,,,,,>).MakeGenericType(paramTypes);
                case 8: return typeof(Func<,,,,,,,>).MakeGenericType(paramTypes);
                default:
                    throw new NotSupportedException(
                        string.Format("Func with so many ({0}) parameters is not supported!", paramTypes.Length));
            }
        }

        public static int GetFirstIndex<T>(this IReadOnlyList<T> source, T item)
        {
            if (source != null)
                for (var i = 0; i < source.Count; ++i)
                    if (ReferenceEquals(source[i], item))
                        return i;
            return -1;
        }

        public static T GetFirst<T>(this IEnumerable<T> source)
        {
            if (source is IList<T> list)
                return list.Count == 0 ? default(T) : list[0];
            using (var items = source.GetEnumerator())
                return items.MoveNext() ? items.Current : default(T);
        }
    }
}