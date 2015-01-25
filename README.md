# Gall
## SYNOPSIS
The commands that manage VSIX on the NuGet Package Manager Console.



## DESCRIPTION
_2015/01/25 unfinished_



## STATUS
_2015/01/25 unfinished_



## QUICK TOUR
I'm planning that developers can manage VSIX on the Package Manager Console as follows: 
```powershell
PM> $query = Search-VSIX -SearchText 'Unit Testing' -Where { $gall.Author -eq 'urasandesu' }
# Create the query that contains 'Unit Testing' and the package author is 'urasandesu'.

PM> $entry = $($query)
# Execute the query.

PM> $extension = Get-VSIX $entry
# Download the VSIX entry.

PM> Install-VSIX $extension
PendingEnable

PM> 
# Install the extension. You have to restart Visual Studio to enable it.

PM> $extension = Get-VSIX 'urasandesu'
# Get the extension that contains 'urasandesu' from installed extensions.

PM> Uninstall-VSIX $extension
# Uninstall the extension.

```
