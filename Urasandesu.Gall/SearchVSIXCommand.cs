/* 
 * File: SearchVSIXCommand.cs
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
using Microsoft.VisualStudio.Shell;
using System.Linq;
using System.Management.Automation;
using Urasandesu.Gall.Mixins.System.Management.Automation;

namespace Urasandesu.Gall
{
    [Cmdlet(VerbsCommon.Search, "VSIX", DefaultParameterSetName = "OrderBy")]
    public class SearchVSIXCommand : PSCmdlet
    {
        [Parameter(Position = 0)]
        public string SearchText { get; set; }

        [Parameter(Position = 1)]
        public ScriptBlock Where { get; set; }

        [Parameter(Position = 2, ParameterSetName = "OrderBy")]
        public ScriptBlock OrderBy { get; set; }

        [Parameter(Position = 2, ParameterSetName = "OrderByDescending")]
        public ScriptBlock OrderByDescending { get; set; }

        [Parameter(Position = 3)]
        public int? Skip { get; set; }

        [Parameter(Position = 4)]
        public int? Take { get; set; }

        protected override void EndProcessing()
        {
            //global::System.Diagnostics.Debugger.Launch();

            var extRepos = (IVsExtensionRepository)Package.GetGlobalService(typeof(SVsExtensionRepository));
            var reposQuery = extRepos.CreateQuery<VSGalleryEntry>(false, true, "ExtensionManagerQuery");
            
            reposQuery.SearchText = SearchText;
            
            var query = (IQueryable<VSGalleryEntry>)reposQuery;
            
            if (Where != null)
                query = query.Where(Where.ToRepositoryQuery<VSGalleryEntry, bool>());

            if (OrderBy != null)
                query = query.OrderBy(OrderBy.ToRepositoryQuery<VSGalleryEntry, object>());
            else if (OrderByDescending != null)
                query = query.OrderByDescending(OrderByDescending.ToRepositoryQuery<VSGalleryEntry, object>());

            if (Skip.HasValue)
                query = query.Skip(Skip.Value);

            if (Take.HasValue)
                query = query.Take(Take.Value);

            WriteObject(query);
        }
    }
}
