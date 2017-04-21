namespace IitOtdrLibrary
{
    public enum ServiceFunctionCommand
    {
        IitServiceXxx = 700,
        Monitor = 701, //mean the same as ..._POINTS
        GetParam = 702,
        Showparam = 703,
        Setbase = 704,
        SetParam = 705,
        MonitorPoints = 701, //monitor by points comparison
        MonitorEvents = 701, //monitor by events comparison
        Getbase = 707,
        SetParamFromSor = 708,
        ShowparamLng = 709,
        Auto = 710,
        Getotdrinfo = 711,
        Getautoparam = 712,
        Setautoparam = 713,

        AutoAnalyse = 714,
        DevInformation = 715,
        ApplyParam = 716,
        AutoParamFind = 717,

        SetparamDefaults = 718,
        GetYscale = 719,
        SetYscaleFlag = 720,
        AutoEof = 721,
        RangeView = 722,
        AutoMeasPrm = 723,
        SaveParams = 724,
        LoadParams = 725,
        GetPower = 726,
        ReloadContext = 727,

        GetmodulesCount = 728,
        GetmodulesName = 729,
        GetmodulesVersion = 730,

        GetMeasstepsCount = 731,
        ApplyFilter = 732,
        GetbaseBuffer = 733,
        SetbaseBuffer = 734,
        MonitorBuffer = 735,
        SetparamFromSorBuffer = 736,
        AutoBuffer = 737,
        ApplyFilterBuffer = 738,

        GetparamForLaser = 739,
        Reserved1 = 740,
        LsControl = 741,
        LsPwrTest = 742,
        LsGetParams = 743,
        SetIntermediateSorPointsNum = 744,
        ParamMeasLmaxGet = 745,
        ParamMeasLmaxSet = 746,
        ParamMeasConqGet = 747,
        UnitGet = 748,
        MeasConnParamsAndLmax = 749,
        ObtainLinkscanParams = 750,
        ApplyLinkscanParamI = 751,
        SetPonParamsForAutoParams = 752,
        ParamDevicePortCommand = 753,

        InitializeTemperatureTemplate = 754,
        InitializeTemperatureCurveGenerator = 755,
        GenerateTemperatureCurve = 756,
        CollectTemperatureData = 757,
        InitializeTemperatureAnalyzer = 758,
        TemperatureAnalysis = 759,
        EstimateNumberOfFastAverages = 760,
        EstimateNumberOfDataPoints = 761,
        WaitForHardwareToGetReady = 762,
        SetFastMeasCustomResolution = 763,
        ParamDwdmLaserHeating = 764,
        OtdrUnitTemperature = 765,

    }
}