# 
# File: NuGet.Get-VSIX.ps1
# 
# Author: Akira Sugiura (urasandesu@gmail.com)
# 
# 
# Copyright (c) 2015 Akira Sugiura
#  
#  This software is MIT License.
#  
#  Permission is hereby granted, free of charge, to any person obtaining a copy
#  of this software and associated documentation files (the "Software"), to deal
#  in the Software without restriction, including without limitation the rights
#  to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
#  copies of the Software, and to permit persons to whom the Software is
#  furnished to do so, subject to the following conditions:
#  
#  The above copyright notice and this permission notice shall be included in
#  all copies or substantial portions of the Software.
#  
#  THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
#  IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
#  FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
#  AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
#  LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
#  OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
#  THE SOFTWARE.
#

function Get-VSIX {
<#
    .SYNOPSIS
        

    .DESCRIPTION
        

    .PARAMETER  xxx
        

    .EXAMPLE
        yyy
        
        DESCRIPTION
        -----------
        zzz

    .INPUTS
        aaa

    .OUTPUTS
        None

    .NOTES
        ccc
        
        You can also refer to the Get-VSIX command by its built-in alias, "GAGet".

    .LINK
        ddd

#>

    [CmdletBinding(DefaultParametersetName = 'Manager')]
    param (
        [Parameter(Position = 0, ParameterSetName = 'Manager')]
        [string]
        $SearchText, 

        [Parameter(Mandatory = $true, Position = 0, ParameterSetName = 'Repository')]
        [Microsoft.VisualStudio.ExtensionManager.IRepositoryEntry]
        $Entry
    )

    switch ($PsCmdlet.ParameterSetName) {
        'Manager' { 
            $extMngr = [Microsoft.VisualStudio.Shell.Package]::GetGlobalService([Microsoft.VisualStudio.ExtensionManager.SVsExtensionManager])
            $extMngr.GetInstalledExtensions() | ? { ($_.Header | Out-String) -match $SearchText }
        }
        'Repository' { 
            $extRepos = [Microsoft.VisualStudio.Shell.Package]::GetGlobalService([Microsoft.VisualStudio.ExtensionManager.SVsExtensionRepository])
            $extRepos.Download($Entry)
        }
    }

}

New-Alias GAGet Get-VSIX

