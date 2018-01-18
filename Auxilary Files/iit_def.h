#ifndef iit_defH
#define iit_defH

#include <cstddef>

#define Spec_Version                   "1.6.0"

#define DLL_INIT_L                      "DllInit"
#define DLL_INIT_2_L                    "DllInit2"
#define IIT_INIT_OTDR_L                 "InitOTDR"
#define IIT_INIT_OTDR_EX                "InitOTDREx"
#define IIT_SERVICE_FUNCTION_L          "ServiceFunction"
#define IIT_MEAS_STEP_L                 "MeasStep"
#define IIT_MEAS_FULL_L                 "MeasFull"
#define IIT_MEAS_STOP_L                 "MeasStop"
#define IIT_SET_MEAS_PRE_L              "MeasPrepare"
#define MEASPRMDLG_SERVICE_FUNCTION_L   "ServiceFunction"

#define IIT_MD_ADD_CONTEXT_L               "MD_AddContext"
#define IIT_MD_ADD_DEVICE_L                "MD_AddDevice"
#define IIT_MD_DELETE_CONTEXT_L            "MD_DeleteContext"
#define IIT_MD_DELETE_DEVICE_L             "MD_DeleteDevice"
#define IIT_MD_INIT_OTDR_L                 "MD_InitOTDR"
#define IIT_MD_INIT_OTDR_EX                "MD_InitOTDREx"
#define IIT_MD_SERVICE_FUNCTION_L          "MD_ServiceFunction"
#define IIT_MD_MEAS_STEP_L                 "MD_MeasStep"
#define IIT_MD_MEAS_FULL_L                 "MD_MeasFull"
#define IIT_MD_MEAS_STOP_L                 "MD_MeasStop"
#define IIT_MD_SET_MEAS_PRE_L              "MD_MeasPrepare"
#define MEASPRMDLG_MD_SERVICE_FUNCTION_L   "MD_ServiceFunction"

#define IIT_MEAS_STEP_BUFFER_L             "MeasStep_Buffer"
#define IIT_MEAS_STOP_BUFFER_L             "MeasStop_Buffer"
#define IIT_MEAS_FULL_BUFFER_L             "MeasFull_Buffer"
#define IIT_MD_MEAS_STEP_BUFFER_L          "MD_MeasStep_Buffer"
#define IIT_MD_MEAS_STOP_BUFFER_L          "MD_MeasStop_Buffer"
#define IIT_MD_MEAS_FULL_BUFFER_L          "MD_MeasFull_Buffer"

#define IIT_GET_SOR_SIZE_L    "GetSorSize"
#define IIT_GET_SOR_DATA_L    "GetSorData"
#define IIT_CREATE_SOR_PTR_L  "CreateSorPtr"
#define IIT_DESTROY_SOR_PTR_L "DestroySorPtr"

#if defined(__LINUX__) || defined(__MICROSOFT__)
#ifndef DLL_INIT
#define DLL_INIT                         DLL_INIT_L
#define DLL_INIT_2                       DLL_INIT_2_L
#endif
#define IIT_INIT_OTDR                    IIT_INIT_OTDR_L
#define IIT_SERVICE_FUNCTION             IIT_SERVICE_FUNCTION_L
#define IIT_MEAS_STEP                    IIT_MEAS_STEP_L
#define IIT_MEAS_FULL                    IIT_MEAS_FULL_L
#define IIT_MEAS_STOP                    IIT_MEAS_STOP_L
#define IIT_SET_MEAS_PRE                 IIT_SET_MEAS_PRE_L
#define MEASPRMDLG_SERVICE_FUNCTION      MEASPRMDLG_SERVICE_FUNCTION_L

#define IIT_MD_ADD_CONTEXT			IIT_MD_ADD_CONTEXT_L
#define IIT_MD_ADD_DEVICE			IIT_MD_ADD_DEVICE_L
#define IIT_MD_DELETE_CONTEXT			IIT_MD_DELETE_CONTEXT_L
#define IIT_MD_DELETE_DEVICE			IIT_MD_DELETE_DEVICE_L
#define IIT_MD_INIT_OTDR			IIT_MD_INIT_OTDR_L
#define	IIT_MD_SERVICE_FUNCTION		 	IIT_MD_SERVICE_FUNCTION_L
#define IIT_MD_MEAS_STEP			IIT_MD_MEAS_STEP_L
#define IIT_MD_MEAS_FULL			IIT_MD_MEAS_FULL_L
#define IIT_MD_MEAS_STOP			IIT_MD_MEAS_STOP_L
#define IIT_MD_SET_MEAS_PRE			IIT_MD_SET_MEAS_PRE_L
#define MEASPRMDLG_MD_SERVICE_FUNCTION			MEASPRMDLG_MD_SERVICE_FUNCTION_L

