Unicode true
RequestExecutionLevel admin

;имя приложения
!define PRODUCT_NAME "IIT Fibertest 2.0"
!define PRODUCT_NAME_2 "IIT_Fibertest_2.0"
;версия приложения
!define PRODUCT_VERSION "2.0"
!define BUILD_NUMBER "1"
!define REVISION "777"

;заставляет всегда появляться выбор языка
;если не заставлять, то берет язык из реестра - ключ ниже, если не сохранен, то из настроек системы 
!define MUI_LANGDLL_ALLLANGUAGES
;запоминает язык установки
!define MUI_LANGDLL_REGISTRY_ROOT "HKLM" 
  ;на самом деле окажется в ветке Software\WOW6432Node\Microsoft\Windows\CurrentVersion\Uninstall\Iit\FiberTest20
!define MUI_LANGDLL_REGISTRY_KEY "Software\Microsoft\Windows\CurrentVersion\Uninstall\Iit\FiberTest20" 
!define MUI_LANGDLL_REGISTRY_VALUENAME "Installer Language"
  
;папки, где где брать исходные файлы, подлежащие сжатию.
!define pkgdir_client "c:\VSProjects\Fibertest20\Client\WpfClient\bin\x86\Release\"
!define pkgdir_datacenter "c:\VSProjects\Fibertest20\DataCenter\DataCenterService\bin\Release\"
!define pkgdir_rtu "c:\VSProjects\Fibertest20\RTU\RtuService\bin\x86\Release\"
!define pkgdir_rtuwatchdog "c:\VSProjects\Fibertest20\RTU\RtuWatchdog\bin\Release\"

!include "MUI.nsh"
!include "LogicLib.nsh"
;SetCompressor /SOLID lzma
SetCompressor /SOLID zlib

!define MUI_ABORTWARNING
!define MUI_ICON "${NSISDIR}\Contrib\Graphics\Icons\modern-install.ico"
InstallDir "C:\IIT-Fibertest\"

;передвигает языковые файлы в начало архива, что улучшает время загрузки
!insertmacro MUI_RESERVEFILE_LANGDLL

  !define MUI_PAGE_CUSTOMFUNCTION_PRE PreLicense
  !insertmacro MUI_PAGE_LICENSE $(MUILicense)
  
 !insertmacro MUI_PAGE_WELCOME
 !insertmacro MUI_PAGE_DIRECTORY
!insertmacro MUI_PAGE_COMPONENTS
!insertmacro MUI_PAGE_INSTFILES
!insertmacro MUI_PAGE_FINISH

;--------------------------------
;язык по умолчанию - первый

!insertmacro MUI_LANGUAGE "English"
!insertmacro MUI_LANGUAGE "Russian"
;!insertmacro MUI_LANGUAGE "German"
  
LangString FT_SETUP ${LANG_ENGLISH} "${PRODUCT_NAME}.${BUILD_NUMBER}.${REVISION} setup"
LangString FT_SETUP ${LANG_RUSSIAN} "Установка ${PRODUCT_NAME}.${BUILD_NUMBER}.${REVISION}"

LangString AppInstalled ${LANG_ENGLISH} "FIBERTEST 2.0 is installed on your PC. It is recommended to uninstall old version. Cancel installation?"
LangString AppInstalled ${LANG_RUSSIAN} "FIBERTEST 2.0 установлен на этом компьютере. Перед продолжением рекомендуется удалить старую версию. Прервать установку?"

;--------------------------------
;Строки с путями файлов с лицензиями

LicenseLangString MUILicense ${LANG_ENGLISH} "doc\license-en\license.rtf"
LicenseLangString MUILicense ${LANG_RUSSIAN} "doc\license-ru\license.rtf"

;--------------------------------

Name "${PRODUCT_NAME}"
Caption "$(FT_SETUP)"
OutFile "${PRODUCT_NAME_2}.${BUILD_NUMBER}.${REVISION}.exe"
ShowInstDetails show

;--------------------------------
;Типы установки

InstType $(ClientInstallation)
InstType $(DataCenterInstallation)
InstType $(RtuManagerInstallation)
InstType /NOCUSTOM

LangString ClientInstallation ${LANG_ENGLISH} "Client"
LangString ClientInstallation ${LANG_RUSSIAN} "Client"

LangString DataCenterInstallation ${LANG_ENGLISH} "Data Center"
LangString DataCenterInstallation ${LANG_RUSSIAN} "Data Center"

LangString RtuManagerInstallation ${LANG_ENGLISH} "Rtu Manager"
LangString RtuManagerInstallation ${LANG_RUSSIAN} "Rtu Manager"

;--------------------------------
Section "CommonBeforeInstall"
	SectionIn RO 1 2 3
	
	CreateDirectory "$SMPROGRAMS\IIT"
	CreateDirectory "$SMPROGRAMS\IIT\FIBERTEST20"	
SectionEnd

Section "Client"
	SectionIn RO 1 2
	
