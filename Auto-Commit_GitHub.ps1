# Use the following line as POST build event command line in your IDE (Visual Studio or SharpDevelop)
# start Powershell -File "T:\Dropbox\Entwicklungsstrang\Shiny ID3 Tagger\_git\Commit-to-GitHub.ps1"
cls

# Change working directory to directory where this .ps1 script is saved. We should now be in our root folder of GIT repository
cd $PSScriptRoot

$git = "T:\Zips\01 - Programme\Database\Git [Portable]\cmd\git.exe"
& $git fetch Shiny-ID3-Tagger
& $git status

# Cancel script in case remote repository has newer files which should be downloaded first before uploading
if (1 -eq 1)
{
    Cmd /c pause
}



echo "Upload current folder to GitHub"
#git add --all
#git commit --amend
#git push origin master