#define IIT_MEAS_STEP_BUFFER          IIT_MEAS_STEP_BUFFER_L
#define IIT_MEAS_STOP_BUFFER          IIT_MEAS_STOP_BUFFER_L
#define IIT_MEAS_FULL_BUFFER          IIT_MEAS_FULL_BUFFER_L
#define IIT_MD_MEAS_STEP_BUFFER       IIT_MD_MEAS_STEP_BUFFER_L
#define IIT_MD_MEAS_STOP_BUFFER       IIT_MD_MEAS_STOP_BUFFER_L
#define IIT_MD_MEAS_FULL_BUFFER       IIT_MD_MEAS_FULL_BUFFER_L

#define IIT_GET_SOR_SIZE    IIT_GET_SOR_SIZE_L
#define IIT_GET_SOR_DATA    IIT_GET_SOR_DATA_L
#define IIT_CREATE_SOR_PTR  IIT_CREATE_SOR_PTR_L
#define IIT_DESTROY_SOR_PTR IIT_DESTROY_SOR_PTR_L

#else

#define DLL_INIT                      	"_DllInit"
#define DLL_INIT_2                      "_DllInit2"
#define IIT_INIT_OTDR                   "_InitOTDR"
#define IIT_SERVICE_FUNCTION            "_ServiceFunction"
#define IIT_MEAS_STEP                   "_MeasStep"
#define IIT_MEAS_FULL                   "_MeasFull"
#define IIT_MEAS_STOP                   "_MeasStop"
#define IIT_SET_MEAS_PRE                "_MeasPrepare"
#define MEASPRMDLG_SERVICE_FUNCTION     "_ServiceFunction"
#define IIT_SET_COOLING					"_SetCooling"
#define IIT_FREE_BUFFER					"_FreeBuffer"

#define IIT_MD_ADD_CONTEXT			       "_MD_AddContext"
#define IIT_MD_ADD_DEVICE			       "_MD_AddDevice"
#define IIT_MD_DELETE_CONTEXT			  "_MD_DeleteContext"
#define IIT_MD_DELETE_DEVICE			  "_MD_DeleteDevice"
#define IIT_MD_INIT_OTDR                   "_MD_InitOTDR"
#define IIT_MD_SERVICE_FUNCTION            "_MD_ServiceFunction"
#define IIT_MD_MEAS_STEP                   "_MD_MeasStep"
#define IIT_MD_MEAS_FULL                   "_MD_MeasFull"
#define IIT_MD_MEAS_STOP                   "_MD_MeasStop"
#define IIT_MD_SET_MEAS_PRE                "_MD_MeasPrepare"
#define MEASPRMDLG_MD_SERVICE_FUNCTION     "_MD_ServiceFunction"

#define IIT_MEAS_STEP_BUFFER               "_"##IIT_MEAS_STEP_BUFFER_L
#define IIT_MEAS_STOP_BUFFER               "_"##IIT_MEAS_STOP_BUFFER_L
#define IIT_MEAS_FULL_BUFFER               "_"##IIT_MEAS_FULL_BUFFER_L
#define IIT_MD_MEAS_STEP_BUFFER            "_"##IIT_MD_MEAS_STEP_BUFFER_L
#define IIT_MD_MEAS_STOP_BUFFER            "_"##IIT_MD_MEAS_STOP_BUFFER_L
#define IIT_MD_MEAS_FULL_BUFFER            "_"##IIT_MD_MEAS_FULL_BUFFER_L

#define IIT_GET_SOR_SIZE    "_"##IIT_GET_SOR_SIZE_L
#define IIT_GET_SOR_DATA    "_"##IIT_GET_SOR_DATA_L
#define IIT_CREATE_SOR_PTR  "_"##IIT_CREATE_SOR_PTR_L
#define IIT_DESTROY_SOR_PTR "_"##IIT_DESTROY_SOR_PTR_L


#endif




