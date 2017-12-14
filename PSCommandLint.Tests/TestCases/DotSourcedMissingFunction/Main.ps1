# ParseErrors = 0
# UnsupportedErrors = 0
# LocalCommands = 2
# ValidationErrors = 1
# Message = Bar is not defined

function Foo
{
	Write-Host "Foo"
	Bar
}

. Included.ps1

Foo