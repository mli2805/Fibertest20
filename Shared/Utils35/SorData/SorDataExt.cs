using Optixsoft.SorExaminer.OtdrDataFormat;

namespace Iit.Fibertest.UtilsLib
{
    public static class SorDataExt
    {
        public static int GetLandmarkIndexForKeyEventIndex(this OtdrDataKnownBlocks sorData, int keyEventIndex)
        {
            var keyEventNumber = keyEventIndex + 1;
            for (int i = 0; i < sorData.LinkParameters.LandmarksCount; i++)
            {
                if (sorData.LinkParameters.LandmarkBlocks[i].RelatedEventNumber == keyEventNumber)
                    return i;
            }

            return -1;
        }

        public static int GetLandmarkToTheLeftFromOwt(this OtdrDataKnownBlocks sorData, int owt)
        {
            var leftLandmarkIndex = 0; 
            for (int i =1; i < sorData.LinkParameters.LandmarksCount; i++)
            {
                if (sorData.LinkParameters.LandmarkBlocks[i].Location < owt)
                    leftLandmarkIndex = i;
                else return leftLandmarkIndex;
            }

            return leftLandmarkIndex; // owt to the right of end
        }
    }
}