#define IIT_PORT_XXX               100
#define FREE_PORT                  -1
#define LAST_PORT                  IIT_PORT_XXX + 0
#define COM_PORT                   IIT_PORT_XXX + 1
#define USB_PORT                   IIT_PORT_XXX + 2
#define TCP_PORT                   IIT_PORT_XXX + 3
#define EMB_PORT                   IIT_PORT_XXX + 4
#define SPI_PORT                   IIT_PORT_XXX + 5
#define ACTIVESYNC_PORT            IIT_PORT_XXX + 6
#define PASSIVETCP_PORT            IIT_PORT_XXX + 7
#define CEDEV_PORT                 IIT_PORT_XXX + 8
#define DLL_NAME_PASSED_FOR_PORT   IIT_PORT_XXX + 9
#define ANDROID_BT_PORT            IIT_PORT_XXX + 10
#define WIN_BT_PORT                IIT_PORT_XXX + 11

#define COM_PORT1                  "com1"
#define COM_PORT2                  "com2"
#define COM_PORT3                  "com3"
#define COM_PORT4                  "com4"

#define TCP_PORT1                  "192.111.111.5"

#define FILTER_NO                  0
#define FILTER_FTT_IRRECOVERABLE   1
#define FILTER_IIT                 2
#define FILTER_AUTO_IRRECOVERABLE  3
#define FILTER_FTT_RECOVERABLE     4
#define FILTER_AUTO_RECOVERABLE    5
#define FILTER_FTT                 FILTER_FTT_IRRECOVERABLE   // for backward code compatibility

#define IIT_MEAS_CMD_XXX           500
#define MEAS_CMD_BEGIN             IIT_MEAS_CMD_XXX + 1
#define MEAS_CMD_LOADREADY         IIT_MEAS_CMD_XXX + 2
#define MEAS_CMD_LOAD              IIT_MEAS_CMD_XXX + 3
#define MEAS_CMD_WAIT              IIT_MEAS_CMD_XXX + 4
#define MEAS_CMD_TERMINATE         IIT_MEAS_CMD_XXX + 5
#define MEAS_CMD_TERMINATEFAST     IIT_MEAS_CMD_XXX + 6
#define MEAS_CMD_FINISH            IIT_MEAS_CMD_XXX + 7



#define IIT_MEAS_MODE_XXX          600
#define MEAS_FAST                  IIT_MEAS_MODE_XXX + 0
#define MEAS_AVER                  IIT_MEAS_MODE_XXX + 1


#define IIT_SERVICE_XXX               700
#define SERVICE_CMD_MONITOR           IIT_SERVICE_XXX + 1  //mean the same as ..._POINTS
#define SERVICE_CMD_GETPARAM          IIT_SERVICE_XXX + 2
#define SERVICE_CMD_SHOWPARAM         IIT_SERVICE_XXX + 3
#define SERVICE_CMD_SETBASE           IIT_SERVICE_XXX + 4
#define SERVICE_CMD_SETPARAM          IIT_SERVICE_XXX + 5
#define SERVICE_CMD_MONITOR_POINTS    SERVICE_CMD_MONITOR  //monitor by points comparison
#define SERVICE_CMD_MONITOR_EVENTS    SERVICE_CMD_MONITOR  //monitor by events comparison
#define SERVICE_CMD_GETBASE           IIT_SERVICE_XXX + 7
#define SERVICE_CMD_SETPARAM_FROM_SOR IIT_SERVICE_XXX + 8
#define SERVICE_CMD_SHOWPARAM_LNG     IIT_SERVICE_XXX + 9
#define SERVICE_CMD_AUTO              IIT_SERVICE_XXX + 10
#define SERVICE_CMD_GETOTDRINFO       IIT_SERVICE_XXX + 11

#define SERVICE_CMD_GETOTDRINFO_MFID      1
#define SERVICE_CMD_GETOTDRINFO_MFSN      2
#define SERVICE_CMD_GETOTDRINFO_OMSN      3
#define SERVICE_CMD_GETOTDRINFO_OMID      4
//#define SERVICE_CMD_GETOTDRINFO_UNIT_INI  5 // commented since not used. See revision 6734 of iit_otdr.cpp

#define SERVICE_CMD_GETAUTOPARAM      IIT_SERVICE_XXX + 12
#define SERVICE_CMD_SETAUTOPARAM      IIT_SERVICE_XXX + 13

#define SERVICE_CMD_AUTOPARAM_LT  1
#define SERVICE_CMD_AUTOPARAM_CT  2
#define SERVICE_CMD_AUTOPARAM_RT  3
#define SERVICE_CMD_AUTOPARAM_ET  4

