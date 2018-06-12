;имя приложения
!define PRODUCT_NAME "IIT Fibertest 2.0"
!define PRODUCT_NAME_2 "IIT_Fibertest_2.0"
;версия приложения
!define PRODUCT_VERSION "2.0"
!define BUILD_NUMBER "1"
!define REVISION "777"

;папки, где где брать исходные файлы, подлежащие сжатию.
!define pkgdir_client "c:\VSProjects\Fibertest20\Client\WpfClient\bin\x86\Release\"
!define pkgdir_datacenter "c:\VSProjects\Fibertest20\Server\DataCenterService\bin\Release\"
!define pkgdir_rtu "c:\VSProjects\Fibertest20\RTU\RtuService\bin\x86\Release\"
!define pkgdir_rtuwatchdog "c:\VSProjects\Fibertest20\RTU\RtuWatchdog\bin\Release\"

!include "MUI.nsh"
SetCompressor /SOLID lzma
!define MUI_ABORTWARNING
!define MUI_ICON "${NSISDIR}\Contrib\Graphics\Icons\modern-install.ico"
InstallDir "C:\IIT-Fibertest\"

!insertmacro MUI_PAGE_WELCOME
!insertmacro MUI_PAGE_DIRECTORY
!insertmacro MUI_PAGE_COMPONENTS
!insertmacro MUI_PAGE_INSTFILES
!insertmacro MUI_PAGE_FINISH

;--------------------------------
;язык по умолчанию - первый

!insertmacro MUI_LANGUAGE "English"
!insertmacro MUI_LANGUAGE "Russian"

  
LangString FT_SETUP ${LANG_ENGLISH} "${PRODUCT_NAME}.${BUILD_NUMBER}.${REVISION} setup"
LangString FT_SETUP ${LANG_RUSSIAN} "${PRODUCT_NAME}.${BUILD_NUMBER}.${REVISION} установка"
  
;--------------------------------

Name "${PRODUCT_NAME}"
Caption "$(FT_SETUP)"
OutFile "${PRODUCT_NAME_2}.${BUILD_NUMBER}.${REVISION}.exe"
ShowInstDetails show

;--------------------------------

Section "Client"

SetOutPath "$InstDir\Client\bin"
File /r "${pkgdir_client}\*.*"

SectionEnd

Section /o "Data Center"

SetOutPath "$InstDir\DataCenter\bin"
File /r "${pkgdir_datacenter}\*.*"

SectionEnd

Section "RTU Manager"

SetOutPath "$InstDir\RtuManager\bin"
File /r "${pkgdir_rtu}\*.*"
File /r "${pkgdir_rtuwatchdog}\*.*"

SectionEnd

;--------------------------------

Function .onInit
	!insertmacro MUI_LANGDLL_DISPLAY
	
FunctionEnd
