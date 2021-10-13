namespace Graph.Tests
{
    public static class RequestFromUri
    {
        public static string Request(this string relativeUri, string httpMethod)
        {
            #region L1
            if (relativeUri == @"monitoring" && httpMethod == @"get")
                return @"GetMonitoringProperties";
            if (relativeUri == @"monitoring" && httpMethod == @"patch")
                return @"SetMonitoringProperty";
            if (IsChangeProxyMode(relativeUri) && httpMethod == @"patch")
                return @"ChangeProxyMode";
            if (IsOneOfUri(relativeUri, @"otaus") && httpMethod == @"patch")
                return @"SwitchOtauToPort";
            if (relativeUri == @"measurements" && httpMethod == @"post")
                return @"DoMeasurementRequest";
            if (IsOneOfUri(relativeUri, @"measurements") && httpMethod == @"get")
                return @"GetMeasurementResult";
            #endregion

            #region GetInfo
            if (relativeUri == @"otdrs" && httpMethod == @"get")
                return @"GetOtdrs";
            if (IsOneOfUri(relativeUri, @"otdrs") && httpMethod == @"get")
                return @"GetOtdr";
            if (relativeUri == @"info" && httpMethod == @"get")
                return @"GetPlatform";
            #endregion

            #region Otau
            if (relativeUri == @"otaus" && httpMethod == @"post")
                return @"CreateOtau";
            if (IsOneOfUri(relativeUri, @"otaus") && httpMethod == @"delete")
                return @"DeleteOtau";
            if (relativeUri == @"otau_cascading" && httpMethod == @"patch")
                return @"ChangeOtauCascadingScheme";
            if (relativeUri == @"otau_cascading" && httpMethod == @"get")
                return @"GetOtauCascadingScheme";
            if (IsOneOfUri(relativeUri, @"otaus") && httpMethod == @"get")
                return @"GetOtau";
            if (relativeUri == @"otaus" && httpMethod == @"get")
                return @"GetOtaus";
            #endregion

            #region Tests
            if (relativeUri == @"monitoring/tests" && httpMethod == @"get")
                return @"GetTests";
            if (IsGetTestUri(relativeUri))
                return @"GetTest";
            if (relativeUri == @"monitoring/tests" && httpMethod == @"post")
                return @"CreateTest";
            if (relativeUri == @"monitoring/test_relations" && httpMethod == @"post")
                return @"AddTestsRelation";
            if (IsOneOfUri(relativeUri, @"monitoring/test_relations") && httpMethod == @"delete")
                return @"DeleteRelation";
            if (IsOneOfUri(relativeUri, @"monitoring/tests") && httpMethod == @"patch")
                return @"ChangeTest";
            if (IsOneOfUri(relativeUri, @"monitoring/tests") && httpMethod == @"delete")
                return @"DeleteTest";
            if (IsSetBaseRefUri(relativeUri))
                return @"SetBaseRef";
            #endregion

            #region MoniResult
            if (IsGetCompletedTestsAfterTimestamp(relativeUri))
                return "GetCompletedTestsAfterTimestamp";
            #endregion

            return @"UnknownRequest";
        }

        private static bool IsOneOfUri(string relativeUri, string branch)
        {
            var parts = relativeUri.Split('/');
            if (parts.Length < 2)
                return false;
            var last = relativeUri.LastIndexOf('/');
            var starts = relativeUri.Substring(0, last);
            return starts == branch;
        }

        private static bool IsChangeProxyMode(string relativeUri)
        {
            return relativeUri.StartsWith(@"otdrs/") &&
                   relativeUri.EndsWith(@"/tcp_proxy");

        }

        private static bool IsGetTestUri(string relativeUri)
        {
            return relativeUri.StartsWith(@"monitoring/") &&
                   relativeUri.EndsWith(@"?fields=*,otauPorts.*,relations.items.*");
        }
        private static bool IsSetBaseRefUri(string relativeUri)
        {
            return relativeUri.StartsWith(@"monitoring/tests/") &&
                   relativeUri.EndsWith(@"/references");
        }
      
        private static bool IsGetCompletedTestsAfterTimestamp(string relativeUri)
        {
            return relativeUri.StartsWith("monitoring/completed?fields=*,items.*&starting=");
        }
    }
}