#define SERVICE_CMD_TEMPERATURE_GETMAXTEMPERATURE			1
#define SERVICE_CMD_TEMPERATURE_GETMINTEMPERATURE			2
#define SERVICE_CMD_TEMPERATURE_GETMAXTEMPERATUREDISTANCE	3
#define SERVICE_CMD_TEMPERATURE_GETMINTEMPERATUREDISTANCE	4

#define SERVICE_CMD_PARAM_UNIT                  1
#define SERVICE_CMD_PARAM_Lmax                  2
#define SERVICE_CMD_PARAM_L1                    3
#define SERVICE_CMD_PARAM_L2                    4
#define SERVICE_CMD_PARAM_Res                   5
#define SERVICE_CMD_PARAM_Pulse                 6
#define SERVICE_CMD_PARAM_NAVR                  7
#define SERVICE_CMD_PARAM_Time                  8
#define SERVICE_CMD_PARAM_IsTime                9
#define SERVICE_CMD_PARAM_GI                    10
#define SERVICE_CMD_PARAM_RI                    10
#define SERVICE_CMD_PARAM_BC                    11
#define SERVICE_CMD_PARAM_HiRes                 12
#define SERVICE_CMD_PARAM_LoPow                 13
#define SERVICE_CMD_PARAM_LowPow                13
#define SERVICE_CMD_PARAM_Refresh               14
#define SERVICE_CMD_PARAM_WLEnabled             15
#define SERVICE_CMD_PARAM_Filter                16
#define SERVICE_CMD_PARAM_L1_VAL                17
#define SERVICE_CMD_PARAM_L2_VAL                18
#define SERVICE_CMD_PARAM_Lmax_VAL              72
#define SERVICE_CMD_PARAM_Pulse_VAL             73
#define SERVICE_CMD_PARAM_NAVR_VAL              74
#define SERVICE_CMD_PARAM_MAX_Lmax              75
#define SERVICE_CMD_PARAM_MAX_Pulse             76
#define SERVICE_CMD_PARAM_Time_VAL              77
#define SERVICE_CMD_PARAM_CONN                  78
#define SERVICE_CMD_PARAM_OverrideScaleAvr      79
#define SERVICE_CMD_PARAM_ScaleAvr              80
#define SERVICE_CMD_PARAM_ScaleAvr_VAL          81
#define SERVICE_CMD_PARAM_DWDM_CHANNEL          82
#define SERVICE_CMD_PARAM_FAST_AVR_NUMBER		83


#define SERVICE_CMD_APARAM_UNIT                 20
#define SERVICE_CMD_APARAM_Lmax                 21
#define SERVICE_CMD_APARAM_L1                   22
#define SERVICE_CMD_APARAM_L2                   23
#define SERVICE_CMD_APARAM_Res                  24
#define SERVICE_CMD_APARAM_Pulse                25
#define SERVICE_CMD_APARAM_NAVR                 26
#define SERVICE_CMD_APARAM_Time                 27
#define SERVICE_CMD_APARAM_IsTime               28
#define SERVICE_CMD_APARAM_GI                   29
#define SERVICE_CMD_APARAM_RI                   29
#define SERVICE_CMD_APARAM_BC                   30
#define SERVICE_CMD_APARAM_HiRes                31
#define SERVICE_CMD_APARAM_LoPow                32
#define SERVICE_CMD_APARAM_LowPow               32
#define SERVICE_CMD_APARAM_Refresh              33
#define SERVICE_CMD_APARAM_WLEnabled            34
#define SERVICE_CMD_APARAM_Filter               35
#define SERVICE_CMD_APARAM_L1_VAL               36
#define SERVICE_CMD_APARAM_L2_VAL               37
#define SERVICE_CMD_APARAM_Lmax_VAL             92
#define SERVICE_CMD_APARAM_Pulse_VAL            93
#define SERVICE_CMD_APARAM_NAVR_VAL             94
#define SERVICE_CMD_APARAM_Time_VAL             95
#define SERVICE_CMD_APARAM_CONN                 96
#define SERVICE_CMD_APARAM_OverrideScaleAvr     97
#define SERVICE_CMD_APARAM_ScaleAvr             98
#define SERVICE_CMD_APARAM_ScaleAvr_VAL         99
#define SERVICE_CMD_APARAM_DWDM_CHANNEL        100
#define SERVICE_CMD_APARAM_FAST_AVR_NUMBER     101



