/* 
 * File: ScriptBlockFuncExpressionVisitor.cs
 * 
 * Author: Akira Sugiura (urasandesu@gmail.com)
 * 
 * 
 * Copyright (c) 2015 Akira Sugiura
 *  
 *  This software is MIT License.
 *  
 *  Permission is hereby granted, free of charge, to any person obtaining a copy
 *  of this software and associated documentation files (the "Software"), to deal
 *  in the Software without restriction, including without limitation the rights
 *  to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
 *  copies of the Software, and to permit persons to whom the Software is
 *  furnished to do so, subject to the following conditions:
 *  
 *  The above copyright notice and this permission notice shall be included in
 *  all copies or substantial portions of the Software.
 *  
 *  THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
 *  IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
 *  FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
 *  AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
 *  LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
 *  OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
 *  THE SOFTWARE.
 */


using System;
using System.Linq;
using System.Linq.Expressions;
using System.Management.Automation;
using System.Management.Automation.Language;

namespace Urasandesu.Gall
{
    class ScriptBlockToRepositoryQueryVisitor<T, TResult> : ScriptBlockToQueryVisitor
    {
        public const string ParameterNameOfT = "gall";

        protected override VariableScanner NewVariableScanner()
        {
            return new ParameterScanner();
        }

        public override object VisitScriptBlock(ScriptBlockAst scriptBlockAst)
        {
            if (scriptBlockAst.DynamicParamBlock != null)
                throw new NotSupportedException(string.Format("The DynamicParam is not supported in the script block '{0}'.", scriptBlockAst));

            if (scriptBlockAst.BeginBlock != null)
                throw new NotSupportedException(string.Format("The Begin block is not supported in the script block '{0}'.", scriptBlockAst));

            if (scriptBlockAst.ProcessBlock != null)
                throw new NotSupportedException(string.Format("The Process block is not supported in the script block '{0}'.", scriptBlockAst));

            if (scriptBlockAst.EndBlock == null)
                throw new NotSupportedException(string.Format("The End block must be not null in the script block '{0}'.", scriptBlockAst));

            var body = (Expression)scriptBlockAst.EndBlock.Visit(this);
            // Erase the type explicitly, because Expression.Lambda<TDelegate> checks the consistency when just constructing it.
            if (typeof(TResult) == typeof(object))
                body = Expression.Convert(body, typeof(object));
            var parameters = Parameters.OrderBy(_ => _.Key).Select(_ => _.Value).ToArray();
            if (parameters.Length != 1)
                throw new NotSupportedException(string.Format("The script block '{0}' doesn't contain any parameters.", scriptBlockAst));
            
            return Expression.Lambda<Func<T, TResult>>(body, parameters);
        }

        public override object VisitVariableExpression(VariableExpressionAst variableExpressionAst)
        {
            if (!variableExpressionAst.VariablePath.IsUnqualified)
                throw new NotSupportedException(string.Format("The qualified variable '{0}' is not supported.", variableExpressionAst));

            var name = variableExpressionAst.VariablePath.UserPath.ToLower();
            if (name != ParameterNameOfT)
                return ResolveAsConstant(variableExpressionAst);

            if (!Parameters.ContainsKey(name))
                Parameters[name] = Expression.Parameter(typeof(T), name);

            return Parameters[name];
        }

        class ParameterScanner : VariableScanner
        {
            bool m_existsVariable;
            public override bool ExistsVariable
            {
                get { return m_existsVariable; }
            }

            public override AstVisitAction VisitVariableExpression(VariableExpressionAst variableExpressionAst)
            {
                m_existsVariable |= variableExpressionAst.VariablePath.UserPath.ToLower() == ParameterNameOfT;
                return m_existsVariable ? AstVisitAction.SkipChildren : base.VisitVariableExpression(variableExpressionAst);
            }
        }
    }
}
