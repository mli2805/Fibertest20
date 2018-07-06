rem start folder is RFTSViewer\workspace\
cd trunk\Source\
del RftsReflect.zip

mkdir RftsReflect\
xcopy RFTSViewer\__WinOut\reflect.exe RftsReflect\*.* /D/Y
xcopy RFTSViewer\__WinOut\borlndmm.dll RftsReflect\*.* /D/Y
xcopy RFTSViewer\__WinOut\cc32110.dll RftsReflect\*.* /D/Y
xcopy RFTSViewer\__WinOut\cc32110mt.dll RftsReflect\*.* /D/Y
xcopy RFTSViewer\_help\ReflectHelp.chm RftsReflect\*.* /D/Y
xcopy OpticalSwitch\__WinOut\oswlaunch.exe RftsReflect\*.* /D/Y

mkdir RftsReflect\etc\
xcopy OtdrMeasEngine\__WinOut\etc_default\*.* RftsReflect\etc\*.* /D/Y

mkdir RftsReflect\etc_gui\
xcopy RFTSViewer\__WinOut\etc_gui_default\*.* RftsReflect\etc_gui\*.* /D/Y

mkdir RftsReflect\lib\
xcopy OtdrMeasEngine\__WinOut\lib\automat.dll RftsReflect\lib\*.* /D/Y
xcopy OtdrMeasEngine\__WinOut\lib\id_com.dll RftsReflect\lib\*.* /D/Y
xcopy OtdrMeasEngine\__WinOut\lib\id_rmtt.dll RftsReflect\lib\*.* /D/Y
xcopy OtdrMeasEngine\__WinOut\lib\id_tcp.dll RftsReflect\lib\*.* /D/Y
xcopy OtdrMeasEngine\__WinOut\lib\id_usbW.dll RftsReflect\lib\*.* /D/Y
xcopy OtdrMeasEngine\__WinOut\lib\meas620.dll RftsReflect\lib\*.* /D/Y

mkdir RftsReflect\lib_gui\
xcopy RFTSViewer\__WinOut\lib_gui\sorInfoD.dll RftsReflect\lib_gui\*.* /D/Y

mkdir RftsReflect\plugins\
xcopy OpticalSwitch\__WinOut\*.dll RftsReflect\plugins\*.* /D/Y

mkdir RftsReflect\share\

"C:\Program Files\7-Zip\7z.exe" a -r RftsReflect.zip RftsReflect\*.* 