#define SERVICE_CMD_IPARAM_UNIT                 40
#define SERVICE_CMD_IPARAM_Lmax                 41
#define SERVICE_CMD_IPARAM_L1                   42
#define SERVICE_CMD_IPARAM_L2                   43
#define SERVICE_CMD_IPARAM_Res                  44
#define SERVICE_CMD_IPARAM_Pulse                45
#define SERVICE_CMD_IPARAM_NAVR                 46
#define SERVICE_CMD_IPARAM_Time                 47
#define SERVICE_CMD_IPARAM_IsTime               48
#define SERVICE_CMD_IPARAM_GI                   49
#define SERVICE_CMD_IPARAM_RI                   49
#define SERVICE_CMD_IPARAM_BC                   50
#define SERVICE_CMD_IPARAM_HiRes                51
#define SERVICE_CMD_IPARAM_LoPow                52
#define SERVICE_CMD_IPARAM_LowPow               52
#define SERVICE_CMD_IPARAM_Refresh              53
#define SERVICE_CMD_IPARAM_WLEnabled            54
#define SERVICE_CMD_IPARAM_OverrideScaleAvr     55
#define SERVICE_CMD_IPARAM_ScaleAvr             56
#define SERVICE_CMD_IPARAM_DWDM_CHANNEL         57
#define SERVICE_CMD_IPARAM_FAST_AVR_NUMBER      58


#define SERVICE_CMD_ALL_BASE					200
#define SERVICE_CMD_PARAM_LMAX_ALL				SERVICE_CMD_ALL_BASE + 0
#define SERVICE_CMD_PARAM_Pulse_ALL				SERVICE_CMD_ALL_BASE + 1
#define SERVICE_CMD_PARAM_Res_ALL				SERVICE_CMD_ALL_BASE + 2
#define SERVICE_CMD_PARAM_Time_ALL				SERVICE_CMD_ALL_BASE + 3


#define SERVICE_CMD_AUTO_ANALYSE        IIT_SERVICE_XXX + 14
#define SERVICE_CMD_DEV_INFORMATION     IIT_SERVICE_XXX + 15
#define SERVICE_CMD_APPLY_PARAM         IIT_SERVICE_XXX + 16
#define SERVICE_CMD_AUTO_PARAM_FIND     IIT_SERVICE_XXX + 17

#define SERVICE_CMD_SETPARAM_DEFAULTS   IIT_SERVICE_XXX + 18
#define SERVICE_CMD_GET_YSCALE          IIT_SERVICE_XXX + 19
#define SERVICE_CMD_SET_YSCALE_FLAG     IIT_SERVICE_XXX + 20
#define SERVICE_CMD_AUTO_EOF            IIT_SERVICE_XXX + 21
#define SERVICE_CMD_RANGE_VIEW          IIT_SERVICE_XXX + 22
#define SERVICE_CMD_AUTO_MEAS_PRM       IIT_SERVICE_XXX + 23
#define SERVICE_CMD_SAVE_PARAMS         IIT_SERVICE_XXX + 24
#define SERVICE_CMD_LOAD_PARAMS         IIT_SERVICE_XXX + 25
#define SERVICE_CMD_GET_POWER           IIT_SERVICE_XXX + 26
#define SERVICE_POWER_RET_FAIL  0
#define SERVICE_POWER_RET_OK    1
#define SERVICE_POWER_RET_BUSY  2
#define SERVICE_POWER_RET_HIGH  3

#define SERVICE_CMD_RELOAD_CONTEXT      IIT_SERVICE_XXX + 27

#define SERVICE_CMD_GETMODULES_COUNT    IIT_SERVICE_XXX + 28
#define SERVICE_CMD_GETMODULES_NAME     IIT_SERVICE_XXX + 29
#define SERVICE_CMD_GETMODULES_VERSION  IIT_SERVICE_XXX + 30

#define SERVICE_CMD_GET_MEASSTEPS_COUNT IIT_SERVICE_XXX + 31
#define SERVICE_CMD_APPLY_FILTER        IIT_SERVICE_XXX + 32

#define SERVICE_CMD_GETBASE_BUFFER                  IIT_SERVICE_XXX + 33
#define SERVICE_CMD_SETBASE_BUFFER                  IIT_SERVICE_XXX + 34
#define SERVICE_CMD_MONITOR_BUFFER                  IIT_SERVICE_XXX + 35
#define SERVICE_CMD_SETPARAM_FROM_SOR_BUFFER        IIT_SERVICE_XXX + 36
#define SERVICE_CMD_AUTO_BUFFER                     IIT_SERVICE_XXX + 37
#define SERVICE_CMD_APPLY_FILTER_BUFFER             IIT_SERVICE_XXX + 38

