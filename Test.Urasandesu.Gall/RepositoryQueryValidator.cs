/* 
 * File: RepositoryQueryValidator.cs
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
using System.Reflection;

namespace Test.Urasandesu.Gall
{
    class RepositoryQueryValidator
    {
        class Definitions
        {
            public static readonly Assembly Implementation = Assembly.Load("Microsoft.VisualStudio.ExtensionManager.Implementation, Version=12.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a");
            public static readonly Type RepositoryQueryTranslatorInfo = Implementation.GetTypes().Where(_ => _.Name == "RepositoryQueryTranslator").First();
            public static readonly ConstructorInfo RepositoryQueryTranslatorInfo_ctor = RepositoryQueryTranslatorInfo.GetConstructor(BindingFlags.NonPublic | BindingFlags.Instance, null, new[] { typeof(bool), typeof(bool) }, null);
            public static readonly MethodInfo RepositoryQueryTranslatorInfo_Translate = RepositoryQueryTranslatorInfo.GetMethod("Translate", BindingFlags.NonPublic | BindingFlags.Instance);
        }

        object m_target;
        public RepositoryQueryValidator()
        {
            m_target = Definitions.RepositoryQueryTranslatorInfo_ctor.Invoke(new object[] { false, false });
        }

        public string Validate(Expression query)
        {
            return (string)Definitions.RepositoryQueryTranslatorInfo_Translate.Invoke(m_target, new object[] { query });
        }
    }
}
