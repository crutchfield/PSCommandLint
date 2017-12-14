# ParseErrors = 0
# UnsupportedErrors = 0
# LocalCommands = 2
# ValidationErrors = 0

function Foo
{
	Write-Host "Foo"
	Bar
}

. Included-A.ps1

Foo