#define SERVICE_CMD_GETPARAM_FOR_LASER              IIT_SERVICE_XXX + 39
#define SERVICE_CMD_RESERVED1                       IIT_SERVICE_XXX + 40
#define SERVICE_CMD_LS_CONTROL                      IIT_SERVICE_XXX + 41
#define SERVICE_CMD_LS_PWR_TEST                     IIT_SERVICE_XXX + 42
#define SERVICE_CMD_LS_GET_PARAMS                   IIT_SERVICE_XXX + 43
#define SERVICE_CMD_SET_INTERMEDIATE_SOR_POINTS_NUM IIT_SERVICE_XXX + 44
#define SERVICE_CMD_PARAM_MEAS_LMAX_GET	            IIT_SERVICE_XXX + 45
#define SERVICE_CMD_PARAM_MEAS_LMAX_SET		        IIT_SERVICE_XXX + 46
#define SERVICE_CMD_PARAM_MEAS_CONQ_GET		        IIT_SERVICE_XXX + 47
#define SERVICE_CMD_UNIT_GET				        IIT_SERVICE_XXX + 48
#define SERVICE_CMD_MEAS_CONN_PARAMS_AND_LMAX       IIT_SERVICE_XXX + 49
#define SERVICE_CMD_OBTAIN_LINKSCAN_PARAMS          IIT_SERVICE_XXX + 50
#define SERVICE_CMD_APPLY_LINKSCAN_PARAM_I          IIT_SERVICE_XXX + 51
#define SERVICE_CMD_SET_PON_PARAMS_FOR_AUTO_PARAMS  IIT_SERVICE_XXX + 52
#define SERVICE_CMD_PARAM_DEVICE_PORT_COMMAND		IIT_SERVICE_XXX + 53

#define SERVICE_CMD_INITIALIZE_TEMPERATURE_TEMPLATE			IIT_SERVICE_XXX + 54
#define SERVICE_CMD_INITIALIZE_TEMPERATURE_CURVE_GENERATOR	IIT_SERVICE_XXX + 55
#define SERVICE_CMD_GENERATE_TEMPERATURE_CURVE				IIT_SERVICE_XXX + 56
#define SERVICE_CMD_COLLECT_TEMPERATURE_DATA				IIT_SERVICE_XXX + 57
#define SERVICE_CMD_INITIALIZE_TEMPERATURE_ANALYZER			IIT_SERVICE_XXX + 58
#define SERVICE_CMD_TEMPERATURE_ANALYSIS					IIT_SERVICE_XXX + 59
#define SERVICE_CMD_ESTIMATE_NUMBER_OF_FAST_AVERAGES	   	IIT_SERVICE_XXX + 60
#define SERVICE_CMD_ESTIMATE_NUMBER_OF_DATA_POINTS			IIT_SERVICE_XXX + 61
#define SERVICE_CMD_WAIT_FOR_HARDWARE_TO_GET_READY			IIT_SERVICE_XXX + 62
#define SERVICE_CMD_SET_FAST_MEAS_CUSTOM_RESOLUTION			IIT_SERVICE_XXX + 63
#define SERVICE_CMD_PARAM_DWDM_LASER_HEATING                IIT_SERVICE_XXX + 64
#define SERVICE_CMD_OTDR_UNIT_TEMPERATURE                   IIT_SERVICE_XXX + 65

#define SERVICE_CMD_SETPARAM_LIST                           IIT_SERVICE_XXX + 66
#define SERVICE_CMD_PAID_OPTION_CHECK                       IIT_SERVICE_XXX + 67

#define SERVICE_CMD_SOR_SET_LANG_CODE                       IIT_SERVICE_XXX + 68


// Old typos, left for backward compatibility with old client code
#define SEVICE_CMD_SAVE_PARAMS SERVICE_CMD_SAVE_PARAMS
#define SERIVE_CMD_LOAD_PARAMS SERVICE_CMD_LOAD_PARAMS
#define SERIVE_CMD_GET_POWER SERVICE_CMD_GET_POWER
#define SERIVE_CMD_LS_CONTROL SERVICE_CMD_LS_CONTROL
#define SERIVE_CMD_LS_PWR_TEST SERVICE_CMD_LS_PWR_TEST
#define SERIVE_CMD_LS_GET_PARAMS SERVICE_CMD_LS_GET_PARAMS

