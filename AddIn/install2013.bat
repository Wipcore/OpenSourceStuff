rem Unsupported installation script for the Enova Class Wizard, VS2013.

call "%ProgramFiles(x86)%\Microsoft Visual Studio 12.0\VC\vcvarsall.bat" x86
gacutil /i Wipcore.eNova.VisualStudio.WizardLib.dll
gacutil /i Divelements.WizardFramework.dll
xcopy /Y /R /E "EnovaIntegrationProject.zip" "%USERPROFILE%\Documents\Visual Studio 2013\Templates\ProjectTemplates\Visual C#"
if ERRORLEVEL == 1 goto COPYFAILED
xcopy /Y /R /E "EnovaAddInFormProject.zip" "%USERPROFILE%\Documents\Visual Studio 2013\Templates\ProjectTemplates\Visual C#"
if ERRORLEVEL == 1 goto COPYFAILED
rem xcopy /Y /R /E "EnovaWebApplication.zip" "%USERPROFILE%\Documents\Visual Studio 2013\Templates\ProjectTemplates\Visual C#"
rem if ERRORLEVEL == 1 goto COPYFAILED
xcopy /Y /R /E "EnovaClassLibrary.zip" "%USERPROFILE%\Documents\Visual Studio 2013\Templates\ProjectTemplates\Visual C#"
if ERRORLEVEL == 1 goto COPYFAILED


xcopy /Y /R /E "EnovaIntegrationMainTask.zip" "%USERPROFILE%\Documents\Visual Studio 2013\Templates\ItemTemplates\Visual C#"
if ERRORLEVEL == 1 goto COPYFAILED
xcopy /Y /R /E "EnovaAddInForm.zip" "%USERPROFILE%\Documents\Visual Studio 2013\Templates\ItemTemplates\Visual C#"
if ERRORLEVEL == 1 goto COPYFAILED
rem xcopy /Y /R /E "EnovaWebForm.zip" "%USERPROFILE%\Documents\Visual Studio 2013\Templates\ItemTemplates\Visual C#"
rem if ERRORLEVEL == 1 goto COPYFAILED
xcopy /Y /R /E "EnovaClass.zip" "%USERPROFILE%\Documents\Visual Studio 2013\Templates\ItemTemplates\Visual C#"
if ERRORLEVEL == 1 goto COPYFAILED
mkdir "%USERPROFILE%\Documents\Visual Studio 2013\AddIns"
xcopy /Y /R /E  "EnovaClassDesignerAddIn\*.*" "%USERPROFILE%\Documents\Visual Studio 2013\AddIns"
if ERRORLEVEL == 1 goto UNZIPFAILED

goto END

:UNZIPFAILED
echo Must unzip EnovaClassDesignerAddIn.zip !!!
goto END

:COPYFAILED
echo Copy failed!!!
goto END
:END
echo "Restart visual studio and happy hacking!!!"
pause
