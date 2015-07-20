Welcome to the psake project.
=============================

psake is a build automation tool written in PowerShell. It avoids the angle-bracket tax associated with executable XML by leveraging the PowerShell syntax in your build scripts. 
psake has a syntax inspired by rake (aka make in Ruby) and bake (aka make in Boo), but is easier to script because it leverages your existing command-line knowledge.

psake is pronounced sake – as in Japanese rice wine. It does NOT rhyme with make, bake, or rake.

## How to get started:

**Step 1:** Download and extract the project

You will need to "unblock" the zip file before extracting - PowerShell by default does not run files downloaded from the internet.
Just right-click the zip and click on "properties" and click on the "unblock" button.

**Step 2:** CD into the directory where you extracted the project (where the psake.psm1 file is)

> Import-Module .\psake.psm1

If you encounter the following error "Import-Module : ...psake.psm1 cannot be loaded because the execution of scripts is disabled on this system." Please see "get-help about_signing" for more details.

1. Run PowerShell as administrator
2. Set-ExecutionPolicy RemoteSigned

> Get-Help Invoke-psake -Full   
> - this will show you help and examples of how to use psake
	
**Step 3:** Run some examples

> CD .\examples
>
> Invoke-psake    					
> - This will execute the "default" task in the "default.ps1"
>
> Invoke-psake .\default.ps1 Clean  
> - will execute the single task in the default.ps1 script

## How To Contribute, Collaborate, Communicate

If you'd like to get involved with psake, we have discussion groups over at google: **[psake-dev](http://groups.google.com/group/psake-dev)** **[psake-users](http://groups.google.com/group/psake-users)**

Anyone can fork the main repository and submit patches, as well. And lastly, the [wiki](http://wiki.github.com/psake/psake/) and [issues list](http://github.com/psake/psake/issues) are also open for additions, edits, and discussion.

Also check out the **[psake-contrib](http://github.com/psake/psake-contrib)** project for scripts, modules and functions to help you with a build.

## License

psake is released under the [MIT license](http://www.opensource.org/licenses/MIT).