#define MONITOR_FLAG_EMBED_BASE    (1 << 0)
#define MONITOR_FLAG_DONT_RECALC   (1 << 1)


#define ERROR_OK                       0

#define IIT_ERROR_XXX                  800
#define ERROR_COM                      IIT_ERROR_XXX  + 1
#define ERROR_COM_IN_USE               ERROR_COM  + 1  //2 mean the same
#define ERROR_COMINUSE                 ERROR_COM  + 1  //2 mean the same
#define ERROR_GETCOMMSTATE             ERROR_COM  + 2  //3
#define ERROR_SETCOMMSTATE             ERROR_COM  + 3  //4
#define ERROR_COM_OPEN                 ERROR_COM  + 4  //5

#define ERROR_MODULENAME               IIT_ERROR_XXX  + 10 //10
#define ERROR_MODULE_ID                IIT_ERROR_XXX  + 11 //11
#define ERROR_OTDR_CONNECT             IIT_ERROR_XXX  + 12 //12
#define ERROR_OTDR_SERVFUNC            IIT_ERROR_XXX  + 13 //13

#define ERROR_OTDR                     IIT_ERROR_XXX  + 14  //14
#define ERROR_ioERROR                  ERROR_OTDR  + 1   //15
#define ERROR_prgERROR                 ERROR_OTDR  + 2   //16
#define ERROR_ioRESET                  ERROR_OTDR  + 3   //17
#define ERROR_ioDEVICE                 ERROR_OTDR  + 4   //18
#define ERROR_errMODEL                 ERROR_OTDR  + 5   //19
#define ERROR_ioMEMDSP                 ERROR_OTDR  + 6   //20
#define ERROR_ioINFO                   ERROR_OTDR  + 7   //21
#define ERROR_ioLOADER                 ERROR_OTDR  + 8   //22
#define ERROR_fileMODEL                ERROR_OTDR  + 9   //23
#define ERROR_fileLOADER               ERROR_OTDR  + 10  //24
#define ERROR_fileMEAS                 ERROR_OTDR  + 11  //25
#define ERROR_ioGOLOADER               ERROR_OTDR  + 12  //26
#define ERROR_ioINFOUNIT               ERROR_OTDR  + 13  //27
#define ERROR_ioLoadFLEX               ERROR_OTDR  + 14  //28
#define ERROR_fileFLEX                 ERROR_OTDR  + 15  //29
#define ERROR_ioFlexSIO                ERROR_OTDR  + 16  //30
#define ERROR_ioSETPARAM               ERROR_OTDR  + 17  //31
#define ERROR_ioSETMEAS                ERROR_OTDR  + 18  //32
#define ERROR_ioWAITMEAS               ERROR_OTDR  + 19  //33
#define ERROR_ioZERO                   ERROR_OTDR  + 20  //34
#define ERROR_ioMRDSP                  ERROR_OTDR  + 21  //35
#define ERROR_ioDATA                   ERROR_OTDR  + 22  //36
#define ERROR_ioRD24                   ERROR_OTDR  + 23  //37
#define ERROR_fileDLL                  ERROR_OTDR  + 24  //38
#define ERROR_comInUse                 ERROR_OTDR  + 25  //39
#define ERROR_comError                 ERROR_OTDR  + 26  //40
#define ERROR_ioDAC                    ERROR_OTDR  + 27  //41
#define ERROR_errDAC                   ERROR_OTDR  + 28  //42
#define ERROR_ioDSP_MONITOR_NOT_READY  ERROR_OTDR  + 29  //43
#define ERROR_ioDSP_INVALID_CONFIRMATION_COMMAND      ERROR_OTDR  + 30  //44
#define ERROR_ERROR_IN_PARAMETERS      ERROR_OTDR  + 31  //45
///////////////////////////////
#define ERROR_LOAD_DLL                 IIT_ERROR_XXX  + 70 //70
#define ERROR_NO_BASE                  IIT_ERROR_XXX  + 71 //71
#define ERROR_PRM_FILE                 IIT_ERROR_XXX  + 72 //72
#define ERROR_NO_PREPARE               IIT_ERROR_XXX  + 73 //73 mean the same
#define ERROR_MEAS_FINISH              IIT_ERROR_XXX  + 73 //73 mean the same
#define ERROR_UNKNOWN                  IIT_ERROR_XXX  + 74 //74
#define ERROR_NO_MEAS                  IIT_ERROR_XXX  + 75 //75
#define ERROR_NO_INITIALIZED           IIT_ERROR_XXX  + 76 //76
//#define ERROR_BAD_BASE						IIT_ERROR_XXX	+ 77 //77
#define ERROR_CANNOT_SETPARAM          IIT_ERROR_XXX  + 78 //78
#define ERROR_CANNOT_SETPARAM_UNIT     IIT_ERROR_XXX  + 79 //79
#define ERROR_CANNOT_SETPARAM_FAST     IIT_ERROR_XXX  + 80 //80
#define ERROR_DOING_AUTO_MONITOR       IIT_ERROR_XXX  + 81 //81
#define ERROR_NO_SERVICE               IIT_ERROR_XXX  + 82 //82
#define ERROR_BAD_PARAMETER            IIT_ERROR_XXX  + 84 //84
#define ERROR_UNSUPPORTED_BUFFER_VERSION IIT_ERROR_XXX  + 85 //85
#define ERROR_UNSUPPORTED_DLLINITPARAMS_VERSION IIT_ERROR_XXX  + 86 //86
#define ERROR_SKIP_REST_OF_LINKSCAN 1


