/* 
 * File: ScriptBlockMixinTest.cs
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


using Microsoft.VisualStudio.ExtensionManager;
using NUnit.Framework;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Management.Automation;
using System.Management.Automation.Runspaces;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Urasandesu.Gall.Mixins.System.Management.Automation;

namespace Test.Urasandesu.Gall.Mixins.System.Management.Automation
{
    [TestFixture]
    public class ScriptBlockMixinTest
    {
        [TestFixtureSetUp]
        public void Init()
        {
            if (Runspace.DefaultRunspace == null)
                new RunspaceInvoke();
            
            var runspace = Runspace.DefaultRunspace;
            Thread.CurrentThread.CurrentCulture = new CultureInfo("en-US");
        }



        [Test]
        [TestCaseSource(typeof(ToRepositoryQueryOfVSGalleryEntryOfBoolTestCaseDataFactory), "TestCases")]
        public string ToRepositoryQueryOfVSGalleryEntryOfBool_can_convert(string script)
        {
            // Arrange
            var scriptBlock = ScriptBlock.Create(script);

            // Act
            var query = scriptBlock.ToRepositoryQuery<VSGalleryEntry, bool>();

            // Assert
            var validator = new RepositoryQueryValidator();
            return validator.Validate(query);
        }

        class ToRepositoryQueryOfVSGalleryEntryOfBoolTestCaseDataFactory
        {
            public static IEnumerable TestCases
            {
                get
                {
                    yield return new TestCaseData("$gall.Author -eq 'urasandesu'").Returns("(Project.Metadata['Author'] = 'urasandesu')").SetName("from_Ieq");
                    yield return new TestCaseData("$gall.Author -eq \"$urasandesu\"").Returns("(Project.Metadata['Author'] = '')").SetName("from_Ieq_with_expandable_string");
                    yield return new TestCaseData("$gall.Author -eq ('urasandesu' > a.txt)").Returns("(Project.Metadata['Author'] = '')").SetName("from_Ieq_with_file_redirection");
                    yield return new TestCaseData("$gall.Author -eq ('urasandesu' 2>&1)").Returns("(Project.Metadata['Author'] = 'urasandesu')").SetName("from_Ieq_with_merging_redirection");
                    yield return new TestCaseData("$gall.Author -ceq 'urasandesu'").Returns("(Project.Metadata['Author'] = 'urasandesu')").SetName("from_Ceq");
                    yield return new TestCaseData("$gall.Author -ceq (5963).ToString('X8')").Returns("(Project.Metadata['Author'] = '0000174B')").SetName("from_Ceq_with_invoke_member_expression");
                    yield return new TestCaseData("$gall.Ranking -ge 10").Returns("(Project.Metadata['Ranking'] >= '10')").SetName("from_Ige");
                    yield return new TestCaseData("$gall.Ranking -ge (++$i)").Returns("(Project.Metadata['Ranking'] >= '1')").SetName("from_Ige_with_unary_expression");
                    yield return new TestCaseData("$gall.Ranking -cge 10").Returns("(Project.Metadata['Ranking'] >= '10')").SetName("from_Cge");
                    yield return new TestCaseData("$gall.SizeInBytes -gt 1024L").Returns("(Project.Metadata['SizeInBytes'] > '1024')").SetName("from_Igt");
                    yield return new TestCaseData("$gall.SizeInBytes -cgt 1024L").Returns("(Project.Metadata['SizeInBytes'] > '1024')").SetName("from_Cgt");
                    yield return new TestCaseData("$gall.LastModified -le [datetime]'2015/1/1'").Returns("(Project.Metadata['LastModified'] <= '1/1/2015 12:00:00 AM')").SetName("from_Ile");
                    yield return new TestCaseData("$gall.LastModified -cle [datetime]'2015/1/1'").Returns("(Project.Metadata['LastModified'] <= '1/1/2015 12:00:00 AM')").SetName("from_Cle");
                    yield return new TestCaseData("$gall.NonNullVsixVersion -lt [version]'1.11.0'").Returns("(Project.Metadata['NonNullVsixVersion'] < '1.11.0')").SetName("from_Ilt");
                    yield return new TestCaseData("$gall.NonNullVsixVersion -clt [version]'1.11.0'").Returns("(Project.Metadata['NonNullVsixVersion'] < '1.11.0')").SetName("from_Clt");
                    yield return new TestCaseData("$gall.CategoryID -ne [guid]'5b055950-a8c1-465b-b44a-20e6d5832d81'").Returns("(Category.Id != 5b055950-a8c1-465b-b44a-20e6d5832d81)").SetName("from_Ine");
                    yield return new TestCaseData("$gall.CategoryID -cne [guid]'5b055950-a8c1-465b-b44a-20e6d5832d81'").Returns("(Category.Id != 5b055950-a8c1-465b-b44a-20e6d5832d81)").SetName("from_Cne");
                    yield return new TestCaseData("$gall.Author -eq $null -and $gall.Ranking -ge 10").Returns("((Project.Metadata['Author'] = '') AND (Project.Metadata['Ranking'] >= '10'))").SetName("from_And");
                    yield return new TestCaseData("$gall.Author -eq $null -band $gall.Ranking -ge 10").Returns("((Project.Metadata['Author'] = '') AND (Project.Metadata['Ranking'] >= '10'))").SetName("from_Band");
                    yield return new TestCaseData("$gall.SizeInBytes -gt 1024L -or $gall.LastModified -le [datetime]'2015/1/1'").Returns("((Project.Metadata['SizeInBytes'] > '1024') OR (Project.Metadata['LastModified'] <= '1/1/2015 12:00:00 AM'))").SetName("from_Or");
                    yield return new TestCaseData("$gall.SizeInBytes -gt 1024L -bor $gall.LastModified -le [datetime]'2015/1/1'").Returns("((Project.Metadata['SizeInBytes'] > '1024') OR (Project.Metadata['LastModified'] <= '1/1/2015 12:00:00 AM'))").SetName("from_Bor");
                    yield return new TestCaseData("$gall.ExtensionIsInstalled -eq $true -and ($gall.RatingsCount -ge 3 -or $gall.Priority -gt [float]4)").Returns("((Project.Metadata['ExtensionIsInstalled'] = 'True') AND ((Release.RatingsCount >= 3) OR (Project.Metadata['Priority'] > '4')))").SetName("from_AndOr");
                    yield return new TestCaseData("$gall.Author -match 'urasandesu'").Returns("(Project.Metadata['Author'] LIKE '%urasandesu%')").SetName("from_Imatch");
                    yield return new TestCaseData("$gall.Author -cmatch 'urasandesu'").Returns("(Project.Metadata['Author'] LIKE '%urasandesu%')").SetName("from_Cmatch");
                    yield return new TestCaseData("$gall.Author -in 'urasandesu', 'akira', 'sugiura'").Returns("((Project.Metadata['Author'] = 'urasandesu') OR (Project.Metadata['Author'] = 'akira') OR (Project.Metadata['Author'] = 'sugiura'))").SetName("from_Iin");
                    yield return new TestCaseData("$gall.Author -in ('urasandesu', 'akira', 'sugiura' | ? { $_ -match '^u|^a' })").Returns("((Project.Metadata['Author'] = 'urasandesu') OR (Project.Metadata['Author'] = 'akira'))").SetName("from_Iin_with_multiple_pipeline");
                    yield return new TestCaseData("$gall.Author -cin @('urasandesu', 'akira', 'sugiura')").Returns("((Project.Metadata['Author'] = 'urasandesu') OR (Project.Metadata['Author'] = 'akira') OR (Project.Metadata['Author'] = 'sugiura'))").SetName("from_Cin");
                    yield return new TestCaseData("$gall.Author -cin @('urasandesu', 'akira', 'sugiura')[-2..-1]").Returns("((Project.Metadata['Author'] = 'akira') OR (Project.Metadata['Author'] = 'sugiura'))").SetName("from_with_index");
                    yield return new TestCaseData("$Gall.(@('Auth'; 'or') -join '') -eq 'urasandesu'").Returns("(Project.Metadata['Author'] = 'urasandesu')").SetName("from_exclude_the_above_but_convertible_into_constant");
                    yield return new TestCaseData("$gall.DownloadCount -shl 2").Throws(typeof(NotSupportedException)).SetName("from_only_the_above_binary_expressions");

                    yield return new TestCaseData("$gall.$(trap { 'error!!' } 'Author') -eq 'urasandesu'").Returns("(Project.Metadata['Author'] = 'urasandesu')").SetName("from_member_with_trap_that_is_convertible_into_constant");
                    yield return new TestCaseData("$gall.$('Author') -eq 'urasandesu'").Returns("(Project.Metadata['Author'] = 'urasandesu')").SetName("from_member_with_string_constant");
                    yield return new TestCaseData("$gall.Author -eq $($author = 'urasandesu'; $author)").Returns("(Project.Metadata['Author'] = 'urasandesu')").SetName("from_sub_expression_with_assignment");
                    yield return new TestCaseData("$gall.Author -eq $(if ($true) { 'urasandesu' } elseif ($false) { throw 'error!!' } else { break; })").Returns("(Project.Metadata['Author'] = 'urasandesu')").SetName("from_sub_expression_with_if_statement");
                    
                    yield return new TestCaseData("[string]$gall.Ranking -eq '10'").Throws(typeof(NotSupportedException)).SetName("from_member_without_casting_parameter");
                    yield return new TestCaseData("\"$gall\" -match 'urasandesu'").Throws(typeof(NotSupportedException)).SetName("from_member_without_expandable_string");
                    yield return new TestCaseData("$gall['Author'] -eq 'urasandesu'").Throws(typeof(NotSupportedException)).SetName("from_member_without_index");
                    yield return new TestCaseData("$gall.Ranking.ToString() -eq '10'").Throws(typeof(NotSupportedException)).SetName("from_member_without_invoke_member_expression");
                    yield return new TestCaseData("(++$gall.Ranking) -eq 11").Throws(typeof(NotSupportedException)).SetName("from_member_without_unary_expression");
                    yield return new TestCaseData("dynamicparam {} begin {} process {} end { $gall.Author -eq 'urasandesu' }").Throws(typeof(NotSupportedException)).SetName("from_expression_without_dynamic_param");
                    yield return new TestCaseData("begin { $gall.Author -eq 'urasandesu' }").Throws(typeof(NotSupportedException)).SetName("from_expression_without_begin_block");
                    yield return new TestCaseData("process { $gall.Author -eq 'urasandesu' }").Throws(typeof(NotSupportedException)).SetName("from_expression_without_process_block");
                    yield return new TestCaseData("").Throws(typeof(NotSupportedException)).SetName("from_expression_with_end_block");
                    yield return new TestCaseData("$$$gall.Author -eq 'urasandesu'").Throws(typeof(ParseException)).SetName("from_expression_with_no_syntax_error");
                    yield return new TestCaseData("$gall.Ranking -match 'urasandesu'").Throws(typeof(NotSupportedException)).SetName("from_match_expression_without_invalid_types");
                    yield return new TestCaseData("$gall.Description -match $gall.Author").Throws(typeof(NotSupportedException)).SetName("from_match_expression_without_non_constant_right");
                    yield return new TestCaseData("$gall.Author -match 'urasandesu.*'").Throws(typeof(NotSupportedException)).SetName("from_match_expression_without_regular_expression_special_characters");
                    yield return new TestCaseData("$gall.Ranking -in 'urasandesu'").Throws(typeof(NotSupportedException)).SetName("from_in_expression_without_invalid_types");
                    yield return new TestCaseData("$gall.Author -in 'urasandesu', 1, 3.14").Throws(typeof(NotSupportedException)).SetName("from_in_expression_without_non_string_elements");

                    yield return new TestCaseData("continue").Throws(typeof(NotSupportedException)).SetName("from_not_only_continue_statement");
                    yield return new TestCaseData("break").Throws(typeof(NotSupportedException)).SetName("from_not_only_break_statement");
                    yield return new TestCaseData("data { 'hoge' }").Throws(typeof(NotSupportedException)).SetName("from_not_only_data_statement");
                    yield return new TestCaseData("do { 'hoge' } until ($i++ -ge 10)").Throws(typeof(NotSupportedException)).SetName("from_not_only_do_until_statement");
                    yield return new TestCaseData("do { 'hoge' } while (++$i -lt 10)").Throws(typeof(NotSupportedException)).SetName("from_not_only_do_while_statement");
                    yield return new TestCaseData("exit").Throws(typeof(NotSupportedException)).SetName("from_not_only_exit_statement");
                    yield return new TestCaseData("try { } catch { } finally { }").Throws(typeof(NotSupportedException)).SetName("from_not_only_try_statement");
                    yield return new TestCaseData("gv").Throws(typeof(NotSupportedException)).SetName("from_not_only_command");
                    yield return new TestCaseData("foreach ($i in $ii) { 'hoge' }").Throws(typeof(NotSupportedException)).SetName("from_not_only_for_each_statement");
                    yield return new TestCaseData("for ($i = 0; $i -lt 10; $i++) { 'hoge' }").Throws(typeof(NotSupportedException)).SetName("from_not_only_for_statement");
                    yield return new TestCaseData("function Hoge { }").Throws(typeof(NotSupportedException)).SetName("from_not_only_function_definition");
                    yield return new TestCaseData("@{ Key = 'Value' }").Throws(typeof(NotSupportedException)).SetName("from_not_only_hashtable");
                    yield return new TestCaseData("if ($true) { 'urasandesu' } elseif ($false) { throw 'error!!' } else { break; }").Throws(typeof(NotSupportedException)).SetName("from_not_only_if_statement");
                    yield return new TestCaseData("return 10").Throws(typeof(NotSupportedException)).SetName("from_not_only_return_statement");
                    yield return new TestCaseData("switch ($i) { 0 { 'hoge' } }").Throws(typeof(NotSupportedException)).SetName("from_not_only_switch_statement");
                    yield return new TestCaseData("throw 'error!!'").Throws(typeof(NotSupportedException)).SetName("from_not_only_throw_statement");
                    yield return new TestCaseData("trap { 'error!!' }").Throws(typeof(NotSupportedException)).SetName("from_not_only_trap_statement");
                    yield return new TestCaseData("while ($i++ -lt 10) { 'hoge' }").Throws(typeof(NotSupportedException)).SetName("from_not_only_while_statement");
                }
            }
        }



        [Test]
        public void ToRepositoryQueryOfVSGalleryEntryOfObject_can_convert_from_struct()
        {
            // Arrange
            var script = "$gall.Ranking";
            var scriptBlock = ScriptBlock.Create(script);

            // Act
            var query = scriptBlock.ToRepositoryQuery<VSGalleryEntry, object>();

            // Assert
            var validator = new RepositoryQueryValidator();
            var result = validator.Validate(query);
            Assert.AreEqual("Project.Metadata['Ranking']", result);
        }
    }
}