SetOutPath "$INSTDIR\Client\bin"
File /r "${pkgdir_client}\*.*"

	
	CreateShortCut "$SMPROGRAMS\IIT\FIBERTEST20\ClientFt20.lnk" "$INSTDIR\Client\bin\Iit.Fibertest.Client.exe"
	CreateShortCut "$DESKTOP\ClientFt20.lnk" "$INSTDIR\Client\bin\Iit.Fibertest.Client.exe"
	;CreateShortCut "$SMPROGRAMS\IIT\FIBERTEST 2.0\RTFS Reflect.lnk" "$R8\TraceEngine\reflect.exe"
		
SectionEnd

Section "Data Center"
	SectionIn RO 2
	
; Check if the service exists
; returns an errorcode if the service doesn't exists (<>0)/service exists (0)
  SimpleSC::ExistsService '243АВА4ЕФ3Ы'

  Pop $0 
  IntCmp $0 0 serviceExists serviceDoesntExist serviceDoesntExist
  
  !define IsExist $0
  IntCmp IsExist 0 serviceExists serviceDoesntExist serviceDoesntExist


  
	Pop $0
	StrCmp $0 'Yes' 0 serviceDoesntExist

	
serviceExists:

  MessageBox MB_OK|MB_ICONINFORMATION "Service exists, will be stopped"

; Stop a service and waits for file release. Be sure to pass the service name, not the display name.
  SimpleSC::StopService "AFibertestDcService" 1 30
  Pop $0 
  IntCmp $0 0 serviceStopped serviceError serviceError
 
serviceStopped: 
serviceDoesntExist:

  MessageBox MB_OK|MB_ICONINFORMATION "Copying service files..."

  SetOutPath "$INSTDIR\DataCenter\bin"
  File /r "${pkgdir_datacenter}\*.*"

  ;IntCmp IsExist 0 endDcInstallation installService installService
  IntCmp $0 0 endDcInstallation installService installService

installService:  
; Install a service - ServiceType own process - StartType automatic - NoDependencies - Logon as System Account
  SimpleSC::InstallService "AFibertestDcService" "AFibertest 2.0 DataCenter Server" "16" "2" "$INSTDIR\DataCenter\bin\Iit.Fibertest.DataCenterService.exe" "" "" ""
  Pop $0 
  IntCmp $0 0 installationSuccess serviceError serviceError
 
serviceError:
   MessageBox MB_OK|MB_ICONSTOP "Service error"
   Quit
   
installationSuccess:
	MessageBox MB_OK|MB_ICONINFORMATION "Datacenter service installed successfully!"
   
endDcInstallation:

SectionEnd

Section "RTU Manager"
	SectionIn RO 3
	
SetOutPath "$INSTDIR\RtuManager\bin"
File /r "${pkgdir_rtu}\*.*"
File /r "${pkgdir_rtuwatchdog}\*.*"
; стартовать сервисы

SectionEnd

; установка Деинсталлятора
Section "UnInstaller"
	SectionIn RO 1 2 3
	
SetOutPath "$INSTDIR"
WriteUninstaller "$INSTDIR\Uninstall.exe"
CreateShortCut "$DESKTOP\UninstFt20.lnk" "$INSTDIR\Uninstall.exe"
CreateShortCut "$SMPROGRAMS\IIT\FIBERTEST20\UninstFt20.lnk" "$INSTDIR\Uninstall.exe"

SectionEnd

; а это - что делает деинсталлятор
Section "Uninstall"
	; изменение контекста переменных окружения, чтобы они относились к All users
	; это нужно для удаления ярлыков из меню Пуск в Vista и Windows 7
	SetShellVarContext all 
	
;Удаление сервисов
	;TODO
  
;Удаление файлов c сохранением некоторых подпапок 
	Delete "$INSTDIR\Uninstall.exe"
	RMDir /r "$INSTDIR\Client\bin"
	RMDir /r "$INSTDIR\DataCenter\bin"
	RMDir /r "$INSTDIR\RtuManager\bin"
	
	Delete "$INSTDIR\*.*"
	
;Удаление ярлыков из меню Пуск
	Delete "$SMPROGRAMS\IIT\FIBERTEST20\*.*"
	RMDir /r "$SMPROGRAMS\IIT\FIBERTEST20"
	
;Удаление ярлыков с рабочего стола
	Delete "$DESKTOP\ClientFt20.lnk"
	Delete "$DESKTOP\UninstFt20.lnk"
	Delete "$DESKTOP\RFTS Reflect.lnk"
	
;Deleting registry keys
	DeleteRegKey HKLM "Software\Microsoft\Windows\CurrentVersion\Uninstall\Iit\FiberTest20"
SectionEnd  

;--------------------------------

Function .onInit
	!insertmacro MUI_LANGDLL_DISPLAY
	
FunctionEnd

Function PreLicense
  ; check installed fibertest
  ReadRegStr '$R1' HKLM 'Software\Microsoft\Windows\CurrentVersion\Uninstall\Iit\FiberTest20' 'UninstallString'
  StrCmp $R1 '' ApplNotInstall 0 
  MessageBox MB_YESNO|MB_ICONINFORMATION "$(AppInstalled)" IDNO ApplNotInstall
  Quit
ApplNotInstall:
   
FunctionEnd

