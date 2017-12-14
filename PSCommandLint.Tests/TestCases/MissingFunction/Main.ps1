# ParseErrors = 0
# UnsupportedErrors = 0
# LocalCommands = 1
# ValidationErrors = 1
# Message = Bar is not defined

function Foo
{
	Write-Host "Foo"
	Bar
}

Foo