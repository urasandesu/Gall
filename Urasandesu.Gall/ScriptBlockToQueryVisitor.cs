/* 
 * File: ScriptBlockLambdaExpressionVisitor.cs
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
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Management.Automation;
using System.Management.Automation.Internal;
using System.Management.Automation.Language;
using System.Reflection;
using System.Text.RegularExpressions;

namespace Urasandesu.Gall
{
    abstract class ScriptBlockToQueryVisitor : ICustomAstVisitor
    {
        Dictionary<string, ParameterExpression> m_parameters;
        protected Dictionary<string, ParameterExpression> Parameters
        {
            get
            {
                if (m_parameters == null)
                    m_parameters = new Dictionary<string, ParameterExpression>();
                return m_parameters;
            }
        }

        protected abstract VariableScanner NewVariableScanner();

        class Definitions
        {
            public static readonly Type StringInfo = typeof(string);
            public static readonly MethodInfo StringInfo_Contains = StringInfo.GetMethod("Contains");
            public static readonly Type EnumerableInfo = typeof(Enumerable);
            public static readonly MethodInfo EnumerableInfo_Contains = EnumerableInfo.GetMethods().Where(_ => _.Name == "Contains").
                                                                                                    Where(_ => _.GetParameters().Length == 2).
                                                                                                    First().
                                                                                                    MakeGenericMethod(typeof(string));
        }

        protected ScriptBlockToQueryVisitor()
        { }

        public virtual object VisitArrayExpression(ArrayExpressionAst arrayExpressionAst)
        {
            return arrayExpressionAst.SubExpression.Visit(this);
        }

        public virtual object VisitArrayLiteral(ArrayLiteralAst arrayLiteralAst)
        {
            if (!CanResolveAsConstant(arrayLiteralAst))
                throw new NotSupportedException(string.Format("The array literal '{0}' must be able to resolve as constant.", arrayLiteralAst));

            return ResolveAsConstant(arrayLiteralAst);
        }

        public virtual object VisitAssignmentStatement(AssignmentStatementAst assignmentStatementAst)
        {
            if (!CanResolveAsConstant(assignmentStatementAst))
                throw new NotSupportedException(string.Format("The assignment statement '{0}' must be able to resolve as constant.", assignmentStatementAst));

            return ResolveAsConstant(assignmentStatementAst);
        }

        public virtual object VisitAttribute(AttributeAst attributeAst)
        {
            throw new NotSupportedException(string.Format("The attribute '{0}' is not supported.", attributeAst));
        }

        public virtual object VisitAttributedExpression(AttributedExpressionAst attributedExpressionAst)
        {
            throw new NotSupportedException(string.Format("The attributed expression '{0}' is not supported.", attributedExpressionAst));
        }

        public virtual object VisitBinaryExpression(BinaryExpressionAst binaryExpressionAst)
        {
            if (CanResolveAsConstant(binaryExpressionAst))
                return ResolveAsConstant(binaryExpressionAst);

            var left = (Expression)binaryExpressionAst.Left.Visit(this);
            var right = (Expression)binaryExpressionAst.Right.Visit(this);
            switch (binaryExpressionAst.Operator)
            {
                case TokenKind.And:
                case TokenKind.Band:
                    return Expression.And(left, right);
                case TokenKind.Or:
                case TokenKind.Bor:
                    return Expression.Or(left, right);
                case TokenKind.Ceq:
                case TokenKind.Ieq:
                    return Expression.Equal(left, right);
                case TokenKind.Cge:
                case TokenKind.Ige:
                    return Expression.GreaterThanOrEqual(left, right);
                case TokenKind.Cgt:
                case TokenKind.Igt:
                    return Expression.GreaterThan(left, right);
                case TokenKind.Cle:
                case TokenKind.Ile:
                    return Expression.LessThanOrEqual(left, right);
                case TokenKind.Clt:
                case TokenKind.Ilt:
                    return Expression.LessThan(left, right);
                case TokenKind.Cne:
                case TokenKind.Ine:
                    return Expression.NotEqual(left, right);
                case TokenKind.Cin:
                case TokenKind.Iin:
                    ValidateInExpressionArguments(left, right);
                    return Expression.Call(Definitions.EnumerableInfo_Contains, right, left);
                case TokenKind.Cmatch:
                case TokenKind.Imatch:
                    ValidateMatchExpressionArguments(left, right);
                    return Expression.Call(left, Definitions.StringInfo_Contains, right);
                default:
                    throw new NotSupportedException(string.Format("The binary operator '{0}' is not supported.", binaryExpressionAst.Operator));
            }
        }

        public virtual object VisitBlockStatement(BlockStatementAst blockStatementAst)
        {
            throw new NotSupportedException(string.Format("The block statement '{0}' is not supported.", blockStatementAst));
        }

        public virtual object VisitBreakStatement(BreakStatementAst breakStatementAst)
        {
            throw new NotSupportedException(string.Format("The break statement '{0}' is not supported.", breakStatementAst));
        }

        public virtual object VisitCatchClause(CatchClauseAst catchClauseAst)
        {
            throw new NotSupportedException(string.Format("The catch clause '{0}' is not supported.", catchClauseAst));
        }

        public virtual object VisitCommand(CommandAst commandAst)
        {
            throw new NotSupportedException(string.Format("The command '{0}' is not supported.", commandAst));
        }

        public virtual object VisitCommandExpression(CommandExpressionAst commandExpressionAst)
        {
            return commandExpressionAst.Expression.Visit(this);
        }

        public virtual object VisitCommandParameter(CommandParameterAst commandParameterAst)
        {
            throw new NotSupportedException(string.Format("The command parameter '{0}' is not supported.", commandParameterAst));
        }

        public virtual object VisitConstantExpression(ConstantExpressionAst constantExpressionAst)
        {
            return Expression.Constant(constantExpressionAst.Value);
        }

        public virtual object VisitContinueStatement(ContinueStatementAst continueStatementAst)
        {
            throw new NotSupportedException(string.Format("The continue statement '{0}' is not supported.", continueStatementAst));
        }

        public virtual object VisitConvertExpression(ConvertExpressionAst convertExpressionAst)
        {
            if (!CanResolveAsConstant(convertExpressionAst))
                throw new NotSupportedException(string.Format("The convert expression '{0}' must be able to resolve as constant.", convertExpressionAst));

            return ResolveAsConstant(convertExpressionAst);
        }

        public virtual object VisitDataStatement(DataStatementAst dataStatementAst)
        {
            throw new NotSupportedException(string.Format("The data statement '{0}' is not supported.", dataStatementAst));
        }

        public virtual object VisitDoUntilStatement(DoUntilStatementAst doUntilStatementAst)
        {
            throw new NotSupportedException(string.Format("The do-until statement '{0}' is not supported.", doUntilStatementAst));
        }

        public virtual object VisitDoWhileStatement(DoWhileStatementAst doWhileStatementAst)
        {
            throw new NotSupportedException(string.Format("The do-while statement '{0}' is not supported.", doWhileStatementAst));
        }

        public virtual object VisitErrorExpression(ErrorExpressionAst errorExpressionAst)
        {
            throw new NotSupportedException(string.Format("The error expression '{0}' is not supported.", errorExpressionAst));
        }

        public virtual object VisitErrorStatement(ErrorStatementAst errorStatementAst)
        {
            throw new NotSupportedException(string.Format("The error statement '{0}' is not supported.", errorStatementAst));
        }

        public virtual object VisitExitStatement(ExitStatementAst exitStatementAst)
        {
            throw new NotSupportedException(string.Format("The exit statement '{0}' is not supported.", exitStatementAst));
        }

        public virtual object VisitExpandableStringExpression(ExpandableStringExpressionAst expandableStringExpressionAst)
        {
            if (!CanResolveAsConstant(expandableStringExpressionAst))
                throw new NotSupportedException(string.Format("The expandable string expression '{0}' must be able to resolve as constant.", expandableStringExpressionAst));

            return ResolveAsConstant(expandableStringExpressionAst);
        }

        public virtual object VisitFileRedirection(FileRedirectionAst fileRedirectionAst)
        {
            throw new NotSupportedException(string.Format("The file redirection '{0}' is not supported.", fileRedirectionAst));
        }

        public virtual object VisitForEachStatement(ForEachStatementAst forEachStatementAst)
        {
            throw new NotSupportedException(string.Format("The for each statement '{0}' is not supported.", forEachStatementAst));
        }

        public virtual object VisitForStatement(ForStatementAst forStatementAst)
        {
            throw new NotSupportedException(string.Format("The for statement '{0}' is not supported.", forStatementAst));
        }

        public virtual object VisitFunctionDefinition(FunctionDefinitionAst functionDefinitionAst)
        {
            throw new NotSupportedException(string.Format("The function definition '{0}' is not supported.", functionDefinitionAst));
        }

        public virtual object VisitHashtable(HashtableAst hashtableAst)
        {
            throw new NotSupportedException(string.Format("The hashtable '{0}' is not supported.", hashtableAst));
        }

        public virtual object VisitIfStatement(IfStatementAst ifStmtAst)
        {
            throw new NotSupportedException(string.Format("The if statement '{0}' is not supported.", ifStmtAst));
        }

        public virtual object VisitIndexExpression(IndexExpressionAst indexExpressionAst)
        {
            if (!CanResolveAsConstant(indexExpressionAst))
                throw new NotSupportedException(string.Format("The index expression '{0}' must be able to resolve as constant.", indexExpressionAst));

            return ResolveAsConstant(indexExpressionAst);
        }

        public virtual object VisitInvokeMemberExpression(InvokeMemberExpressionAst invokeMemberExpressionAst)
        {
            if (!CanResolveAsConstant(invokeMemberExpressionAst))
                throw new NotSupportedException(string.Format("The invoke member expression '{0}' must be able to resolve as constant.", invokeMemberExpressionAst));

            return ResolveAsConstant(invokeMemberExpressionAst);
        }

        public virtual object VisitMemberExpression(MemberExpressionAst memberExpressionAst)
        {
            var member = memberExpressionAst.Member.Visit(this) as ConstantExpression;
            var propertyName = default(string);
            if (member == null || (member.Value != null && (propertyName = member.Value as string) == null))
                throw new NotSupportedException(string.Format("The member expression '{0}' contains some parameters.", memberExpressionAst));

            var expr = (Expression)memberExpressionAst.Expression.Visit(this);
            return Expression.Property(expr, propertyName);
        }

        public virtual object VisitMergingRedirection(MergingRedirectionAst mergingRedirectionAst)
        {
            throw new NotSupportedException(string.Format("The merging redirection '{0}' is not supported.", mergingRedirectionAst));
        }

        public virtual object VisitNamedAttributeArgument(NamedAttributeArgumentAst namedAttributeArgumentAst)
        {
            throw new NotSupportedException(string.Format("The named attribute argument '{0}' is not supported.", namedAttributeArgumentAst));
        }

        public virtual object VisitNamedBlock(NamedBlockAst namedBlockAst)
        {
            var traps = namedBlockAst.Traps;
            if (traps != null)
                throw new NotSupportedException(string.Format("The named block '{0}' can not contain trap statement.", namedBlockAst));
            
            var statements = namedBlockAst.Statements;
            if (statements.Count != 1)
                throw new NotSupportedException(string.Format("The named block '{0}' can contain only one statement.", namedBlockAst));

            return statements[0].Visit(this);
        }

        public virtual object VisitParamBlock(ParamBlockAst paramBlockAst)
        {
            throw new NotSupportedException(string.Format("The param block '{0}' is not supported.", paramBlockAst));
        }

        public virtual object VisitParameter(ParameterAst parameterAst)
        {
            throw new NotSupportedException(string.Format("The parameter '{0}' is not supported.", parameterAst));
        }

        public virtual object VisitParenExpression(ParenExpressionAst parenExpressionAst)
        {
            if (CanResolveAsConstant(parenExpressionAst))
                return ResolveAsConstant(parenExpressionAst);

            return parenExpressionAst.Pipeline.Visit(this);
        }

        public virtual object VisitPipeline(PipelineAst pipelineAst)
        {
            if (CanResolveAsConstant(pipelineAst))
                return ResolveAsConstant(pipelineAst);

            var elems = pipelineAst.PipelineElements;
            if (elems.Count != 1)
                throw new NotSupportedException(string.Format("The pipeline '{0}' can contain only one element.", pipelineAst));

            return elems[0].Visit(this);
        }

        public virtual object VisitReturnStatement(ReturnStatementAst returnStatementAst)
        {
            throw new NotSupportedException(string.Format("The return statement '{0}' is not supported.", returnStatementAst));
        }

        public virtual object VisitScriptBlock(ScriptBlockAst scriptBlockAst)
        {
            throw new NotSupportedException(string.Format("The script block '{0}' is not supported.", scriptBlockAst));
        }

        public virtual object VisitScriptBlockExpression(ScriptBlockExpressionAst scriptBlockExpressionAst)
        {
            throw new NotSupportedException(string.Format("The script block expression '{0}' is not supported.", scriptBlockExpressionAst));
        }

        public virtual object VisitStatementBlock(StatementBlockAst statementBlockAst)
        {
            if (!CanResolveAsConstant(statementBlockAst))
                throw new NotSupportedException(string.Format("The statement block '{0}' must be able to resolve as constant.", statementBlockAst));

            return ResolveAsConstant(statementBlockAst);
        }

        public virtual object VisitStringConstantExpression(StringConstantExpressionAst stringConstantExpressionAst)
        {
            return Expression.Constant(stringConstantExpressionAst.Value);
        }

        public virtual object VisitSubExpression(SubExpressionAst subExpressionAst)
        {
            return subExpressionAst.SubExpression.Visit(this);
        }

        public virtual object VisitSwitchStatement(SwitchStatementAst switchStatementAst)
        {
            throw new NotSupportedException(string.Format("The switch statement '{0}' is not supported.", switchStatementAst));
        }

        public virtual object VisitThrowStatement(ThrowStatementAst throwStatementAst)
        {
            throw new NotSupportedException(string.Format("The throw statement '{0}' is not supported.", throwStatementAst));
        }

        public virtual object VisitTrap(TrapStatementAst trapStatementAst)
        {
            throw new NotSupportedException(string.Format("The trap statement '{0}' is not supported.", trapStatementAst));
        }

        public virtual object VisitTryStatement(TryStatementAst tryStatementAst)
        {
            throw new NotSupportedException(string.Format("The try statement '{0}' is not supported.", tryStatementAst));
        }

        public virtual object VisitTypeConstraint(TypeConstraintAst typeConstraintAst)
        {
            throw new NotSupportedException(string.Format("The type constraint '{0}' is not supported.", typeConstraintAst));
        }

        public virtual object VisitTypeExpression(TypeExpressionAst typeExpressionAst)
        {
            throw new NotSupportedException(string.Format("The type expression '{0}' is not supported.", typeExpressionAst));
        }

        public virtual object VisitUnaryExpression(UnaryExpressionAst unaryExpressionAst)
        {
            if (!CanResolveAsConstant(unaryExpressionAst))
                throw new NotSupportedException(string.Format("The unary expression '{0}' must be able to resolve as constant.", unaryExpressionAst));

            return ResolveAsConstant(unaryExpressionAst);
        }

        public virtual object VisitUsingExpression(UsingExpressionAst usingExpressionAst)
        {
            throw new NotSupportedException(string.Format("The using expression '{0}' is not supported.", usingExpressionAst));
        }

        public virtual object VisitVariableExpression(VariableExpressionAst variableExpressionAst)
        {
            throw new NotSupportedException(string.Format("The variable expression '{0}' is not supported.", variableExpressionAst));
        }

        public virtual object VisitWhileStatement(WhileStatementAst whileStatementAst)
        {
            throw new NotSupportedException(string.Format("The while statement '{0}' is not supported.", whileStatementAst));
        }

        public static ConstantExpression ResolveAsConstant(Ast ast)
        {
            var value = (PSObject)ScriptBlock.Create(ast + "").InvokeReturnAsIs();
            if (value == null ||
                value.BaseObject == AutomationNull.Value ||
                    value.BaseObject is PSCustomObject && string.IsNullOrEmpty(value + ""))
                return Expression.Constant(null);

            var obj = value.BaseObject;
            var objs = default(object[]);
            if ((objs = obj as object[]) == null)
                return Expression.Constant(obj);

            var ss1 = objs.OfType<string>().ToArray();
            if (objs.Length == ss1.Length)
                return Expression.Constant(ss1);

            var ss2 = objs.OfType<PSObject>().Select(_ => _.BaseObject).OfType<string>().ToArray();
            if (objs.Length == ss2.Length)
                return Expression.Constant(ss2);

            return Expression.Constant(objs);
        }

        static void ValidateInExpressionArguments(Expression left, Expression right)
        {
            if (left.Type != typeof(string) || right.Type != typeof(string[]))
                throw new NotSupportedException(string.Format("The arguments of the in expression must be the following types. " +
                    "The left is string and the right is string array. Left: {0}({1}), Right: {2}({3})", left, left.Type, right, right.Type));
        }

        static void ValidateMatchExpressionArguments(Expression left, Expression right)
        {
            if (left.Type != typeof(string) || right.Type != typeof(string))
                throw new NotSupportedException(string.Format(
                    "The all arguments of the match expression must be string type. Left: {0}({1}), Right: {2}({3})", left, left.Type, right, right.Type));

            var value = default(string);
            var constExpr = right as ConstantExpression;
            if (constExpr == null || (value = constExpr.Value as string) == null)
                throw new NotSupportedException(string.Format("The right of the match expression must be able to resolve as string constant. Right: {0}", right));

            if (value != Regex.Escape(value))
                throw new NotSupportedException(string.Format("The right of the match expression must not contain regular expression special characters. Right: {0}", right));
        }

        bool CanResolveAsConstant(Ast ast)
        {
            var scanner = NewVariableScanner();
            ast.Visit(scanner);
            return !scanner.ExistsVariable;
        }

        protected abstract class VariableScanner : AstVisitor
        {
            public abstract bool ExistsVariable { get; }
        }
    }
}