#define IIT_RETURN_XXX                 10000
#define RETURN_FINISH                  IIT_RETURN_XXX + 1
#define RETURN_NOLINK                  IIT_RETURN_XXX + 2
#define RETURN_NOACTLEVEL              IIT_RETURN_XXX + 3
#define RETURN_FIBERBREAK              IIT_RETURN_XXX + 4


#define COM_SPEED01  9600
#define COM_SPEED02  14400
#define COM_SPEED03  19200
#define COM_SPEED04  38400
#define COM_SPEED05  56000
#define COM_SPEED06  57600
#define COM_SPEED07  115200

#define ID_USB_USE_INTERNAL_OTDR   1
#define ID_USB_INTERNAL_OTDR_DEVICE ID_USB_USE_INTERNAL_OTDR
#define ID_USB_USE_EXTERNAL_OTDR   2
#define ID_USB_EXTERNAL_OTDR_DEVICE ID_USB_USE_EXTERNAL_OTDR
#define ID_USB_USE_INERNAL_AND_EXTERNAL_OTDR 3 // 1 | 2
#define ID_USB_USE_INTERNAL_OPM		0x4
#define ID_USB_INTERNAL_OPM_DEVICE ID_USB_USE_INTERNAL_OPM
#define ID_USB_USE_EXTERNAL_OPM		0x8
#define ID_USB_EXTERNAL_OPM_DEVICE ID_USB_USE_EXTERNAL_OPM
#define ID_USB_USE_INTERNAL_AND_EXTERNAL_OPM 0xC
#define ID_USB_UNKNOWN_DEVICE 0xffff

namespace IITOTDRDll
{
    struct DllInitParams
    {
        static const unsigned int LastKnownVersion = 1;
        unsigned int Version;
        void * LogFile;
        void * LenUnit;
        const char * LogDir;
        const char * LibDir;
        const char * IniDir;
    };

	struct BufferData
	{
		static const unsigned int LastKnownVersion = 1;
		unsigned int Version;
		unsigned long Size;
		unsigned char * Buffer;
		BufferData(unsigned long size = 0, unsigned char * buffer = NULL) :  Version(LastKnownVersion), Size(size), Buffer(buffer){};
		void Free() {if (Buffer != NULL) delete [] Buffer; Buffer = NULL; Size = 0;};
	};
	
	struct ConnectionParams
	{
		float reflectance; // R -dB
		float splice; // dB (better say "loss")
		float snr_almax;
	};
	struct TPONParams
	{
		enum PonType { ptNotAPon, ptManualPon, ptAutoPon };
		PonType type;
		int splitterRatio; // 1xN
		int secondSplitterRatio;
		bool isCO2ONT; //Cental Office to ONT  or vice versa
		float length; // km

		TPONParams()
		{
			type = ptNotAPon;
			splitterRatio = 1;
			secondSplitterRatio = 1;
			isCO2ONT = true;
			length = 25;
		}
	};
	enum OtdrPaidOptions { optEnabledAll, optDisabledAll, optVScout, optAnalysis };
}



#endif
